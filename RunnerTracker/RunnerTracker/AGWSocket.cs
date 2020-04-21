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
using System.IO;

namespace RunnerTracker
{
    public class AGWSocket
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
        enum PacketTypes {MM = 0, MI = 2, MU = 4, MS = 8, MC = 16, MTX = 32, MRJ = 64, MVIA = 1 }
        public bool Connected_to_AGWserver;
        public bool InitInProcess;
        public bool Registered;
        public bool Connected_to_Database;
        public bool Monitoring;
        public string Version;
        private byte[] ViaString = new byte[3*10];  // space for 3 callsigns, 10 bytes each
        private int ViaCount = 0;
        private Int32 VerNum;
        private bool HeartBeat;
        private int FiveSec;
        bool IsCreated, PortsFound;
        int MonitorKind;
        public PORTS Ports = new PORTS();
//        Common Common;
        Socket client;
//        bool InUse;
        System.Timers.Timer OneSecond;
        string ConnectErrorMessage = "Cannot connect to the AGWPE server!";

        public class Pdata
        {
//	        public Form1.Channels channel;
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
        class TXHEADER
        {
            public int Port;
            public long DataKind;
            public string CallFrom;
            public string CallTo = null;
            public int DataLen;
            public long User = 0;  // reserved, unused
            public string Data;
        };
        class PACKET
        {
            public byte[] Header = new byte[SizeOfHeader];
            public byte[] DataBuff = new byte[500];         // can probably reduce to about 256
//            string Data;
        }
        #endregion

        public AGWSocket()
        {
	        Connected_to_AGWserver = false;
            InitInProcess = false;
            Monitoring = false;
	        IsCreated = false;
            HeartBeat = false;
            Registered = false;
            FiveSec = 10;       // give it 10 sec. for initialization
	        MonitorKind = 62;
            PortsFound = false;
//            InUse = false;
            VerNum = 0;
//            Common = new Common();          // init the Common class

            // start the One Second timer
            OneSecond = new System.Timers.Timer();
            OneSecond.AutoReset = true;
            OneSecond.Interval = 1000;     // 1 sec.
            OneSecond.Elapsed += new ElapsedEventHandler(OneSecond_Elapsed);
            OneSecond.Start();
        }

        #region 1 sec timer
        #region One Second timer
//        static int ConnectCount = 0;
        bool HeaderRcvd = false;
        PACKET packet;
        void OneSecond_Elapsed(object source, ElapsedEventArgs e)
        {
            //
            //	this timer function will be used for 3 purposes:
            //      1. test for connect:
            //          a. if ConnectCount != 0, decrement it
            //          b. if ConnectCount == 1, decrement it, then test client.Connected
            //          c. if client.Connected == true, set IsConnected = true
            //                                 == false, start thread to ask if user would like to start AGWPE
            //		2. watch for data to receive from the AGWPE Server
            //      3. a 5 second counter is used to create a Heart Beat to test the AGWPE connection
            //

            //// test for connect
            //if (ConnectCount != 0)
            //{
            //    ConnectCount--;
            //    if (ConnectCount == 1)
            //    {
            //        ConnectCount--;
            //        if (client.Connected)
            //        {
            //            IsConnected = true;
            //        }
            //        else
            //        {
            //            // start thread to ask if user would like to start AGWPE
            //            ThreadPool.QueueUserWorkItem(new WaitCallback(StartAGWPEThread));
            //        }
            //    }
            //}

            // 5 second counter
            FiveSec--;
            if (FiveSec <= 0)
            {
// 1/10/16                FiveSec = 5;
                FiveSec = 10;

                // test for the Heart Beat only when the connection has been made
                if (Connected_to_AGWserver)
                {
                    // test if HeartBeat is still set, if so, AGWPE has stopped
                    if (HeartBeat)
                    {
//                        MessageBox.Show("AGWPE has stopped", "AGWPE Stopped", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(TellUserStoppedThread));
                        Connected_to_AGWserver = false;
                    }
                    else
                    {
                        HeartBeat = true;               // set the flag
                        HEADER Hed = new HEADER();      // send the Version request
                        Hed.DataKind = (long)('R');
                        SendAGWFrame(Hed);
                        if (!Connected_to_AGWserver)
                        {
//                            MessageBox.Show("AGWPE has stopped", "AGWPE Stopped", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(TellUserStoppedThread));
                        }
                    }
                }
            }

            try
            {
                // is there any new data from the client?
                int NewBytes = client.Available;
                if (NewBytes > 0)
                {
                    // are we waiting for more data?
                    if (HeaderRcvd)
                    {       // waiting for more data
//                        int w = 8;      // set breakpoint here
                    }
                    else
                    {       // not waiting for more data - this is totally new data
                        // is there enough data for the Header?
                        if (NewBytes < SizeOfHeader)
                            return;     // quit early - too little data

                        // create a new Packet
                        packet = new PACKET();

                        // read the socket data into the packet buffer
                        client.Receive(packet.Header, SizeOfHeader, SocketFlags.None);

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
                            client.Receive(packet.DataBuff, DataLen, SocketFlags.None);
                        }

                        // clear the flag
                        HeaderRcvd = false;

                        // go process the packet
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketThread),packet);
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }
        }
        #endregion

