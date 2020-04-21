using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;

namespace RunnerTracker
{
    public class DB_AGWSocket
    {
        //  Updates:
        //  July 27, 2017 - added Label variables: Initting and AGWPE_Connected
        //                  added delegate: MakeVisible
        //                  made labels show when AGWPE is connected
        //  August 5, 2017 - added time delay in InitAGWThread to improve getting connected completely
        //                   changed write to Anonymous Pipe from Write to WriteLine
        //  8/7/17 - changed SendMessageWithID to handle message = 0
        //

        // As part of the AGWPE initialization (Properties),
        // if using a KPC3, set it this way:
        //  choose COM port
        //  9600
        //  KPC3
        //  KISS Simple
        //  IniKiss1 = INTFACE KISS
        //  IniKiss2 = RESET
        //  IniKiss3 =   <blank>
        //  ExitKiss On Exit checked

        // When a Tactical Callsign is used, this is how the FCC callsign will be advertised:
        //
        //      A 10 minute timer will be used to set a flag (BeaconTimeElapsed)
        //          This flag is initialized as set
        //      After each normal transmission, the flag is tested.
        //          If flag set, send Beacon text, clear the flag and start the timer
        //
        //      Also, if using Tactical callsign, send the Beacon text after a failed Connect attempt
        //

        //Also: 
        //
        //RT Database APRS Requirements 
        //
        //1. DB station must respond to the query ?AZRT? by sending this Status report: 
        //	>092345zRT Central Database 
        //	The numbers at the beginning are the time in DHM zulu format: day-of-month, hour, minute 
        // 
        //2. Listen for any station sending this Status report:  >RT Station name = <aid station name> 
        //	Respond by sending the Welcome message and marking this station as Active. 
        // 
        //3. Or ... instead of listening for a Status report, look for a Message from any station with this same text. 
        //	Respond as described above. 

        #region Variables and declarations
        private bool Database_Socket;
        bool Receive_Loop_shouldStop = false;
        public Button Connect_Button { get; set; }
        enum PacketTypes { MM = 0, MI = 2, MU = 4, MS = 8, MC = 16, MTX = 32, MRJ = 64, MVIA = 1 }
        public bool Connected_to_AGWserver;
        public bool InitInProcess;
        public bool Registered;
        public bool Tactical_Registered;
        public bool ConnectInProcess;
// ???        public bool Connected_to_Database;
        public bool Cannot_Connect_to_DB;
//        public bool Monitoring;
        private bool RegisterOnInit;
        public string Version;
        private byte[] ViaString = new byte[3*10];  // space for 3 callsigns, 10 bytes each
        private int ViaCount = 0;
        private Int32 VerNum;
        private bool HeartBeat;
        private int FiveSec;
        private int DB_Connect_Count = 0;
        public Label Initting { get; set; }         // 7/27/17
        public Label AGWPE_Connected { get; set; }  // 7/27/17
        bool IsCreated, PortsFound;
//        int MonitorKind;
        private string Callsign_Registered;
        private string Tactical_Callsign_Registered;
        private string Call_to_Register;
        public PORTS Ports = new PORTS();
        Socket AGWPEclient;
        bool InUse;
        bool BeaconTimeElapsed = false;
        System.Timers.Timer TenMinute;
        System.Timers.Timer OneSecond;
        System.Timers.Timer TenSecond;
        System.Timers.Timer OneMinute;
        string ConnectErrorMessage = "Cannot connect to the AGWPE server!";
        public RichTextBox Packet_Node_Packets { get; set; }
        delegate void ButtonTextdel(Button butn, string text, Color forecolor);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        AnonymousPipeServerStream PacketInData;      // data from AGWSocket to Packet_StationWorker
        StreamWriter sw_data_to_Worker;
//        StreamReader sr_data_from_Worker;
        Dictionary<string, StreamWriter> PipeWriteDictionary;
        List<ConnectedStation> ConnectedStations;
        public class Pdata
        {
            public string PortName;
        };
        public class PORTS
        {
	        public int Num;
            public Pdata[] Pdata = new Pdata[10];    // more than enough space for 3 radio channels
        };
        class HEADER
        {
            public int Port;
            public long DataKind;
            public string CallFrom;
            public string CallTo;
            public int DataLen;
            public long User = 0;   // reserved, unused
        };
        const int SizeOfHeader = 36;
        public class TXHEADER
        {
            public int Port;
            public long DataKind;
            public string CallFrom;
            public string CallTo;
            public int DataLen;
            public long User;  // reserved, unused
            public string Data;
        };
        class PACKET
        {
            public byte[] Header = new byte[60];
            public byte[] DataBuff = new byte[500];         // can probably reduce to about 256
        }
        class ConnectedStation
        {
            public string Callsign;
            public AnonymousPipeServerStream PacketOutData;
        }

        // added 4/7/16, to display the AGWPE Statistics
        public class AGWPEPortStat
        {
            public byte BaudRate;
            public byte TrafficLevel;
            public byte TxDelay;
            public byte TxTail;
            public byte Persist;
            public byte SlotTime;
            public byte MaxFrame;
            public byte NumConnections;
            public Int32 NumBytesReceived;
            public Int32 NumPendingConnectionFrames;
            public Int32 NumPendingPortFrames;
        }

        delegate void MakeVisibledel(Control cntrl, bool visible);      // 7/27/17
        #endregion

        public DB_AGWSocket(RichTextBox rtb, bool DBRole)
        {
            Packet_Node_Packets = rtb;
            Database_Socket = DBRole;
            Callsign_Registered = string.Empty;
            Connected_to_AGWserver = false;
            InitInProcess = false;
//            Monitoring = false;
	        IsCreated = false;
            HeartBeat = false;
            Registered = false;
            Cannot_Connect_to_DB = false;
            ConnectInProcess = false;
//            MonitorKind = 62;
            PortsFound = false;
            InUse = false;
            VerNum = 0;

            // init the Pipe Stream Server to send the Packet data to the Worker thread
            PacketInData = new AnonymousPipeServerStream(PipeDirection.Out);
            Form1.PacketInDataHandle = PacketInData.GetClientHandleAsString();
            //sr_data_from_Worker = new StreamReader(PacketOutData);
            //sw_data_to_Worker = new StreamWriter(PacketInData);
            PipeWriteDictionary = new Dictionary<string, StreamWriter>();
            ConnectedStations = new List<ConnectedStation>();

            // start the One Second timer
            OneSecond = new System.Timers.Timer();
            OneSecond.AutoReset = true;
            OneSecond.Interval = 1000;     // 1 sec.
            OneSecond.Elapsed += new ElapsedEventHandler(OneSecond_Elapsed);
            OneSecond.Start();

            // initialize the Ten Second timer - but do not start here
            TenSecond = new System.Timers.Timer();
            TenSecond.AutoReset = true;
//            TenSecond.Interval = 10000;     // 10 sec.
            TenSecond.Interval = 15000;     // 10 sec. - changed to 15
            TenSecond.Elapsed += new ElapsedEventHandler(TenSecond_Elapsed);
// 4/7/16 - will not use the HeartBeat function to test AGWPE            TenSecond.Start();

            // initialize the One Minute timer - but start after creating the AGWSocket client
            OneMinute = new System.Timers.Timer();
            OneMinute.AutoReset = true;
            OneMinute.Interval = 60000;     // 60 sec. = 1 minute
            OneMinute.Elapsed += new ElapsedEventHandler(OneMinute_Elapsed);

            // initialize the Ten Minute timer - but do not start here
            TenMinute = new System.Timers.Timer();
            TenMinute.AutoReset = false;
            TenMinute.Interval = 1000 * 60 * 10;     // 1 sec. x 60 sec. x 10 min.
            TenMinute.Elapsed += new ElapsedEventHandler(TenMinute_Elapsed);
        }

