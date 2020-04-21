using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace RunnerTracker
{
    public class Aid_Worker
    {
        //
        //  Updates:
        //  2/24/16 - changed Receive to Asynchronous
        //            Connect and Send still synchronous
        //  7/25/17 - changed when change to Connected & Active state
        //          - had to add new variable: InfoFile_Loaded
        //  8/6/17 - in FiveSecondTimeEvent, changed DB_AGWSocket to Aid_AGWSocket
        //  8/7/17 - added Connenction_Type for APRS to not always test for APRS messages
        //           changed APRS_Message_Status
        //           added another test: (Central_String_In != "") before the switch test
        //           added test for Connection_Type in Welcome_Message processing to tell if can change to Connected and Active
        //           added ProcessAPRSBulletinThread to handle the APRS File Sending Bulletins
        //           began adding the Arrays to hold the File Sending Bulletins
        //  8/8/17 - added more string arrays for the File Sending Bulletins
        //  8/9/17 - added Needxxx to get File Sending Bulletins only when needed
        //  8/10/17 - added a new Expecting_State: Bib_List;
        //  8/11/17 - added test for Alert bulletins being received in Receive Thread
        //  8/15/17 - added 1 minute timers for the File Sending Bulletin reception
        //            added into each Filetype the stopping and starting of the 1 minute timers
        //  5/8/19 - added xx_Not_Available = false to all requests for lists that are good
        //

        #region Variables and declarations
        delegate void MakeVisibledel(Control cntrl, bool state);
        delegate void SetTextdel(TextBox tb, string str);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        delegate void SetBtnTextdel(Button btn, string str);
        public Label Server_Initting { get; set; }
        public Label Server_Connected { get; set; }
        public Label Server_Connected_Active { get; set; }
        public Label Server_Attempting_Connection { get; set; }
        public Label AGWPE_Connected { get; set; }
        public Label Cannot_Connect { get; set; }
        public Label Error_Connecting { get; set; }
        public Label Lost_Connection { get; set; }
        public TextBox Server_Error_Message { get; set; }
        public RichTextBox Ethernet_Packets { get; set; }
        public RichTextBox Packet_Packets { get; set; }
        public string Station_Name { get; set; }
        public Button Connect_Button { get; set; }
        public Button Download { get; set; }
        public Button AGWPE_Start { get; set; }
        public Button AGWPE_Connect { get; set; }
        public string Server_IP_Address { get; set; }
        public int Server_Port_Number { get; set; }
        public string Command { get; set; }
        public string Data { get; set; }
        public string Welcome_Message { get; set; }
        public string Downloaded_Stations_Info { get; set; }    // worker sets this with Stations File data, so Form1 can pull it out
        public string Downloaded_Runner_List { get; set; }    // worker sets this with Runner List data, so Form1 can pull it out
        public string Downloaded_DNS_List { get; set; }    // worker sets this with DNS List data, so Form1 can pull it out
        public string Downloaded_DNF_List { get; set; }    // worker sets this with DNF List data, so Form1 can pull it out
        public string Downloaded_Watch_List { get; set; }    // worker sets this with Watch List data, so Form1 can pull it out
        public string Downloaded_Issues { get; set; }    // worker sets this with Issues List data, so Form1 can pull it out
        public string Downloaded_Info { get; set; }    // worker sets this with Info File data, so Form1 can pull it out
        public bool Stations_Download_Complete { get; set; }   // set when the Stations File download is complete
        public bool Runners_Download_Request_Complete { get; set; }   // set when the Runner List download is complete - 8/3/16 changed - now indicates the request is complete, need to check Available flag to see if data was downloaded.
        public bool DNS_Download_Request_Complete { get; set; }   // set when the DNS download is complete - 8/3/16 - same as above
        public bool DNF_Download_Request_Complete { get; set; }   // set when the DNF download is complete - 8/3/16 - same as above
        public bool Watch_Download_Request_Complete { get; set; }   // set when the Watch download is complete - 8/3/16 - same as above
        public bool Info_Download_Request_Complete { get; set; }   // set when the Info download is complete - 8/3/16 - same as above
        private bool InfoFile_Loaded = false;       // 7/25/17 - added to determine if Connected & Active
        public bool Issues_Download_Complete { get; set; }   // set when the Issues download is complete
        public bool Stations_Not_Available { get; set; }   // set when the Stations File download is Not available
        public bool Runners_Not_Available { get; set; }   // set when the Runners List download is Not available
        public bool DNS_Not_Available { get; set; }   // set when the DNS download is Not available
        public bool DNF_Not_Available { get; set; }   // set when the DNF download is Not available
        public bool Watch_Not_Available { get; set; }   // set when the Watch download is Not available
        public bool Issues_Not_Available { get; set; }   // set when the Issues download is Not available
        public bool Info_Not_Available { get; set; }   // set when the Info download is Not available
        public bool Connected_to_AGWServer { get; set; }   // = false;
        public bool Connected_to_DB { get; set; }   // = false;
        public bool Connected_and_Active { get; set; }
        bool Connection_Lost;
        public static bool Runner_Status_Received { get; set; }
        public bool Attempting_to_Connect_to_Server { get; set; }
        bool Restart_Receive;
        public Form1.Connect_Medium Connection_Type { get; set; }
        public Form1.Connect_Medium New_Connection_Type { get; set; }
        bool StationName_Sent = false;
        string error;

        // APRS unique
        class APRS_Message
        {
            public int port { get; set; }
// 8/7/17            public int number { get; set; }
            public int msg_number { get; set; }     // 8/7/17
            public DateTime time_sent { get; set; }
            public string message { get; set; }
            public bool acknowledged { get; set; }
            public int number_times_resend { get; set; }      // 8/7/17 - max of 5, then disconnect
        }
        int Next_Msg_Num;
        List<APRS_Message> APRS_Message_Status;
        string[] InfoArray;     // 8/7/17
        string[] RunnersArray;  // 8/8/17
        string[] DNSArray;      // 8/8/17
        string[] DNFArray;      // 8/8/17
        string[] WatchArray;    // 8/8/17
        string[] StationArray;  // 8/8/17
        string[] BibsArray;     // 8/9/17
        public bool NeedBibs = true;       // 8/9/17
        public bool NeedRunners = true;    // 8/9/17
        public bool NeedDNS = true;    // 8/9/17
        public bool NeedDNF = true;    // 8/9/17
        public bool NeedWatch = true;    // 8/9/17
        public bool NeedInfo = true;     // 8/9/17
        public bool NeedStations = true;    // 8/9/17
        System.Timers.Timer F1timer, F2timer, F3timer, F4timer, F5timer, F6timer, F7timer, F8timer, F9timer;      // 8/15/17

        bool Already_Tried_To_Connect = false;
        bool Central_Data_Ready_to_Send = false;    // this flag indicates that there is data to send to the Central Database server
        String Central_String_Out;      // data going to the Central Database server

        // Ethernet Server states:
// 7/18/16        public enum Server_State { Not_Initted, Attempting, Connected_To_Server, Connected_To_DB, Connected_Active, Error_Connecting }
// 7/22/16        public enum Server_State { Not_Initted, Cannot_Connect, Attempting_Connect, Error_Connecting, Connected_To_DB, Connected_Active }
// 7/22/16        public enum Server_State { Not_Initted, Cannot_Connect, Attempting_Connect, Error_Connecting, Connected_To_AGWServer, Connected_To_DB, Connected_Active }
        public enum Server_State { Not_Initted, Cannot_Connect, Attempting_Connect, Error_Connecting, Connected_To_AGWServer, Connected_To_DB, Connected_Active, Lost_Connection }
        // AGWPE Server states:
        //        public enum Server_State { Not_Initted, Cannot_Connect, Attempting, Error_Connecting, Connected_To_Server, Connected_To_DB, Connected_Active }
        public Server_State state = Server_State.Not_Initted;
        public enum Expecting_State { Nothing, StationName_Request, Welcome_Message, Station_Info_File, Connected_Active, Runner_List, Bib_List,    // 8/10/17 - added Bib_List
            DNS_List, DNF_List, Watch_List, Issues, Info_File, Request_Runner }
        Expecting_State expecting = Expecting_State.StationName_Request;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool Server_shouldStop = false;
        private volatile bool Connect_Request = false;
        private volatile String Central_String_In = string.Empty;
        private volatile Socket CentralClient;
        private volatile char[] charsToTrim = { '\0' };      // need to remove all the nulls at the end of the buffer
        public volatile System.Timers.Timer Fivesecond = new System.Timers.Timer();
        int Snumbytes;
        AnonymousPipeServerStream PacketOutData;    // data from AGWSocket to Worker
        AnonymousPipeClientStream PacketInData;     // data from Worker to AGWSocket

        // Asynchronous Client variables - added 2/16/16
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

        public Aid_Worker()
        {
            //// init the Pipe Stream Server to send the Packet data to the AGWSocket
            //PacketOutData = new AnonymousPipeServerStream(PipeDirection.Out);
            //Form1.PacketOutDataHandle = PacketOutData.GetClientHandleAsString();

            //
            //  Server states for Packet and APRS:
            //      Not_Initted - program has just started
            //      Connected_To_AGWServer - connected to AGWPE
            //      Waiting  - connected to AGWPE server, waiting for packet/APRS connection to the Database
            //      Connected_To_DB - connected to the Database callsign
            //      Connected_and_Active - received Welcome message
            //      Error  - Cannot connect to AGWPE server or Database callsign
            //

            //
            //  Server states for Ethernet
            //      Not_Initted
            //      Cannot Connect
            //      Attempting to Connect
            //      Error Connecting
            //      Connected
            //      Connected and Active
            //

            // 8/15/17 - init the 1 minute timers used for APRS File Sending Bulletins
            F1timer = new System.Timers.Timer();
            F1timer.Interval = 60000;   // set for 60 seconds
            F1timer.Elapsed += new System.Timers.ElapsedEventHandler(F1TimeEvent);
            F2timer = new System.Timers.Timer();
            F2timer.Interval = 60000;   // set for 60 seconds
            F2timer.Elapsed += new System.Timers.ElapsedEventHandler(F2TimeEvent);
            F3timer = new System.Timers.Timer();
            F3timer.Interval = 60000;   // set for 60 seconds
            F3timer.Elapsed += new System.Timers.ElapsedEventHandler(F3TimeEvent);
            F4timer = new System.Timers.Timer();
            F4timer.Interval = 60000;   // set for 60 seconds
            F4timer.Elapsed += new System.Timers.ElapsedEventHandler(F4TimeEvent);
            F5timer = new System.Timers.Timer();
            F5timer.Interval = 60000;   // set for 60 seconds
            F5timer.Elapsed += new System.Timers.ElapsedEventHandler(F5TimeEvent);
            F6timer = new System.Timers.Timer();
            F6timer.Interval = 60000;   // set for 60 seconds
            F6timer.Elapsed += new System.Timers.ElapsedEventHandler(F6TimeEvent);
            F7timer = new System.Timers.Timer();
            F7timer.Interval = 60000;   // set for 60 seconds
            F7timer.Elapsed += new System.Timers.ElapsedEventHandler(F7TimeEvent);
            F8timer = new System.Timers.Timer();
            F8timer.Interval = 60000;   // set for 60 seconds
            F8timer.Elapsed += new System.Timers.ElapsedEventHandler(F8TimeEvent);
            F9timer = new System.Timers.Timer();
            F9timer.Interval = 60000;   // set for 60 seconds
            F9timer.Elapsed += new System.Timers.ElapsedEventHandler(F9TimeEvent);
        }

        #region 1 minute timers Event Handlers - added 8/15/17
        void F1TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // Bib only
            Push_a_Button(Form1.Button_to_Push.Bib_Only);
        }

        void F2TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // complete Runners List
            Push_a_Button(Form1.Button_to_Push.Runners_List);
        }

        void F3TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // Runners status
        }

        void F4TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // DNS list
            Push_a_Button(Form1.Button_to_Push.DNS_List);
        }

        void F5TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // DNF list
            Push_a_Button(Form1.Button_to_Push.DNF_List);
        }

        void F6TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // Watch list
            Push_a_Button(Form1.Button_to_Push.Watch_List);
        }

        void F7TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // Info file
            Push_a_Button(Form1.Button_to_Push.Info_List);
        }

        void F8TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // station info file
            Push_a_Button(Form1.Button_to_Push.Station_Info_File);
        }

        void F9TimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // not yet defined
        }
        #endregion

        public void Aid_Worker_Connect_and_Send_Thread(object client)
        {
            // give the thread a name
            Thread.CurrentThread.Name = "Station Worker Connect/Send thread";

            //// init the Pipe Stream Client to receive the Packet data from the AGWSocket
            //if (Connection_Type == Form1.Connect_Medium.Packet)
            //{
            //    string Pipehandle;
            //    do
            //    {
            //        Thread.Sleep(10);
            //        Pipehandle = Form1.PacketInDataHandle;
            //    }
            //    while (Pipehandle == null);
            //    PacketInData = new AnonymousPipeClientStream(PipeDirection.In, Pipehandle);
            //}

            // init all flags
            Connected_to_AGWServer = false;
            Connected_to_DB = false;
            Connected_and_Active = false;
            Connection_Lost = false;
            Attempting_to_Connect_to_Server = false;
            DNS_Download_Request_Complete = false;
            DNF_Download_Request_Complete = false;
            Watch_Download_Request_Complete = false;
            DNS_Not_Available = false;
            DNF_Not_Available = false;
            Watch_Not_Available = false;
            Runner_Status_Received = false;
            APRS_Message_Status = new List<APRS_Message>();
            Next_Msg_Num = 1;

            // what to do when starting the Ethernet connection
            if (Connection_Type == Form1.Connect_Medium.Ethernet)
            {
                // start a five second timer to test the Ethernet connection
                Fivesecond.Interval = 5000;
                Fivesecond.Elapsed += new System.Timers.ElapsedEventHandler(FiveSecondTimeEvent);
                Fivesecond.Start();

                // initial state should be 'Initting'
                // verify Ethernet IP address and Server Port # are good
                if ((Server_IP_Address != "") && (Server_Port_Number != 0))
                {
                    // attempt to Connect
                    Attempt_To_Connect();
                }
                else
                {
                    // cannot connect - lacking information
                    ChangeState(Server_State.Cannot_Connect);
                }
            }

            // Connect requests for Packet and APRS are handled through AGWPE
            // but still need to start the 5-second timer
            if ((Connection_Type == Form1.Connect_Medium.APRS) || (Connection_Type == Form1.Connect_Medium.Packet))
            {
                // start a five second timer to test the connection
                Fivesecond.Interval = 5000;
                Fivesecond.Elapsed += new System.Timers.ElapsedEventHandler(FiveSecondTimeEvent);
                Fivesecond.Start();

                // connection attempt will be handled after connected to AGWPE
                Attempt_To_Connect();   // ???????
            }

            // processing loop
            while (!Server_shouldStop)      // test if need to stop
            {
                if (Connected_to_DB)    // test if Connected
                {
                    #region Send Data to Central Database
                    // check for any commands to send out
                    if ((!Central_Data_Ready_to_Send) && (Form1.CommandQue.Count != 0))
                    {
                        Form1.Command newCommand = new Form1.Command();
                        lock (Form1.CommandQue)
                        {
                            //Form1.Command newCommand = new Form1.Command();
                            newCommand = Form1.CommandQue.Dequeue();
                        }
                        expecting = newCommand.Expecting;
                        Send_to_Central(newCommand.Data);
                    }
                    #endregion
                }
                Application.DoEvents();
            }

            // Stop requested - this section moved from ServerTimeEvent - 8/11/15
            Console.WriteLine("Station Worker Connect/Send thread terminating gracefully.");

            // close the connections
            if (Connection_Type == Form1.Connect_Medium.Ethernet)
            {
                if (CentralClient != null)
                {
                    if (CentralClient.Connected)
                        CentralClient.Shutdown(SocketShutdown.Both);
                    CentralClient.Close();
                }
                Fivesecond.Stop();
                Fivesecond.Close();
            }
        }

        public void Aid_Worker_Receive_Thread(object client)    // receive data from Database
        {
            string teststring;

            // give the thread a name
            Thread.CurrentThread.Name = "Station Worker Receive thread";

            // reset the flag
            Restart_Receive = false;

            // processing loop
            while (!Server_shouldStop)      // test if need to stop
            {
                if (Connected_to_DB)    // test if Connected
                {
                    #region Received Data
                    // get the new receive data
                    Receive_from_Central();
                    receiveDone.WaitOne();      // wait here for any receive data

                    // test if we came out of Receive because we should stop or Restart Receiving
                    if (!Server_shouldStop && !Restart_Receive)
                    {
                        while (Central_String_In != "")
                        {       // continue looking until all received data has been looked at
                            if (Connection_Type == Form1.Connect_Medium.APRS)       // 8/7/17
                            {
                                // if this is an APRS message, acknowledge it
                                string charit = Central_String_In.Substring(Central_String_In.Length - 6,1);    // expecting: '{nnnn\r'
                                if (Central_String_In.Substring(Central_String_In.Length - 6, 1) == "{")    // expecting: '{nnnn\r'
                                {
                                    string number = Central_String_In.Substring(Central_String_In.IndexOf("{") + 1);
                                    Form1.Aid_AGWSocket.SendMessageWOid(Form1.AGWPERadioPort, Form1.DatabaseFCCCallsign, "ack" + number);
                                    Central_String_In = Central_String_In.Remove(Central_String_In.IndexOf("{"));       // 8/7/17 - remove the message number
                                }

                                // if this is an acknowledge, handle in APRS Message List - added 8/7/17 (copied from DB_APRS_StationWorker.cs)
                                if (Central_String_In.StartsWith("ack"))
                                {
                                    APRS_Message mess = APRS_Message_Status.Find(x => x.msg_number == Convert.ToInt16(Central_String_In.Substring(3, 4)));
                                    mess.acknowledged = true;
                                    Central_String_In = "";        // and clear the data
                                }

                                // if this is a File Sending Bulletin, open a thread to handle - added 8/7/17
                                if (Central_String_In.StartsWith(":F"))     // 8/7/17
                                {
                                    // start a thread to handle this bulletin line
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSBulletinThread), Central_String_In);       // 8/7/17
                                    Central_String_In = "";        // and clear the data
                                }

                                // if this is an Alert being received - added 8/11/17
                                if (Central_String_In.StartsWith(":Alert"))     // 8/11/17
                                {
                                    Form1.Incoming_Alert = Central_String_In.Substring(11);     // 8/11/17
                                    Form1.Incoming_Alrt = true;         // 8/11/17
                                    Central_String_In = "";         // 8/11/17
                                }
                            }

                            // Alerts can come at any time
                            if (Central_String_In.StartsWith("Alert:"))
                            {
                                string input = GetOneEntry();
                                Form1.Incoming_Alert = input;
                                Form1.Incoming_Alrt = true;
                            }

                            // could also be an incoming message
                            if (Central_String_In.StartsWith("Message:"))
                            {
                                Form1.Incoming_Message = Central_String_In.Substring(8);
                                Form1.Incoming_Mess = true;
                                Central_String_In = "";
                            }

                            if (Central_String_In != "")        // added 8/7/17
                            {
                                // what are we expecting
                                switch (expecting)
                                #region Look at all of the expecting states
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
                                            SendCommand("Station name = " + Station_Name, Expecting_State.Welcome_Message);
                                            Console.WriteLine("Sent Station name to Central Database");
                                            StationName_Sent = true;
                                            Central_String_In = "";
                                        }
                                        break;
                                    case Expecting_State.Welcome_Message:
                                        teststring = "Welcome message:";
                                        Welcome_Message = Central_String_In.Substring(teststring.Length);
                                        Welcome_Message.Replace("\r", "");   // remove any CR,LF at end
                                        Welcome_Message.Replace("\n", "");   // remove any CR,LF at end
                                        Console.WriteLine("Welcome message = " + Welcome_Message);
                                        Central_String_In = "";

                                        // change the Expected State
                                        expecting = Expecting_State.Nothing;

                                        // change the state - 3/16/16
                                        // 7/25/17 - added this to check if this is the initial or a subsequent connection to DB
// 8/7/17                                        if (InfoFile_Loaded)     // 7/25/17 - test if this is a subsequent connection
                                        if (InfoFile_Loaded || (Connection_Type != Form1.Connect_Medium.Ethernet))     // 7/25/17 - test if this is a subsequent connection  8/7/17 - or an APRS connection
                                            ChangeState(Server_State.Connected_Active);
                                        break;
                                    case Expecting_State.Station_Info_File:
                                        // verify that the response is for this action
                                        teststring = "Stations Info File:";
                                        if (Central_String_In.StartsWith(teststring))
                                        {
                                            Downloaded_Stations_Info = Central_String_In.Substring(teststring.Length);
                                            if (Downloaded_Stations_Info.StartsWith("inaccessible"))
                                            {       // file not available
                                                Modeless_MessageBox_Exclamation("Stations List is not available", "List data not available");
                                                Stations_Not_Available = true;
                                            }
                                            else
                                            {
                                                SetBtnText(Download, "Downloaded\nfrom Central");
                                                Stations_Download_Complete = true;
                                                // 7/13/17                                            Modeless_MessageBox_Exclamation("Station Info File received from Central Database\n\n   Click the Save button to save it on this PC.", "File data received");
                                                Modeless_MessageBox_Information("Station Info File received from Central Database\n\n   Click the Save button to save it on this PC.", "File data received");
                                                Stations_Not_Available = false;  // 5/8/19
                                            }
                                            Central_String_In = "";
                                        }
                                        break;
                                    case Expecting_State.Runner_List:
                                        // verify that the response is for this action
                                        teststring = "Runner List File";
                                        if (Central_String_In.StartsWith(teststring))
                                        {
                                            // 7/24/17                                        SetBtnText(Download, "Refresh");
                                            SetBtnText(Download, "Refresh from Central Database");        // 7/24/17
                                            Downloaded_Runner_List = Central_String_In.Substring(teststring.Length + 1);
                                            if (Downloaded_Runner_List.EndsWith("is not available"))
                                            {       // file not available
                                                Modeless_MessageBox_Exclamation("Runner List is not available", "List data not available");
                                                Runners_Not_Available = true;
                                            }
                                            else
                                            {
                                                // 8/3/16                                            Runners_Download_Request_Complete = true;
                                                //                                            Modeless_MessageBox_Exclamation("Runner List received from Central Database", "List data received");
                                                Modeless_MessageBox_Information("Runner List received from Central Database", "List data received");
                                                Runners_Not_Available = false;  // 5/8/19
                                            }
                                            Runners_Download_Request_Complete = true;   // moved here 8/3/16
                                            Central_String_In = "";
                                        }
                                        break;
                                    case Expecting_State.Bib_List:       // 8/10/17
                                        // verify that the response is for this action
                                        teststring = "Bib List";
                                        if (Central_String_In.StartsWith(teststring))
                                        {
                                            SetBtnText(Download, "Download Bib # only");
                                            Downloaded_Runner_List = Central_String_In.Substring(teststring.Length + 1);
                                            if (Downloaded_Runner_List.EndsWith("is not available"))
                                            {       // file not available
                                                Modeless_MessageBox_Exclamation("Bib List is not available", "List data not available");
                                                Runners_Not_Available = true;
                                            }
                                            else
                                            {
                                                Modeless_MessageBox_Information("Runner List received from Central Database", "List data received");
                                                Runners_Not_Available = false;  // 5/8/19
                                            }
                                            Runners_Download_Request_Complete = true;
                                            Central_String_In = "";
                                        }
                                        break;       // 8/10/17
                                    case Expecting_State.DNS_List:
                                        // verify that the response is for this action
                                        teststring = "DNS List File";
                                        if (Central_String_In.StartsWith(teststring))
                                        {
                                            SetBtnText(Download, "Download DNS List from Central Database");
                                            Downloaded_DNS_List = Central_String_In.Substring(teststring.Length + 1);
                                            if (Downloaded_DNS_List.EndsWith("is not available"))
                                            {       // file not available
                                                Modeless_MessageBox_Exclamation("DNS List is not available", "List data not available");
                                                DNS_Not_Available = true;
                                            }
                                            else
                                            {
                                                // 8/3/16                                            DNS_Download_Request_Complete = true;
                                                // 7/13/17                                            Modeless_MessageBox_Exclamation("DNS List received from Central Database", "List data received");
                                                Modeless_MessageBox_Information("DNS List received from Central Database", "List data received");
                                                DNS_Not_Available = false;  // 5/8/19
                                            }
                                            DNS_Download_Request_Complete = true;   // moved here 8/3/16
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
                                                Modeless_MessageBox_Exclamation("DNF List is not available", "List data not available");
                                                DNF_Not_Available = true;
                                            }
                                            else
                                            {
                                                // 8/3/16                                            DNF_Download_Request_Complete = true;
                                                // 7/13/17                                            Modeless_MessageBox_Exclamation("DNF List received from Central Database", "List data received");
                                                Modeless_MessageBox_Information("DNF List received from Central Database", "List data received");
                                                DNF_Not_Available = false;  // 5/8/19
                                            }
                                            DNF_Download_Request_Complete = true;   // moved here 8/3/16
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
                                                Modeless_MessageBox_Exclamation("Watch List is not available", "List data not available");
                                                Watch_Not_Available = true;
                                            }
                                            else
                                            {
                                                // 8/3/16                                            Watch_Download_Request_Complete = true;
                                                //                                            Modeless_MessageBox_Exclamation("Watch List received from Central Database", "List data received");
                                                Modeless_MessageBox_Information("Watch List received from Central Database", "List data received");
                                                Watch_Not_Available = false;  // 5/8/19
                                            }
                                            Watch_Download_Request_Complete = true;     // moved here 8/3/16
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
                                                Modeless_MessageBox_Exclamation("Info File is not available", "List data not available");
                                                Info_Not_Available = true;
                                            }
                                            else
                                            {
                                                // 8/3/16                                            Info_Download_Request_Complete = true;
                                                // 7/13/17                                            Modeless_MessageBox_Exclamation("Info File received from Central Database", "List data received");
                                                Modeless_MessageBox_Information("Info File received from Central Database", "List data received");
                                                Info_Not_Available = false;  // 5/8/19
                                            }
                                            Info_Download_Request_Complete = true;  // moved here 8/3/16
                                            Central_String_In = "";

                                            // end of Lists requesting - change expecting state
                                            expecting = Expecting_State.Nothing;

                                            // 7/25/17 - change to Connected & Active state
                                            // this handles the initial connection to DB
                                            ChangeState(Server_State.Connected_Active);     // 7/25/17
                                            InfoFile_Loaded = true;             // 7/25/17
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
                                                Modeless_MessageBox_Exclamation("Issues are not available", "List data not available");
                                                Issues_Not_Available = true;
                                            }
                                            else
                                            {
                                                Issues_Download_Complete = true;
                                                // 7/13/17                                            Modeless_MessageBox_Exclamation("Issues received from Central Database", "List data received");
                                                Modeless_MessageBox_Information("Issues received from Central Database", "List data received");
                                                Issues_Not_Available = false;  // 5/8/19
                                            }
                                            Central_String_In = "";
                                        }
                                        break;
                                    case Expecting_State.Request_Runner:
                                        string[] Parts;
                                        char[] splitter = new char[] { ',', '-' };
                                        Parts = Central_String_In.Split(splitter);
                                        if (Parts.Length != 6)
                                        {
                                            Modeless_MessageBox_Exclamation("Runner Status packet in wrong format!", "Bad packet format");
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
                                            //                                        rs.StationName = Parts[0];
                                            rs.Station = Parts[0];
                                            if (Parts[1] != "")
                                                rs.TimeIn = Parts[1];
                                            if (Parts[2] != "")
                                                rs.TimeOut = Parts[2];
                                            if (Parts[3] != "")
                                                rs.TimeAtStation = Parts[3];
                                            if (Parts[4] != "")
                                                //                                            rs.TimeToPrev = Parts[4];
                                                rs.TimeFromPrev = Parts[4];
                                            if (Parts[5] != "")
                                                rs.TimeToNext = Parts[5];
                                            Form1.RunnersStatus.Add(rs);
                                        }
                                        Central_String_In = "";
                                        break;
                                    case Expecting_State.Connected_Active:
                                        // test if the expected string was received
                                        //                                if (Central_String_In == "You are Active")
                                        teststring = "You are Active";
                                        if (Central_String_In.StartsWith(teststring))
                                        {
                                            // tell Form1 we are connected and active
                                            ChangeState(Server_State.Connected_Active);
                                            //                                    Central_String_In = "";
                                            Central_String_In = Central_String_In.Substring(teststring.Length);     // only remove the teststring

                                            // tell Central Database how many Log Points we have
                                            SendCommand("Log Points:" + Form1.NumLogPts.ToString(), Expecting_State.Welcome_Message);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                }
                Application.DoEvents();
                Restart_Receive = false;
            }

            // Stop requested
            Console.WriteLine("Station Worker Receive thread terminating gracefully.");
        }

        #region File Sending Bulletins
        private void ProcessAPRSBulletinThread(object info)     // 8/7/17 - APRS only
        {
            string Central_String_In = (string)info;

            // get the parameters for this line:  ":F1001\009:data"
            string Filetype = Central_String_In.Substring(0, 3);
// 8/8/17            int numthisline = Convert.ToInt16(Central_String_In.Substring(3, 3));
            int numthisline = Convert.ToInt16(Central_String_In.Substring(3, 3)) - 1;           // 8/8/17
            int numlines = Convert.ToInt16(Central_String_In.Substring(7,3));
            string linedata = Central_String_In.Substring(11);

            // go handle the specific file
            switch(Filetype)
            {
                case ":F1":     // Runners list, Bib numbers only
                    // do we need this list?
                    if (NeedBibs)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F1timer.Stop();         // 8/15/17

                        // does the Bibs Array already exist?
                        if (BibsArray == null)
                            // if not, create it
                            BibsArray = new string[numlines];

                        // add this line to the array
                        BibsArray[numthisline] = linedata;

                        // test if the array is full
                        if (!BibsArray.Any(item => item == null))       // not full if any item is null
                        {
                            // if full, pass to Form1 and Load
                            SetBtnText(Download, "Download Bib # only");
                            Downloaded_Runner_List = "";
                            foreach (string line in BibsArray)
                                Downloaded_Runner_List += line;
                            BibsArray = null;
                            Modeless_MessageBox_Information("Bibs Only List received from Central Database", "List data received");
                            Runners_Download_Request_Complete = true;
                            NeedBibs = false;       // 8/9/17
                        }
                        else
                            F1timer.Start();    // 8/15/17
                    }
                    break;
                case ":F2":     // complete Runners list - added this section 8/8/17
                    // do we need this list?
                    if (NeedRunners)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F2timer.Stop();         // 8/15/17

                        // does the Runners Array already exist?
                        if (RunnersArray == null)
                            // if not, create it
                            RunnersArray = new string[numlines];

                        // add this line to the array
                        RunnersArray[numthisline] = linedata;

                        // test if the array is full
                        if (!RunnersArray.Any(item => item == null))       // not full if any item is null
                        {
                            // if full, pass to Form1 and Load
                            SetBtnText(Download, "Refresh from Central Database");
                            Downloaded_Runner_List = "";
                            foreach (string line in RunnersArray)
                                Downloaded_Runner_List += line;
                            RunnersArray = null;
                            Modeless_MessageBox_Information("Runner List received from Central Database", "List data received");
                            Runners_Download_Request_Complete = true;
                            NeedRunners = false;       // 8/9/17
                        }
                        else
                            F2timer.Start();    // 8/15/17
                    }
                    break;
                case ":F3":     // Runner Status
                    break;
                case ":F4":     // DNS list
                    // do we need this list?
                    if (NeedDNS)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F4timer.Stop();         // 8/15/17

                        // does the DNS Array already exist?
                        if (DNSArray == null)
                            // if not, create it
                            DNSArray = new string[numlines];

                        // add this line to the array
                        DNSArray[numthisline] = linedata;

                        // test if the array is full
                        if (!DNSArray.Any(item => item == null))       // not full if any item is null
                        {
                            SetBtnText(Download, "Download DNS List from Central Database");
                            Downloaded_DNS_List = "";
                            foreach (string line in DNSArray)
                                Downloaded_DNS_List += line;
                            DNSArray = null;
                            Modeless_MessageBox_Information("DNS List received from Central Database", "List data received");
                            DNS_Download_Request_Complete = true;
                            NeedDNS = false;       // 8/9/17
                        }
                        else
                            F4timer.Start();    // 8/15/17
                    }
                    break;
                case ":F5":     // DNF list
                    // do we need this list?
                    if (NeedDNF)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F5timer.Stop();         // 8/15/17

                        // does the DNF Array already exist?
                        if (DNFArray == null)
                            // if not, create it
                            DNFArray = new string[numlines];

                        // add this line to the array
                        DNFArray[numthisline] = linedata;

                        // test if the array is full
                        if (!DNFArray.Any(item => item == null))       // not full if any item is null
                        {
                            SetBtnText(Download, "Download DNF List from Central Database");
                            Downloaded_DNF_List = "";
                            foreach (string line in DNFArray)
                                Downloaded_DNF_List += line;
                            DNFArray = null;
                            Modeless_MessageBox_Information("DNF List received from Central Database", "List data received");
                            DNF_Download_Request_Complete = true;
                            NeedDNF = false;       // 8/9/17
                        }
                        else
                            F5timer.Start();    // 8/15/17
                    }
                    break;
                case ":F6":     // Watch list
                    // do we need this list?
                    if (NeedWatch)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F6timer.Stop();         // 8/15/17

                        // does the Watch Array already exist?
                        if (WatchArray == null)
                            // if not, create it
                            WatchArray = new string[numlines];

                        // add this line to the array
                        WatchArray[numthisline] = linedata;

                        // test if the array is full
                        if (!WatchArray.Any(item => item == null))       // not full if any item is null
                        {
                            SetBtnText(Download, "Download Watch List from Central Database");
                            Downloaded_Watch_List = "";
                            foreach (string line in WatchArray)
                                Downloaded_Watch_List += line;
                            WatchArray = null;
                            Modeless_MessageBox_Information("Watch List received from Central Database", "List data received");
                            Watch_Download_Request_Complete = true;
                            NeedWatch = false;       // 8/9/17
                        }
                        else
                            F6timer.Start();    // 8/15/17
                    }
                    break;
                case ":F7":     // Info File
                    // do we need this list?
                    if (NeedInfo)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F7timer.Stop();         // 8/15/17

                        // does the Info Array already exist?
                        if (InfoArray == null)
                            // if not, create it
                            InfoArray = new string[numlines];

                        // add this line to the array
                        InfoArray[numthisline] = linedata;

                        // test if the array is full
                        if (!InfoArray.Any(item => item == null))       // not full if any item is null
                        {
                            // if full, pass to Form1 and Load  -  completed 8/8/17
                            SetBtnText(Download, "Download Info from Central Database");
                            Downloaded_Info = "";
                            foreach (string line in InfoArray)
                                Downloaded_Info += line;
                            InfoArray = null;
                            Modeless_MessageBox_Information("Info File received from Central Database", "List data received");
                            Info_Download_Request_Complete = true;
                            NeedInfo = false;       // 8/9/17
                            InfoFile_Loaded = true;
                        }
                        else
                            F7timer.Start();    // 8/15/17
                    }
                    break;
                case ":F8":     // Station Info file
                    // do we need this list?
                    if (NeedStations)       // 8/9/17
                    {
                        // stop the 1 minute timer, if it is running - 8/15/17
                        F8timer.Stop();         // 8/15/17

                        // does the Station Array already exist?
                        if (StationArray == null)
                            // if not, create it
                            StationArray = new string[numlines];

                        // add this line to the array
                        StationArray[numthisline] = linedata;

                        // test if the array is full
                        if (!StationArray.Any(item => item == null))       // not full if any item is null
                        {
                            Downloaded_Stations_Info = "";
                            foreach (string line in StationArray)
                                Downloaded_Stations_Info += line;
                            StationArray = null;
                            Modeless_MessageBox_Information("Station Info File received from Central Database\n\n   Click the Save button to save it on this PC.", "File data received");
                            Stations_Download_Complete = true;
                            NeedStations = false;       // 8/9/17
                        }
                        else
                            F8timer.Start();    // 8/15/17
                    }
                    break;
            }
        }
        #endregion

        void Receive_from_Central()
        {
            switch (Connection_Type)
            {
                case Form1.Connect_Medium.APRS:
                    receiveDone.Reset();
                    receiveDonebool = false;
                    break;
                case Form1.Connect_Medium.Packet:
                    receiveDone.Reset();
                    receiveDonebool = false;
                    break;
                case Form1.Connect_Medium.Ethernet:
                    //                    Receive_Ethernet();
                    try
                    {
                        // Create the state object.
                        StateObject state = new StateObject();
                        state.workSocket = CentralClient;

                        // clear the flag
                        receiveDone.Reset();

                        // Begin receiving the data from the remote device.
                        CentralClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(Receive_Ethernet_Callback), state);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    break;
            }
        }

        //void Receive_Ethernet()
        //{
        //    try
        //    {
        //        // Create the state object.
        //        StateObject state = new StateObject();
        //        state.workSocket = CentralClient;

        //        // clear the flag
        //        receiveDone.Reset();

        //        // Begin receiving the data from the remote device.
        //        CentralClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //            new AsyncCallback(Receive_Ethernet_Callback), state);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        void Receive_Ethernet_Callback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                if (client.Connected)   // make sure we are still connected (not disposed)
                {
                    int bytesRead = client.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There might be more data, so store the data received so far.
                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                        // Get the rest of the data.
                        // changed this 2/20/16                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        //                        new AsyncCallback(ReceiveCallback), state);
                        // this change found here: https://social.msdn.microsoft.com/Forums/vstudio/en-US/05dcda20-06f9-419b-bfc0-bcb8cfeb3693/socket-receivecallback-not-sending-notification-when-buffer-has-emptied?forum=csharpgeneral
                        if (state.workSocket.Available > 0)
                        {
                            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                new AsyncCallback(Receive_Ethernet_Callback), state);
                        }
                        else
                        {
                            // All the data has arrived; put it in response.
                            if (state.sb.Length > 1)
                            {
                                Central_String_In = state.sb.ToString();
                                Console.WriteLine("Received this from Central: " + Central_String_In);

                                // display in the Richtextbox
                                AddRichText(Ethernet_Packets, Central_String_In, Color.Black);
                            }
                            // Signal that all bytes have been received.
                            receiveDone.Set();
                        }
                    }
                    else
                    {
                        // All the data has arrived; put it in response.
                        if (state.sb.Length > 1)
                        {
                            Central_String_In = state.sb.ToString();
                        }
                        else
                        {
                            switch (Connection_Type)
                            {
                                case Form1.Connect_Medium.Ethernet:
                                    try
                                    {
                                        byte[] tmp = new byte[1];

                                        CentralClient.Blocking = false;
                                        CentralClient.Send(tmp, 0, 0);
                                        //                                    return true;
                                        CentralClient.Blocking = true;
                                    }
                                    catch (SocketException ex)
                                    {
                                        // 10035 == WSAEWOULDBLOCK
                                        if (ex.NativeErrorCode.Equals(10035))
                                        {
                                            //                                        return true;
                                        }
                                        else
                                        {
                                            //                                        return false;
//                                            Connected_to_Server = false;
// 7/21/16                                            Connected_to_DB = false;
                                            ChangeState(Server_State.Error_Connecting);
                                            CentralClient.Close();
                                            CentralClient.Dispose();
                                            Console.WriteLine("CentralClient has been closed because Poll failed in Callback");

                                            // added 7/22/16
                                            Restart_Receive = true;     // this will cause Receiving Thread to restart receiving
                                            receiveDonebool = receiveDone.Set();      // this should clear the wait for receive data
                                        }
                                    }
                                    finally
                                    {
                                        //CentralClient.Blocking = true;
                                    }
                                    break;
                            }
                        }

                        // Signal that all bytes have been received.
                        receiveDone.Set();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                // we may be here because we are shutting down
                if (Server_shouldStop)
                    receiveDone.Set();  // if so, get us out of here and test Server_shouldStop after it
            }
        }

        void Push_a_Button(Form1.Button_to_Push button)
        {
            lock (Form1.Buttons_to_Push)
            {
                Form1.Buttons_to_Push.Enqueue(button);
            }
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
            Console.WriteLine("Sent this request to Central: " + Data);
        }

        void Attempt_To_Connect()  // Ethernet/MESH only
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(AttemptConnectThread));
        }

        private void AttemptConnectThread(object info)  // Ethernet/MESH only
        {
            // clear the flag and change the state
            Connect_Request = false;
            Attempting_to_Connect_to_Server = true;

            // depends upon what the connection medium is
            switch (Connection_Type)
            {
                #region Ethernet
                case Form1.Connect_Medium.Ethernet:
                    //if ((Server_IP_Address == "") || (Server_Port_Number == 0))
                    //{
                    //    Modeless_MessageBox("Central Database IP Address or Port # is not correct", "Central Database not accessible");
                    //    break;
                    //}

                    ChangeState(Server_State.Attempting_Connect);      // first change the state

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
                        CentralClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        CentralClient.Connect(Server_IP_Address, Server_Port_Number);
// 7/18/16                        ChangeState(Server_State.Connected_To_Server);
                        Application.DoEvents();
                        Thread.Sleep(1000);
                        Application.DoEvents();
                        ChangeState(Server_State.Connected_To_DB);
                        expecting = Expecting_State.StationName_Request;
                        AddRichText(Ethernet_Packets, Environment.NewLine + "Connected to: " + Server_IP_Address + ":" + Server_Port_Number, Color.Green);
                        Already_Tried_To_Connect = false;
                        Console.WriteLine("Connected to: " + Server_IP_Address + ":" + Server_Port_Number);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        ChangeState(Server_State.Error_Connecting);
                        Attempting_to_Connect_to_Server = false;

//// shouldn't need these because have not connected yet.                        // added 7/22/16
//                        Restart_Receive = true;     // this will cause Receiving Thread to restart receiving
//                        receiveDonebool = receiveDone.Set();      // this should clear the wait for receive data
                    }
                    break;
                #endregion
                #region Packet
                case Form1.Connect_Medium.Packet:
                    // try to connect to Packet
                    try
                    {
                        if (Already_Tried_To_Connect)
                            AddRichText(Packet_Packets, ".", Color.Green);
                        else
                        {
                            AddRichText(Packet_Packets, Environment.NewLine + "Attempting to connect to: " + Form1.DatabaseFCCCallsign, Color.Green);
                            Already_Tried_To_Connect = true;
                        }

                        // connect to the AGWPE server
                        Thread.Sleep(5000);     // wait 5 secs.
                        Push_a_Button(Form1.Button_to_Push.Aid_AGW_Start);
                        Thread.Sleep(5000);     // wait 5 secs.
                        Push_a_Button(Form1.Button_to_Push.Aid_AGW_Connect);
                        if (Form1.Aid_AGWSocket.Connected_to_AGWserver)
// 7/18/16 need to use it properly                            ChangeState(Server_State.Connected_To_Server);
                            ChangeState(Server_State.Connected_To_AGWServer);  // 7/22/16
                        expecting = Expecting_State.StationName_Request;
                        AddRichText(Packet_Packets, Environment.NewLine, Color.Green);
                        Already_Tried_To_Connect = false;
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        ChangeState(Server_State.Error_Connecting);
                        Attempting_to_Connect_to_Server = false;
                    }
                    break;
                #endregion
                #region APRS
                case Form1.Connect_Medium.APRS:
                    // try to connect to APRS
                    try
                    {
                        if (Already_Tried_To_Connect)
                            AddRichText(Packet_Packets, ".", Color.Green);
                        else
                        {
                            AddRichText(Packet_Packets, Environment.NewLine + "Attempting to connect to: " + Form1.DatabaseFCCCallsign, Color.Green);
                            Already_Tried_To_Connect = true;
                        }

                        // connect to the AGWPE server first, if needed
                        if (!Form1.Aid_AGWSocket.Connected_to_AGWserver)
                        {
                            Thread.Sleep(5000);     // wait 5 secs.
                            Push_a_Button(Form1.Button_to_Push.Aid_AGW_Start);
                            Thread.Sleep(5000);     // wait 5 secs.
//                            Push_a_Button(Form1.Button_to_Push.Aid_APRS_Connect);
                            if (Form1.Aid_AGWSocket.Connected_to_AGWserver)
                                ChangeState(Server_State.Connected_To_AGWServer);  // 7/22/16
                        }
                        Push_a_Button(Form1.Button_to_Push.Aid_APRS_Connect);

// need to change more here
                        expecting = Expecting_State.StationName_Request;
                        AddRichText(Packet_Packets, Environment.NewLine, Color.Green);
                        Already_Tried_To_Connect = false;
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        ChangeState(Server_State.Error_Connecting);
                        Attempting_to_Connect_to_Server = false;
                    }
                    break;
                #endregion
                default:
                    break;
            }
        }

