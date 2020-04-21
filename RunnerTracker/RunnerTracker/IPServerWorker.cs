using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace RunnerTracker
{
    //
    //  The sole purpose for this Worker Thread is to create a Station Worker Thread
    //  for each client that connects to this server
    //

    class IPServerWorker
    {
        #region variables and declarations
        // handles back into the form objects - labels, textboxes and strings
        delegate void MakeVisibledel(Control cntrl, bool state);
        delegate void SetTextdel(TextBox tb, string str);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        public Label Server_Not_Initted { get; set; }
        public Label Server_Waiting { get; set; }
        public Label Server_Client_Connected { get; set; }
        public Label Server_Cannot_Init { get; set; }
        public Label Need_Station_List { get; set; }
        public int Server_Port_Number { get; set; }
        public string Server_IP_Address { get; set; }
        public TextBox Server_Error_Message { get; set; }
        public TextBox Welcome_Message { get; set; }
        public int Number_of_Sockets { get; set; }
        public RichTextBox Ethernet_Packets { get; set; }
        private volatile TcpListener TTC_Listener;
        private volatile bool Server_shouldStop = false;
        private enum Server_State { Not_Initted, Waiting, ClientConnected, Error }
        private Server_State state = Server_State.Not_Initted;
        private string error = string.Empty;
        public System.Timers.Timer Threesecond = new System.Timers.Timer();
        #endregion

        public void Start()
        {
            // give the thread a name and clear the count
            Thread.CurrentThread.Name = "IP Server worker thread";
            Number_of_Sockets = 0;

            // start the Server
            try
            {
                IPAddress ipAd = IPAddress.Parse(Server_IP_Address);
                TTC_Listener = new TcpListener(ipAd, Server_Port_Number);
                ChangeState(Server_State.Waiting);
                TTC_Listener.Start();     // Start Listeneting at the specified port

//// this method hogged too much CPU time, so will trade for a 3 second timer
//                // loop until stop is requested
//                while (!Server_shouldStop)
//                {
//                    // wait for a connection
//                    if (!TTC_Listener.Pending())
//                    {
//                        Application.DoEvents();     // just waiting
//                    }
//                    else
//                    {
//                        //Accept the pending client connection and return a TcpClient object initialized for communication.
//                        TcpClient tcpClient = TTC_Listener.AcceptTcpClient();
//                        Console.WriteLine("Connection accepted from " + tcpClient.Client.RemoteEndPoint.ToString());
//                        ChangeState(Server_State.ClientConnected);

//                        // increment the count
//                        Number_of_Sockets++;

//                        // open a new Socket worker thread
//                        Thread StationThread = new Thread((new StationWorker()).Start);
//                        StationThread.Start(tcpClient);
//                        Console.WriteLine("Starting new Station worker thread, for IP: " + tcpClient.Client.RemoteEndPoint);
//                    }
//                }

                // start a three second timer for the actual processing
                Threesecond.Interval = 3000;
                Threesecond.AutoReset = true;
                Threesecond.Elapsed += new System.Timers.ElapsedEventHandler(ServerTimeEvent);
                Threesecond.Start();
            }
            catch (Exception e)
            {
                error = e.Message;
                ChangeState(Server_State.Error);
            }
        }

        void ServerTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            //
            // this thread occurs every 3 seconds
            // Purpose:  1. test if Stop has been requested
            //           2. look for new connect requests
            //
            Thread.CurrentThread.Name = "Server worker Time event thread";
            if (!Server_shouldStop)
            {
                // check for a new connection
                if (TTC_Listener.Pending())
                {
                    // Accept the pending client connection and return a TcpClient object initialized for communication.
                    TcpClient tcpClient = TTC_Listener.AcceptTcpClient();
                    Console.WriteLine("Connection accepted from " + tcpClient.Client.RemoteEndPoint.ToString());
                    AddRichText(Ethernet_Packets, "Connection accepted from " + tcpClient.Client.RemoteEndPoint.ToString() + Environment.NewLine, Color.Green);
                    ChangeState(Server_State.ClientConnected);

                    // increment the count
                    Number_of_Sockets++;

                    // open a new Socket worker thread
                    DB_IP_StationWorker New_Station = new DB_IP_StationWorker();
                    New_Station.Ethernet_Packets = Ethernet_Packets;
//                    Thread StationThread = new Thread((new DB_IP_StationWorker()).Start);
                    Thread StationThread = new Thread(New_Station.Start);
                    StationThread.Start(tcpClient);
                    Console.WriteLine("Starting new Station worker thread, for IP: " + tcpClient.Client.RemoteEndPoint);
                }
            }
            else
            {
                Console.WriteLine("Server worker Time event thread terminating gracefully.");
                Threesecond.Stop();
                Threesecond.Close();
            }
        }

        public void RequestStop()
        {
            Server_shouldStop = true;
        }

        void ChangeState(Server_State newstate)
        {
            // set the new state
            state = newstate;

            // clear all the message labels
            MakeVisible(Server_Not_Initted, false);
            MakeVisible(Server_Waiting, false);
            MakeVisible(Server_Client_Connected, false);
            MakeVisible(Server_Cannot_Init, false);

            // set the new message label or text
            switch (state)
            {
                case Server_State.Not_Initted:
                    MakeVisible(Server_Not_Initted, true);
                    break;
                case Server_State.Waiting:
                    MakeVisible(Server_Waiting, true);
                    break;
                case Server_State.ClientConnected:
                    MakeVisible(Server_Client_Connected, true);
                    MakeVisible(Need_Station_List, false);      // also turn off this label if it is on
                    break;
                case Server_State.Error:
                    SetText(Server_Error_Message, error);
                    MakeVisible(Server_Cannot_Init, true);
                    break;
                default:
                    break;
            }
        }

        public void MakeVisible(Control cntrl, bool state)
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

        public void SetText(TextBox cntrl, string str)
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
                    rtb.Update();
                }
            }
        }
    }
}
