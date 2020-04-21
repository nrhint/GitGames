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

namespace RunnerTracker
{
    public class AGWSocket_Aid
    {
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

        #region Variables and declarations
        public Button Connect_Button { get; set; }
        enum PacketTypes {MM = 0, MI = 2, MU = 4, MS = 8, MC = 16, MTX = 32, MRJ = 64, MVIA = 1 }
        public bool Connected_to_AGWserver;
        public bool InitInProcess;
        public bool Registered;
        public bool ConnectInProcess;
        public bool Connected_to_Database;
        public bool Cannot_Connect_to_DB;
        public bool Monitoring;
        private bool RegisterOnInit;
        public string Version;
        private byte[] ViaString = new byte[3*10];  // space for 3 callsigns, 10 bytes each
        private int ViaCount;
        private Int32 VerNum;
        private bool HeartBeat;
        private int FiveSec;
        private int DB_Connect_Count = 0;
        bool IsCreated, PortsFound;
        int MonitorKind;
        public PORTS Ports = new PORTS();
        Socket AGWPEclient;
        bool InUse;
//        System.Timers.Timer OneSecond;
        System.Timers.Timer OneSecond;
        System.Timers.Timer TenSecond;
        string ConnectErrorMessage = "Cannot connect to the AGWPE server!";
        public RichTextBox Packet_Node_Packets { get; set; }
        delegate void ButtonTextdel(Button butn, string text, Color forecolor);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);

        public class Pdata
        {
            public string PortName;
        };
        public class PORTS
        {
	        public int Num;
            public Pdata[] Pdata = new Pdata[10];    // space for more than 3 radio channels
        };
        class HEADER
        {
            public int Port;
            public long DataKind;
            public string CallFrom;
            public string CallTo;
            public int DataLen;
            public long User;   // reserved, unused
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
        #endregion

        public AGWSocket_Aid(RichTextBox rtb)
        {
            Packet_Node_Packets = rtb;
	        Connected_to_AGWserver = false;
            InitInProcess = false;
            Monitoring = false;
	        IsCreated = false;
            HeartBeat = false;
            Registered = false;
            Connected_to_Database = false;
            Cannot_Connect_to_DB = false;
            ConnectInProcess = false;
//            FiveSec = 10;       // give it 10 sec. for initialization
	        MonitorKind = 62;
            PortsFound = false;
            InUse = false;
            VerNum = 0;

            // start the Five Second timer
            OneSecond = new System.Timers.Timer();
            OneSecond.AutoReset = true;
            OneSecond.Interval = 1000;     // 1 sec.
//            OneSecond.Interval = 5000;     // 5 sec.
            OneSecond.Elapsed += new ElapsedEventHandler(OneSecond_Elapsed);
            OneSecond.Start();

            // start the Ten Second timer
            TenSecond = new System.Timers.Timer();
            TenSecond.AutoReset = true;
            TenSecond.Interval = 10000;     // 10 sec.
            TenSecond.Elapsed += new ElapsedEventHandler(TenSecond_Elapsed);
//            TenSecond.Start();
        }

        #region Timers
        #region One Second timer
//        static int ConnectCount = 0;
        bool HeaderRcvd = false;
        PACKET packet;
        void OneSecond_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function will be used for 2 purposes:
            //      1. watch for data to receive from the AGWPE Server
            //      2. test if connected with the Central Database, using DB_Connect_Count
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
                            int w = 8;
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

//// removed 1/15/16            // test if Connected to Central Database
//            if (DB_Connect_Count != 0)
//            {
//                DB_Connect_Count--;
//                if (DB_Connect_Count == 0)
//                {
//                    if (!Connected_to_Database)
//                    {
//                        AddRichText(Packet_Node_Packets, "Failed to connect to Central Database!" + Environment.NewLine, Color.Green);
//                        MessageBox.Show("Did not connect to Central Database", "Not connected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//                        ChangeButtonText(Connect_Button, "Connect");
//                        //                        Form1.ChangeState(Form1.Server_State.Cannot_Connect);
//                        Cannot_Connect_to_DB = true;
//                    }
//                    else
//                    {
//                        Connect_Button.Visible = false;
//                    }
//                    ConnectInProcess = false;
//                }
//            }
        }
        #endregion