        #region Timers
        #region One Second timer
        //        static int ConnectCount = 0;
        bool HeaderRcvd = false;
        PACKET packet;
        void OneSecond_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function has only one purpose:
            //      1. watch for data to receive from the AGWPE Server
            //

            try
            {
                if (AGWPEclient != null)
                {
                    // is there any new data from the client?
                    int NewBytes = AGWPEclient.Available;
                    if (NewBytes > 0)
                    {
                        // are we waiting for more data?
                        if (HeaderRcvd)
                        {       // waiting for more data
                            int w = 8;		// set breakpoint here
                        }
                        else
                        {       // not waiting for more data - this is totally new data
                            // is there enough data for the Header?
                            if (NewBytes < SizeOfHeader)
                                return;     // quit early - too little data

                            // create a new Packet
                            packet = new PACKET();

                            // read the socket data into the packet buffer
                            AGWPEclient.Receive(packet.Header, SizeOfHeader, SocketFlags.None);

                            // set the flag
                            HeaderRcvd = true;

                            // is there a data section to the packet?
                            int DataLen = packet.Header[28];
                            if (DataLen > 0)
                            {       // Data is included with this packet
                                // have all the data bytes been received?
                                if ((SizeOfHeader + DataLen) > NewBytes)
                                    return;     // need to wait for more bytes

                                // move the data bytes to the data buffer
                                AGWPEclient.Receive(packet.DataBuff, DataLen, SocketFlags.None);
                            }

                            // clear the flag
                            HeaderRcvd = false;

                            // go process the packet
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketThread), packet);
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }
        }
        #endregion

        #region Ten Second timer
        void TenSecond_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function has only one purpose:
            //      1. a Heart Beat to test the AGWPE connection
            //

