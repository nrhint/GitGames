using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
//using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using System.Timers;
using System.Windows.Forms;

namespace RunnerTracker
{
    //
    //  The purpose for this worker thread is to handle communication with a station
    //

    //
    //  Updates:
    //  July 27, 2017 - fixed recognizing the need to acknodledge a message
    //  August 1, 2017 - removed message # from end of Station name
    //                   changed number in APRS_Message to msg_number
    //                   added number_times_resend to APRS_Message and incremented when resending, to max of 5, then disconnect station
    //                   began adding function Send_File_APRS(Filename)
    //  August 6, 2017 - added wait for previous line to be used in DB_APRS_Worker_Receive_Thread
    //  8/7/17 - added another test: (String_Coming_from_Station != "")
    //           began changing to use new method to send files - just Info file first
    //           changed Send_File_APRS_Thread to not send comment lines
    //  8/9/17 - changed time for File Sending Bulletin to 4 seconds between and put it before sending
    //  8/10/17 - added Send_Bib_List() and case in Start to handle it
    //            changed the Array creation code in Send_File_APRS_Thread to put multiple bib numbers on one line
    //  8/11/17 - changed Send_Alert to use Send_Bulletin
    //

    public class DB_APRS_StationWorker
    {
        #region Variables and declarations
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        public RichTextBox Packet_Packets { get; set; }
        private volatile String Central_String_In = string.Empty;
        public string Station_Callsign;
        public int Station_List_index = -1;
        string String_Going_to_Station;
        string String_Coming_from_Station;
        string Station_Name;
        public bool New_Message = false;
        public int Test6 = 6;
        public static int Test7 = 7;
        public int NumLogPts;
        public bool InitialLogPoint = true;
        int Stations_Index;
        int Name_Count;
        bool Server_shouldStop = false;
        bool Connected_to_Station = false;
        bool Data_Ready_to_Send_to_Station = false;     // this flag indicates data is ready to send to the station
        bool Data_Received_from_Station = false;        // this flag indicates data has been received from the station
// 8/5/17 - not used        StreamWriter sw_data_to_DBAGW;
        StreamReader sr_data_from_DBAGW;
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

        class APRS_Message
        {
            public int port { get; set; }
            public int msg_number { get; set; }
            public DateTime time_sent { get; set; }
            public string message { get; set; }
            public bool acknowledged { get; set; }
            public int number_times_resend { get; set; }      // 8/1/17 - max of 5, then disconnect
        }
        int Next_Msg_Num;
        List<APRS_Message> APRS_Message_Status;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile String Server_String_In;
        private volatile Byte[] Receivebytes;
        private volatile Byte[] Sendbytes;
        public volatile System.Timers.Timer Fivesecond = new System.Timers.Timer();
        private bool Server_Busy = false;
// 8/2/17        int Snumbytes;
        char[] EOLchars = { '\n', '\r' };

        // Asynchronous Client variables - added 2/16/16
        // State object for receiving data from remote device.
        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            //            public TcpClient workSocket = null;
            // Size of receive buffer.
            //            public const int BufferSize = 256;
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
// 8/2/17        private static bool receiveDonebool = true;

//        AnonymousPipeServerStream PacketOutData;    // data from AGWSocket to Worker
        AnonymousPipeClientStream PacketInData;     // data from Worker to AGWSocket
        string Pipehandle;
        #endregion

        public DB_APRS_StationWorker(RichTextBox rtb, string pipeHandle)
        {
            //// init the Pipe Stream Server to send the Packet data to the AGWSocket
            //PacketOutData = new AnonymousPipeServerStream(PipeDirection.Out);
            //Form1.PacketOutDataHandle = PacketOutData.GetClientHandleAsString();

            // save the PipeHandle for the Receive Thread init
            Pipehandle = pipeHandle;

            // save the rtb
            Packet_Packets = rtb;
        }

