using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO;

namespace RunnerTracker
{
    //
    //  The purpose for this worker thread is to handle communication with a station using Ethernet
    //
    // 7/25/16 - began changing Receive to aysnchronous
    //
    //  5/21/19 - changed in Data_received_from_Station section to prevent hang when closing after new station name is not accepted
    //            moved line in Close_Station() to make DB top status change to No Clients when new station closes after not being accepted.
    //  7/7/19 - added OutputMessageQueue to design
    //  7/9/19 - added Put_Message_in_Queue in this file
    //           inserted Message ID at beginning of data sent
    //

    public class DB_IP_StationWorker
    {
        #region Variables and declarations
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        TcpClient StationClient;
        EndPoint ClientEndPoint;
        string RemoteEndPointIP;
        public int Station_List_index = -1;
        string String_Going_to_Station;
        string String_Coming_from_Station;
        string Station_Name;
        public bool New_Message = false;
        public int Test6 = 6;
        public static int Test7 = 7;
//        private int NLP;
        public int NumLogPts;
//        {
//            get { return NLP; }
//            set { NLP = value; }
//        }      // 0 = indicates no change.  != 0 tells new Number of Log Points.
        public bool InitialLogPoint = true;
        int Stations_Index;
        int Name_Count;
        bool Server_shouldStop = false;
        bool Connected_to_Station = false;
        bool Data_Ready_to_Send_to_Station = false;     // this flag indicates data is ready to send to the station
        bool Data_Received_from_Station = false;        // this flag indicates data has been received from the station
        NetworkStream MyNetStream;
        Thread ReceiveThread;
        public enum Server_State { Not_Initted, Attempting_Connect, ClientConnected, Expecting_StationName, Connected_Active }
        public Server_State state = Server_State.Not_Initted;
        public class Message
        {
            public int Number { get; set; }
            public DateTime Received { get; set; }
            public int Size { get; set; }
            public string TheMessage { get; set; }
        }
        public List<Message> Incoming_Messages;
        public List<Message> Outgoing_Messages;
        int Number_Incoming_Messages, Number_Outgoing_Messages;
        public RichTextBox Ethernet_Packets { get; set; }

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile String Server_String_In;
        private volatile Byte[] Receivebytes;
        private volatile Byte[] Sendbytes;
        public volatile System.Timers.Timer Fivesecond = new System.Timers.Timer();
        private bool Server_Busy = false;
        bool Restart_Receive;
        int Snumbytes;
        char[] EOLchars = { '\n', '\r' };

        // Asynchronous Client variables - added 7/26/16
        // State object for receiving data from remote device.
        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 2048;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static bool receiveDonebool = true;
        #endregion

