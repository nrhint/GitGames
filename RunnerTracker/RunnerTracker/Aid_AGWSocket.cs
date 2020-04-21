﻿using System;
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
    public class Aid_AGWSocket
    {
        //
        // Updates:
        //  August 5, 2017 - added time delay in InitAGWThread to improve getting connected completely
        //  8/7/17 - changed SendMessageWithID so that if number = 0, send to SendMessageWOID
        //           added test for File Sending Bulletins being received
        //  8/11/17 - added test for APRS Alerts and Messages to all
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

        #region Variables and declarations
        private bool APRS_mode;
        public Button Connect_Button { get; set; }
        public Button Disconnect_Button { get; set; }
        enum PacketTypes { MM = 0, MI = 2, MU = 4, MS = 8, MC = 16, MTX = 32, MRJ = 64, MVIA = 1 }
        public bool Connected_to_AGWserver;
        public bool InitInProcess;
        public bool Registered;
        public bool Tactical_Registered;
        public bool ConnectInProcess;
        public bool Connected_to_Database;
        public bool Cannot_Connect_to_DB;
        public bool Monitoring;
        private bool RegisterOnInit;
        public string Version;
        private byte[] ViaString = new byte[3*10];  // space for 3 callsigns, 10 bytes each
        private int ViaCount = 0;
        private Int32 VerNum;
        private bool HeartBeat;
        private int DB_Connect_Count = 0;
        bool IsCreated, PortsFound;
        int MonitorKind;
        private string Callsign_Registered;
        private string Tactical_Callsign_Registered;
        private string Call_to_Register;
        public string Connected_to_Callsign = string.Empty;
        public string APRS_DB_Callsign = string.Empty;
        public PORTS Ports = new PORTS();
        Socket AGWPEclient;
        bool InUse;
        bool BeaconTimeElapsed = false;
        System.Timers.Timer TenMinute;
        System.Timers.Timer OneSecond;
// 5/1/17        System.Timers.Timer TenSecond;
        System.Timers.Timer ThirtySecond;
        System.Timers.Timer OneMinute;
        System.Timers.Timer TwoMinute;
        string ConnectErrorMessage = "Cannot connect to the AGWPE server!";
        public RichTextBox Packet_Node_Packets { get; set; }
        public RichTextBox APRS_Received_Packets { get; set; }
        public RichTextBox APRS_Sent_Packets { get; set; }
        delegate void ButtonTextdel(Button butn, string text, Color forecolor);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
//        AnonymousPipeServerStream PacketInData;      // data from AGWSocket to Worker
//        AnonymousPipeClientStream PacketOutData;     // data from Worker to AGWSocket
        public string Receive_Buffer = string.Empty;
        public bool Receive_DataAvailable = false;

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
            public byte[] Header = new byte[SizeOfHeader];
            public byte[] DataBuff = new byte[500];         // can probably reduce to about 256
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
        bool PortStatReceived;
        #endregion

        public Aid_AGWSocket(RichTextBox rtb, bool APRSmode)
        {
            Packet_Node_Packets = rtb;
            APRS_mode = APRSmode;
            Callsign_Registered = string.Empty;
	        Connected_to_AGWserver = false;
            InitInProcess = false;
            Monitoring = false;
	        IsCreated = false;
            HeartBeat = false;
            Registered = false;
            Tactical_Registered = false;
            Connected_to_Database = false;
            Cannot_Connect_to_DB = false;
            ConnectInProcess = false;
	        MonitorKind = 62;
            PortsFound = false;
            InUse = false;
            VerNum = 0;

            // init the Pipe Stream Server to send the Packet data to the Worker thread
//            PacketInData = new AnonymousPipeServerStream(PipeDirection.Out);
//            Form1.PacketInDataHandle = PacketInData.GetClientHandleAsString();

            // initialize and start the One Second timer
            OneSecond = new System.Timers.Timer();
            OneSecond.AutoReset = true;
            OneSecond.Interval = 1000;     // 1 sec.
            OneSecond.Elapsed += new ElapsedEventHandler(OneSecond_Elapsed);
            OneSecond.Start();

// 5/1/17           // initialize the Ten Second timer - but do not start here
//            TenSecond = new System.Timers.Timer();
//            TenSecond.AutoReset = true;
//            TenSecond.Interval = 10000;     // 10 sec.
//            TenSecond.Elapsed += new ElapsedEventHandler(TenSecond_Elapsed);
//// 4/7/16 - will not use the HeartBeat function to test AGWPE            TenSecond.Start();      // ??????? do not start???

            // initialize the 30 Second timer - but do not start here 
            ThirtySecond = new System.Timers.Timer(); 
            ThirtySecond.AutoReset = true; 
            ThirtySecond.Interval = 30000;     // 30 sec. 
            ThirtySecond.Elapsed += new ElapsedEventHandler(ThirtySecond_Elapsed); 

            // initialize the One Minute timer - but start after creating the AGWSocket client
            OneMinute = new System.Timers.Timer();
            OneMinute.AutoReset = true;
            OneMinute.Interval = 60000;     // 60 sec. = 1 minute
            OneMinute.Elapsed += new ElapsedEventHandler(OneMinute_Elapsed);
 
            // initialize the 2 Minute timer - but start after Query sent to Central Database
            TwoMinute = new System.Timers.Timer(); 
            TwoMinute.AutoReset = true; 
            TwoMinute.Interval = 120000;     // 120 sec. = 2 minutes 
            TwoMinute.Elapsed += new ElapsedEventHandler(TwoMinute_Elapsed); 

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

// 5/1/17        //#region Ten Second timer
        //void TenSecond_Elapsed(object source, ElapsedEventArgs e)
        //{
        //    //
        //    //	this timer function has only one purpose:
        //    //      1. a Heart Beat to test the AGWPE connection
        //    //

        //    // test for the Heart Beat only when the connection has been made and the client is still creaated (not shutting down)
        //    if (Connected_to_AGWserver && IsCreated)
        //    {
        //        // test if HeartBeat is still set, if so, AGWPE has stopped
        //        if (HeartBeat)
        //        {
        //            ThreadPool.QueueUserWorkItem(new WaitCallback(TellUserStoppedThread));
        //            Connected_to_AGWserver = false;
        //        }
        //        else
        //        {
        //            // has the Version number been set yet?
        //            if (VerNum != 0)
        //                HeartBeat = true;               // Yes - set the flag so a new version number will be tested
        //            HEADER Hed = new HEADER();      // send the Version request
        //            Hed.DataKind = (long)('R');
        //            SendAGWFrame(Hed);
        //            if (!Connected_to_AGWserver)
        //            {
        //                ThreadPool.QueueUserWorkItem(new WaitCallback(TellUserStoppedThread));
        //            }
        //        }
        //    }
        //}
        //#endregion

        #region Thirty Second timer 
        void ThirtySecond_Elapsed(object source, ElapsedEventArgs e) 
        { 
            // 
            //	this timer event happens because the Central Database did not respond to the Query 
            //      wait 2 minutes and try again 
            // 
            TwoMinute.Start(); 
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
        
        #region Two Minute timer 
        void TwoMinute_Elapsed(object source, ElapsedEventArgs e) 
        { 
            // 
            //	this timer event happens because we have been waiting until sending another Central Database Query 
            //      send the Query and start the 30 second timer 
            // 
//            SendDBQuery(); 
            ThirtySecond.Start(); 
        } 
        #endregion 

        #region Ten Minute timer
        void TenMinute_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function has only one purpose:
            //      1. set the Beacon time elapsed flag
            //
// New:      When connect, send BTEXT and start 10 min timer
//       When timer fires, send BTEXT

            BeaconTimeElapsed = true;
        }
        #endregion
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

        #region Thread to Process a received Packet
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
                    // only for APRS, not Packet
                    // watch only for packets sent to the desired APRS Network
                    if (TxHed.CallTo.Substring(0,TxHed.CallTo.IndexOf('\0')) == Form1.APRSnetworkName)
                    {
                        if (response.Contains("zRT Central Database"))
                        {
                            if (Connected_to_Database)
                            {
                            }
                            else
                            {
                                Connected_to_Database = true;
                                APRS_DB_Callsign = TxHed.CallFrom;
                            }
                        }
                        else
                        {
                            string resp2 = response.Substring(response.IndexOf("]\r") + 2);     // remove the header
                            if (resp2.StartsWith(":" + Form1.StationFCCCallsign.PadRight(9) + ":"))     // test if it is a message to this station
                            {
                                // first remove CR NULL from end of it
// the data must include a CR, LF                                resp2 = resp2.Replace("\r", "");
//                                resp2 = resp2.Replace("\r", "");
                                resp2 = resp2.Replace("\r\r","\r");
                                resp2 = resp2.Replace("\0", "");

                                // now pass it on to the Aid_Worker
                                lock (Receive_Buffer)
                                {
                                    Receive_Buffer = resp2.Substring(11);
                                    Receive_DataAvailable = true;
                                }
                            }
                            else       // 8/7/17
                            {       // now test for File Sending Bulletins - added 8/7/17
// 8/11/17                                if (resp2.StartsWith(":F"))
                                if (resp2.StartsWith(":F") || resp2.StartsWith(":Ale") || resp2.StartsWith(":M"))     // 8/11/17 - added test for Alerts and Messages to all
                                {
                                    // first remove CR NULL from end of it
                                    resp2 = resp2.Replace("\r\r", "\r");
                                    resp2 = resp2.Replace("\0", "");

                                    // now pass it on to the Aid_Worker
                                    lock (Receive_Buffer)
                                    {
// 8/8/17                                        Receive_Buffer = resp2.Substring(11);
                                        Receive_Buffer = resp2;     // 8/8/17
                                        Receive_DataAvailable = true;
                                    }
                                }
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
                case 'g':   // Port Information - '6D'
                    Form1.AGWPEPortStatistics.BaudRate = (byte)response[0];
                    Form1.AGWPEPortStatistics.TrafficLevel = (byte)response[1];
                    Form1.AGWPEPortStatistics.TxDelay = (byte)response[2];
                    Form1.AGWPEPortStatistics.TxTail = (byte)response[3];
                    Form1.AGWPEPortStatistics.Persist = (byte)response[4];
                    Form1.AGWPEPortStatistics.SlotTime = (byte)response[5];
                    Form1.AGWPEPortStatistics.MaxFrame = (byte)response[6];
                    Form1.AGWPEPortStatistics.NumConnections = (byte)response[7];
                    Form1.AGWPEPortStatistics.NumBytesReceived = response[8];
                    PortStatReceived = true;
                    break;
                case 'y':   // Outstanding frames waiting on Port - '79'
                    Form1.AGWPEPortStatistics.NumPendingPortFrames = response[0];
                    break;
                case 'Y':   // Outstanding frames waiting on Connection - '59'
                    Form1.AGWPEPortStatistics.NumPendingConnectionFrames = response[0];
                    break;
                case 'X':   // response to Register a callsign - '88'
                    if ((TxHed.DataLen == 1) && ((byte)response[0] == 1))
                    {
                        if (Call_to_Register == Form1.StationFCCCallsign)
                        {
                            Registered = true;
                            Callsign_Registered = Call_to_Register;
                        }
                        else
                        {
                            Tactical_Registered = true;
                            Tactical_Callsign_Registered = Call_to_Register;
                        }
                    }
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
                    ConnectInProcess = false;
                    if (TxHed.DataLen == 1)
                    {
                        AddRichText(Packet_Node_Packets, "Connected to Central Database" + Environment.NewLine, Color.Green);
                        Form1.AddtoLogFile("Connected to Central Database");
                        Connected_to_Callsign = TxHed.CallFrom;
                        Connected_to_Database = true;
                    }
                    else
                        Connected_to_Database = false;
                    break;
                case 'C':   // Connected - '67'
                    ConnectInProcess = false;
                    if (TxHed.CallFrom != "")
                    {
                        AddRichText(Packet_Node_Packets, "Connected to Central Database" + Environment.NewLine, Color.Green);
                        Form1.AddtoLogFile("Connected to Central Database");
                        Connected_to_Callsign = TxHed.CallFrom;
                        Connected_to_Database = true;
                    }
                    else
                        Connected_to_Database = false;
                    break;
                case 'd':   // - '100'
                    // look at CallFrom and response
                    // possible responses: "*** DISCONNECTED RETRYOUT"
                    //                      "*** DISCONNECTED From"
                    if ((response.StartsWith("*** DISCONNECTED RETRYOUT")) || (response.StartsWith("*** DISCONNECTED From")))
                    {
                        Connected_to_Database = false;
                        ConnectInProcess = false;

                        // moved from timer
                        AddRichText(Packet_Node_Packets, "Failed to connect to Central Database!" + Environment.NewLine, Color.Green);
                        Form1.AddtoLogFile("Failed to connect to Central Database!");
                        Modeless_MessageBox("Did not connect to Central Database", "Not connected");
                        ChangeButtonText(Connect_Button, "Connect", Color.Black);
                        Cannot_Connect_to_DB = true;
//                        ConnectInProcess = false;
                    }
                    break;
                case 'D':   // Connected data received - '68'
                    // only for Packet, not APRS
                    lock (Receive_Buffer)
                        {
                            Receive_Buffer += response;
                            Receive_DataAvailable = true;
                        }
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
            if (!button.IsDisposed)
            {
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

        public void GetPorts(string Str)
        {
            string[] Parts = new string[10];     // plenty of space for 3 ports for the 3 radio channels

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
        //    string date = DateTime.Now.ToString(" [MM/dd/yy ");
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
            //  Register the station callsign - not for APRS
            //  Try to connect to the Database
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
//// 5/20/17 - already tested in the button handler            if ((Form1.StationFCCCallsign == null) || (Form1.StationFCCCallsign == ""))
//            {
//                MessageBox.Show("Cannot start the AGWPE server because\n\nthe Station FCC Callsign has not been set!", "Missing AGWPE Station Callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//                return;     // quit early
//            }

            // connect to the server
            try
            {
//                AddRichText(Packet_Node_Packets, "Attempting to connect to AGWPE server" + Environment.NewLine, Color.Green);
                AddRichText(Packet_Node_Packets, "Attempting to connect to AGWPE server", Color.Green);

                //                // set the Connect counter so it can be tracked in the One second timer
                //                ConnectCount = 2;       // give it one second to connect

                // Establish the remote endpoint for the socket.
                //                IPHostEntry ipHostInfo = Dns.Resolve(Form1.AGWPEServerName);
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
                AddRichText(Packet_Node_Packets, "Connected to AGWPE server" + Environment.NewLine, Color.Green);
                Form1.AddtoLogFile("Connected to AGWPE server");
                OneMinute.Start();      // can now start getting AGWPE Statistics
                Application.DoEvents();

                // get the version number
                Thread.Sleep(10000);         // 8/5/17 - to improve connecting completely
                HEADER Hed = new HEADER();
                Hed.DataKind = (long)('R');
                SendAGWFrame(Hed);
                Application.DoEvents();

                // build the radio ports table
                Hed.DataKind = (long)('G');
                SendAGWFrame(Hed);
                Application.DoEvents();

                // register the callsign if we need to connect to other stations
                if (register)
                {
                    //if (Form1.Use_TacticalCallsign)
                    //{
                    //    // set the Beacon text first

                    //    // select the Tactical Callsign
                    //    Hed.CallFrom = Form1.TacticalCallsign;
                    //}
                    //else
                    //{
                    //    // select the Station (FCC) Callsign
                    //    Hed.CallFrom = Form1.StationCallsign;
                    //}

                    //// Register the selected callsign
                    //Hed.DataKind = (long)('X');
                    ////                        Hed.CallFrom = Form1.StationCallsign;
                    //SendAGWFrame(Hed);
                    //Application.DoEvents();
                    RegisterCalls();
                }

                // Monitor frames - if APRS is needed
                Hed.DataKind = (long)('m');
                SendAGWFrame(Hed);

                // get the initial Statistics
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
                                            else
                                                if (File.Exists(path + "\\AGW Packet Engine.exe - Shortcut"))
                                                {
                                                    path = path + "\\AGW Packet Engine.exe - Shortcut";
                                                }


                        Process.Start(path);
                        Thread.Sleep(10000); // wait 10 secs.
                        InitAGWPE(RegisterOnInit);        // try to connect again
                    }
                    catch (Exception enew)
                    {
                        Console.WriteLine("Cannot start the AGWPE exe program, with this message: " + enew.Message);
                        MessageBox.Show("The AGW Packet Engine shortcut is not on the Desktop" , "Cannot start application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }

            // clear the flag
            InitInProcess = false;
        }

        private void RegisterCalls()
        {
            HEADER Hed = new HEADER();
            Call_to_Register = Form1.StationFCCCallsign;

            // make sure the requested callsign has been entered
            if (Call_to_Register == "")
                return;     // quit here, without registering

            // has another callsign already been registered?
            if (Callsign_Registered != "")
            {       // yes - need to unregister it first
                // UnRegister the previous callsign
                Hed.CallFrom = Callsign_Registered;
                Hed.DataKind = (long)('x');
                SendAGWFrame(Hed);
                Application.DoEvents();
            }

            // now Register the selected callsign
            Hed.CallFrom = Call_to_Register;
            Hed.DataKind = (long)('X');
            SendAGWFrame(Hed);      // the response to this should save the registered callsign
            Application.DoEvents();

            // wait until it gets registered
                while (!Registered)
                    Application.DoEvents();

            // now register the Tactical Callsign, if it is being used
            Hed = new HEADER();
            if (Form1.Use_Station_TacticalCallsign)
            {
                // select the Tactical Callsign
                Call_to_Register = Form1.StationTacticalCallsign;

                // make sure the requested callsign has been entered
                if (Call_to_Register == "")
                    return;     // quit here, without registering

                // has another callsign already been registered?
                if (Tactical_Callsign_Registered != "")
                {       // yes - need to unregister it first
                    // UnRegister the previous callsign
                    Hed.CallFrom = Tactical_Callsign_Registered;
                    Hed.DataKind = (long)('x');
                    SendAGWFrame(Hed);
                    Application.DoEvents();
                }

                // now Register the selected callsign
                Hed.CallFrom = Call_to_Register;
                Hed.DataKind = (long)('X');
                SendAGWFrame(Hed);      // the response to this should save the registered callsign
                Application.DoEvents();
            }
        }

        private void UnRegisterCalls()
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

        public void Connect_to_Database()
        {
            // if this request is made while we are already in the process of
            // trying to connect, then requesting it again will cause it to cancel the connect request
            if (ConnectInProcess)
            {       // already trying to Connect - Cancel it
                //HEADER Hed = new HEADER();     // create a new one to zero everything
                //byte[] frame = null;
                //frame = new byte[SizeOfHeader];     // create the frame
                //byte[] bytearray;
                //if (Form1.AGWPERadioPort == -1)
                //    MessageBox.Show("AGWPE RadioPort = -1");
                //Hed.Port = Form1.AGWPERadioPort;
                //frame[0] = (byte)Hed.Port;          // for Big-Endian
                //Hed.DataKind = (long)('d');
                //frame[4] = (byte)Hed.DataKind;      // for Big-Endian
                //Hed.CallFrom = Form1.StationCallsign;
                //if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
                //{
                //    bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
                //    System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
                //}
                //Hed.CallTo = Form1.DatabaseCallsign;
                //if ((Hed.CallTo != null) && (Hed.CallTo != ""))
                //{
                //    bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
                //    System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
                //}
                //Hed.DataLen = 0;
                //frame[28] = (byte)Hed.DataLen;      // for Big-Endian

                //// now send the frame
                //Send(AGWPEclient, frame);
                //Thread.Sleep(1000);
                //Send(AGWPEclient, frame);       // need to send it twice, to make it happen immediately

                Disconnect();
            }
            else
            {
                if (Form1.AGWPERadioPort == -1)
                    MessageBox.Show("AGWPE RadioPort = -1\n\nNeed to select a Radio Port");
                else
                {
                    ChangeButtonText(Connect_Button, "Trying to\nConnect", Color.Red);
                    AddRichText(Packet_Node_Packets, "Attempting to connect to Central Database" + Environment.NewLine, Color.Green);
                    ConnectInProcess = true;
                    byte[] frame = null;
                    frame = new byte[SizeOfHeader];     // create the frame

                    if (Form1.Connection_Type == Form1.Connect_Medium.Packet)
                    {
                        HEADER Hed = new HEADER();     // create a new one to zero everything
                        byte[] bytearray;
                        Hed.Port = Form1.AGWPERadioPort;
                        Hed.CallFrom = Form1.StationFCCCallsign;
                        Hed.CallTo = Form1.DatabaseFCCCallsign;

                        // decide if it needs to be Connect or Connect Via
                        if (Form1.Packet_Connect_Mode != Form1.Packet_Connect_Method.ViaString)
                            //if (ViaCount == 0)
                        {       // just cconnect
                            Hed.DataKind = (long)('C');
                            Hed.DataLen = 0;
                        }
                        else
                        {       // must do Connect Via
                            // first prep the ViaString
                            InitViaString();

                            // now add to the frame
                            Hed.DataKind = (long)('v');
                            int viaoffset = (ViaCount) * 10;
                            frame = new byte[SizeOfHeader + viaoffset + 1];     // create the frame
                            Hed.DataLen = ViaCount * 10 + 1;
                            frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
                            System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);    // now copy in the Via string
                        }

                        // put in the common items
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
                    }
                    if (Form1.Connection_Type == Form1.Connect_Medium.APRS)
                    {
                        HEADER Hed = new HEADER();     // create a new one to zero everything
                        byte[] bytearray;
                        if (Form1.AGWPERadioPort == -1)
                            MessageBox.Show("AGWPE RadioPort = -1");
                        Hed.Port = Form1.AGWPERadioPort;
                        Hed.CallFrom = Form1.StationFCCCallsign;
                        Hed.CallTo = Form1.DatabaseFCCCallsign;

                        // decide if it needs to be Connect or Connect Via
                        if (ViaCount == 0)
                        {       // just cconnect
                            Hed.DataKind = (long)('C');
                            Hed.DataLen = 0;
                        }
                        else
                        {       // must do Connect Via
                            Hed.DataKind = (long)('v');
                            int viaoffset = (ViaCount) * 10;
                            frame = new byte[SizeOfHeader + viaoffset + 1];     // create the frame
                            Hed.DataLen = ViaCount * 10 + 1;
                            frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
                            System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);    // now copy in the Via string
                        }

                        // put in the common items
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
                    }

                    // now send the frame
                    Send(AGWPEclient, frame);

                    // start a timer so we can tell user if it does not connect
                    DB_Connect_Count = 30;
                }
            }
        }

        public void Disconnect()
        {
            // To Disconnect, it will actually be a several step process:
            //      1. Verify there is a connection by using the 'g' request
            //      2. Attempt to Disconnect
            //      3. Use the 'g' request again to verify that the connection has gone
            //

            // display in rtb
            AddRichText(Packet_Node_Packets, "Disconnecting from Central Database" + Environment.NewLine, Color.Green);

            // issue the 'g' request
            HEADER Hed;
            IssueSmallG();

            // if there is a connection, continue, otherwise, quit
            if (Form1.AGWPEPortStatistics.NumConnections < 1)
            {
                Connected_to_Database = false;
                return;
            }

            // issue the Disconnect request
            if (Form1.AGWPERadioPort == -1)
                MessageBox.Show("AGWPE RadioPort = -1\n\nNeed to select a Radio Port");
            else
            {
                Hed = new HEADER();     // create a new one to zero everything
                byte[] frame = null;
                frame = new byte[SizeOfHeader];     // create the frame
                byte[] bytearray;
                Hed.Port = Form1.AGWPERadioPort;
                frame[0] = (byte)Hed.Port;          // for Big-Endian
                Hed.DataKind = (long)('d');
                frame[4] = (byte)Hed.DataKind;      // for Big-Endian
                Hed.CallFrom = Form1.StationFCCCallsign;
                if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
                {
                    bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
                    System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
                }
                Hed.CallTo = Form1.DatabaseFCCCallsign;
                if ((Hed.CallTo != null) && (Hed.CallTo != ""))
                {
                    bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
                    System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
                }
                Hed.DataLen = 0;
                frame[28] = (byte)Hed.DataLen;      // for Big-Endian

                // now send the frame
                Send(AGWPEclient, frame);
            }

            // wait for the Disconnect response
            while (Connected_to_Database)
                Application.DoEvents();

            // issue the 'g' request again
            IssueSmallG();

            if (Form1.AGWPEPortStatistics.NumConnections == 0)
                return;
            else
            {
                Console.WriteLine("Still have more connections to Disconnect!");
                Disconnect();   // call itself again to remove extra connections
            }
        }

        void IssueSmallG()
        {
            PortStatReceived = false;
            HEADER Hed = new HEADER();  // zero everything
            Hed.Port = Form1.AGWPERadioPort;
            Hed.DataKind = (long)('g');
            SendAGWFrame(Hed);
            while (!PortStatReceived)
                Application.DoEvents();     // just wait for the 'g' request to finish
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

        public void SendAGWTXFrameVia(TXHEADER Hed)
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
            int viaoffset = (ViaCount) * 10;
            byte[] frame = new byte[SizeOfHeader + Hed.Data.Length + viaoffset + 1];
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
            Hed.DataLen = Hed.Data.Length + ViaCount*10 + 1;
            frame[28] = (byte)Hed.DataLen;      // for Big-Endian
            // skip over USER
            frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
            System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);
            if ((Hed.Data != null) && (Hed.Data != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.Data);
                System.Buffer.BlockCopy(bytearray, 0, frame, 37 + viaoffset, Hed.Data.Length);
            }
            Send(AGWPEclient, frame);
        }

        public void SendAGWTXFrameNotVia(TXHEADER Hed)
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
//            int viaoffset = (ViaCount) * 10;
//            byte[] frame = new byte[SizeOfHeader + Hed.Data.Length + viaoffset + 1];
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
//            Hed.DataLen = Hed.Data.Length + ViaCount * 10 + 1;
            Hed.DataLen = Hed.Data.Length;
            frame[28] = (byte)Hed.DataLen;      // for Big-Endian
            // skip over USER
//            frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
//            System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);
            if ((Hed.Data != null) && (Hed.Data != ""))
            {
                bytearray = Encoding.ASCII.GetBytes(Hed.Data);
//                System.Buffer.BlockCopy(bytearray, 0, frame, 37 + viaoffset, Hed.Data.Length);
                System.Buffer.BlockCopy(bytearray, 0, frame, 36, Hed.Data.Length);
            }
            Send(AGWPEclient, frame);
        }

        public void CloseAGWPE()
        {
            // are we connected to Central Database?
            if (Connected_to_Database)
                Disconnect();
            while (Connected_to_Database)
                Application.DoEvents();       // wait until we are disconnected

            // stop timers
            OneSecond.Stop();
            OneSecond.Close();
// 5/1/17            TenSecond.Stop();
// 5/1/17            TenSecond.Close();
            TenMinute.Stop();
            TenMinute.Close();

            // is it connected?
            if (Connected_to_AGWserver)
            {
		        Connected_to_AGWserver = false;

                // UnRegister the callsign
                if (Registered)
                    UnRegisterCalls();
            }

            // Has the socket been Created?
            if (IsCreated)
            {
                // Release the socket.
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

        public int InitViaString()
        {
            //
            // Init the internal VIA string
            //
            // Return number of stations in VIA string
            //
//            int viacount;
            int viacount = 0;
            ViaString = new byte[3 * 10];   // clear the array
            string[] Parts = new string[4];     // leave space for 4, but should only get 3 maximum

	        // prepare the VIA string
	        if ((Form1.VIAstring != null) && (Form1.VIAstring.Length != 0))
	        {
                // get another copy to use with strtok
                Parts = Form1.VIAstring.Split(new char[] { ',' });

                viacount = Parts.Length;
                if (viacount > 3)
                {
                    //			exit (7998);	// too many callsigns (>3) for VIA string
                    MessageBox.Show("Too many VIA strings", "Invalid Network Unproto string", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return 0;
                }
                ViaCount = viacount;

                int i = 0;
                byte[] bytearray;
                foreach (string str in Parts)
                {
                    bytearray = Encoding.ASCII.GetBytes(str);
                    System.Buffer.BlockCopy(bytearray, 0, ViaString, i*10, str.Length);
                    i++;
                }
	        }
            //else
            //{
            //    viacount=0;
            //    ViaString = new byte[3 * 10];   // clear the array
            //}
	        return viacount;
        }
        
        public void AGWSend(int port, string str)
        {
	        int length;
//	        int viacount;

	        // verify the incoming string length is less than 101 bytes
// 4/3/16 - changed to 200            if (str.Length > 100)
            if (str.Length > 200)
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

//            // decide whether to just send or send with VIA string
//            viacount = ViaCount;
//            if (Form1.Packet_Connect_Mode == Form1.Packet_Connect_Method.ViaString)
////	        if (viacount != 0)
//            {	// send with VIA string
//                Hed.DataKind = (long)('V');	// Transmit data Unproto Via
//                Hed.CallFrom = Form1.StationCallsign;
////                Hed.CallTo = Form1.APRSnetworkName;
//                Hed.CallTo = Form1.DatabaseCallsign;    // 3/24/16
//                viacount *= 10;
//                viacount++;
//                Hed.Data = ViaString + str;
//                Hed.DataLen = length + viacount;
////                Hed.Data = str;
//                SendAGWTXFrameVIA(Hed);
//            }
//            else
//            {	// send without VIA string
                Hed.DataKind = (long)('D');	// Transmit connected data
                Hed.CallFrom = Form1.StationFCCCallsign;
                Hed.CallTo = Form1.DatabaseFCCCallsign;    // 3/24/16
                Hed.DataLen = length;
                Hed.Data = str;
                SendAGWTXFrameNotVia(Hed);
            //}

            // put in the RTB
            AddRichText(Packet_Node_Packets, str, Color.Red);
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
            // remove CR, LF from end of data, if exists
            string data2 = data.Replace("\r", "");
            data2 = data2.Replace("\n", "");
            if (number == 0)        // 8/7/17
                SendMessageWOid(port, TO, data2);   // 8/7/17
            else      // 8/7/17
            {
                //            string message = ":" + TO.PadRight(9) + ":" + data + "{" + number.ToString("D4");   // will use max 4 digits
                string message = ":" + TO.PadRight(9) + ":" + data2 + "{" + number.ToString("D4");   // will use max 4 digits
                TXdataUnproto(port, message);
            }       // 8/7/17
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
            Hed.CallFrom = Form1.StationFCCCallsign;
//            Hed.CallTo = TO;
            Hed.CallTo = Form1.APRSnetworkName;
            Hed.DataLen = data.Length;
            Hed.Data = data;
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
// remove for testing                FormatSentData(data);
            }
//            catch (Exception e)
            catch
            {
                Connected_to_AGWserver = false;
            }
        }

        void FormatSentData(byte[] frame)
        {
            //
            //	Format the data, changing it from AGWPE to standard APRS strings
            //
//                        frame = new byte[SizeOfHeader];     // create the frame
                    //    Hed.DataLen = 0;
                    //}
                    //else
                    //{       // must do Connect Via
                    //    Hed.DataKind = (long)('v');
                    //    int viaoffset = (ViaCount) * 10;
                    //    frame = new byte[SizeOfHeader + viaoffset + 1];     // create the frame
                    //    Hed.DataLen = ViaCount * 10 + 1;
                    //    frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
                    //    System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);    // now copy in the Via string
                    //}

//                    // put in the common items
//                    frame[0] = (byte)Hed.Port;          // for Big-Endian
//                    frame[4] = (byte)Hed.DataKind;      // for Big-Endian
//                    if ((Hed.CallFrom != null) && (Hed.CallFrom != ""))
//                    {
//                        bytearray = Encoding.ASCII.GetBytes(Hed.CallFrom);
//                        System.Buffer.BlockCopy(bytearray, 0, frame, 8, Hed.CallFrom.Length);
//                    }
//                    if ((Hed.CallTo != null) && (Hed.CallTo != ""))
//                    {
//                        bytearray = Encoding.ASCII.GetBytes(Hed.CallTo);
//                        System.Buffer.BlockCopy(bytearray, 0, frame, 18, Hed.CallTo.Length);
//                    }

            // do only if not the heartbeat 'R' frame and not the Get Ports 'G' frame
            if ((frame[4] != 'R') && (frame[4] != 'G') && (frame[4] != 'X') && (frame[4] != 'm') && (frame[4] != 'C'))
            {
                char[] EndTrim = { '\0' };
                string line = Encoding.UTF8.GetString(frame, 8, 10);     // Call From
                string line2 = line.TrimEnd(EndTrim);
//                line = line2 + ">" + Encoding.UTF8.GetString(frame, 18, 10);     // Call To
                line = Encoding.UTF8.GetString(frame, 18, 10);     // Call To
//                line = line.TrimEnd(EndTrim);
                line2 += ">" + line.TrimEnd(EndTrim);
                int datalen = frame[28];

                // is it a VIA packet
                if (frame[4] == 'v')
                {
                    //    int viaoffset = (ViaCount) * 10;
                    //    frame = new byte[SizeOfHeader + viaoffset + 1];     // create the frame
                    //    Hed.DataLen = ViaCount * 10 + 1;
                    //    frame[36] = (byte)ViaCount;     // # of digis - just 1 byte
                    int digicount = frame[36];
                    //    System.Buffer.BlockCopy(ViaString, 0, frame, 37, viaoffset);    // now copy in the Via string
                    string ViaString = Encoding.UTF8.GetString(frame, 37, digicount * 10);
                }

                line = line2 + ":" + Encoding.UTF8.GetString(frame, 37, datalen);     // the data
//                line += ":" + Encoding.UTF8.GetString(frame, 8, 10);     // the data
                Form1.NewSentPacket = line;
                Form1.NewAGWPEpacketSent = true;
                AddRichText(Packet_Node_Packets, "Sent: " + line + Environment.NewLine, Color.Red);
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
                Console.WriteLine("Sent {0} bytes to AGWPE server.", bytesSent);

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