        public void Start(object client)
        {
            //// init the Pipe Stream Client to receive the Packet data from the AGWSocket
            //string Pipehandle;
            //do
            //{
            //    Thread.Sleep(10);
            //    Pipehandle = Form1.PacketInDataHandle;
            //} while (Pipehandle == null);
            //PacketInData = new AnonymousPipeClientStream(PipeDirection.In, Pipehandle);
            //sr_data_from_DBAGW = new StreamReader(PacketInData);
            //sw_data_to_DBAGW = new StreamWriter(PacketOutData);

            // create the Message lists
            Incoming_Messages = new List<Message>();
            Outgoing_Messages = new List<Message>();
            Number_Incoming_Messages = 0;
            Number_Outgoing_Messages = 0;
            APRS_Message_Status = new List<APRS_Message>();
            Next_Msg_Num = 1;

            // set the flag
            Connected_to_Station = true;
            NumLogPts = 0;      // 0 = indicates no change.  != 0 tells new Number of Log Points.

            // get the station Callsign
            Station_Callsign = (String)client;

            // tell the world it has connected
            AddRichText(Packet_Packets, "Connected to: " + Station_Callsign + Environment.NewLine, Color.Green);

            // give the thread a name
            Thread.CurrentThread.Name = "APRS Station worker thread for Callsign: " + Station_Callsign;

            // start a five second timer for the actual data processing
            Name_Count = 60;     // allow 300 seconds = 5 minutes (60 * 5) for the Station Name to come in
            Fivesecond.Interval = 5000;
            Fivesecond.Elapsed += new System.Timers.ElapsedEventHandler(APRSstationTimeEvent);
            Fivesecond.Start();

            // send request for Station Name
            String_Going_to_Station = "Station name?";
            Data_Ready_to_Send_to_Station = true;
            Console.WriteLine("Sent Station Name request to: " + Station_Callsign);
            state = Server_State.Expecting_StationName;

            // processing loop
            while (!Server_shouldStop)
            {
                #region Test for station name being received
                if (Name_Count == 1)
                {
                    Name_Count--;
                    if (state == Server_State.Expecting_StationName)
                    {
                        MessageBox.Show("Station with Callsign:\n\n   " + Station_Callsign + "\n\nHas not responded with it's Station Name.\n\nThis station will be removed!", "Station Name not received", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        Server_shouldStop = true;       // terminate this thread
                        Form1.Stations_Activity_Flag = true;     // tell Form1 that a station has changed
                    }
                }
                #endregion

                #region If data received
                if (Data_Received_from_Station)
                {
                    // if this is an APRS message, acknowledge it
                    string charit = String_Coming_from_Station.Substring(String_Coming_from_Station.Length - 7, 1);    // expecting: '{nnnn\r'
// 7/27/17                    if (String_Coming_from_Station.Substring(String_Coming_from_Station.Length - 7) == "{")
                    if (String_Coming_from_Station.Substring(String_Coming_from_Station.Length - 7, 1) == "{")      // 7/27/17
                    {
                        string number = String_Coming_from_Station.Substring(String_Coming_from_Station.IndexOf("{") + 1);        // 8/6/17
// 8/6/17                        Form1.DB_AGWSocket.SendMessageWOid(Form1.AGWPERadioPort, Station_Callsign, "ack" + "");
                        Form1.DB_AGWSocket.SendMessageWOid(Form1.AGWPERadioPort, Station_Callsign, "ack" + number);     // 8/6/17
                    }

                    // if this is an acknowledge, handle in APRS Message List
                    if (String_Coming_from_Station.StartsWith("ack"))
                    {
  //                      string numbstr = String_Coming_from_Station.Substring(3);
//                        APRS_Message mess = APRS_Message_Status.Find(x => x.number == Convert.ToInt16(numbstr));
// 8/6/17                        APRS_Message mess = APRS_Message_Status.Find(x => x.msg_number == Convert.ToInt16(String_Coming_from_Station.Substring(3)));
                        APRS_Message mess = APRS_Message_Status.Find(x => x.msg_number == Convert.ToInt16(String_Coming_from_Station.Substring(3,4)));    // 8/6/17
                        mess.acknowledged = true;

                        String_Coming_from_Station = "";        // and clear the data
                    }

                    if (String_Coming_from_Station != "")        // added 8/7/17
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

                                    // also need to remove the message # - 8/1/17
                                    Station_Name = Station_Name.Substring(0, Station_Name.IndexOf('{'));    // 8/1/17

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
                                            MessageBox.Show("Station with name of: " + Station_Name + "\n\nand Callsign of: " + Station_Callsign + "\n\nhas issued another request to connect", "Duplicate Connect Request", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            Server_shouldStop = true;       // terminate this thread, removing this station request
                                        }
                                        else
                                        {
                                            // add his IP info and set active
                                            Form1.Stations[Station_List_index].APRS_StationWorker = this;
                                            Form1.Stations[Station_List_index].Active = true;
                                            Form1.Stations[Station_List_index].IP_Address_Callsign = Station_Callsign;   // other: Callsign
                                            Form1.Stations[Station_List_index].Medium = "APRS";       // other: AGWPE/Packet
                                            Form1.Stations_Activity_Flag = true;
                                            state = Server_State.ClientConnected;
                                            Name_Count = 0; // use as a flag
                                            Stations_Index = Station_List_index;
                                            Form1.AddtoLogFile(Station_Name + " connected");
                                            Form1.Modeless_MessageBox_Information("Station:\n\n" + Station_Name + "\n\nHas just connected!", "Station activated");
                                        }
                                    }
                                    else
                                    {       // this station name does not exist in the Station list - ask the user if it should be added
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(New_Station_Question));
                                        //DialogResult res = MessageBox.Show("New station, with this name:\n\n" + Station_Name + "\n\nAdd it to the Stations List?", "New Station", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                                        //    if (res == DialogResult.Yes)
                                        //    {
                                        //        Form1.NewStation newst = new Form1.NewStation();
                                        //        newst.Name = Station_Name;
                                        //        newst.APRS_StationWorker = this;
                                        //        newst.IP_Address_Callsign = Station_Callsign;
                                        //        newst.Medium = Form1.Connect_Medium.APRS.ToString();
                                        //        lock (Form1.NewActiveStationQue)
                                        //        {// lock
                                        //            Form1.NewActiveStationQue.Enqueue(newst);
                                        //            Form1.New_Active_Station_entry = true;
                                        //        }// unlock
                                        //    }
                                    }
                                }
                                Data_Received_from_Station = false;     // change flag after using data
                                String_Coming_from_Station = "";        // and clear the data
                                break;
                            default:
                                // try to decode the data coming in
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
                                            endindex += 0;
                                        string message = String_Coming_from_Station.Substring(startindex, endindex - startindex);
                                        Add_In_Message(message);
                                        Form1.AddtoLogFile("Station: " + Station_Name + " sent this Incoming Message: " + message);
                                        String_Coming_from_Station = String_Coming_from_Station.Remove(0, endindex + 3);
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
                                    case "Request Bib List":
                                        GetOneEntry();
                                        Send_Bib_List();
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
                                        int x = 9;  // breakpoint here
                                        break;
                                }
                                break;
                        }
                    }       // 8/7/17
