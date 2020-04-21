using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace RunnerTracker
{
    public class Worker
    {
        #region Variables and declarations
        delegate void MakeVisibledel(Control cntrl, bool state);
        delegate void SetTextdel(TextBox tb, string str);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        delegate void SetBtnTextdel(Button btn, string str);
        public Label Server_Not_Initted { get; set; }
        public Label Server_Connected { get; set; }
        public Label Server_Connected_Active { get; set; }
        public Label Server_Waiting { get; set; }
        public Label Cannot_Connect { get; set; }
        public Label Error_Connecting { get; set; }
        public TextBox Server_Error_Message { get; set; }
        public RichTextBox Ethernet_Packets { get; set; }
        public RichTextBox Packet_Packets { get; set; }
        public string Station_Name { get; set; }
        public Button Connect_Button { get; set; }
        public Button Download { get; set; }
        public string Server_IP_Address { get; set; }
        public int Server_Port_Number { get; set; }
        public string Command { get; set; }
        public string Data { get; set; }
        public string Welcome_Message { get; set; }
        public string Downloaded_Stations_Info { get; set; }    // worker sets this with Stations File data, so Form1 can pull it out
        public string Downloaded_Runner_List { get; set; }    // worker sets this with DNS List data, so Form1 can pull it out
        public string Downloaded_DNS_List { get; set; }    // worker sets this with DNS List data, so Form1 can pull it out
        public string Downloaded_DNF_List { get; set; }    // worker sets this with DNF List data, so Form1 can pull it out
        public string Downloaded_Watch_List { get; set; }    // worker sets this with Watch List data, so Form1 can pull it out
        public string Downloaded_Issues { get; set; }    // worker sets this with Issues List data, so Form1 can pull it out
        public string Downloaded_Info { get; set; }    // worker sets this with Info File data, so Form1 can pull it out
        public bool Stations_Download_Complete { get; set; }   // set when the Stations File download is complete
        public bool Runners_Download_Complete { get; set; }   // set when the Runner List download is complete
        public bool DNS_Download_Complete { get; set; }   // set when the DNS download is complete
        public bool DNF_Download_Complete { get; set; }   // set when the DNF download is complete
        public bool Watch_Download_Complete { get; set; }   // set when the Watch download is complete
        public bool Issues_Download_Complete { get; set; }   // set when the Issues download is complete
        public bool Info_Download_Complete { get; set; }   // set when the Info download is complete
        public bool Stations_Not_Available { get; set; }   // set when the Stations File download is Not available
        public bool Runners_Not_Available { get; set; }   // set when the Runners List download is Not available
        public bool DNS_Not_Available { get; set; }   // set when the DNS download is Not available
        public bool DNF_Not_Available { get; set; }   // set when the DNF download is Not available
        public bool Watch_Not_Available { get; set; }   // set when the Watch download is Not available
        public bool Issues_Not_Available { get; set; }   // set when the Issues download is Not available
        public bool Info_Not_Available { get; set; }   // set when the Info download is Not available
        public bool Connected_to_Server { get; set; }   // = false;
        public bool Connected_and_Active { get; set; }
        public static bool Runner_Status_Received { get; set; }
        public bool Attempting_to_Connect_to_Server { get; set; }   // = false;
        public Form1.Connect_Medium Connection_Type { get; set; }
        public Form1.Connect_Medium New_Connection_Type { get; set; }
        bool StationName_Sent = false;
        string error;

        bool Already_Tried_To_Connect = false;
        bool Central_Data_Ready_to_Send = false;    // this flag indicates that there is data to send to the Central Database server
        bool Central_Data_Received = false;         // this flag indicates that data has been received from the Central Database server
        String Central_String_Out;      // data going to the Central Database server

        public enum Server_State { Not_Initted, Waiting, Connected, Connected_Active, Error }
        public Server_State state = Server_State.Not_Initted;
        public enum Expecting_State { Nothing, StationName_Request, Welcome_Message, Station_Info_File, Connected_Active, Runner_List, DNS_List, DNF_List, Watch_List, Issues, Info_File, Request_Runner }
        Expecting_State expecting = Expecting_State.StationName_Request;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool Server_shouldStop = false;
        private volatile bool Connect_Request = false;
        private volatile String Central_String_In = string.Empty;
        private volatile TcpClient ServerClient;
        private volatile NetworkStream MyNetStream;
        private volatile char[] charsToTrim = { '\0' };      // need to remove all the nulls at the end of the buffer
        private volatile Byte[] Receivebytes;
        private volatile Byte[] Sendbytes;
        public volatile System.Timers.Timer Onesecond = new System.Timers.Timer();
        int Snumbytes;
        #endregion

        public void DoServerWork(object client)
        {
            string teststring;

            // give the thread a name
            Thread.CurrentThread.Name = "Station Worker thread";

            // change the state to Waiting
            Connected_to_Server = false;
            Connected_and_Active = false;
            Attempting_to_Connect_to_Server = false;
            ChangeState(Server_State.Waiting);

            // clear the flags
            DNS_Download_Complete = false;
            DNF_Download_Complete = false;
            Watch_Download_Complete = false;
            DNS_Not_Available = false;
            DNF_Not_Available = false;
            Watch_Not_Available = false;
            Runner_Status_Received = false;

            // start a one second timer for the actual data processing
            Onesecond.Interval = 1000;
            Onesecond.Elapsed += new System.Timers.ElapsedEventHandler(ServerTimeEvent);
            Onesecond.Start();

            // attempt to Connect
            Attempt_To_Connect();      // Connected_to_Server will not be true if not connected

            // processing loop
            while (!Server_shouldStop)      // test if need to stop
            {
                if (Connected_to_Server)    // test if Connected
                {
                    #region Received Data
                    if (Central_Data_Received)
                    {
                        // Alerts can come at any time
                        if (Central_String_In.StartsWith("Alert:"))
                        {
                            string input = GetOneEntry();
                            Form1.Incoming_Alert = input;
                            Form1.Incoming_Alrt = true;
                        }

                        // what are we expecting
                        switch (expecting)
                        {
                            case Expecting_State.Nothing:
                                // could be an incoming message
                                if (Central_String_In.StartsWith("Message:"))
                                {
                                    Form1.Incoming_Message = Central_String_In.Substring(8);
                                    Form1.Incoming_Mess = true;
                                    Central_String_In = "";
                                }
                                if (Central_String_In.StartsWith("Alert:"))
                                {
                                    Form1.Incoming_Alert = Central_String_In.Substring(6);
                                    Form1.Incoming_Alrt = true;
                                    Central_String_In = "";
                                }
                                if (Central_String_In.StartsWith("Start:"))
                                {
                                    Form1.Start_Time = Central_String_In.Substring(6);
                                    Form1.Start_Time_Rcvd = true;
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.StationName_Request:
                                // send the station name
                                if (Central_String_In.StartsWith("Station name?"))
                                {
                                    SendCommand("Station name = " + Station_Name, Expecting_State.Connected_Active);
                                    Console.WriteLine("Sent Station name to Central Database");
                                    StationName_Sent = true;
                                    Central_String_In = "";

                                    // change the Expected State
                                    expecting = Expecting_State.Connected_Active;
                                }
                                break;
                            case Expecting_State.Welcome_Message:
                                teststring = "Welcome message:";
                                Welcome_Message = Central_String_In.Substring(teststring.Length);
                                Console.WriteLine("Welcome message = " + Central_String_In);
                                Central_String_In = "";

                                // change the Expected State
                                expecting = Expecting_State.Nothing;
                                break;
                            case Expecting_State.Station_Info_File:
                                // verify that the response is for this action
                                teststring = "Stations Info File:";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    Downloaded_Stations_Info = Central_String_In.Substring(teststring.Length);
                                    if (Downloaded_Stations_Info.StartsWith("inaccessible"))
                                    {       // file not available
                                        Modeless_MessageBox("Stations List is not available", "List data not available");
                                        Stations_Not_Available = true;
                                    }
                                    else
                                    {
                                        SetBtnText(Download, "Downloaded\nfrom Central");
                                        Stations_Download_Complete = true;
                                        Modeless_MessageBox("Station Info File received from Central Database\n\n   Click the Save button to save it on this PC.", "File data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.Runner_List:
                                // verify that the response is for this action
                                teststring = "Runner List File";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    SetBtnText(Download, "Refresh");
                                    Downloaded_Runner_List = Central_String_In.Substring(teststring.Length + 1);
                                    if (Downloaded_Runner_List.EndsWith("is not available"))
                                    {       // file not available
                                        Modeless_MessageBox("Runner List is not available", "List data not available");
                                        Runners_Not_Available = true;
                                    }
                                    else
                                    {
                                        Runners_Download_Complete = true;
                                        Modeless_MessageBox("Runner List received from Central Database", "List data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.DNS_List:
                                // verify that the response is for this action
                                teststring = "DNS List File";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    SetBtnText(Download, "Download DNS List from Central Database");
                                    Downloaded_DNS_List = Central_String_In.Substring(teststring.Length + 1);
                                    if (Downloaded_DNS_List.EndsWith("is not available"))
                                    {       // file not available
                                        Modeless_MessageBox("DNS List is not available", "List data not available");
                                        DNS_Not_Available = true;
                                    }
                                    else
                                    {
                                        DNS_Download_Complete = true;
                                        Modeless_MessageBox("DNS List received from Central Database", "List data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.DNF_List:
                                // verify that the response is for this action
                                teststring = "DNF List File";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    SetBtnText(Download, "Download DNF List from Central Database");
                                    Downloaded_DNF_List = Central_String_In.Substring(teststring.Length + 1);
                                    if (Downloaded_DNF_List.EndsWith("is not available"))
                                    {       // file not available
                                        Modeless_MessageBox("DNF List is not available", "List data not available");
                                        DNF_Not_Available = true;
                                    }
                                    else
                                    {
                                        DNF_Download_Complete = true;
                                        Modeless_MessageBox("DNF List received from Central Database", "List data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.Watch_List:
                                // verify that the response is for this action
                                teststring = "Watch List File";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    SetBtnText(Download, "Download Watch List from Central Database");
                                    Downloaded_Watch_List = Central_String_In.Substring(teststring.Length + 1);
                                    if (Downloaded_Watch_List.StartsWith("is not available"))
                                    {       // file not available
                                        Modeless_MessageBox("Watch List is not available", "List data not available");
                                        Watch_Not_Available = true;
                                    }
                                    else
                                    {
                                        Watch_Download_Complete = true;
                                        Modeless_MessageBox("Watch List received from Central Database", "List data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.Info_File:
                                // verify that the response is for this action
                                teststring = "Info File";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    SetBtnText(Download, "Download Info from Central Database");
                                    Downloaded_Info = Central_String_In.Substring(teststring.Length + 1);
                                    if (Downloaded_Info.StartsWith("is not available"))
                                    {       // file not available
                                        Modeless_MessageBox("Info File is not available", "List data not available");
                                        Info_Not_Available = true;
                                    }
                                    else
                                    {
                                        Info_Download_Complete = true;
                                        Modeless_MessageBox("Info File received from Central Database", "List data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.Issues:
                                // verify that the response is for this action
                                teststring = "Aid Issues File";
                                if (Central_String_In.StartsWith(teststring))
                                {
                                    SetBtnText(Download, "Get list\nfrom Central\nDatabase");
                                    Downloaded_Issues = Central_String_In.Substring(teststring.Length + 1);
                                    if (Downloaded_Issues.StartsWith("is not available"))
                                    {       // file not available
                                        Modeless_MessageBox("Issues are not available", "List data not available");
                                        Issues_Not_Available = true;
                                    }
                                    else
                                    {
                                        Issues_Download_Complete = true;
                                        Modeless_MessageBox("Issues received from Central Database", "List data received");
                                    }
                                    Central_String_In = "";
                                }
                                break;
                            case Expecting_State.Request_Runner:
                                string[] Parts;
                                char[] splitter = new char[] { ',' , '-' };
                                Parts = Central_String_In.Split(splitter);
                                if (Parts.Length != 6)
                                {
                                    Modeless_MessageBox("Runner Status packet in wrong format!", "Bad packet format");
                                }
                                else
                                    if (Parts[0] == "End")
                                    {
                                            Runner_Status_Received = true;
                                            expecting = Expecting_State.Nothing;
                                    }
                                    else
                                    {
                                        Form1.RunnerStatus rs = new Form1.RunnerStatus();
                                        rs.StationName = Parts[0];
                                        if (Parts[1] != "")
                                            rs.TimeIn = Parts[1];
                                        if (Parts[2] != "")
                                            rs.TimeOut = Parts[2];
                                        if (Parts[3] != "")
                                            rs.TimeAtStation = Parts[3];
                                        if (Parts[4] != "")
                                            rs.TimeToPrev = Parts[4];
                                        if (Parts[5] != "")
                                            rs.TimeToNext = Parts[5];
                                        Form1.RunnersStatus.Add(rs);
                                    }
                                Central_String_In = "";
                                break;
                            case Expecting_State.Connected_Active:
                                // test if the expected string was received
                                if (Central_String_In == "You are Active")
                                {
                                    // tell Form1 we are connected and active
                                    ChangeState(Server_State.Connected_Active);
                                    Central_String_In = "";

                                    // tell Central Database how many Log Points we have
                                    SendCommand("Log Points:" + Form1.NumLogPts.ToString(), Expecting_State.Welcome_Message);
                                }
                                break;
                            default:
                                int x = 3;
                                break;
                        }

                        // data has been processed - clear the flag
                        Central_Data_Received = false;
                    }
                    #endregion

                    #region Send Data to Central Database
                    // check for any commands to send out
                    if ((!Central_Data_Ready_to_Send) && (Form1.CommandQue.Count != 0))
                    {
                        lock (Form1.CommandQue)
                        {
                            Form1.Command newCommand = new Form1.Command();
                            newCommand = Form1.CommandQue.Dequeue();
                            Central_String_Out = newCommand.Data;
                            expecting = newCommand.Expecting;
                            Central_Data_Ready_to_Send = true;
                            ////// test if there is any data going from the Server to the Central Database
                            ////if (Central_Data_Ready_to_Send)
                            ////{
                            //    // create the full string to send to the Central Database
                            //    String sendstring = "";
                            //    sendstring += Central_String_Out;

                            //    // send the data to the Central Database
                            //    Byte[] sendBytes = Encoding.ASCII.GetBytes(sendstring);

                            //    // this is where I need the test for server still alive - it died here
                            //    MyNetStream.Write(sendBytes, 0, sendBytes.Length);
                            ////    // clear the worker flag
                            ////    Central_Data_Ready_to_Send = false;
                            ////}
                        }
                    }
                    #endregion
                }
                Application.DoEvents();
            }

            // Stop requested - this section moved from ServerTimeEvent - 8/11/15
            Console.WriteLine("Server worker thread terminating gracefully.");

            // close the connections
            ServerClient.Close();
            if (MyNetStream != null)
                MyNetStream.Close();
            Onesecond.Stop();
            Onesecond.Close();
        }

        string GetOneEntry()
        {   // returns just the substring after the ':' and including the first set of '\r\n'
            int firstentryreturnstartindex = Central_String_In.IndexOf(':') + 1;
            int firstentrysize = Central_String_In.IndexOf('\n') - 1;

            // first get the substring to be returned
            string returnstring = Central_String_In.Substring(firstentryreturnstartindex, firstentrysize - firstentryreturnstartindex);

            // then rmove the entire first entry
            Central_String_In = Central_String_In.Remove(0, firstentrysize + 2); // first remove entire first entry

            return returnstring;
        }

        private void SendCommand(string Data, Expecting_State Expecting)
        {
            Form1.Command newcommand = new Form1.Command();
            if (Data.EndsWith(Environment.NewLine))    // add EOL if it does not exist
                newcommand.Data = Data;
            else
                newcommand.Data = Data + Environment.NewLine;
            newcommand.Expecting = Expecting;
            lock (Form1.CommandQue)
            {// lock
                Form1.CommandQue.Enqueue(newcommand);
            }// unlock
        }

        void Attempt_To_Connect()  // Ethernet/MESH
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(AttemptConnectThread));
        }

        private void AttemptConnectThread(object info)  // Ethernet/MESH
        {
            // set the flag and change the state
            Connect_Request = false;
            Attempting_to_Connect_to_Server = true;
            ChangeState(Server_State.Waiting);

            // depends upon what the connection medium is
            switch (Connection_Type)
            {
                case Form1.Connect_Medium.Ethernet:
                    // try to connect to Ethernet
                    try
                    {
                        if (Already_Tried_To_Connect)
                            AddRichText(Ethernet_Packets, ".", Color.Green);
                        else
                        {
                            AddRichText(Ethernet_Packets, Environment.NewLine + "Attempting to connect to: " + Server_IP_Address + ":" + Server_Port_Number, Color.Green);
                            Already_Tried_To_Connect = true;
                        }

                        // connect to the server
                        ServerClient = new TcpClient();
                        ServerClient.Connect(Server_IP_Address, Server_Port_Number);
                        MyNetStream = ServerClient.GetStream();
                        Receivebytes = new Byte[ServerClient.ReceiveBufferSize];
                        Sendbytes = new Byte[ServerClient.SendBufferSize];
                        ChangeState(Server_State.Connected);
                        expecting = Expecting_State.StationName_Request;
                        AddRichText(Ethernet_Packets, Environment.NewLine, Color.Green);
                        Already_Tried_To_Connect = false;
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        ChangeState(Server_State.Error);
                        Attempting_to_Connect_to_Server = false;
                    }
                    break;
                case Form1.Connect_Medium.Packet:
                    // try to connect to Packet
                    try
                    {
                        if (Already_Tried_To_Connect)
                            AddRichText(Packet_Packets, ".", Color.Green);
                        else
                        {
                            AddRichText(Packet_Packets, Environment.NewLine + "Attempting to connect to: " + Server_IP_Address + ":" + Server_Port_Number, Color.Green);
                            Already_Tried_To_Connect = true;
                        }

                        // connect to the AGWPE server

                        //ServerClient = new TcpClient();
                        //ServerClient.Connect(Server_IP_Address, Server_Port_Number);
                        //MyNetStream = ServerClient.GetStream();
                        //Receivebytes = new Byte[ServerClient.ReceiveBufferSize];
                        //Sendbytes = new Byte[ServerClient.SendBufferSize];
                        
                        ChangeState(Server_State.Connected);
                        expecting = Expecting_State.StationName_Request;
                        AddRichText(Packet_Packets, Environment.NewLine, Color.Green);
                        Already_Tried_To_Connect = false;
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        ChangeState(Server_State.Error);
                        Attempting_to_Connect_to_Server = false;
                    }
                    break;
                default:
                    int r = 3;  // breakpoint
                    break;
            }
        }

        void ServerTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // this happens every second
            // See if there is data to send or receive from the Central Database
                // this prevents overalpping threads when debugging
// 8/11/15                if (!Server_Busy)
//                {
                    // has another Connect Request been issued?
                    if (Connect_Request)
                    {
                        // if we are already connected = then need to Disconnect first
                        if (Connected_to_Server)
                        {
                            ServerClient.Close();
                            if (MyNetStream != null)
                                MyNetStream.Close();
                        }

                        // now connect
                        Attempt_To_Connect();
                    }
                    else
                    {
                        // do only if connected
                        if (Connected_to_Server)
                        {
                            // first test if the connection is still good
                            try
                            {
                                bool mode = ServerClient.Client.Poll(1, SelectMode.SelectRead);
                                int amt = ServerClient.Client.Available;
                                if (ServerClient.Client.Poll(1, SelectMode.SelectRead) && ServerClient.Client.Available == 0)
                                {
                                    Connected_to_Server = false;
                                    ServerClient.Close();
                                }
                            }
                            catch (SocketException)
                            {
                                Connected_to_Server = false;
                                ServerClient.Close();
                            }

                            // test again, in case connection lost
                            if (Connected_to_Server)
                            {
                                // set the flag
// 8/11/15                                Server_Busy = true;

                                // look for any new data from the Central Database
                                if (MyNetStream.DataAvailable)
                                {
                                    // get the new byte(s) from the Central Database
                                    Snumbytes = MyNetStream.Read(Receivebytes, 0, (int)ServerClient.ReceiveBufferSize);
                                    if (Snumbytes == 512)
                                        return;    // stop here
                                    Central_String_In += Encoding.ASCII.GetString(Receivebytes);   // add to the string being built
                                    Central_String_In = Central_String_In.Substring(0, Snumbytes);     // only keep the new bytes

                                    // display in the Richtextbox
                                    AddRichText(Ethernet_Packets, Central_String_In, Color.Black);

                                    Central_Data_Received = true;
                                }
                                else     // no new data, check for any left over data
                                {
                                    if ((Central_String_In != string.Empty) && (Central_String_In[0] != '\0'))
                                    {
                                        int length = Central_String_In.Length;
                                    }
                                }

                                // test if there is any data going from the Server to the Central Database
                                if (Central_Data_Ready_to_Send)
                                {
                                    // create the full string to send to the Central Database
                                    String sendstring = "";
                                    sendstring += Central_String_Out;

                                    // send the data to the Central Database
                                    Byte[] sendBytes = Encoding.ASCII.GetBytes(sendstring);

                                    // this is where I need the test for server still alive - it died here
                                    MyNetStream.Write(sendBytes, 0, sendBytes.Length);

                                    // display in the Richtextbox
                                    AddRichText(Ethernet_Packets, Central_String_Out, Color.Red);

                                    // clear the worker flag
                                    Central_Data_Ready_to_Send = false;
                                }

                                // clear the flag
// 8/11/15                                Server_Busy = false;
                            }
                        }
                }
                Application.DoEvents();
        }

        void ChangeState(Server_State newstate)
        {
            if (state != newstate)      // if already at this state, no change
            {
                // set the new state
                state = newstate;

                // clear all the message labels
                MakeVisible(Cannot_Connect, false);
                MakeVisible(Server_Waiting, false);
                MakeVisible(Server_Connected, false);
                MakeVisible(Server_Connected_Active, false);
                MakeVisible(Error_Connecting, false);

                // set the new message label
                switch (state)
                {
                    case Server_State.Not_Initted:
                        break;
                    case Server_State.Waiting:
                        MakeVisible(Server_Waiting, true);
                        break;
                    case Server_State.Connected:
                        Connected_to_Server = true;
                        Attempting_to_Connect_to_Server = false;
                        break;
                    case Server_State.Error:
                        SetText(Server_Error_Message, error);
                        MakeVisible(Error_Connecting, true);
                        MakeVisible(Connect_Button, true);
                        Attempting_to_Connect_to_Server = false;
                        break;
                    case Server_State.Connected_Active:
                        MakeVisible(Server_Connected, true);    // 7/30/15
                        Connected_and_Active = true;    // 8/2/15
                        Attempting_to_Connect_to_Server = false;  // 7/30/15
                        break;
                    default:
                        break;
                }
            }
        }

        void MakeVisible(Control cntrl, bool state)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (cntrl.InvokeRequired)
            {
                MakeVisibledel d = new MakeVisibledel(MakeVisible);
                cntrl.Invoke(d, new object[] { cntrl, state });
            }
            else
            {
                cntrl.Visible = state;
                cntrl.Update();
            }
        }

        void SetText(TextBox cntrl, string str)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (cntrl.InvokeRequired)
            {
                SetTextdel d = new SetTextdel(SetText);
                cntrl.Invoke(d, new object[] { cntrl, str });
            }
            else
            {
                cntrl.Text = str;
                cntrl.Update();
            }
        }

        void AddRichText(RichTextBox rtb, string str, Color color)
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

        void SetBtnText(Button cntrl, string str)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (cntrl.InvokeRequired)
            {
                SetBtnTextdel d = new SetBtnTextdel(SetBtnText);
                cntrl.Invoke(d, new object[] { cntrl, str });
            }
            else
            {
                cntrl.Text = str;
                cntrl.Update();
            }
        }

        public void RequestStop()
        {
            Runner_Status_Received = true;
            Server_shouldStop = true;
        }

        public void RequestConnect()
        {
            Connect_Request = true;
        }

        public static void Modeless_MessageBox(string message, string title) 
        { 
            // Start the message box thread 
            new Thread(new ThreadStart(delegate 
            { 
                MessageBox.Show 
                ( 
                  message, 
                  title, 
                  MessageBoxButtons.OK, 
                  MessageBoxIcon.Exclamation 
                ); 
            })).Start(); 
            // Continue doing stuff while the message box is visible to the user. 
            // The message box thread will end itself when the user clicks OK. 
        } 
    }
}