//        // not needed for Sync
//        void ConnectCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the socket from the state object.
//                Socket client = (Socket)ar.AsyncState;
////                TcpClient client = (TcpClient)ar.AsyncState;

//                // Complete the connection.
//                client.EndConnect(ar);

//                Console.WriteLine("Socket connected to {0}",
//                    client.RemoteEndPoint.ToString());

//                // Signal that the connection has been made.
//                connectDone.Set();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

        void FiveSecondTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {       // this happens every 5 seconds
            if (!Server_shouldStop)
            {
                // test connection state for Packet
                if (Connection_Type == Form1.Connect_Medium.Packet)
                {
//// 7/18/16 need to change this                    // test if just connected to the Central Database
//                    if ((state == Server_State.Connected_To_Server) && (Form1.Aid_AGWSocket.Connected_to_Database))
//                        ChangeState(Server_State.Connected_To_DB);

//                    // test if have just become disconnected from the Central Database
//                    if (((state == Server_State.Connected_To_DB) || (state == Server_State.Connected_Active)) && (!Form1.Aid_AGWSocket.Connected_to_Database))
//                    {
//                        ChangeState(Server_State.Connected_To_Server);
//                        expecting = Expecting_State.StationName_Request;
//                    }
                }

                // test connection state for APRS
                //     public enum Server_State { Not_Initted, Cannot_Connect, Attempting_Connect, Error_Connecting, Connected_To_AGWServer, Connected_To_DB, Connected_Active, Lost_Connection }
                if (Connection_Type == Form1.Connect_Medium.APRS)
                {
                    // test if connected to AGWServer yet
                    if ((state == Server_State.Not_Initted) && (Form1.Aid_AGWSocket.Connected_to_AGWserver))
                        ChangeState(Server_State.Connected_To_AGWServer);

                    // test if just connected to the Central Database
                    if ((state == Server_State.Connected_To_AGWServer) && (Form1.Aid_AGWSocket.Connected_to_Database))
                        ChangeState(Server_State.Connected_To_DB);

                    //// test if have just become disconnected from the Central Database
                    //if (((state == Server_State.Connected_To_DB) || (state == Server_State.Connected_Active)) && (!Form1.Aid_AGWSocket.Connected_to_Database))
                    //{
                    //    ChangeState(Server_State.Connected_To_Server);
                    //    expecting = Expecting_State.StationName_Request;
                    //}


                    #region Test if an APRS message is overdue being acknowledged - should not test so often
                    foreach (APRS_Message mess in APRS_Message_Status)
                    {
                        if (!mess.acknowledged)
                        {
                            DateTime Now = DateTime.Now;
                            TimeSpan elapsed = Now - mess.time_sent;
                            if (elapsed.Seconds > 35)
                            {
                                // resend the message
// 8/6/17                                Form1.DB_AGWSocket.SendMessageWithID(mess.port, Form1.DatabaseFCCCallsign, mess.message, mess.number);
// 8/7/17                                Form1.Aid_AGWSocket.SendMessageWithID(mess.port, Form1.DatabaseFCCCallsign, mess.message, mess.number);      // 8/6/17
                                Form1.Aid_AGWSocket.SendMessageWithID(mess.port, Form1.DatabaseFCCCallsign, mess.message, mess.msg_number);      // 8/7/17
                                mess.time_sent = Now;   // update the time
// 8/7/17                                Console.WriteLine("Had to resend APRS message # " + mess.number.ToString() + " to: " + Form1.DatabaseFCCCallsign);
                                Console.WriteLine("Had to resend APRS message # " + mess.msg_number.ToString() + " to: " + Form1.DatabaseFCCCallsign);  // 8/7/17
                            }
                        }
                    }
                    #endregion
                }

                // 7/22/16          // See if there is data to send or receive from the Central Database
                // has another Connect Request been issued?
                if (Connect_Request)
                {
                    // if we are already connected = then need to Disconnect first
                    if (Connected_to_DB)
                    {
                        CentralClient.Close();
                        CentralClient.Dispose();
                    }

                    // now try to connect again
                    Attempt_To_Connect();
                }
                else
                {
                    // test if lost connection last time
                    if (Connection_Lost)
                    {
                        ChangeState(Server_State.Lost_Connection);
                        CentralClient.Close();
                        CentralClient.Dispose();
                        Console.WriteLine("CentralClient has been closed because Poll failed in 5 sec timer");

                        // added 7/22/16
                        Restart_Receive = true;     // this will cause Receiving Thread to restart receiving
                        receiveDonebool = receiveDone.Set();      // this should clear the wait for receive data
                        Connection_Lost = false;
                    }
                    else
                    {
                        // do only if connected
                        if (Connected_to_DB)
                        {
                            // first test if the connection is still good
                            switch (Connection_Type)
                            {
                                case Form1.Connect_Medium.Ethernet:
                                    if (CentralClient.Poll(1000, SelectMode.SelectRead) && CentralClient.Available == 0)
                                    {
                                        Console.WriteLine("CentralClient will be closed because Poll failed in 5 sec timer");
                                        Connection_Lost = true;
                                    }
                                    break;
                            }

                            // test again, in case connection lost
                            if (Connected_to_DB)
                            {
                                #region Receive data - only AGWSocket data
                                // test for packet data from AGWSocket
                                if ((Connection_Type == Form1.Connect_Medium.Packet) || (Connection_Type == Form1.Connect_Medium.APRS))
                                {
                                    if (!receiveDonebool && Form1.Aid_AGWSocket.Receive_DataAvailable)  // do not look if already got data or Receive not started
                                    {
                                        lock (Form1.Aid_AGWSocket.Receive_Buffer)
                                        {
                                            Central_String_In += Form1.Aid_AGWSocket.Receive_Buffer;
                                            Form1.Aid_AGWSocket.Receive_Buffer = "";
                                            Form1.Aid_AGWSocket.Receive_DataAvailable = false;
                                        }
                                        Console.WriteLine("Received this from Central: " + Central_String_In);

                                        // display in the Richtextbox
                                        AddRichText(Packet_Packets, Central_String_In, Color.Black);

                                        receiveDonebool = receiveDone.Set();      // this creates semi-Asynchronous mode
                                    }
                                }
                                #endregion

                                #region Send data - nothing here
                                // test if there is any data going from the Server to the Central Database
                                //// 7/9/16                            if (Central_Data_Ready_to_Send)
                                //                            {
                                //                                Send_to_Central(Central_String_Out);

                                //                                // clear the worker flag
                                //                                Central_Data_Ready_to_Send = false;
                                //                            }
                                #endregion
                            }
                        }
                    }
                }
                Application.DoEvents();
            }
        }

        void SendNewAPRSmessage(int port, string data)
        {
//            Form1.Aid_AGWSocket.SendMessageWithID(port, Station_Callsign, data, Next_Msg_Num);
            Form1.Aid_AGWSocket.SendMessageWithID(port, Form1.DatabaseFCCCallsign, data, Next_Msg_Num);

            APRS_Message newmess = new APRS_Message();
            newmess.time_sent = DateTime.Now;
            newmess.acknowledged = false;
            newmess.message = data;
// 8/7/17            newmess.number = Next_Msg_Num;
            newmess.msg_number = Next_Msg_Num;      // 8/7/17
            newmess.port = port;
            APRS_Message_Status.Add(newmess);       // we add new entries, but never remove any. Should we?

            Next_Msg_Num++;
        }

        void Send_to_Central(string data)
        {
            switch (Connection_Type)
            {
                case Form1.Connect_Medium.APRS:
                    //                    Form1.Aid_AGWSocket.TXdataUnproto(Form1.AGWPERadioPort, "BEACON", ">" + data);
                    //                    Form1.Aid_AGWSocket.TXdataUnproto(Form1.AGWPERadioPort, ">" + data);
                    SendNewAPRSmessage(Form1.AGWPERadioPort, data);
                    AddRichText(Packet_Packets, data, Color.Red);
                    break;
                case Form1.Connect_Medium.Packet:
                    Form1.Aid_AGWSocket.AGWSend(Form1.AGWPERadioPort, data);

                    // display in the Richtextbox
// 4/3/16 - already in AGWSend                    AddRichText(Packet_Packets, Central_String_Out, Color.Red);
                    break;
                case Form1.Connect_Medium.Ethernet:
                    // Convert the string data to byte data using ASCII encoding.
                    byte[] byteData = Encoding.ASCII.GetBytes(data);

                    // Begin sending the data to the remote device.
                    CentralClient.Send(byteData, 0, byteData.Length, 0);    //, new AsyncCallback(SendCallback_Ethernet), CentralClient);

                    // display in the Richtextbox
                    AddRichText(Ethernet_Packets, data, Color.Red);
                    break;
            }
        }

        void ChangeState(Server_State newstate)
        {
            if (state != newstate)      // if already at this state, no change
            {
                // set the new state
                state = newstate;

                // clear all the message labels
                MakeVisible(Server_Initting, false);
                MakeVisible(Cannot_Connect, false);
                MakeVisible(Server_Attempting_Connection, false);
                MakeVisible(Server_Connected, false);
                MakeVisible(Server_Connected_Active, false);
                MakeVisible(AGWPE_Connected, false);
                MakeVisible(Error_Connecting, false);
                MakeVisible(Lost_Connection, false);

                // set the new message label
                switch (state)
                {
                    case Server_State.Not_Initted:
                        MakeVisible(Server_Initting, true);
                        break;
                    case Server_State.Cannot_Connect:
                        MakeVisible(Cannot_Connect, true);
                        break;
                    case Server_State.Attempting_Connect:
                        expecting = Expecting_State.StationName_Request;
                        MakeVisible(Server_Attempting_Connection, true);
                        if (Connection_Type == Form1.Connect_Medium.Packet)
                            Attempting_to_Connect_to_Server = false;
                        break;
//// 7/18/16 need to add this back in for AGWPE                    case Server_State.Connected_To_Server:
//                        Connected_to_Server = true;
//                        Attempting_to_Connect_to_Server = false;
//                        break;
                    case Server_State.Connected_To_DB:
                        MakeVisible(Server_Connected, true);
                        Connected_to_DB = true;
                        Attempting_to_Connect_to_Server = false;
                        break;
                    case Server_State.Connected_Active:
//                        MakeVisible(Server_Connected, true);    // 7/30/15
                        MakeVisible(Server_Connected_Active, true);    // 7/30/15
                        Connected_and_Active = true;    // 8/2/15
//                        Form1.Connected_to_Central_Database = true;  // 3/31/17
                        Attempting_to_Connect_to_Server = false;  // 7/30/15
                        break;
                    case Server_State.Connected_To_AGWServer:
                        MakeVisible(AGWPE_Connected, true);
                        Connected_to_AGWServer = true;
                        Attempting_to_Connect_to_Server = false;
                        break;
                    case Server_State.Error_Connecting:
                        SetText(Server_Error_Message, error);
                        MakeVisible(Error_Connecting, true);
                        MakeVisible(Connect_Button, true);
                        Attempting_to_Connect_to_Server = false;
                        Connected_to_DB = false;
                        Connected_and_Active = false;
                        break;
                    case Server_State.Lost_Connection:
                        SetText(Server_Error_Message, "Lost connection");
                        MakeVisible(Lost_Connection, true);
                        Attempting_to_Connect_to_Server = false;
                        Connected_to_DB = false;
                        Connected_and_Active = false;
                        break;
                    default:
                        break;
                }
            }
        }

        #region delegates
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
        #endregion

        public void RequestStop()
        {
            Server_shouldStop = true;
            receiveDonebool = receiveDone.Set();      // this should clear the wait for receive data
            Runner_Status_Received = true;
//            Server_shouldStop = true;
        }

        public void RequestConnect()
        {
            Connect_Request = true;
        }

        public static void Modeless_MessageBox_Exclamation(string message, string title)    // changed name 7/13/17
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

        public static void Modeless_MessageBox_Information(string message, string title)    // added 7/13/17
        {
            // Start the message box thread 
            new Thread(new ThreadStart(delegate
            {
                MessageBox.Show
                (
                  message,
                  title,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information
                );
            })).Start();
            // Continue doing stuff while the message box is visible to the user. 
            // The message box thread will end itself when the user clicks OK. 
        }
    }

    #region Asynchronous Client Functions - originals added 2/16/16, but not implemented until 2/19/16
    //
    //    // the original example was found here:  https://msdn.microsoft.com/en-us/library/bew39x2a%28v=vs.110%29.aspx
    //