//                    }
                    if (String_Coming_from_Station == "")
                        Data_Received_from_Station = false;     // change flag after using data
                }
                #endregion

                #region Test if an APRS message is overdue being acknowledged
                foreach(APRS_Message mess in APRS_Message_Status)
                {
                    if (!mess.acknowledged)
                    {
                        DateTime Now = DateTime.Now;
                        TimeSpan elapsed = Now - mess.time_sent;
                        if (elapsed.Seconds > 35)
                        {
                            // 8/1/17 - Resend the message only up to 5 times, then disconnect
                            if (mess.number_times_resend < 5)   // 8/1/17
                            {
                                // resend the message
                                Form1.DB_AGWSocket.SendMessageWithID(mess.port, Station_Callsign, mess.message, mess.msg_number);
                                mess.time_sent = Now;   // update the time
                                Console.WriteLine("Had to resend APRS message # " + mess.msg_number.ToString() + " to: " + Station_Callsign);

                                // increment the number of times it has been resent - 8/1/17
                                mess.number_times_resend++;     // 8/1/17
                            }
                            else
                            {
                                // disconnect the station
                                int e = 34;
                            }
                        }
                    }
                }
                #endregion

                #region Test if Form1 is still active
                if (!Form1.DBRole)
                    RequestStop();  // stop this StationWorker
                #endregion

                Application.DoEvents();
            }
            Console.WriteLine("APRS Station worker thread for Callsign: " + Station_Callsign + ": terminating gracefully.");
            