        #region Ten Second timer
        void TenSecond_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function will be used for only 1 purpose:
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
                    if ((MonitorKind & (int)PacketTypes.MU) == (int)PacketTypes.MU)
                    {
// Monitoring not used here                        FormatData(Hed.Port, response, Hed.DataLen);
                    }
                    break;
                case 'm':   // request to start Monitoring
                    if ((MonitorKind & (int)PacketTypes.MU) == (int)PacketTypes.MU)
                    {
                    }
                    break;
			    case 'T':   // Response from sending data - '84'
//                  enum PacketTypes {MM = 0, MI = 2, MU = 4, MS = 8, MC = 16, MTX = 32, MRJ = 64, MVIA = 1 }
                    int test = (MonitorKind & (int)PacketTypes.MTX);
                    if ((MonitorKind & (int)PacketTypes.MTX) == (int)PacketTypes.MTX)
                    {
                        int x = 5;
    ////					Term[Header.port]->InsertTerm(szData,Header.size);
                    }
    //                flag=NOMORE2READ;
    //                continue;
                    break;
			    case 'S':   //MONITOR HEADER
                    if ((MonitorKind & (int)PacketTypes.MS) == (int)PacketTypes.MS)
                    {
    ////					Term[Header.port]->InsertTerm(szData,Header.size);
                    }
    //                flag=NOMORE2READ;
    //                continue;
                    break;
			    case 'I'    ://MONITOR  HEADER+DATA CONNECT OTHER STATIONS
                    if ((MonitorKind & (int)PacketTypes.MI) == (int)PacketTypes.MI)
                    {
    ////					Term[Header.port]->InsertTerm(szData,Header.size);
                    }
    //                flag=NOMORE2READ;
    //                continue;
                    break;
			    case 'H':   //MHeardList
    //                if (Header.callfrom[0]==' ')  
    //                    continue;
    //                wsprintf(HeardListStr,"Port%d>%s\r",Header.port+1,szData);
    ////				Term[Header.port]->InsertTerm((unsigned char*)HeardListStr,lstrlen(HeardListStr)+1);
    //                flag=NOMORE2READ;
    //                continue;
                    break;
			    case 'G':   // Radio Ports list - '71'
                    GetPorts(response);
                    PortsFound = true;
                    break;
                case 'X':   // response to Register a callsign - '88'
                    if (TxHed.DataLen == 1)
                        Registered = true;
                    else
                        Registered = false;
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
                        Connected_to_Database = true;
                    else
                        Connected_to_Database = false;
                    break;
                case 'C':   // Connected - '67'
                    break;
                case 'd':   // - '100'
                    // look at CallFrom
                    // look at response
                    if (response.StartsWith("*** DISCONNECTED RETRYOUT"))
                    {
                        Connected_to_Database = false;
                        ConnectInProcess = false;

                        // moved from timer
                        AddRichText(Packet_Node_Packets, "Failed to connect to Central Database!" + Environment.NewLine, Color.Green);
//                        MessageBox.Show("Did not connect to Central Database", "Not connected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        Modeless_MessageBox("Did not connect to Central Database", "Not connected");
                        ChangeButtonText(Connect_Button, "Connect", Color.Black);
                        Cannot_Connect_to_DB = true;
                        ConnectInProcess = false;
                    }
                    break;
                default:
//				    wsprintf((char *)szData,"%d=%c",Header.port,Header.DataKind);
//				    flag=NOMORE2READ;
                    int e = 7;      // set breakpoint here
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
        #endregion

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

        void FormatData(int port, string szData, int size)
        {
            //
            //	Format the data, changing it from AGWPE to standard APRS strings
            //
            //	then transfer it into the output buffer
            //
            string date = DateTime.Now.ToString(" [MM/dd/yy ");
            string[] Parts = szData.Split(new char[] { ' ', '\r' });
            string line;

            // verify port number
            if ((port + 1) == Convert.ToInt16(szData.Substring(1, 1)))
            {
                // port is assigned, transfer the data
                line = Parts[2] + ">" + Parts[4];	    // move in Fm and To callsigns
                if (Parts[5] == "Via")
                {
                    line += "," + Parts[6];
                }

                // add in the date
                line += date + Parts[10].Substring(2, 5) + "]:";

                // add in the rest of the data
                line += szData.Substring(szData.IndexOf('\r') + 1);

                Form1.NewRcvdPacket = line;
                Form1.NewAGWPErawPacket = szData;
                Form1.NewAGWPEpacketRcvd = true;

                AddRichText(Packet_Node_Packets, line, Color.Black);
            }
        }

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
            //  Connect to the AGWPE Server
            //  Get the AGWPE Version Number
            //  Get the Radio ports
            //  Register the base callsign
            //  Try to connect to the Database

            bool register = (bool)info;

            // connect to the server
            try
            {
                AddRichText(Packet_Node_Packets, "Attempting to connect to AGWPE server" + Environment.NewLine, Color.Green);

//                // set the Connect counter so it can be tracked in the One second timer
//                ConnectCount = 2;       // give it one second to connect

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo = Dns.Resolve(Form1.AGWPEServerName);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Convert.ToInt16(Form1.AGWPEServerPort));