        #region Thread to ask user if he wants to start AGWPE, because we can connect to it, so it must not be running
        private void StartAGWPEThread(object info)
        {
            DialogResult res = MessageBox.Show("Connection to the AGWPE server failed, with this error message:\n\n" + ConnectErrorMessage + "\n\n              Start the AGWPE server now?", "AGWPE Connection Failed!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes)
            {
                try
                {
//                    Process.Start(Form1.AGWPEexeFilename);
                    Thread.Sleep(2000); // wait 2 secs.
                    InitAGWPE();        // try to connect again
                }
//                catch (Exception e)
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
            HEADER Hed = new HEADER();
            Hed.Port = packet.Header[0];
            Hed.DataKind = packet.Header[4];
            Hed.DataLen = packet.Header[28];
            byte[] bytearray = new byte[10];
            System.Buffer.BlockCopy(packet.Header, 8, bytearray, 0, 9);
            Hed.CallFrom = Encoding.ASCII.GetString(bytearray);
            bytearray = new byte[10];
            System.Buffer.BlockCopy(packet.Header, 18, bytearray, 0, 9);
            Hed.CallTo = Encoding.ASCII.GetString(bytearray);
            string response = Encoding.ASCII.GetString(packet.DataBuff, 0, Hed.DataLen);

        	switch(Hed.DataKind)
		    {
			    case 'U':   // MONITOR DATA UNPROTO
                    if ((MonitorKind & (int)PacketTypes.MU) == (int)PacketTypes.MU)
                    {
//                        FormatData(Hed.Port, response, Hed.DataLen);
                    }
                    break;
			    case 'T':   // Response from sending data
                    if ((MonitorKind & (int)PacketTypes.MTX) == (int)PacketTypes.MTX)
                    {
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
			    case 'G':   // Radio Ports list
                    GetPorts(response);
                    PortsFound = true;
                    break;
                case 'X':   // response to Register a callsign
                    if (Hed.DataLen == 1)
                        Registered = true;
                    else
                        Registered = false;
                    break;
                case 'R':   // version number
                    if (Hed.DataLen == 8)
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
			    default:
//				    wsprintf((char *)szData,"%d=%c",Header.port,Header.DataKind);
//				    flag=NOMORE2READ;
//                    int e = 7;      // set breakpoint here
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
        #endregion

        public void GetPorts(string Str)
        {
            string[] Parts = new string[10];     // more than enough space for 3 ports for the 3 radio channels

            Parts = Str.Split(new char[] {';'});
            Ports.Num = Convert.ToInt16(Parts[0]);
            for (int x=0;x<Ports.Num;x++)
            {
                Ports.Pdata[x] = new Pdata();
                Ports.Pdata[x].PortName = Parts[x + 1];
//                Ports.Pdata[x].channel = Form1.Channels.None;
            }
        }

//        //
//        //	Format the data, changing it from AGWPE to standard APRS strings
//        //
//        //	then transfer it into the output buffer
//        //
//        void FormatData(int port, string szData, int size)
//        {
//            Form1.Channels channel;
////	        date = theTime.Format(" [%m/%d/%y ");
//            string date = DateTime.Now.ToString(" [MMddyy ");
////            extra += dt.ToString(" yyyyMMddHHmm");
//            string[] Parts = szData.Split(new char[] { ' ', '\r' });
//            string line;
	
//            // get the channel assigned to this port
//            if ((channel = Ports.Pdata[port].channel) != Form1.Channels.None)
//            {
//                // verify port number
//                if ((port + 1) == Convert.ToInt16(szData.Substring(1, 1)))
//                {
//                    // check if this packet is to be displayed
//                    if ((Form1.ARMSonly) && (Parts[4] != Form1.NetworkName))
//                        return;		// do not continue if not a valid ARMS packet

//                    // port is assigned, transfer the data
//                    //		        token=strtok((char *)szData," ");	// get the port number and Fm:
//                    /*
//                            //		ChannelData[channel].dwRead = size;
//                            //		MoveMemory(ChannelData[channel].ReadBuff,szData,size);
		
//                                    strcpy((char*)ChannelData[channel].ReadBuff,token=strtok(NULL," "));	// move in Fm callsign  */
//                    line = Parts[2] + ">" + Parts[4];	    // move in Fm and To callsigns
//            /*                        token=strtok(NULL," ");	// skip over 'To'
//                                    strcat((char*)ChannelData[channel].ReadBuff,">");	// move in '>'
//                                    strcat((char*)ChannelData[channel].ReadBuff,token=strtok(NULL," "));	// move in To callsign  */


////                                    token=strtok(NULL," ");	// get Via or time
////                                    if (strncmp(token,"Via",3) == 0)
//                                    if (Parts[5] == "Via")
//                                    {
////                                        token=strtok(NULL," ");	// skip over 'Via'
////                                        strcat((char*)ChannelData[channel].ReadBuff,",");	// move in ','
////                                        strcat((char*)ChannelData[channel].ReadBuff,token);	// move in Via path
//                                        line += "," + Parts[6];
//                                    }
////                                    token=strtok(NULL,"[");	// skip over 'pid, etc'
//                            //		strcat((char*)ChannelData[channel].ReadBuff," [");	// move in ' ['
//                            // add in date
////                                    strcat((char*)ChannelData[channel].ReadBuff,date);	// move in date
//                    line += date + " " + Parts[10].Substring(2,9);
//                            //		strcat((char*)ChannelData[channel].ReadBuff," ");	// move in ' '
///*                                    strcat((char*)ChannelData[channel].ReadBuff,token=strtok(NULL,"]"));	// move in time
//                                    strcat((char*)ChannelData[channel].ReadBuff,"]:");	// move in ']:'
//                                    token=strtok(NULL,"\n");
//                                    strcat((char*)ChannelData[channel].ReadBuff,token+1);	// move in the rest of the data */
//                    line += Parts[11];

///*                                    ChannelData[channel].dwRead = strlen((char*)ChannelData[channel].ReadBuff);
//                                    TransferBytes(channel);     */
//                    Common.ProcessPacket(channel, line);
//                }
//            } 
//        }

        public void InitAGWPE()
        {
            // first test if it is already connected
            if (!Connected_to_AGWserver)
            {
                // just start a thread to do the actual AGWPE Socket initialization
                ThreadPool.QueueUserWorkItem(new WaitCallback(InitAGWThread));
            }
        }

        private void InitAGWThread(object info)
        {
            //  Verify all the parameters have been set
            //  Connect to the AGWPE Server
            //  Get the AGWPE Version Number
            //  Get the Radio ports
            //  Register the base callsign
            //
            // wait for the Form1 to be displayed first
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
            if ((Form1.DatabaseCallsign == null) || (Form1.DatabaseCallsign == ""))
            {
                MessageBox.Show("Cannot start the AGWPE server because\n\nthe Server Callsign has not been set!", "Missing AGWPE Server Callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit early
            }

            // set the flag
            InitInProcess = true;

            // set the Connect counter so it can be tracked in the One second timer
//            ConnectCount = 2;       // give it one second to connect

            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo = Dns.Resolve(Form1.AGWPEServerName);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Convert.ToInt16(Form1.AGWPEServerPort));

                // Create a TCP/IP socket.
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint
                client.Connect(remoteEP);
                Connected_to_AGWserver = true;

                // get the version number
                HEADER Hed = new HEADER();
                Hed.DataKind = (long)('R');
                SendAGWFrame(Hed);

                // build the radio ports table
//                BuildPortTable();
//                HEADER Hed = new HEADER();
                Hed.DataKind = (long)('G');
                SendAGWFrame(Hed);

                // Register callsign
                Hed = new HEADER();
                Hed.DataKind = (long)('X');
                Hed.CallFrom = Form1.DatabaseCallsign;
                SendAGWFrame(Hed);
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

//                        Process.Start(path + "\\AGW Packet Engine.lnk");
                        Process.Start(path);
//                        Thread.Sleep(2000); // wait 2 secs.
                        Thread.Sleep(10000); // wait 10 secs.
                        InitAGWPE();        // try to connect again
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

        //private void BuildPortTable()
        //{
        //    HEADER Hed = new HEADER();
        //    Hed.DataKind = (long)('G');
        //    SendAGWFrame(Hed);
        //}

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
            Send(client, frame);
        }

        private void SendAGWTXFrame(TXHEADER Hed)
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
            bytearray = Encoding.ASCII.GetBytes(Hed.Data);
            System.Buffer.BlockCopy(bytearray, 0, frame, 37 + viaoffset, Hed.Data.Length);
            Send(client, frame);
        }

        public void CloseAGWPE()
        {
            // is it connected?
            if (!Connected_to_AGWserver)
            {
		        Connected_to_AGWserver = false;
            }

            // Has the socket been Created?
            if (!IsCreated)
            {
                // Release the socket
                if (client.Connected)
                    client.Shutdown(SocketShutdown.Both);
                client.Close();
        
                IsCreated = false;
            }
        }

        ////
        //// Init the Unproto string for the VIA string
        ////
        //// Return number of stations in VIA string
        ////
        //int InitUnproto()
        //{
        //    int viacount;
        //    string[] Parts = new string[4];     // leave space for 4, but should only get 3 maximum

        //    // prepare the VIA string
        //    //	        if (Form1.NetworkUnproto.Length != 0)
        //    if (Form1.VIAstring.Length != 0)
        //    {
        //        // get another copy to use with strtok
        //        //                Parts = Form1.NetworkUnproto.Split(new char[] { ',' });
        //        Parts = Form1.VIAstring.Split(new char[] { ',' });

        //        viacount = Parts.Length;
        //        if (viacount > 3)
        //        {
        //            //			exit (7998);	// too many callsigns (>3) for VIA string
        //            MessageBox.Show("Too many VIA strings", "Invalid Network Unproto string", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //            return 0;
        //        }
        //        ViaCount = viacount;

        //        int i = 0;
        //        byte[] bytearray;
        //        foreach (string str in Parts)
        //        {
        //            bytearray = Encoding.ASCII.GetBytes(str);
        //            System.Buffer.BlockCopy(bytearray, 0, ViaString, i * 10, str.Length);
        //            i++;
        //        }
        //    }
        //    else
        //    {
        //        viacount = 0;
        //        ViaString = new byte[3 * 10];   // clear the array
        //    }
        //    return viacount;
        //}

        //public void CloseChannel(Form1.Channels channel)
        //{
        //    int i;

        //    // verify the port exists
        //    string portName = string.Empty;
        //    switch (channel)        // first get the assigned port name
        //    {
        //        case Form1.Channels.Main:
        //            portName = Form1.MainChannel.AGWPort;
        //            break;
        //        case Form1.Channels.Side1:
        //            portName = Form1.Side1Channel.AGWPort;
        //            break;
        //        case Form1.Channels.Side2:
        //            portName = Form1.Side2Channel.AGWPort;
        //            break;
        //    }
        //    if (portName != "")
        //    {
        //        for (i = 0; i < Ports.Num; i++)
        //        {
        //            if (portName == Ports.Pdata[i].PortName)    // find it in the Ports table
        //            {
        //                Ports.Pdata[i].channel = Form1.Channels.None;
        //                break;
        //            }
        //        }
        //    }
        //}

        ////
        //// Init a channel to use AGWPE
        ////
        //// only one channel can be assigned to use each AGWPE port at a time, so need to check if it is already in use
        ////
        //public int InitChannel(Form1.Channels channel, string name)	// returns 0 for good, error # for bad
        //{
        //    int i;
        //    int result = 0;

        //    // open the AGW port
        //    if (!Connected_to_AGWserver)   	// verify the AGWPE is Connected
        //    {
        //        // attempt to Connect to the AGWPE server
        //        InitAGWPE();

        //        while (InitInProcess)   // wait if it is still in process
        //            ;
        //        if (!Connected_to_AGWserver)
        //        {
        //            result = 60;		// AGWPE not open
        //            return result;
        //        }
        //    }

        //    // wait for the Ports to be enumerated (if the channel is being opened on startup)
        //    if (!PortsFound)
        //    {
        //        i = 10;
        //        while (!PortsFound)
        //        {
        //            if (i > 0)
        //            {
        //                Application.DoEvents();
        //                Thread.Sleep(2000);     // wait 2 sec.
        //            }
        //            else
        //            {
        //                i--;
        //                if (i==0)
        //                    return 70;      // AGWPE radio ports have not been found
        //            }
        //        }
        //    }
        
        //    // verify the port exists
        //    string portName = string.Empty;
        //    switch (channel)        // first get the assigned port name
        //    {
        //        case Form1.Channels.Main:
        //            portName = Form1.MainChannel.AGWPort;
        //            break;
        //        case Form1.Channels.Side1:
        //            portName = Form1.Side1Channel.AGWPort;
        //            break;
        //        case Form1.Channels.Side2:
        //            portName = Form1.Side2Channel.AGWPort;
        //            break;
        //    }
        //    if (portName == "")
        //        return 75;      // channel name has not been selected in Settings
        //    for (i = 0; i < Ports.Num; i++)
        //    {
        //        if (portName == Ports.Pdata[i].PortName)    // find it in the Ports table
        //            break;
        //    }
        //    if (i == Ports.Num)     // has it looked beyond the end of the table?
        //    {
        //        // Desired port does not exist
        //        result = 61;
        //        return result;
        //    }
        //    if (Ports.Pdata[i].channel != Form1.Channels.None)
        //        return 62;        // Port has already been assigned
        //    else
        //        Ports.Pdata[i].channel = channel;       // assign it to this channel

        //    // begin Monitoring packets, if not already monitoring
        //    if (!Monitoring)
        //    {
        //        // Monitor frames
        //        HEADER Hed = new HEADER();
        //        Hed.DataKind = (long)('m');
        //        SendAGWFrame(Hed);
        //    }

        //    // Initialize Channel name, channel number
        //    Form1.ChannelData[(int)channel].channel = channel;
        //    Form1.ChannelData[(int)channel].Inited = true;
        //    return result;
        //}

        public void AGWSend(int channel, string str)
        {       //  "AC7R>ARMS:?ARMS?\r"
	        int length;
	        int port;
	        int viacount;

	        // verify the incoming string length is less than 101 bytes
            if ((length = str.Length) > 100)
            {
                //	exit (7890);
                MessageBox.Show("Requested string to send:\n\n" + str + "\n\nis more than 100 bytes", "Invalid string length", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // need to format the string for AGWPE
        	// start by stripping off the callsign and to callsign at the beginning
        //	token = strtok(ptr,":");	// get the two callsigns
        //	token = strtok(NULL,"\n");	// get the rest of the string
            str = str.Substring(str.IndexOf(':') + 1);

        /* for reference
        struct Pdata
        {
	        int channel;
	        char PortName[180];
        };
        struct PORTS
        {
	        int Num;
        //	char Port[100][180];
	        struct Pdata Pdata[4];
        };*/

        //    // find which port is assigned to this channel
        //    bool good = false;
        //    for (port=0; port<Ports.Num; port++)
        //    {
        //        if (channel == (int)Ports.Pdata[port].channel)
        //        {
        //            good = true;
        //            break;
        //        }
        //    }
        //    if (!good)
        //    {
        ////		exit (7889);	// could not find port channel to match channel to send to
        //        MessageBox.Show("Desired send channel not found in Port list!","Channel not in Port list", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        return;
        //    }

	        // now prepare the Header for AGWPE
        //	ZeroMemory(&Hed,sizeof(Hed));
            TXHEADER Hed = new TXHEADER();
            port = 0;
	        Hed.Port=port;

	        // decide whether to just send or send with VIA string
//	        viacount = ViaString[0];
            viacount = ViaCount;
	        if (viacount != 0)
	        {	// send with VIA string
//		        Hed.DataKind=MAKELONG('V',0);	// Transmit data Unproto Via
//		        lstrcpy((char *)Hed.callfrom,Base_Callsign);
//		        lstrcpy((char *)Hed.callto,Network_Name);
                Hed.DataKind = (long)('V');	// Transmit data Unproto Via
                Hed.CallFrom = Form1.DatabaseCallsign;
//                Hed.CallTo = Form1.StationCallsign;
//                viacount *= 10;
//                viacount++;
////		        memcpy(Hed.Data,ViaString,viacount);
//                Hed.Data = ViaString + str;
////		        strcpy((char *)Hed.Data+viacount,ptr);
////		        Hed.size=length+viacount;
//                Hed.DataLen = length + viacount;
                Hed.Data = str;
	        }
	        else
	        {	// send without VIA string
//		        exit (7888);	// will not complete at this time (not expected to happen)
                MessageBox.Show("Not sending with Via String!", "Invalid format", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
	        }
//	        Send(&Hed,sizeof(Hed));
//            Hed.DataKind = (long)('X');
//            Hed.CallFrom = Form1.BaseCallsign;
            SendAGWTXFrame(Hed);
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
                    Console.WriteLine("Sent {0} bytes to server.", bytesSent);

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