            // test for the Heart Beat only when the connection has been made and the client is still creaated (not shutting down)
            if (Connected_to_AGWserver && IsCreated)
            {
                // test if HeartBeat is still set, if so, AGWPE has stopped
                if (HeartBeat)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(TellUserStoppedThread));
                    Connected_to_AGWserver = false;
                }
                else
                {
                    // has the Version number been set yet?
                    if (VerNum != 0)
                        HeartBeat = true;               // Yes - set the flag so a new version number will be tested
                    HEADER Hed = new HEADER();      // send the Version request
                    Hed.DataKind = (long)('R');
                    SendAGWFrame(Hed);
                    if (!Connected_to_AGWserver)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(TellUserStoppedThread));
                    }
                }
            }
        }
        #endregion

        #region One Minute timer
        void OneMinute_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function has only one purpose:
            //      1. send the 'y', 'Y' and 'g' requests to AGWPE
            //          This will update the Statistics for AGWPE
            //

            // Send the 'y' request
            HEADER Hed = new HEADER();  // zero everything
            Hed.Port = Form1.AGWPERadioPort;
            Hed.DataKind = (long)('y'); // queries # of Pending TX frames for the port
            SendAGWFrame(Hed);

            // Send the 'Y' request
            Hed = new HEADER();  // zero everything
            Hed.Port = Form1.AGWPERadioPort;
            Hed.CallFrom = Callsign_Registered;
            Hed.DataKind = (long)('Y');
            SendAGWFrame(Hed);

            // Send the 'g' request
            Hed = new HEADER();  // zero everything
            Hed.Port = Form1.AGWPERadioPort;
            Hed.DataKind = (long)('g');
            SendAGWFrame(Hed);
        }
        #endregion

        #region Ten Minute timer
        void TenMinute_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function has only one purpose:
            //      1. set the Beacon time elapsed flag
            //

            BeaconTimeElapsed = true;
        }
        #endregion
        #endregion

        #region Delegates
        void MakeVisible(Control cntl, bool visible)        // added 7/27/17
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!cntl.IsDisposed)
            {
                if (cntl.InvokeRequired)
                {
                    MakeVisibledel d = new MakeVisibledel(MakeVisible);
                    cntl.Invoke(d, new object[] { cntl, visible });
                }
                else
                {
                    cntl.Visible = visible;
                    cntl.Update();
                    Application.DoEvents();
                }
            }
        }
        #endregion

        #region Processing threads
        #region Thread to ask user if he wants to start AGWPE, because we cannot connect to it, so it must not be running
        private void StartAGWPEThread(object info)
        {
            DialogResult res = MessageBox.Show("Connection to the AGWPE server failed, with this error message:\n\n" + ConnectErrorMessage + "\n\n              Start the AGWPE server now?", "AGWPE Connection Failed!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes)
            {
                try
                {
                    Thread.Sleep(6000); // wait 6 secs.
                    InitAGWPE(RegisterOnInit);        // try to connect again
                }
                catch
                {
                    Console.WriteLine("Cannot start the AGWPE exe program");
                }
            }
        }
        #endregion

        #region Thread to Process the Packet
        private void ProcessPacketThread(object info)
        {
            PACKET packet = (PACKET)info;
            TXHEADER TxHed = new TXHEADER();
            TxHed.Port = packet.Header[0];
            TxHed.DataKind = packet.Header[4];
            TxHed.DataLen = packet.Header[28];
            byte[] bytearray = new byte[10];
            System.Buffer.BlockCopy(packet.Header, 8, bytearray, 0, 9);
            TxHed.CallFrom = Encoding.ASCII.GetString(bytearray);
            bytearray = new byte[10];
            System.Buffer.BlockCopy(packet.Header, 18, bytearray, 0, 9);
            TxHed.CallTo = Encoding.ASCII.GetString(bytearray);
            string response = Encoding.ASCII.GetString(packet.DataBuff, 0, TxHed.DataLen);

        	switch(TxHed.DataKind)
		    {
			    case 'U':   // MONITOR DATA UNPROTO - '85'
                    // do only for APRS, not Packet
                    // watch only for packets sent to the desired APRS Network
                    if (TxHed.CallTo.Substring(0, TxHed.CallTo.IndexOf('\0')) == Form1.APRSnetworkName)
                    {
                        // test if an Aid Station is looking for the Central Database
                        if (response.Contains("?AZRT?"))
                        {
                            // create the zulu formatted time, to respond to Aid Station queries of ?AZRT? 
                            DateTime Now = DateTime.Now.ToUniversalTime();
                            string report = ">" + Now.Day.ToString("00") + Now.Hour.ToString("00") + Now.Minute.ToString("00") + "zRT Central Database";
                            TXdataUnproto(Form1.AGWPERadioPort, report);

                            // create a new Connect thread
                            ConnectThreadInfo threadinfo2 = new ConnectThreadInfo();
                            threadinfo2.Callsign = TxHed.CallFrom;
                            threadinfo2.response = "APRS";
                            ThreadPool.QueueUserWorkItem(new WaitCallback(New_ConnectThread), threadinfo2);
                        }
                        else
                        {
                            string CallFrom = TxHed.CallFrom.Substring(0,TxHed.CallFrom.IndexOf('\0'));
                            if (CallFrom != Callsign_Registered)    // test if it is seeing its own packet being received
                            {
                                string resp2 = response.Substring(response.IndexOf("]\r") + 2); // strip off the front formatting, leaving only the data

                                // remove CR, LF and NULL from end of the data
//// 5/31/17 must include a CR                                resp2 = resp2.Replace("\r", "");
//                                resp2 = resp2.Replace("\r", "");
                                resp2 = resp2.Replace("\r\r", "\r");
                                resp2 = resp2.Replace("\n", "");
                                resp2 = resp2.Replace("\0", "");

                                // expecting either a status report (>) or an APRS message
                                // test if it is a status report
                                if (resp2.StartsWith(">"))
                                    resp2 = resp2.Substring(1);

                                // test if an APRS message
                                if (resp2.StartsWith(":" + Callsign_Registered.PadRight(9) + ":"))
                                    resp2 = resp2.Substring(11);    // remove the APRS message formatting

                                // now send it to the station worker
                                Send_Data_to_Packet_Worker(CallFrom, resp2);
                                Console.WriteLine("DB_AGWSocket sent this to DB_APRS_Stationworker: " + resp2);
                            }
                        }
                        AddRichText(Packet_Node_Packets, response, Color.Black);
                    }
                    break;
                case 'm':   // request to start Monitoring
                    break;
			    case 'T':   // Response from sending data - '84'
                    break;
			    case 'S':   //MONITOR HEADER
                    break;
			    case 'I'    ://MONITOR  HEADER+DATA CONNECT OTHER STATIONS
                    break;
			    case 'H':   //MHeardList
                    break;
			    case 'G':   // Radio Ports list - '71'
                    GetPorts(response);
                    PortsFound = true;
                    break;
                case 'g':   // Port Information - '6D' - 103
                    Form1.AGWPEPortStatistics.BaudRate = (byte)response[0];
                    Form1.AGWPEPortStatistics.TrafficLevel = (byte)response[1];
                    Form1.AGWPEPortStatistics.TxDelay = (byte)response[2];
                    Form1.AGWPEPortStatistics.TxTail = (byte)response[3];
                    Form1.AGWPEPortStatistics.Persist = (byte)response[4];
                    Form1.AGWPEPortStatistics.SlotTime = (byte)response[5];
                    Form1.AGWPEPortStatistics.MaxFrame = (byte)response[6];
                    Form1.AGWPEPortStatistics.NumConnections = (byte)response[7];
                    Form1.AGWPEPortStatistics.NumBytesReceived = response[8];
                    break;
                case 'y':   // Outstanding frames waiting on Port - '79'
                    Form1.AGWPEPortStatistics.NumPendingPortFrames = response[0];
                    break;
                case 'Y':   // Outstanding frames waiting on Connection - '59'
                    Form1.AGWPEPortStatistics.NumPendingConnectionFrames = response[0];
                    break;
                case 'X':   // response to Register a callsign - '88'
                    if (TxHed.DataLen == 1)
                    {
                        Registered = true;
                        Callsign_Registered = Call_to_Register;
                    }
                    else
                        Registered = false;
                    break;
                case 'x':   // response to un-Register a callsign - '88' ???
                    // nothing is returned by AGWPE for this call
                    break;
                case 'R':   // version number - '82'
                    if (TxHed.DataLen == 8)
                    {
                        // has the HeartBeat been sent?
                        if (HeartBeat)
                        {
                            // verify the response is valid
                            if (VerNum != packet.DataBuff[0])
                            {
                                MessageBox.Show("AGWPE Version number does not match!", "Invalid Version number", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                HeartBeat = false;
                            }
                        }
                        else
                        {
                            VerNum = packet.DataBuff[0];
                            Version = packet.DataBuff[0].ToString() + "." + packet.DataBuff[4].ToString();
                        }
                    }
                    break;
                case 'v':   // response to Connect Via
//// not needed for Database                    ConnectInProcess = false;
//                    if (TxHed.DataLen == 1)
//                        Connected_to_Database = true;
//                    else
//                        Connected_to_Database = false;
                    break;
                case 'C':   // Connected - '67'
                    ConnectThreadInfo threadinfo = new ConnectThreadInfo();
                    threadinfo.Callsign = TxHed.CallFrom;
                    threadinfo.response = response;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(New_ConnectThread), threadinfo);
                    break;
                case 'd':   // - '100'
                    // look at CallFrom and response
                    // possible responses: "*** DISCONNECTED RETRYOUT"  -  get this when a Connect attempt times out, which should not happen with the Database
                    //                     "*** DISCONNECTED From"   -  get this when the station disconnects itself
                    //
                    // If station disconnects:  CallFrom = station,  CallTo = Database
                    // If Database disconnects station:  CallFrom = Database,  CallTo = station
                    //
                    string callsignFrom = TxHed.CallFrom.Substring(0, TxHed.CallFrom.IndexOf('\0'));
                    string callsignTo = TxHed.CallTo.Substring(0, TxHed.CallTo.IndexOf('\0'));

                    // this is the old method
                    //// verify the callsign is in the PipeWriteDictionary
                    //if (!PipeWriteDictionary.ContainsKey(callsignFrom))
                    //{
                    //    AddRichText(Packet_Node_Packets, Environment.NewLine + "Station with callsign: " + callsignFrom + " attempted to disconnect, but is not in the Dictionary!" + Environment.NewLine, Color.Green);
                    //    Modeless_MessageBox("Station with callsign: " + callsignFrom + " has attempted to disconnect," + Environment.NewLine + "but is not in the Dictionary!", "Disconnect");
                    //}
                    //else
                    //{       // callsign is in disctionary - ok to disconnect and remove
                    //    // if response says: "DISCONNECTED From", then the station disconnected. We need to disconnect if from this side also
                    //    if (response.Contains("CTED From"))
                    //    {
                    //        Disconnect(callsignFrom);
                    //    }
                    //    else
                    //    {       // must be "DISCONNECTED RETRYOUT", so we just disconnected him
                    //        AddRichText(Packet_Node_Packets, Environment.NewLine + "Station with callsign: " + callsignFrom + " has just disconnected!" + Environment.NewLine, Color.Green);
                    //        Modeless_MessageBox("Station with callsign: " + callsignFrom + " has just disconnected!", "Disconnect");
                    //        lock (Form1.Packet_Connects_Disconnects)
                    //        {
                    //            Form1.Queue_Packet_Connect_Disconnect new_connect = new Form1.Queue_Packet_Connect_Disconnect();
                    //            new_connect.ConDis = 'D';
                    //            new_connect.Reason = response;
                    //            new_connect.Callsign = callsignFrom;
                    //            Form1.Packet_Connects_Disconnects.Enqueue(new_connect);
                    //        }
                    //        Send_Data_to_Packet_Worker(callsignFrom, response);
                    //        PipeWriteDictionary.Remove(callsignFrom);
                    //    }
                    //}

                    // new method

                    if (callsignFrom == Form1.DatabaseFCCCallsign)
                    {       // 2nd step of a station disconnect
                        callsignFrom = callsignTo;      // switch to the station callsign
                        AddRichText(Packet_Node_Packets, Environment.NewLine + "Station with callsign: " + callsignFrom + " has just disconnected!" + Environment.NewLine, Color.Green);
                        Modeless_MessageBox("Station with callsign: " + callsignFrom + " has just disconnected!", "Disconnect");
                        lock (Form1.Packet_Connects_Disconnects)
                        {
                            Form1.Queue_Packet_Connect_Disconnect new_connect = new Form1.Queue_Packet_Connect_Disconnect();
                            new_connect.ConDis = 'D';
                            new_connect.Reason = response;
                            new_connect.Callsign = callsignFrom;
                            Form1.Packet_Connects_Disconnects.Enqueue(new_connect);
                        }
                        Send_Data_to_Packet_Worker(callsignFrom, response);
                        PipeWriteDictionary.Remove(callsignFrom);
                    }
                    else
                    {       // 1st step of station disconnect
                        // verify the callsign is in the PipeWriteDictionary
                        if (!PipeWriteDictionary.ContainsKey(callsignFrom))
                        {
                            AddRichText(Packet_Node_Packets, Environment.NewLine + "Station with callsign: " + callsignFrom + " attempted to disconnect, but is not in the Dictionary!" + Environment.NewLine, Color.Green);
                            Modeless_MessageBox("Station with callsign: " + callsignFrom + " has attempted to disconnect," + Environment.NewLine + "but is not in the Dictionary!", "Disconnect");
                        }
                        else
                        {       // callsign is in disctionary - ok to move to 2nd step of disconnect
                            Disconnect(callsignFrom);
                        }
                    }
                    break;
                case 'D':   // Connected data received - '68'
                    Send_Data_to_Packet_Worker(TxHed.CallFrom.Substring(0, TxHed.CallFrom.IndexOf('\0')), response);
                    break;
                case 'K':   //  Raw Frame - '75'
                    break;
                case 'k':   // start monitoring using Raw Frames - '95'
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Connect Thread
//        AnonymousPipeServerStream PacketOutData;    // data out from AGWSocket to Packet_StationWorker
        //    string Pipehandle;
        //    do
        //    {
        //        Thread.Sleep(10);
        //        Pipehandle = Form1.PacketInDataHandle;
        //    } while (Pipehandle == null);
        //    PacketInData = new AnonymousPipeClientStream(PipeDirection.In, Pipehandle);
        //    sr_data_from_DBAGW = new StreamReader(PacketInData);
        //
        //// init the Pipe Stream Server to send the Packet data to the AGWSocket
        //  PacketOutData = new AnonymousPipeServerStream(PipeDirection.Out);
        //  Form1.PacketOutDataHandle = PacketOutData.GetClientHandleAsString();
        class ConnectThreadInfo
        {
            public string Callsign { get; set; }
            public string response { get; set; }
        }
        private void New_ConnectThread(object info)
        {
            ConnectThreadInfo threadinfo = (ConnectThreadInfo)info;
            string CallFrom = threadinfo.Callsign.Substring(0, threadinfo.Callsign.IndexOf('\0'));
            string response = threadinfo.response;

            // create a new Pipe for data out to the Packet_StationWorker
            AnonymousPipeServerStream PacketOutData = new AnonymousPipeServerStream(PipeDirection.Out);
            string PacketOutDataHandle = PacketOutData.GetClientHandleAsString();

            // tell Form1, so new Packet StationWorker can be created
            lock (Form1.Packet_Connects_Disconnects)
            {
                Form1.Queue_Packet_Connect_Disconnect new_connect = new Form1.Queue_Packet_Connect_Disconnect();
                if (response == "APRS")
                    new_connect.ConDis = 'A';
                else
                    new_connect.ConDis = 'C';
                new_connect.Reason = response;
                new_connect.Callsign = CallFrom;
                new_connect.PipeHandle = PacketOutDataHandle;
                Form1.Packet_Connects_Disconnects.Enqueue(new_connect);
            }

            // add this new Pipe to the Pipe Collection
            ConnectedStation conn_stat = new ConnectedStation();
            conn_stat.Callsign = CallFrom;
            conn_stat.PacketOutData = PacketOutData;
            ConnectedStations.Add(conn_stat);

            // create a new StreamWriter
            StreamWriter sw_data_to_Packet_StationWorker = new StreamWriter(PacketOutData);

            // add entry to Dictionary
            PipeWriteDictionary.Add(CallFrom, sw_data_to_Packet_StationWorker);     // need to check if it already exists - callsign already connected
        }
        #endregion

        #region Thread to tell the user the AGWPE server has stopped
        private void TellUserStoppedThread(object info)
        {
            // tell the user
            MessageBox.Show("AGWPE has stopped", "AGWPE Stopped", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            //// close any windows using the AGWPE
            //for (int i=0;i<Ports.Num;i++)
            //{
            //    int ch = (int)Ports.Pdata[i].channel;
            //    if (Form1.ChannelData[ch].TNCtype == "AGWPE")
            //        Form1.CloseChannel(ch);
            //}
        }
        #endregion

        public void ChangeButtonText(Button button, string text, Color forecolor)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (button.InvokeRequired)
            {
                ButtonTextdel d = new ButtonTextdel(ChangeButtonText);
                button.Invoke(d, new object[] { button, text, forecolor });
            }
            else
            {
                button.Text = text;
                button.ForeColor = forecolor;
                button.Update();
            }
        }

        public void DB_AGWSocket_Receive_Thread(object client)      // receive data from Worker - to send to Station
        {
            // give the thread a name
            Thread.CurrentThread.Name = "DB AGWSocket Receive thread";
            
            sw_data_to_Worker = new StreamWriter(PacketInData);     // create the pipe for writing to the Packet Worker

            // now just wait for any incoming data
            while (!Receive_Loop_shouldStop)
            {
                if (Form1.Packet_Send_Que.Count > 0)    // any data coming from Packet_StationWorker or APRS_StationWorker?
                {
                    Form1.Packet_Send packet = new Form1.Packet_Send();
                    packet = Form1.Packet_Send_Que.Dequeue();
                    if (packet.APRS)
//                        SendMessageWOid(packet.port, packet.Callsign, packet.data);        // send the data via radio to connected APRS station
                        SendMessageWithID(packet.port, packet.Callsign, packet.data, packet.number);        // send the data via radio to connected APRS station
                    else
                        AGWSend(packet.port, packet.Callsign, packet.data);     // send the data via radio to connected Packet station
                }
                Application.DoEvents();
            }
        }
        #endregion

        public void StartMonitoring(int port)
        {
            // Monitor frames
            HEADER Hed = new HEADER();
            Hed.Port = port;
            Hed.DataKind = (long)('m');
            SendAGWFrame(Hed);
        }

        public void Send_Data_to_Packet_Worker(string Callsign, string data)     // use pipe to send data received from radio to the Packet Worker
        {
            // get the StreamWriter
            StreamWriter sw = PipeWriteDictionary[Callsign];

            // send the data
            try
            {
                sw.AutoFlush = true;
// 8/5/17                sw.Write(data);     // this data must include CR or LF to be received by the Packet worker
                sw.WriteLine(data);     // this data must include CR or LF to be received by the Packet worker
            }
            catch (IOException e)
            {
                Console.WriteLine("AGWSocket to Worker Pipe error: " + e.Message);
            }
        } 

        public void GetPorts(string Str)
        {
            string[] Parts = new string[10];     // more than enough space for 3 ports for the 3 radio channels

            Parts = Str.Split(new char[] {';'});
            Ports.Num = Convert.ToInt16(Parts[0]);
            for (int x=0;x<Ports.Num;x++)
            {
                Ports.Pdata[x] = new Pdata();
                Ports.Pdata[x].PortName = Parts[x + 1];
            }
        }

        //void FormatData(int port, string szData, int size)
        //{
        //    //
        //    //	Format the data, changing it from AGWPE to standard APRS strings
        //    //
        //    //	then transfer it into the output buffer
        //    //
        //    string date = DateTime.Now.ToString(" [MMddyy ");
        //    string[] Parts = szData.Split(new char[] { ' ', '\r' });
        //    string line;
	
        //    // verify port number
        //    if ((port + 1) == Convert.ToInt16(szData.Substring(1, 1)))
        //    {
        //        // port is assigned, transfer the data
        //        line = Parts[2] + ">" + Parts[4];	    // move in Fm and To callsigns
        //        if (Parts[5] == "Via")
        //        {
        //            line += "," + Parts[6];
        //        }

        //        // add in the date
        //        line += date + Parts[10].Substring(2, 5) + "]:";

        //        // add in the rest of the data
        //        line += szData.Substring(szData.IndexOf('\r') + 1);

        //        Form1.NewRcvdPacket = line;
        //        Form1.NewAGWPErawPacket = szData;
        //        Form1.NewAGWPEpacketRcvd = true;

        //        AddRichText(Packet_Node_Packets, line, Color.Black);
        //    }
        //}

        public void InitAGWPE(bool register)
        {
            // first test if it is already connected or trying to
            if (!Connected_to_AGWserver && !InitInProcess)
            {
                // save the Selected request
                RegisterOnInit = register;

                // set the flag
                InitInProcess = true;

                // just start a thread to do the actual AGWPE Socket initialization
                ThreadPool.QueueUserWorkItem(new WaitCallback(InitAGWThread), register);
            }
        }

        private void InitAGWThread(object info)
        {
            // init the Pipe Stream to receive data from the Worker thread
            //  Connect to the AGWPE Server
            //  Get the AGWPE Version Number
            //  Get the Radio ports
            //  Register the database callsign - not for APRS
            //
            // wait for the Form1 to be displayed first

            bool register = (bool)info;

            bool wait = true;
            while (wait)
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is Form1)
                    {
                        wait = false;
                    }
                    Application.DoEvents();
                }
                Application.DoEvents();
            }

            // verify the parameters have been set
            if ((Form1.AGWPEServerName == null) || (Form1.AGWPEServerName == ""))
            {
                MessageBox.Show("Cannot start the AGWPE server because\n\n    the Server Name has not been set!", "Missing AGWPE Server Name", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit early
            }
            if ((Form1.AGWPEServerPort == null) || (Form1.AGWPEServerPort == ""))
            {
                MessageBox.Show("Cannot start the AGWPE server because\n\nthe Server Port number has not been set!", "Missing AGWPE Server Port number", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit early
            }
            if ((Form1.DatabaseFCCCallsign == null) || (Form1.DatabaseFCCCallsign == ""))
            {
                MessageBox.Show("Cannot start the AGWPE server because\n\nthe Database FCC Callsign has not been set!", "Missing AGWPE Database Callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit early
            }

            // set the Connect counter so it can be tracked in the One second timer
//            ConnectCount = 2;       // give it one second to connect

            try
            {
                AddRichText(Packet_Node_Packets, "Attempting to connect to AGWPE server", Color.Green);

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Form1.AGWPEServerName);
// 3/25/16 - does not handle IPv6 addresses                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = ipHostInfo.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);    // handles IPv6 addresses
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Convert.ToInt16(Form1.AGWPEServerPort));

                // Create a TCP/IP socket.
                AGWPEclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IsCreated = true;
                AddRichText(Packet_Node_Packets, Environment.NewLine + "AGWPE server TCP/IP socket successfully created" + Environment.NewLine, Color.Green);

                // Connect to the remote endpoint
                if (remoteEP.ToString() != "127.0.0.1:8000")
                {
                    AddRichText(Packet_Node_Packets, "remoteEP not correct: " + remoteEP.ToString() + Environment.NewLine, Color.Green);
                    return;
                }
                AGWPEclient.Connect(remoteEP);
                Connected_to_AGWserver = true;
                MakeVisible(Initting, false);       // 7/27/17
                MakeVisible(AGWPE_Connected, true); // 7/27/17
                AddRichText(Packet_Node_Packets, "Connected to AGWPE server" + Environment.NewLine, Color.Green);
                OneMinute.Start();      // can now start getting AGWPE Statistics
                Application.DoEvents();

                // get the version number
                Thread.Sleep(10000);         // 8/5/17 - to improve connecting completely
                HEADER Hed = new HEADER();
                Hed.DataKind = (long)('R');
                SendAGWFrame(Hed);

                // build the radio ports table
                Hed.DataKind = (long)('G');
                SendAGWFrame(Hed);

                // Register callsign
                Hed = new HEADER();
                Hed.DataKind = (long)('X');
                Hed.CallFrom = Form1.DatabaseFCCCallsign;
                Call_to_Register = Form1.DatabaseFCCCallsign;
// not needed for APRS                SendAGWFrame(Hed);
                Callsign_Registered = Call_to_Register;     // use this for APRS

                // Monitor frames - only for APRS
                Hed.DataKind = (long)('m');
                SendAGWFrame(Hed);

                // get initial Statistics
                OneMinute_Elapsed(null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ConnectErrorMessage = e.Message;
                DialogResult res = MessageBox.Show("Connection to the AGWPE server failed, with this error message:\n\n" + ConnectErrorMessage + "\n\n              Start the AGWPE server now?", "AGWPE Connection Failed!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == DialogResult.Yes)
                {
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        if (File.Exists(path + "\\AGW Packet Engine.lnk"))
                        {
                            path = path + "\\AGW Packet Engine.lnk";
                        }
                        else
                            if (File.Exists(path + "\\Shortcut to AGW Packet Engine.exe"))
                            {
                                path = path + "\\Shortcut to AGW Packet Engine.exe";
                            }
                            else
                                if (File.Exists(path + "\\Shortcut to AGW Packet Engine.exe.lnk"))
                                {
                                    path = path + "\\Shortcut to AGW Packet Engine.exe.lnk";
                                }
                                else
                                    if (File.Exists(path + "\\AGW Packet Engine.exe"))
                                    {
                                        path = path + "\\AGW Packet Engine.exe";
                                    }
                                    else
                                        if (File.Exists(path + "\\AGW Packet Engine - Shortcut.exe"))
                                        {
                                            path = path + "\\AGW Packet Engine - Shortcut.exe";
                                        }
                                        else
                                            if (File.Exists(path + "\\AGW Packet Engine - Shortcut.lnk"))
                                            {
                                                path = path + "\\AGW Packet Engine - Shortcut.lnk";
                                            }

                        Process.Start(path);
                        Thread.Sleep(10000); // wait 10 secs.
                        InitAGWPE(RegisterOnInit);        // try to connect again
                    }
                    catch
                    {
                        Console.WriteLine("Cannot start the AGWPE exe program");
                        MessageBox.Show("The AGW Packet Engine shortcut is not on the Desktop", "Cannot start application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }

            // clear the flag
            InitInProcess = false;
        }

        //private void RegisterCall()
        //{
        //    HEADER Hed = new HEADER();
        //    //if (Form1.Use_Station_TacticalCallsign)
        //    //{
        //    //    // select the Tactical Callsign
        //    //    Call_to_Register = Form1.StationTacticalCallsign;
        //    //}
        //    //else
        //    //{
        //    //    // select the Station (FCC) Callsign
        //        Call_to_Register = Form1.StationFCCCallsign;
        //    //}

        //    // make sure the requested callsign has been entered
        //    if (Call_to_Register == "")
        //        return;     // quit here, without registering

        //    // has another callsign already been registered?
        //    if (Callsign_Registered != "")
        //    {       // yes - need to unregister it first
        //        // UnRegister the previous callsign
        //        Hed.CallFrom = Callsign_Registered;
        //        Hed.DataKind = (long)('x');
        //        SendAGWFrame(Hed);
        //        Application.DoEvents();
        //    }

        //    // Register the selected callsign
        //    Hed.CallFrom = Call_to_Register;
        //    Hed.DataKind = (long)('X');
        //    SendAGWFrame(Hed);      // the response to this should save the registered callsign
        //    Application.DoEvents();

        //    // wait for it to be registered
        //    while (!Registered)
        //        Application.DoEvents();

        //    // now register the Tactical Callsign, if it is being used
        //    Hed = new HEADER();
        //    if (Form1.Use_Station_TacticalCallsign)
        //    {
        //        // select the Tactical Callsign
        //        Call_to_Register = Form1.StationTacticalCallsign;

        //        // make sure the requested callsign has been entered
        //        if (Call_to_Register == "")
        //            return;     // quit here, without registering

        //        // has another callsign already been registered?
        //        if (Tactical_Callsign_Registered != "")
        //        {       // yes - need to unregister it first
        //            // UnRegister the previous callsign
        //            Hed.CallFrom = Tactical_Callsign_Registered;
        //            Hed.DataKind = (long)('x');
        //            SendAGWFrame(Hed);
        //            Application.DoEvents();
        //        }

        //        // now Register the selected callsign
        //        Hed.CallFrom = Call_to_Register;
        //        Hed.DataKind = (long)('X');
        //        SendAGWFrame(Hed);      // the response to this should save the registered callsign
        //        Application.DoEvents();
        //    }
        //}

        private void UnRegisterCall()
        {
            if (Registered)
            {
                // make sure a callsign has been Registered
                if (Callsign_Registered == "")
                    return;     // quit here, without unregistering

                // UnRegister the callsign
                HEADER Hed = new HEADER();
                Hed.CallFrom = Callsign_Registered;
                Hed.DataKind = (long)('x');
                SendAGWFrame(Hed);
            }

            if (Tactical_Registered)
            {
                // make sure a callsign has been Registered
                if (Tactical_Callsign_Registered == "")
                    return;     // quit here, without unregistering

                // UnRegister the callsign
                HEADER Hed = new HEADER();
                Hed.CallFrom = Tactical_Callsign_Registered;
                Hed.DataKind = (long)('x');
                SendAGWFrame(Hed);
            }
        }

        public void Disconnect(string callsign)
        {
            if (Form1.AGWPERadioPort == -1)
                MessageBox.Show("AGWPE RadioPort = -1\n\nNeed to select a Radio Port");
            else
            {
                HEADER Hed = new HEADER();     // create a new one to zero everything
                byte[] frame = null;
                frame = new byte[SizeOfHeader];     // create the frame
                byte[] bytearray;
                Hed.Port = Form1.AGWPERadioPort;
                frame[0] = (byte)Hed.Port;          // for Big-Endian
                Hed.DataKind = (long)('d');
                frame[4] = (byte)Hed.DataKind;      // for Big-Endian
               Hed.CallFrom = Form1.DatabaseFCCCallsign;
                if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
                {
                    bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
                    System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
                }
                Hed.CallTo = callsign;
                if ((Hed.CallTo != null) && (Hed.CallTo != ""))
                {
                    bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
                    System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
                }
                Hed.DataLen = 0;
                frame[28] = (byte)Hed.DataLen;      // for Big-Endian

                // now send the frame
                Send(AGWPEclient, frame);
                Thread.Sleep(1000);
                Send(AGWPEclient, frame);       // need to send it twice, to make it happen immediately
            }
        }

        private void SendAGWFrame(HEADER Hed)   // prepare the AGW frame and send to AGW
        {
            // The AGW frame format is:
            //      int Port - 4 bytes
            //      int DataKind - 4 bytes
            //      unsigned char CallFrom - 10 bytes, null terminated
            //      unsigned char CallTo - 10 bytes, null terminated
            //      int DataLen - 4 bytes
            //      int USER - 4 bytes (reserved, undefined)
            //      Data would follow this
            //
            byte[] frame = new byte[SizeOfHeader];
            byte[] bytearray;

            frame[0] = (byte)Hed.Port;          // for Big-Endian
            frame[4] = (byte)Hed.DataKind;      // for Big-Endian
            if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
                System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
            }
            if ((Hed.CallTo != null) && (Hed.CallTo != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
                System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
            }
            frame[28] = (byte)Hed.DataLen;      // for Big-Endian
            Send(AGWPEclient, frame);
        }

        private void SendAGWTXFrameVia(TXHEADER Hed)   // no VIA string - 5/20/17
        {
            // The AGW frame format is:
            //      int Port - 4 bytes
            //      int DataKind - 4 bytes
            //      unsigned char CallFrom - 10 bytes, null terminated
            //      unsigned char CallTo - 10 bytes, null terminated
            //      int DataLen - 4 bytes
            //      int USER - 4 bytes (reserved, undefined)
            //      Data would follow this
            //
// 5/20/17            int viaoffset = (ViaCount) * 10;
// 5/20/17            byte[] frame = new byte[SizeOfHeader + Hed.Data.Length + viaoffset + 1];
            byte[] frame = new byte[SizeOfHeader + Hed.Data.Length];
            byte[] bytearray;

            frame[0] = (byte)Hed.Port;          // for Big-Endian
            frame[4] = (byte)Hed.DataKind;      // for Big-Endian
            if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
                System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
            }
            if ((Hed.CallTo != null) && (Hed.CallTo != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
                System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
            }
// 5/18/17            Hed.DataLen = Hed.Data.Length + ViaCount*10 + 1;
            frame[28] = (byte)Hed.DataLen;      // for Big-Endian
            // skip over USER
// 5/20/17            frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
// 5/18/17            System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);
// 5/20/17            System.Buffer.BlockCopy(ViaString, 0, frame, 36, viaoffset);
            bytearray = Encoding.ASCII.GetBytes(Hed.Data);
// 5/18/17            System.Buffer.BlockCopy(bytearray, 0, frame, 37 + viaoffset, Hed.Data.Length);
// 5/20/17            System.Buffer.BlockCopy(bytearray, 0, frame, 36 + viaoffset, Hed.Data.Length);
            System.Buffer.BlockCopy(bytearray, 0, frame, 36, Hed.Data.Length);
            Send(AGWPEclient, frame);
        }

        private void SendAGWTXFrameNotVia(TXHEADER Hed)
        {
            // The AGW frame format is:
            //      int Port - 4 bytes
            //      int DataKind - 4 bytes
            //      unsigned char CallFrom - 10 bytes, null terminated
            //      unsigned char CallTo - 10 bytes, null terminated
            //      int DataLen - 4 bytes
            //      int USER - 4 bytes (reserved, undefined)
            //      Data would follow this
            //
            byte[] frame = new byte[SizeOfHeader + Hed.Data.Length];
            byte[] bytearray;

            frame[0] = (byte)Hed.Port;          // for Big-Endian
            frame[4] = (byte)Hed.DataKind;      // for Big-Endian
            if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
                System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
            }
            if ((Hed.CallTo != null) && (Hed.CallTo != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
                System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
            }
            Hed.DataLen = Hed.Data.Length;
            frame[28] = (byte)Hed.DataLen;      // for Big-Endian
            // skip over USER
            bytearray = Encoding.ASCII.GetBytes(Hed.Data);
            System.Buffer.BlockCopy(bytearray, 0, frame, 36, Hed.Data.Length);
            Send(AGWPEclient, frame);
        }

        public void CloseAGWPE()
        {
            // disconnect from any stations
            for (int i = 0; i < Form1.Stations.Count; i++)
            {
                if (Form1.Stations[i].Active && (Form1.Stations[i].Medium == "Packet"))
                {
                    Disconnect(Form1.Stations[i].Packet_StationWorker.Station_Callsign);
                }
            }

            // tell stations in Pipe Dictionary to Quit
            //foreach (string callsign in PipeWriteDictionary[])
            //{

            //}
            //for (int i = 0; i<PipeWriteDictionary.Count;i++)
            //{
            //    StreamWriter sw = PipeWriteDictionary[i];
            //    try
            //    {
            //        sw.AutoFlush = true;
            //        sw.Write("QUIT!\r");     // this data must include CR or LF to be received by the Packet worker
            //    }
            //    catch (IOException e)
            //    {
            //        Console.WriteLine("AGWSocket to Worker Pipe error: " + e.Message);
            //    }
            //}

            // stop the Receive data loop
            Receive_Loop_shouldStop = true;

            // stop timers
            OneSecond.Stop();
            OneSecond.Close();
            TenSecond.Stop();
            TenSecond.Close();
            TenMinute.Stop();
            TenMinute.Close();

            // is it connected?
            if (!Connected_to_AGWserver)
            {
		        Connected_to_AGWserver = false;

                // UnRegister the callsign
                if (Registered)
                    UnRegisterCall();
            }

            // Has the socket been Created?
            if (IsCreated)
            {
                // Release the socket
                if (AGWPEclient.Connected)
                    AGWPEclient.Shutdown(SocketShutdown.Both);
                AGWPEclient.Close();
        
                IsCreated = false;
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

        public void AGWSend(int port, string To_Callsign, string str)
        {
	        int length;
	        int viacount;

            // verify port is valid
            if (Form1.AGWPERadioPort == -1)
                MessageBox.Show("AGWPE RadioPort = -1\n\nNeed to select a Radio Port");

	        // verify the incoming string length is less than 101 bytes
// 4/3/16 - changed to 200            if (str.Length > 100)
            if (str.Length > 2000)
            {
//                MessageBox.Show("Requested string to send:\n\n" + str + "\n\nis more than 100 bytes", "Invalid string length", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MessageBox.Show("Requested string to send:\n\n" + str + "\n\nis more than 200 bytes", "Invalid string length", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // need to format the string for AGWPE
        	// start by stripping off the callsign and to callsign at the beginning
// only for APRS            str = str.Substring(str.IndexOf(':') + 1);
            length = str.Length;

	        // now prepare the Header for AGWPE
            TXHEADER Hed = new TXHEADER();
	        Hed.Port=port;

	        // decide whether to just send or send with VIA string
            viacount = ViaCount;
	        if (viacount != 0)
	        {	// send with VIA string
                Hed.DataKind = (long)('V');	// Transmit data Unproto Via
                Hed.CallFrom = Form1.DatabaseFCCCallsign;
                Hed.CallTo = To_Callsign;
                viacount *= 10;
                viacount++;
                Hed.Data = ViaString + str;
                Hed.DataLen = length + viacount;
                Hed.Data = str;
                SendAGWTXFrameVia(Hed);
            }
	        else
	        {	// send without VIA string
// 3/24/16                MessageBox.Show("Not sending with Via String!", "Invalid format", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
// don't stop it now                return;
// 3/24/16 - this worked, but it is for Unproto               Hed.DataKind = (long)('M');	// Transmit data Unproto
// 3/24/16 - this did not work                Hed.DataKind = (long)('T');	// Transmit connected data
                Hed.DataKind = (long)('D');	// Transmit connected data
                Hed.CallFrom = Form1.DatabaseFCCCallsign;
                Hed.CallTo = To_Callsign;    // 3/24/16
// 3/24/16                Hed.CallTo = Form1.APRSnetworkName;
                Hed.DataLen = length;
                Hed.Data = str;
                SendAGWTXFrameNotVia(Hed);
            }
//            SendAGWTXFrame(Hed);

            // put in the RTB
            AddRichText(Packet_Node_Packets, "Sent to: " + To_Callsign + ": " + str + Environment.NewLine, Color.Red);
        }

        // Send an APRS message without ID
        public void SendMessageWOid(int port, string TO, string data)
        {
            // just need to change the data format
            string message = ":" + TO.PadRight(9) + ":" + data;
            TXdataUnproto(port, message);
        }

        // Send an APRS message with ID
        public void SendMessageWithID(int port, string TO, string data, int number)
        {
            if (number == 0)        // 8/7/17
                SendMessageWOid(port, TO, data);    // 8/7/17
            else        // 8/7/17
            {           // 8/7/17
                // just need to change the data format
                string message = ":" + TO.PadRight(9) + ":" + data + "{" + number.ToString("D4");   // will use max 4 digits
                TXdataUnproto(port, message);
            }           // 8/7/17
        }

        public void TXdataUnproto(int port, string data)
        {
            // verify the incoming string length is less than 200 bytes 
            if (data.Length > 200) 
            { 
                MessageBox.Show("Requested string to send:\n\n" + data + "\n\nis more than 200 bytes", "Invalid string length", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                return; 
            } 
 
            // now prepare the Header for AGWPE 
            TXHEADER Hed = new TXHEADER(); 
            Hed.Port = port; 
            Hed.DataKind = (long)('M'); // Transmit data unproto 
//            Hed.CallFrom = Form1.StationFCCCallsign; 
            Hed.CallFrom = Callsign_Registered; 
//            Hed.CallTo = TO;
            Hed.CallTo = Form1.APRSnetworkName;
            Hed.DataLen = data.Length; 
            Hed.Data = data; 
//            SendAGWTXFrame(Hed);
            SendAGWTXFrameNotVia(Hed); 
 
            // put in the RTB 
            AddRichText(Packet_Node_Packets, data + Environment.NewLine, Color.Red);
        }

        public static void Modeless_MessageBox(string message, string title)
        {       // only puts in the OK button
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

        public bool Prep_APRS_File(string path, bool suppress_error_msg, int expected_number_of_lines)
        {
            StreamReader reader;

            // open the file
            // do this only if the FileName is not empty
            if (path != "")
            {
                try
                {
                    reader = File.OpenText(path);
                }
                catch
                {
                    if (!suppress_error_msg)
                        MessageBox.Show("Selected file:\n\n" + path + "\n\nis not accessible!             If it has not been loaded yet, then all is OK.", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                string line;
                string[] Parts;
                //                string[] FileLines;
                string[] FileLines = new string[expected_number_of_lines];
                int longestLine = 0;
                int lineno = 0;
                string[] splitter = new string[] { ": " };

                // read each item, extracting the information
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    if (!Parts[0].StartsWith("*"))
                    {
                        FileLines[lineno] = line;
                        lineno++;
                        if (line.Length > longestLine)
                            longestLine = line.Length;
                    }
                }

                // close the file
                reader.Close();
            }
            return true;
        }


        #region Socket Functions
        // below is the MSDN Asynchronous Client example

        // State object for receiving data from remote device.
        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        // ManualResetEvent instances signal completion.
        //private static ManualResetEvent connectDone =
        //    new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static String response = String.Empty;
        private static byte[] responsebytes = new byte[StateObject.BufferSize];
        private static byte[] sentbytes = new byte[256];

        private void Send(Socket client, byte[] data)
        {
            // Begin sending the data to the remote device.
            try
            {
                client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), client);
                sentbytes = data;
            }
            catch
            {
                Connected_to_AGWserver = false;
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to DB AGWPE server.", bytesSent);
//                Console.WriteLine("Sent {0} bytes to DB AGWPE server: {1}", bytesSent, Encoding.UTF8.GetString(sentbytes));

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }

}