//// State object for receiving data from remote device.
//public class StateObject {
//    // Client socket.
//    public Socket workSocket = null;
//    // Size of receive buffer.
//    public const int BufferSize = 256;
//    // Receive buffer.
//    public byte[] buffer = new byte[BufferSize];
//    // Received data string.
//    public StringBuilder sb = new StringBuilder();
//}
 
//public class AsynchronousClient {
//    // The port number for the remote device.
//    private const int port = 11000;
 
//    // ManualResetEvent instances signal completion.
//    private static ManualResetEvent connectDone =
//        new ManualResetEvent(false);
//    private static ManualResetEvent sendDone =
//        new ManualResetEvent(false);
//    private static ManualResetEvent receiveDone =
//        new ManualResetEvent(false);
 
//    // The response from the remote device.
//    private static String response = String.Empty;
 
//    private static void StartClient() {
//        // Connect to a remote device.
//        try {
//            // Establish the remote endpoint for the socket.
//            // The name of the
//            // remote device is "host.contoso.com".
//            IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
//            IPAddress ipAddress = ipHostInfo.AddressList[0];
//            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
 
//            // Create a TCP/IP socket.
//            Socket client = new Socket(AddressFamily.InterNetwork,
//                SocketType.Stream, ProtocolType.Tcp);
 