        public void Start(object client)
        {
            // create the Message lists
            Incoming_Messages = new List<Message>();
            Outgoing_Messages = new List<Message>();
            Number_Incoming_Messages = 0;
            Number_Outgoing_Messages = 0;

            // set the flag
            Connected_to_Station = true;
            NumLogPts = 0;      // 0 = indicates no change.  != 0 tells new Number of Log Points.

            // get the tcpclient
            StationClient = (TcpClient)client;
            ClientEndPoint = StationClient.Client.RemoteEndPoint;
            RemoteEndPointIP = ClientEndPoint.ToString();

            // give the thread a name
            Thread.CurrentThread.Name = "Station worker thread for IP: " + RemoteEndPointIP;

            // Start the Receive thread
//            ReceiveThread = new Thread(new ParameterizedThreadStart(Receive_Thread));
//            ReceiveThread.Start();
//            Console.WriteLine("Starting Station Worker Receive thread...");

            // Get a stream object for reading and writing
            MyNetStream = StationClient.GetStream();
            Receivebytes = new Byte[StationClient.ReceiveBufferSize];
            Sendbytes = new Byte[StationClient.SendBufferSize];

            //// start a three second timer for the actual data processing
            //Name_Count = 10;     // allow 30 seconds for the Station Name to come in
            //Threesecond.Interval = 5000;
            //Threesecond.Elapsed += new System.Timers.ElapsedEventHandler(StationTimeEvent);
            //Threesecond.Start();
            // start a five second timer for the actual data processing
            Name_Count = 10;     // allow 50 seconds for the Station Name to come in
            Fivesecond.Interval = 5000;
            Fivesecond.Elapsed += new System.Timers.ElapsedEventHandler(StationFiveSecondTimeEvent);
            Fivesecond.Start();

            // send request for Station Name
            String_Going_to_Station = "Station name?";
            Data_Ready_to_Send_to_Station = true;
            Console.WriteLine("Sent Station Name request to: " + RemoteEndPointIP);
            state = Server_State.Expecting_StationName;

            // processing loop
            while (!Server_shouldStop)
            {
                if (Name_Count == 1)
                {
                    Name_Count--;
                    if (state == Server_State.Expecting_StationName)
                    {
                        MessageBox.Show("Station with IP:\n\n   " + RemoteEndPointIP + "\n\nHas not responded with it's Station Name." + "\n\nThis station will be removed!", "Station Name not received", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        Server_shouldStop = true;       // terminate this thread
                        Form1.Stations_Activity_Flag = true;     // tell Form1 that a station has changed
                    }
                }

                if (Data_Received_from_Station)
                {
                    // what are we expecting?
                    switch (state)
                    {
                        case Server_State.Expecting_StationName:
                            // extract the station name
                            string teststr = "Station name = ";
                            if (String_Coming_from_Station.StartsWith(teststr))
                            {
                                String_Coming_from_Station = String_Coming_from_Station.Substring(teststr.Length);
                                Station_Name = String_Coming_from_Station.TrimEnd(EOLchars);

                                // there are 3 possibilities at this point:
                                // 1. this is a duplicate connection request
                                // 2. this is the first request for a station already in the Station list
                                // 3. this is the first request for a station NOT in the Station list - ask the user if this should be added to the Staation list

                                // find the station in the list
                                Station_List_index = Form1.Find_Station(Station_Name);
                                if (Station_List_index != -1)
                                {       // this station name is in the Station list - is it a duplicaate request?
                                    if (Form1.Stations[Station_List_index].Active)
                                    {       // duplicate - tell user
                                        MessageBox.Show("Station with name of: " + Station_Name + "\n\nand IP address of: " + RemoteEndPointIP + "\n\nhas issued another request to connect", "Duplicate Connect Request", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        Server_shouldStop = true;       // terminate this thread, removing this station request
                                    }
                                    else
                                    {
                                        // add his IP info and set active
                                        Form1.Stations[Station_List_index].IP_StationWorker = this;
                                        Form1.Stations[Station_List_index].Active = true;
                                        Form1.Stations[Station_List_index].IP_Address_Callsign = RemoteEndPointIP;   // other: Callsign
                                        Form1.Stations[Station_List_index].Medium = Form1.Connect_Medium.Ethernet.ToString();  // "IP";       // others: AGWPE/Packet
                                        Form1.Stations_Activity_Flag = true;
                                        state = Server_State.ClientConnected;
                                        Name_Count = 0; // use as a flag
                                        Stations_Index = Station_List_index;
                                        Form1.AddtoLogFile(Station_Name + " connected");
                                        Form1.Modeless_MessageBox_Information("Station:\n\n" + Station_Name + "\n\nHas just connected!", "Station activated");
// removed 3/16/16                                        String_Going_to_Station = "You are Active";
// removed 3/16/16                                        Data_Ready_to_Send_to_Station = true;
                                    }
                                }
                                else
                                {       // this station name does not exist in the Station list - ask the user if it should be added
                                    DialogResult res = MessageBox.Show("New station, with this name:\n\n          " + Station_Name + "\n\nHas just connected!\n\nAdd it to the Stations List?", "New Station", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                                    if (res == DialogResult.Yes)
                                    {
                                        Form1.NewStation newst = new Form1.NewStation();
                                        newst.Name = Station_Name;
                                        newst.IP_StationWorker = this;
                                        newst.Medium = Form1.Connect_Medium.Ethernet.ToString();
                                        newst.IP_Address_Callsign = RemoteEndPointIP;
                                        lock (Form1.NewActiveStationQue)
                                        {// lock
                                            Form1.NewActiveStationQue.Enqueue(newst);
                                            Form1.New_Active_Station_entry = true;
                                        }// unlock
                                        Name_Count = 0;

                                        // wait for it to be put into the Stations list - moved here 5/21/19, to prevent hang when closing when
                                        //      only one station, that is not in the station list tries to connect, but is not added to the list
                                        while (Station_List_index == -1)
                                            Station_List_index = Form1.Find_Station(Station_Name);
                                        Stations_Index = Station_List_index;
                                    }

// 5/21/19 moved above                                    // wait for it to be put into the Stations list
// 5/21/19                                    while (Station_List_index == -1)
// 5/21/19                                        Station_List_index = Form1.Find_Station(Station_Name);
// 5/21/19                                    Stations_Index = Station_List_index;
                                }
                            }
                            Data_Received_from_Station = false;     // change flag after using data
                            String_Coming_from_Station = "";        // and clear the data
                            break;
                        default:
                            // try to decode the data coming in - could also be: "Station name = <name>"
                            string Request = String_Coming_from_Station.Substring(0, String_Coming_from_Station.IndexOf(':'));
                            switch (Request)
                            {
                                case "Runner In":
                                    Add_Runner_In(GetOneEntry());
                                    break;
                                case "Runner Out":
                                    Add_Runner_Out(GetOneEntry());
                                    break;
                                case "Aid Station Issue":
                                    Add_Aid_Station_Issue(GetOneEntry());
                                    break;
                                case "Message":
                                    int startindex = String_Coming_from_Station.IndexOf(':') + 1;
                                    int endindex = String_Coming_from_Station.IndexOf((char)3);
                                    if (endindex == -1)
                                    {
                                        endindex += 0;
                                    }
                                    string message = String_Coming_from_Station.Substring(startindex, endindex - startindex);
                                    Add_In_Message(message);
                                    Form1.AddtoLogFile("Station: " + Station_Name + " sent this Incoming Message: " + message);
                                    if (endindex == String_Coming_from_Station.Length - 3)
                                        String_Coming_from_Station = "";        // clear the data
                                    else
                                    {
                                        endindex += 0;
                                    }
                                    break;
                                case "Request Runner":
                                    Send_Runner(GetOneEntry());
                                    break;
                                case "Request Station Info":
                                    GetOneEntry();
                                    Send_Station_Info_File();
                                    break;
                                case "Request Runner List":
                                    GetOneEntry();
                                    Send_RunnerList_File();
                                    break;
                                case "Request DNS List":
                                    GetOneEntry();
                                    Send_DNSList_File();
                                    break;
                                case "Request DNF List":
                                    GetOneEntry();
                                    Send_DNFList_File();
                                    break;
                                case "Request Watch List":
                                    GetOneEntry();
                                    Send_WatchList_File();
                                    break;
                                case "Request Info File":
                                    GetOneEntry();
                                    Send_Info_File();
                                    break;
                                case "Request RFID Assignments":
                                    GetOneEntry();
                                    Send_RFID_Assignments_File();
                                    break;
                                case "Start?":
                                    Send_RaceTime(Form1.Start_Time);
                                    break;
                                case "Log Points":
                                    NumLogPts = Convert.ToInt16(GetOneEntry());
                                    String_Coming_from_Station = "";        // clear the data
                                    break;
                                case "Watch Runner":
                                    Add_Watch_Runner(GetOneEntry());  // should be sending: Bib number, Station, Time, Notes
                                    break;
                                case "DNF Runner":
                                    Add_DNF_Runner(GetOneEntry());  // should be sending: Bib number, Station, Time, Notes
                                    break;
                                case "Request Issues":
                                    GetOneEntry();
                                    Send_ASIssues();
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                    if (String_Coming_from_Station == "")
                        Data_Received_from_Station = false;     // change flag after using data
                }
                Application.DoEvents();
            }
            Console.WriteLine("IP Station worker thread for IP: " + RemoteEndPointIP + ": terminating gracefully.");

            // close the connections
            StationClient.Close();
            MyNetStream.Close();
            Fivesecond.Stop();
            Fivesecond.Close();
        }

//        #region Asynchronuous Receive Functions
//        public void Receive_Thread(object client)    // receive data from Database
//        {
//            string teststring;

//            // give the thread a name
//            Thread.CurrentThread.Name = "Station Worker Receive thread";

//            // reset the flag
//            Restart_Receive = false;

//            // processing loop
//            while (!Server_shouldStop)      // test if need to stop
//            {
//                //                if (Connected_to_Server)    // test if Connected
//                if (Connected_to_DB)    // test if Connected
//                {
//                    #region Received Data
//                    // get the new receive data
//                    try
//                    {
//                        // Create the state object.
//                        StateObject state = new StateObject();
//                        state.workSocket = CentralClient;

//                        // clear the flag
//                        receiveDone.Reset();

//                        // Begin receiving the data from the remote device.
//                        CentralClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
//                            new AsyncCallback(Receive_Ethernet_Callback), state);
//                    }
//                    catch (Exception e)
//                    {
//                        Console.WriteLine(e.ToString());
//                    }
//                    receiveDone.WaitOne();      // wait here for any receive data

//                    // 7/22/16                    // test if we came out of Receive because we should stop
//                    // test if we came out of Receive because we should stop or Restart Receiving
//                    // 7/22/16                    if (!Server_shouldStop)
//                    if (!Server_shouldStop && !Restart_Receive)
//                    {
//                        while (Central_String_In != "")
//                        {       // continue looking until all received data has been looked at
//                        }
//                    }
//                    #endregion
//                }
//            }
//        }

//        void Receive_Ethernet_Callback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the state object and the client socket
//                // from the asynchronous state object.
//                StateObject state = (StateObject)ar.AsyncState;
//                Socket client = state.workSocket;

//                // Read data from the remote device.
//                if (client.Connected)   // make sure we are still connected (not disposed)
//                {
//                    int bytesRead = client.EndReceive(ar);
//                    if (bytesRead > 0)
//                    {
//                        // There might be more data, so store the data received so far.
//                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

//                        // Get the rest of the data.
//                        // changed this 2/20/16                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
//                        //                        new AsyncCallback(ReceiveCallback), state);
//                        // this change found here: https://social.msdn.microsoft.com/Forums/vstudio/en-US/05dcda20-06f9-419b-bfc0-bcb8cfeb3693/socket-receivecallback-not-sending-notification-when-buffer-has-emptied?forum=csharpgeneral
//                        if (state.workSocket.Available > 0)
//                        {
//                            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
//                                new AsyncCallback(Receive_Ethernet_Callback), state);
//                        }
//                        else
//                        {
//                            // All the data has arrived; put it in response.
//                            if (state.sb.Length > 1)
//                            {
////// 7/26 - for now                                Central_String_In = state.sb.ToString();
////                                Console.WriteLine("Received this from Central: " + Central_String_In);

////                                // display in the Richtextbox
////                                AddRichText(Ethernet_Packets, Central_String_In, Color.Black);
//                            }
//                            // Signal that all bytes have been received.
//                            receiveDone.Set();
//                        }
//                    }
//                    else
//                    {
//                        // All the data has arrived; put it in response.
//                        if (state.sb.Length > 1)
//                        {
//                            Central_String_In = state.sb.ToString();
//                        }
//                        else
//                        {
//                            //switch (Connection_Type)
//                            //{
//                            //    case Form1.Connect_Medium.Ethernet:
//                                    try
//                                    {
//                                        byte[] tmp = new byte[1];

//                                        CentralClient.Blocking = false;
//                                        CentralClient.Send(tmp, 0, 0);
//                                        //                                    return true;
//                                        CentralClient.Blocking = true;
//                                    }
//                                    catch (SocketException ex)
//                                    {
//                                        // 10035 == WSAEWOULDBLOCK
//                                        if (ex.NativeErrorCode.Equals(10035))
//                                        {
//                                            //                                        return true;
//                                        }
//                                        else
//                                        {
//                                            //                                        return false;
//                                            //                                            Connected_to_Server = false;
//                                            // 7/21/16                                            Connected_to_DB = false;
//// 7/26                                            ChangeState(Server_State.Error_Connecting);
//                                            CentralClient.Close();
//                                            CentralClient.Dispose();
//                                            Console.WriteLine("CentralClient has been closed because Poll failed");

//                                            // added 7/22/16
//                                            Restart_Receive = true;     // this will cause Receiving Thread to restart receiving
//                                            receiveDonebool = receiveDone.Set();      // this should clear the wait for receive data
//                                        }
//                                    }
//                                    finally
//                                    {
//                                        //CentralClient.Blocking = true;
//                                    }
//                            //        break;
//                            //}
//                        }

//                        // Signal that all bytes have been received.
//                        receiveDone.Set();
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());

//                // we may be here because we are shutting down
//                if (Server_shouldStop)
//                    receiveDone.Set();  // if so, get us out of here and test Server_shouldStop after it
//            }
//        }
//        #endregion

        private void Add_Aid_Station_Issue(string AidIssue)
        {
            if (AidIssue != "")
            {
                char[] splitter = new char[] { '|' };
                string[] Parts = AidIssue.Split(splitter);
                Form1.Queue_Issue issue = new Form1.Queue_Issue();
                issue.EntryDate = Parts[0];
                issue.ResolveDate = Parts[1];
                issue.EntryPerson = Parts[2];
                issue.Station = Parts[3];
                issue.Type = Parts[4];
                issue.Description = Parts[5];
                lock (Form1.AidStationIssuesQue)
                {// lock
                    Form1.AidStationIssuesQue.Enqueue(issue);
                    Form1.New_AidStation_Issue_entry = true;
                }// unlock
            }
        }

        private void Add_DNF_Runner(string runnerD)  // should be sending: Bib number, Station, Time, Notes
        {
            char[] splitter = new char[] { ',' };
            string[] Parts = runnerD.Split(splitter);
            Form1.RunnerDNFWatch runnerDNF = new Form1.RunnerDNFWatch();
            runnerDNF.BibNumber = Parts[0];
            runnerDNF.Station = Parts[1];
            runnerDNF.Time = Parts[2];
            runnerDNF.Note = Parts[3];
            lock (Form1.DNFInQue)
            {// lock
                Form1.DNFInQue.Enqueue(runnerDNF);
                Form1.New_DNF_Runner_entry = true;
            }// unlock
        }

        private void Add_Watch_Runner(string runnerW)  // should be sending: Bib number, Station, Time, Notes
        {
            char[] splitter = new char[] { ',' };
            string[] Parts = runnerW.Split(splitter);
            Form1.RunnerDNFWatch runnerWatch = new Form1.RunnerDNFWatch();
            runnerWatch.BibNumber = Parts[0];
            runnerWatch.Station = Parts[1];
            runnerWatch.Time = Parts[2];
            runnerWatch.Note = Parts[3];
            lock (Form1.WatchInQue)
            {// lock
                Form1.WatchInQue.Enqueue(runnerWatch);
                Form1.New_Watch_Runner_entry = true;
            }// unlock
        }

        string GetOneEntry()
        {   // returns just the substring after the ':' and including the first set of '\r\n'
            int firstentryreturnstartindex = String_Coming_from_Station.IndexOf(':') + 1;
            int firstentrysize = String_Coming_from_Station.IndexOf('\n') - 1;

            // first get the substring to be returned
            string returnstring = String_Coming_from_Station.Substring(firstentryreturnstartindex, firstentrysize - firstentryreturnstartindex);

            // then remove the entire first entry
            String_Coming_from_Station = String_Coming_from_Station.Remove(0, firstentrysize + 2); // first remove entire first entry

            return returnstring;
        }

        private void Add_Runner_Out(string numtime)
        {
            char[] splitter = new char[] { ',' };
            string[] Parts = numtime.Split(splitter);
            Form1.DB_Runner runnerOut = new Form1.DB_Runner();
            runnerOut.BibNumber = Parts[0];
            runnerOut.Station = Station_Name;
            runnerOut.TimeOut = Parts[1];
            lock (Form1.RunnerOutQue)       // need to use a queue because multiple Stations may be sending Runner numbers
            {// lock
                Form1.RunnerOutQue.Enqueue(runnerOut);
                Form1.New_RunnerOutQue_entry = true;
            }// unlock
        }

        private void Add_Runner_In(string numtime)
        {
            char[] splitter = new char[] { ',' };
            string[] Parts = numtime.Split(splitter);
            Form1.DB_Runner runnerIn = new Form1.DB_Runner();
            runnerIn.BibNumber = Parts[0];
            runnerIn.Station = Station_Name;
            runnerIn.TimeIn = Parts[1];
            lock (Form1.RunnerInQue)
            {// lock
                Form1.RunnerInQue.Enqueue(runnerIn);
                Form1.New_RunnerInQue_entry = true;
            }// unlock
        }

        private void Add_In_Message(string str)
        {
            DateTime Now = DateTime.Now;
            Message new_mess = new Message();
            Number_Incoming_Messages++;
            Form1.Stations[Stations_Index].Number_Incoming_Messages++;
            new_mess.Number = Number_Incoming_Messages;
            new_mess.Received = Now;
            new_mess.Size = str.Length;
            new_mess.TheMessage = str;
            Incoming_Messages.Add(new_mess);
            New_Message = true;
        }

        public void Add_Out_Message(string str)
        {
            DateTime Now = DateTime.Now;
            Message new_mess = new Message();
            Number_Outgoing_Messages++;
            new_mess.Number = Number_Outgoing_Messages;
            new_mess.Received = Now;
            new_mess.Size = str.Length;
            new_mess.TheMessage = str;
            Outgoing_Messages.Add(new_mess);
            Send_Message(str);
        }

        public void Add_Alert(string alert)
        {
            DateTime Now = DateTime.Now;
            Message new_alert = new Message();
            Number_Outgoing_Messages++;
            new_alert.Number = Number_Outgoing_Messages;
            new_alert.Received = Now;
            new_alert.Size = alert.Length;
            new_alert.TheMessage = alert;
            Outgoing_Messages.Add(new_alert);
            Send_Alert(alert);
        }

        void Send_Runner(string number)
        {
            Form1.AddtoLogFile(Station_Name + " requested status of Runner # " + number.ToString());
            Form1.RunnersStatus = Form1.Get_Runner_Status(number);
            foreach (Form1.RunnerStatus rs in Form1.RunnersStatus)
            {
                string line = string.Empty;
                if ((rs.TimeIn != "") || (rs.TimeOut != ""))
                {
                    line += rs.Station + "-";
                    if (rs.TimeIn != "")
                        line += rs.TimeIn;
                    line += ",";
                    if (rs.TimeOut != "")
                        line += rs.TimeOut;
                    line += ",";
                    if (rs.TimeAtStation != null)
                        line += rs.TimeAtStation;
                    line += ",";
                    if (rs.TimeFromPrev != null)
                        line += rs.TimeFromPrev;
                    line += ",";
                    if (rs.TimeToNext != null)
                        line += rs.TimeToNext;
//                    String_Going_to_Station = line;
                    String_Going_to_Station = line + Environment.NewLine;
                    Data_Ready_to_Send_to_Station = true;
                    // do we need to log this event?

                    // wait until this line has been sent
                    while (Data_Ready_to_Send_to_Station)
                    {
                        Application.DoEvents();
                        Application.DoEvents();
                    }
//                    Thread.Sleep(500);  // then wait an extra 1/2 sec.
                    Thread.Sleep(1000);  // then wait an extra 1 sec.
                }
                //String_Going_to_Station = "End-,,,,";
                //Data_Ready_to_Send_to_Station = true;
                //// do we need to log this event?
            }
            String_Going_to_Station = "End-,,,,";
            Data_Ready_to_Send_to_Station = true;
            // do we need to log this event?
        }

        void Send_RaceTime(string time)
        {
            String_Going_to_Station = "Start:" + time;
            Data_Ready_to_Send_to_Station = true;
            Form1.AddtoLogFile("Station: " + Station_Name + " requested the Race Start time, which is: " + time);
        }

        #region Files to send
        void Send_Station_Info_File()
        {
            Send_Complete_File("Stations Info File:", Form1.Stations_Info_Filename);
            Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Stations Info File from: " + Form1.Stations_Info_Filename);
            Console.WriteLine("Station: " + Station_Name + " downloaded the Stations Info File from: " + Form1.Stations_Info_Filename);
        }

        void Send_RunnerList_File()
        {
            if (Form1.RunnerList_Has_Entries)
            {
                Send_File("Runner List File:", Form1.RunnerListPath);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Runner List from: " + Form1.RunnerListPath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the Runner List from: " + Form1.RunnerListPath);
            }
            else
            {
                String_Going_to_Station = "Runner List File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_DNSList_File()
        {
            if (Form1.DNSList_Has_Entries)
            {
                Send_File("DNS List File:", Form1.DNSListPath);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the DNS List from: " + Form1.DNSListPath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the DNS List from: " + Form1.DNSListPath);
            }
            else
            {
                String_Going_to_Station = "DNS List File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_DNFList_File()
        {
            if (Form1.DNFList_Has_Entries)
            {
                Send_File("DNF List File:", Form1.DNFListPath);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the DNF List from: " + Form1.DNFListPath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the DNF List from: " + Form1.DNFListPath);
            }
            else
            {
                String_Going_to_Station = "DNF List File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_WatchList_File()
        {
            if (Form1.WatchList_Has_Entries)
            {
                Send_File("Watch List File:", Form1.WatchListPath);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Watch List from: " + Form1.WatchListPath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the Watch List from: " + Form1.WatchListPath);
            }
            else
            {
                String_Going_to_Station = "Watch List File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_Info_File()
        {
            if (Form1.InfoLoaded)
            {
                Send_File("Info File:", Form1.InfoFilePath);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Info File from: " + Form1.InfoFilePath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the Info File from: " + Form1.InfoFilePath);
            }
            else
            {
                String_Going_to_Station = "Info File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_ASIssues()
        {
            if (Form1.ASIssues_Has_Entries)
            {
                Send_File("Aid Issues File:", Form1.ASIssuesFilePath);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Aid Station Issues from: " + Form1.ASIssuesFilePath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the Aid Station Issues from: " + Form1.ASIssuesFilePath);
            }
            else
            {
                String_Going_to_Station = "Issues File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_Message(string mess)
        {
            String_Going_to_Station = "Message:" + mess;
            Data_Ready_to_Send_to_Station = true;
            Form1.AddtoLogFile("This Outgoing Message was sent to Station: " + Station_Name + ": " + mess);
            Console.WriteLine("This Outgoing Message was sent to Station: " + Station_Name + ": " + mess);
        }

        void Send_Alert(string alert)
        {
            String_Going_to_Station = "Alert:" + alert;
            Data_Ready_to_Send_to_Station = true;
            // do we need to log this event?
        }
                            
        void Send_RFID_Assignments_File()
        {
            Send_File("RFID Assignments File:", Form1.RFID_Assignments_Filename);
            Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the RFID Assignments File from: " + Form1.RFID_Assignments_Filename);
            Console.WriteLine("Station: " + Station_Name + " downloaded the RFID Assignments File from: " + Form1.RFID_Assignments_Filename);
        }

        void Send_File(string preface, string filename)
        {
            // do this only if the Filename is not empty
            if (filename != "")
            {
                try
                {
                    StreamReader reader = File.OpenText(filename);
                    string line;
//                    String_Going_to_Station = preface + reader.ReadToEnd();
                    String_Going_to_Station = preface;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (!line.StartsWith("*"))
                        {
                            String_Going_to_Station += (line + Environment.NewLine);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + filename + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    String_Going_to_Station = preface + "Selected file:\n\n" + filename + "\n\nis not accessible!";
                }
            }
            else
            {
                String_Going_to_Station = preface + "File requested is un-named!";
            }
            Data_Ready_to_Send_to_Station = true;
        }

        void Send_Complete_File(string preface, string filename)
        {
            // do this only if the Filename is not empty
            if (filename != "")
            {
                try
                {
                    StreamReader reader = File.OpenText(filename);
                    String_Going_to_Station = preface + reader.ReadToEnd();
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + filename + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    String_Going_to_Station = preface + "Selected file:\n\n" + filename + "\n\nis not accessible!";
                }
            }
            else
            {
                String_Going_to_Station = preface + "File requested is un-named!";
            }
            Data_Ready_to_Send_to_Station = true;
        }
        #endregion

        // Asynch example
//        #region
//        private void ConnectCallback(IAsyncResult result)
//        {
//            try
//            {
//                //We are connected successfully.

//                NetworkStream networkStream = tcpClient.GetStream();

//                byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

//                //Now we are connected start asyn read operation.

//                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
//            }
//              Catch(Exception ex)
//              {
//                Logger.WriteLog(LogLevel.Error, "ex.Message);
//               }
//        }



//        /// Callback for Read operation
//        private void ReadCallback(IAsyncResult result)
//        {

//            NetworkStream networkStream;

//            try
//            {

//                networkStream = tcpClient.GetStream();

//            }

//            catch
//            {
//                Logger.WriteLog(LogLevel.Warning, "ex.Message);
//             return;

//            }

//            byte[] buffer = result.AsyncState as byte[];

//            string data = ASCIIEncoding.ASCII.GetString(buffer, 0, buffer.Length);

//            //Do something with the data object here.

//            //Then start reading from the network again.

//            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);

//        }
//#endregion

        // this event handler actually sends and receives the data to/from the station
        // this happens every 5 seconds

        void StationFiveSecondTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Thread.CurrentThread.Name = "Station worker Time event thread for IP: " + RemoteEndPointIP;
            if (!Server_shouldStop)
            {
                // this prevents overlapping threads when debugging
                if (!Server_Busy)
                {
                    // test if the Station Name shoud have already been received
                    if (Name_Count > 1)
                        Name_Count--;

                    // test if just became connected
                    if (state == Server_State.ClientConnected)
                        if (Name_Count == 0)
                        {   // just received Station Name.  change flag to change state next time
                            Name_Count--;    // indicate change to Active next time
                        }
                        else
                        {   // change to Active state and send Welcome message
                            state = Server_State.Connected_Active;
                            
                            // send the Welcome message
                            String_Going_to_Station = "Welcome message:" + Form1.Welcome_Message;
                            Data_Ready_to_Send_to_Station = true;
                            Console.WriteLine("Sent Welcome message to: " + Station_Name + " at: " + RemoteEndPointIP);
                            Form1.AddtoLogFile("Sent Welcome message to: " + Station_Name + " at: " + RemoteEndPointIP);
                        }

                    // do only if connected
                    if (Connected_to_Station)
                    {
                        // first test if the connection is still good
                        try
                        {
                            bool mode = StationClient.Client.Poll(1, SelectMode.SelectRead);
                            int amt = StationClient.Client.Available;
                            if (StationClient.Client.Poll(1, SelectMode.SelectRead) && StationClient.Client.Available == 0)
                            {
                                Close_Station();
                            }
                        }
                        catch (SocketException)
                        {
                            Close_Station();
                        }

                        // test again, in case connection was lost
                        if (Connected_to_Station)
                        {
                            // set the flag
                            Server_Busy = true;

                            #region Receive Data
                            // test if there is any data available to read on the server
                            if (MyNetStream.DataAvailable)
                            {
                                Snumbytes = MyNetStream.Read(Sendbytes, 0, (int)StationClient.ReceiveBufferSize);
                                Server_String_In = Encoding.ASCII.GetString(Sendbytes);
                                Server_String_In = Server_String_In.Substring(0, Snumbytes);
                                String_Coming_from_Station = Server_String_In;
                                if (Station_Name == null)
//                                    AddRichText(Ethernet_Packets, "From " + RemoteEndPointIP + ": " + String_Coming_from_Station + Environment.NewLine, Color.Black);
                                    AddRichText(Ethernet_Packets, "From " + RemoteEndPointIP + ": " + String_Coming_from_Station, Color.Black);
                                else
//                                    AddRichText(Ethernet_Packets, "From " + Station_Name + ": " + String_Coming_from_Station + Environment.NewLine, Color.Black);
                                    AddRichText(Ethernet_Packets, "From " + Station_Name + ": " + String_Coming_from_Station, Color.Black);
//                                AddRichText(Ethernet_Packets, String_Coming_from_Station + Environment.NewLine, Color.Black);
                                Data_Received_from_Station = true;
                            }
                            #endregion

                            #region Data to Send
                            // test if there is any data ready to send to the Station
                            if (Data_Ready_to_Send_to_Station)
                            {
                                //try
                                //{
                                //    // send the data to the Server
                                //    Byte[] sendBytes = Encoding.ASCII.GetBytes(String_Going_to_Station);
                                //    MyNetStream.Write(sendBytes, 0, sendBytes.Length);

                                //    // clear the worker flag
                                //    Data_Ready_to_Send_to_Station = false;
                                //}
                                //catch (Exception except)
                                //{
                                //    Console.WriteLine(except.ToString());

                                //    // terminate this thread
                                //    Server_shouldStop = true;
                                //}

                                Send_Data_to_Station(String_Going_to_Station);
                            }
                            #endregion

                            // clear the flag
                            Server_Busy = false;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Station worker Time event thread for IP: " + RemoteEndPointIP + ": terminating gracefully.");

                // close the connections
                StationClient.Close();
                MyNetStream.Close();
                Fivesecond.Stop();
                Fivesecond.Close();
            }
        }

        private void Send_Data_to_Station(string data)
        {
            try
            {
                // put into OutputMessageQueue and get Message ID - 7/9/19
                int ID = Form1.Put_Message_in_Queue(Station_Name, "IP");
                data = (char)(1) + ID.ToString() + (char)(2) + data;    // put SOH ID STX in front

                // send the data to the Server
                Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                MyNetStream.Write(sendBytes, 0, sendBytes.Length);

                // put in the packet rich textbox
                if (Station_Name == null)
                    AddRichText(Ethernet_Packets, "To " + RemoteEndPointIP + ": " + data + Environment.NewLine, Color.Red);
                else
                    AddRichText(Ethernet_Packets, "To " + Station_Name + ": " + data + Environment.NewLine, Color.Red);

                // clear the worker flag
                Data_Ready_to_Send_to_Station = false;
            }
            catch (Exception except)
            {
                Console.WriteLine(except.ToString());

                // terminate this thread
                Server_shouldStop = true;
            }
        }

        void Close_Station()
        {
            Connected_to_Station = false;
            if (Station_List_index != -1)
            {
                Form1.Stations[Station_List_index].Active = false;
                Form1.Stations[Station_List_index].Number_Incoming_Messages = 0;
                Form1.Stations[Station_List_index].Number_Outgoing_Messages = 0;
                Form1.Stations[Station_List_index].IP_Address_Callsign = "";
                Form1.Stations[Station_List_index].Medium = "";
// 5/21/19 moved below                Form1.Stations_Activity_Flag = true;
            }
            Form1.Stations_Activity_Flag = true;    // 5/21/19 moved here to change top status to change to No Clients if new station
                    // tries to connect but is not accepted
            state = Server_State.Not_Initted;
            Form1.AddtoLogFile(Station_Name + " disconnected");
            Form1.Modeless_MessageBox_Exclamation("Station:\n\n" + Station_Name + "\n\nHas become disconnected!", "Station deactivated");
            Server_shouldStop = true;
        }

        public void RequestStop()
        {
            Server_shouldStop = true;
        }

        #region Delegates
        void AddRichText(RichTextBox rtb, string str, Color color)
        {
            if (!rtb.IsDisposed)
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (rtb.InvokeRequired)
                {
                    SetRichTextdel d = new SetRichTextdel(AddRichText);
                    rtb.Invoke(d, new object[] { rtb, str, color });
                }
                else
                {
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;
                    rtb.SelectionColor = color;
                    rtb.AppendText(str);
                    rtb.Update();
                }
            }
        }
        #endregion
    }
}