//            Form1.DB_AGWSocket.CloseAGWPE();
            Fivesecond.Stop();
            Fivesecond.Close();
        }

        private void New_Station_Question(object info)
        {
            DialogResult res = MessageBox.Show("New station, with this name:\n\n" + Station_Name + "\n\nAdd it to the Stations List?", "New Station", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes)
            {
                Form1.NewStation newst = new Form1.NewStation();
                newst.Name = Station_Name;
                newst.APRS_StationWorker = this;
                newst.IP_Address_Callsign = Station_Callsign;
                newst.Medium = Form1.Connect_Medium.APRS.ToString();
                lock (Form1.NewActiveStationQue)
                {// lock
                    Form1.NewActiveStationQue.Enqueue(newst);
                    Form1.New_Active_Station_entry = true;
                }// unlock
            }
        }

        void SendNewAPRSmessage(int port, string data)
        {
            Form1.DB_AGWSocket.SendMessageWithID(port, Station_Callsign, data, Next_Msg_Num);

            APRS_Message newmess = new APRS_Message();
            newmess.time_sent = DateTime.Now;
            newmess.acknowledged = false;
            newmess.message = data;
            newmess.msg_number = Next_Msg_Num;
            newmess.port = port;
            newmess.number_times_resend = 0;        // 8/1/17
            APRS_Message_Status.Add(newmess);       // we add new entries, but never remove any. Should we?

            Next_Msg_Num++;
        }

        #region File Sending Bulletins
        enum FileType { BibRunners, CompRunners, RunnerStatus, DNS, DNF, Watch, Info, Stations }
        private void Send_File_APRS(FileType Filetype)    // began adding 8/1/17
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Send_File_APRS_Thread), Filetype);      // 8/2/17
        }

        private void Send_File_APRS_Thread(object info)     // 8/2/17
        {
            // Files will be sent in APRS Format as Bulletins
            //    The Bulletin will show the File name and a designation of what line in the file it is,
            //      ie.  Info:3/7
            //  Files that need to be sent:
            //      Runners, DNS, DNF, Watch, Info
            //
            // the prefix format:
            //      : 9 chars :
            //      :Fxy\zzz  :
            //      where x = 1 - 9 : 1 = Bib only Runners list
            //                        2 = Complete Runners list
            //                        3 = Runner status
            //                        4 = DNS
            //                        5 = DNF
            //                        6 = Watch         not included, but might be: RFID, Start time, Issues
            //                        7 = Info
            //                        8 = Stations
            //                        9 = undefined
            //            y = line number of total lines in file, can be 1 - 3 digits
            //            z = total number of lines in the file, can be 1 - 3 digits

            FileType Filetype = (FileType)info;

            // Build the collection of lines that comprise the file
            string Filename = string.Empty;
            string prefix = string.Empty;
            switch (Filetype)
            {
                case FileType.BibRunners:
                    Filename = Form1.RunnerListPath;    // 8/10/17
                    prefix += "F1";     // 8/7/17 - used to have ':' before F
                    break;
                case FileType.CompRunners:
                    Filename = Form1.RunnerListPath;
                    prefix += "F2";
                    break;
                case FileType.RunnerStatus:
//                    Filename = Form1.WatchListPath;
                    prefix += "F3";
                    break;
                case FileType.DNS:
                    Filename = Form1.DNSListPath;
                    prefix += "F4";
                    break;
                case FileType.DNF:
                    Filename = Form1.DNFListPath;
                    prefix += "F5";
                    break;
                case FileType.Watch:
                    Filename = Form1.WatchListPath;
                    prefix += "F6";
                    break;
                case FileType.Info:
                    Filename = Form1.InfoFilePath;
                    prefix += "F7";
                    break;
                case FileType.Stations:
                    Filename = Form1.Stations_Info_Filename;
                    prefix += "F8";
                    break;
            }

            // create the Array to send
            int numlines = File.ReadLines(Filename).Count();    // 8/7/17 - this also includes the comment lines
            string[] Lines = new string[numlines];          // 8/7/17 - this also includes the comment lines
            string lineread;        // 8/7/17
            string newline = string.Empty;  // 8/10/17
            int numlinesnotcomment = 0;     // 8/7/17
            using (var reader = File.OpenText(Filename))
            {
                if (prefix == "F1")     // 8/10/17 - handle special for Bib only
                {       // 8/10/17
                    while (!reader.EndOfStream)
                    {
                        lineread = reader.ReadLine();
                        if (!lineread.StartsWith("*"))
                        {
                            int indx = lineread.IndexOf(',');
                            if (indx == -1)     // line could be just a number or contain other data
                                newline += lineread + "\r";     // file only has numbers
                            else
                                newline += lineread.Substring(0,indx) + "\r";    // file includes other data as well as the Bib number
                            if (newline.Length >= 65)       // limit the size of the line to send
                            {
                                Lines[numlinesnotcomment] = newline;
                                numlinesnotcomment++;
                                newline = "";       // clear it again
                            }
                        }
                    }
                    if (newline != "")
                    {
                        Lines[numlinesnotcomment] = newline;
                        numlinesnotcomment++;
                    }
                }       // 8/10/17
                else
                {
                    while (!reader.EndOfStream)
                    {
                        // 8/8/17                    Lines[i] = reader.ReadLine();
                        lineread = reader.ReadLine();       // 8/7/17
                        if (!lineread.StartsWith("*"))      // 8/7/17 - ignore comment lines
                        {       // 8/7/17
                            Lines[numlinesnotcomment] = lineread;
                            numlinesnotcomment++;
                        }       // 8/7/17
                    }
                }
            }

            // send the lines one at a time, with 3 seconds between each one
// 8/7/17            i = 1;
            int i = 1;
// 8/7/17            string numlinesstr = numlines.ToString("D3");   // total number of lines always displayed as 3 digit number
            string numlinesstr = numlinesnotcomment.ToString("D3");   // total number of lines always displayed as 3 digit number
            foreach (string line in Lines)
            {
                // 8/7/17 - needed to add check for the comment lines that were not added
                if (line != null)   // 8/7/17
                {
                    // wait 4 seconds - changed 8/9/17
                    Thread.Sleep(4000);     // 8/9/17

                    // send the line
                    // 8/7/17                string send = prefix + i.ToString("D3") + "\\" + numlinesstr + ":" + line.Substring(0, Math.Min(67, line.Length));    // last section truncates the text to 67 chars
                    Send_Bulletin(prefix + i.ToString("D3") + "\\" + numlinesstr, line.Substring(0, Math.Min(67, line.Length)));

// 8/9/17                    // wait 3 seconds
// 8/9/17                    Thread.Sleep(3000);

                    // increment the counter
                    i++;
                }
            }
        }

        //  8/7/17
        void Send_Bulletin(string prefix, string data)      // 8/7/17
        {
            lock (Form1.Packet_Send_Que)
            {
                Form1.Packet_Send packet = new Form1.Packet_Send();
                packet.port = Form1.AGWPERadioPort;
                packet.Callsign = prefix;
                packet.data = data;
                packet.APRS = true;
                packet.number = 0;      // this tells it to not use messaging format
                Form1.Packet_Send_Que.Enqueue(packet);
            }
        }
        #endregion

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
// 8/7/17            Send_Complete_File("Stations Info File:", Form1.Stations_Info_Filename);
            Send_File_APRS(FileType.Stations);      // 8/7/17
            Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Stations Info File from: " + Form1.Stations_Info_Filename);
            Console.WriteLine("Station: " + Station_Name + " downloaded the Stations Info File from: " + Form1.Stations_Info_Filename);
        }

        void Send_RunnerList_File()
        {
            if (Form1.RunnerList_Has_Entries)
            {
// 8/7/17                Send_File("Runner List File:", Form1.RunnerListPath);
                Send_File_APRS(FileType.CompRunners);
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Runner List from: " + Form1.RunnerListPath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the Runner List from: " + Form1.RunnerListPath);
            }
            else
            {
                String_Going_to_Station = "Runner List File is not available";
                Data_Ready_to_Send_to_Station = true;
            }
        }

        void Send_Bib_List()        // added 8/10/17
        {
            if (Form1.RunnerList_Has_Entries)
            {
                Send_File_APRS(FileType.BibRunners);    // 8/10/17
                Form1.AddtoLogFile("Station: " + Station_Name + " downloaded the Bib List from: " + Form1.RunnerListPath);
                Console.WriteLine("Station: " + Station_Name + " downloaded the Bib List from: " + Form1.RunnerListPath);
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
// 8/7/17                Send_File("DNS List File:", Form1.DNSListPath);
                Send_File_APRS(FileType.DNS);
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
// 8/7/17                Send_File("DNF List File:", Form1.DNFListPath);
                Send_File_APRS(FileType.DNF);
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
// 8/7/17                Send_File("Watch List File:", Form1.WatchListPath);
                Send_File_APRS(FileType.Watch);
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
// 8/7/17                Send_File("Info File:", Form1.InfoFilePath);
                Send_File_APRS(FileType.Info);
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
            // 8/11/17            String_Going_to_Station = "Alert:" + alert;
            // 8/11/17            Data_Ready_to_Send_to_Station = true;
            // 8/11/17            // do we need to log this event?

            // 8/11/17 - for APRS, use Bulletins, so all stations can see it
            Send_Bulletin("Alert", alert);
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

        public void DB_APRS_Worker_Receive_Thread(object client)
        {
            // give the thread a name
            Thread.CurrentThread.Name = "APRS Station Worker Receive thread via Pipe";

            PacketInData = new AnonymousPipeClientStream(PipeDirection.In, Pipehandle);
            sr_data_from_DBAGW = new StreamReader(PacketInData);
            
            // processing loop
            while (!Server_shouldStop)      // test if need to stop
            {
// original method                string temp = string.Empty;
// 4/17/16                while ((temp = sr_data_from_DBAGW.ReadLine()) != null)
                string rcvd_data = sr_data_from_DBAGW.ReadLine();        // removes '/r' from end of line - stalls here when trying to close
                Console.WriteLine("DB_APRS_Stationworker received this from DB_AGWSocket: " + rcvd_data);    // testing 8/1/17
                if (!Server_shouldStop)     // test again if just stopped
                {
                    if ((rcvd_data != null) && (rcvd_data != ""))
                    {
                        // 8/6/17 - wait until the previous line has been used
                        while (Data_Received_from_Station)  // 8/6/17
                            Application.DoEvents();         // 8/6/17

                        String_Coming_from_Station += rcvd_data + "\r\n";        // 4/2/16 - add the End of Line back in
                        Console.WriteLine("Received this from " + Station_Callsign + ": " + String_Coming_from_Station);
                        // 5/31/17                    AddRichText(Packet_Packets, "Received from " + Station_Callsign + ":" + String_Coming_from_Station, Color.Black);
                        Data_Received_from_Station = true;
                    }
                    Application.DoEvents();
                }
// 8/5/17                int ch = sr_data_from_DBAGW.Peek();
// 8/5/17                rcvd_data = sr_data_from_DBAGW.ReadLine();
                //                sr_data_from_DBAGW.Close();
                //              sr_data_from_DBAGW.Dispose();
                //                PacketInData.Close();
                //              PacketInData.Dispose();
                //            PacketInData = new AnonymousPipeClientStream(PipeDirection.In, Pipehandle);
                //            sr_data_from_DBAGW = new StreamReader(PacketInData);
                //          string rcvd_data2 = sr_data_from_DBAGW.ReadLine();        // removes '/r' from end of line - stalls here when trying to close
            }

            // Stop requested
            Console.WriteLine("APRS Station Worker Receive thread terminating gracefully.");
        }

        // this event handler actually sends and receives the data to/from the station
        // this happens every 5 seconds
        void APRSstationTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Thread.CurrentThread.Name = "APRS Station worker Time event thread for Callsign: " + Station_Callsign;
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
                            Console.WriteLine("Sent Welcome message to: " + Station_Name + " at: " + Station_Callsign);
                            Form1.AddtoLogFile("Sent Welcome message to: " + Station_Name + " at: " + Station_Callsign);
                        }

                    // do only if connected
                    if (Connected_to_Station)
                    {
                        // set the flag
                        Server_Busy = true;

                        #region Receive data from AGWSocket - nothing here
                            //                            // test if there is any data available to read on the server
                            ////// Aid method                            if (Form1.DB_AGWSocket.Receive_DataAvailable)
                            //                            string temp = string.Empty;
                            //                            if ((temp = sr_data_from_DBAGW.ReadLine()) != null)
                            //                            {
                            ////// Aid method                                lock (Form1.Aid_AGWSocket.Receive_Buffer)
                            ////                                {
                            ////                                    Central_String_In += Form1.Aid_AGWSocket.Receive_Buffer;
                            ////                                    Form1.Aid_AGWSocket.Receive_Buffer = "";
                            ////                                    Form1.Aid_AGWSocket.Receive_DataAvailable = false;
                            ////                                }
                            //                                Central_String_In += temp;
                            //                                Console.WriteLine("Received this from Central: " + Central_String_In);

                            //                                Data_Received_from_Station = true;
                            //                            }
                            #endregion

                        #region Send data to Station, using APRS messaging
                        // test if there is any data ready to send to the Station
                        if (Data_Ready_to_Send_to_Station)
                        {
                            try
                            {
                                //                                Send_to_DBAGW(Form1.AGWPERadioPort, Station_Callsign, String_Going_to_Station);
                                lock (Form1.Packet_Send_Que)
                                {
                                    Form1.Packet_Send packet = new Form1.Packet_Send();
                                    packet.port = Form1.AGWPERadioPort;
                                    packet.Callsign = Station_Callsign;
                                    packet.data = String_Going_to_Station;
                                    packet.APRS = true;
                                    packet.number = Next_Msg_Num;
                                    Form1.Packet_Send_Que.Enqueue(packet);
                                }
//                                Form1.DB_AGWSocket.SendMessageWithID(port, Station_Callsign, data, Next_Msg_Num);

                                APRS_Message newmess = new APRS_Message();
                                newmess.time_sent = DateTime.Now;
                                newmess.acknowledged = false;
                                newmess.message = String_Going_to_Station;
                                newmess.msg_number = Next_Msg_Num;
                                newmess.port = Form1.AGWPERadioPort;
                                newmess.number_times_resend = 0;        // 8/1/17
                                APRS_Message_Status.Add(newmess);       // we add new entries, but never remove any. Should we?

                                Next_Msg_Num++;

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
                        #endregion

                        // clear the flag
                        Server_Busy = false;
                    }
                }
            }
            else
            {
                Console.WriteLine("APRS Station worker Time event thread for Callsign: " + Station_Callsign + ": terminating gracefully.");

                // close the connections
//                StationClient.Close();
//                MyNetStream.Close();
                Fivesecond.Stop();
                Fivesecond.Close();
            }
        }

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
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;    // this should keep the vertical slider at the bottom of the multiline testbox
                    rtb.Update();
                }
            }
        }

        void Close_Station(string response)
        {
            Connected_to_Station = false;
// moved below            Server_shouldStop = true;
            if (Station_List_index != -1)
            {
                Form1.Stations[Station_List_index].Active = false;
                Form1.Stations[Station_List_index].Number_Incoming_Messages = 0;
                Form1.Stations[Station_List_index].Number_Outgoing_Messages = 0;
                Form1.Stations[Station_List_index].IP_Address_Callsign = "";
                Form1.Stations[Station_List_index].Medium = "";
                Form1.Stations_Activity_Flag = true;
            }
            state = Server_State.Not_Initted;
            Form1.AddtoLogFile(Station_Name + " disconnected");
            Form1.Modeless_MessageBox_Exclamation("Station:\n\n" + Station_Name + "\n\nHas become disconnected!", "Station deactivated");
            Server_shouldStop = true;
        }

        public void RequestStop()
        {
            Server_shouldStop = true;
            Form1.DB_AGWSocket.Send_Data_to_Packet_Worker(Station_Callsign, "QUIT!\r");   // this should clear the queue
                                                                                          // did not work            sr_data_from_DBAGW.Close();
                                                                                          // did not work StreamWriter sw_data_to_Packet_StationWorker = new StreamWriter(Pipehandle);
                                                                                          //sw_data_to_Packet_StationWorker.Write("QUIT!\r");
        }
    }
}