//            // Connect to the remote endpoint.
//            client.BeginConnect( remoteEP,
//                new AsyncCallback(ConnectCallback), client);
//            connectDone.WaitOne();
 
//            // Send test data to the remote device.
//            Send(client,"This is a test<EOF>");
//            sendDone.WaitOne();
 
//            // Receive the response from the remote device.
//            Receive(client);
//            receiveDone.WaitOne();
 
//            // Write the response to the console.
//            Console.WriteLine("Response received : {0}", response);
 
//            // Release the socket.
//            client.Shutdown(SocketShutdown.Both);
//            client.Close();
 
//        } catch (Exception e) {
//            Console.WriteLine(e.ToString());
//        }
//    }
 
//    private static void ConnectCallback(IAsyncResult ar) {
//        try {
//            // Retrieve the socket from the state object.
//            Socket client = (Socket) ar.AsyncState;
 
//            // Complete the connection.
//            client.EndConnect(ar);
 
//            Console.WriteLine("Socket connected to {0}",
//                client.RemoteEndPoint.ToString());
 
//            // Signal that the connection has been made.
//            connectDone.Set();
//        } catch (Exception e) {
//            Console.WriteLine(e.ToString());
//        }
//    }

//    private static void Receive(Socket client) {
//        try {
//            // Create the state object.
//            StateObject state = new StateObject();
//            state.workSocket = client;
 