                // Create a TCP/IP socket.
                AGWPEclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IsCreated = true;

                // Connect to the remote endpoint
                AGWPEclient.Connect(remoteEP);
                Connected_to_AGWserver = true;
                AddRichText(Packet_Node_Packets, "Connected to AGWPE server" + Environment.NewLine, Color.Green);
                Application.DoEvents();

                // get the version number
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
                    // Register the Station callsign
                    Hed.DataKind = (long)('X');
                    Hed.CallFrom = Form1.StationCallsign;
                    SendAGWFrame(Hed);
                    Application.DoEvents();
                }

                // process the Via String
                InitUnproto();

                // try to connect to the Database
// let Form1 do this call                Connect_to_Database();
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

        public void StartMonitoring(int port)
        {
            // Monitor frames
            HEADER Hed = new HEADER();
            Hed.Port = port;
            Hed.DataKind = (long)('m');
            SendAGWFrame(Hed);
        }

        public void Connect_to_Database()
        {
            // if this request is made while we are already in the process of
            // trying to connect, then requesting it again will cause it to cancel the connect request
            if (ConnectInProcess)
            {       // already trying to Connect - Cancel it
            }
            else
            {
//                Connect_Button.Text = "Trying to\nConnect";
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
                    Hed.CallFrom = Form1.StationCallsign;
                    Hed.CallTo = Form1.DatabaseCallsign;

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
                if (Form1.Connection_Type == Form1.Connect_Medium.APRS)
                {
                    HEADER Hed = new HEADER();     // create a new one to zero everything
                    byte[] bytearray;
                    Hed.Port = Form1.AGWPERadioPort;
                    Hed.CallFrom = Form1.StationCallsign;
                    Hed.CallTo = Form1.DatabaseCallsign;

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

        public void SendAGWTXFrame(TXHEADER Hed)
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

        public void CloseAGWPE()
        {
            // is it connected?
            if (!Connected_to_AGWserver)
            {
		        Connected_to_AGWserver = false;
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

        //
        // Init the Unproto string for the VIA string
        //
        // Return number of stations in VIA string
        //
        int InitUnproto()
        {
	        int viacount;
            string[] Parts = new string[4];     // leave space for 4, but should only get 3 maximum

	        // prepare the VIA string
	        if (Form1.VIAstring.Length != 0)
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
	        else
	        {
		        viacount=0;
                ViaString = new byte[3 * 10];   // clear the array
	        }
	        return viacount;
        }
        
        public void AGWSend(int port, string str)
        {       //  "AC7R>ARMS:?ARMS?\r"
	        int length;
	        int viacount;

	        // verify the incoming string length is less than 101 bytes
            if (str.Length > 100)
            {
                MessageBox.Show("Requested string to send:\n\n" + str + "\n\nis more than 100 bytes", "Invalid string length", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // need to format the string for AGWPE
        	// start by stripping off the callsign and to callsign at the beginning
            str = str.Substring(str.IndexOf(':') + 1);
            length = str.Length;

	        // now prepare the Header for AGWPE
            TXHEADER Hed = new TXHEADER();
	        Hed.Port=port;

	        // decide whether to just send or send with VIA string
            viacount = ViaCount;
	        if (viacount != 0)
	        {	// send with VIA string
                Hed.DataKind = (long)('V');	// Transmit data Unproto Via
                Hed.CallFrom = Form1.StationCallsign;
                Hed.CallTo = Form1.APRSnetworkName;
                viacount *= 10;
                viacount++;
                Hed.Data = ViaString + str;
                Hed.DataLen = length + viacount;
                Hed.Data = str;
	        }
	        else
	        {	// send without VIA string
                MessageBox.Show("Not sending with Via String!", "Invalid format", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
// don't stop it now                return;
                Hed.DataKind = (long)('M');	// Transmit data Unproto
                Hed.CallFrom = Form1.StationCallsign;
//                Hed.CallTo = Form1.DatabaseCallsign;
                Hed.CallTo = Form1.APRSnetworkName;
//                viacount *= 10;
//                viacount++;
                Hed.DataLen = length;
                Hed.Data = str;
            }
            SendAGWTXFrame(Hed);

            // put in the RTB
            AddRichText(Packet_Node_Packets, str, Color.Red);
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
                FormatSentData(data);
            }
            catch (Exception e)
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
            if ((frame[4] != 'R') && (frame[4] != 'G') && (frame[4] != 'X') && (frame[4] != 'm'))
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