//            // Begin receiving the data from the remote device.
//            client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
//                new AsyncCallback(ReceiveCallback), state);
//        } catch (Exception e) {
//            Console.WriteLine(e.ToString());
//        }
//    }
 
//    private static void ReceiveCallback( IAsyncResult ar ) {
//        try {
//            // Retrieve the state object and the client socket
//            // from the asynchronous state object.
//            StateObject state = (StateObject) ar.AsyncState;
//            Socket client = state.workSocket;
 
//            // Read data from the remote device.
//            int bytesRead = client.EndReceive(ar);
 
//            if (bytesRead > 0) {
//                // There might be more data, so store the data received so far.
//            state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));
 
//                // Get the rest of the data.
//                client.BeginReceive(state.buffer,0,StateObject.BufferSize,0,
//                    new AsyncCallback(ReceiveCallback), state);
//            } else {
//                // All the data has arrived; put it in response.
//                if (state.sb.Length > 1) {
//                    response = state.sb.ToString();
//                }
//                // Signal that all bytes have been received.
//                receiveDone.Set();
//            }
//        } catch (Exception e) {
//            Console.WriteLine(e.ToString());
//        }
//    }
 
//    private static void Send(Socket client, String data) {
//        // Convert the string data to byte data using ASCII encoding.
//        byte[] byteData = Encoding.ASCII.GetBytes(data);
 
//        // Begin sending the data to the remote device.
//        client.BeginSend(byteData, 0, byteData.Length, 0,
//            new AsyncCallback(SendCallback), client);
//    }
 
//    private static void SendCallback(IAsyncResult ar) {
//        try {
//            // Retrieve the socket from the state object.
//            Socket client = (Socket) ar.AsyncState;

//            // Complete sending the data to the remote device.
//            int bytesSent = client.EndSend(ar);
//            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

//            // Signal that all bytes have been sent.
//            sendDone.Set();
//        } catch (Exception e) {
//            Console.WriteLine(e.ToString());
//        }
//    }

//    public static int Main(String[] args) {
//        StartClient();
//        return 0;
//    }
//}
    #endregion
}