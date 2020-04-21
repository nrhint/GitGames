using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;     // 8/14/17
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RunnerTracker
{
    public partial class Form1 : Form
    {
        #region Notes and History
        //
        //  Updates:
        //  July 14, 2017 - changed SelectRole so Aid station can select Station Name from Stations.txt choices
        //  July 21, 2017 - added button for Start Aid Station to put all runners in Runners Out list at Official Start Time
        //  July 22, 2017 - changed DB Runners tab Runner listbox to sort numerically instead of alphabetically
        //                  changed some actions that happen when get Connected to when Connected & Active
        //                  changed the definition of Connected & Active for the Ethernet Connection Medium to be after all the lists are loaded
        //                  changed Runners tab Runners listbox from begin populted by reports to being populated as the Runners List is loaded, so they are the same
        //                  DB - moved Datafile loading up to be part of the Runners List loading
        //                  DB - moved Stations list loading above Runners List loading
        //                  DB - send Alert when Runners List is loaded
        //                  DB - Runners status shows if runner in one of the 3 lists
        //  July 24, 2017 - DB - made DNS listbox numeric sort and Find work
        //                  Aid - changed DNS listbox to numeric sort
        //                  Aid - Runners status shows if runner is in one of the 3 lists
        //  July 25, 2017 - Aid - changed how some things become visible when Connected & Active and then Lost Connection
        //  July 26, 2017 - added MakeEnabled and changed a few that needed it
        //  July 27, 2017 - DB - added status labels for Connected to AGWPE
        //  August 4, 2017 - Aid - added error message to say need Filename when click Load Stations Info file button with the name blank
        //                         also had to add test if it is blank when starting up
        //                         added Test_Station_Name to Load Stations Info file button click handler
        //  8/7/17 - DB changed sending of files to not include the '*' lines
        //  8/9/17 - Aid - added setting flags when lists are requested to be sent
        //  8/10/17 - Aid - added Download Bib Only button to Runners List tab. Made it visible only when using Packet or APRS
        //                  added a new Command: RequestBibList
        //                  added a new Aid_Worker.Expecting_State: Bib_List;
        //          - DB - changed Send_Alert to handle the 3 mediums
        //  8/11/17 - DB - changed Send_Alert_All_Active to send only 1 for APRS
        //  8/14/17 - Aid - added Sounds button to Settings tab.
        //                  added Sounds class and form
        //                  made Registry functions public static so they can be accessed in Sounds
        //                  added variables for Sound
        //                  read the 5 parameters at beginning of Form1
        //            Aid - added into Add_AlertThread the sound for an Alert
        //            Aid - same for Add_In_MessageThread for the Messages sound
        //             DB - added Connection Sound to New_ActiveStation thread
        //             DB - added Sounds button to Settings tab.
        //  8/15/17 - Aid - added more buttons to Push, to support restarting File Sending Bulletins
        //  8/18/17 - Aid - corrected Sounds for Alerts and Messages
        //  3/12/19 - changed all copyright dates to 2019
        //            added This PC's IP address textbox to Aid | Settings | Ethernet
        //            added 2 Cancel buttons in SelectRole form
        //            changed Official Start Time to not show AM/PM
        //            changed Aid Message tab to show "fully connected" when not yet connected
        //  3/27/19 - changed Aid Station DGV to decrease width when not showing Lat/Long
        //            changed DB Station DGV to not PipeHandle
        //            started adding to DB Station DGV ability to double-click # to assign sequential numbers to stations
        //  3/29/19 - created icon at this online site: https://image.online-convert.com/convert-to-ico
        //            changed handling the 14001 labels in Aid and DB
        //  4/18/19 - changed NIC adapter choice from several forms to one form to show and choose
        //  4/23/19 - changed: DB - Runner Status DGV vertical height matches number of stations,
        //                     Aid - Runner Out DGV decreased to match column width
        //          - assigned version to match month, day and time
        //  4/24/19 - DB: Stations Move Up/Down were changed from Enabled to Visible
        //              Form1_Closing - if Quit_Early, do not Save Registry
        //  4/25/10 - Aid: began adding checkboxes in APRS tab of Settings tab to enable loading lists
        //  5/1/19 - Aid: began adding new tab: Reconciled Runners
        //  5/10/19 - added SelectStationFrom to select Station for DNF tab in DB
        //  5/13/19 - DB - changed DNF tab to select Station for new runner from new form
        //  5/15/19 - DB - changed Watch tab to select Station for new runner from new form
        //  5/21/19 - DB - changes to IP_StationWorker.cs so Aid Stations that use a name not in the Stations list will be properly handled
        //  6/2/19  - DB - changed Info/Lists tab to Lists/Info
        //  7/7/19  - DB - began adding the Output Message Queue
        //                 added groupbox for "     "       " on DB Settings tab
        //
        #endregion


        #region Variables and Declarations
        #region Common Variables and Declarations
        #region classes
        public class RFID
        {
            public string String { get; set; }
            public int RunnerNumber { get; set; }
        }
        public class DB_Issue
        {
            public string EntryDate { get; set; }
            public string ResolveDate { get; set; }
            public string EntryPerson { get; set; }
            public bool Broken { get; set; }
            public bool Enhancement { get; set; }
            public string Description { get; set; }
        }
        public class Aid_Issue
        {
            public string EntryDate { get; set; }
            public string ResolveDate { get; set; }
            public string EntryPerson { get; set; }
            public string Station { get; set; }
            public bool Broken { get; set; }
            public bool Enhancement { get; set; }
            public string Description { get; set; }
        }
        public class Queue_Issue
        {
            public string EntryDate { get; set; }
            public string ResolveDate { get; set; }
            public string EntryPerson { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string Station { get; set; }
        }
        public class RunnersList
        {
            public string BibNumber { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
        }
        public class RunnerStatus
        {
            public string Station { get; set; }
            public string TimeIn { get; set; }
            public string TimeOut { get; set; }
            public string TimeAtStation { get; set; }
            public string TimeFromPrev { get; set; }
            public string TimeToNext { get; set; }
        }
        public class AllRunnerStatus
        {
            public string BibNumber { get; set; }
            public StationTime [] StationTimes { get; set; }
        }
        public class Queue_Buttons
        {
            public string Button_name { get; set; }
        }
        public class Queue_Packet_Connect_Disconnect
        {
            public char ConDis { get; set; }    // 'C', 'D'
            public string Reason { get; set; }  // "*** DIS ..", "new"
            public string Callsign { get; set; }
            public string PipeHandle { get; set; }  // handle for Pipe created in DB-AGWSocket
        }
        #endregion
        // lists
        public static List<RunnersList> RunnerList;
        public static bool RunnerList_Has_Entries;
        List<DB_Issue> DBIssues;
        public static bool New_AidStation_Issue_entry;
        List<Aid_Issue> ASIssues;
        public static List<RunnerStatus> RunnersStatus;
        public static List<AllRunnerStatus> AllRunnersStatus;
        List<RFID> RFIDAssignments;
        // queues
        public static Queue<Queue_Issue> AidStationIssuesQue;
        // timers
        System.Timers.Timer DictTimer;
        System.Timers.Timer ElapsedTimeClock;
        DateTime PrevClockTime;
        Int16 Minutes = 0;
        Int16 Hours = 0;
        // delegates
        delegate void SetTextdel(TextBox tb, string str);
//        delegate void SetCtlTextdel(Control cntrl, string str);
        delegate void SetDGVsourceStatusDel(List<RunnerStatus> RunnersStatus);
        delegate void SetDGVsourceAllStatusDel();
        delegate void Add_Portnamesdel();
        delegate void MakeEnableddel(Control cntrl, bool enable);
        delegate void MakeVisibledel(Control cntrl, bool visible);
        delegate void ChangeTabCallback(int tabIndex);
        delegate void SetRichTextdel(RichTextBox rtb, string str, Color color);
        delegate string GetTBtextdel(TextBox tb);
        delegate void SetCtlForeColordel(Control cntrl, Color clr);
        delegate void SetCtlBackColordel(Control cntrl, Color clr);
        delegate void SetCtlFocusdel(Control cntrl);

        public static bool DBRole = false;   // if Registry does not exist, make it an Aid Station
        Assembly assem;     // this program
        AssemblyName assemName; // the program's name
        Version assemVer;   // the program's version
        bool Init_Registry;
        bool Show_LatLong = true;
        bool Quit_Early = false;
        bool Loading_Info = false;
        bool DNF_List_Changed = false;
        bool Editting_DNF = false;
        bool Watch_List_Changed = false;
        bool Editting_Watch = false;
        public static bool DNSList_Has_Entries;
        public static bool DNFList_Has_Entries;
        public static bool WatchList_Has_Entries;
        public static string RunnerListPath;
        public static string DNSListPath;
        public static string DNFListPath;
        public static string WatchListPath;
        const int Station_DGV_Width = 75;
        public static int NumLogPts = 1;        // default to only 1 Log Point
        public static string Start_Time;
        SelectRoleForm SelectRole;
        public static string DataDirectory = string.Empty;
        public static String DatabaseFCCCallsign;
        public static String DatabaseTacticalCallsign;
        public static int AGWPERadioPort;
        public static String AGWPEServerName;
        public static String AGWPEServerPort;
        public static String VIAstring;
        int AGW_Count = 0;
        private Color StationC, SettingsC;
        private Control _last;
        public static String PacketInDataHandle;    // this is the handle for the Pipe Stream for data from the AGWSocket to the Worker
        public static String PacketOutDataHandle;   // this is the handle for the Pipe Stream for data from the Worker to the AGWSocket

        // added 4/7/16, to display AGWPE Statistics
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
        public static AGWPEPortStat AGWPEPortStatistics = new AGWPEPortStat();

        // added 8/14/17 for the Sounds
        string Sounds_Directory = string.Empty;
        string Connections_Sound = string.Empty;
        string File_Download_Sound = string.Empty;
        string Alerts_Sound = string.Empty;
        string Messages_Sound = string.Empty;
        #endregion

        #region Aid Station Variables and declarations
        #region classes
        public class Aid_Station
        {
            public string Name { get; set; }    // 0
            public int Number { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Previous { get; set; }
            public double DistPrev { get; set; }    // 5
            public string Next { get; set; }
            public double DistNext { get; set; }
            public double Difficulty { get; set; }
            public bool Accessible { get; set; }
            public int Number_of_Log_Points { get; set; }  // 10   Input and Output readers? 1 or 2?
            public DateTime First_Runner { get; set; }
            public DateTime Cuttoff_Time { get; set; }
        }
        public class Aid_Runner
        {
            public uint BibNumber { get; set; }
            public DateTime TimeIn { get; set; }
            public DateTime TimeOut { get; set; }
            public uint Minutes { get; set; }    // minutes at this station
            public bool Sent { get; set; }
            public string RFIDnumber { get; set; }
        }
        public class In_Message
        {
            public int Message_Number { get; set; }
            public DateTime Received_Time { get; set; }
            public int Size { get; set; }
            public string Message_string { get; set; }
        }
        public class Out_Message
        {
            public int Message_Number { get; set; }
            public DateTime Received_Time { get; set; }
            public int Size { get; set; }
            public bool Sent { get; set; }
            public bool Acknowledged { get; set; }
            public string Message_string { get; set; }
        }
        public class Command
        {
            public string Data { get; set; }
            public Aid_Worker.Expecting_State Expecting { get; set; }
        }
        public class Aid_RunnerDNFWatch
        {
            public string BibNumber { get; set; }
            public string Station { get; set; }
            public string Time { get; set; }
            public string Note { get; set; }
            public bool SentToDB { get; set; }
        }
        public class Runner_Times   // 5/5/19
        {
            public string BibNumber { get; set; }
            public DateTime TimeIn { get; set; }
            public DateTime TimeOut { get; set; }
            public bool DNSorDNF { get; set; }
            public bool New { get; set; }
        }
        #endregion
        // lists
        public static Queue<Command> CommandQue;
        List<Aid_Runner> Runners;
        List<Aid_Runner> RunnersAtStation;
        List<Aid_Runner> RunnersIn;
        List<Aid_Runner> RunnersOut;
        List<Aid_RunnerDNFWatch> Aid_DNFList;
        List<Aid_RunnerDNFWatch> Aid_WatchList;
        List<Aid_Station> Aid_Stations;
        List<In_Message> Incoming_Messages;
        List<Out_Message> Outgoing_Messages;
        List<Runner_Times> Reconciled_Runners;  // 5/5/19
        List<Runner_Times> Runners_thru_Station;    // 5/5/19
        // queues
        public static Queue<Button_to_Push> Buttons_to_Push;
        public enum Button_to_Push { Aid_AGW_Start, Aid_AGW_Connect, Aid_APRS_Connect,
            Station_Info_File, Bib_Only, Runners_List, DNS_List, DNF_List, Watch_List, Info_List }      // 8/15/17

        // strings
        public static String StationFCCCallsign;
        public static String StationTacticalCallsign;
        public static String PacketNodeCallsign;
        public static String TacticalBeaconText;
        public static String APRSnetworkName;
        public static bool NewAGWPEpacketRcvd;
        public static bool NewAGWPEpacketSent;
        public static String NewAGWPErawPacket;
        public static String NewRcvdPacket;
        public static String NewSentPacket;

        public enum Connect_Medium { APRS, Packet, Ethernet, Cellphone }
        public static Connect_Medium Connection_Type = Connect_Medium.Packet;   // default to Packet
        public enum Packet_Connect_Method { Direct, Node, ViaString }
        public static Packet_Connect_Method Packet_Connect_Mode;
        public static Boolean Use_Station_TacticalCallsign;
        struct Port
        {
            public String COMport;
            public Boolean used;
        };
        Port[] ports;
        int Total_Number_of_Runners = 0;
        int RunnerListCount = 0;
        int Remove_from_Runners_list_Minutes;
        int prev_Incoming_COM = 0;
        int prev_Outgoing_COM = 0;
        SerialPort IncomingSerialPort = new SerialPort();
        SerialPort OutgoingSerialPort = new SerialPort();
        string IncomingNumber = string.Empty;
        string OutgoingNumber = string.Empty;
        char StartOfRFIDstring;
        char EndOfRFIDstring;
        public static Aid_AGWSocket Aid_AGWSocket;
        int NumAPRSlines = 0, NumAPRSnetworklines = 0;
        bool APRSnetworkOnly;
        int APRS_TX_Count = 0;
        int APRS_Message_Number = 1;
        string Latitude, Longitude;
        public string Station_Name;
        public bool Connected_to_Packet_Node
        {
            get { return CtoPN; }
            set
            {
                CtoPN = value;
                if (CtoPN)
                    lbl_Connected_to_Packet_Node.Visible = true;
                else
                    lbl_Connected_to_Packet_Node.Visible = false;
            }
        }
        private bool CtoPN; // Connected_to_Packet_Node
        public static bool Incoming_Mess;      // flag set by Worker to indicate an incoming message has been received
        public static string Incoming_Message;
        public static bool Incoming_Alrt;
        public static string Incoming_Alert;
        public static bool Start_Time_Rcvd;     // flag set by Worker to indicate the Start time has been received
        bool One_Reader_Only = false;
        bool Auto_Send_Runner_Update = false;
        int Auto_Send_Minutes = 0;
        int Auto_Send_Elapsed_Minutes = 0;
// 8/5/16        bool Connected_to_Central_Database = false;
        bool Incoming_Message_File_Started = false;
        bool Outgoing_Message_File_Started = false;
// 7/21/16        bool Attempting_to_Connect_to_Server = false;
// 7/22/16        bool Connected_to_Server = false;
        bool Server_Mesh_Auto_Reconnect = false;
        bool Aid_IP_Good = false;
        bool Aid_Server_Port_Good = false;
        string Outgoing_Message_Filename = string.Empty;
        string Incoming_Message_Filename = string.Empty;
        string IssuesFilePath = string.Empty;
        bool IssuesLoaded = false;
        int Original_Issues_Count = 0;
        static Aid_Worker WorkerObject = new Aid_Worker();
        Thread WorkerConnectSendThread, WorkerReceiveThread;
        Thread PacketStationThread, PacketStationRcvThread;
        Thread APRSstationThread, APRSstationRcvThread;
        System.Timers.Timer Elapsed1minTimer;
        System.Timers.Timer Elapsed30secTimer;
        System.Timers.Timer Elapsed1secTimer;
        System.Timers.Timer Elapsed5secTimer;
        System.Timers.Timer APRSconnect30secTimer;
        System.Timers.Timer APRSconnect2minTimer;
        int Welcome_Count = -1;     // starting value
        delegate void GetAGWPE_PortDel();
        delegate bool TestTextboxDel(TextBox tb);
        delegate void SetDGVInMessDel(string message);
        delegate void SetDGVRASsourceDel();
        delegate void SelectTabDel();
        delegate void Bind_RunnerListdel();
        delegate void Bind_RunnersIndel();
        delegate void Bind_RunnersOutdel();
        delegate void Bind_RunnersAtStationdel();
        delegate void SetDGV2sourceDel(List<RunnerStatus> RunnersStatus);
        delegate void MakeCheckeddel(CheckBox cntrl, bool checkd);
        delegate void MakeRBCheckeddel(RadioButton rb, bool checkd);
        delegate void SetCtlTextdel(Control cntrl, string str);
        delegate void AppendTextCallback(TextBox tb, string str);
        delegate void AppendTextRtbCallback(RichTextBox tb, string str);
        delegate void LoadRunnersdel(StreamReader reader);
        delegate void LoadDNSdel(StreamReader reader);
        delegate void LoadDNFdel(StreamReader reader);
        delegate void LoadWatchdel(StreamReader reader);
        delegate void LoadIssuedel(StreamReader reader);
// 7/18/16        public enum Server_State { Not_Initted, Initted, Cannot_Connect, Attempting_Connect, Error_Connecting, Connected, Connected_Active }
        public enum Server_State { Not_Initted, Cannot_Connect, Attempting_Connect, Error_Connecting, Connected, Connected_Active }
// Aid_Worker        public enum Server_State { Not_Initted, Cannot_Connect, Attempting, Error_Connecting, Connected_To_DB, Connected_Active }
// 7/21/16        Server_State state = Server_State.Not_Initted;
// 7/21/16        public static Server_State newstate = Server_State.Not_Initted;
        enum Commands { LogPoints, SendDNFRunner, SendWatchRunner, SendIssue, RequestRunner, RequestStationInfo, RequestInfo,
            RequestRFIDAssignments, Message, RequestStartTime, RequestRunnerList, RequestBibList, RequestDNSlist,       // 8/10/17 - added RequestBibList
            RequestDNFlist, RequestWatchlist, RequestIssues, RunnerIn, RunnerOut }
        enum InitActions { Welcome, LogPoints, Runner, DNS, DNF, Watch, Info, Issues, Done }
        InitActions Current_InitAction = InitActions.Welcome;
        bool RunnerList_loading = false;
        bool DNS_loading = false;
        bool DNF_loading = false;
        bool Watch_loading = false;
        bool Info_loading = false;
        public static bool UsingRFID;
        bool Aid_ConnectedandActive = false;
        //public class RunnerStatus
        //{
        //    public string StationName { get; set; }
        //    public string TimeIn { get; set; }
        //    public string TimeOut { get; set; }
        //    public string TimeAtStation { get; set; }
        //    public string TimeToPrev { get; set; }
        //    public string TimeToNext { get; set; }
        //}
        bool APRS_Load_Runner = false;
        bool APRS_Load_DNS = false;
        bool APRS_Load_DNF = false;
        bool APRS_Load_Watch = false;
        bool APRS_Load_Info = false;
        bool Runner_List_Changed = false;   // 5/5/19
        bool Runners_Thru_Changed = false;  // 5/11/19
        #endregion

        #region Database Variables and declarations
        #region  classes
        public class NewStation
        {
            public string Name { get; set; }
            public DB_IP_StationWorker IP_StationWorker { get; set; }
            public DB_Packet_StationWorker Packet_StationWorker { get; set; }
            public DB_APRS_StationWorker APRS_StationWorker { get; set; }
            public string IP_Address_Callsign { get; set; }
            public string Medium { get; set; }
        }
        public class DB_Station
        {
            public string Name { get; set; }    // 0
            public int Number { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Previous { get; set; }
            public double DistPrev { get; set; }    // 5
            public string Next { get; set; }
            public double DistNext { get; set; }
            public double Difficulty { get; set; }
            public bool Accessible { get; set; }    // 9
            public bool Active { get; set; }        // 10
            public int Number_of_Log_Points { get; set; }  // 11   Input and Output readers? 1 or 2?
            public DateTime First_Runner_Time { get; set; }
            public DateTime Cuttoff_Time { get; set; }
            public string Medium { get; set; }      // 14   AGWPE (Packet) or IP
            public string IP_Address_Callsign { get; set; } // 15
            public int Number_Incoming_Messages { get; set; }   // 16
            public int Number_Outgoing_Messages { get; set; }   // 17
            public DB_IP_StationWorker IP_StationWorker { get; set; }    // 18
            public DB_Packet_StationWorker Packet_StationWorker { get; set; }    // 19
            public DB_APRS_StationWorker APRS_StationWorker { get; set; }    // 20
            public string PipeHandle { get; set; }    // 21
        }
        public class DB_Runner
        {
            public string Station { get; set; }
            public string BibNumber { get; set; }
            public string TimeIn { get; set; }
            public string TimeOut { get; set; }
            public uint RFIDnumber { get; set; }
        }
        public class StationReport
        {
            public string Station { get; set; }
            public string TimeIn { get; set; }
            public string TimeOut { get; set; }
            public string TimeAtStation { get; set; }
            public string TimeToPrev { get; set; }
            public string TimeToNext { get; set; }
        }
        public class StationTime
        {
            public string Name { get; set; }
            public string TimeIn { get; set; }
            public string TimeOut { get; set; }
        }
        public class RunnerDNFWatch
        {
            public string BibNumber { get; set; }
            public string Station { get; set; }
            public string Time { get; set; }
            public string Note { get; set; }
        }
        public class RunnerData
        {
            public string BibNumber { get; set; }
            public uint RFIDnumber { get; set; }
            public StationReport[] StationReports { get; set; }
        }
        public class Messages_Selected_Station
        {
            public int Message_Number { get; set; }
            public DateTime Time_Received { get; set; }
            public int Message_Size { get; set; }
        }
        public class Packet_Send
        {
            public int port { get; set; }
            public string Callsign { get; set; }
            public string data { get; set; }
            public bool APRS { get; set; }
            public int number { get; set; }     // if number == 0, then do APRS Message without ID
        }
        public class Race_Info
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public string Sponsor { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string Count { get; set; }
            public string Contact_Name { get; set; }
            public string Contact_Phone { get; set; }
            public string Packet { get; set; }
        }
        #endregion
        // lists
        List<Messages_Selected_Station> Incoming_Messages_Selected_Stations;
        List<Messages_Selected_Station> Outgoing_Messages_Selected_Stations;
        public static List<DB_Station> Stations;
        // dictionary
        public static Dictionary<string, RunnerData> RunnerDictionary;

        // queues
        public static Queue<DB_Runner> RunnerInQue;
        public static bool New_RunnerInQue_entry;
        public static Queue<DB_Runner> RunnerOutQue;
        public static bool New_RunnerOutQue_entry;
        public static Queue<NewStation> NewActiveStationQue;
        public static bool New_Active_Station_entry;
        public static Queue<RunnerDNFWatch> WatchInQue;
        public static bool New_Watch_Runner_entry;
        public static Queue<RunnerDNFWatch> DNFInQue;
        public static bool New_DNF_Runner_entry;
        public static Queue<Queue_Packet_Connect_Disconnect> Packet_Connects_Disconnects;
        public static Queue<Packet_Send> Packet_Send_Que;

        delegate void SetDGVsourceDel(List<RunnerStatus> RunnersStatus);
        delegate void UpdateStationDGVDel();
        delegate void BindStationDGVDel();
        delegate void TestTabControlTabNameDel();
        delegate void CheckCBdel(CheckBox cb, bool checkd);
        delegate void CheckRBdel(RadioButton rb, bool checkd);
        delegate void SetFocus(Control cntrl);
        delegate void AddRunnerInDel(string BibNumber);
        delegate void LB_Run_Sel_ChangedDel();
        public static string Welcome_Message;
        public static Boolean Use_Database_TacticalCallsign;
        public static bool Stations_Activity_Flag = false;   // this flag will be set by the StationWorker when the Stations List has changed
// 8/14/17 not needed        public static bool Add_Station = false;   // this flag will be set by the StationWorker when a new station connects that is not in the Station list
  //      public static string New_Station_Name;      // name of new station to add
    //    public static DB_IP_StationWorker New_Station_Worker; // class for new station just added
        public static string Stations_Info_Filename;
        public static string RFID_Assignments_Filename;
        public static DB_AGWSocket DB_AGWSocket;
        static DataFile DataFile;
        bool RRS = false;
        bool Runners_Red_Showing
        {
            get { return RRS; }
            set { RRS = value;
                MakeVisible(lbl_Runners_in_Red, true);
            }
        }
        int Rotating_Index = 0;
        bool Welcome_Message_Exists = false;
        bool Binding_Stations_DGV = false;
        bool OK_to_start_IP_Server = false;
        public static int Num_Stations = 0;
        System.Timers.Timer ElapsedTime1sec;
        System.Timers.Timer Elapsed1minTimerDB;
        private Color DBIssuesC, AidIssuesC;
        static IPServerWorker ServerObj = new IPServerWorker();
        Thread ServerThread = new Thread(ServerObj.Start);
        bool Server_Thread_Initted = false;
        List<RunnerDNFWatch> DNFList;
        List<RunnerDNFWatch> WatchList;
        public static bool InfoLoaded = true;
        public static string InfoFilePath;
        public static string DBIssuesFilePath = string.Empty;
        public static string ASIssuesFilePath = string.Empty;
        bool DBIssuesLoaded = false;
        bool ASIssuesLoaded = false;
        bool New_AS_Issue = false;
        public static bool ASIssues_Has_Entries;
        public static bool DBIssues_Has_Entries;
        public static string LogFileName;
        public bool Distance_5mph;
        public bool Stay_10min;
        public bool After_Cutoff;
        bool InfoPageShown = false;
        bool ConnectViaEthernet, ConnectViaAPRS, ConnectViaPacket;
        int Connected_Stations = 0;
        #endregion
        #endregion


        #region Form1
        public Form1()
        {
            InitializeComponent();

            // Get the version of the executing assembly (that is, this assembly).
            assem = Assembly.GetEntryAssembly();
            assemName = assem.GetName();
            assemVer = assemName.Version;
            lbl_Aid_Program_Version.Text = "Version: " + assemVer.ToString();
            lbl_DB_Program_Version.Text = "Version: " + assemVer.ToString();
            Console.WriteLine("Application: {0}, Version: {1}", assemName.Name, assemVer.ToString());

            // need this, if SelectRole chooses DB
            RunnerDictionary = new Dictionary<string, RunnerData>();

            #region Initial Settings and Select Role
            // read the Registry
            string temp = string.Empty;
            Station_Name = "Not yet identified";
            Stations_Info_Filename = "";    // 7/14/17
            if (Test_Registry())
            {       // now only get those items needed for the SelectRoleForm
                Station_Name = Read_Registry("Station Name");
                Stations_Info_Filename = Read_Registry("Stations Info File");       // 7/14/17 moved here
                NumLogPts = 1;
                temp = Read_Registry("Connection Type");
                switch (temp)
                {
                    case "APRS":
                        Connection_Type = Connect_Medium.APRS;
                        rb_Use_APRS.Checked = true;
                        break;
                    case "Packet":
                        Connection_Type = Connect_Medium.Packet;
                        rb_Use_Packet.Checked = true;
                        break;
                    case "Ethernet":
                        Connection_Type = Connect_Medium.Ethernet;
                        rb_Use_Ethernet.Checked = true;
                        break;
                    case "Cellphone":
                        Connection_Type = Connect_Medium.Cellphone;
                        rb_Use_Cellphone.Checked = true;
                        break;
                }
                if (Read_Registry("Connect via Ethernet") == "Yes")
                    ConnectViaEthernet = true;
                else
                    ConnectViaEthernet = false;
                if (Read_Registry("Connect via Packet") == "Yes")
                    ConnectViaPacket = true;
                else
                    ConnectViaPacket = false;
                if (Read_Registry("Connect via APRS") == "Yes")
                    ConnectViaAPRS = true;
                else
                    ConnectViaAPRS = false;
                if (Read_Registry("Using RFID") == "Yes")
                    UsingRFID = true;
                else
                    UsingRFID = false;
                if (Read_Registry("Number of Log Points") == "2")
                    NumLogPts = 2;
                DataDirectory = Read_Registry("Data Directory");
                temp = Read_Registry("Program Role");   // read this one last
            }
            if (temp == "Database")
                DBRole = true;

            // prepare the Select Role form
            SelectRole = new RunnerTracker.SelectRoleForm(DataDirectory, Connection_Type, NumLogPts, DBRole);
            if (Station_Name != "")
                SelectRole.StationName = Station_Name;
            SelectRole.Stations_Info_Filename = Stations_Info_Filename;     // 7/14/17
            SelectRole.UsingRFID = UsingRFID;
            SelectRole.ConnectViaEthernet = ConnectViaEthernet;
            SelectRole.ConnectViaPacket = ConnectViaPacket;
            SelectRole.ConnectViaAPRS = ConnectViaAPRS;
            DialogResult RoleResult = SelectRole.ShowDialog();
            if (RoleResult == System.Windows.Forms.DialogResult.Cancel)
            {
                Quit_Early = true;
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                return;     // do not do anything else
            }

            // get the Sound parameters from the Registry
            if (Test_Registry())
            {
                // get the 5 parameters
                Sounds_Directory = Read_Registry("Sounds Directory");
                Connections_Sound = Read_Registry("Connections Sound");
                File_Download_Sound = Read_Registry("File Download Sound");
                Alerts_Sound = Read_Registry("Alerts Sound");
                Messages_Sound = Read_Registry("Messages Sound");
            }

            // finish initializing the Registry
            if (!Test_Registry())
            {   // write default values
                Save_Registry("Program Role", "Aid Station");
                Save_Registry("Remove from List", "1");     // remove Runners from Station list after how many minutes?
                Save_Registry("Connection Type", "Ethernet");
                Save_Registry("Connect via Ethernet", "No");
                Save_Registry("Connect via Packet", "No");
                Save_Registry("Connect via APRS", "No");
                Save_Registry("Using RFID", "No");
                Save_Registry("Number of Log Points", "1"); // 1 = into only, 2 = in and out of station
                Save_Registry("AGWPE Radio Port", "");
                Save_Registry("AGWPE Server Name", "localhost");
                Save_Registry("AGWPE Server Port", "8000");
                Save_Registry("APRS Network packets only", "No");
                Save_Registry("APRS Network Name", "");
                Save_Registry("Longitude", "");
                Save_Registry("Latitude", "");
                Save_Registry("Station FCC Callsign", "");
                Save_Registry("Use Station Tactical", "No");
                Save_Registry("Station Tactical Callsign", "");
                Save_Registry("Database FCC Callsign", "");
                Save_Registry("Use Database Tactical", "No");
                Save_Registry("Database Tactical Callsign", "");
                Save_Registry("Packet Node Callsign", "");
                Save_Registry("Packet Connect", "Direct");
                Save_Registry("VIA string", "");
                Save_Registry("Tactical Beacon Text", "");
                Save_Registry("Incoming RFID COM port", "");
                Save_Registry("Mesh IP Address", "");
                Save_Registry("Mesh Server Port #", "14001");
                Save_Registry("Mesh Auto Reconnect", "Yes");
                Save_Registry("Outgoing RFID COM port", "");
                Save_Registry("RFID Assignments File", "");
                Save_Registry("Stations Info File", "");
                Save_Registry("Show Lat/Long", "Yes");
                Save_Registry("Station Name", "Not yet identified");
                Save_Registry("Data Directory", "");
                Save_Registry("Auto Send Update", "No");
                Save_Registry("Auto Send Time", "1");
                Save_Registry("Welcome Message", "");
                Save_Registry("Highlight Distance covered less than 5mph", "");
                Save_Registry("Highlight Staying at Station longer than 10min", "");
                Save_Registry("Highlight Arrive/Leave after Station Cuttoff", "");
                Save_Registry("Load APRS Runners", "No");
                Save_Registry("Load APRS DNS", "No");
                Save_Registry("Load APRS DNF", "No");
                Save_Registry("Load APRS Watch", "No");
                Save_Registry("Load APRS Info", "No");
                Save_Registry("Ethernet response (sec)", "30");     // added 7/7/19
                Save_Registry("APRS/Packet response (sec)", "60");  // added 7/7/19
            }

            // start the clock timer
            ElapsedTimeClock = new System.Timers.Timer();
            ElapsedTimeClock.AutoReset = false;
            ElapsedTimeClock.Interval = 30000;   // set for 30 seconds just to get started
            ElapsedTimeClock.Elapsed += new ElapsedEventHandler(Elapsed_SetClock_Handler);
            ElapsedTimeClock.Start();
            SetTBtext(tb_Current_Time, DateTime.Now.ToString("HH:mm"));    // display the current time - displays leading 0 for Hours

            // determine which Role we are doing today
            if (SelectRole.DatabaseRole)    // if Database Role has been selected, just continue here
            {
            #endregion

                #region Database Form1 actions
                _last = (Control)tb_original_focus;
                MakeVisible(lbl_Start_Time_Change, true);   // 7/18/17

                AddRichText(rtb_DB_Packet_Packets, "Starting the Database role" + Environment.NewLine, Color.Fuchsia);
                AddRichText(rtb_APRS_Packets_Received_DB, "Starting the Database role" + Environment.NewLine, Color.Fuchsia);
                AddRichText(rtb_Ethernet_Packets, "Starting the Database role" + Environment.NewLine, Color.Fuchsia);

                // initialize the lists and queues
                RunnersStatus = new List<RunnerStatus>();
                AllRunnersStatus = new List<AllRunnerStatus>();
                Stations = new List<DB_Station>();
                Incoming_Messages_Selected_Stations = new List<Messages_Selected_Station>();
                Outgoing_Messages_Selected_Stations = new List<Messages_Selected_Station>();
                RFIDAssignments = new List<RFID>();
//                RunnerDictionary = new Dictionary<string, RunnerData>();
                RunnerList = new List<RunnersList>();
                DNFList = new List<RunnerDNFWatch>();
                WatchList = new List<RunnerDNFWatch>();
                DBIssues = new List<DB_Issue>();
                ASIssues = new List<Aid_Issue>();
                RunnerInQue = new Queue<DB_Runner>();
                RunnerOutQue = new Queue<DB_Runner>();
                WatchInQue = new Queue<RunnerDNFWatch>();
                DNFInQue = new Queue<RunnerDNFWatch>();
                NewActiveStationQue = new Queue<NewStation>();
                AidStationIssuesQue = new Queue<Queue_Issue>();
                Packet_Connects_Disconnects = new Queue<Queue_Packet_Connect_Disconnect>();
                Packet_Send_Que = new Queue<Packet_Send>();
                New_RunnerInQue_entry = false;
                New_RunnerOutQue_entry = false;
                New_Watch_Runner_entry = false;
                New_DNF_Runner_entry = false;
                New_Active_Station_entry = false;
                New_AidStation_Issue_entry = false;

                // initialize the OutputMessageQueue - added 7/7/19
                Output_Message_Queue_Init();

                // Tab Header color
                foreach (TabPage tp in tabControl_DB_Main.TabPages)
                    SetTabHeaderDBMain(tp, Color.FromKnownColor(KnownColor.Control));
                StationC = Color.FromKnownColor(KnownColor.Control);
                SettingsC = Color.FromKnownColor(KnownColor.Control);
                DBIssuesC = Color.FromKnownColor(KnownColor.Control);
                this.tabControl_DB_Main.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl_Main_DB_DrawItem);
                foreach (TabPage tp in tabControl_Issues.TabPages)    // also for Issues tab pages
                    SetTabHeaderDBIssues(tp, Color.FromKnownColor(KnownColor.Control));
                AidIssuesC = Color.FromKnownColor(KnownColor.Control);
                this.tabControl_Issues.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl_Issues_DB_DrawItem);

                // start the Timers
                ElapsedTime1sec = new System.Timers.Timer();
                ElapsedTime1sec.AutoReset = true;
                ElapsedTime1sec.Interval = 1000;   // 1 second
                ElapsedTime1sec.Elapsed += new ElapsedEventHandler(Elapsed_DB_1sec_Handler);
                ElapsedTime1sec.Start();
                Elapsed1minTimerDB = new System.Timers.Timer();
                Elapsed1minTimerDB.AutoReset = true;
                Elapsed1minTimerDB.Interval = 60000;   // 1 minute
                Elapsed1minTimerDB.Elapsed += new ElapsedEventHandler(Elapsed1minDBHandler);
                Elapsed1minTimerDB.Start();

                // start a timer to look at its tabs to see if any errors have occurred
                DictTimer = new System.Timers.Timer();
                DictTimer.Interval = 500;      // set for 1/2 seconds
                DictTimer.AutoReset = true;    // repeat
                DictTimer.Elapsed += new System.Timers.ElapsedEventHandler(DictTimeEvent);
                DictTimer.Start();

                #region Read the Registry for the Database
                // read the Registry
                Init_Registry = true;
                if (DBRole != SelectRole.DatabaseRole)     // first save the Role, if it has changed
                {
                    DBRole = SelectRole.DatabaseRole;
                    if (DBRole)
                        Save_Registry("Program Role", "Database");
                    else
                        Save_Registry("Program Role", "Aid Station");
                }
                temp = Read_Registry("AGWPE Radio Port");
                if (temp != "")
                    AGWPERadioPort = Convert.ToInt16(temp);
                AGWPEServerName = Read_Registry("AGWPE Server Name");
// 7/26/17                tb_DB_AGWPEServer.Text = AGWPEServerName;
                SetTBtext(tb_DB_AGWPEServer, AGWPEServerName);  // 7/26/17
                AGWPEServerPort = Read_Registry("AGWPE Server Port");
// 7/26/17                tb_DB_AGWPEPort.Text = AGWPEServerPort;
                SetTBtext(tb_DB_AGWPEPort, AGWPEServerPort);    // 7/26/17
                APRSnetworkName = Read_Registry("APRS Network Name");
// 7/26/17                tb_DB_APRS_Networkname.Text = APRSnetworkName;
                SetTBtext(tb_DB_APRS_Networkname, APRSnetworkName); // 7/26/17
                DatabaseFCCCallsign = Read_Registry("Database FCC Callsign");
                if ((DatabaseFCCCallsign != null) && (DatabaseFCCCallsign != ""))
// 7/26/17                    tb_DB_FCC_Callsign.Text = DatabaseFCCCallsign;
                    SetTBtext(tb_DB_FCC_Callsign, DatabaseFCCCallsign);     // 7/26/17
                DatabaseTacticalCallsign = Read_Registry("Database Tactical Callsign");
                if ((DatabaseTacticalCallsign != null) && (DatabaseTacticalCallsign != ""))
// 7/26/17                    tb_DB_Tactical_Callsign.Text = DatabaseTacticalCallsign;
                    SetTBtext(tb_DB_Tactical_Callsign, DatabaseTacticalCallsign);   // 7/26/17
                TacticalBeaconText = Read_Registry("Database Tactical Beacon Text");
                if ((TacticalBeaconText != null) && (TacticalBeaconText != ""))
// 7/26/17                    tb_DB_Tactical_Beacon_text.Text = TacticalBeaconText;
                    SetTBtext(tb_DB_Tactical_Beacon_text, TacticalBeaconText);  // 7/26/17
                temp = Read_Registry("Use Database Tactical");
                if (temp == "Yes")
                {
// 7/26/17                    chk_DB_Use_Tactical_Callsign.Checked = true;
                    MakeCBChecked(chk_DB_Use_Tactical_Callsign, true);  // 7/26/17
                    Use_Database_TacticalCallsign = true;
                }
                else
                {
// 7/26/17                    chk_DB_Use_Tactical_Callsign.Checked = false;
                    MakeCBChecked(chk_DB_Use_Tactical_Callsign, false); // 7/26/17
                    Use_Database_TacticalCallsign = false;
                }
                chk_DB_Use_Tactical_Callsign_CheckedChanged(null, null);
// 7/26/17                tb_Server_Port_Number.Text = Read_Registry("Mesh Server Port #");
                SetTBtext(tb_Server_Port_Number, Read_Registry("Mesh Server Port #"));  // 7/26/17
                if (Read_Registry("Using RFID") == "Yes")
// 7/26/17                    chk_Using_RFID.Checked = true;
                    MakeCBChecked(chk_Using_RFID, true);    // 7/26/17
                else
// 7/26/17                    chk_Using_RFID.Checked = false;
                    MakeCBChecked(chk_Using_RFID, false);    // 7/26/17
                RFID_Assignments_Filename = Read_Registry("RFID Assignments File");
// 7/26/17                tb_RFID_Assignments_Filename.Text = RFID_Assignments_Filename;
                SetTBtext(tb_RFID_Assignments_Filename, RFID_Assignments_Filename);     // 7/26/17
// 7/14/17 moved up before SelectRole                Stations_Info_Filename = Read_Registry("Stations Info File");
// 7/26/17                tb_Stations_Info_Filename.Text = Stations_Info_Filename;
                SetTBtext(tb_Stations_Info_Filename, Stations_Info_Filename);   // 7/26/17
// 7/26/17                tb_Station_Info_Filename.Text = Stations_Info_Filename;
                SetTBtext(tb_Station_Info_Filename, Stations_Info_Filename);    // 7/26/17
                temp = Read_Registry("Show Lat/Long");
                if (temp == "Yes")
                {
                    Show_LatLong = true;
// 7/26/17                    chk_DB_Show_LatLong.Checked = true;
                    MakeCBChecked(chk_DB_Show_LatLong, true);   // 7/26/17
                }
                else
                {
                    Show_LatLong = false;
// 7/26/17                    chk_DB_Show_LatLong.Checked = false;
                    MakeCBChecked(chk_DB_Show_LatLong, false);   // 7/26/17
                }
                Welcome_Message = Read_Registry("Welcome Message");
// 7/26/17                tb_Welcome_Message.Text = Welcome_Message;
                SetTBtext(tb_Welcome_Message, Welcome_Message);     // 7/26/17
                if (tb_Welcome_Message.TextLength == 0)
                    Welcome_Message_Exists = false;
                else
                    Welcome_Message_Exists = true;
                if (Read_Registry("Highlight Distance covered less than 5mph") == "Yes")
//                    chk_Distance_5mph.Checked = true;
                    MakeCBChecked(chk_Distance_5mph, true);
                else
//                    chk_Distance_5mph.Checked = false;
                    MakeCBChecked(chk_Distance_5mph, false);
                if (Read_Registry("Highlight Staying at Station longer than 10min") == "Yes")
// 7/26/17                    chk_Stay_10min.Checked = true;
                    MakeCBChecked(chk_Stay_10min, true);    // 7/26/17
                else
// 7/26/17                    chk_Stay_10min.Checked = false;
                    MakeCBChecked(chk_Stay_10min, false);   // 7/26/17
                if (Read_Registry("Highlight Arrive/Leave after Station Cuttoff") == "Yes")
// 7/26/17                    chk_After_Cutoff.Checked = true;
                    MakeCBChecked(chk_After_Cutoff, true);      // 7/26/17
                else
// 7/26/17                    chk_After_Cutoff.Checked = false;
                    MakeCBChecked(chk_After_Cutoff, false);      // 7/26/17

                // get the Directory from SelectRole
                if (DataDirectory != SelectRole.DataDirectory)
                {
                    Save_Registry("Data Directory", SelectRole.DataDirectory);   // this is a new directory, new files will be created
                    DataDirectory = SelectRole.DataDirectory;
                }
// 7/26/17                tb_DB_Settings_Data_Directory.Text = DataDirectory;
                SetTBtext(tb_DB_Settings_Data_Directory, DataDirectory);    // 7/26/17

                // get the Connection Types Expected from SelectRole
                if (ConnectViaEthernet != SelectRole.ConnectViaEthernet)
                {
                    ConnectViaEthernet = SelectRole.ConnectViaEthernet;
                    if (ConnectViaEthernet)
                        Save_Registry("Connect via Ethernet", "Yes");
                    else
                        Save_Registry("Connect via Ethernet", "No");
                }
                if (ConnectViaPacket != SelectRole.ConnectViaPacket)
                {
                    ConnectViaPacket = SelectRole.ConnectViaPacket;
                    if (ConnectViaPacket)
                        Save_Registry("Connect via Packet", "Yes");
                    else
                        Save_Registry("Connect via Packet", "No");
                }
                if (ConnectViaAPRS != SelectRole.ConnectViaAPRS)
                {
                    ConnectViaAPRS = SelectRole.ConnectViaAPRS;
                    if (ConnectViaAPRS)
                        Save_Registry("Connect via APRS", "Yes");
                    else
                        Save_Registry("Connect via APRS", "No");
                }

                // Output Message Queue - added 7/7/19
                temp = Read_Registry("Ethernet response (sec)");
                if ((temp == null) || (temp == ""))
                    SetTBtext(tb_DB_Out_Mess_Q_Eth_sec, "0");
                else
                    SetTBtext(tb_DB_Out_Mess_Q_Eth_sec, temp);
                temp = Read_Registry("APRS/Packet response (sec)");
                if ((temp == null) || (temp == ""))
                    SetTBtext(tb_DB_Out_Mess_Q_Eth_sec, "0");
                else
                    SetTBtext(tb_DB_Out_Mess_Q_APRS_sec, temp);
                Ethernet_Response_Time = Convert.ToInt16(GetTBtext(tb_DB_Out_Mess_Q_Eth_sec));
                APRS_Packet_Response_Time = Convert.ToInt16(GetTBtext(tb_DB_Out_Mess_Q_APRS_sec));

                Init_Registry = false;
                #endregion

                // load Info file
                InfoFilePath = DataDirectory + "\\Race Info.txt";
                InfoLoaded = Load_Info(InfoFilePath, true);

                // load the Station Info file - moved here 7/22/17
                dgv_Stations.AutoGenerateColumns = true;
                dgv_Stations.RowHeadersVisible = false;
                btn_Load_Station_Info_Filename_Click(null, null);

                // load Runner list
                RunnerListPath = DataDirectory + "\\Runner List.txt";

                // load the Runner data from an existing data file - moved here 7/22/17
                DataFile = new DataFile();
                if ((SelectRole.RDFilename != null) && (SelectRole.RDFilename != ""))
                {
// 7/26/17                    tb_DB_Settings_RunnersDataFile.Text = SelectRole.RDFilename;
                    SetTBtext(tb_DB_Settings_RunnersDataFile, SelectRole.RDFilename);   // 7/26/17
                    DataFile.Load_Runner_Data(SelectRole.RDFilename);
                    OK_to_start_IP_Server = true;   // 4/4/17 - why is this here?
                }
                //                DataFile.Load_Info(SelectRole.RDFilename);      // remove for testing
                else
                {       // if not loading the Datafile data, do the original Runners List load - 7/22/17
                    if (Load_Runner_List(RunnerListPath, true))
// 7/26/17                        tb_Upload_Runner_List_Path.Text = RunnerListPath;
                        SetTBtext(tb_Upload_Runner_List_Path, RunnerListPath);  // 7/26/17
                    if (RunnerList.Count == 0)
                        RunnerList_Has_Entries = false;
                    else
                        RunnerList_Has_Entries = true;
                }

                // load DNS list
                DNSListPath = DataDirectory + "\\DNSlist.txt";
                if (Load_DNS(DNSListPath, true))
// 7/26/17                    tb_Upload_DNS_Path.Text = DNSListPath;
                    SetTBtext(tb_Upload_DNS_Path, DNSListPath);     // 7/26/17
                if (lb_DB_DNS.Items.Count == 0)
                {
                    DNSList_Has_Entries = false;
// 7/26/17                    btn_Clear_DNS.Visible = false;
                    MakeVisible(btn_Clear_DNS, false);  // 7/26/17
                }
                else
                {
                    DNSList_Has_Entries = true;
// 7/26/17                    btn_Clear_DNS.Visible = true;
                    MakeVisible(btn_Clear_DNS, true);   // 7/26/17
                }

                // load DNF list
                DNFListPath = DataDirectory + "\\DNFlist.txt";
                if (Load_DNF(DNFListPath, true))
// 7/26/17                    tb_DNF_Upload_Path.Text = DNFListPath;
                    SetTBtext(tb_DNF_Upload_Path, DNFListPath);     // 7/26/17
                if (DNFList.Count == 0)
                {
                    DNFList_Has_Entries = false;
// 7/26/17                    btn_Clear_DNF.Visible = false;
                    MakeVisible(btn_Clear_DNF, false);  // 7/26/17
                }
                else
                {
                    DNFList_Has_Entries = true;
// 7/26/17                    btn_Clear_DNF.Visible = true;
                    MakeVisible(btn_Clear_DNF, true);   // 7/26/17
                }

                // load Watch list
                WatchListPath = DataDirectory + "\\Watchlist.txt";
                if (Load_Watch(WatchListPath, true))
// 7/26/17                    tb_Watch_Upload_Path.Text = WatchListPath;
                    SetTBtext(tb_Watch_Upload_Path, WatchListPath);     // 7/26/17
                if (WatchList.Count == 0)
                {
                    WatchList_Has_Entries = false;
// 7/26/17                    btn_Clear_Watch.Visible = false;
                    MakeVisible(btn_Clear_Watch, false);    // 7/26/17
                }
                else
                {
                    WatchList_Has_Entries = true;
// 7/26/17                    btn_Clear_Watch.Visible = true;
                    MakeVisible(btn_Clear_Watch, true);     // 7/26/17
                }

                // load Issues files
                DBIssuesFilePath = DataDirectory + "\\Database Issues.txt";
                DBIssuesLoaded = Load_DB_Issues(DBIssuesFilePath, true);
                if (DBIssues.Count == 0)
                    DBIssues_Has_Entries = false;
                else
                    DBIssues_Has_Entries = true;
                ASIssuesFilePath = DataDirectory + "\\AidStation Issues.txt";
                ASIssuesLoaded = Load_AS_Issues(ASIssuesFilePath, true);
                if (ASIssues.Count == 0)
                    ASIssues_Has_Entries = false;
                else
                    ASIssues_Has_Entries = true;

                // load the RFID assignments list
                Load_RFID_Assignments(tb_RFID_Assignments_Filename.Text);

//// 7/22/17                // load the Station Info file
//                dgv_Stations.AutoGenerateColumns = true;
//                dgv_Stations.RowHeadersVisible = false;
//                btn_Load_Station_Info_Filename_Click(null, null);

//// 7/22/17                // load the Runner data from an existing data file
//                DataFile = new DataFile();
//                if ((SelectRole.RDFilename != null) && (SelectRole.RDFilename != ""))
//                {
//                    tb_DB_Settings_RunnersDataFile.Text = SelectRole.RDFilename;
//                    DataFile.Load_Runner_Data(SelectRole.RDFilename);
//                    OK_to_start_IP_Server = true;   // 4/4/17 - why is this here?
//                }
////                DataFile.Load_Info(SelectRole.RDFilename);      // remove for testing

                // start a Log File
                CreateLogFile(DBRole);

                // Start the IP Server - if expecting Ethernet (MESH) connections
                if (ConnectViaEthernet)
                {
// 4/4/17                    if (OK_to_start_IP_Server)      // 4/4/17 - why is this here?
                        Find_This_PC_IP_Address();
                    MakeEnabled(gb_Ethernet_Mesh, true);
                }
                else
                {
                    MakeEnabled(gb_Ethernet_Mesh, false);
                    MakeVisible(lbl_My_IP_Address, false);
                    MakeVisible(tb_My_IP_Address, false);
                }

                // Start AGWPE if it is available and needed
                if (ConnectViaPacket || ConnectViaAPRS)
                {
                    if (ConnectViaPacket)
                        DB_AGWSocket = new DB_AGWSocket(rtb_DB_Packet_Packets, DBRole);
                    if (ConnectViaAPRS)
                        DB_AGWSocket = new DB_AGWSocket(rtb_APRS_Packets_Received_DB, DBRole);
                    Thread DBAGWRcvThread = new Thread(DB_AGWSocket.DB_AGWSocket_Receive_Thread);
                    AGW_Count = 5;      // wait 5 seconds after AGWPE finishes Initting to get the settings
                    DB_AGWSocket.Initting = lbl_DB_Initting;    // 7/27/17
                    DB_AGWSocket.AGWPE_Connected = lbl_DB_Connected_to_AGWPE;   // 7/27/17
                    DB_AGWSocket.InitAGWPE(true);
                    DBAGWRcvThread.Start();
                }

                // make the Database tabcontrol visible
//// 7/26/17                tabControl_DB_Main.Visible = true;
//                panel_DB_Top.Visible = true;
                MakeVisible(tabControl_DB_Main,true);       // 7/26/17
                MakeVisible(panel_DB_Top, true);        // 7/26/17
                Labels_TabPages_DB_Connections();
                this.Text = "Runner Tracker Database";

                //// start a Log File
                //CreateLogFile(DBRole);

                // display initial AGWPE Statistics
                DisplayDBAGWPEportStats(AGWPEPortStatistics);
                #endregion
            }
            else    // otherwise, start the Aid Station Form1 actions
            {
                #region Aid Station Form1 actions
                _last = (Control)tb_Aid_original_focus;

                AddRichText(rtb_Aid_Packet_Node_Packets, "Starting the Aid Station role" + Environment.NewLine, Color.Green);
                AddRichText(rtb_Aid_Ethernet_Packets, "Starting the Aid Station role" + Environment.NewLine, Color.Green);

                // init all Lists and Queues
                Runners = new List<Aid_Runner>();
                RunnersAtStation = new List<Aid_Runner>();
                RunnersIn = new List<Aid_Runner>();
                RunnersOut = new List<Aid_Runner>();
                RunnersStatus = new List<RunnerStatus>();
                Aid_Stations = new List<Aid_Station>();
                RFIDAssignments = new List<RFID>();
                Outgoing_Messages = new List<Out_Message>();
                Incoming_Messages = new List<In_Message>();
                RunnerList = new List<RunnersList>();
                Aid_DNFList = new List<Aid_RunnerDNFWatch>();
                Aid_WatchList = new List<Aid_RunnerDNFWatch>();
                ASIssues = new List<Aid_Issue>();
                Reconciled_Runners = new List<Runner_Times>();  // 5/5/19
                Runners_thru_Station = new List<Runner_Times>();    // 5/5/19
                CommandQue = new Queue<Command>();
//                Buttons_to_Push = new Queue<Queue_Buttons>();
                Buttons_to_Push = new Queue<Button_to_Push>();
                AGW_Count = 0;
                NewAGWPEpacketRcvd = false;

                // Tab Header color
                foreach (TabPage tp in tabControl_Main_Aid.TabPages)
                    SetTabHeader_Aid(tp, Color.FromKnownColor(KnownColor.Control));
                StationC = Color.FromKnownColor(KnownColor.Control);
                SettingsC = Color.FromKnownColor(KnownColor.Control);
                this.tabControl_Main_Aid.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl_Main_Aid_DrawItem);

                #region Timers
                // start the Elapsed Timers
                Elapsed1minTimer = new System.Timers.Timer();
                Elapsed1minTimer.AutoReset = true;
                Elapsed1minTimer.Interval = 60000;   // 1 minute
                Elapsed1minTimer.Elapsed += new ElapsedEventHandler(Elapsed1minHandler);
                Elapsed1minTimer.Start();
                Elapsed30secTimer = new System.Timers.Timer();
                Elapsed30secTimer.AutoReset = true;
                Elapsed30secTimer.Interval = 30000;   // 30 seconds
                Elapsed30secTimer.Elapsed += new ElapsedEventHandler(Elapsed30secHandler);
                Elapsed30secTimer.Start();
                Elapsed5secTimer = new System.Timers.Timer();
                Elapsed5secTimer.AutoReset = true;
                Elapsed5secTimer.Interval = 5000;   // 5 second
                Elapsed5secTimer.Elapsed += new ElapsedEventHandler(Elapsed5secHandler);
                Elapsed5secTimer.Start();
                Elapsed1secTimer = new System.Timers.Timer();
                Elapsed1secTimer.AutoReset = true;
                Elapsed1secTimer.Interval = 1000;   // 1 second
                Elapsed1secTimer.Elapsed += new ElapsedEventHandler(Elapsed_Aid_1secHandler);
                Elapsed1secTimer.Start();
                APRSconnect30secTimer = new System.Timers.Timer();
                APRSconnect30secTimer.AutoReset = false;
                APRSconnect30secTimer.Interval = 30000;   // 30 seconds
                APRSconnect30secTimer.Elapsed += new ElapsedEventHandler(APRSconnect30secTimerHandler);
                APRSconnect2minTimer = new System.Timers.Timer();
                APRSconnect2minTimer.AutoReset = false;
                APRSconnect2minTimer.Interval = 120000;   // 2 minutes
                APRSconnect2minTimer.Elapsed += new ElapsedEventHandler(APRSconnect2minTimerHandler);

                // start a timer to look at its tabs to see if any errors have occurred
                DictTimer = new System.Timers.Timer();
                DictTimer.Interval = 500;      // set for 1/2 seconds
                DictTimer.AutoReset = true;    // repeat
                DictTimer.Elapsed += new System.Timers.ElapsedEventHandler(Aid_DictTimeEvent);
                DictTimer.Start();
                #endregion

                #region COM ports
                // get the list of COM ports available - creating the ports array
                string[] PCports = System.IO.Ports.SerialPort.GetPortNames();
                ports = new Port[PCports.Length + 1];        // make same size as the number of ports available + 1 for 'none'
                for (int i = 0; i < PCports.Length; i++)
                {
                    ports[i] = new Port();
                    ports[i].COMport = PCports[i];
                    ports[i].used = false;
                }

                // fill in the COM port combo boxes
                cmb_Incoming.Items.Add(" none");
                cmb_Outgoing.Items.Add(" none");
                for (int i = 0; i < PCports.Length; i++)
                {
                    cmb_Incoming.Items.Add(ports[i].COMport);
                    cmb_Outgoing.Items.Add(ports[i].COMport);
                }

                // preselect 'none' choice and set Red
                cmb_Incoming.SelectedIndex = 0;
                cmb_Incoming.BackColor = Color.FromArgb(255, 128, 128);
                cmb_Incoming.ForeColor = Color.White;
                cmb_Outgoing.SelectedIndex = 0;
                cmb_Outgoing.BackColor = Color.FromArgb(255, 128, 128);
                cmb_Outgoing.ForeColor = Color.White;
                #endregion

                // initialize for the SparkFun RFID Reader
                StartOfRFIDstring = (char)0x2;
                EndOfRFIDstring = (char)0x3;

                #region Read Registry and fill textboxes
                // read the Registry
                Init_Registry = true;

                if (DBRole != SelectRole.DatabaseRole)     // first save the Role, if it has changed
                {
                    DBRole = SelectRole.DatabaseRole;
                    if (DBRole)
                        Save_Registry("Program Role", "Database");
                    else
                        Save_Registry("Program Role", "Aid Station");
                }
                //                string temp = Read_Registry("AGWPE Radio Port");
                temp = Read_Registry("AGWPE Radio Port");
                if (temp != "")
                    AGWPERadioPort = Convert.ToInt16(temp);
                temp = Read_Registry("Remove from List");
                lb_Remove_from_Station_List.SelectedItem = temp;
                Remove_from_Runners_list_Minutes = Convert.ToInt16(temp);
                AGWPEServerName = Read_Registry("AGWPE Server Name");
// 7/26/17                tb_DB_AGWPEServer.Text = AGWPEServerName;
// 7/26/17                tb_Aid_AGWPEServer.Text = AGWPEServerName;
                SetTBtext(tb_Aid_AGWPEServer, AGWPEServerName);     // 7/26/17
                AGWPEServerPort = Read_Registry("AGWPE Server Port");
// 7/26/17                tb_DB_AGWPEPort.Text = AGWPEServerPort;
// 7/26/17                tb_Aid_AGWPEPort.Text = AGWPEServerPort;
                SetTBtext(tb_Aid_AGWPEPort, AGWPEServerPort);   // 7/26/17
                temp = Read_Registry("APRS Network packets only");
                if (temp == "Yes")
                {
                    APRSnetworkOnly = true;
// 7/26/17                    rb_DB_Only_Network_APRS_Packets.Checked = true;
                    MakeRBChecked(rb_DB_Only_Network_APRS_Packets, true);   // 7/26/17
                }
                else
                {
                    APRSnetworkOnly = false;
// 7/26/17                    rb_DB_All_APRS_Packets.Checked = true;
                    MakeRBChecked(rb_DB_Only_Network_APRS_Packets, false);   // 7/26/17
                }
// 7/26/17                tb_NumAPRSnetwork_DB.Text = NumAPRSnetworklines.ToString();
                SetTBtext(tb_NumAPRSnetwork_DB, NumAPRSnetworklines.ToString());    // 7/26/17
// 7/26/17                tb_NumAPRSlines_DB.Text = NumAPRSlines.ToString();
                SetTBtext(tb_NumAPRSlines_DB, NumAPRSlines.ToString());     // 7/26/17
                APRSnetworkName = Read_Registry("APRS Network Name");
// 7/26/17                tb_APRS_Networkname.Text = APRSnetworkName;
                SetTBtext(tb_APRS_Networkname, APRSnetworkName);   // 7/26/17
                StationFCCCallsign = Read_Registry("Station FCC Callsign");
// 7/26/17                tb_AidStation_FCC_Callsign.Text = StationFCCCallsign;
                SetTBtext(tb_AidStation_FCC_Callsign, StationFCCCallsign);      // 7/26/17
                DatabaseFCCCallsign = Read_Registry("Database FCC Callsign");
// 7/26/17                tb_Database_FCC_Callsign.Text = DatabaseFCCCallsign;
                SetTBtext(tb_Database_FCC_Callsign, DatabaseFCCCallsign);   // 7/26/17
                DatabaseTacticalCallsign = Read_Registry("Database Tactical Callsign");
// 7/26/17                tb_Database_Tactical_Callsign.Text = DatabaseTacticalCallsign;
                SetTBtext(tb_Database_Tactical_Callsign, DatabaseTacticalCallsign);     // 7/26/17
                StationTacticalCallsign = Read_Registry("Station Tactical Callsign");
// 7/26/17                tb_AidStation_Tactical_Callsign.Text = StationTacticalCallsign;
                SetTBtext(tb_AidStation_Tactical_Callsign, StationTacticalCallsign);    // 7/26/17
                temp = Read_Registry("Use Station Tactical");
                if (temp == "Yes")
                {
// 7/26/17                    chk_Use_Station_Tactical_Callsign.Checked = true;
                    MakeCBChecked(chk_Use_Station_Tactical_Callsign, true); // 7/26/17
                    Use_Station_TacticalCallsign = true;
                }
                else
                {
// 7/26/17                    chk_Use_Station_Tactical_Callsign.Checked = false;
                    MakeCBChecked(chk_Use_Station_Tactical_Callsign, false); // 7/26/17
                    Use_Station_TacticalCallsign = false;
                }
                chk_Use_Station_Tactical_Callsign_CheckedChanged(null, null);
                temp = Read_Registry("Use Database Tactical");
                if (temp == "Yes")
                {
// 7/26/17                    chk_Use_Database_Tactical_Callsign.Checked = true;
                    MakeCBChecked(chk_Use_Database_Tactical_Callsign, true);    // 7/26/17
                    Use_Database_TacticalCallsign = true;
                }
                else
                {
// 7/26/17                    chk_Use_Database_Tactical_Callsign.Checked = false;
                    MakeCBChecked(chk_Use_Database_Tactical_Callsign, false);    // 7/26/17
                    Use_Database_TacticalCallsign = false;
                }
                chk_Use_Database_Tactical_Callsign_CheckedChanged(null, null);
                TacticalBeaconText = Read_Registry("Station Tactical Beacon Text");
                if ((TacticalBeaconText != null) && (TacticalBeaconText != ""))
// 7/26/17                    tb_AidStation_Tactical_Beacon_Text.Text = TacticalBeaconText;
                    SetTBtext(tb_AidStation_Tactical_Beacon_Text, TacticalBeaconText);  // 7/26/17
                PacketNodeCallsign = Read_Registry("Packet Node Callsign");
// 7/26/17                tb_Packet_Node_Callsign.Text = PacketNodeCallsign;
                SetTBtext(tb_Packet_Node_Callsign, PacketNodeCallsign);     // 7/26/17
                temp = Read_Registry("Packet Connect");
                switch (temp)
                {
                    default:
                    case "Direct":
// 7/26/17                        rb_Connect_Packet_Direct.Checked = true;
// 7/26/17                        rb_Connect_Packet_Via_Node.Checked = false;
// 7/26/17                        rb_Connect_Packet_Via_String.Checked = false;
                        MakeRBChecked(rb_Connect_Packet_Direct, true);     // 7/26/17
                        MakeRBChecked(rb_Connect_Packet_Via_Node, false);   // 7/26/17
                        MakeRBChecked(rb_Connect_Packet_Via_String, false); // 7/26/17
                        Packet_Connect_Mode = Packet_Connect_Method.Direct;
                        break;
                    case "Node":
// 7/26/17                        rb_Connect_Packet_Direct.Checked = false;
// 7/26/17                        rb_Connect_Packet_Via_Node.Checked = true;
// 7/26/17                        rb_Connect_Packet_Via_String.Checked = false;
                        MakeRBChecked(rb_Connect_Packet_Direct, false);     // 7/26/17
                        MakeRBChecked(rb_Connect_Packet_Via_Node, true);   // 7/26/17
                        MakeRBChecked(rb_Connect_Packet_Via_String, false); // 7/26/17
                        Packet_Connect_Mode = Packet_Connect_Method.Node;
                        break;
                    case "Via":
// 7/26/17                        rb_Connect_Packet_Direct.Checked = false;
// 7/26/17                        rb_Connect_Packet_Via_Node.Checked = false;
// 7/26/17                        rb_Connect_Packet_Via_String.Checked = true;
                        MakeRBChecked(rb_Connect_Packet_Direct, false);     // 7/26/17
                        MakeRBChecked(rb_Connect_Packet_Via_Node, false);   // 7/26/17
                        MakeRBChecked(rb_Connect_Packet_Via_String, true); // 7/26/17
                        Packet_Connect_Mode = Packet_Connect_Method.ViaString;
                        break;
                }
                Longitude = Read_Registry("Longitude");
// 7/26/17                tb_APRS_Longitude.Text = Longitude;
                SetTBtext(tb_APRS_Longitude, Longitude);    // 7/26/17
                Latitude = Read_Registry("Latitude");
// 7/26/17                tb_APRS_Latitude.Text = Latitude;
                SetTBtext(tb_APRS_Latitude, Latitude);  // 7/26/17
                VIAstring = Read_Registry("VIA string");
// 7/26/17                tb_AGWPE_ViaString.Text = VIAstring;
                SetTBtext(tb_AGWPE_ViaString, VIAstring);   // 7/26/17
// 7/26/17                tb_Station_Name.Text = Station_Name;
                SetTBtext(tb_Station_Name, Station_Name);   // 7/26/17
// 7/26/17                tb_Station_Name_Settings.Text = Station_Name;
                SetTBtext(tb_Station_Name_Settings, Station_Name);  // 7/26/17
// 7/14/17 moved up before SelectRole                Stations_Info_Filename = Read_Registry("Stations Info File");
// 7/26/17                tb_Station_Info_Filename.Text = Stations_Info_Filename;
                SetTBtext(tb_Station_Info_Filename, Stations_Info_Filename);    // 7/26/17
// 7/26/17                tb_Stations_Info_Filename.Text = Stations_Info_Filename;
                SetTBtext(tb_Stations_Info_Filename, Stations_Info_Filename);    // 7/26/17
// 7/26/17                tb_Aid_Station_Info_Filename.Text = Stations_Info_Filename;
                SetTBtext(tb_Aid_Station_Info_Filename, Stations_Info_Filename);    // 7/26/17
                temp = Read_Registry("Show Lat/Long");
                if (temp == "Yes")
                {
                    Show_LatLong = true;
// 7/26/17                    chk_Aid_Show_LatLong.Checked = true;
                    MakeCBChecked(chk_Aid_Show_LatLong, true);  // 7/26/17
                }
                else
                {
                    Show_LatLong = false;
// 7/26/17                    chk_Aid_Show_LatLong.Checked = false;
                    MakeCBChecked(chk_Aid_Show_LatLong, false);  // 7/26/17
                }
                RFID_Assignments_Filename = Read_Registry("RFID Assignments File");
// 7/26/17                tb_RFIDnumber_Assignment_file.Text = RFID_Assignments_Filename;
                SetTBtext(tb_RFIDnumber_Assignment_file, RFID_Assignments_Filename);    // 7/26/17
                Load_RFID_Assignments(RFID_Assignments_Filename);
                cmb_Incoming.SelectedItem = Read_Registry("Incoming RFID COM port");
                cmb_Outgoing.SelectedItem = Read_Registry("Outgoing RFID COM port");
// 7/26/17                tb_Aid_Mesh_IP_address.Text = Read_Registry("Mesh IP Address");
                SetTBtext(tb_Aid_Mesh_IP_address, Read_Registry("Mesh IP Address"));    // 7/26/17
// 7/26/17                tb_Aid_Server_Port_Number.Text = Read_Registry("Mesh Server Port #");
                SetTBtext(tb_Aid_Server_Port_Number, Read_Registry("Mesh Server Port #"));  // 7/26/17
                temp = Read_Registry("Mesh Auto Reconnect");
                if (temp == "Yes")
// 7/26/17                    cb_Server_Auto_Reconnect.Checked = true;
                    MakeCBChecked(cb_Server_Auto_Reconnect, true);  // 7/26/17
                else
// 7/26/17                    cb_Server_Auto_Reconnect.Checked = false;
                    MakeCBChecked(cb_Server_Auto_Reconnect, false);  // 7/26/17
                temp = Read_Registry("Number of RFID Readers");
                if (temp == "1")
// 7/26/17                    rb_Two_Readers.Checked = false;
                    MakeRBChecked(rb_Two_Readers, false);   // 7/26/17
                else
// 7/26/17                    rb_Two_Readers.Checked = true;
                    MakeRBChecked(rb_Two_Readers, true);   // 7/26/17
                temp = Read_Registry("Auto Send Update");
                if (temp == "Yes")
// 7/26/17                    cb_Auto_Send_Runner.Checked = true;
                    MakeCBChecked(cb_Auto_Send_Runner, true);   // 7/26/17
                else
// 7/26/17                    cb_Auto_Send_Runner.Checked = false;
                    MakeCBChecked(cb_Auto_Send_Runner, false);   // 7/26/17
                temp = Read_Registry("Auto Send Time");
                lb_Auto_Send_Minutes.SelectedItem = temp;
                Auto_Send_Minutes = Convert.ToInt16(temp);

                // make any changes from the SelectRole Form
                if (DataDirectory != SelectRole.DataDirectory)
                {
                    Save_Registry("Data Directory", SelectRole.DataDirectory);   // this is a new directory, new files will be created
                    DataDirectory = SelectRole.DataDirectory;
                }
// 7/26/17                tb_Data_Directory.Text = DataDirectory;
                SetTBtext(tb_Data_Directory, DataDirectory);    // 7/26/17
// 7/26/17                if (SelectRole.StationName != tb_Station_Name.Text)
                if (SelectRole.StationName != GetTBtext(tb_Station_Name))       // 7/26/17
                {
// 7/26/17                    tb_Station_Name.Text = SelectRole.StationName;
                    SetTBtext(tb_Station_Name, SelectRole.StationName);     // 7/26/17
// 7/26/17                    tb_Station_Name_Settings.Text = SelectRole.StationName;
                    SetTBtext(tb_Station_Name_Settings, SelectRole.StationName);    // 7/26/17
                    Save_Registry("Station Name", SelectRole.StationName);
                    Station_Name = SelectRole.StationName;      // 7/13/17
                }
                if (SelectRole.Connection_Type != Connection_Type)
                {
                    switch (SelectRole.Connection_Type)
                    {
                        case Connect_Medium.APRS:
                            Save_Registry("Connection Type", "APRS");
                            break;
                        case Connect_Medium.Packet:
                            Save_Registry("Connection Type", "Packet");
                            break;
                        case Connect_Medium.Ethernet:
                            Save_Registry("Connection Type", "Ethernet");
                            break;
                        case Connect_Medium.Cellphone:
                            Save_Registry("Connection Type", "Cellphone");
                            break;
                    }
                    Connection_Type = SelectRole.Connection_Type;
                }
                switch (Connection_Type)
                {
                    case Connect_Medium.APRS:
                        rb_Use_APRS.Checked = true;

                        // set the APRS list loading checkboxes
                        temp = Read_Registry("Load APRS Runners");
                        if (temp == "Yes")
                            MakeCBChecked(chk_Runner_List, true);
                        else
                            MakeCBChecked(chk_Runner_List, false);
                        temp = Read_Registry("Load APRS DNS");
                        if (temp == "Yes")
                            MakeCBChecked(chk_DNS_List, true);
                        else
                            MakeCBChecked(chk_DNS_List, false);
                        temp = Read_Registry("Load APRS DNF");
                        if (temp == "Yes")
                            MakeCBChecked(chk_DNF_List, true);
                        else
                            MakeCBChecked(chk_DNF_List, false);
                        temp = Read_Registry("Load APRS Watch");
                        if (temp == "Yes")
                            MakeCBChecked(chk_Watch_List, true);
                        else
                            MakeCBChecked(chk_Watch_List, false);
                        temp = Read_Registry("Load APRS Info");
                        if (temp == "Yes")
                            MakeCBChecked(chk_Info, true);
                        else
                            MakeCBChecked(chk_Info, false);

// Also need to add Saving the default value earlier.

                        break;
                    case Connect_Medium.Packet:
                        rb_Use_Packet.Checked = true;
                        break;
                    case Connect_Medium.Ethernet:
                        rb_Use_Ethernet.Checked = true;
                        break;
                    case Connect_Medium.Cellphone:
                        break;
                }
                if (UsingRFID != SelectRole.UsingRFID)
                {
                    if (SelectRole.UsingRFID)
                    {
                        Save_Registry("Using RFID", "Yes");
                    }
                    else
                    {
                        Save_Registry("Using RFID", "No");
                    }
                    UsingRFID = SelectRole.UsingRFID;
                }
                if (UsingRFID)
// 7/26/17                    cb_Use_RFID_Readers.Checked = true;
                    MakeCBChecked(cb_Use_RFID_Readers, true);   // 7/26/17
                else
// 7/26/17                    cb_Use_RFID_Readers.Checked = false;
                    MakeCBChecked(cb_Use_RFID_Readers, false);   // 7/26/17

                if (NumLogPts != SelectRole.NumLogPts)
                {
                    if (SelectRole.NumLogPts == 1)
                    {
                        Save_Registry("Number of Log Points", "1");
                    }
                    else
                    {
                        Save_Registry("Number of Log Points", "2");
                    }
                    NumLogPts = SelectRole.NumLogPts;
                }
                if (NumLogPts == 1)
                {
// 7/26/17                    rb_One_Entry_Point.Checked = true;
// 7/26/17                    rb_One_Log_Point.Checked = true;
                    MakeRBChecked(rb_One_Entry_Point, true);    // 7/26/17
                    MakeRBChecked(rb_One_Log_Point, true);    // 7/26/17
                }
                else
                {
// 7/26/17                    rb_In_and_Out_Entry.Checked = true;
// 7/26/17                    rb_Two_Log_Pts.Checked = true;
                    MakeRBChecked(rb_In_and_Out_Entry, true);    // 7/26/17
                    MakeRBChecked(rb_Two_Log_Pts, true);    // 7/26/17
                }
                Init_Registry = false;
                #endregion

                // load the Station Info file
                Aid_dgv_Stations.AutoGenerateColumns = true;
                Aid_dgv_Stations.RowHeadersVisible = false;
                dgv_Incoming_Messages.AutoGenerateColumns = true;    // also initialize the Message DGVs
                dgv_Incoming_Messages.RowHeadersVisible = false;
                dgv_Outgoing_Messages.AutoGenerateColumns = true;
                dgv_Outgoing_Messages.RowHeadersVisible = false;
                if (Stations_Info_Filename != "")       // 8/4/17
                    btn_Load_Aid_Station_Info_Filename_Click(null, null);
                Test_Station_Name();        // 7/21/17

                // load the Issues
                IssuesFilePath = DataDirectory + "\\AidStation Issues.txt";    // init to load the Issues file
                IssuesLoaded = Load_Issues(IssuesFilePath, true);

                // start a Log File
                CreateLogFile(DBRole);

                // Start AGWPE if it is available and needed
                if ((Connection_Type == Connect_Medium.Packet) || (Connection_Type == Connect_Medium.APRS))
                {
                    if (Connection_Type == Connect_Medium.Packet)
                    {
                        AddRichText(rtb_APRS_Packets_Received_Aid, "Starting Aid Station with Packet" + Environment.NewLine, Color.Fuchsia);
// 5/1/17                        Aid_AGWSocket = new Aid_AGWSocket(rtb_Aid_Packet_Node_Packets, DBRole);
                        Aid_AGWSocket = new Aid_AGWSocket(rtb_Aid_Packet_Node_Packets, false);
                    }
                    else
                    {
                        AddRichText(rtb_APRS_Packets_Received_Aid, "Starting Aid Station with APRS" + Environment.NewLine, Color.Fuchsia);
// 5/1/17                        Aid_AGWSocket = new Aid_AGWSocket(rtb_APRS_Packets_Received, DBRole);
                        Aid_AGWSocket = new Aid_AGWSocket(rtb_APRS_Packets_Received_Aid, true);
                    }
                    Aid_AGWSocket.Connect_Button = btn_AGWPE_Connect_DB;
                    Aid_AGWSocket.Disconnect_Button = btn_Aid_AGWPE_Disconnect;
                    MakeVisible(btn_Aid_Download_Bib_Only_from_DB, true);       // 8/10/17
                }

                // start the Worker thread
                WorkerObject.Connection_Type = Connection_Type;
                WorkerObject.New_Connection_Type = Connection_Type; // set to the same for now
//// 7/22/16                if (Connection_Type == Connect_Medium.APRS)
//                    lbl_Connected_Central_Database.Text = "Connected to AGWPE";
                WorkerObject.Server_Initting = lbl_Initting;
                WorkerObject.Server_Connected = lbl_Connected_Central_Database;
                WorkerObject.Server_Connected_Active = lbl_Connected_Active;
                WorkerObject.Cannot_Connect = lbl_Cannot_Connect;
                WorkerObject.AGWPE_Connected = lbl_Connected_to_AGWPE;
                WorkerObject.Error_Connecting = lbl_Error_Connecting;
                WorkerObject.Lost_Connection = lbl_Lost_Connection;
                WorkerObject.Server_IP_Address = tb_Aid_Mesh_IP_address.Text;
// 7/27/16                if (tb_Aid_Server_Port_Number.Text == "")
                if (GetTBtext(tb_Aid_Server_Port_Number) == "")     // 7/27/17
                    WorkerObject.Server_Port_Number = 0;
                else
                {
                    try
                    {
                        WorkerObject.Server_Port_Number = Convert.ToInt16(tb_Aid_Server_Port_Number.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Server Port # is Invalid!", "Server Port # Invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        WorkerObject.Server_Port_Number = 0;
                    }
                }
                WorkerObject.Server_Error_Message = tb_Aid_Server_Error_Message;
                WorkerObject.Connect_Button = btn_Connect_to_Mesh_Server;
                WorkerObject.AGWPE_Connect = btn_AGWPE_Connect_DB;
                WorkerObject.AGWPE_Start = btn_Aid_AGWPE_Start_Refresh;
                WorkerObject.Server_Attempting_Connection = lbl_Attempting_Connection;
// 7/22/16                WorkerObject.Connected_to_DB = Connected_to_Server;
                WorkerObject.Station_Name = tb_Station_Name.Text;
                WorkerObject.Ethernet_Packets = rtb_Aid_Ethernet_Packets;
                WorkerObject.Packet_Packets = rtb_Aid_Packet_Node_Packets;
// 7/21/16                Prep_WorkerObject();
                WorkerConnectSendThread = new Thread(new ParameterizedThreadStart(WorkerObject.Aid_Worker_Connect_and_Send_Thread));
                WorkerReceiveThread = new Thread(new ParameterizedThreadStart(WorkerObject.Aid_Worker_Receive_Thread));

                // added 3/18/16
                WorkerConnectSendThread.Start();
                Console.WriteLine("Starting Station Worker Connect/Send thread...");
                WorkerReceiveThread.Start();
                Console.WriteLine("Starting Station Worker Receive thread...");

                // determine which Connection tab pages should be displayed
                if (!UsingRFID)     // should the RFID tab be displayed?
                    tabControl_Main_Aid.TabPages.Remove(tabPage_RFID_Reading);  // no - remove it
                Labels_TabPages_Aid_Connections();      // this will cause connection attempts for Ethernet and to AGWPE

                //// 7/18/16                // change to Initted state
                //                ChangeState(Server_State.Initted);

                // test if we should try to connect
//// 7/21/16                if ((tb_Aid_Server_Port_Number.Text == "") || (tb_Aid_Mesh_IP_address.Text == ""))
//                    ChangeState(Server_State.Cannot_Connect);
//                else
//                {
                    if (Server_Mesh_Auto_Reconnect & (Connection_Type == Connect_Medium.Ethernet))
                        IP_Server_Connect();
// 7/21/16                }

                // if connecting by Ethernet
                if (Connection_Type == Connect_Medium.Ethernet) // 3/12/19
                    Find_This_PC_IP_Address();  // 3/12/19

                // make the Aid Station tabcontrol visible
//// 7/26/17                tabControl_Main_Aid.Visible = true;
//                panel_Aid_Top.Visible = true;
                MakeVisible(tabControl_Main_Aid, true); // 7/26/17
                MakeVisible(panel_Aid_Top, true);       // 7/26/17
                this.Text = "Runner Tracker Aid Station";
                Thread.Sleep(1000);     // need to delay before starting

                //// start a Log File
                //CreateLogFile(DBRole);

                // display the initial AGWPE Statistics
                DisplayAidAGWPEportStats(AGWPEPortStatistics);
                #endregion
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // stop the timers
            if (!Quit_Early)
            {
                DictTimer.Stop();
                DictTimer.Close();
                ElapsedTimeClock.Stop();
                ElapsedTimeClock.Close();

                // stop the Output Message Queue - added 7/7/19
                Output_Messge_Queue_Term();
            }

            if (DBRole)
            {       // closing Database
                // stop timer
                if (!Quit_Early)
                {
                    ElapsedTime1sec.Stop();
                    ElapsedTime1sec.Close();
                }

                // shut down the Server Thread worker and AGWPE
                ServerObj.RequestStop();
                if (DB_AGWSocket != null)
                    DB_AGWSocket.CloseAGWPE();

                // shut down all of the Station Worker threads
                if (Stations != null)
                {
                    for (int i = 0; i < Stations.Count; i++)
                    {
                        if (Stations[i].Active)
                        {
                            switch (Stations[i].Medium)
                            {
                                case "Ethernet":
                                    Stations[i].IP_StationWorker.RequestStop();
                                    break;
                                case "Packet":
                                    Stations[i].Packet_StationWorker.RequestStop();
                                    break;
                                case "APRS":
                                    Stations[i].APRS_StationWorker.RequestStop();
                                    break;
                            }
                        }
                    }
                }

                // change DBRole to show that Form1 has closed
                DBRole = false;


//// removed 8/12/16                // ask if save Runner data - if there is any data
//                if (RunnerDictionary != null)
//                {
//                    if (RunnerDictionary.Count != 0)
//                    {
//                        DialogResult res = MessageBox.Show("Before quitting Runner Tracker Database,\n\nDo you want to save the Runner data?", "Save Runner data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
//                        if (res == System.Windows.Forms.DialogResult.Yes)
//                        {
//                            // save the data to filename: RunnerData.txt
//                            //   this file could already exist, if answered Yes when the program was started.

//                            // display this messagebox for 5 seconds:  "Runner data has been saved."
//                        }
//                    }
//                }
            }
            else
            {       // Aid Station Form1_FormClosing
                // stop timers
                if (!Quit_Early)
                {
                    Elapsed1secTimer.Stop();
                    Elapsed1secTimer.Close();
                    Elapsed5secTimer.Stop();
                    Elapsed5secTimer.Close();
                    Elapsed30secTimer.Stop();
                    Elapsed30secTimer.Close();
                    Elapsed1minTimer.Stop();
                    Elapsed1minTimer.Close();
                }

                // stop the client threads
                WorkerObject.RequestStop();
                if (Aid_AGWSocket != null)
                    Aid_AGWSocket.CloseAGWPE();
                if (WorkerReceiveThread != null)
                    WorkerReceiveThread.Join();

                // save some Registry values
                if (!Quit_Early)    // 4/24/19
                {
                    Save_Registry("Remove from List", Remove_from_Runners_list_Minutes.ToString());     // remove Runners from Station list after how many minutes?
                    if (rb_Use_Ethernet.Checked)
                        Connection_Type = Connect_Medium.Ethernet;
                    else
                    {
                        if (rb_Use_Packet.Checked)
                            Connection_Type = Connect_Medium.Packet;
                        else
                        {
                            if (rb_Use_APRS.Checked)
                                Connection_Type = Connect_Medium.APRS;
                            else
                                Connection_Type = Connect_Medium.Cellphone;
                        }
                    }
                    switch (Connection_Type)
                    {
                        case Connect_Medium.APRS:
                            Save_Registry("Connection Type", "APRS");
                            break;
                        case Connect_Medium.Packet:
                            Save_Registry("Connection Type", "Packet");
                            break;
                        case Connect_Medium.Ethernet:
                            Save_Registry("Connection Type", "Ethernet");
                            break;
                        case Connect_Medium.Cellphone:
                            Save_Registry("Connection Type", "Cellphone");
                            break;
                    }
                }

                // save the Messages if they have not been saved yet
                if (chk_Save_Messages_in_Files.Checked)
                {
                    if (!Incoming_Message_File_Started)
                    {       // if the Message File has been started, then all the messages have already been written
                        if (Incoming_Messages.Count != 0)   // are there any messages to write?
                        {       // yes - write out the messages
                            // create the file
                            CreateInMess();

                            // save all of the messages
                        }
                    }
                    if (!Outgoing_Message_File_Started)
                    {       // if the Message File has been started, then all the messages have already been written
                        if (Outgoing_Messages.Count != 0)   // are there any messages to write?
                        {       // yes - write out the messages
                            // create the file
                            CreateOutMess();

                            // save all of the messages
                        }
                    }
                }

                // has runner data not been sent to the Database?
                if ((RunnersIn != null) && (RunnersOut != null))
                {
                    if ((RunnersIn.Count != 0) || (RunnersOut.Count != 0))
                    {
// 7/22/16                        if (Connected_to_Server)
                        if (WorkerObject.Connected_and_Active)
                        {   // connected - tell user will be waiting while it is saved before closing
                        }
                        else
                        {   // not connected to server = ask user if want to save the data
                        }
                    }
                }
            }

            // look at all threads still open
            ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
            foreach (ProcessThread thread in currentThreads)
            {
                // Do whatever you need
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Quit_Early)
                Application.Exit();
        }
        #endregion


        #region Common Functions
        public static void Modeless_MessageBox_Exclamation(string message, string title)
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

        public static void Modeless_MessageBox_Information(string message, string title)
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
        
        void Elapsed_SetClock_Handler(object source, ElapsedEventArgs e)   // synchronizes our clock with the PC clock
        {
            // is this the first time into this handler?
            if (ElapsedTimeClock.Interval == 30000)
            {       // yes
                // get the current time
                DateTime Now = DateTime.Now;

                // change the Interval to get to the next minute
                ElapsedTimeClock.Interval = 60000 - (Now.Second * 1000) - Now.Millisecond;
                ElapsedTimeClock.Start();
            }
            else
            {       // no - this is the second time
                // set the Previous Clock Time
                PrevClockTime = DateTime.Now;

                // set the Interval to One minute
                ElapsedTimeClock.Interval = 60000;
                ElapsedTimeClock.AutoReset = true;

                // change the Event Handler
                ElapsedTimeClock.Elapsed -= Elapsed_SetClock_Handler;
                ElapsedTimeClock.Elapsed += new ElapsedEventHandler(Elapsed_Clock_Handler);
                ElapsedTimeClock.Start();
            }
        }

        void Elapsed_Clock_Handler(object source, ElapsedEventArgs e)   // updates current time displayed
        {
            // This event occurs every minute.  Its only purpose is to update the Current Time being displayed

            // update the time
            if ((Minutes == 0) && (Hours == 0)) // this condition will happen only when the program starts up and between midnight and 12:01am
            {
                // get the real time
                DateTime Now = DateTime.Now;
                Minutes = (Int16)Now.Minute;
                Hours = (Int16)Now.Hour;
            }
            else
            {
                // normally just increment the minutes and then hours if needed
                Minutes++;
                if (Minutes == 60)
                {
                    // set back to zero
                    Minutes = 0;

                    // now need to increment the hours
                    Hours++;
                    if (Hours == 24)
                    {
                        // set back to zero
                        Hours = 0;
                    }

                    // now calculate if the Clock Timer Elapsed Interval value needs to be changed
                    DateTime Now = DateTime.Now;    // get the real PC time
                    DateTime Now30 = Now.AddSeconds(30);    // use this to solve a midnight overlap problem: Now=23:59:59, My=0:0:0
//                    DateTime MyTime = new DateTime(Now.Year, Now.Month, Now.Day, Hours, Minutes, 0);  // create my time
                    DateTime MyTime = new DateTime(Now30.Year, Now30.Month, Now30.Day, Hours, Minutes, 0);  // create my time
                    Int32 minuteselapsed = (Int32)(Now - PrevClockTime).TotalMinutes;   // calculate # of minutes since last update
                    if (Now > MyTime)       // is the PC time faster than my time
                    {       // yes - my time needs to go faster by making the timer elapsed time shorter
                        Int32 timediff = (Int32)(Now - MyTime).TotalMilliseconds;
                        ElapsedTimeClock.Interval -= timediff / minuteselapsed;     // break it into minute chunks
                    }
                    else
                    {       // no - my time needs to go slower by making the timer elapsed time longer
                        Int32 timediff = (Int32)(MyTime - Now).TotalMilliseconds;
                        ElapsedTimeClock.Interval += timediff / minuteselapsed;     // break it into minute chunks
                    }

                    // set the Previous Clock Time
                    PrevClockTime = Now;
                }
            }

            // display the new time
            SetTBtext(tb_Current_Time, Hours.ToString() + ":" + Minutes.ToString("D2"));
        }

        #region delegate functions
        void SetTBtext(TextBox tb, string str)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!tb.IsDisposed)
            {
                if (tb.InvokeRequired)
                {
                    SetTextdel d = new SetTextdel(SetTBtext);
                    tb.Invoke(d, new object[] { tb, str });
                }
                else
                {
                    tb.Text = str;
                    tb.Update();
                    Application.DoEvents();
                }
            }
        }

        string GetTBtext(TextBox tb)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!tb.IsDisposed)
            {
                if (tb.InvokeRequired)
                {
                    GetTBtextdel d = new GetTBtextdel(GetTBtext);
                    tb.Invoke(d, new object[] { tb });
                }
                else      // 8/5/17
                {
                    return tb.Text;
                }
            }
            return null;
        }

        //void SetCtlText(Control cntrl, string str)
        //{
        //    // InvokeRequired required compares the thread ID of the
        //    // calling thread to the thread ID of the creating thread.
        //    // If these threads are different, it returns true.
        //    if (!cntrl.IsDisposed)
        //    {
        //        if (cntrl.InvokeRequired)
        //        {
        //            SetCtlTextdel d = new SetCtlTextdel(SetCtlText);
        //            cntrl.Invoke(d, new object[] { cntrl, str });
        //        }
        //        else
        //        {
        //            cntrl.Text = str;
        //            cntrl.Update();
        //            Application.DoEvents();
        //        }
        //    }
        //}

        void AddRichText(RichTextBox rtb, string str, Color color)
        {
            if (!rtb.IsDisposed)
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (!rtb.IsDisposed)
                {
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
                        Application.DoEvents();
                    }
                }
            }
        }

        void MakeEnabled(Control cntl, bool enable)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!cntl.IsDisposed)
            {
                if (cntl.InvokeRequired)
                {
                    MakeEnableddel d = new MakeEnableddel(MakeEnabled);
                    cntl.Invoke(d, new object[] { cntl, enable });
                }
                else
                {
                    cntl.Enabled = enable;
                    cntl.Update();
                    Application.DoEvents();
                }
            }
        }

        void MakeVisible(Control cntl, bool visible)
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

        void SetCtlForeColor(Control cntl, Color clr)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!cntl.IsDisposed)
            {
                if (cntl.InvokeRequired)
                {
                    SetCtlForeColordel d = new SetCtlForeColordel(SetCtlForeColor);
                    cntl.Invoke(d, new object[] { cntl, clr });
                }
                else
                {
                    cntl.ForeColor = clr;
                    cntl.Update();
                    Application.DoEvents();
                }
            }
        }

        void SetCtlBackColor(Control cntl, Color clr)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!cntl.IsDisposed)
            {
                if (cntl.InvokeRequired)
                {
                    SetCtlBackColordel d = new SetCtlBackColordel(SetCtlBackColor);
                    cntl.Invoke(d, new object[] { cntl, clr });
                }
                else
                {
                    cntl.BackColor = clr;
                    cntl.Update();
                    Application.DoEvents();
                }
            }
        }

        void SetCtlFocus(Control cntl)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!cntl.IsDisposed)
            {
                if (cntl.InvokeRequired)
                {
                    SetCtlFocusdel d = new SetCtlFocusdel(SetCtlFocus);
                    cntl.Invoke(d, new object[] { cntl });
                }
                else
                {
                    cntl.Focus();
                    Application.DoEvents();
                }
            }
        }
        #endregion

        #region Log File
        private void CreateLogFile(bool DBrole)
        {
            DateTime dt = DateTime.Now;
            string dateformat = "MMM_dd_HH_mm";
            //            LogFileName = DataDirectory + "\\Log_" + dt.ToString(dateformat) + ".txt";
            if (DBrole)
            {
                LogFileName = DataDirectory + "\\Database Log Files" + "\\Log_" + dt.ToString(dateformat) + ".txt";
                if (!Directory.Exists(DataDirectory + "\\Database Log Files"))
                    Directory.CreateDirectory(DataDirectory + "\\Database Log Files");
            }
            else
            {
                LogFileName = DataDirectory + "\\Aid Station Log Files" + "\\Log_" + dt.ToString(dateformat) + ".txt";
                if (!Directory.Exists(DataDirectory + "\\Aid Station Log Files"))
                    Directory.CreateDirectory(DataDirectory + "\\Aid Station Log Files");
            }
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;

            // verify the file is good by trying to open it
            try
            {
                fs = new FileStream(LogFileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                writer = new StreamWriter(fs);
            }
            //catch
            //{
            //    MessageBox.Show("Selected file:\n\n" + LogFileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    return;
            //}
            catch (IOException ioex)
            {
                if (ioex.Message.EndsWith("already exists."))
                {
                    MessageBox.Show("Selected file:\n\n" + LogFileName + "\n\nalready exists!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            // put the ID text in it
            string line;
            if (DBrole)
                line = "Runner Tracker Database launched at " + dt.ToShortTimeString() + " on " + dt.ToShortDateString() + ".";
            else
                line = "Runner Tracker Aid Station launched at " + dt.ToShortTimeString() + " on " + dt.ToShortDateString() + ".";
            writer.WriteLine(line);
            writer.Close();
        }

        public static void AddtoLogFile(string text)
        {
            string FileName = LogFileName;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {       // where did the file go?
                MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            DateTime dt = DateTime.Now;
            string format = "HH:mm> ";      // do we need to add the date too? :  10/24 13:45>
            //            writer.WriteLine(text); // now write the string
            writer.WriteLine(dt.ToString(format) + text); // now write the string
            writer.Close();
        }
        #endregion

        #region Registry Test, Read and Save
// 8/14/17        private bool Test_Registry()        // returns true if the Registry entry exists, false if it does not exist
        public static bool Test_Registry()        // returns true if the Registry entry exists, false if it does not exist - made public 8/14/17
        {
            // open the registry keys
            RegistryKey rkey = Registry.CurrentUser;
            RegistryKey skey = rkey.OpenSubKey("Software\\Runner Tracker\\Settings");
            if (skey == null)
                return false;
            else
                return true;
        }

        // 8/14/17        private String Read_Registry(String keyname)
        public static String Read_Registry(String keyname)        // 8/14/17
        {
            // open the registry keys
            RegistryKey rkey = Registry.CurrentUser;
            RegistryKey skey = rkey.OpenSubKey("Software\\Runner Tracker\\Settings");

            // if the SubKey does not exist, because this is the first time this program is used,
            // then create the SubKey
            if (skey == null)
            {
                skey = rkey.CreateSubKey("Software\\Runner Tracker\\Settings");
            }

            // Retrieve the value for the keyname requested
            return (string)skey.GetValue(keyname);
        }

        // 8/14/17        private void Save_Registry(String keyname, String value)
        public static void Save_Registry(String keyname, String value)        // 8/14/17
        {
            // open the registry keys
            RegistryKey rkey = Registry.CurrentUser;
            RegistryKey skey = rkey.OpenSubKey("Software\\Runner Tracker\\Settings", true);    // no - just need to set the write access flag

            // if the Subkey does not exist, because this is the first time this program is used,
            // then create the Subkey
            if (skey == null)
            {
                skey = rkey.CreateSubKey("Software\\Runner Tracker\\Settings");
            }

            // Save the value for the keyname requested
            if (value != null)
                skey.SetValue(keyname, value);
            else
                skey.SetValue(keyname, "");
        }
        #endregion

        #region Output Message Queue - added 7/7/19
        #region Variables
        private static Int16 Ethernet_Response_Time;       // seconds needed to receive response from Aid Station connected by Ethernet (Mesh)
        private static Int16 APRS_Packet_Response_Time;    //    "  ...  APRS / Packet
        private static Random RandomMessageID;
        public class OutputMessageQueueClass
        {
            public string ToWhom { get; set; }
            public int SecRemaining { get; set; }
            public DateTime TimeSent { get; set; }
            public int RandomMessageID { get; set; }
        }
        static List<OutputMessageQueueClass> OutputMessageQueue;

        // Control characters used around Message ID that will be used before the oroginal data being sent in the message:
        const byte SOH = 0x01;  // Start of Header - first byte, preceeds message ID
        const byte STX = 0x02;  // Start of Text - follows message ID, preceeds original data
        const byte ACK = 0x06;  // Acknowledge - sent in place of SOH, in response to a previous message
        #endregion

        #region Variable Textbox Actions
        private void tb_DB_Out_Mess_Q_Eth_sec_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Out_Mess_Q_Eth_sec.Text == "")
                tb_DB_Out_Mess_Q_Eth_sec.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Out_Mess_Q_Eth_sec.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Out_Mess_Q_Eth_sec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Out_Mess_Q_Eth_sec_Leave(null, null);
        }

        private void tb_DB_Out_Mess_Q_Eth_sec_Leave(object sender, EventArgs e)
        {
            Save_Registry("Ethernet response (sec)", tb_DB_Out_Mess_Q_Eth_sec.Text);
        }

        private void tb_DB_Out_Mess_Q_APRS_sec_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Out_Mess_Q_APRS_sec.Text == "")
                tb_DB_Out_Mess_Q_APRS_sec.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Out_Mess_Q_APRS_sec.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Out_Mess_Q_APRS_sec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Out_Mess_Q_APRS_sec_Leave(null, null);
        }

        private void tb_DB_Out_Mess_Q_APRS_sec_Leave(object sender, EventArgs e)
        {
            Save_Registry("APRS/Packet response (sec)", tb_DB_Out_Mess_Q_APRS_sec.Text);
        }
        #endregion

        #region Functions
        private void Output_Message_Queue_Init()
        {
            OutputMessageQueue = new List<OutputMessageQueueClass>();
            RandomMessageID = new Random();
        }

        public static int Put_Message_in_Queue(string ToWhom, string Medium)
        {
            OutputMessageQueueClass entry = new OutputMessageQueueClass();
            entry.ToWhom = ToWhom;
            switch (Medium)
            {
                case "IP" :
                    entry.SecRemaining = Ethernet_Response_Time;
                    break;
                case "AGWPE" :
                    entry.SecRemaining = APRS_Packet_Response_Time;
                    break;
            }
            entry.TimeSent = DateTime.Now;
            entry.RandomMessageID = RandomMessageID.Next();
            OutputMessageQueue.Add(entry);
            return entry.RandomMessageID;
        }

        private void Test_Output_Messaage_Queue()
        {
            if (OutputMessageQueue.Count != 0)
            {
                foreach (OutputMessageQueueClass entry in OutputMessageQueue)
                {
                    entry.SecRemaining--;
                    if (entry.SecRemaining == 0)
                    {
                        Modeless_MessageBox_Exclamation("No response from " + entry.ToWhom, "Output message not acknowledged");
                    }
                }
            }
        }

        private void Remove_Message_from_Queue(string ToWhom)   // or do we want to look it up by the RandomMessageID ???
        {
            int index = OutputMessageQueue.FindIndex(entry => entry.ToWhom == ToWhom);
            OutputMessageQueue.RemoveAt(index);
        }

        private void Output_Messge_Queue_Term()
        {
        }
        #endregion
        #endregion
        #endregion


        #region Database Functions
        private void tabControl_DB_Main_Enter(object sender, EventArgs e)
        {
            // when first enter this tab, check if the Info File has been loaded
            // if the Info File has not been loaded, go to that tab so the data can be loaded.
            if (!InfoLoaded && !InfoPageShown)
            {
                tabControl_DB_Main.SelectedIndex = 3;  // select the Info/Lists tab
                tabControl_Lists.SelectedIndex = 4; // select the Info tab
                tabControl_Info.SelectedIndex = 0;  // select the Event Info tab
                InfoPageShown = true;
            }
        }

        private void Labels_TabPages_DB_Connections()
        {
            if (!tabControl_Communication.IsDisposed)
            {
//                tabControl_Communication.TabPages.Clear();

                // test if using Ethernet
                if (!ConnectViaEthernet)
                    tabControl_Communication.TabPages.Remove(tabPage_DB_Ethernet);

                // test if using APRS or Packet
                if (ConnectViaAPRS || ConnectViaPacket)
                    tabControl_DB_Settings_AGWPE.Visible = true;
                else
                    tabControl_DB_Settings_AGWPE.Visible = false;

                if ((DB_AGWSocket != null) && DB_AGWSocket.Connected_to_AGWserver)
                {
                    // add the tabpages to the tabcontrols
                    if (ConnectViaAPRS)
                    // 3/25/16 not ready for APRS yet                tabControl_Communication.TabPages.Add(tabPage_DB_APRS);
                        tabControl_Communication.TabPages.Add(tabPage_DB_APRS);
                    if (ConnectViaPacket)
                        tabControl_Communication.TabPages.Add(tabPage_DB_Packet);
                }
                else
                {
                    // remove the tabpages from the tabcontrols
                    tabControl_Communication.TabPages.Remove(tabPage_DB_APRS);
                    tabControl_Communication.TabPages.Remove(tabPage_DB_Packet);
                }
            }
        }

        #region Tab Header coloring
        private Dictionary<TabPage, Color> TabColorsMain = new Dictionary<TabPage, Color>();
        private Dictionary<TabPage, Color> TabColorsIssues = new Dictionary<TabPage, Color>();
        private void SetTabHeaderDBMain(TabPage page, Color color)
        {
            TabColorsMain[page] = color;
            tabControl_DB_Main.Invalidate();
        }

        private void SetTabHeaderDBIssues(TabPage page, Color color)
        {
            TabColorsIssues[page] = color;
            tabControl_Issues.Invalidate();
        }

        /*      code found on Internet at: http://stackoverflow.com/questions/5338587/set-tabpage-header-color
         *          must set the tabControl DrawMode to OwnerDrawFixed, not the default of Normal
         */
        private void tabControl_Main_DB_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color TabColor;
            Brush TextBrush;
            // determine the Tab color
            if (e.State == System.Windows.Forms.DrawItemState.Selected)
                TabColor = Color.White;
            else
                TabColor = TabColorsMain[tabControl_DB_Main.TabPages[e.Index]];
            // determine the Text color: Black if no error, White if not selected, Red if selected
            if (TabColorsMain[tabControl_DB_Main.TabPages[e.Index]] == Color.Red)  // this indicates errors have occurred
            {       // need to know if selected
                if (e.State == System.Windows.Forms.DrawItemState.Selected)
                    TextBrush = Brushes.Red;
                else
                    TextBrush = Brushes.White;
            }
            else
            {       // no errors, set it Black
                TextBrush = Brushes.Black;
            }
            using (Brush br = new SolidBrush(TabColor))
            {
                Rectangle rect = e.Bounds;
                rect.Height -= 1;
                e.Graphics.FillRectangle(br, rect);
                SizeF sz = e.Graphics.MeasureString(tabControl_DB_Main.TabPages[e.Index].Text, e.Font);
                e.Graphics.DrawString(tabControl_DB_Main.TabPages[e.Index].Text, e.Font, TextBrush, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);
            }
        }

        private void tabControl_Issues_DB_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color TabColor;
            Brush TextBrush;
            // determine the Tab color
            if (e.State == System.Windows.Forms.DrawItemState.Selected)
                TabColor = Color.White;
            else
                TabColor = TabColorsIssues[tabControl_Issues.TabPages[e.Index]];
            // determine the Text color: Black if no error, White if not selected, Red if selected
            if (TabColorsIssues[tabControl_Issues.TabPages[e.Index]] == Color.Red)  // this indicates errors have occurred
            {       // need to know if selected
                if (e.State == System.Windows.Forms.DrawItemState.Selected)
                    TextBrush = Brushes.Red;
                else
                    TextBrush = Brushes.White;
            }
            else
            {       // no errors, set it Black
                TextBrush = Brushes.Black;
            }
            using (Brush br = new SolidBrush(TabColor))
            {
                Rectangle rect = e.Bounds;
                rect.Height -= 1;
                e.Graphics.FillRectangle(br, rect);
                SizeF sz = e.Graphics.MeasureString(tabControl_Issues.TabPages[e.Index].Text, e.Font);
                e.Graphics.DrawString(tabControl_Issues.TabPages[e.Index].Text, e.Font, TextBrush, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);
            }
        }

        public void DictTimeEvent(object source, ElapsedEventArgs e)
        {       // this is an ISR that happens every .5 sec, so it needs to be fast
            // test Stations tab to see if we need to load or create a Station list
            if (Stations.Count == 0)
            {
                if (StationC != Color.Red)
                {
                    SetTabHeaderDBMain(tabPage_Stations, Color.Red);
                    StationC = Color.Red;
                }
            }
            else
            {
                if (StationC != Color.FromKnownColor(KnownColor.Control))
                {
                    SetTabHeaderDBMain(tabPage_Stations, Color.FromKnownColor(KnownColor.Control));
                    StationC = Color.FromKnownColor(KnownColor.Control);
                }
            }

            // test Settings tab to see if Welcome message and the Runners Data File name has been entered
// 8/5/17            if (Welcome_Message_Exists && (tb_DB_Settings_RunnersDataFile.Text != ""))
//string str = GetTBtext(tb_DB_Settings_RunnersDataFile);     // 8/11/17 - testing
            if (Welcome_Message_Exists && (GetTBtext(tb_DB_Settings_RunnersDataFile) != ""))
            {
                if (SettingsC != Color.FromKnownColor(KnownColor.Control))
                {
                    SetTabHeaderDBMain(tabPage_Settings, Color.FromKnownColor(KnownColor.Control));
                    SettingsC = Color.FromKnownColor(KnownColor.Control);

// 3/12/19                    // test if the IP Server has already been started
// 3/12/19                    if (!OK_to_start_IP_Server)
// 3/12/19                    {       // has not started yet - start it
// 3/12/19                        Find_This_PC_IP_Address();
// 3/12/19                        OK_to_start_IP_Server = true;   // 4/4/17 - why is this here
// 3/12/19                    }
                }
            }
            else
            {
                if (SettingsC != Color.Red)
                {
                    SetTabHeaderDBMain(tabPage_Settings, Color.Red);
                    SettingsC = Color.Red;
                }
            }

            // test if a new Aid Station issue has been added
            if (New_AS_Issue)
            {
                if (DBIssuesC != Color.Red)
                {
                    SetTabHeaderDBMain(tabPage_Issues, Color.Red);
                    DBIssuesC = Color.Red;
                }
                if (AidIssuesC != Color.Red)
                {
                    SetTabHeaderDBIssues(tabPage_AidStation_Issues, Color.Red);
                    AidIssuesC = Color.Red;
                }
            }
            else
            {
                if (DBIssuesC != Color.FromKnownColor(KnownColor.Control))
                {
                    SetTabHeaderDBMain(tabPage_Issues, Color.FromKnownColor(KnownColor.Control));
                    DBIssuesC = Color.FromKnownColor(KnownColor.Control);
                }
                if (AidIssuesC != Color.FromKnownColor(KnownColor.Control))
                {
                    SetTabHeaderDBIssues(tabPage_AidStation_Issues, Color.FromKnownColor(KnownColor.Control));
                    AidIssuesC = Color.FromKnownColor(KnownColor.Control);
                }
            }
        }
        
        private void ChangeTab(int tab)
        {
            if (tabControl_DB_Main.InvokeRequired)
                tabControl_DB_Main.BeginInvoke(new ChangeTabCallback(ChangeTab), tab);
            else
            {
                tabControl_DB_Main.SelectedIndex = 1;
                tabControl_DB_Main.Update();
            }
        }
        #endregion

        #region Timers and associated threads for Database
        void Elapsed_DB_1sec_Handler(object source, ElapsedEventArgs e)    // changed name 7/8/19
        {
            // This event happens every second.
            //
            // Its main purpose is to look for and handle changes from the Stations, ie:
            //      1. When the station name is received, it will be marked as Active
            //      2. When a new station connects (that is not in the Station List), it needs to be added
            //
            // It will also be used to delay 5 seconds after AGWPE finishes Initting to display the settings

            // Test if the OutputMessageQueue has any pending messages - added 7/7/19
            Test_Output_Messaage_Queue();

            // test if the AGWPE socket has finished Initting
            if (AGW_Count != 0)
            {
                if (!Form1.DB_AGWSocket.InitInProcess)
                {
                    AGW_Count--;
                    if (AGW_Count == 0)
                    {
//                        Get_AGWPE_Settings();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(Get_AGWPE_Setts));
                    }
                }
            }

            // test if a new Packet station has just connected
            if (Packet_Connects_Disconnects.Count != 0)
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_Packet_or_APRS_Station));

            // test if a new station has been added or info added
            if (Stations_Activity_Flag)
            {
//                // test if it is adding a station
//// 8/4/16                if (Add_Station)
//                {
//                    // this will be a problem if more than 1 new station comes online at the same time
//                    Add_Station_Name(New_Station_Name);
//                }
//                else
//                {
                    UpdateStationDGV();
                    Stations_Activity_Flag = false;
//                }
            }
            if (New_Active_Station_entry)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_ActiveStation));
            }

            // test if any new messages have been received from a station
            //   or the Number of Log Points value has changed
            if (Num_Stations != 0)
            {
                if (Stations[Rotating_Index].Active)
                {
                    switch (Stations[Rotating_Index].Medium)
                    {
                        case "Ethernet":
                            // test for new messages
                            if (Stations[Rotating_Index].IP_StationWorker.New_Message)
                            {
                                // start thread to: 1) tell the user, 2) update dgv & display, if on Message tab, 3) turn off the flag
                                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageThread), Rotating_Index);
                            }

                            // test for change in Number of Log Points
                            if (Stations[Rotating_Index].IP_StationWorker.NumLogPts != 0)
                            {
                                // start thread to: 1) change entry in Station List & save it, 2) notify user if it is not during station initting
                                ThreadPool.QueueUserWorkItem(new WaitCallback(NumLogPtsThread), Rotating_Index);
                            }
                            break;
                        case "Packet":
                            // test for new messages
                            if (Stations[Rotating_Index].Packet_StationWorker.New_Message)
                            {
                                // start thread to: 1) tell the user, 2) update dgv & display, if on Message tab, 3) turn off the flag
                                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageThread), Rotating_Index);
                            }

                            // test for change in Number of Log Points
                            if (Stations[Rotating_Index].Packet_StationWorker.NumLogPts != 0)
                            {
                                // start thread to: 1) change entry in Station List & save it, 2) notify user if it is not during station initting
                                ThreadPool.QueueUserWorkItem(new WaitCallback(NumLogPtsThread), Rotating_Index);
                            }
                            break;
                        case "APRS":
                            // test for new messages
                            if (Stations[Rotating_Index].APRS_StationWorker.New_Message)
                            {
                                // start thread to: 1) tell the user, 2) update dgv & display, if on Message tab, 3) turn off the flag
                                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageThread), Rotating_Index);
                            }

                            // test for change in Number of Log Points
                            if (Stations[Rotating_Index].APRS_StationWorker.NumLogPts != 0)
                            {
                                // start thread to: 1) change entry in Station List & save it, 2) notify user if it is not during station initting
                                ThreadPool.QueueUserWorkItem(new WaitCallback(NumLogPtsThread), Rotating_Index);
                            }
                            break;
                    }
                }
                Rotating_Index++;
                Rotating_Index = Rotating_Index % Num_Stations;
            }

            // test for new runners coming in
            if (New_RunnerInQue_entry)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_RunnersIn));
            }

            // test for new runners going out
            if (New_RunnerOutQue_entry)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_RunnersOut));
            }

            // test for new Watch runners coming in
            if (New_Watch_Runner_entry)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_WatchRunners));
            }

            // test for new DNF runners coming in
            if (New_DNF_Runner_entry)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_DNFRunners));
            }

            // test for new Aid Station Issues coming in
            if (New_AidStation_Issue_entry)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(New_ASIssues));
            }
        }

        void Elapsed1minDBHandler(object source, ElapsedEventArgs e)  // send Runner data
        {
            // this event happens every 1 minute.
            // its only purpose for this timer is to update the AGWPE Statistics, if AGWPE is running
            if (DB_AGWSocket != null)
                DisplayDBAGWPEportStats(AGWPEPortStatistics);
        }

        private void NumLogPtsThread(object info)
        {
            // 1) change entry in Station List & save it, 2) notify user if it is not during station initting
            int index = (int)info;

            // get the new NumLogPts value and clear the flag
            
            int newNumLogPts = Stations[index].IP_StationWorker.NumLogPts;
            Stations[index].IP_StationWorker.NumLogPts = 0;

            // 1) change entry in Station List & save it
            int oldNumLogPts = Stations[index].Number_of_Log_Points;
            Stations[index].Number_of_Log_Points = newNumLogPts;
            Save_Stations(Stations, tb_Save_Station_Info_Filename.Text);
            Bind_DB_Station_DGV();
            
            // 2) notify user if it is not during station initting
            if (!Stations[index].IP_StationWorker.InitialLogPoint)
            {
                // tell the user
                if (oldNumLogPts == 1)
                    Modeless_MessageBox_Exclamation("Station:\n\n" + Stations[index].Name + "\n\nhas changed from 1 Log Point to 2 Log Points", "Change in Log Points");
                else
                    Modeless_MessageBox_Exclamation("Station:\n\n" + Stations[index].Name + "\n\nhas changed from 2 Log Points to 1 Log Point", "Change in Log Points");

                // clear the flag
                Stations[index].IP_StationWorker.InitialLogPoint = false;
            }
        }

        private void MessageThread(object info)
        {
            // 1) tell the user, 2) update dgv & display, if on Message tab, 3) turn off the flag
            int index = (int)info;

            // turn off flag
            switch (Stations[index].Medium)
            {
                case "Ethernet":
                    Stations[index].IP_StationWorker.New_Message = false;
                    break;
                case "Packet":
                    Stations[index].Packet_StationWorker.New_Message = false;
                    break;
            }

            // tell the user
            Modeless_MessageBox_Information("New message received from station:\n\n      " + Stations[index].Name, "New Message");

            // update dgv & display if on Message tab
            TestTabControlTabName();

            // put focus back on the last textbox
            LastFocus(_last);
        }

        void Get_AGWPE_Setts(object info)
        {
            Get_AGWPE_Settings();
        }

        public void New_Packet_or_APRS_Station(object info)
        {
            lock (Packet_Connects_Disconnects)
            {
                while (Packet_Connects_Disconnects.Count > 0)
                {
                    // get the queue packet
                    Queue_Packet_Connect_Disconnect new_station = Packet_Connects_Disconnects.Dequeue();

                    // is this for a Connect or Disconnect?
                    switch (new_station.ConDis)
                    {
                        case 'C':
                            // create a new Packet Station Worker
                            DB_Packet_StationWorker PacketWorkerObject = new DB_Packet_StationWorker(rtb_DB_Packet_Packets, new_station.PipeHandle);

                            // open a new Packet worker thread
                            PacketStationThread = new Thread(new ParameterizedThreadStart(PacketWorkerObject.Start));
                            PacketStationThread.Start(new_station.Callsign);
                            PacketStationRcvThread = new Thread(new ParameterizedThreadStart(PacketWorkerObject.DB_Packet_Worker_Receive_Thread));
                            PacketStationRcvThread.Start();
                            Console.WriteLine("Starting new Packet Station worker threads, for Callsign: " + new_station.Callsign + Environment.NewLine);

                            // increment the connected stations count
                            Connected_Stations++;
                            break;
                        case 'D':
                            Connected_Stations--;
                            break;
                        case 'A':       // APRS
                            // create a new APRS Station Worker thread
                            DB_APRS_StationWorker APRSWorkerObject = new DB_APRS_StationWorker(rtb_APRS_Packets_Received_DB, new_station.PipeHandle);

                            // open a new APRS worker thread
                            APRSstationThread = new Thread(new ParameterizedThreadStart(APRSWorkerObject.Start));
                            APRSstationThread.Start(new_station.Callsign);
                            APRSstationRcvThread = new Thread(new ParameterizedThreadStart(APRSWorkerObject.DB_APRS_Worker_Receive_Thread));
                            APRSstationRcvThread.Start();
                            Console.WriteLine("Starting new APRS Station worker threads, for Callsign: " + new_station.Callsign + Environment.NewLine);

                            // increment the connected stations count
                            Connected_Stations++;
                            break;
                    }
                }
            }
        }

        public void New_ActiveStation(object info)  // thread to add a new connected station to the Station List
        {
            lock (NewActiveStationQue)
            {// lock
                while (NewActiveStationQue.Count > 0)
                {
                    NewStation newst = NewActiveStationQue.Dequeue();
                    string name = newst.Name;
                    Add_Station_Name(name);

                    int index = Find_Station(name);
                    if (index != -1)
                    {
                        switch (newst.Medium)
                        {
                            case "APRS":
                                Stations[index].APRS_StationWorker = newst.APRS_StationWorker;
                                Stations[index].Active = true;
                                Stations[index].IP_Address_Callsign = newst.IP_Address_Callsign;
                                Stations[index].Medium = newst.Medium;
                                AddtoLogFile("New station: " + name + " connected and added to Station List");
                                newst.APRS_StationWorker.state = DB_APRS_StationWorker.Server_State.ClientConnected;
                                newst.APRS_StationWorker.Station_List_index = index;
                                Bind_DB_Station_DGV();  // do it again, so the Active flag will be seen
                                break;
                            case "Packet":
                                Stations[index].Packet_StationWorker = newst.Packet_StationWorker;
                                Stations[index].Active = true;
                                Stations[index].IP_Address_Callsign = newst.IP_Address_Callsign;
                                Stations[index].Medium = newst.Medium;
                                AddtoLogFile("New station: " + name + " connected and added to Station List");
                                newst.Packet_StationWorker.state = DB_Packet_StationWorker.Server_State.ClientConnected;
                                newst.Packet_StationWorker.Station_List_index = index;
                                Bind_DB_Station_DGV();  // do it again, so the Active flag will be seen
                                break;
                            case "Ethernet":
                                Stations[index].IP_StationWorker = newst.IP_StationWorker;
                                Stations[index].Active = true;
                                Stations[index].IP_Address_Callsign = newst.IP_Address_Callsign;
                                Stations[index].Medium = newst.Medium;
                                AddtoLogFile("New station: " + name + " connected and added to Station List");
                                newst.IP_StationWorker.state = DB_IP_StationWorker.Server_State.ClientConnected;
                                newst.IP_StationWorker.Station_List_index = index;
                                Bind_DB_Station_DGV();  // do it again, so the Active flag will be seen
                                break;
                        }
////                        Stations[index].IP_StationWorker = newst.IP_StationWorker;
//                        Stations[index].Active = true;
//                        Stations[index].IP_Address_Callsign = newst.IP_Address_Callsign;
//                        Stations[index].Medium = newst.Medium;
//                        AddtoLogFile("New station: " + name + " connected and added to Station List");
//                        newst.IP_StationWorker.state = DB_IP_StationWorker.Server_State.ClientConnected;
//                        newst.IP_StationWorker.Station_List_index = index;
//                        Bind_DB_Station_DGV();  // do it again, so the Active flag will be seen
                    }
                }
                New_Active_Station_entry = false;
            }// unlock

            // notify user by playing the Connection Sound - added 8/14/17
        }

        public void New_RunnersIn(object info)
        {
            lock (RunnerInQue)
            {// lock
                while (RunnerInQue.Count > 0)
                {
                    AddUpdateRunner(RunnerInQue.Dequeue(), true);      // time is 'Time in'
                }
                New_RunnerInQue_entry = false;
            }// unlock
        }

        public void New_RunnersOut(object info)
        {
            lock (RunnerOutQue)
            {// lock
                while (RunnerOutQue.Count > 0)
                {
                    AddUpdateRunner(RunnerOutQue.Dequeue(), false);      // time is 'Time out'
                }
                New_RunnerOutQue_entry = false;
            }// unlock
        }

        public void New_WatchRunners(object info)
        {
            lock (WatchInQue)
            {// lock
                while (WatchInQue.Count > 0)
                {
                    AddWatchRunner(WatchInQue.Dequeue());
                }
                New_Watch_Runner_entry = false;
            }// unlock
        }

        public void New_DNFRunners(object info)
        {
            lock (DNFInQue)
            {// lock
                while (DNFInQue.Count > 0)
                {
                    AddDNFRunner(DNFInQue.Dequeue());
                }
                New_DNF_Runner_entry = false;
            }// unlock
        }

        public void New_ASIssues(object info)
        {
            lock (AidStationIssuesQue)
            {//lock
                while (AidStationIssuesQue.Count > 0)
                {
                    AddNewASIssueFromAS(AidStationIssuesQue.Dequeue());
                }
                New_AidStation_Issue_entry = false;
            }// unlock
        }

        private void LastFocus(Control cntrl)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (cntrl.InvokeRequired)
            {
                SetFocus d = new SetFocus(LastFocus);
                cntrl.Invoke(d, new object[] { cntrl });
            }
            else
            {
                cntrl.Focus();
            }
        }

        public void TestTabControlTabName()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (tabControl_DB_Main.InvokeRequired)
            {
                TestTabControlTabNameDel d = new TestTabControlTabNameDel(TestTabControlTabName);
                tabControl_DB_Main.Invoke(d, new object[] { });
            }
            else
            {
                if (tabControl_DB_Main.SelectedTab.Name == "tabPage_Messages")
                {
                    // update dgvs
                    MakeVisible(dgv_Incoming_Messages_for_Selected_Station, true);      // 7/17/17
                    Bind_Incoming_Messages_Selected_Station_DGV(dgv_Messages_Stations.CurrentRow.Index);
                    Bind_Messages_Station_DGV();
                }
            }
        }
        #endregion

        #region IP Server
        private void Find_This_PC_IP_Address()
        {
            // first test if a Network Interface exists to connect to
            if (NetworkInterface.GetIsNetworkAvailable())
            {       // Yes - there are Network Adapters on this system
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                int niccount = adapters.Length;
                int usableniccount = niccount;
                int goodnic = 0;
                int GWAcount = 0;
                string GWA = string.Empty;
                string GWA_IP = string.Empty;
                string InterfaceType = string.Empty;
                string IP_Address = string.Empty;
                for (int i = 0; i < usableniccount; i++)
                {
                    if (adapters[i].NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        adapters[i].NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        niccount--;
                    }
                    else
                    {
                        IPInterfaceProperties properties = adapters[i].GetIPProperties();
                        GWAcount = properties.GatewayAddresses.Count;
                        if (GWAcount != 0)
                        {
                            goodnic = i;
                        }
                    }
                }
//// 4/18/19 removed                switch (niccount)
//                {
//                    case 1:
//                        IP_Address = Test_IP_GW(adapters[goodnic]);
//                        Set_IPs(IP_Address);
//                        break;
//                    case 2:
//                        TwoNICs twonics = new TwoNICs(adapters);
//                        DialogResult dr2 = twonics.ShowDialog();
//                        if (dr2 == System.Windows.Forms.DialogResult.OK)
//                        {
//                            IP_Address = twonics.IP_Address;
//                            Set_IPs(IP_Address);
//                        }
//                        twonics.Dispose();
//                        break;
//                    case 3:
//                        ThreeNICs threenics = new ThreeNICs(adapters);
//                        DialogResult dr3 = threenics.ShowDialog();
//                        if (dr3 == System.Windows.Forms.DialogResult.OK)
//                        {
//                            IP_Address = threenics.IP_Address;
//                            Set_IPs(IP_Address);
//                        }
//                        threenics.Dispose();
//                        break;
//                    case 4:     // this section added 7/13/17
//                        FourNICs fournics = new FourNICs(adapters);
//                        DialogResult dr4 = fournics.ShowDialog();
//                        if (dr4 == System.Windows.Forms.DialogResult.OK)
//                        {
//                            IP_Address = fournics.IP_Address;
//                            Set_IPs(IP_Address);
//                        }
//                        fournics.Dispose();
//                        break;
//                    default:
//                        TooManyNICs toomanynics = new TooManyNICs();
//                        toomanynics.ShowDialog();
//                        toomanynics.Dispose();
//                        break;
//                }

                // let user choose, if more than one network adapter is available - new 4/18/19
                if (niccount == 1)
                {
                    IP_Address = Test_IP_GW(adapters[goodnic]);
                    Set_IPs(IP_Address);
                }
                else
                {
                    NICadapters nicadapters = new NICadapters(adapters);
                    DialogResult dr = nicadapters.ShowDialog();
                    if (dr == System.Windows.Forms.DialogResult.OK)
                    {
                        IP_Address = nicadapters.IP_Address;
                        Set_IPs(IP_Address);
                    }
                    nicadapters.Dispose();
                }

                // initialize the IP Server
                //                if ((tb_Mesh_IP_Address.Text != "") && (tb_Server_Port_Number.Text != "") && (Stations.Count != 0))   // Station list must also be loaded
                if ((tb_Mesh_IP_Address.Text != "") && (tb_Server_Port_Number.Text != ""))
                {
                    MakeVisible(lbl_My_IP_Address, true);
                    MakeVisible(tb_My_IP_Address, true);
                    Server_Init();
                }
                else
                {
                    MakeVisible(lbl_My_IP_Address, false);
                    MakeVisible(tb_My_IP_Address, false);
                    MakeVisible(lbl_Need_Station_List, false);  // 4/4/17
                    MakeVisible(lbl_Server_cannot_Init, true);
                }
            }
            else
            {
                NoNICs nonics = new NoNICs();
                nonics.ShowDialog();
                nonics.Dispose();
                MakeVisible(lbl_My_IP_Address, false);
                MakeVisible(tb_My_IP_Address, false);
                MakeVisible(lbl_Need_Station_List, false);  // 4/4/17
                MakeVisible(lbl_Server_cannot_Init, true);
            }
        }

        void Set_IPs(string IP_Address)
        {
            SetTBtext(tb_Mesh_IP_Address, IP_Address);
            SetTBtext(tb_My_IP_Address, IP_Address);
            SetTBtext(tb_Aid_ThisPC_IP_address, IP_Address);
        }

        public static string Test_IP_GW(NetworkInterface adapter)
        {
            // Test whether the 'adapter' has one and only one IP, Mask and Gateway address
            //  The easiest way to do this is to verify that have exactly one of these by count
            //  But the Mask does not have a count, so just make sure it is not an empty string
            //  Also verify that the IP address family is Internetwork
            //  If the test passes, return the IP address
            IPInterfaceProperties properties = adapter.GetIPProperties();
            int j = 0;
            while (properties.UnicastAddresses[j].Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                j++;
            //            int IPcount = properties.UnicastAddresses.Count;
            int GWAcount = properties.GatewayAddresses.Count;
            string Mask = properties.UnicastAddresses[j].IPv4Mask.ToString();
            //            if ((IPcount != 1) || (GWAcount != 1) || (Mask == ""))
            if ((GWAcount != 1) || (Mask == ""))
                return "   not available";     // 4/4/17 - added text
            else
            {
                //UnicastIPAddressInformation ip = properties.UnicastAddresses[0];
                //{
                //    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                //        return ip.Address.ToString();
                //    else
                //        return "";
                //}
                return properties.UnicastAddresses[j].Address.ToString();
            }
        }

        private void Server_Init()
        {
            // don't try to do it a second time
            if (!Server_Thread_Initted)
            {
                try
                {
                    // start the Server worker thread
                    ServerObj.Server_Not_Initted = lbl_Server_Error;
                    ServerObj.Server_Waiting = lbl_IP_Server_Waiting_Client;
                    ServerObj.Server_Client_Connected = lbl_Server_Client_Connected;
                    ServerObj.Server_Cannot_Init = lbl_Server_cannot_Init;
                    ServerObj.Need_Station_List = lbl_Need_Station_List;
                    ServerObj.Server_Error_Message = tb_Server_Error_Message;
                    ServerObj.Welcome_Message = tb_Welcome_Message;
                    ServerObj.Server_IP_Address = tb_Mesh_IP_Address.Text;
                    ServerObj.Server_Port_Number = Convert.ToInt16(tb_Server_Port_Number.Text);
                    ServerObj.Ethernet_Packets = rtb_Ethernet_Packets;
                    ServerThread.Start();
                    Console.WriteLine("Starting Server worker thread...");
                    Server_Thread_Initted = true;
                    lbl_Server_cannot_Init.Visible = false;
                }
                catch
                {
                    MessageBox.Show("Ethernet Server Port # is Invalid!", "Server Port # Invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        #endregion

        #region Delegate functions
        public void UpdateStationDGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_Stations.InvokeRequired)
            {
                UpdateStationDGVDel d = new UpdateStationDGVDel(UpdateStationDGV);
                dgv_Stations.Invoke(d, new object[] { });
            }
            else
            {
                Bind_DB_Station_DGV();
                Bind_Messages_Station_DGV();
                Application.DoEvents();
            }
        }

        void AppendTBtext(TextBox cntrl, string str)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (cntrl.InvokeRequired)
            {
                SetTextdel d = new SetTextdel(AppendTBtext);
                cntrl.Invoke(d, new object[] { cntrl, str });
            }
            else
            {
                cntrl.Text += str;
                cntrl.Update();
                Application.DoEvents();
            }
        }

        void CheckCB(CheckBox cb, bool checkd)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (cb.InvokeRequired)
            {
                CheckCBdel d = new CheckCBdel(CheckCB);
                cb.Invoke(d, new object[] { cb, checkd });
            }
            else
            {
                cb.Checked = checkd;
                cb.Update();
                Application.DoEvents();
            }
        }

        void CheckRB(RadioButton rb, bool checkd)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (rb.InvokeRequired)
            {
                CheckRBdel d = new CheckRBdel(CheckRB);
                rb.Invoke(d, new object[] { rb, checkd });
            }
            else
            {
                rb.Checked = checkd;
                rb.Update();
                Application.DoEvents();
            }
        }
        #endregion

        #region Runners tab
//        private void btn_Show_All_Runners_Progress_Click(object sender, EventArgs e)
//        {
//            // change the panel displayed
//            MakeVisible(panel_Single_Runner_Progress, false);
//            MakeVisible(panel_All_Runner_Progress, true);

//            // must generate the All Runners Status list for the DGV each time
//            // look at all reported runners
//            AllRunnersStatus.Clear();
//            for (int i=0; i < lb_Runners.Items.Count; i++)
//            {
//                // create a runner entry
//                AllRunnerStatus allrunner = new AllRunnerStatus();
//                allrunner.BibNumber = lb_Runners.Items[i].ToString();

//                // add all the stations to the entry
//                //                allrunner.StationTimes = new StationTime(Stations.Count);
//                StationTime [] st = new StationTime[Stations.Count];
//                int stat = 0;
//                foreach (DB_Station station in Stations)
//                {
//                    StationTime time = new StationTime();
//                    time.Name = station.Name;
//                    //                    allrunner.StationTimes.ad += time;
//                    st[stat] = time;
////                    allrunner.StationTimes[stat] = time;
//                    stat++;
//                }
//                allrunner.StationTimes = st;

//                // add the entry
//                AllRunnersStatus.Add(allrunner);
//            }

//            // now display the dgv
//            Bind_AllRunnerStatus_dgv();
//            btn_Show_All_Runners_Progress.Text = "Refresh\nAll Runners\nStatus";
//        }

        public void Bind_SingleRunnerStatus_dgv(List<RunnerStatus> RunnersStatus)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_Runner_Status.InvokeRequired)
            {
                SetDGVsourceStatusDel d = new SetDGVsourceStatusDel(Bind_SingleRunnerStatus_dgv);
                dgv_Runner_Status.Invoke(d, new object[] { RunnersStatus });
            }
            else
            {
                dgv_Runner_Status.DataSource = null;
                dgv_Runner_Status.DataSource = RunnersStatus;
                dgv_Runner_Status.Columns[0].Width = 75;
                dgv_Runner_Status.Columns[0].HeaderText = "Station";
                dgv_Runner_Status.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runner_Status.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Runner_Status.Columns[1].Width = 68;
                dgv_Runner_Status.Columns[1].HeaderText = "Time In";
                dgv_Runner_Status.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runner_Status.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Runner_Status.Columns[2].Width = 75;
                dgv_Runner_Status.Columns[2].HeaderText = "Time Out";
                dgv_Runner_Status.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runner_Status.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Runner_Status.Columns[3].Width = 66;
//                dgv_Runner_Status.Columns[3].HeaderText = "Time at Station";
                dgv_Runner_Status.Columns[3].HeaderText = "Minutes at Station";
                dgv_Runner_Status.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runner_Status.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Runner_Status.Columns[4].Width = 79;
//                dgv_Runner_Status.Columns[4].HeaderText = "Time from Previous";
                dgv_Runner_Status.Columns[4].HeaderText = "Minutes from Previous";
                dgv_Runner_Status.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runner_Status.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Runner_Status.Columns[5].Width = 68;
//                dgv_Runner_Status.Columns[5].HeaderText = "Time to Next";
                dgv_Runner_Status.Columns[5].HeaderText = "Minutes to Next";
                dgv_Runner_Status.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runner_Status.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Runner_Status.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // test for items to highlight
//                dgv_Runner_Status.Rows[1].Cells[2].Style.ForeColor = Color.Red;     // this is just a test - where shall we use it?
                if (chk_After_Cutoff.Checked)
                {
                    for (int i = 0; i < dgv_Runner_Status.RowCount; i++)
                    {
                    }
                }
                if (chk_Distance_5mph.Checked)
                {
                    for (int i = 0; i < dgv_Runner_Status.RowCount; i++)
                    {
                    }
                }
                if (chk_Stay_10min.Checked)
                {
                    for (int i = 0; i < dgv_Runner_Status.RowCount; i++)
                    {
                        if (dgv_Runner_Status.Rows[i].Cells[3].Value != null)
                        {
                            TimeSpan ts = TimeSpan.Parse("00:" + (string)dgv_Runner_Status.Rows[i].Cells[3].Value);
                            if (ts > TimeSpan.FromMinutes(10))
                                dgv_Runner_Status.Rows[1].Cells[3].Style.ForeColor = Color.Red;
                        }
                    }
                }

                // gray out two columns for stations with less than 2 Log Points
                int gi;
                for (gi = 0; gi < Num_Stations; gi++)
                {
                    if (Stations[gi].Number_of_Log_Points < 2)
                    {
                        if (gi == 0)
                        {
                            dgv_Runner_Status.Rows[gi].Cells[1].Style.BackColor = Color.LightGray;   // Start Time In
                            dgv_Runner_Status.Rows[gi].Cells[4].Style.BackColor = Color.LightGray;   // Start Time from Previous
                        }
                        else
                        {
                            dgv_Runner_Status.Rows[gi].Cells[2].Style.BackColor = Color.LightGray;
                            dgv_Runner_Status.Rows[gi].Cells[2].Value = "";
                        }
                        dgv_Runner_Status.Rows[gi].Cells[3].Style.BackColor = Color.LightGray;
                        dgv_Runner_Status.Rows[gi].Cells[3].Value = "";
                    }
                }
                if (Num_Stations != 0)   // this is needed for action before it is created
                    dgv_Runner_Status.Rows[gi-1].Cells[5].Style.BackColor = Color.LightGray;   // Finish Time to Next

                if (dgv_Runner_Status.CurrentRow != null)   // this is needed for action before it is created
                    dgv_Runner_Status.CurrentRow.Selected = false;

                // change the size of the DGV to hold only the Stations - 4/22/19
                dgv_Runner_Status.ClientSize = new Size(dgv_Runner_Status.Columns.GetColumnsWidth(DataGridViewElementStates.None) + 3, Num_Stations * dgv_Runner_Status.Rows[0].Height + dgv_Runner_Status.ColumnHeadersHeight + 2);   // 4/22/19

                dgv_Runner_Status.Update();
            }
        }

//        public void Bind_AllRunnerStatus_dgv()
//        {
//            // InvokeRequired required compares the thread ID of the
//            // calling thread to the thread ID of the creating thread.
//            // If these threads are different, it returns true.
//            if (dgv_All_Runner_Status.InvokeRequired)
//            {
//                SetDGVsourceAllStatusDel d = new SetDGVsourceAllStatusDel(Bind_AllRunnerStatus_dgv);
////                dgv_All_Runner_Status.Invoke(d, new object[] { RunnersStatus });
//                dgv_All_Runner_Status.Invoke(d, new object[] { });
//            }
//            else
//            {
//                dgv_All_Runner_Status.DataSource = null;
//                dgv_All_Runner_Status.DataSource = AllRunnersStatus;
//                dgv_All_Runner_Status.Columns[0].Width = 38;
//                dgv_All_Runner_Status.Columns[0].HeaderText = "Bib";
//                dgv_All_Runner_Status.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
//                dgv_All_Runner_Status.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
//                dgv_All_Runner_Status.Columns[0].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
//                dgv_All_Runner_Status.Update();
//            }
//        }

        public void AddRunnerInLB(string BibNumber)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lb_Runners.InvokeRequired)
            {
                AddRunnerInDel d = new AddRunnerInDel(AddRunnerInLB);
                lb_Runners.Invoke(d, new object[] { BibNumber });
            }
            else
            {
                lb_Runners.Items.Add(BibNumber);     // add to the runners listbox
// 7/22/17                lb_Runners.Sorted = true;           // 7/22/17 - this did not work because it doing it alphabetically, not numerically
                SortListboxNumeric(lb_Runners);         // 7/22/17
                SetTBtext(tb_NumRunnersIn, lb_Runners.Items.Count.ToString());
                Application.DoEvents();
            }
        }

        private void SortListboxNumeric(ListBox lb)     // added 7/22/17
        {
            // Get the original items as an array
            int num_items = lb.Items.Count;
            object[] items = new object[num_items];
            for (int i = 0; i < num_items; i++)
                items[i] = Convert.ToInt16(lb.Items[i]);

            // sort them numerically
            Array.Sort(items);
            string[] strgs = new string[num_items];
            //            Array.Copy(items, strgs, num_items);
            for (int i = 0; i < num_items; i++)
                strgs[i] = items[i].ToString();
            lb.Sorted = false;
            lb.Items.Clear();
//            lb.Items.AddRange(items);       // this forces the Listbox to contain integers, not strings - thus need to Find using an integer
            lb.Items.AddRange(strgs);       // this forces the Listbox to contain integers, not strings - thus need to Find using an integer
        }

        void AddUpdateRunner(DB_Runner runner, bool In)      // if not already in list then add
        {
            // provide protection for overlapping requests when reading a previous XML file
            lock (RunnerDictionary)
            {
                // make the Runner Status datagrid visible
                MakeVisible(panel_Single_Runner_Progress, true);

                // find the index of the Station & # of Log Points
                int index = Find_Station(runner.Station);
                int LogPts = 0;         // 7/22/17
                if (index != -1)        // 7/22/17
//                int LogPts = Stations[index].Number_of_Log_Points;
                    LogPts = Stations[index].Number_of_Log_Points;      // 7/22/17

                // test if this Runner is already in the list
                if (lb_Runners.FindStringExact(runner.BibNumber.ToString()) != -1)
                {       // already exists - TimeIn or TimeOut must be new
                    if (In)
                    {
                        RunnerDictionary[runner.BibNumber].StationReports[index].TimeIn = runner.TimeIn;
                        if (LogPts < 2)     // no 2nd Log Point
                            RunnerDictionary[runner.BibNumber].StationReports[index].TimeOut = runner.TimeIn;
                    }
                    else
                        RunnerDictionary[runner.BibNumber].StationReports[index].TimeOut = runner.TimeOut;
                    DataFile.Add_Runner_Time(tb_DB_Settings_RunnersDataFile.Text, runner);
                }
                else
                {       // add a new runner
                    AddRunnerInLB(runner.BibNumber);
                    if (!Find_Runner_in_RunnerList(runner.BibNumber))
                        Runners_Red_Showing = true;

                    // add it to the Runner Dictionary
                    RunnerData RD = new RunnerData();
                    RD.BibNumber = runner.BibNumber;
                    RD.StationReports = new StationReport[Num_Stations];
                    for (int i = 0; i < Num_Stations; i++)
                    {
                        StationReport SR = new StationReport();
                        SR.Station = Stations[i].Name;
                        SR.TimeIn = "";
                        SR.TimeOut = "";
                        SR.TimeAtStation = "";
                        SR.TimeToPrev = "";
                        SR.TimeToNext = "";
                        RD.StationReports[i] = SR;  // set all the StationReports to be empty
                    }
                    if (index != -1)        // 7/22/17
                    {
                        StationReport SRnew = new StationReport();    // now add in the new report
                        SRnew.Station = runner.Station;
                        if (In)
                        {
                            SRnew.TimeIn = runner.TimeIn;
                            //                    if (LogPts < 2)
                            if (LogPts == 2)     // if there are not 2 Log Points, then set TimeOut = TimeIn
                                SRnew.TimeOut = "";
                            else
                                SRnew.TimeOut = runner.TimeIn;
                        }
                        else
                        {
                            SRnew.TimeIn = "";
                            SRnew.TimeOut = runner.TimeOut;
                        }
                        RD.StationReports[index] = SRnew;
                    }
                    RunnerDictionary.Add(runner.BibNumber, RD);
                    DataFile.Add_Runner_Time(tb_DB_Settings_RunnersDataFile.Text, runner);

                    // increment the number of Runners reported
                    SetTBtext(tb_Number_Runners, lb_Runners.Items.Count.ToString());
                }

                // update the grid
                lb_Runners_SelectedIndexChanged(null, null);
// 9/1/16                Bind_AllRunnerStatus_dgv();
            }
        }

        private void lb_Runners_SelectedIndexChanged(object sender, EventArgs e)
        {
            LB_Run_Sel_Changed();
        }

        private void lb_Runners_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == lb_Runners.SelectedIndex)
                e.Graphics.DrawString(lb_Runners.Items[e.Index].ToString(), lb_Runners.Font, Brushes.White, e.Bounds);
            else
            {
                if (Find_Runner_in_RunnerList(lb_Runners.Items[e.Index].ToString()))
                    e.Graphics.DrawString(lb_Runners.Items[e.Index].ToString(), lb_Runners.Font, Brushes.Black, e.Bounds);
                else
                    e.Graphics.DrawString(lb_Runners.Items[e.Index].ToString(), lb_Runners.Font, Brushes.Red, e.Bounds);
            }
        }

        public void LB_Run_Sel_Changed()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lb_Runners.InvokeRequired)
            {
                LB_Run_Sel_ChangedDel d = new LB_Run_Sel_ChangedDel(LB_Run_Sel_Changed);
                lb_Runners.Invoke(d, new object[] { });
            }
            else
            {
                if (lb_Runners.SelectedIndex != -1)
                {
                    // determine the selected runner
                    string Runnernumber = lb_Runners.SelectedItem.ToString();

                    // get his status
                    RunnersStatus = Get_Runner_Status(Runnernumber);
                    
                    //// build his status list
                    //RunnersStatus.Clear();
                    //Dictionary<string, RunnerData>.KeyCollection keyColl = RunnerDictionary.Keys;
                    //foreach (string s in keyColl)
                    //{
                    //    if (s == Runnernumber)
                    //    {
                    //        for (int i = 0; i < Num_Stations; i++)
                    //        {
                    //            // put in what we already know
                    //            RunnerStatus rs = new RunnerStatus();
                    //            rs.Station = RunnerDictionary[s].StationReports[i].Station;
                    //            rs.TimeIn = RunnerDictionary[s].StationReports[i].TimeIn;
                    //            rs.TimeOut = RunnerDictionary[s].StationReports[i].TimeOut;

                    //            // calculate the other things that we can
                    //            if ((rs.TimeIn != "") && (rs.TimeOut != ""))
                    //            {
                    //                // calculate 'at station' time
                    //                TimeSpan time = DateTime.Parse(rs.TimeOut).Subtract(DateTime.Parse(rs.TimeIn));
                    //                rs.TimeAtStation = time.TotalMinutes.ToString();
                    //            }
                    //            if ((i > 0) && (RunnerDictionary[s].StationReports[i - 1].TimeOut != "") && (rs.TimeIn != ""))
                    //            {
                    //                // calculate time to Previous station
                    //                TimeSpan time = DateTime.Parse(rs.TimeIn).Subtract(DateTime.Parse(RunnerDictionary[s].StationReports[i - 1].TimeOut));
                    //                rs.TimeToPrev = time.TotalMinutes.ToString();
                    //            }
                    //            if ((i < Num_Stations) && (rs.TimeOut != "") && (RunnerDictionary[s].StationReports[i + 1].TimeIn != ""))
                    //            {
                    //                // calculate time to Next station
                    //                TimeSpan time = DateTime.Parse(RunnerDictionary[s].StationReports[i + 1].TimeIn).Subtract(DateTime.Parse(rs.TimeOut));
                    //                rs.TimeToNext = time.TotalMinutes.ToString();
                    //            }
                    //            if ((i > 0) && (rs.TimeIn != "") && (RunnerDictionary[s].StationReports[i - 1].TimeOut == ""))
                    //            {
                    //                // cannot highlight yet, dgv has not been created                                dgv_Runner_Status.Rows[i - 1].Cells[2].Style.ForeColor = Color.Red;
                    //            }

                    //            // now add it
                    //            RunnersStatus.Add(rs);
                    //        }
                    //    }
                    //}

                    // put in the grid
                    Bind_SingleRunnerStatus_dgv(RunnersStatus);
                    lb_Runners.Refresh();

                    // also show if this runner is in one of the lists - added 7/22/17
                    MakeVisible(lbl_DB_Runner_in_DNFList, false);
                    MakeVisible(lbl_DB_Runner_in_DNSList, false);
                    MakeVisible(lbl_DB_Runner_in_WatchList, false);
                    if (Find_Runner_in_DNS(Runnernumber))
                    {
                        MakeVisible(lbl_DB_Runner_in_DNSList, true);
                    }
                    if (Find_Runner_in_DNF(Runnernumber))
                    {
                        MakeVisible(lbl_DB_Runner_in_DNFList, true);
                    }
                    if (Find_Runner_in_Watch(Runnernumber))
                    {
                        MakeVisible(lbl_DB_Runner_in_WatchList, true);
                    }
                }
            }
        }

        public static List<RunnerStatus> Get_Runner_Status(string Runnernumber)
        {
            List<RunnerStatus> RunnersStatusLocal = new List<RunnerStatus>();

            // build his status list
            Dictionary<string, RunnerData>.KeyCollection keyColl = RunnerDictionary.Keys;
            foreach (string s in keyColl)
            {
                if (s == Runnernumber)
                {
                    for (int i = 0; i < Num_Stations; i++)
                    {
                        // put in what we already know
                        RunnerStatus rs = new RunnerStatus();
                        rs.Station = RunnerDictionary[s].StationReports[i].Station;
                        rs.TimeIn = RunnerDictionary[s].StationReports[i].TimeIn;
                        rs.TimeOut = RunnerDictionary[s].StationReports[i].TimeOut;

                        // calculate 'at station' time
                        if ((rs.TimeIn != "") && (rs.TimeOut != ""))
                        {
                            TimeSpan time = DateTime.Parse(rs.TimeOut).Subtract(DateTime.Parse(rs.TimeIn));
                            rs.TimeAtStation = time.TotalMinutes.ToString();
                        }

                        // calculate time to Previous station
                        if ((i > 0) && (RunnerDictionary[s].StationReports[i - 1].TimeOut != "") && (rs.TimeIn != ""))
                        {
                            TimeSpan time = DateTime.Parse(rs.TimeIn).Subtract(DateTime.Parse(RunnerDictionary[s].StationReports[i - 1].TimeOut));
                            rs.TimeFromPrev = time.TotalMinutes.ToString();
                        }

                        // calculate time to Next station
                        if ((i < Num_Stations - 1) && (rs.TimeOut != "") && (RunnerDictionary[s].StationReports[i+1].TimeIn != ""))
                        {
                            TimeSpan time = DateTime.Parse(RunnerDictionary[s].StationReports[i+1].TimeIn).Subtract(DateTime.Parse(rs.TimeOut));
                            rs.TimeToNext = time.TotalMinutes.ToString();
                        }
                        //if ((i > 0) && (rs.TimeIn != "") && (RunnerDictionary[s].StationReports[i - 1].TimeOut == ""))
                        //{
                        //    // cannot highlight yet, dgv has not been created                                dgv_Runner_Status.Rows[i - 1].Cells[2].Style.ForeColor = Color.Red;
                        //}

                        // now add it
                        RunnersStatusLocal.Add(rs);
                    }
                }
            }
            return RunnersStatusLocal;
        }

        #region Checkboxes
        private void cb_Distance_5mph_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Distance_5mph.Checked)
            {
                Distance_5mph = true;
                Save_Registry("Highlight Distance covered less than 5mph", "Yes");
            }
            else
            {
                Distance_5mph = false;
                Save_Registry("Highlight Distance covered less than 5mph", "No");
            }
            Bind_SingleRunnerStatus_dgv(RunnersStatus);
        }

        private void cb_Stay_10min_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Stay_10min.Checked)
            {
                Stay_10min = true;
                Save_Registry("Highlight Staying at Station longer than 10min", "Yes");
            }
            else
            {
                Stay_10min = false;
                Save_Registry("Highlight Staying at Station longer than 10min", "No");
            }
            Bind_SingleRunnerStatus_dgv(RunnersStatus);
        }

        private void cb_After_Cutoff_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_After_Cutoff.Checked)
            {
                After_Cutoff = true;
                Save_Registry("Highlight Arrive/Leave after Station Cuttoff", "Yes");
            }
            else
            {
                After_Cutoff = false;
                Save_Registry("Highlight Arrive/Leave after Station Cuttoff", "No");
            }
            Bind_SingleRunnerStatus_dgv(RunnersStatus);
        }
        #endregion
        #endregion

        #region Stations tab
        // Function to read in the Station data file (.csv or .txt suffix)
        // Returns true if loaded, false if file not accessible
        bool Load_Stations(string FileName)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            StreamReader reader;

            try
            {
                reader = File.OpenText(FileName);
            }
            catch
            {
                MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            Stations.Clear();
            string last_name = string.Empty;
            string first_name = string.Empty;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                Parts = line.Split(splitter);
                if (!Parts[0].StartsWith("*"))
                {
                    DB_Station station = new DB_Station();
                    station.Name = Parts[0].TrimStart(front);
                    station.Number = Convert.ToInt16(Parts[1]);
                    station.Latitude = Convert.ToDouble(Parts[2]);
                    station.Longitude = Convert.ToDouble(Parts[3]);
                    station.Previous = Parts[4].TrimStart(front);
                    station.DistPrev = Convert.ToDouble(Parts[5]);
                    station.Next = Parts[6].TrimStart(front);
                    station.DistNext = Convert.ToDouble(Parts[7]);
                    station.Difficulty = Convert.ToDouble(Parts[8]);
                    string access = Parts[9].TrimStart(front);
                    if ((access == "Y") || (access == "y") || (access == "yes") || (access == "Yes") || (access == "YES"))
                    {
                        station.Accessible = true;
                    }
                    station.Number_of_Log_Points = Convert.ToInt16(Parts[10]);
                    if (Parts[11] != "")
                        station.First_Runner_Time = Convert.ToDateTime(Parts[11]);
                    if (Parts[12] != "")
                        station.Cuttoff_Time = Convert.ToDateTime(Parts[12]);
                    Stations.Add(station);      // add to the Stations list

                    // also add to the Testing listboxes
                    lb_Stations_Testing_Entering.Items.Add(station.Name);
                    lb_Stations_Testing_Leaving.Items.Add(station.Name);
                    if (first_name == "")
                        first_name = station.Name;
                    else
                        last_name = station.Name;
                }
            }
            lbl_Stations_file_Loaded.Visible = true;
            lbl_Stations_Info_File_Loaded.Visible = true;

            // remove the first station (Start) from the Entering list and the last station (Finish) from the Leaving List
            lb_Stations_Testing_Entering.Items.Remove(first_name);
            lb_Stations_Testing_Leaving.Items.Remove(last_name);

            // make the Station saving impossible
            Make_Saving_Possible(false);

            // remove the label telling it needs to be done
            MakeVisible(lbl_Need_Station_List, false);

            return true;
        }

        bool Save_Stations(List<DB_Station> Stations, string FileName)
        {
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (Stations.Count == 0)
            {
                MessageBox.Show("Station List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
//// removed 5/6/15                DialogResult result = MessageBox.Show("The Save file:\n\n" +
//                                        FileName +
//                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
//                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
//                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
//                else
//                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "* The file used to store the Station data could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (13 items)" + Environment.NewLine +
                            "* Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station," + Environment.NewLine +
                            "* Next station, distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)," + Environment.NewLine +
                            "* # of Log Points, First runner expected Time, Cutoff Time" + Environment.NewLine;
            writer.Write(header);

            // save each station
            foreach (DB_Station station in Stations)
            {
                // Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station, Next station,
                // distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)
                string line = station.Name + ",";
                line += station.Number + ",";
                line += station.Latitude.ToString() + ",";
                line += station.Longitude.ToString() + ",";
                line += station.Previous + ",";
                line += station.DistPrev.ToString() + ",";
                line += station.Next + ",";
                line += station.DistNext.ToString() + ",";
                line += station.Difficulty.ToString() + ",";
                if (station.Accessible)
                    line += "Yes,";
                else
                    line += "No,";
                line += station.Number_of_Log_Points.ToString() + ",";
                line += station.First_Runner_Time.ToString() + ",";
                line += station.Cuttoff_Time.ToString();
                writer.WriteLine(line);
            }
            writer.Close();

            // make the Station saving impossible
            Make_Saving_Possible(false);

            // this file name to the Load filename textbox
            SetTBtext(tb_Station_Info_Filename, FileName);

            // save the new name
            Save_Registry("Stations Info File", FileName);

            // remove the label telling it needs to be done
            MakeVisible(lbl_Need_Station_List, false);

// why do this here???            // init the Server Worker Thread if not already done
//            if (!Server_Thread_Initted)
//                Server_Init();

            return true;
        }

        private void tb_Station_Info_Filename_TextChanged(object sender, EventArgs e)
        {
            if (tb_Station_Info_Filename.Text == "")
            {
                tb_Station_Info_Filename.BackColor = Color.FromArgb(255, 128, 128);
                tb_Stations_Info_Filename.Text = "";
            }
            else
            {
                tb_Station_Info_Filename.BackColor = Color.FromKnownColor(KnownColor.Window);
                tb_Stations_Info_Filename.Text = tb_Station_Info_Filename.Text;
                Stations_Info_Filename = tb_Station_Info_Filename.Text;
            }
        }

        private void Bind_DB_Station_DGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_Stations.InvokeRequired)
            {
                BindStationDGVDel d = new BindStationDGVDel(Bind_DB_Station_DGV);
                dgv_Stations.Invoke(d, new object[] { });
            }
            else
            {
                Binding_Stations_DGV = true;
                dgv_Stations.DataSource = null;
                dgv_Stations.DataSource = Stations;
                dgv_Stations.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[0].Width = Station_DGV_Width;     // Name
                dgv_Stations.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[0].HeaderText = "Station Name";
                dgv_Stations.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[1].Width = 20;     // Number
                dgv_Stations.Columns[1].HeaderText = "#";
                if (Show_LatLong)
                {
                    dgv_Stations.Columns[2].Width = 50;     // Latitude
                    dgv_Stations.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_Stations.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgv_Stations.Columns[3].Width = 55;     // Longitude
                    dgv_Stations.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_Stations.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                else
                {
                    dgv_Stations.Columns[2].Visible = false;
                    dgv_Stations.Columns[3].Visible = false;
                }
                dgv_Stations.Columns[4].Width = Station_DGV_Width;     // Previous
                dgv_Stations.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[4].HeaderText = "Previous Station";
                dgv_Stations.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[5].Width = 51;
                dgv_Stations.Columns[5].HeaderText = "Dist. to Previous";
                dgv_Stations.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[6].Width = Station_DGV_Width;     // Next
                dgv_Stations.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[6].HeaderText = "Next Station";
                dgv_Stations.Columns[6].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[7].Width = 51;
                dgv_Stations.Columns[7].HeaderText = "Distance to Next";
                dgv_Stations.Columns[7].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[7].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[8].Width = 52;     // Difficulty
                dgv_Stations.Columns[9].Width = 63;     // Accessible
                dgv_Stations.Columns[10].Width = 40;    // Active
                dgv_Stations.Columns[11].Width = 48;    // Number of Log points (1 or 2)
                dgv_Stations.Columns[11].HeaderText = "# of Log pts";
                dgv_Stations.Columns[11].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[11].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[12].HeaderText = "First Runner Expected";
                dgv_Stations.Columns[12].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[12].DefaultCellStyle.Format = "HH:mm";
                dgv_Stations.Columns[12].Width = 55;
                dgv_Stations.Columns[12].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[13].HeaderText = "Cutoff Time";
                dgv_Stations.Columns[13].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[13].DefaultCellStyle.Format = "HH:mm";
                dgv_Stations.Columns[13].Width = 55;
                dgv_Stations.Columns[13].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[14].Width = 57;
                dgv_Stations.Columns[14].HeaderText = "Medium";
                dgv_Stations.Columns[14].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[14].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[15].Width = 125;   // 1/19/16
                dgv_Stations.Columns[15].HeaderText = "IP Address/Callsign";
                dgv_Stations.Columns[15].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Stations.Columns[15].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Stations.Columns[16].Visible = false;   // # of Incoming messages
                dgv_Stations.Columns[17].Visible = false;   // # of Outgoing messages
                dgv_Stations.Columns[18].Visible = false;   // IP StationWorker
                dgv_Stations.Columns[19].Visible = false;   // Packet StationWorker
                dgv_Stations.Columns[20].Visible = false;   // APRS StationWorker
                dgv_Stations.Columns[21].Visible = false;   // Pipe Handle - 3/27/19
                dgv_Stations.Update();

                // total the miles and the active station count
                double miles = 0;
                int Active = 0;
                for (int i = 0; i < Stations.Count; i++)
                {
                    miles += Stations[i].DistNext;
                    if (Stations[i].Active)
                        Active++;
                }
                SetTBtext(tb_Total_Miles, miles.ToString("F2"));
                Num_Stations = Stations.Count;    // and display the number of stations
                SetTBtext(tb_Number_Stations, Num_Stations.ToString());
                SetTBtext(tb_Number_of_Stations, Num_Stations.ToString());
                SetTBtext(tb_Number_of_Active_Stations, Active.ToString());
                if (Active == 0)
                {       // No stations connected
                    if (ConnectViaEthernet)
                    {
                        if (Server_Thread_Initted)
                            MakeVisible(lbl_IP_Server_Waiting_Client, true);
                        MakeVisible(lbl_Server_Client_Connected, false);
                    }
                    if (ConnectViaPacket)
                    {
                        if ((DB_AGWSocket != null) && (DB_AGWSocket.Registered))
                            MakeVisible(lbl_Waiting_Packet_Stations, true);
                        MakeVisible(lbl_Packet_Stations_Connected, false);
                    }
                    MakeVisible(btn_Create_New_Message, false);
                }
                else
                {       // at least one station is connected
                    if (ConnectViaEthernet)
                    {
                        MakeVisible(lbl_Server_Client_Connected, true);
                        MakeVisible(lbl_IP_Server_Waiting_Client, false);
                        MakeVisible(lbl_Need_Station_List, false);      // 8/3/16
                    }
                    if (ConnectViaPacket)
                    {
                        MakeVisible(lbl_Waiting_Packet_Stations, false);
                        MakeVisible(lbl_Packet_Stations_Connected, true);
                    }
                    MakeVisible(btn_Create_New_Message, true);
                }
                Binding_Stations_DGV = false;
            }
        }

        private void Make_Saving_Possible(bool state)
        {
            if (!Init_Registry && !Binding_Stations_DGV)
            {
                MakeVisible(btn_Save_Station_Changes, state);
                MakeVisible(tb_Save_Station_Info_Filename, state);
                SetTBtext(tb_Save_Station_Info_Filename, tb_Station_Info_Filename.Text);
                MakeVisible(btn_Browse_Save_Stations_Info_Filename, state);
            }
        }

        private void chk_Make_Editable_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Make_Editable.Checked)
            {
                dgv_Stations.ReadOnly = false;
                MakeEnabled(btn_Add_Station, true);
                MakeEnabled(btn_Clear_Station_List, true);
                MakeEnabled(btn_Add_a_Station, true);
// 4/24/19                MakeEnabled(btn_Move_Station_Down, true);
// 4/24/19                MakeEnabled(btn_Move_Station_Up, true);
                MakeVisible(btn_Move_Station_Down, true);   // 4/24/19
                MakeVisible(btn_Move_Station_Up, true); // 4/24/19
            }
            else
            {
                dgv_Stations.ReadOnly = true;
                MakeEnabled(btn_Add_Station, false);
                MakeEnabled(btn_Clear_Station_List, false);
                MakeEnabled(btn_Add_a_Station, false);
// 4/24/19                MakeEnabled(btn_Move_Station_Down, false);
// 4/24/19                MakeEnabled(btn_Move_Station_Up, false);
                MakeVisible(btn_Move_Station_Down, false);  // 4/24/19
                MakeVisible(btn_Move_Station_Up, false);    // 4/24/19
            }
        }

        public static int Find_Station(string StationName)     // returns index into Stations list: -1 = not found
        {
            if (Stations.Count != 0)
            {
                int index = Stations.FindIndex(
                    delegate(DB_Station station)
                    {
                        return station.Name == StationName;
                    });
                return index;
            }
            else
                return -1;
        }

        private void dgv_Stations_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // make the Station saving possible
            Make_Saving_Possible(true);
        }

        private void dgv_Stations_CellDoubleClick(object sender, DataGridViewCellEventArgs e)   // 3/27/19
        {
            if ((e.ColumnIndex == 1) && (e.RowIndex == -1))
            {
            }
        }

        private void chk_DB_Show_LatLong_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_DB_Show_LatLong.Checked)
            {
                Show_LatLong = true;
                Save_Registry("Show Lat/Long", "Yes");
            }
            else
            {
                Show_LatLong = false;
                Save_Registry("Show Lat/Long", "No");
            }
            if (!Init_Registry)
                Bind_DB_Station_DGV();
        }

        #region Buttons
        private void btn_Browse_Station_Info_Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SetTBtext(tb_Station_Info_Filename, ofd.FileName);
            }
        }

        private void btn_Load_Station_Info_Filename_Click(object sender, EventArgs e)
        {
            if (tb_Station_Info_Filename.Text != "")
            {
                // save the new name
                Save_Registry("Stations Info File", tb_Station_Info_Filename.Text);

                // load the file data
                Load_Stations(tb_Station_Info_Filename.Text);

                // put the data into the DataGridView
                if (Stations.Count != 0)
                {
                    Bind_DB_Station_DGV();
                    Bind_Messages_Station_DGV();
                }
            }
        }

        private void btn_Clear_Station_List_Click(object sender, EventArgs e)
        {
            Stations.Clear();
            dgv_Stations.DataSource = null;
        }

        private void btn_Add_a_Station_Click(object sender, EventArgs e)
        {
            Add_Station_Name("");
        }

        private void btn_Move_Station_Up_Click(object sender, EventArgs e)
        {
            // determine position in the list
            int index = dgv_Stations.CurrentRow.Index;

            // move up only if it is not the top row
            if (index != 0)
            {
                // make the Station saving possible
                Make_Saving_Possible(true);

                // swap position in list
                DB_Station temp = Stations[index];
                Stations[index] = Stations[index - 1];
                Stations[index - 1] = temp;

                // swap previous and next entries for three stations each
                if (index != 1)
                {
                    Stations[index - 1].Previous = Stations[index - 2].Name;
                    Stations[index - 2].Next = Stations[index - 1].Name;
                }
                else
                {
                    Stations[index - 1].Previous = "";
                }
                Stations[index].Previous = Stations[index - 1].Name;
                Stations[index - 1].Next = Stations[index].Name;
                if ((index + 1) != Stations.Count)
                {
                    Stations[index + 1].Previous = Stations[index].Name;
                    Stations[index].Next = Stations[index + 1].Name;
                }
                else
                {
                    Stations[index].Next = "";
                }

                // if distances and difficulty exist, change the value to negative, for three stations
                // this alerts the operator that changes have been made and more changes are needed
                for (int i = index - 2; i <= index; i++)
                {
                    if (i < 0)
                        continue;
                    if (Stations[i].DistNext != 0)
                    {
                        Stations[i].DistNext = -(Stations[i].DistNext);
                    }
                    if (Stations[i].Difficulty != 0)
                    {
                        Stations[i].Difficulty = -(Stations[i].Difficulty);
                    }
                }

                // repaint the DGV
                Bind_DB_Station_DGV();
                Bind_Messages_Station_DGV();
            }
        }

        private void btn_Move_Station_Down_Click(object sender, EventArgs e)
        {
            // determine position in the list
            int index = dgv_Stations.CurrentRow.Index;

            // move down only if it is not the bottom row
            if ((index + 1) < Stations.Count)
            {
                // make the Station saving possible
                Make_Saving_Possible(true);

                // swap position in list
                DB_Station temp = Stations[index];
                Stations[index] = Stations[index + 1];
                Stations[index + 1] = temp;

                // swap previous and next entries for three stations each
                if (index != 0)
                {
                    Stations[index].Previous = Stations[index - 1].Name;
                    Stations[index - 1].Next = Stations[index].Name;
                }
                else
                {
                    Stations[index].Previous = "";
                }
                Stations[index + 1].Previous = Stations[index].Name;
                Stations[index].Next = Stations[index + 1].Name;
                if ((index + 2) < Stations.Count)
                {
                    Stations[index + 2].Previous = Stations[index + 1].Name;
                    Stations[index + 1].Next = Stations[index + 2].Name;
                }
                else
                {
                    Stations[index + 1].Next = "";
                }

                // if distances and difficulty exist, change the value to negative, for three stations
                for (int i = index - 1; i <= index + 1; i++)
                {
                    if (i < 0)
                        continue;
                    if (Stations[i].DistNext != 0)
                    {
                        Stations[i].DistNext = -(Stations[i].DistNext);
                    }
                    if (Stations[i].Difficulty != 0)
                    {
                        Stations[i].Difficulty = -(Stations[i].Difficulty);
                    }
                }

                // repaint the DGV
                Bind_DB_Station_DGV();
                Bind_Messages_Station_DGV();
            }
        }

        private void btn_Save_Station_Changes_Click(object sender, EventArgs e)
        {
            if (tb_Save_Station_Info_Filename.Text != "")
                Save_Stations(Stations, tb_Save_Station_Info_Filename.Text);
            else
                MessageBox.Show("File name must be entered!", "Need file name", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void btn_Browse_Save_Stations_Info_Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
//                tb_Save_Station_Info_Filename.Text = ofd.FileName;
                SetTBtext(tb_Save_Station_Info_Filename, ofd.FileName);
            }
        }

        private void Add_Station_Name(string Name)
        {
            DB_Station station = new DB_Station();

            // make the Station saving possible
            Make_Saving_Possible(true);

            // put in some initial data
            if (Stations.Count != 0)
            {
                station.Previous = Stations[Stations.Count - 1].Name;
                Stations[Stations.Count - 1].Next = Name;
            }
            station.Name = Name;

            // add it to the list and display it
            Stations.Add(station);
            AddtoLogFile("Added new station to Station List: " + station);
            Bind_DB_Station_DGV();
            Bind_Messages_Station_DGV();
        }
        #endregion
        #endregion

        #region Messages tab
        #region Buttons
        private void btn_Cancel_Send_Reply_Click(object sender, EventArgs e)
        {
            tabControl_DB_Main.Focus();        // this takes the focus away from the clock time display
            tb_Message_Reply.Visible = false;
//            lbl_Selected_Message.Visible = false;
            lbl_Selected_Message.Visible = true;
            lbl_Message_Reply.Visible = false;
//            btn_Start_a_Reply.Visible = true;
            btn_Start_a_Reply.Visible = false;
            btn_Create_New_Message.Visible = true;
            btn_Send_Message_Reply.Visible = false;
            btn_Cancel_Send_New_Messaage.Visible = false;
            btn_Cancel_Send_Reply.Visible = false;
        }

        private void btn_Cancel_Send_New_Messaage_Click(object sender, EventArgs e)
        {
            tabControl_DB_Main.Focus();        // this takes the focus away from the clock time display
            tb_Message_Reply.Visible = false;
            lbl_Selected_Message.Visible = false;
            lbl_Message_Reply.Visible = false;
            lbl_New_Message.Visible = false;
            btn_Create_New_Message.Visible = true;
            btn_Cancel_Send_New_Messaage.Visible = false;
            btn_Send_New_Message.Visible = false;
            tb_New_Message.Visible = false;
            rb_Send_Message_to_Selected.Visible = false;
            rb_Send_Message_to_All.Visible = false;
        }

        private void btn_Send_Message_Reply_Click(object sender, EventArgs e)
        {
            int index = dgv_Messages_Stations.CurrentRow.Index;
            DB_Station station = Stations[index];
            //btn_Send_Message_Reply.ForeColor = Color.Red;
            //string text = btn_Send_Message_Reply.Text;
            //btn_Send_Message_Reply.Text = "Sending";
            //btn_Send_Message_Reply.Update();
            Send_Message(station, tb_Message_Reply.Text);
            //btn_Send_Message_Reply.ForeColor = Color.Black;
            //btn_Send_Message_Reply.Text = text;
            //btn_Send_Message_Reply.Update();
            station.Number_Outgoing_Messages++;
            Bind_Messages_Station_DGV();
            btn_Cancel_Send_Reply_Click(null, null);     // clear the labels and textbox
        }

        private void btn_Start_a_Reply_Click(object sender, EventArgs e)
        {
            btn_Cancel_Send_Reply.Visible = true;
            btn_Cancel_Send_Reply.Focus();        // this takes the focus away from the clock time display
            tb_Message_Reply.Clear();
            tb_Message_Reply.Visible = true;
            lbl_Selected_Message.Visible = true;
            lbl_Message_Reply.Visible = true;
            btn_Start_a_Reply.Visible = false;
            btn_Create_New_Message.Visible = false;
            tb_Message_Reply.Focus();
        }

        private void btn_Create_New_Message_Click(object sender, EventArgs e)
        {
// 9/5/15            btn_Cancel_Send_New_Messaage.Focus();        // this takes the focus away from the clock time display
// 7/17/17            btn_Start_a_Reply.Visible = false;
            MakeVisible(btn_Start_a_Reply, false);
// 7/17/17            btn_Create_New_Message.Visible = false;
            MakeVisible(btn_Create_New_Message, false);
            tb_New_Message.Clear();
// 7/17/17            rb_Send_Message_to_All.Visible = true;
            MakeVisible(rb_Send_Message_to_All,true);
// 9/6/15            rb_Send_Message_to_All.Focus();        // this takes the focus away from the clock time display
// 7/17/17            rb_Send_Message_to_Selected.Visible = true;
            MakeVisible(rb_Send_Message_to_Selected, true);
// 7/17/17            rb_Send_Message_to_All.Checked = false;
            MakeRBChecked(rb_Send_Message_to_All, false);
// 7/17/17            rb_Send_Message_to_Selected.Checked = false;
            MakeRBChecked(rb_Send_Message_to_Selected, false);
        }

        private void btn_Send_New_Message_Click(object sender, EventArgs e)
        {
            // determine if for a single station or to all stations
            if (rb_Send_Message_to_Selected.Checked)
            {       // single station
                int index = dgv_Messages_Stations.CurrentRow.Index;
                DB_Station station = Stations[index];
                if (station.Active)
                {
                    string text = btn_Send_New_Message.Text;
//                    btn_Send_New_Message.Text = "Sending";
                    SetCtlText(btn_Send_New_Message, "Sending");
//                    btn_Send_New_Message.ForeColor = Color.Red;
                    SetCtlForeColor(btn_Send_New_Message, Color.Red);
//                    btn_Send_New_Message.Update();
                    Send_Message(station, tb_New_Message.Text);
//                    btn_Send_New_Message.ForeColor = Color.Black;
                    SetCtlForeColor(btn_Send_New_Message, Color.Black);
//                    btn_Send_New_Message.Text = text;
                    SetCtlText(btn_Send_New_Message, text);
  //                  btn_Send_New_Message.Update();
                    btn_Cancel_Send_New_Messaage_Click(null, null); // clear the labels and textbox
                    station.Number_Outgoing_Messages++;
                    Bind_Messages_Station_DGV();
                }
                else
                    MessageBox.Show("Selected station is not Active", "Station not Active", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {       // send to all active stations
                string text = btn_Send_New_Message.Text;
//                btn_Send_New_Message.Text = "Sending";
                SetCtlText(btn_Send_New_Message, "Sending");
//                btn_Send_New_Message.ForeColor = Color.Red;
                SetCtlForeColor(btn_Send_New_Message, Color.Red);
  //              btn_Send_New_Message.Update();
                Send_Message_All_Active(tb_New_Message.Text);
//                btn_Send_New_Message.ForeColor = Color.Black;
                SetCtlForeColor(btn_Send_New_Message, Color.Black);
//                btn_Send_New_Message.Text = text;
                SetCtlText(btn_Send_New_Message, text);
//                btn_Send_New_Message.Update();
                btn_Cancel_Send_New_Messaage_Click(null, null); // clear the labels and textbox
                Bind_Messages_Station_DGV();
            }
        }

        private void Send_Message_All_Active(string message)
        {
            foreach (DB_Station station in Stations)
            {
                if (station.Active)
                {
                    Send_Message(station, message);
                    station.Number_Outgoing_Messages++;
                }
            }
        }

        private void Send_Alert_All_Active(string alert, string exceptStation)
        {
            foreach (DB_Station station in Stations)
            {
                if (station.Active && (station.Name != exceptStation))
                {
                    Send_Alert(station, alert);
                    station.Number_Outgoing_Messages++;
                }
            }
        }

        private void Send_Alert_All_Active(string alert)
        {
            bool Sent = false;      // 8/11/17
            foreach (DB_Station station in Stations)
            {
                if (station.Active)
                {
                    if (station.Medium != "APRS")       // 8/11/17
                    {
                        Send_Alert(station, alert);
                        station.Number_Outgoing_Messages++;
                    }
                    else       // 8/11/17
                    {
                        if (!Sent)      // 8/11/17 - this causes the Alert to be sent only 1 time for APRS
                        {
                            Send_Alert(station, alert);
                            station.Number_Outgoing_Messages++;
                            Sent = true;    // 8/11/17
                        }
                    }
                }
            }
        }

        private void Send_Alert(DB_Station station, string alert)
        {
            switch (station.Medium)     // added switch 8/10/17
            {
                case "Ethernet":
                    station.IP_StationWorker.Add_Alert(alert);      // this was the original
                    break;
                case "Packet":
                    station.Packet_StationWorker.Add_Alert(alert);
                    break;
                case "APRS":
                    station.APRS_StationWorker.Add_Alert(alert);
                    break;
            }
        }

        private void Send_Message(DB_Station station, string message)
        {
            switch (station.Medium)
            {
                case "Ethernet":
                    station.IP_StationWorker.Add_Out_Message(message);
                    break;
                case "Packet":
                    station.Packet_StationWorker.Add_Out_Message(message);
                    break;
                case "APRS":
                    station.APRS_StationWorker.Add_Out_Message(message);
                    break;
            }
        }
        #endregion

        private void tb_New_Message_Leave(object sender, EventArgs e)
        {
            _last = (Control)sender;
        }

        private void tb_Message_Reply_Leave(object sender, EventArgs e)
        {
            _last = (Control)sender;
        }

        private void tb_Selected_Message_TextChanged(object sender, EventArgs e)
        {
            if (tb_Selected_Message.Text != "")
            {
                //                lbl_Message_Reply.Visible = true;
                //                tb_Message_Reply.Visible = true;
            }
            else
            {
                lbl_Message_Reply.Visible = false;
                tb_Message_Reply.Visible = false;
            }
        }

        private void rb_Send_Message_to_All_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Send_Message_to_All.Checked)
            {
                //btn_Cancel_Send_New_Messaage.Visible = true;
                //tb_New_Message.Visible = true;
                //lbl_New_Message.Visible = true;
                MakeVisible(btn_Cancel_Send_New_Messaage, true);
                MakeVisible(tb_New_Message, true);
                MakeVisible(lbl_New_Message, true);
                SetCtlFocus(tb_New_Message);        // 7/18/17
            }
        }

        private void rb_Send_Message_to_Selected_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Send_Message_to_Selected.Checked)
            {
                // make sure the selected station is Active
                int index = dgv_Messages_Stations.CurrentRow.Index;
                DB_Station station = Stations[index];
                if (station.Active)
                {
                    //btn_Cancel_Send_New_Messaage.Visible = true;
                    //tb_New_Message.Visible = true;
                    //lbl_New_Message.Visible = true;
                    //tb_New_Message.Focus();
                    MakeVisible(btn_Cancel_Send_New_Messaage, true);
                    MakeVisible(tb_New_Message, true);
                    MakeVisible(lbl_New_Message, true);
                    SetCtlFocus(tb_New_Message);
                }
                else
                {
                    MessageBox.Show("Selected station is not Active", "Station not Active", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //                    rb_Send_Message_to_All.Checked = true;
                    //rb_Send_Message_to_Selected.Checked = false;
                    //btn_Cancel_Send_New_Messaage.Visible = false;
                    //tb_New_Message.Visible = false;
                    //lbl_New_Message.Visible = false;
                    MakeRBChecked(rb_Send_Message_to_Selected, false);
                    MakeVisible(btn_Cancel_Send_New_Messaage, false);
                    MakeVisible(tb_New_Message, false);
                    MakeVisible(lbl_New_Message, false);
                }
            }
        }

        private void Bind_Messages_Station_DGV()
        {
            dgv_Messages_Stations.DataSource = null;
            dgv_Messages_Stations.DataSource = Stations;
            dgv_Messages_Stations.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Messages_Stations.Columns[0].Width = 80;     // Name
            dgv_Messages_Stations.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Messages_Stations.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_Messages_Stations.Columns[1].Width = 20;     // Number
            dgv_Messages_Stations.Columns[1].HeaderText = "#";
            dgv_Messages_Stations.Columns[2].Visible = false;
            dgv_Messages_Stations.Columns[3].Visible = false;
            dgv_Messages_Stations.Columns[4].Visible = false;
            dgv_Messages_Stations.Columns[5].Visible = false;
            dgv_Messages_Stations.Columns[6].Visible = false;
            dgv_Messages_Stations.Columns[7].Visible = false;
            dgv_Messages_Stations.Columns[8].Visible = false;
            dgv_Messages_Stations.Columns[9].Visible = false;
            dgv_Messages_Stations.Columns[10].Width = 39;    // Active
            dgv_Messages_Stations.Columns[11].Visible = false;
            dgv_Messages_Stations.Columns[12].Visible = false;
            dgv_Messages_Stations.Columns[13].Visible = false;
            dgv_Messages_Stations.Columns[14].Visible = false;   // Medium
            dgv_Messages_Stations.Columns[15].Visible = false;   // IP/Callsign
            dgv_Messages_Stations.Columns[16].Width = 53;     // Number of Incoming messages
            dgv_Messages_Stations.Columns[16].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Messages_Stations.Columns[16].HeaderText = "# of In Messages";
            dgv_Messages_Stations.Columns[17].Width = 53;     // Number of Outgoing messages
            dgv_Messages_Stations.Columns[17].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Messages_Stations.Columns[17].HeaderText = "# of Out Messages";
            dgv_Messages_Stations.Columns[18].Visible = false;   // IP StationWorker
            dgv_Messages_Stations.Columns[19].Visible = false;   // Packet StationWorker    // 7/17/17
            dgv_Messages_Stations.Columns[20].Visible = false;   // APRS StationWorker    // 7/17/17
            dgv_Messages_Stations.Columns[21].Visible = false;   // Pipe Handle    // 7/17/17
            dgv_Messages_Stations.Update();
        }

        private void tb_Message_Reply_TextChanged(object sender, EventArgs e)
        {
            if (tb_Message_Reply.Text != "")
            {
                btn_Send_Message_Reply.Visible = true;
            }
            else
            {
                btn_Send_Message_Reply.Visible = false;
            }
        }

        private void tb_New_Message_TextChanged(object sender, EventArgs e)
        {
            if (tb_New_Message.Text != "")
                btn_Send_New_Message.Visible = true;
            else
                btn_Send_New_Message.Visible = false;
        }

        private void dgv_Messages_Stations_SelectionChanged(object sender, EventArgs e)
        {
            // determine position in the list
            int index = dgv_Messages_Stations.CurrentRow.Index;
            DB_Station mess_station = Stations[index];

            // has this station sent or received any messages?
            if ((mess_station.Number_Incoming_Messages != 0) || (mess_station.Number_Outgoing_Messages != 0))
            {
                MakeVisible(lbl_Messages_Selected_DGV, true);
                MakeVisible(tb_Selected_Message, true);
                MakeVisible(lbl_Selected_Message, true);
                if (mess_station.Number_Incoming_Messages != 0)
                {
                    MakeVisible(lbl_Incoming, true);
                    MakeVisible(dgv_Incoming_Messages_for_Selected_Station, true);
                    Bind_Incoming_Messages_Selected_Station_DGV(index);
                }
                if (mess_station.Number_Outgoing_Messages != 0)
                {
                    MakeVisible(lbl_Outgoing, true);
                    MakeVisible(dgv_Outgoing_Messages_to_Selected_Station, true);
                    Bind_Outgoing_Messages_Selected_Station_DGV(index);
                }
            }
            else
            {
                MakeVisible(lbl_Messages_Selected_DGV, false);
                MakeVisible(tb_Selected_Message, false);
                MakeVisible(lbl_Selected_Message, false);
                MakeVisible(lbl_Incoming, false);
                MakeVisible(dgv_Incoming_Messages_for_Selected_Station, false);
                MakeVisible(btn_Start_a_Reply, false);      // 7/18/17
                MakeVisible(lbl_Outgoing, false);
                MakeVisible(dgv_Outgoing_Messages_to_Selected_Station, false);
            }
        }

        private void Bind_Incoming_Messages_Selected_Station_DGV(int index)
        {
            // populate the Incoming_Messages_Selected_Stations list
            DB_Station message_station = Stations[index];
            int Num_I_Mess = message_station.Number_Incoming_Messages;
            Incoming_Messages_Selected_Stations.Clear();
            switch (message_station.Medium)
            {
                case "Ethernet":
                    DB_IP_StationWorker SW = message_station.IP_StationWorker;
                    for (int j = 0; j < Num_I_Mess; j++)
                    {
                        Messages_Selected_Station selstat = new Messages_Selected_Station();
                        selstat.Message_Number = SW.Incoming_Messages[j].Number;
                        selstat.Time_Received = SW.Incoming_Messages[j].Received;
                        selstat.Message_Size = SW.Incoming_Messages[j].Size;
                        Incoming_Messages_Selected_Stations.Add(selstat);
                    }
                    break;
                case "Packet":
                    DB_Packet_StationWorker SWP = message_station.Packet_StationWorker;
                    for (int j = 0; j < Num_I_Mess; j++)
                    {
                        Messages_Selected_Station selstat = new Messages_Selected_Station();
                        selstat.Message_Number = SWP.Incoming_Messages[j].Number;
                        selstat.Time_Received = SWP.Incoming_Messages[j].Received;
                        selstat.Message_Size = SWP.Incoming_Messages[j].Size;
                        Incoming_Messages_Selected_Stations.Add(selstat);
                    }
                    break;
                case "APRS":
                    DB_APRS_StationWorker SWA = message_station.APRS_StationWorker;
                    for (int j = 0; j < Num_I_Mess; j++)
                    {
                        Messages_Selected_Station selstat = new Messages_Selected_Station();
                        selstat.Message_Number = SWA.Incoming_Messages[j].Number;
                        selstat.Time_Received = SWA.Incoming_Messages[j].Received;
                        selstat.Message_Size = SWA.Incoming_Messages[j].Size;
                        Incoming_Messages_Selected_Stations.Add(selstat);
                    }
                    break;
            }
//            Incoming_Messages_Selected_Stations.Clear();
            //for (int j = 0; j < Num_I_Mess; j++)
            //{
            //    Messages_Selected_Station selstat = new Messages_Selected_Station();
            //    selstat.Message_Number = SW.Incoming_Messages[j].Number;
            //    selstat.Time_Received = SW.Incoming_Messages[j].Received;
            //    selstat.Message_Size = SW.Incoming_Messages[j].Size;
            //    Incoming_Messages_Selected_Stations.Add(selstat);
            //}

            // set the width of the dgv depending on how many messages are in the list,
            // so the vertical scroll bar does not cause the horizontal scroll bar to also appear
            if (Num_I_Mess > 6)
            {
                dgv_Incoming_Messages_for_Selected_Station.Width = 137;      // default + 17
                dgv_Incoming_Messages_for_Selected_Station.Left = 397 - 8;   // center with default
            }
            else
            {
                dgv_Incoming_Messages_for_Selected_Station.Width = 120;      // default settings
                dgv_Incoming_Messages_for_Selected_Station.Left = 397;
            }

            dgv_Incoming_Messages_for_Selected_Station.DataSource = null;
            dgv_Incoming_Messages_for_Selected_Station.DataSource = Incoming_Messages_Selected_Stations;
            dgv_Incoming_Messages_for_Selected_Station.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Incoming_Messages_for_Selected_Station.Columns[0].Width = 20;     // Incoming Number
            dgv_Incoming_Messages_for_Selected_Station.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Incoming_Messages_for_Selected_Station.Columns[0].HeaderText = "#";
            dgv_Incoming_Messages_for_Selected_Station.Columns[1].Width = 65;     // Incoming Time
            dgv_Incoming_Messages_for_Selected_Station.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Incoming_Messages_for_Selected_Station.Columns[1].HeaderText = "Time";
            dgv_Incoming_Messages_for_Selected_Station.Columns[1].DefaultCellStyle.Format = "HH:mm:ss";
            dgv_Incoming_Messages_for_Selected_Station.Columns[2].Width = 32;     // Incoming Size
            dgv_Incoming_Messages_for_Selected_Station.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Incoming_Messages_for_Selected_Station.Columns[2].HeaderText = "Size";
            dgv_Incoming_Messages_for_Selected_Station.Update();
        }

        private void Bind_Outgoing_Messages_Selected_Station_DGV(int index)
        {
            // populate the Outgoing_Messages_Selected_Stations list
            Outgoing_Messages_Selected_Stations.Clear();
            DB_Station mess_station = Stations[index];
            int Num_O_Mess = mess_station.Number_Outgoing_Messages;
            switch (mess_station.Medium)
            {
                case "Ethernet":
                    DB_IP_StationWorker SWI = mess_station.IP_StationWorker;
                    for (int j=0; j<Num_O_Mess; j++)
                    {
                        Messages_Selected_Station selstat = new Messages_Selected_Station();
                        selstat.Message_Number = SWI.Outgoing_Messages[j].Number;
                        selstat.Time_Received = SWI.Outgoing_Messages[j].Received;
                        selstat.Message_Size = SWI.Outgoing_Messages[j].Size;
                        Outgoing_Messages_Selected_Stations.Add(selstat);
                    }
                    break;
                case "Packet":
                    DB_Packet_StationWorker SWP = mess_station.Packet_StationWorker;
                    for (int j=0; j<Num_O_Mess; j++)
                    {
                        Messages_Selected_Station selstat = new Messages_Selected_Station();
                        selstat.Message_Number = SWP.Outgoing_Messages[j].Number;
                        selstat.Time_Received = SWP.Outgoing_Messages[j].Received;
                        selstat.Message_Size = SWP.Outgoing_Messages[j].Size;
                        Outgoing_Messages_Selected_Stations.Add(selstat);
                    }
                    break;
                case "APRS":
                    DB_APRS_StationWorker SWA = mess_station.APRS_StationWorker;
                    for (int j=0; j<Num_O_Mess; j++)
                    {
                        Messages_Selected_Station selstat = new Messages_Selected_Station();
                        selstat.Message_Number = SWA.Outgoing_Messages[j].Number;
                        selstat.Time_Received = SWA.Outgoing_Messages[j].Received;
                        selstat.Message_Size = SWA.Outgoing_Messages[j].Size;
                        Outgoing_Messages_Selected_Stations.Add(selstat);
                    }
                    break;
            }
//            Outgoing_Messages_Selected_Stations.Clear();
            //for (int j=0; j<Num_O_Mess; j++)
            //{
            //    Messages_Selected_Station selstat = new Messages_Selected_Station();
            //    selstat.Message_Number = SW.Outgoing_Messages[j].Number;
            //    selstat.Time_Received = SW.Outgoing_Messages[j].Received;
            //    selstat.Message_Size = SW.Outgoing_Messages[j].Size;
            //    Outgoing_Messages_Selected_Stations.Add(selstat);
            //}

            // set the width of the dgv depending on how many messages are in the list,
            // so the vertical scroll bar does not cause the horizontal scroll bar to also appear
            if (Num_O_Mess > 6)
            {
                dgv_Outgoing_Messages_to_Selected_Station.Width = 137;      // default + 17
                dgv_Outgoing_Messages_to_Selected_Station.Left = 553 - 8;   // center with default
            }
            else
            {
                dgv_Outgoing_Messages_to_Selected_Station.Width = 120;      // default settings
                dgv_Outgoing_Messages_to_Selected_Station.Left = 553;
            }

            dgv_Outgoing_Messages_to_Selected_Station.DataSource = null;
            dgv_Outgoing_Messages_to_Selected_Station.DataSource = Outgoing_Messages_Selected_Stations;
            dgv_Outgoing_Messages_to_Selected_Station.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages_to_Selected_Station.Columns[0].Width = 20;     // Outgoing Number
            dgv_Outgoing_Messages_to_Selected_Station.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages_to_Selected_Station.Columns[0].HeaderText = "#";
            dgv_Outgoing_Messages_to_Selected_Station.Columns[1].Width = 65;     // Outgoing Time
            dgv_Outgoing_Messages_to_Selected_Station.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages_to_Selected_Station.Columns[1].HeaderText = "Time";
            dgv_Outgoing_Messages_to_Selected_Station.Columns[1].DefaultCellStyle.Format = "HH:mm:ss";
            dgv_Outgoing_Messages_to_Selected_Station.Columns[2].Width = 32;     // Outgoing Size
            dgv_Outgoing_Messages_to_Selected_Station.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages_to_Selected_Station.Columns[2].HeaderText = "Size";
            dgv_Outgoing_Messages_to_Selected_Station.Update();
        }

        private void dgv_Incoming_Messages_for_Selected_Station_SelectionChanged(object sender, EventArgs e)
        {
            // determine position in the list
            Point rowcol = dgv_Incoming_Messages_for_Selected_Station.CurrentCellAddress;

            // proceed only if a cell is selected
            if (rowcol.X == -1)
                return;

                if ((int)dgv_Incoming_Messages_for_Selected_Station.Rows[rowcol.Y].Cells[0].Value != 0)
                {
                dgv_Outgoing_Messages_to_Selected_Station.ClearSelection();     // 7/18/17
                    switch (Stations[dgv_Messages_Stations.CurrentRow.Index].Medium)
                    {
                        case "Ethernet":
                            SetTBtext(tb_Selected_Message, Stations[dgv_Messages_Stations.CurrentRow.Index].IP_StationWorker.Incoming_Messages[rowcol.Y].TheMessage);
                            break;
                        case "Packet":
                            SetTBtext(tb_Selected_Message, Stations[dgv_Messages_Stations.CurrentRow.Index].Packet_StationWorker.Incoming_Messages[rowcol.Y].TheMessage);
                            break;
                        case "APRS":
                            SetTBtext(tb_Selected_Message, Stations[dgv_Messages_Stations.CurrentRow.Index].APRS_StationWorker.Incoming_Messages[rowcol.Y].TheMessage);
                            break;
                    }
                MakeVisible(lbl_Selected_Message, true);
                MakeVisible(tb_Selected_Message, true);
                MakeVisible(btn_Start_a_Reply, true);
            }
            else
            {
                SetTBtext(tb_Selected_Message, "");
                MakeVisible(lbl_Selected_Message, false);
                MakeVisible(tb_Selected_Message, false);
                MakeVisible(btn_Start_a_Reply, false);
            }
        }

//        private void dgv_Incoming_Messages_for_Selected_Station_CellEnter(object sender, DataGridViewCellEventArgs e)
//        {
//            Point rowcol = dgv_Incoming_Messages_for_Selected_Station.CurrentCellAddress;
//// 7/17/17            if (rowcol.X != 0)
//// 7/17/17            {
//                this.BeginInvoke(new MethodInvoker(() =>
//                {
//// 7/17/17                    Move_Incoming_SSMess_Bib_Cell(rowcol.Y, rowcol.X);
//                    dgv_Incoming_Messages_for_Selected_Station.CurrentCell = dgv_Incoming_Messages_for_Selected_Station.Rows[rowcol.Y].Cells[0];       // 7/17/17
//                    dgv_Incoming_Messages_for_Selected_Station.CurrentCell.Selected = true;
//                }));
//// 7/17/17            }
//            dgv_Outgoing_Messages_to_Selected_Station.CurrentCell = null;
//        }

// 7/17/17        private void Move_Incoming_SSMess_Bib_Cell(int index, int cell)
// 7/17/17        {
// 7/17/17                dgv_Incoming_Messages_for_Selected_Station.CurrentCell = dgv_Incoming_Messages_for_Selected_Station.Rows[index].Cells[0];       // got an exception here
// 7/17/17        }

        private void dgv_Outgoing_Messages_to_Selected_Station_SelectionChanged(object sender, EventArgs e)
        {
            // determine position in the list
            Point rowcol = dgv_Outgoing_Messages_to_Selected_Station.CurrentCellAddress;

            // proceed only if a cell is selected
            if (rowcol.X == -1)
                return;

            dgv_Incoming_Messages_for_Selected_Station.ClearSelection();    // 7/18/17
            MakeVisible(lbl_Message_Reply, false);
            MakeVisible(tb_Message_Reply, false);
            MakeVisible(btn_Start_a_Reply, false);
            switch (Stations[dgv_Messages_Stations.CurrentRow.Index].Medium)
            {
                case "Ethernet":
                    SetTBtext(tb_Selected_Message, Stations[dgv_Messages_Stations.CurrentRow.Index].IP_StationWorker.Outgoing_Messages[rowcol.Y].TheMessage);
                    break;
                case "Packet":
                    SetTBtext(tb_Selected_Message, Stations[dgv_Messages_Stations.CurrentRow.Index].Packet_StationWorker.Outgoing_Messages[rowcol.Y].TheMessage);
                    break;
                case "APRS":
                    SetTBtext(tb_Selected_Message, Stations[dgv_Messages_Stations.CurrentRow.Index].APRS_StationWorker.Outgoing_Messages[rowcol.Y].TheMessage);
                    break;
            }
// 7/17/17            MakeVisible(lbl_Message_Reply, true);
// 7/17/17            MakeVisible(tb_Message_Reply, true);
        }

        //        private void dgv_Outgoing_Messages_to_Selected_Station_CellEnter(object sender, DataGridViewCellEventArgs e)
        //        {
        //            Point rowcol = dgv_Outgoing_Messages_to_Selected_Station.CurrentCellAddress;
        //// 7/17/17            if (rowcol.X != 0)
        //// 7/17/17            {
        //                this.BeginInvoke(new MethodInvoker(() =>
        //                {
        //// 7/17/17                    Move_Outgoing_SSMess_Bib_Cell(rowcol.Y, rowcol.X);
        //                    dgv_Outgoing_Messages_to_Selected_Station.CurrentCell = dgv_Outgoing_Messages_to_Selected_Station.Rows[rowcol.Y].Cells[0];      // 7/17/17
        //                    dgv_Outgoing_Messages_to_Selected_Station.CurrentCell.Selected = true;      // 7/17/17
        //                }));
        //            // 7/17/17            }
        //            dgv_Incoming_Messages_for_Selected_Station.CurrentCell = null;
        //        }

        // 7/17/17        private void Move_Outgoing_SSMess_Bib_Cell(int index, int cell)
        // 7/17/17        {
        // 7/17/17            dgv_Outgoing_Messages_to_Selected_Station.CurrentCell = dgv_Outgoing_Messages_to_Selected_Station.Rows[index].Cells[0];
        // 7/17/17        }

//        private void dgv_Incoming_Messages_for_Selected_Station_CellContentClick(object sender, DataGridViewCellEventArgs e)    // 7/17/17
//        {
////            if (e.RowIndex != -1)
////            {
////                dgv_Incoming_Messages_for_Selected_Station.CurrentCell = dgv_Incoming_Messages_for_Selected_Station.Rows[e.RowIndex].Cells[0];       // 7/17/17
////// 7/18/17                dgv_Outgoing_Messages_to_Selected_Station.CurrentCell = null;
////                dgv_Outgoing_Messages_to_Selected_Station.ClearSelection();     // 7/18/17
////            }
//        }

//        private void dgv_Outgoing_Messages_to_Selected_Station_CellContentClick(object sender, DataGridViewCellEventArgs e)     // 7/17/17
//        {
////            if (e.RowIndex != -1)
////            {
////                dgv_Outgoing_Messages_to_Selected_Station.CurrentCell = dgv_Outgoing_Messages_to_Selected_Station.Rows[e.RowIndex].Cells[0];      // 7/17/17
////// 7/18/17                dgv_Incoming_Messages_for_Selected_Station.CurrentCell = null;
////                dgv_Incoming_Messages_for_Selected_Station.ClearSelection();    // 7/18/17
////            }
//        }

        private void dgv_Incoming_Messages_for_Selected_Station_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)     // 7/18/17
        {
            if (e.RowIndex != -1)
                dgv_Incoming_Messages_for_Selected_Station.CurrentCell = dgv_Incoming_Messages_for_Selected_Station[0, e.RowIndex];
        }

        private void dgv_Outgoing_Messages_to_Selected_Station_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)      // 7/18/17
        {
            if (e.RowIndex != -1)
                dgv_Outgoing_Messages_to_Selected_Station.CurrentCell = dgv_Outgoing_Messages_to_Selected_Station[0, e.RowIndex];
        }

        private void dgv_Messages_Stations_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex != -1)
                dgv_Messages_Stations.CurrentCell = dgv_Messages_Stations[0, e.RowIndex];
        }
        #endregion

        #region Settings tab
        #region buttons
        private void btn_DB_Sounds_Click(object sender, EventArgs e)
        {
            Sounds sounds = new Sounds();
            sounds.Sounds_Directory = Sounds_Directory;
            sounds.Connections = Connections_Sound;
            sounds.File_Download = File_Download_Sound;
            sounds.Alerts = Alerts_Sound;
            sounds.Messages = Messages_Sound;
            DialogResult res = sounds.ShowDialog();
            if (res == DialogResult.OK)
            {
                Sounds_Directory = sounds.Sounds_Directory;
                Connections_Sound = sounds.Connections;
                File_Download_Sound = sounds.File_Download;
                Alerts_Sound = sounds.Alerts;
                Messages_Sound = sounds.Messages;
            }
        }

        private void btn_Browse_Stations_Info_Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
//                tb_Stations_Info_Filename.Text = ofd.FileName;
                SetTBtext(tb_Stations_Info_Filename, ofd.FileName);
            }
        }

        private void btn_Browse_RFID_Assignments_Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
//                tb_RFID_Assignments_Filename.Text = ofd.FileName;
                SetTBtext(tb_RFID_Assignments_Filename, ofd.FileName);
            }
        }

        private void btn_DB_Settings_Browse_Data_Directory_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
//                tb_DB_Settings_Data_Directory.Text = ofd.FileName;
                SetTBtext(tb_DB_Settings_Data_Directory, ofd.FileName);
            }
        }

        private void btn_DB_Settings_Browse_RunnersDataFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml files (*.xml)|*.xml";
            ofd.CheckFileExists = false;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
//                tb_DB_Settings_RunnersDataFile.Text = ofd.FileName;
                SetTBtext(tb_DB_Settings_RunnersDataFile, ofd.FileName);
            }
//            tb_DB_Settings_RunnersDataFile.Focus();      // this should cause the Leave action of the Runners Data file name to happen
//            tb_Server_Error_Message.Focus();      // this should cause the Leave action of the Runners Data file name to happen
            SetCtlFocus(tb_DB_Settings_RunnersDataFile);
            SetCtlFocus(tb_Server_Error_Message);
        }
        #endregion

        private void chk_Using_RFID_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Using_RFID.Checked)
            {
                chk_Bib_Number_equals_RFID.Visible = true;
                lbl_RFID_Assignments_file_Loaded.Visible = true;
                lbl_RFID_Assign_FIle.Visible = true;
                tb_RFID_Assignments_Filename.Visible = true;
                btn_Browse_RFID_Assignments_Filename.Visible = true;
                if (!Init_Registry)
                    Save_Registry("Using RFID", "Yes");
            }
            else
            {
                chk_Bib_Number_equals_RFID.Visible = false;
                lbl_RFID_Assignments_file_Loaded.Visible = false;
                lbl_RFID_Assign_FIle.Visible = false;
                tb_RFID_Assignments_Filename.Visible = false;
                btn_Browse_RFID_Assignments_Filename.Visible = false;
                if (!Init_Registry)
                    Save_Registry("Using RFID", "No");
            }
        }

        #region Textboxes
        #region RFID Assignments Filename Textbox
        private void tb_RFID_Assignments_Filename_TextChanged(object sender, EventArgs e)
        {
            if (tb_RFID_Assignments_Filename.Text == "")
                tb_RFID_Assignments_Filename.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_RFID_Assignments_Filename.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_RFID_Assignments_Filename_Leave(object sender, EventArgs e)
        {
            if (tb_RFID_Assignments_Filename.Text == "")
                RFID_Assignments_Filename = string.Empty;
            else
                RFID_Assignments_Filename = tb_RFID_Assignments_Filename.Text;
            Save_Registry("RFID Assignments File", RFID_Assignments_Filename);
        }

        private void tb_RFID_Assignments_Filename_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_RFID_Assignments_Filename_Leave(null, null);
        }
        #endregion

        #region Stations Info Filename Textbox
        private void tb_Stations_Info_Filename_TextChanged(object sender, EventArgs e)
        {
            if (tb_Stations_Info_Filename.Text == "")
                tb_Stations_Info_Filename.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Stations_Info_Filename.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Stations_Info_Filename_Leave(object sender, EventArgs e)
        {
            if (tb_Stations_Info_Filename.Text == "")
            {
                Stations_Info_Filename = string.Empty;
                tb_Station_Info_Filename.Text = "";
            }
            else
            {
                tb_Station_Info_Filename.Text = tb_Stations_Info_Filename.Text;
                Stations_Info_Filename = tb_Stations_Info_Filename.Text;
            }
            Save_Registry("Stations Info File", Stations_Info_Filename);
        }

        private void tb_Stations_Info_Filename_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Stations_Info_Filename_Leave(null, null);
        }
        #endregion

        #region Data Directory Textbox
        private void tb_DB_Settings_Data_Directory_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Settings_Data_Directory.Text == "")
                tb_DB_Settings_Data_Directory.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Settings_Data_Directory.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Settings_Data_Directory_Leave(object sender, EventArgs e)
        {
            if (tb_DB_Settings_Data_Directory.Text == "")
                DataDirectory = string.Empty;
            else
                DataDirectory = tb_DB_Settings_Data_Directory.Text;
            Save_Registry("Data Directory", DataDirectory);
        }

        private void tb_DB_Settings_Data_Directory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Settings_Data_Directory_Leave(null, null);
        }
        #endregion

        #region Runners Data File Textbox
        private void tb_DB_Settings_RunnersDataFile_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Settings_RunnersDataFile.Text == "")
                tb_DB_Settings_RunnersDataFile.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Settings_RunnersDataFile.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Settings_RunnersDataFile_Leave(object sender, EventArgs e)
        {
            Save_Registry("Runners Data File", tb_DB_Settings_RunnersDataFile.Text);
            if (tb_DB_Settings_RunnersDataFile.Text != "")
            {
                // verify the file exists, or ask if create
                string RDFilename = tb_DB_Settings_RunnersDataFile.Text;
                if (File.Exists(RDFilename))
                {
                    DialogResult res = MessageBox.Show("Would you like to create a copy of the file:\n\n" + RDFilename + "\n\nbefore using it?", "Create a copy", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (res == DialogResult.Yes)
                    {
                        bool CopyIt = true;
                        string newfilename = Path.ChangeExtension(RDFilename, null) + " - Copy.xml";

                        // make sure this new name does not already exist
                        bool exists = true;
                        while (exists)
                        {
                            if (File.Exists(newfilename))
                            {
                                MessageBox.Show("Automatic name assignment of the Copied file failed!\n\nYou must change the name.", "File name exists", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                OpenFileDialog ofd1 = new OpenFileDialog();
                                ofd1.Filter = "xml files (*.xml)|*.xml";
                                ofd1.RestoreDirectory = true;
                                ofd1.CheckFileExists = false;
                                ofd1.FileName = newfilename;
                                DialogResult res2 = ofd1.ShowDialog();
                                switch (res2)
                                {
                                    case DialogResult.OK:
                                        newfilename = ofd1.FileName;
                                        break;
                                    case DialogResult.Cancel:
                                        exists = false;
                                        CopyIt = false;
                                        break;
                                }
                            }
                            else
                                exists = false;
                        }
                        if (CopyIt)
                            File.Move(RDFilename, newfilename);     // rename the original file
                    }
                }
                else
                {
                    // create the new file
                    StreamWriter sw = File.CreateText(RDFilename);
                    sw.Close();
                    sw.Dispose();
                }
            }
        }

        private void tb_DB_Settings_RunnersDataFile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Settings_RunnersDataFile_Leave(null, null);
        }
        #endregion

        #region Welcome Message Textbox
        private void tb_Welcome_Message_TextChanged(object sender, EventArgs e)
        {
            if (tb_Welcome_Message.Text == "")
                tb_Welcome_Message.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Welcome_Message.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Welcome_Message_Leave(object sender, EventArgs e)
        {
            if (tb_Welcome_Message.Text == "")
            {
                Welcome_Message_Exists = false;
                Welcome_Message = string.Empty;
            }
            else
            {
                Welcome_Message_Exists = true;
                Welcome_Message = tb_Welcome_Message.Text;
            }
            Save_Registry("Welcome Message", Welcome_Message);
        }

        private void tb_Welcome_Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Welcome_Message_Leave(null, null);
        }
        #endregion
        #endregion

        bool Load_RFID_Assignments(string FileName)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            StreamReader reader;

            // do this only if the FileName is not empty
            if (FileName != "")
            {
                try
                {
                    reader = File.OpenText(FileName);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                RFIDAssignments.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        RFID rfid = new RFID();
                        rfid.String = Parts[0].TrimStart(front);
                        rfid.RunnerNumber = Convert.ToInt16(Parts[1]);
                        RFIDAssignments.Add(rfid);
                    }
                }
                lbl_RFID_Assignments_file_Loaded.Visible = true;
            }
            return true;
        }

        bool Save_RFID_Assignents(List<RFID> rfid, string FileName)
        {
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (rfid.Count == 0)
            {
                MessageBox.Show("Station List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                                        FileName +
                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
//                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                else
                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save each station
            foreach (RFID rfidd in rfid)
            {
                // Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station, Next station,
                // distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)
                string line = rfidd.String + ",";
                line += rfidd.RunnerNumber.ToString();
                writer.WriteLine(line);
            }
            writer.Close();

            return true;
        }

        #region Ethernet/Mesh groupbox
        #region Server Port Number Textbox
        private void tb_Server_Port_Number_TextChanged(object sender, EventArgs e)
        {
            if (tb_Server_Port_Number.Text == "")
                tb_Server_Port_Number.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Server_Port_Number.BackColor = Color.FromKnownColor(KnownColor.Window);
        }
        
        private void tb_Server_Port_Number_Leave(object sender, EventArgs e)
        {
            Save_Registry("Mesh Server Port #", tb_Server_Port_Number.Text);
            MessageBox.Show("You must Close and Restart this program\n\nto make this change take affect");  // 3/29/19
        }

        private void tb_Server_Port_Number_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Server_Port_Number_Leave(null, null);
        }
        #endregion

        #region Server IP Address Textbox
        private void tb_Mesh_IP_Address_TextChanged(object sender, EventArgs e)
        {
            if (tb_Mesh_IP_Address.Text == "")
                tb_Mesh_IP_Address.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Mesh_IP_Address.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Mesh_IP_Address_Leave(object sender, EventArgs e)
        {
            Save_Registry("Mesh IP Address", tb_Aid_Mesh_IP_address.Text);
        }

        private void tb_Mesh_IP_Address_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Mesh_IP_Address_Leave(null, null);
        }
        #endregion

        private void lbl_DB_14001_Click(object sender, EventArgs e)
        {
            string str = lbl_DB_14001.Text;
            str = str.Remove(0, 1);     // remove the leading '('
            str = str.Remove(str.Length - 1, 1);    // remove the trailing ')'
// 3/29/19            tb_Server_Port_Number.Text = str;
            SetTBtext(tb_Server_Port_Number, str);    // 3/29/19
            tb_Server_Port_Number_Leave(null, null);    // 3/29/19

        }

        private void btn_Find_Network_Adapters_Click(object sender, EventArgs e)
        {
            Find_This_PC_IP_Address();
        }
        #endregion

        #region AGWPE tab
        #region AGWPE subtab
        private void lbl_localhost_Click(object sender, EventArgs e)
        {
            string str = lbl_localhost.Text;
            str = str.Remove(0, 1);     // remove the leading '('
            str = str.Remove(str.Length - 1, 1);    // remove the trailing ')'
            tb_DB_AGWPEServer.Text = str;
        }

        private void lbl_8000_Click(object sender, EventArgs e)
        {
            string str = lbl_8000.Text;
            str = str.Remove(0, 1);     // remove the leading '('
            str = str.Remove(str.Length - 1, 1);    // remove the trailing ')'
            tb_DB_AGWPEPort.Text = str;
        }

        private void btn_AGWPE_Start_Refresh_Click(object sender, EventArgs e)
        {
            btn_AGWPE_Start_Refresh.Visible = false;
            btn_AGWPE_Start_Refresh.Update();

            // first save the textboxes
            Save_Registry("AGWPE Radio Port", AGWPERadioPort.ToString());
            Save_Registry("AGWPE Server Name", AGWPEServerName);
            Save_Registry("AGWPE Server Port", AGWPEServerPort);
            Save_Registry("Database FCC Callsign", DatabaseFCCCallsign);

            if (!Form1.DB_AGWSocket.Connected_to_AGWserver)
            {
                Form1.DB_AGWSocket.InitAGWPE(true);
                while (Form1.DB_AGWSocket.InitInProcess)        // this hangs if AGWPE is not started first
                    Application.DoEvents();       // wait for the Init process to finish
                Thread.Sleep(4000);     // wait four seconds, before getting the new settings
            }
            Get_AGWPE_Settings();
        }

        private void chk_DB_Use_Tactical_Callsign_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chk_DB_AGWPE_Connected_CheckedChanged(object sender, EventArgs e)
        {
            Labels_TabPages_DB_Connections();
            if (chk_DB_AGWPE_Connected.Checked)
            {
                btn_AGWPE_Start_Refresh.Visible = true;
                btn_AGWPE_Start_Refresh.Text = "Refresh";
                btn_AGWPE_Start_Refresh.BackColor = Color.FromArgb(255, 192, 192);
// ??                TabPage packets = tabControl_DB_Main.TabPages[7];
// 4/12/16                AddRichText(rtb_DB_Packet_Packets, "Connected to AGWPE server" + Environment.NewLine, Color.Green);
            }
            else
            {
                //                cb_Callsign_Registered.Checked = false;
                btn_AGWPE_Start_Refresh.Text = "Start";
                btn_AGWPE_Start_Refresh.BackColor = Color.FromArgb(128, 255, 128);
            }
        }

        private void Get_AGWPE_Settings()
        {
            // test if changing from Callsign Not Registered to Registered
            if (!cb_DB_FCC_Callsign_Registered.Checked && DB_AGWSocket.Registered)
                AddRichText(rtb_DB_Packet_Packets, "Callsign registered with AGWPE" + Environment.NewLine, Color.Green);
            CheckCB(cb_DB_FCC_Callsign_Registered, Form1.DB_AGWSocket.Registered);
            MakeVisible(btn_AGWPE_Start_Refresh, true);

            // test if connected to the AGW server
            if (Form1.DB_AGWSocket.Connected_to_AGWserver)
            {
                CheckCB(chk_DB_AGWPE_Connected, true);
                SetTBtext(tb_Num_AGWPE_Radio_Ports, DB_AGWSocket.Ports.Num.ToString());
                if (Form1.DB_AGWSocket.Ports.Num != 0)
                {
                    Add_Portnames();
                }
            }
            else
                CheckCB(chk_DB_AGWPE_Connected, false);
            SetTBtext(tb_DB_AGWPEServer, Form1.AGWPEServerName);
            SetTBtext(tb_DB_AGWPEPort, Form1.AGWPEServerPort);
            SetTBtext(tb_DB_AGWPE_Version, Form1.DB_AGWSocket.Version);
            SetTBtext(tb_DB_FCC_Callsign, Form1.DatabaseFCCCallsign);
        }

        private void lb_AGWPE_Radio_Ports_SelectedIndexChanged(object sender, EventArgs e)
        {
            AGWPERadioPort = lb_AGWPE_Radio_Ports.SelectedIndex;
        }

        #region Textboxes
        #region AGWPE Server Textbox
        private void tb_DB_AGWPEServer_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_AGWPEServer.Text == "")
                tb_DB_AGWPEServer.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_AGWPEServer.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_AGWPEServer_Leave(object sender, EventArgs e)
        {
            if (tb_DB_AGWPEServer.Text == "")
                AGWPEServerName = string.Empty;
            else
                AGWPEServerName = tb_DB_AGWPEServer.Text;
            Save_Registry("AGWPE Server Name", AGWPEServerName);
        }

        private void tb_DB_AGWPEServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_AGWPEServer_Leave(null, null);
        }
        #endregion

        #region AGWPE Port Textbox
        private void tb_DB_AGWPEPort_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_AGWPEPort.Text == "")
                tb_DB_AGWPEPort.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_AGWPEPort.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_AGWPEPort_Leave(object sender, EventArgs e)
        {
            if (tb_DB_AGWPEPort.Text == "")
                AGWPEServerPort = string.Empty;
            else
                AGWPEServerPort = tb_DB_AGWPEPort.Text;
            Save_Registry("AGWPE Server Port", AGWPEServerPort);

        }

        private void tb_DB_AGWPEPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_AGWPEPort_Leave(null, null);
        }
        #endregion

        #region Database FCC Callsign Textbox
        private void tb_DB_FCC_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_FCC_Callsign.Text == "")
                tb_DB_FCC_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_FCC_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_FCC_Callsign_Enter(object sender, EventArgs e)
        {
            if (tb_DB_FCC_Callsign.Text == "(9 chars max)")
                tb_DB_FCC_Callsign.Text = "";
        }

        private void tb_DB_FCC_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_DB_FCC_Callsign.Text == "")
                DatabaseFCCCallsign = string.Empty;
            else
                DatabaseFCCCallsign = tb_DB_FCC_Callsign.Text;
            Save_Registry("Database FCC Callsign", DatabaseFCCCallsign);
        }

        private void tb_DB_FCC_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_FCC_Callsign_Leave(null, null);
        }
        #endregion

        #region Database Tactical Callsign
        private void tb_DB_Tactical_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Tactical_Callsign.Text == "")
                tb_DB_Tactical_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Tactical_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Tactical_Callsign_Enter(object sender, EventArgs e)
        {
            if (tb_DB_Tactical_Callsign.Text == "(9 chars max)")
                tb_DB_Tactical_Callsign.Text = "";
        }

        private void tb_DB_Tactical_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_DB_Tactical_Callsign.Text == "")
                DatabaseTacticalCallsign = string.Empty;
            else
                DatabaseTacticalCallsign = tb_DB_Tactical_Callsign.Text;
            Save_Registry("Database Tactical Callsign", DatabaseTacticalCallsign);
        }

        private void tb_DB_Tactical_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Tactical_Callsign_Leave(null, null);
        }
        #endregion

        #region Beacon Text Textbox
        private void tb_DB_Beacon_text_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Tactical_Beacon_text.Text == "")
                tb_DB_Tactical_Beacon_text.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Tactical_Beacon_text.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Tactical_Beacon_text_Enter(object sender, EventArgs e)
        {
            if (tb_DB_Tactical_Beacon_text.Text == "(include FCC Callsign)")
                tb_DB_Tactical_Beacon_text.Text = "";
        }

        private void tb_DB_Beacon_text_Leave(object sender, EventArgs e)
        {
            if (tb_DB_Tactical_Beacon_text.Text == "")
            //    BeaconText = string.Empty;
            //else
            //    BeaconText = tb_DB_Tactical_Beacon_text.Text;
            //Save_Registry("Beacon Text", BeaconText);
            TacticalBeaconText = string.Empty;
            else
                TacticalBeaconText = tb_DB_Tactical_Beacon_text.Text;
            Save_Registry("Database Tactical Beacon Text", TacticalBeaconText);
        }

        private void tb_DB_Beacon_text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Beacon_text_Leave(null, null);
        }
        #endregion
        #endregion

        //private void tb_AGWPE_DB_Callsign_TextChanged(object sender, EventArgs e)
        //{
        //    if (!Init_Registry)
        //        btn_Settings_SavetoRegistry.Visible = true;
        //    if (tb_AGWPE_DB_Callsign.Text == "")
        //        tb_AGWPE_DB_Callsign.BackColor = Color.FromArgb(255, 128, 128);
        //    else
        //    {
        //        tb_AGWPE_DB_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        //        DatabaseCallsign = tb_AGWPE_DB_Callsign.Text;
        //    }
        //}

        public void Add_Portnames()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lb_AGWPE_Radio_Ports.InvokeRequired)
            {
                Add_Portnamesdel d = new Add_Portnamesdel(Add_Portnames);
                lb_AGWPE_Radio_Ports.Invoke(d, new object[] { });
            }
            else
            {
                lb_AGWPE_Radio_Ports.Items.Clear();
//                for (int i = 0; i < Form1.DB_AGWSocket.Ports.Num; i++)
                for (int i = 0; i < DB_AGWSocket.Ports.Num; i++)
                {
//                    string portname = Form1.DB_AGWSocket.Ports.Pdata[i].PortName;
                    string portname = DB_AGWSocket.Ports.Pdata[i].PortName;
                    lb_AGWPE_Radio_Ports.Items.Add(portname);
                }
                // highlight the selected port, if it is valid
                if (AGWPERadioPort < lb_AGWPE_Radio_Ports.Items.Count)
                    lb_AGWPE_Radio_Ports.SelectedIndex = AGWPERadioPort;
                lb_AGWPE_Radio_Ports.Update();
            }
        }

        //private void rb_DB_Use_FCC_Callsign_CheckedChanged(object sender, EventArgs e)
        //{
        //    // make register button visible
        //    if (!Init_Registry)
        //        btn_DB_Register_change_w_AGWPE.Visible = true;

        //    //if (rb_DB_Use_FCC_Callsign.Checked)
        //    //{
        //    //    panel_DB_FCC_Callsign.Visible = true;
        //    //    panel_DB_Tactical_Callsign.Visible = false;
        //    //}
        //    //else
        //    //{
        //    //    panel_DB_FCC_Callsign.Visible = false;
        //    //    panel_DB_Tactical_Callsign.Visible = true;
        //    //}
        //}

        //private void btn_DB_Register_change_w_AGWPE_Click(object sender, EventArgs e)
        //{
        //    // test if the textboxes have been populated
        //    if (rb_DB_Use_FCC_Callsign.Checked)
        //    {
        //    }
        //    else
        //    {
        //        if ((tb_DB_Tactical_Callsign.Text == "") || (tb_DB_Beacon_text.Text == ""))
        //        {
        //        }
        //    }
        //}

        //private void tb_DB_FCC_Callsign_TextChanged(object sender, EventArgs e)
        //{
        //    if (tb_DB_FCC_Callsign.Text == "")
        //    {
        //        DatabaseFCCCallsign = string.Empty;
        //        tb_DB_FCC_Callsign.BackColor = Color.FromArgb(255, 128, 128);
        //    }
        //    else
        //    {
        //        tb_DB_FCC_Callsign.Text = tb_DB_FCC_Callsign.Text.ToUpper();
        //        tb_DB_FCC_Callsign.SelectionStart = tb_DB_FCC_Callsign.Text.Length;
        //        DatabaseFCCCallsign = tb_DB_FCC_Callsign.Text;
        //        tb_DB_FCC_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        //    }
        //    Save_Registry("Database FCC Callsign", DatabaseFCCCallsign);

        //    // make register button visible
        //    if (!Init_Registry)
        //        btn_Register_change_w_AGWPE.Visible = true;
        //}

        //private void tb_DB_Tactical_Callsign_TextChanged(object sender, EventArgs e)
        //{
        //    if (tb_DB_Tactical_Callsign.Text == "")
        //    {
        //        StationTacticalCallsign = string.Empty;
        //        tb_DB_Tactical_Callsign.BackColor = Color.FromArgb(255, 128, 128);
        //    }
        //    else
        //    {
        //        StationTacticalCallsign = tb_DB_Tactical_Callsign.Text;
        //        tb_DB_Tactical_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        //    }
        //    Save_Registry("Station Tactical Callsign", StationTacticalCallsign);

        //    // make register button visible
        //    if (!Init_Registry)
        //        btn_Register_change_w_AGWPE.Visible = true;
        //}
        #endregion

        #region AGWPE Statistics subtab
        /*
         * 
         * The format of the 'g' frame would be:
 
        Field             Length           Meaning
        AGWPE Port        1 Bytes        Port being queried
                                        0x00 Port1
                                        0x01 Port2 ….
        Reserved        3 Bytes        0x00 0x00 0x00
        DataKind        1 Byte        ‘g’ (ASCII 0x67)
        Reserved        1 Byte        0x00
        PID             1 Byte        0x00
        Reserved        1 Byte        0x00
        CallFrom        10 Bytes        10 0x00
        CallTo        10 Bytes        10 0x00
        DataLen        4 Bytes        12
        User (Reserved)        4 Bytes        0
  
        12 bytes of data would follow (as indicated by the DataLen field) containing the following information about the particular port referenced by the header’s AGWPEPort field :
  
        Offset (Byte or Characters) into the Data Area        Meaning
        +00                 On air baud rate (0=1200/1=2400/2=4800/3=9600…)
        +01                 Traffic level (if 0xFF the port is not in autoupdate mode)
        +02                 TX Delay
        +03                 TX Tail
        +04                 Persist
        +05                 SlotTime
        +06                 MaxFrame
        +07                 How Many connections are active on this port
        +08 LSB Low Word    HowManyBytes (received in the last 2 minutes) as a 32 bits
        +09 MSB Low Word     (4 bytes) integer. Updated every two minutes.
        +10 LSB High Word
        +11 MSB High Word
         * 
         */

        void DisplayDBAGWPEportStats(AGWPEPortStat Stats)
        {
            string statstr;
            switch (Stats.BaudRate)
            {
                case 0:
                    statstr = "1200";
                    break;
                case 1:
                    statstr = "2400";
                    break;
                case 2:
                    statstr = "4800";
                    break;
                case 3:
                    statstr = "9600";
                    break;
                default:
                    statstr = "none";
                    break;
            }
            SetTBtext(tb_DB_Port_Baudrate, statstr);
            SetTBtext(tb_DB_Port_TrafficLevel, Stats.TrafficLevel.ToString());
            SetTBtext(tb_DB_Port_TxDelay, Stats.TxDelay.ToString());
            SetTBtext(tb_DB_Port_TxTail, Stats.TxTail.ToString());
            SetTBtext(tb_DB_Port_Persist, Stats.Persist.ToString());
            SetTBtext(tb_DB_Port_SlotTime, Stats.SlotTime.ToString());
            SetTBtext(tb_DB_Port_MaxFrame, Stats.MaxFrame.ToString());
            SetTBtext(tb_DB_Port_NumConnections, Stats.NumConnections.ToString());
            SetTBtext(tb_DB_Port_NumBytesReceived, Stats.NumBytesReceived.ToString());
            SetTBtext(tb_DB_Port_NumPendPortFrames, Stats.NumPendingPortFrames.ToString());
            SetTBtext(tb_DB_Port_NumPendConnectFrames, Stats.NumPendingConnectionFrames.ToString());

            // also display the number of Connected Stations in the tab and in the Status Message label
//            tb_DB_Num_Connected_Stations.Text = Connected_Stations.ToString();
            SetTBtext(tb_DB_Num_Connected_Stations, Connected_Stations.ToString());
//            lbl_Packet_Stations_Connected.Text = lbl_Packet_Stations_Connected.Text.Remove(lbl_Packet_Stations_Connected.Text.Length-1) + tb_DB_Num_Connected_Stations.Text;
            SetCtlText(lbl_Packet_Stations_Connected, lbl_Packet_Stations_Connected.Text.Remove(lbl_Packet_Stations_Connected.Text.Length - 1) + tb_DB_Num_Connected_Stations.Text);
        }

        private void btn_DB_AGWPE_Statistics_Refresh_Click(object sender, EventArgs e)
        {
            DisplayDBAGWPEportStats(AGWPEPortStatistics);
        }
        #endregion
        #endregion

        #region APRS tab
        #region Textboxes
        #region Latitude Textbox
        private void tb_DB_Latitude_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Latitude.Text == "")
                tb_DB_Latitude.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Latitude.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Latitude_Leave(object sender, EventArgs e)
        {
            if (tb_DB_Latitude.Text == "")
                Latitude = string.Empty;
            else
                Latitude = tb_DB_Latitude.Text;
            Save_Registry("Latitude", Latitude);
        }

        private void tb_DB_Latitude_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Latitude_Leave(null, null);
        }
        #endregion

        #region Longitude Textbox
        private void tb_DB_Longitude_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Longitude.Text == "")
                tb_DB_Longitude.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_DB_Longitude.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_DB_Longitude_Leave(object sender, EventArgs e)
        {
            if (tb_DB_Longitude.Text == "")
                Longitude = string.Empty;
            else
                Longitude = tb_APRS_Longitude.Text;
            Save_Registry("Longitude", Longitude);
        }

        private void tb_DB_Longitude_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_DB_Longitude_Leave(null, null);
        }
        #endregion

        #region Network Textbox
        private void tb_DB_APRS_Networkname_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_APRS_Networkname.Text == "")
            {
                tb_DB_APRS_Networkname.BackColor = Color.FromArgb(255, 128, 128);
                //                tb_APRS_Networkname.Text = "(9 chars max)";
//                MakeVisible(btn_APRS_Connect_to_DB, false);
            }
            else
            {
                tb_DB_APRS_Networkname.Text = tb_DB_APRS_Networkname.Text.ToUpper();
                tb_DB_APRS_Networkname.SelectionStart = tb_DB_APRS_Networkname.Text.Length;
                tb_DB_APRS_Networkname.BackColor = Color.FromKnownColor(KnownColor.Window);
//                MakeVisible(btn_APRS_Connect_to_DB, true);
            }
        }

        private void tb_DB_APRS_Networkname_Leave(object sender, EventArgs e)
        {
            if (tb_DB_APRS_Networkname.Text == "")
                APRSnetworkName = string.Empty;
            else
                APRSnetworkName = tb_DB_APRS_Networkname.Text;
            Save_Registry("APRS Network Name", APRSnetworkName);
        }

        private void tb_DB_APRS_Networkname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_APRS_Networkname_Leave(null, null);
        }
        #endregion
        #endregion

        private void tb_APRS_test_packet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                MakeVisible(lbl_Sent_APRS_test_packet, false);
                DB_AGWSocket.TXdataUnproto(AGWPERadioPort, tb_APRS_test_packet.Text);
                MakeVisible(lbl_Sent_APRS_test_packet, true);
            }
        }
        #endregion
        #endregion

        #region Connumication Packets tab
        private void rb_DB_All_APRS_Packets_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_DB_All_APRS_Packets.Checked)
            {
                MakeVisible(lbl_DB_APRS_NumAllPackets_Received, true);
                MakeVisible(tb_NumAPRSlines_DB, true);
                MakeVisible(lbl_DB_NumAPRSnetwork, false);
                MakeVisible(tb_NumAPRSnetwork_DB, false);
            }
            else
            {
                MakeVisible(lbl_DB_NumAPRSnetwork, true);
                MakeVisible(tb_NumAPRSnetwork_DB, true);
                MakeVisible(lbl_DB_APRS_NumAllPackets_Received, false);
                MakeVisible(tb_NumAPRSlines_DB, false);
            }
        }

        //private void rb_DB_Only_Network_APRS_Packets_CheckedChanged(object sender, EventArgs e)
        //{

        //}
        #endregion

        #region Testing tab
        private void btn_Add_Station_Click(object sender, EventArgs e)
        {
            if (rb_IP.Checked)
                Add_Station_Name_IP_Medium(tb_Add_Station.Text, tb_IP_Address.Text, "IP");
            else
                Add_Station_Name_IP_Medium(tb_Add_Station.Text, tb_Callsign.Text, "AGWPE/Packet");

            // clear the textboxes, to be ready for the next one
            tb_Add_Station.Text = "";
            tb_Callsign.Text = "";
            tb_IP_Address.Text = "";
        }

        private void rb_IP_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_IP.Checked)
            {
                lbl_IP_Address.Visible = true;
                lbl_Callsign.Visible = false;
                tb_IP_Address.Visible = true;
                tb_Callsign.Visible = false;
            }
            else
            {
                lbl_IP_Address.Visible = false;
                lbl_Callsign.Visible = true;
                tb_IP_Address.Visible = false;
                tb_Callsign.Visible = true;
            }
        }

        private void btn_Add_Runner_Entering_Click(object sender, EventArgs e)
        {
            // if the listbox station has not been selected yet, assume the first
            if (lb_Stations_Testing_Entering.SelectedIndex == -1)
                lb_Stations_Testing_Entering.SelectedIndex = 0;

            // now create the new runner
            DB_Runner runner = new DB_Runner();
            runner.Station = lb_Stations_Testing_Entering.SelectedItem.ToString();
            runner.BibNumber = tb_Add_Runner_Entering.Text;
            runner.TimeIn = DateTime.Now.ToShortTimeString();
            AddUpdateRunner(runner, true);      // time is 'Time in'

            // clear the textbox to get ready for the next
            tb_Add_Runner_Entering.Text = "";
        }

        private void btn_Add_Runner_Leaving_Click(object sender, EventArgs e)
        {
            // if the listbox station has not been selected yet, assume the first
            if (lb_Stations_Testing_Leaving.SelectedIndex == -1)
                lb_Stations_Testing_Leaving.SelectedIndex = 0;

            // now create the new runner
            DB_Runner runner = new DB_Runner();
            runner.Station = lb_Stations_Testing_Leaving.SelectedItem.ToString();
            runner.BibNumber = tb_Add_Runner_Leaving.Text;
            runner.TimeOut = DateTime.Now.ToShortTimeString();
            AddUpdateRunner(runner, false);     // time is 'Time Out'

            // clear the textbox to get ready for the next
            tb_Add_Runner_Leaving.Text = "";
        }

        private void Add_Station_Name_IP_Medium(string Name, string IP, string Medium)
        {
            DB_Station station = new DB_Station();

            // make the Station saving possible
            Make_Saving_Possible(true);

            // put in some initial data
            if (Stations.Count != 0)
            {
                station.Previous = Stations[Stations.Count - 1].Name;
            }
            station.Name = Name;
            station.IP_Address_Callsign = IP;
            station.Medium = Medium;

            // add it to the list and display it
            Stations.Add(station);
            Bind_DB_Station_DGV();

            // also add to the Testing listbox
            lb_Stations_Testing_Entering.Items.Add(station.Name);
        }
        #endregion

        #region Info/Lists tab
        #region Runner List
        private void btn_Reload_Runner_List_Click(object sender, EventArgs e)
        {
            Load_Runner_List(tb_Upload_Runner_List_Path.Text);
//// 7/21/17            DialogResult res = MessageBox.Show("Would you like to change the name of this file to the default name?", "Change name?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
//            if (res == DialogResult.Yes)
//            {
//                File.Move(tb_Upload_Runner_List_Path.Text, RunnerListPath);
//            }
            RunnerListPath = tb_Upload_Runner_List_Path.Text;       // 7/21/17
            Notify_Runners();       // 7/22/17
        }

        private bool Load_Runner_List(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
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
//                    lbl_Runner_List_Not_available.Visible = true;
//                    lbl_Runner_List_Not_available.Update();
                    return false;
                }

                RunnerList.Clear();
                RunnerDictionary.Clear();       // 7/22/17
                lb_Runners.Items.Clear();       // 7/22/17
//                lbl_Runner_List_Not_available.Visible = false;
//                lbl_Runner_List_Not_available.Update();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        RunnersList runner = new RunnersList();
                        runner.BibNumber = Parts[0];
                        if (Parts.Length > 1)       // 7/21/17
                            runner.Name = Parts[1];
                        RunnerList.Add(runner);

                        // add to the Runner Dictionary - added 7/22/17
                        DB_Runner DBrunner = new DB_Runner();
                        DBrunner.BibNumber = Parts[0];
                        AddUpdateRunner(DBrunner, true);      // station and time not entered
                    }
                }
                dgv_RunnerList.ReadOnly = true;
                Bind_RunnerList_DGV();
                tb_Official_Number_Runners.Text = RunnerList.Count.ToString();
                tb_Official_Number_Runners2.Text = RunnerList.Count.ToString();

                // update the count flag
                if (RunnerList.Count == 0)
                    RunnerList_Has_Entries = false;
                else
                    RunnerList_Has_Entries = true;

                // close the file
                reader.Close();
            }
            return true;
        }

        private bool Load_Runner_List(string path)  // path = path of the Runner List file name
        {
            return Load_Runner_List(path, false);
        }

        private void btn_Runner_List_Browse_Click(object sender, EventArgs e)
        {
            string folderPath = "";     // set this to the previous value

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                folderPath = ofd.FileName;
                tb_Upload_Runner_List_Path.Text = folderPath;
            }
        }

        private void Bind_RunnerList_DGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_RunnerList.InvokeRequired)
            {
                BindStationDGVDel d = new BindStationDGVDel(Bind_RunnerList_DGV);
                dgv_RunnerList.Invoke(d, new object[] { });
            }
            else
            {
                dgv_RunnerList.DataSource = null;
                dgv_RunnerList.DataSource = RunnerList;
                dgv_RunnerList.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
//                dgv_RunnerList.Columns[0].Width = 48;     // Bib number
                dgv_RunnerList.Columns[0].Width = 45;     // Bib number
                dgv_RunnerList.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnerList.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_RunnerList.Columns[0].HeaderText = "Bib #";
                dgv_RunnerList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Name
                dgv_RunnerList.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnerList.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_RunnerList.Columns[1].HeaderText = "Name";
                dgv_RunnerList.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
//                dgv_RunnerList.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Gender
                dgv_RunnerList.Columns[2].Width = 50;     // Gender
                dgv_RunnerList.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnerList.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_RunnerList.Update();
            }
        }

        private void btn_Import_Runner_List_Click(object sender, EventArgs e)
        {
            ImportRunnerData Import = new ImportRunnerData();
            DialogResult res = Import.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
                Modeless_MessageBox_Exclamation("Import successful/n/nYou can load the new data file now", "New Data File read");
        }

        bool Find_Runner_in_RunnerList(string RunnerNumber)
        {
            int index = RunnerList.FindIndex(runner => runner.BibNumber == RunnerNumber);
            if (index >= 0)
                return true;
            else
                return false;
        }

        private void Notify_Runners()       // added 7/22/17
        {
            // Send Alert to all Active stations
            // message = "Runner List has changed!"
            Send_Alert_All_Active("Runner List has changed!" + Environment.NewLine);
        }
        #endregion

        #region DNS list
        bool DNS_List_Changed = false;
        StringCollection DNS_original = new StringCollection();

        private void tabPage_DNS_Enter(object sender, EventArgs e)
        {
            DNS_List_Changed = false;
            for (int i = 0; i < lb_DB_DNS.Items.Count; i++)    // make a copy of the listbox in case we need to undo
            {
// 7/24/17                DNS_original.Add((string)lb_DB_DNS.Items[i]);
                DNS_original.Add(lb_DB_DNS.Items[i].ToString());        // 7/24/17 - added .ToString()
            }
        }

        private void tabPage_DNS_Leave(object sender, EventArgs e)
        {
            if (DNS_List_Changed)
            {
                DialogResult res = MessageBox.Show("DNS List has changed!\n\nDo you want to save the changes?", "DNS List changed", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_DNS_Save_Changes_Click(null, null);
                else
                {
                    // change the list back to the original
                    lb_DB_DNS.Items.Clear();
                    foreach (string item in DNS_original)
                    {
                        lb_DB_DNS.Items.Add(item);
                    }
                    MakeVisible(btn_DNS_Save_Changes, false);       // 7/25/17
                }
            }
        }

        private void btn_Clear_DNS_Click(object sender, EventArgs e)
        {
            lb_DB_DNS.Items.Clear();
            btn_DNS_Save_Changes.Visible = true;
            btn_Clear_DNS.Visible = false;
            btn_Delete_DNS.Visible = false;
            DNSList_Has_Entries = false;
            DNS_List_Changed = true;
            tb_Number_DNS_runners.Text = "0";
        }

        private void btn_Add_DNS_Click(object sender, EventArgs e)
        {
            if (tb_Add_DNS.Text != "")
            {
                lb_DB_DNS.Items.Add(tb_Add_DNS.Text);
                tb_Add_DNS.Text = "";
// 7/22/17                lb_DB_DNS.Sorted = true;
                SortListboxNumeric(lb_DB_DNS);         // 7/22/17
                btn_DNS_Save_Changes.Visible = true;
                btn_Clear_DNS.Visible = true;
                btn_Delete_DNS.Visible = true;
                DNSList_Has_Entries = true;
                DNS_List_Changed = true;
                tb_Number_DNS_runners.Text = lb_DB_DNS.Items.Count.ToString();
            }
            else
            {
                MessageBox.Show("Enter a runner number in the textbox first!", " Missing Bib #", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btn_Delete_DNS_Click(object sender, EventArgs e)
        {
            if (lb_DB_DNS.Items.Count != 0)
            {
                // determine which runner has been selected
                string sel = (string)lb_DB_DNS.SelectedItem;

                // continue if an item has been selected
                if (sel == null)
                    return;

                DialogResult res = MessageBox.Show("Are you sure you want to delete\n\n      Runner #  " + sel, "Verify Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    lb_DB_DNS.Items.Remove(sel);
                    btn_DNS_Save_Changes.Visible = true;
                    DNS_List_Changed = true;
                    if (lb_DB_DNS.Items.Count == 0)
                    {
                        DNSList_Has_Entries = false;
                        btn_Clear_DNS.Visible = false;  // list is empty, no need to clear
                        btn_Delete_DNS.Visible = false;
                    }
                    tb_Number_DNS_runners.Text = lb_DB_DNS.Items.Count.ToString();
                }
            }
        }

        private void Notify_DNS()
        {
            // Send Alert to all Active stations
            // message = "DNS List has changed!"
            Send_Alert_All_Active("DNS List has changed!" + Environment.NewLine);
        }

        // user will click this button to Upload a DNS file provided by the Race people
        private void btn_DNS_Upload_Click(object sender, EventArgs e)
        {
            Load_DNS(tb_Upload_DNS_Path.Text);
        }

        private bool Load_DNS(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
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

                // read each item, adding to Listbox
                lb_DB_DNS.Items.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        for (int i = 0; i < Parts.Length; i++)
                        {
                            lb_DB_DNS.Items.Add(Parts[i]);
                        }
                    }
                }

                // sort it
// 7/22/17                lb_DB_DNS.Sorted = true;
                SortListboxNumeric(lb_DB_DNS);         // 7/22/17
                tb_Number_DNS_runners.Text = lb_DB_DNS.Items.Count.ToString();

                // display buttons
                if (lb_DB_DNS.Items.Count == 0)
                {
                    DNSList_Has_Entries = false;
                    btn_Clear_DNS.Visible = false;
                    btn_Delete_DNS.Visible = false;
                }
                else
                {
                    DNSList_Has_Entries = true;
                    btn_Clear_DNS.Visible = true;
                    btn_Delete_DNS.Visible = true;
                }

                // close the file
                reader.Close();
            }
            return true;
        }

        private bool Load_DNS(string path)  // path = path of the DNS file name
        {
            return Load_DNS(path, false);
        }

        private void btn_DNS_Save_Changes_Click(object sender, EventArgs e)
        {
            Save_DNS();
            Notify_DNS();
            btn_DNS_Save_Changes.Visible = false;
            DNS_List_Changed = false;
        }

        private bool Save_DNS()
        {
            string FileName = DNSListPath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (lb_DB_DNS.Items.Count == 0)
            {
                MessageBox.Show("DNS List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DNSList_Has_Entries = false;
                return false;
            }

            DNSList_Has_Entries = true;

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                                        FileName +
                //                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //                else
                //                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {

                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the DNS List could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (1 item)" + Environment.NewLine +
                            "* Bib Number, Bib Number, Bib Number . . ." + Environment.NewLine +
                            "* one or more Bib Numbers can be on a line, separated by commas" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each Runner number in the DNS list
            int i;
            for (i = 0; i < lb_DB_DNS.Items.Count; i++)
            {
// 7/24/17                string line = (string)lb_DB_DNS.Items[i];
                string line = lb_DB_DNS.Items[i].ToString();        // 7/24/17 added .ToString()
                writer.WriteLine(line);
            }
            writer.Close();

            return true;
        }

        // Browse for the DNS csv file provided by the Race people
        private void btn_Browse_DNS_Click(object sender, EventArgs e)
        {
            string folderPath = "";     // set this to the previous value

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                folderPath = ofd.FileName;
                tb_Upload_DNS_Path.Text = folderPath;
            }
        }

        private void tb_Add_DNS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_DNS_Click(null, null);
        }

        bool Find_Runner_in_DNS(string RunnerNumber)
        {
// 7/24/17            return lb_DB_DNS.Items.Contains(RunnerNumber);
            //int value = Convert.ToUInt16(RunnerNumber);
            //bool ret = lb_DB_DNS.Items.Contains(value);
            //Object val = RunnerNumber;
            //ret = lb_DB_DNS.Items.Contains(val);
            //val = value;
            //ret = lb_DB_DNS.Items.Contains(val);
            //return lb_DB_DNS.Items.Contains(Convert.ToUInt16(RunnerNumber));    // 7/24/17 had to convert to integer because Listbox contains integers after sorting
            return lb_DB_DNS.Items.Contains(RunnerNumber);
        }
        #endregion

        #region DNF list
        int DNF_Edit_index = -1;
        List<RunnerDNFWatch> DNFList_original = new List<RunnerDNFWatch>();

        private void tabPage_DNF_Enter(object sender, EventArgs e)
        {
            DNF_List_Changed = false;
            foreach (RunnerDNFWatch runner in DNFList)
            {
                DNFList_original.Add(runner);
            }
        }

        private void tabPage_DNF_Leave(object sender, EventArgs e)
        {
            if (DNF_List_Changed)
            {
                DialogResult res = MessageBox.Show("DNF List has changed!\n\nDo you want to save the changes?", "DNF List changed", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_DNF_Save_Changes_Click(null, null);
                else
                {
                    // change the list back to the original
                    foreach (RunnerDNFWatch runner in DNFList_original)
                    {
                        DNFList.Add(runner);
                    }
                }
            }
        }

        // user will click this button to Upload a DNF file provided by the Race people
        private void btn_DNF_Upload_Click(object sender, EventArgs e)
        {
            Load_DNF(tb_DNF_Upload_Path.Text);
            btn_DNF_Save_Changes.Visible = true;
        }

        private bool Load_DNF(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            StreamReader reader;

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
                        MessageBox.Show("Selected file:\n\n" + path + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                DNFList.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        RunnerDNFWatch runner = new RunnerDNFWatch();
                        runner.BibNumber = Parts[0];
                        runner.Station = Parts[1];
                        runner.Time = Parts[2];
                        runner.Note = Parts[3];
                        DNFList.Add(runner);
                    }
                }
                dgv_DNF.ReadOnly = true;
                Bind_DNF_DGV();
                tb_Number_DNF_runners.Text = DNFList.Count.ToString();

                // display buttons
                if (DNFList.Count == 0)
                {
                    DNFList_Has_Entries = false;
                    btn_Clear_DNF.Visible = false;
                    btn_Delete_DNF.Visible = false;
                }
                else
                {
                    DNFList_Has_Entries = true;
                    btn_Clear_DNF.Visible = true;
                    btn_Delete_DNF.Visible = true;
                }

                // close the file
                reader.Close();
            }
            return true;
        }

        private bool Load_DNF(string path)  // path = path of the DNF file name
        {
            return Load_DNF(path, false);
        }

        private void Bind_DNF_DGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_DNF.InvokeRequired)
            {
                BindStationDGVDel d = new BindStationDGVDel(Bind_DNF_DGV);
                dgv_DNF.Invoke(d, new object[] { });
            }
            else
            {
                dgv_DNF.DataSource = null;
                dgv_DNF.DataSource = DNFList;
                dgv_DNF.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DNF.Columns[0].Width = 48;     // Bib number
                dgv_DNF.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DNF.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_DNF.Columns[0].HeaderText = "Bib #";
                dgv_DNF.Columns[1].Width = Station_DGV_Width;     // Station
                dgv_DNF.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DNF.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_DNF.Columns[1].HeaderText = "Station";
                dgv_DNF.Columns[2].Width = 54;    // Time
                dgv_DNF.Columns[2].HeaderText = "Time";
                dgv_DNF.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
//                dgv_DNF.Columns[3].Width = 539;     // Notes
                dgv_DNF.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Notes
                dgv_DNF.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DNF.Columns[3].HeaderText = "Notes";
                dgv_DNF.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dgv_DNF.Update();
            }
        }

        private void btn_Clear_DNF_Click(object sender, EventArgs e)
        {
            DNFList.Clear();
            Editting_DNF = false;
            dgv_DNF.ReadOnly = true;
            Bind_DNF_DGV();
            DNFList_Has_Entries = false;
            btn_DNF_Save_Changes.Visible = true;
            btn_Clear_DNF.Visible = false;
            btn_Delete_DNF.Visible = false;
            tb_Number_DNF_runners.Text = "0";
        }

        private void AddDNFRunner(RunnerDNFWatch runnerDNFWatch)
        {
            DNFList.Add(runnerDNFWatch);
            DNFList.Sort(
                delegate(RunnerDNFWatch l1, RunnerDNFWatch l2)
                {
                    return l1.BibNumber.CompareTo(l2.BibNumber);
                }
                );
            Bind_DNF_DGV();
            DNFList_Has_Entries = true;
            MakeVisible(btn_DNF_Save_Changes, true);
            MakeVisible(btn_Clear_DNF, true);
            MakeVisible(btn_Delete_DNF, true);
            DNF_List_Changed = true;
            SetTBtext(tb_Number_DNF_runners, DNFList.Count.ToString());
        }

        private void btn_Add_DNF_Click(object sender, EventArgs e)
        {
            if (tb_DNF_Add.Text != "")
            {
                RunnerDNFWatch runner = new RunnerDNFWatch();
                runner.BibNumber = tb_DNF_Add.Text;
                DNFList.Add(runner);
                tb_DNF_Add.Text = "";
                DNFList.Sort(
                    delegate(RunnerDNFWatch l1, RunnerDNFWatch l2)
                    {
                        return l1.BibNumber.CompareTo(l2.BibNumber);
                    }
                    );
                Bind_DNF_DGV();
                MakeVisible(btn_DNF_Save_Changes, true);
                MakeVisible(btn_Clear_DNF, true);
                MakeVisible(btn_Delete_DNF, true);
                DNFList_Has_Entries = true;
                DNF_List_Changed = true;
                tb_Number_DNF_runners.Text = DNFList.Count.ToString();
            }
            else
            {
                MessageBox.Show("Enter a runner number in the textbox first!", " Missing Bib #", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btn_Delete_DNF_Click(object sender, EventArgs e)
        {
            if (DNFList.Count != 0)
            {
                // determine which runner has been selected
                int index = dgv_DNF.CurrentRow.Index;

                // continue if an item has been selected
                if (index == -1)
                    return;

                DialogResult res = MessageBox.Show("Are you sure you want to delete\n\n      Runner #  " + DNFList[index].BibNumber, "Verify Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    DNFList.RemoveAt(index);
                    Bind_DNF_DGV();
                    btn_DNF_Save_Changes.Visible = true;
                    if (DNFList.Count == 0)
                    {
                        DNFList_Has_Entries = false;
                        btn_Clear_DNF.Visible = false;  // list is empty, no need to clear
                        btn_Delete_DNF.Visible = false;
                    }
                }
                tb_Number_DNF_runners.Text = DNFList.Count.ToString();
            }
        }

        private void btn_DNF_Edit_Click(object sender, EventArgs e)
        {
            if (DNFList.Count != 0)
            {
                // determine which runner has been selected
                DNF_Edit_index = dgv_DNF.CurrentRow.Index;

                // continue if an item has been selected
                if (DNF_Edit_index == -1)
                    return;

                // now which field is being editted?  (0 = number, 1 = station, 2 = time, 3 = notes)
                Point rowcol = dgv_DNF.CurrentCellAddress;

                // proceed only if a cell is selected
                if (rowcol.X == -1)
                    return;

                // start editting
                dgv_DNF.ReadOnly = false;
                dgv_DNF.BeginEdit(true);
                Editting_DNF = true;
            }
        }

        private void dgv_DNF_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // test when enter a cell to see if still on the same runner.
            // if change runner, turn off editting
            if (Editting_DNF)
            {
                int index = dgv_DNF.CurrentRow.Index;
                if (index != DNF_Edit_index)
                {
                    Editting_DNF = false;
                    dgv_DNF.ReadOnly = true;
                }
            }
        }

        private void dgv_DNF_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            // need to save the changed value
            if (Editting_DNF)
            {
                Point rowcol = dgv_DNF.CurrentCellAddress;
                string text = (string)dgv_DNF.CurrentCell.Value;
                switch (rowcol.X)
                {
                    case 0:
                        DNFList[rowcol.Y].BibNumber = text;
                        break;
                    case 1:
                        DNFList[rowcol.Y].Station = text;
                        break;
                    case 2:
                        DNFList[rowcol.Y].Time = text;
                        break;
                    case 3:
                        DNFList[rowcol.Y].Note = text;
                        break;
                }
                btn_DNF_Save_Changes.Visible = true;
            }
        }

        private void dgv_DNF_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl tb = e.Control as DataGridViewTextBoxEditingControl;
                tb.KeyDown -= dgv_DNF_KeyDown;
                tb.KeyDown += new KeyEventHandler(dgv_DNF_KeyDown);
            }
        }

        private void dgv_DNF_KeyDown(object sender, KeyEventArgs e)
        {
            btn_DNF_Save_Changes.Visible = true;
        }

        private void btn_DNF_Save_Changes_Click(object sender, EventArgs e)
        {
            Save_DNF();
            Notify_DNF();
            Editting_DNF = false;
            dgv_DNF.ReadOnly = true;
            btn_DNF_Save_Changes.Visible = false;
            DNF_List_Changed = false;
        }

        bool Save_DNF()
        {
//            string FileName = DataDirectory + "\\DNFlist.txt";
            string FileName = DNFListPath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (DNFList.Count == 0)
            {
                MessageBox.Show("DNF List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DNSList_Has_Entries = false;
                return false;
            }

            DNSList_Has_Entries = true;

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                        FileName +
                //                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //else
                //    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the DNF List could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (4 items)" + Environment.NewLine +
                            "* Bib Number, Station name, Time, Notes" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each runner
            foreach (RunnerDNFWatch runner in DNFList)
            {
                string line = runner.BibNumber + ",";
                line += runner.Station + ",";
                line += runner.Time + ",";
                line += runner.Note;
                writer.WriteLine(line);
            }
            writer.Close();
            return true;
        }

        private void Notify_DNF()
        {
            // Send Alert to all Active stations
            // message = "DNF List has changed!"
            Send_Alert_All_Active("DNF List has changed!" + Environment.NewLine);
        }

        // Browse for the DNF csv file provided by the Race people
        private void btn_Browse_DNF_Click(object sender, EventArgs e)
        {
            string folderPath = "";     // set this to the previous value

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                folderPath = ofd.FileName;
                tb_DNF_Upload_Path.Text = folderPath;
            }
        }

        private void tb_DNF_Add_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_DNF_Click(null, null);
        }

        bool Find_Runner_in_DNF(string RunnerNumber)
        {
            int index = DNFList.FindIndex(runner => runner.BibNumber == RunnerNumber);
            if (index >= 0)
                return true;
            else
                return false;
        }

        private void dgv_DNF_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)      // 5/12/19
        {
            if ((e.ColumnIndex == 1))
            {
                DataGridViewCell cell = dgv_DNF.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.ToolTipText = "Click the Station cell while editting to select a Station";
            }
        }

        private void dgv_DNF_CellClick(object sender, DataGridViewCellEventArgs e)      // 5/12/19
        {
            if ((e.ColumnIndex == 1) && Editting_DNF)
            {
                SelectStationForm select = new SelectStationForm();
                DialogResult res = select.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    dgv_DNF.CurrentCell.Value = select.Station_Name;
                }
            }
        }
        #endregion

        #region Watch list
//        bool Watch_List_Changed = false;
//        bool Editting_Watch = false;
        int Watch_Edit_index = -1;
        List<RunnerDNFWatch> WatchList_original = new List<RunnerDNFWatch>();

        private void tabPage_Watch_Enter(object sender, EventArgs e)
        {
            Watch_List_Changed = false;
            foreach (RunnerDNFWatch runner in WatchList)
            {
                WatchList_original.Add(runner);
            }
        }

        private void tabPage_Watch_Leave(object sender, EventArgs e)
        {
            if (Watch_List_Changed)
            {
                DialogResult res = MessageBox.Show("Watch List has changed!\n\nDo you want to save the changes?", "Watch List changed", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_Watch_Save_Changes_Click(null, null);
                else
                {
                    // change the list back to the original
                    foreach (RunnerDNFWatch runner in WatchList_original)
                    {
                        WatchList.Add(runner);
                    }
                }
            }

        }

        // user will click this button to Upload a Watch file provided by the Race people
        private void btn_Watch_Upload_Click(object sender, EventArgs e)
        {
            Load_Watch(tb_Watch_Upload_Path.Text);
            btn_Watch_Save_Changes.Visible = true;
        }

        private bool Load_Watch(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            StreamReader reader;

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
                        MessageBox.Show("Selected file:\n\n" + path + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                WatchList.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        RunnerDNFWatch runner = new RunnerDNFWatch();
                        runner.BibNumber = Parts[0];
                        runner.Station = Parts[1];
                        runner.Time = Parts[2];
                        runner.Note = Parts[3];
                        WatchList.Add(runner);
                    }
                }
                dgv_Watch.ReadOnly = true;
                Bind_Watch_DGV();
                tb_Number_Watch_runners.Text = WatchList.Count.ToString();

                // display buttons
                if (WatchList.Count == 0)
                {
                    WatchList_Has_Entries = false;
                    btn_Clear_Watch.Visible = false;
                    btn_Delete_Watch.Visible = false;
                }
                else
                {
                    WatchList_Has_Entries = true;
                    btn_Clear_Watch.Visible = true;
                    btn_Delete_Watch.Visible = true;
                }

                // close the file
                reader.Close();
            }
            return true;
        }

        private bool Load_Watch(string path)  // path = path of the DNF file name
        {
            return Load_Watch(path, false);
        }

        private void Bind_Watch_DGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_Watch.InvokeRequired)
            {
                BindStationDGVDel d = new BindStationDGVDel(Bind_Watch_DGV);
                dgv_Watch.Invoke(d, new object[] { });
            }
            else
            {
                dgv_Watch.DataSource = null;
                dgv_Watch.DataSource = WatchList;
                dgv_Watch.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Watch.Columns[0].Width = 48;     // Bib number
                dgv_Watch.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Watch.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Watch.Columns[0].HeaderText = "Bib #";
                dgv_Watch.Columns[1].Width = Station_DGV_Width;     // Station
                dgv_Watch.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Watch.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_Watch.Columns[1].HeaderText = "Station";
                dgv_Watch.Columns[2].Width = 54;    // Time
                dgv_Watch.Columns[2].HeaderText = "Time";
//                dgv_Watch.Columns[3].Width = 539;     // Notes
                dgv_Watch.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Notes
                dgv_Watch.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Watch.Columns[3].HeaderText = "Notes";
                dgv_Watch.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dgv_Watch.Update();
            }
        }

        private void btn_Clear_Watch_Click(object sender, EventArgs e)
        {
            WatchList.Clear();
            Editting_Watch = false;
            dgv_Watch.ReadOnly = true;
            Bind_Watch_DGV();
            WatchList_Has_Entries = false;
            btn_Watch_Save_Changes.Visible = true;
            btn_Clear_Watch.Visible = false;
            btn_Delete_Watch.Visible = false;
            tb_Number_Watch_runners.Text = "0";
        }

        private void AddWatchRunner(RunnerDNFWatch runnerDNFWatch)
        {
            WatchList.Add(runnerDNFWatch);
            WatchList.Sort(
                delegate(RunnerDNFWatch l1, RunnerDNFWatch l2)
                {
                    return l1.BibNumber.CompareTo(l2.BibNumber);
                }
                );
            Bind_Watch_DGV();
            MakeVisible(btn_Watch_Save_Changes, true);
            MakeVisible(btn_Clear_Watch, true);
            MakeVisible(btn_Delete_Watch, true);
            WatchList_Has_Entries = true;
            Watch_List_Changed = true;
//            tb_Number_Watch_runners.Text = WatchList.Count.ToString();
            SetTBtext(tb_Number_Watch_runners, WatchList.Count.ToString());
        }

        private void btn_Add_Watch_Click(object sender, EventArgs e)
        {
            if (tb_Watch_Add.Text != "")
            {
                RunnerDNFWatch runner = new RunnerDNFWatch();
                runner.BibNumber = tb_Watch_Add.Text;
                WatchList.Add(runner);
                tb_Watch_Add.Text = "";
                WatchList.Sort(
                    delegate(RunnerDNFWatch l1, RunnerDNFWatch l2)
                    {
                        return l1.BibNumber.CompareTo(l2.BibNumber);
                    }
                    );
                Bind_Watch_DGV();
                btn_Watch_Save_Changes.Visible = true;
                btn_Clear_Watch.Visible = true;
                btn_Delete_Watch.Visible = true;
                WatchList_Has_Entries = true;
                Watch_List_Changed = true;
                //            tb_Number_Watch_runners.Text = WatchList.Count.ToString();
                SetTBtext(tb_Number_Watch_runners, WatchList.Count.ToString());
            }
            else
            {
                MessageBox.Show("Enter a runner number in the textbox first!", " Missing Bib #", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btn_Delete_Watch_Click(object sender, EventArgs e)
        {
            if (WatchList.Count != 0)
            {
                // determine which runner has been selected
                int index = dgv_Watch.CurrentRow.Index;

                // continue if an item has been selected
                if (index == -1)
                    return;

                DialogResult res = MessageBox.Show("Are you sure you want to delete\n\n      Runner #  " + WatchList[index].BibNumber, "Verify Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    WatchList.RemoveAt(index);
                    Bind_Watch_DGV();
                    btn_Watch_Save_Changes.Visible = true;
                    if (WatchList.Count == 0)
                    {
                        WatchList_Has_Entries = false;
                        btn_Clear_Watch.Visible = false;    // empty, no need to clear
                        btn_Delete_Watch.Visible = false;
//                        tb_Number_Watch_runners.Text = WatchList.Count.ToString();
                    }
                    //            tb_Number_Watch_runners.Text = WatchList.Count.ToString();
                    SetTBtext(tb_Number_Watch_runners, WatchList.Count.ToString());
                }
            }
        }

        private void btn_Watch_Edit_Click(object sender, EventArgs e)
        {
            if (WatchList.Count != 0)
            {
                // determine which runner has been selected
                Watch_Edit_index = dgv_Watch.CurrentRow.Index;

                // continue if an item has been selected
                if (Watch_Edit_index == -1)
                    return;

                // now which field is being editted?  (0 = number, 1 = station, 2 = time, 3 = notes)
                Point rowcol = dgv_Watch.CurrentCellAddress;

                // proceed only if a cell is selected
                if (rowcol.X == -1)
                    return;

                // start editting
                dgv_Watch.ReadOnly = false;
                dgv_Watch.BeginEdit(true);
                Editting_Watch = true;
            }
        }

        private void dgv_Watch_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // test when enter a cell to see if still on the same runner.
            // if change runner, turn off editting
            if (Editting_Watch)
            {
                int index = dgv_Watch.CurrentRow.Index;
                if (index != Watch_Edit_index)
                {
                    Editting_Watch = false;
                    dgv_Watch.ReadOnly = true;
                }
            }
        }

        private void dgv_Watch_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            // need to save the changed value
            if (Editting_Watch)
            {
                Point rowcol = dgv_Watch.CurrentCellAddress;
                string text = (string)dgv_Watch.CurrentCell.Value;
                switch (rowcol.X)
                {
                    case 0:
                        WatchList[rowcol.Y].BibNumber = text;
                        break;
                    case 1:
                        WatchList[rowcol.Y].Station = text;
                        break;
                    case 2:
                        WatchList[rowcol.Y].Time = text;
                        break;
                    case 3:
                        WatchList[rowcol.Y].Note = text;
                        break;
                }
                btn_Watch_Save_Changes.Visible = true;
            }
        }

        private void dgv_Watch_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl tb = e.Control as DataGridViewTextBoxEditingControl;
                tb.KeyDown -= dgv_Watch_KeyDown;
                tb.KeyDown += new KeyEventHandler(dgv_Watch_KeyDown);
            }
        }

        private void dgv_Watch_KeyDown(object sender, KeyEventArgs e)
        {
            btn_Watch_Save_Changes.Visible = true;
        }

        private void btn_Watch_Save_Changes_Click(object sender, EventArgs e)
        {
            Save_Watch();
            Notify_Watch();
            Editting_Watch = false;
            dgv_Watch.ReadOnly = true;
            btn_Watch_Save_Changes.Visible = false;
            Watch_List_Changed = false;
        }

        bool Save_Watch()
        {
//            string FileName = DataDirectory + "\\Watchlist.txt";
            string FileName = WatchListPath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (WatchList.Count == 0)
            {
                MessageBox.Show("Watch List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                WatchList_Has_Entries = false;
                return false;
            }

            WatchList_Has_Entries = true;

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                        FileName +
                //                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //else
                //    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the Watch List could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (4 items)" + Environment.NewLine +
                            "* Bib Number, Station name, Time, Notes" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each runner
            foreach (RunnerDNFWatch runner in WatchList)
            {
                string line = runner.BibNumber + ",";
                line += runner.Station + ",";
                line += runner.Time + ",";
                line += runner.Note;
                writer.WriteLine(line);
            }
            writer.Close();
            return true;
        }

        private void Notify_Watch()
        {
            // Send Alert to all Active stations
            // message = "Watch List has changed!"
            Send_Alert_All_Active("Watch List has changed!" + Environment.NewLine);
        }

        // Browse for the Watch csv file provided by the Race people
        private void btn_Watch_Upload_Browse_Click(object sender, EventArgs e)
        {
            string folderPath = "";     // set this to the previous value

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                folderPath = ofd.FileName;
                tb_Watch_Upload_Path.Text = folderPath;
            }
        }

        private void tb_Watch_Add_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_Watch_Click(null, null);
        }

        bool Find_Runner_in_Watch(string RunnerNumber)
        {
            int index = WatchList.FindIndex(runner => runner.BibNumber == RunnerNumber);
            if (index >= 0)
                return true;
            else
                return false;
        }

        private void dgv_Watch_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)     // 5/15/19
        {
            if ((e.ColumnIndex == 1))
            {
                DataGridViewCell cell = dgv_Watch.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.ToolTipText = "Click the Station cell while editting to select a Station";
            }
        }

        private void dgv_Watch_CellClick(object sender, DataGridViewCellEventArgs e)    // 5/15/19
        {
            if ((e.ColumnIndex == 1) && Editting_Watch)
            {
                SelectStationForm select = new SelectStationForm();
                DialogResult res = select.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    dgv_Watch.CurrentCell.Value = select.Station_Name;
                }
            }
        }
        #endregion

        #region Info File
        bool DatePickerTimeChanged = false;
// 3/12/19        bool DatePickerDateChanged = false;

        private bool Load_Info(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            string[] splitter = new string[] { ": " };
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

                // read each item, extracting the information
                Loading_Info = true;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    if (!Parts[0].StartsWith("*"))
                    {
                        switch (Parts[0])
                        {
                            case "Race Name":
                                tb_RaceName.Text = Parts[1];
                                break;
                            case "Race Location":
                                tb_RaceLocation.Text = Parts[1];
                                break;
                            case "Sponsor":
                                tb_RaceSponsor.Text = Parts[1];
                                break;
                            case "Race Date":
                                dateTimePicker_Date.Value = Convert.ToDateTime(Parts[1]);
                                break;
                            case "Start Time":
                                dateTimePicker_Time.Value = Convert.ToDateTime(Parts[1]);
// 3/12/19                                Start_Time = dateTimePicker_Time.Value.ToShortTimeString();
// 3/12/19                                tb_Start_Time.Text = Start_Time;
                                Start_Time = dateTimePicker_Time.Value.ToString("HH:mm");
                                SetTBtext(tb_Start_Time, Start_Time);
                                break;
                            case "# of Runners":
                                tb_NumberofRunners.Text = Parts[1];
                                break;
                            case "Contact person":
                                tb_ContactName.Text = Parts[1];
                                break;
                            case "Contact phone":
                                tb_ContactPhone.Text = Parts[1];
                                break;
                            case "Packet Frequency":
                                tb_Info_Packet_Frequency.Text = Parts[1];
                                break;
                        }
                    }
                }
                Loading_Info = false;

                // close the file
                reader.Close();
            }
            return true;
        }

        private bool Load_Info(string path)
        {
            return Load_Info(path, false);
        }

        private void btn_Save_Info_Click(object sender, EventArgs e)
        {
            btn_Save_Info.Visible = false;
            Save_Info();
            InfoLoaded = true;
            Notify_Info();

            // after saving, put time in Start Time textbox
//            tb_Start_Time.Text = dateTimePicker_Time.Text;
// 3/12/19            tb_Start_Time.Text = dateTimePicker_Time.Value.ToShortTimeString();
            Start_Time = dateTimePicker_Time.Value.ToString("HH:mm");
            SetTBtext(tb_Start_Time, Start_Time);
        }

        private void Notify_Info()
        {
            // Send Alert to all Active stations
            // message = "Info File has changed!"
            Send_Alert_All_Active("Info File has changed!" + Environment.NewLine);
        }

        private bool Save_Info()
        {
            string FileName = InfoFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                                        FileName +
                //                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //                else
                //                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {

                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the Race Info could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (9 items, each on its own line)" + Environment.NewLine +
                            "* Race Name:, Race Location:, Sponsor:, Race Date:, Start Time:, # of Runners:, Contact person:, Contact phone:, Packet Network/Frequency:" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each item in the Info file
            Race_Info Info = new Race_Info();
            string line;
            if (tb_RaceName.Text != "")
            {
                line = "Race Name: " + tb_RaceName.Text;
                writer.WriteLine(line);
                Info.Name = tb_RaceName.Text;
            }
            if (tb_RaceLocation.Text != "")
            {
                line = "Race Location: " + tb_RaceLocation.Text;
                writer.WriteLine(line);
                Info.Location = tb_RaceLocation.Text;
            }
            if (tb_RaceSponsor.Text != "")
            {
                line = "Sponsor: " + tb_RaceSponsor.Text;
                writer.WriteLine(line);
                Info.Sponsor = tb_RaceSponsor.Text;
            }
            //            if (DatePickerDateChanged)
            // always save the Race Date
            {
                line = "Race Date: " + dateTimePicker_Date.Value.ToShortDateString();
                writer.WriteLine(line);
                Info.Date = dateTimePicker_Date.Value.ToShortDateString();
// 3/12/19                DatePickerDateChanged = false;
            }
            // always save the Race Time
            {
// 3/12/19                tb_Start_Time.Text = dateTimePicker_Time.Value.ToShortTimeString();
// 3/12/19                line = "Start Time: " + tb_Start_Time.Text;
                line = "Start Time: " + Start_Time;
                writer.WriteLine(line);
// 3/12/19                Info.Time = tb_Start_Time.Text;
                Info.Time = Start_Time;
            }
            if (tb_NumberofRunners.Text != "")
            {
                line = "# of Runners: " + tb_NumberofRunners.Text;
                writer.WriteLine(line);
                Info.Count = tb_NumberofRunners.Text;
            }
            if (tb_ContactName.Text != "")
            {
                line = "Contact person: " + tb_ContactName.Text;
                writer.WriteLine(line);
                Info.Contact_Name = tb_ContactName.Text;
            }
            if (tb_ContactPhone.Text != "")
            {
                line = "Contact phone: " + tb_ContactPhone.Text;
                writer.WriteLine(line);
                Info.Contact_Phone = tb_ContactPhone.Text;
            }
            if (tb_Info_Packet_Frequency.Text != "")
            {
                line = "Packet Frequency: " + tb_Info_Packet_Frequency.Text;
                writer.WriteLine(line);
                Info.Packet = tb_Info_Packet_Frequency.Text;
            }
            writer.Close();
            DataFile.Save_Info(tb_DB_Settings_RunnersDataFile.Text, Info);

            // notify the Aid stations if the time changed
            if (DatePickerTimeChanged)
            {
// 3/12/19                Start_Time = dateTimePicker_Time.Value.ToShortTimeString();
                Start_Time = dateTimePicker_Time.Value.ToString("HH:mm");
                Send_Alert_All_Active("New Start Time");

                // need to notify
                DatePickerTimeChanged = false;
            }

            return true;
        }

        #region testboxes
        private void Make_Save_Visible()
        {
            if (!Loading_Info)
                btn_Save_Info.Visible = true;
        }

        private void tb_RaceName_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }

        private void tb_RaceLocation_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }

        private void tb_RaceSponsor_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }

        private void dateTimePicker_Date_ValueChanged(object sender, EventArgs e)
        {
            if (!Loading_Info)
            {
                btn_Save_Info.Visible = true;
// 3/12/19                DatePickerDateChanged = true;
            }
        }

        private void dateTimePicker_Time_ValueChanged(object sender, EventArgs e)
        {
            if (!Loading_Info)
            {
                btn_Save_Info.Visible = true;
                DatePickerTimeChanged = true;
            }
        }

        private void tb_NumberofRunners_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }

        private void tb_ContactName_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }

        private void tb_ContactPhone_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }

        private void tb_Info_Packet_Frequency_TextChanged(object sender, EventArgs e)
        {
            Make_Save_Visible();
        }
        #endregion
        #endregion
        #endregion

        #region Issues tab
        #region Database Issues tab
        // This is newer code from the Aid Station program.  It may be better than the code in the next tab
        bool Adding_DB_Issue = false;
        bool Loading_DB_Issues = false;

        private bool Load_DB_Issues(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { '|' };
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

                // read each item, extracting the information
                Loading_DB_Issues = true;
                DBIssues.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        DB_Issue issue = new DB_Issue();
                        issue.EntryDate = Parts[0];
                        issue.ResolveDate = Parts[1];
                        issue.EntryPerson = Parts[2];
                        if (Parts[3] == "B")
                            issue.Broken = true;
                        else
                            issue.Broken = false;
                        if (Parts[3] == "E")
                            issue.Enhancement = true;
                        else
                            issue.Enhancement = false;
                        issue.Description = Parts[4];
                        DBIssues.Add(issue);
                    }
                }
                Loading_DB_Issues = false;

                // close the file
                reader.Close();

                // display it
                Bind_DB_Issues_DGV();
            }
            return true;
        }

        private bool Save_DB_Issues()
        {
            string FileName = DBIssuesFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                                        FileName +
                //                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //                else
                //                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the Issues List could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (5 items, separated by the '|' character)" + Environment.NewLine +
                            "* Entry Date, Resolve Date, Entry Person, Type (\"B\" or \"E\"), Description" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each item in the Issues List
            foreach (DB_Issue issue in DBIssues)
            {
                string line = issue.EntryDate + "|";
                line += issue.ResolveDate + "|";
                line += issue.EntryPerson + "|";
                if (issue.Broken)
                    line += "B|";
                if (issue.Enhancement)
                    line += "E|";
                line += issue.Description;
                writer.WriteLine(line);
            }
            writer.Close();

            return true;
        }

        private bool Add_DB_Issue_to_File(string EntryPerson, string Type, string Description) // Type = "B" or "E"
        {
            string FileName = DBIssuesFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                // verify the file is good by trying to open it
                try
                {
                    //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    //                    fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                    fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            else
            {       // new file - tell the user the file does not exist
                MessageBox.Show("Selected file:\n\n" + FileName + "\n\ndoes not exist!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // append this one item to the Issues List file
            string line = DateTime.Now.ToString("MM/dd/yy") + "||";      // no Resolve Date entered
            //            string line = Environment.NewLine;
            //            line += DateTime.Now.ToString("MM/dd/yy") + "||";      // no Resolve Date entered
            line += EntryPerson + "|";
            line += Type + "|";
            line += Description;
            writer.WriteLine(line);
            writer.Close();

            return true;
        }

        private bool Send_DB_Issues_before_CLosing()
        {
            return true;
        }

        private void btn_Load_DB_Issues_Click(object sender, EventArgs e)
        {
            //btn_Get_Issues.Text = "Downloading";
            //WorkerObject.Download = btn_Get_Issues;
            ////            tb_Downloaded_Station_Info_File.Visible = true;
            ////            btn_Browse_Downloaded_Station_Info_File.Visible = true;
            //SendCommand(Commands.RequestIssues, "");

            Load_DB_Issues(DBIssuesFilePath, false);
        }

        private void btn_Add_New_DB_Issue_Click(object sender, EventArgs e)
        {
            // has the info already been entered?
            if (!Adding_DB_Issue)
            {
                // clear the tecxtboxes and radio buttons
                tb_DB_Issue_Name.Clear();
                tb_DB_Issue_Issue.Clear();
                rb_DB_Issues_Enhancement.Checked = false;
                rb_DB_Issues_Broken.Checked = false;

                // make the labels and textboxes visible
                panel_DB_Issues.Visible = true;
// 7/26/17                btn_Add_New_DB_Issue.Enabled = false;
                MakeEnabled(btn_Add_New_DB_Issue, false);   // 7/26/17
                btn_DB_Issues_Cancel_Add.Visible = true;
                Application.DoEvents();

                // tell user to enter info and click it again
                MessageBox.Show("Enter name and issue info,\n\nThen click the Add button again.", "Click again", MessageBoxButtons.OK, MessageBoxIcon.Hand);

                // change the flag
                Adding_DB_Issue = true;
            }
            else
            {
                // test if the two textboxes have data in them.
                if ((tb_DB_Issue_Issue.Text == "") || (tb_DB_Issue_Name.Text == ""))
                {
                    // tell user to enter info and click it again
                    MessageBox.Show("Enter name and issue info,\n\nThen click the Add button again.", "Click again", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    // add to the list
                    DB_Issue issue = new DB_Issue();
                    issue.EntryPerson = tb_DB_Issue_Name.Text;
                    issue.Description = tb_DB_Issue_Issue.Text;
                    if (rb_DB_Issues_Broken.Checked)
                        issue.Broken = true;
                    if (rb_DB_Issues_Enhancement.Checked)
                        issue.Enhancement = true;
                    //                    issue.EntryDate = DateTime.Now.ToShortDateString("MM/dd/yy");
                    issue.EntryDate = DateTime.Now.ToString("MM/dd/yy");
                    DBIssues.Add(issue);
                    Bind_DB_Issues_DGV();

                    // add to the local file
                    if (DBIssuesLoaded)
                    {
                        // add just one line
                        string type = string.Empty;
                        if (rb_DB_Issues_Broken.Checked)
                            type = "B";
                        if (rb_DB_Issues_Enhancement.Checked)
                            type = "E";
                        Add_DB_Issue_to_File(tb_DB_Issue_Name.Text, type, tb_DB_Issue_Issue.Text);
                    }
                    else
                    {   // no file loaded on start up - need to create the file
                        Save_DB_Issues();
                    }

                    // make the labels and textboxes invisible
                    panel_DB_Issues.Visible = false;
                    btn_DB_Issues_Cancel_Add.Visible = false;
                    Application.DoEvents();

                    // change the flag
                    Adding_DB_Issue = false;
                }
            }
        }

        //private void btn_Send_DB_Issue_to_DB_Click(object sender, EventArgs e)
        //{
        //    // send to the Central Database
        //    btn_Send_Issue_to_DB.Text = "Sending";
        //    WorkerObject.Download = btn_Send_Issue_to_DB;
        //    //            tb_Downloaded_Station_Info_File.Visible = true;
        //    //            btn_Browse_Downloaded_Station_Info_File.Visible = true;
        //    SendCommand(Commands.SendIssue, "");

        //    // make the button invisible
        //    btn_Send_Issue_to_DB.Visible = false;
        //    btn_Send_Issue_to_DB.Update();
        //}

        private void btn_DB_Issues_Cancel_Add_Click(object sender, EventArgs e)
        {
//// 7/26/17            panel_AS_Issues.Visible = false;
//            btn_DB_Issues_Cancel_Add.Visible = false;
//            btn_Add_New_DB_Issue.Enabled = true;
            MakeEnabled(btn_Add_New_DB_Issue,true);     // 7/26/17
            MakeVisible(btn_DB_Issues_Cancel_Add, false);   // 7/26/17
            MakeVisible(btn_DB_Issues_Cancel_Add, false);   // 7/26/17
            Adding_DB_Issue = false;
        }

        private void rb_DB_Issues_Broken_CheckedChanged(object sender, EventArgs e)
        {
            Test_Add_DB_Issue();
        }

        private void rb_DB_Issues_Enhancement_CheckedChanged(object sender, EventArgs e)
        {
            Test_Add_DB_Issue();
        }

        private void tb_DB_Issue_Name_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Issue_Name.Text == "")
                tb_DB_Issue_Name.BackColor = Color.FromArgb(255, 224, 192);
            else
                tb_DB_Issue_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
            Test_Add_DB_Issue();
        }

        private void tb_DB_Issue_Issue_TextChanged(object sender, EventArgs e)
        {
            if (tb_DB_Issue_Issue.Text == "")
                tb_DB_Issue_Issue.BackColor = Color.FromArgb(255, 224, 192);
            else
                tb_DB_Issue_Issue.BackColor = Color.FromKnownColor(KnownColor.Window);
            Test_Add_DB_Issue();
        }

        private void Test_Add_DB_Issue()
        {
            if ((tb_DB_Issue_Issue.Text == "") || (tb_DB_Issue_Name.Text == "") || ((!rb_DB_Issues_Enhancement.Checked) && (!rb_DB_Issues_Broken.Checked)))
// 7/26/17                btn_Add_New_DB_Issue.Enabled = false;
                MakeEnabled(btn_Add_New_DB_Issue, false);   // 7/26/17
            else
// 7/26/17                btn_Add_New_DB_Issue.Enabled = true;
                MakeEnabled(btn_Add_New_DB_Issue, false);   // 7/26/17
        }

        private void Bind_DB_Issues_DGV()
        {
            dgv_DB_Issues.DataSource = null;
            if (DBIssues.Count != 0)
            {
                dgv_DB_Issues.DataSource = DBIssues;
                dgv_DB_Issues.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[0].Width = 55;     // Entry Date
                dgv_DB_Issues.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_DB_Issues.Columns[0].HeaderText = "Entry Date";
                dgv_DB_Issues.Columns[1].Width = 55;     // Resolve Date
                dgv_DB_Issues.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_DB_Issues.Columns[1].HeaderText = "Resolve Date";
                dgv_DB_Issues.Columns[2].Width = 98;    // Entry Person
                dgv_DB_Issues.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //                dgv_Issues.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_DB_Issues.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dgv_DB_Issues.Columns[2].HeaderText = "Entry Person";
                dgv_DB_Issues.Columns[3].Width = 43;     // Broken type
                dgv_DB_Issues.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_DB_Issues.Columns[3].HeaderText = "Broken";
                dgv_DB_Issues.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[4].Width = 73;     // Enhancement type
                dgv_DB_Issues.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                dgv_DB_Issues.Columns[4].HeaderText = "Enhancement";
                dgv_DB_Issues.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //                dgv_Issues.Columns[5].Width = 539;     // Description
                dgv_DB_Issues.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Description
                dgv_DB_Issues.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_DB_Issues.Columns[5].HeaderText = "Description";
                dgv_DB_Issues.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            dgv_DB_Issues.Update();
        }
        #endregion

        #region Aid Station Issues tab
        bool Adding_AS_Issue = false;
        bool Loading_AS_Issues = false;

        private bool Load_AS_Issues(string path, bool suppress_error_msg)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { '|' };
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

                // read each item, extracting the information
                Loading_AS_Issues = true;
                ASIssues.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        Aid_Issue issue = new Aid_Issue();
                        issue.EntryDate = Parts[0];
                        issue.ResolveDate = Parts[1];
                        issue.EntryPerson = Parts[2];
                        issue.Station = Parts[3];
                        if (Parts[4] == "B")
                            issue.Broken = true;
                        else
                            issue.Broken = false;
                        if (Parts[4] == "E")
                            issue.Enhancement = true;
                        else
                            issue.Enhancement = false;
                        issue.Description = Parts[5];
                        ASIssues.Add(issue);
                    }
                }
                Loading_AS_Issues = false;

                // close the file
                reader.Close();

                // display it
                Bind_AS_Issues_DGV();
            }
            return true;
        }

        private bool Save_AS_Issues()
        {
            string FileName = ASIssuesFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                                        FileName +
                //                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //                else
                //                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the Issues List could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (5 items, separated by the '|' character)" + Environment.NewLine +
                            "* Entry Date, Resolve Date, Entry Person, Type (\"B\" or \"E\"), Description" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each item in the Issues List
            foreach (Aid_Issue issue in ASIssues)
            {
                string line = issue.EntryDate + "|";
                line += issue.ResolveDate + "|";
                line += issue.EntryPerson + "|";
                line += issue.Station + "|";
                if (issue.Broken)
                    line += "B|";
                if (issue.Enhancement)
                    line += "E|";
                line += issue.Description;
                writer.WriteLine(line);
            }
            writer.Close();

            return true;
        }

        private void Notify_Issues()
        {
            // Send Alert to all Active stations
            // message = "Issues File has changed!"
            Send_Alert_All_Active("Issues File has changed!" + Environment.NewLine);
        }

        private void Notify_Issues(string exceptStation)
        {
            // Send Alert to all Active stations
            // message = "Issues File has changed!"
            Send_Alert_All_Active("Issues File has changed!" + Environment.NewLine, exceptStation);
        }

        private bool Add_AS_Issue_to_File(string EntryPerson, string Station, string Type, string Description) // Type = "B" or "E"
        {
            string FileName = ASIssuesFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                // verify the file is good by trying to open it
                try
                {
                    //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    //                    fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                    fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            else
            {       // new file - tell the user the file does not exist
                MessageBox.Show("Selected file:\n\n" + FileName + "\n\ndoes not exist!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // append this one item to the Issues List file
            string line = DateTime.Now.ToString("MM/dd/yy") + "||";      // no Resolve Date entered
            //            string line = Environment.NewLine;
            //            line += DateTime.Now.ToString("MM/dd/yy") + "||";      // no Resolve Date entered
            line += EntryPerson + "|";
//            line += "DB" + "|";
            line += Station + "|";
            line += Type + "|";
            line += Description;
            writer.WriteLine(line);
            writer.Close();

            return true;
        }

        private bool Send_AS_Issues_before_CLosing()
        {
            return true;
        }

        private void btn_Load_AS_Issues_Click(object sender, EventArgs e)
        {
            //btn_Get_Issues.Text = "Downloading";
            //WorkerObject.Download = btn_Get_Issues;
            ////            tb_Downloaded_Station_Info_File.Visible = true;
            ////            btn_Browse_Downloaded_Station_Info_File.Visible = true;
            //SendCommand(Commands.RequestIssues, "");

            Load_AS_Issues(ASIssuesFilePath, false);
        }

        private void AddNewASIssueFromAS(Queue_Issue aidStationIssue)
        {
            // add to the list
            Aid_Issue issue = new Aid_Issue();
            issue.EntryDate = aidStationIssue.EntryDate;
            issue.EntryPerson = aidStationIssue.EntryPerson;
            issue.Station = aidStationIssue.Station;
            issue.Description = aidStationIssue.Description;
            //if (rb_AS_Issues_Broken.Checked)
            //    issue.Broken = true;
            //if (rb_AS_Issues_Enhancement.Checked)
            //    issue.Enhancement = true;
            if (aidStationIssue.Type == "B")
                issue.Broken = true;
            else
                issue.Broken = false;
            if (aidStationIssue.Type == "E")
                issue.Enhancement = true;
            else
                issue.Enhancement = false;
            ASIssues.Add(issue);
            Bind_AS_Issues_DGV();

            // add to the local file
            if (ASIssuesLoaded)
            {
                // add just one line
                //string type = string.Empty;
                //if (rb_AS_Issues_Broken.Checked)
                //    type = "B";
                //if (rb_AS_Issues_Enhancement.Checked)
                //    type = "E";
//                Add_AS_Issue_to_File(tb_AS_Issue_Name.Text, type, tb_AS_Issue_Issue.Text);
                Add_AS_Issue_to_File(issue.EntryPerson, issue.Station, aidStationIssue.Type, issue.Description);
            }
            else
            {   // no file loaded on start up - need to create the file
                Save_AS_Issues();
            }

            // notify all other stations that the Issues file has changed
            Notify_Issues(issue.Station);

            // tell the user
            Modeless_MessageBox_Information("New Aid Station Issue has been received!", "New Issue");
            New_AS_Issue = true;
        }

        private void btn_Add_New_AS_Issue_Click(object sender, EventArgs e)
        {
            // has the info already been entered?
            if (!Adding_AS_Issue)
            {
                // clear the tecxtboxes and radio buttons
                tb_AS_Issue_Name.Clear();
                tb_AS_Issue_Issue.Clear();
                rb_AS_Issues_Enhancement.Checked = false;
                rb_AS_Issues_Broken.Checked = false;

                // make the labels and textboxes visible
                panel_AS_Issues.Visible = true;
                btn_Add_New_AS_Issue.Enabled = false;
                btn_AS_Issues_Cancel_Add.Visible = true;
                Application.DoEvents();

                // tell user to enter info and click it again
                MessageBox.Show("Enter name and issue info,\n\nThen click the Add button again.", "Click again", MessageBoxButtons.OK, MessageBoxIcon.Hand);

                // change the flag
                Adding_AS_Issue = true;
            }
            else
            {
                // test if the two textboxes have data in them.
                if ((tb_AS_Issue_Issue.Text == "") || (tb_AS_Issue_Name.Text == ""))
                {
                    // tell user to enter info and click it again
                    MessageBox.Show("Enter name and issue info,\n\nThen click the Add button again.", "Click again", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    // add to the list
                    Aid_Issue issue = new Aid_Issue();
                    issue.EntryPerson = tb_AS_Issue_Name.Text;
                    issue.Description = tb_AS_Issue_Issue.Text;
                    if (rb_AS_Issues_Broken.Checked)
                        issue.Broken = true;
                    if (rb_AS_Issues_Enhancement.Checked)
                        issue.Enhancement = true;
                    //                    issue.EntryDate = DateTime.Now.ToShortDateString("MM/dd/yy");
                    issue.EntryDate = DateTime.Now.ToString("MM/dd/yy");
                    ASIssues.Add(issue);
                    Bind_AS_Issues_DGV();

                    // add to the local file
                    if (ASIssuesLoaded)
                    {
                        // add just one line
                        string type = string.Empty;
                        if (rb_AS_Issues_Broken.Checked)
                            type = "B";
                        if (rb_AS_Issues_Enhancement.Checked)
                            type = "E";
                        Add_AS_Issue_to_File(tb_AS_Issue_Name.Text, "DB", type, tb_AS_Issue_Issue.Text);
                    }
                    else
                    {   // no file loaded on start up - need to create the file
                        Save_AS_Issues();
                    }

                    // alert all stations that the Issues file has been changed
                    Notify_Issues();

                    // make the labels and textboxes invisible
                    panel_AS_Issues.Visible = false;
                    btn_AS_Issues_Cancel_Add.Visible = false;
                    Application.DoEvents();

                    // change the flag
                    Adding_AS_Issue = false;
                }
            }
        }

        private void btn_AS_Issues_Cancel_Add_Click(object sender, EventArgs e)
        {
            panel_AS_Issues.Visible = false;
            btn_AS_Issues_Cancel_Add.Visible = false;
            btn_Add_New_AS_Issue.Enabled = true;
            Adding_AS_Issue = false;
        }

        private void rb_AS_Issues_Broken_CheckedChanged(object sender, EventArgs e)
        {
            Test_AS_Add_Issue();
        }

        private void rb_AS_Issues_Enhancement_CheckedChanged(object sender, EventArgs e)
        {
            Test_AS_Add_Issue();
        }

        private void tb_AS_Issue_Name_TextChanged(object sender, EventArgs e)
        {
            if (tb_AS_Issue_Name.Text == "")
                tb_AS_Issue_Name.BackColor = Color.FromArgb(255, 224, 192);
            else
                tb_AS_Issue_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
            Test_AS_Add_Issue();
        }

        private void tb_AS_Issue_Issue_TextChanged(object sender, EventArgs e)
        {
            if (tb_AS_Issue_Issue.Text == "")
                tb_AS_Issue_Issue.BackColor = Color.FromArgb(255, 224, 192);
            else
                tb_AS_Issue_Issue.BackColor = Color.FromKnownColor(KnownColor.Window);
            Test_AS_Add_Issue();
        }

        private void Test_AS_Add_Issue()
        {
            if ((tb_AS_Issue_Issue.Text == "") || (tb_AS_Issue_Name.Text == "") || ((!rb_AS_Issues_Enhancement.Checked) && (!rb_AS_Issues_Broken.Checked)))
                btn_Add_New_AS_Issue.Enabled = false;
            else
                btn_Add_New_AS_Issue.Enabled = true;
        }

        private void Bind_AS_Issues_DGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_AS_Issues.InvokeRequired)
            {
                BindStationDGVDel d = new BindStationDGVDel(Bind_AS_Issues_DGV);
                dgv_AS_Issues.Invoke(d, new object[] { });
            }
            else
            {
                dgv_AS_Issues.DataSource = null;
                if (ASIssues.Count != 0)
                {
                    dgv_AS_Issues.DataSource = ASIssues;
                    dgv_AS_Issues.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[0].Width = 55;     // Entry Date
                    dgv_AS_Issues.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgv_AS_Issues.Columns[0].HeaderText = "Entry Date";
                    dgv_AS_Issues.Columns[1].Width = 55;     // Resolve Date
                    dgv_AS_Issues.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                    dgv_AS_Issues.Columns[1].HeaderText = "Resolve Date";
                    dgv_AS_Issues.Columns[2].Width = 98;    // Entry Person
                    dgv_AS_Issues.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                    dgv_AS_Issues.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dgv_AS_Issues.Columns[2].HeaderText = "Entry Person";
                    dgv_AS_Issues.Columns[3].Width = Station_DGV_Width;     // Station
                    dgv_AS_Issues.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                    dgv_AS_Issues.Columns[3].HeaderText = "Station";
                    dgv_AS_Issues.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[4].Width = 43;     // Broken type
                    dgv_AS_Issues.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                    dgv_AS_Issues.Columns[4].HeaderText = "Broken";
                    dgv_AS_Issues.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[5].Width = 73;     // Enhancement type
                    dgv_AS_Issues.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                    dgv_AS_Issues.Columns[5].HeaderText = "Enhancement";
                    dgv_AS_Issues.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Description
                    dgv_AS_Issues.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_AS_Issues.Columns[6].HeaderText = "Description";
                    dgv_AS_Issues.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }
                dgv_AS_Issues.Update();
            }
        }

        private void btn_Save_AidIssue_Changes_Click(object sender, EventArgs e)
        {

        }

        private void btn_Edit_AidIssues_Click(object sender, EventArgs e)
        {

        }

        private void tabPage_AidStation_Issues_Enter(object sender, EventArgs e)
        {
            New_AS_Issue = false;
        }
        #endregion
        #endregion
        #endregion


        #region Aid Station Functions
        private void Labels_TabPages_Aid_Connections()
        {
            tabControl_Aid_Settings.TabPages.Clear();
            if (Connection_Type == Connect_Medium.APRS)
            {
                lbl_Medium_APRS.Visible = true;
                rb_Use_APRS.Checked = true;

                // add the tabpages to the tabcontrols
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_Settings_AGWPE);
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_Settings_APRS);
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_AGWPE_Statistics);
                tabControl_Main_Aid.TabPages.Remove(tabPage_APRSpackets);
                tabControl_Main_Aid.TabPages.Remove(tabPage_PacketNodePackets);
                tabControl_Main_Aid.TabPages.Remove(tabPage_EthernetPackets);
                tabControl_Main_Aid.TabPages.Add(tabPage_APRSpackets);

                // start the AGWPE
                AGW_Count = 5;      // wait 5 seconds after AGWPE finishes Initting to get the settings
// 7/21/16                ChangeState(Server_State.Attempting_Connect);
// 5/20/17                Aid_AGWSocket.InitAGWPE(true);
            }
            else
                lbl_Medium_APRS.Visible = false;

            if (Connection_Type == Connect_Medium.Packet)
            {
                lbl_Medium_Packet.Visible = true;
                rb_Use_Packet.Checked = true;

                // add the tabpages to the tabcontrols
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_Settings_AGWPE);
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_Settings_Packet);
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_AGWPE_Statistics);
                tabControl_Main_Aid.TabPages.Remove(tabPage_APRSpackets);
                tabControl_Main_Aid.TabPages.Remove(tabPage_PacketNodePackets);
                tabControl_Main_Aid.TabPages.Remove(tabPage_EthernetPackets);
                tabControl_Main_Aid.TabPages.Add(tabPage_PacketNodePackets);

                // start the AGWPE
                AGW_Count = 5;      // wait 5 seconds after AGWPE finishes Initting to get the settings
// 7/21/16                ChangeState(Server_State.Attempting_Connect);
                Aid_AGWSocket.InitAGWPE(true);      // 5/20/17 - may need to remove this later
            }
            else
                lbl_Medium_Packet.Visible = false;

            if (Connection_Type == Connect_Medium.Ethernet)
            {
                lbl_Medium_Ethernet.Visible = true;
                rb_Use_Ethernet.Checked = true;

                // add the tabpages to the tabcontrols
                tabControl_Aid_Settings.TabPages.Add(tabPage_Aid_Settings_Ethernet);
                tabControl_Main_Aid.TabPages.Remove(tabPage_PacketNodePackets);
                tabControl_Main_Aid.TabPages.Remove(tabPage_APRSpackets);
                tabControl_Main_Aid.TabPages.Remove(tabPage_EthernetPackets);
                tabControl_Main_Aid.TabPages.Add(tabPage_EthernetPackets);

                // attempt to connect to the Server
                if ((tb_Aid_Mesh_IP_address.Text != "") && (tb_Server_Port_Number.Text != ""))
                    IP_Server_Connect();
            }
            else
                lbl_Medium_Ethernet.Visible = false;

            if (Connection_Type == Connect_Medium.Cellphone)
            {
                lbl_Medium_Cellphone.Visible = true;
                rb_Use_Cellphone.Checked = true;
            }
            else
                lbl_Medium_Cellphone.Visible = false;
        }

        #region Tab Header coloring
        private Dictionary<TabPage, Color> TabColors = new Dictionary<TabPage, Color>();
        private void SetTabHeader_Aid(TabPage page, Color color)
        {
            TabColors[page] = color;
            tabControl_Main_Aid.Invalidate();
        }

        /*      code found on Internet at: http://stackoverflow.com/questions/5338587/set-tabpage-header-color
         *          must set the tabControl DrawMode to OwnerDrawFixed, not the default of Normal
         */
        private void tabControl_Main_Aid_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color TabColor;
            Brush TextBrush;
            // determine the Tab color
            if (e.State == System.Windows.Forms.DrawItemState.Selected)
                TabColor = Color.White;
            else
                TabColor = TabColors[tabControl_Main_Aid.TabPages[e.Index]];
            // determine the Text color: Black if no error, White if not selected, Red if selected
            if (TabColors[tabControl_Main_Aid.TabPages[e.Index]] == Color.Red)  // this indicates errors have occurred
            {       // need to know if selected
                if (e.State == System.Windows.Forms.DrawItemState.Selected)
                    TextBrush = Brushes.Red;
                else
                    TextBrush = Brushes.White;
            }
            else
            {       // no errors, set it Black
                TextBrush = Brushes.Black;
            }
            using (Brush br = new SolidBrush(TabColor))
            {
                Rectangle rect = e.Bounds;
                rect.Height -= 1;
                e.Graphics.FillRectangle(br, rect);
                SizeF sz = e.Graphics.MeasureString(tabControl_Main_Aid.TabPages[e.Index].Text, e.Font);
                e.Graphics.DrawString(tabControl_Main_Aid.TabPages[e.Index].Text, e.Font, TextBrush, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);
            }
        }

        public void Aid_DictTimeEvent(object source, ElapsedEventArgs e)
        {       // this is an ISR that happens every .5 sec, so it needs to be fast
            // test Stations tab to see if we need to load or create a Station list
            if (Aid_Stations.Count == 0)
            {
                if (StationC != Color.Red)
                {
                    SetTabHeader_Aid(tabPage_Aid_Stations, Color.Red);
                    StationC = Color.Red;
                }
            }
            else
            {
                if (StationC != Color.FromKnownColor(KnownColor.Control))
                {
                    SetTabHeader_Aid(tabPage_Aid_Stations, Color.FromKnownColor(KnownColor.Control));
                    StationC = Color.FromKnownColor(KnownColor.Control);
                }
            }

            // test Settings tab to see if Ethernet IP address and Port number have been entered
//            if ((tb_Aid_Mesh_IP_address.Text != "") && (tb_Server_Port_Number.Text != ""))
//            if (TestTextbox(tb_Aid_Mesh_IP_address) && TestTextbox(tb_Aid_Server_Port_Number))
            switch (Connection_Type)
            {
                case Connect_Medium.Ethernet:
                    if (Aid_IP_Good && Aid_Server_Port_Good)
                    {
                        if (SettingsC != Color.FromKnownColor(KnownColor.Control))
                        {
                            SetTabHeader_Aid(tabPage_Aid_Settings, Color.FromKnownColor(KnownColor.Control));
                            SettingsC = Color.FromKnownColor(KnownColor.Control);
                        }
                    }
                    else
                    {
                        if (SettingsC != Color.Red)
                        {
                            SetTabHeader_Aid(tabPage_Aid_Settings, Color.Red);
                            SettingsC = Color.Red;
                        }
                    }
                    break;
                case Connect_Medium.Packet:
                    if ((Packet_Connect_Mode == Packet_Connect_Method.ViaString) && ((VIAstring == null) || (VIAstring == "")) ||
                        ((tb_AidStation_FCC_Callsign.Text == "") || (tb_Database_FCC_Callsign.Text == "")))
                    {
                        if (SettingsC != Color.Red)
                        {
                            SetTabHeader_Aid(tabPage_Aid_Settings, Color.Red);
                            SettingsC = Color.Red;
                        }
                    }
                    else
                    {
                        if (SettingsC != Color.FromKnownColor(KnownColor.Control))
                        {
                            SetTabHeader_Aid(tabPage_Aid_Settings, Color.FromKnownColor(KnownColor.Control));
                            SettingsC = Color.FromKnownColor(KnownColor.Control);
                        }
                    }
                    break;
            }
        }

        //private void ChangeTab_Aid(int tab)
        //{
        //    if (tabControl_Main_Aid.InvokeRequired)
        //        tabControl_Main_Aid.BeginInvoke(new ChangeTabCallback(ChangeTab_Aid), tab);
        //    else
        //    {
        //        tabControl_Main_Aid.SelectedIndex = 1;
        //        tabControl_Main_Aid.Update();
        //    }
        //}
        #endregion

        #region Message testing
        private void CreateOutMess()
        {
            CreateMess("Output");
        }

        private bool CreateMess(string str)
        {
            // verify that the Messages folder has been created
            // verify that the requested sub-folder has been created
            // create the desired file
            StreamWriter writer = StreamWriter.Null;
            //            FileStream fs;
            //            FileInfo fi = new FileInfo(FileName);
            DirectoryInfo di = new DirectoryInfo("path");

            // determine if this is a new file or existing
            //            if (fi.Exists)
            {       // existing file - ask if overwrite
                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                    //                                      FileName +
                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                    fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        //                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        //                  MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                else
                    return false;   // quit, do not overwrite existing file
            }
            //            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    //            fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    //                    writer = new StreamWriter(fs);
                }
                catch
                {
                    //          MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save each station
            foreach (DB_Station station in Stations)
            {
                // Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station, Next station,
                // distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)
                string line = station.Name + ",";
                line += station.Number + ",";
                line += station.Latitude.ToString() + ",";
                line += station.Longitude.ToString() + ",";
                line += station.Previous + ",";
                line += station.DistPrev.ToString() + ",";
                line += station.Next + ",";
                line += station.DistNext.ToString() + ",";
                line += station.Difficulty.ToString() + ",";
                if (station.Accessible)
                    line += "Yes";
                else
                    line += "No";
                writer.WriteLine(line);
            }
            writer.Close();
            return true;
        }

        private void CreateInMess()
        {
            CreateMess("Input");
        }
        #endregion

        private void IP_Server_Connect()
        {
// 7/21/16            if (!Connected_to_Server && (state != Server_State.Not_Initted))   // test if we are already connected or not yet Initted
// 7/22/16            if (!Connected_to_Server)   // test if we are already connected
            if (!WorkerObject.Connected_to_DB && !WorkerObject.Connected_and_Active && !WorkerObject.Attempting_to_Connect_to_Server)   // test if we are already connected
            {
                // verify that conditions are good to start a connection
                if ((tb_Aid_Mesh_IP_address.Text == "") || (tb_Aid_Server_Port_Number.Text == ""))
                {
                    // change to Cannot Connect state
// 7/21/16                    ChangeState(Server_State.Cannot_Connect);
                }
                else
                {
                    // change the state
// 7/21/16                    ChangeState(Server_State.Attempting_Connect);

                    // clear the Welcome message textboxes
                    SetTBtext(tb_Welcome_Message_received, "");

//// 7/21/16                    if ((WorkerConnectSendThread != null) && (!WorkerConnectSendThread.IsAlive))   // test if the thread has already been started
//                    {       // not already started
//                        //    //                    lbl_Connected_Central_Database.Text = "Connected to AGWPE";
//                        //WorkerObject.Server_Connected = lbl_Connected_Central_Database;
//                        //WorkerObject.Server_Connected_Active = lbl_Connected_Active;
//                        //WorkerObject.Cannot_Connect = lbl_Cannot_Connect;
//                        //WorkerObject.Error_Connecting = lbl_Error_Connecting;
//                        //WorkerObject.Server_IP_Address = tb_Aid_Mesh_IP_address.Text;
//                        //if (tb_Aid_Server_Port_Number.Text == "")
//                        //    WorkerObject.Server_Port_Number = 0;
//                        //else
//                        //    //                        WorkerObject.Server_Port_Number = Convert.ToInt16(tb_Server_Port_Number.Text);
//                        //    WorkerObject.Server_Port_Number = Convert.ToInt16(tb_Aid_Server_Port_Number.Text);
//                        //WorkerObject.Server_Error_Message = tb_Aid_Server_Error_Message;
//                        //WorkerObject.Connect_Button = btn_Connect_to_Mesh_Server;
//                        //WorkerObject.Server_Attempting_Connection = lbl_Attempting_Connection;
//                        //WorkerObject.Connected_to_Server = Connected_to_Server;
//                        //WorkerObject.Station_Name = tb_Station_Name.Text;
//                        //WorkerObject.Ethernet_Packets = rtb_Aid_Ethernet_Packets;
//                        Prep_WorkerObject();
//                        WorkerConnectSendThread.Start();
//                        Console.WriteLine("Starting Station Worker Connect/Send thread...");
//                        WorkerReceiveThread.Start();
//                        Console.WriteLine("Starting Station Worker Receive thread...");
//                    }
//                    else
//                    {       // WorkerThread already started
                        try
                        {
                            // make any changes
                            WorkerObject.Server_IP_Address = tb_Aid_Mesh_IP_address.Text;
                            if (tb_Aid_Server_Port_Number.Text == "")
                                WorkerObject.Server_Port_Number = 0;
                            else
                                //                        WorkerObject.Server_Port_Number = Convert.ToInt16(tb_Server_Port_Number.Text);
                                WorkerObject.Server_Port_Number = Convert.ToInt16(tb_Aid_Server_Port_Number.Text);
                            WorkerObject.Station_Name = tb_Station_Name.Text;

                            // thread already started, give the command to try to re-connect
                            WorkerObject.RequestConnect();
                        }
                        catch
                        {
                            MessageBox.Show("Server Port # is Invalid", "Server Port # Invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
// 7/21/16                    }
                    return;

                    //#region original testing method
                    //const int PORT_NO = 14001;
                    //const string SERVER_IP = "127.0.0.1";
                    //try
                    //{
                    //    //---data to send to the server---
                    //    string textToSend = DateTime.Now.ToString();

                    //    //---create a TCPClient object at the IP and port no.---
                    //    //                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                    //    TcpClient client = new TcpClient(tb_Aid_Mesh_IP_address.Text, Convert.ToInt16(tb_Server_Port_Number.Text));

                    //    // if get to this point, then connected to server
                    //    Connected_to_Central_Database = true;

                    //    // prepare to send data
                    //    NetworkStream nwStream = client.GetStream();
                    //    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                    //    //---send the text---
                    //    Console.WriteLine("Sending : " + textToSend);
                    //    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    //    //---read back the text---
                    //    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    //    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    //    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                    //    Console.ReadLine();
                    //    client.Close();

                    //    // here is another version:
                    //    TcpClient tcpclnt = new TcpClient();
                    //    Console.WriteLine("Connecting.....");

                    //    //                tcpclnt.Connect("172.21.5.99", 8001);
                    //    tcpclnt.Connect(SERVER_IP, PORT_NO);

                    //    Console.WriteLine("Connected");
                    //    Console.Write("Enter the string to be transmitted : ");

                    //    String str = Console.ReadLine();
                    //    Stream stm = tcpclnt.GetStream();

                    //    ASCIIEncoding asen = new ASCIIEncoding();
                    //    byte[] ba = asen.GetBytes(str);
                    //    Console.WriteLine("Transmitting.....");

                    //    stm.Write(ba, 0, ba.Length);

                    //    byte[] bb = new byte[100];
                    //    int k = stm.Read(bb, 0, 100);

                    //    for (int i = 0; i < k; i++)
                    //        Console.Write(Convert.ToChar(bb[i]));

                    //    tcpclnt.Close();

                    //}
                    //catch (Exception e)
                    //{
                    //    Console.WriteLine("Error..... " + e.Message);
                    //    lbl_Error_Connecting.Visible = true;
                    //    tb_Server_Error_Message.Text = "Error: " + e.Message;
                    //}
                    //#endregion
                }
            }
        }

        public void Send_LogPts()
        {
            SendCommand(Commands.LogPoints, NumLogPts.ToString());
        }

        bool SendCommand(Commands command, string data)     // returns false if not Connected to Server
        {
            string dataout = "";
            Aid_Worker.Expecting_State expecting = Aid_Worker.Expecting_State.Nothing;
            if (!WorkerObject.Connected_to_DB)
            {
                return false;
            }
                switch (command)
                {
                    case Commands.SendDNFRunner:
                        dataout = "DNF Runner:" + data;
                        break;
                    case Commands.SendWatchRunner:
                        dataout = "Watch Runner:" + data;
                        break;
                    case Commands.SendIssue:
                        dataout = "Aid Station Issue:" + data;
                        break;
                    case Commands.LogPoints:
                        dataout = "Log Points:" + data;
                        break;
                    case Commands.RunnerIn:
                        dataout = "Runner In:" + data;
                        break;
                    case Commands.RunnerOut:
                        dataout = "Runner Out:" + data;
                        break;
                    case Commands.Message:
                        dataout = "Message:" + data;
                        break;
                    case Commands.RequestRunner:
                        dataout = "Request Runner:" + data;
                        expecting = Aid_Worker.Expecting_State.Request_Runner;
                        RunnersStatus.Clear();
                        break;
                    case Commands.RequestStartTime:
                        dataout = "Start?:";
                        break;
                    case Commands.RequestRFIDAssignments:
                        dataout = "Request RFID Assignments:";
                        break;
                    case Commands.RequestStationInfo:
                        dataout = "Request Station Info:";
                        expecting = Aid_Worker.Expecting_State.Station_Info_File;
                        break;
                    case Commands.RequestRunnerList:
                        dataout = "Request Runner List:";
                        expecting = Aid_Worker.Expecting_State.Runner_List;
                        break;
                    case Commands.RequestBibList:       // 8/10/17
                        dataout = "Request Bib List:";       // 8/10/17
                        expecting = Aid_Worker.Expecting_State.Bib_List;       // 8/10/17
                        break;       // 8/10/17
                    case Commands.RequestDNSlist:
                        dataout = "Request DNS List:";
                        expecting = Aid_Worker.Expecting_State.DNS_List;
                        break;
                    case Commands.RequestDNFlist:
                        dataout = "Request DNF List:";
                        expecting = Aid_Worker.Expecting_State.DNF_List;
                        break;
                    case Commands.RequestWatchlist:
                        dataout = "Request Watch List:";
                        expecting = Aid_Worker.Expecting_State.Watch_List;
                        break;
                    case Commands.RequestIssues:
                        dataout = "Request Issues:";
                        expecting = Aid_Worker.Expecting_State.Issues;
                        break;
                    case Commands.RequestInfo:
                        dataout = "Request Info File:";
                        expecting = Aid_Worker.Expecting_State.Info_File;
                        break;
                    default:
                        break;
                }
                Command newcommand = new Command();
                if (dataout.EndsWith(Environment.NewLine))    // add EOL if it does not exist
                    newcommand.Data = dataout;
                else
                    newcommand.Data = dataout + Environment.NewLine;
                newcommand.Expecting = expecting;
                lock (CommandQue)
                {// lock
                    CommandQue.Enqueue(newcommand);
                }// unlock
                Console.WriteLine("Sent this request to Central: " + dataout);
                return true;
//            }
//            return false;
        }

        #region Station Name Textbox
        private void tb_Station_Name_TextChanged(object sender, EventArgs e)
        {
            if ((tb_Station_Name.Text == "") || (tb_Station_Name.Text == "Not yet identified"))
                tb_Station_Name.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Station_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Station_Name_Leave(object sender, EventArgs e)
        {
            if (tb_Station_Name.Text == "")
                lbl_Station_Name_notin_Station_List.Visible = false;    // also turn off this label
            else
            {
                //// also check if this name appears in the Station List
                //if (Aid_Stations.Count != 0)
                //{
                //    int index = Aid_Stations.FindIndex(
                //        delegate (Aid_Station station)
                //        {
                //            return station.Name == tb_Station_Name.Text;
                //        });
                //    if (index == -1)
                //        lbl_Station_Name_notin_Station_List.Visible = true;
                //    else
                //        lbl_Station_Name_notin_Station_List.Visible = false;
                //}
                Test_Station_Name();
            }
            tb_Station_Name_Settings.Text = tb_Station_Name.Text;
            Save_Registry("Station Name", tb_Station_Name.Text);
        }

        private void tb_Station_Name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tb_Station_Name_Leave(null, null);
            }
        }

        void Test_Station_Name()    // changed 7/21/17 - added test if Start
        {
            // check if this name appears in the Station List
            if (Aid_Stations.Count != 0)
            {
                int index = Aid_Stations.FindIndex(
                    delegate (Aid_Station station)
                    {
                        return station.Name == tb_Station_Name.Text;
                    });
                if (index == -1)
                    lbl_Station_Name_notin_Station_List.Visible = true;
                else
                {
                    lbl_Station_Name_notin_Station_List.Visible = false;

                    // now test if it is the Start Aid Station, so can change Enter Runners tab - added 7/21/17
                    if (Station_Name == "Start")
                    {
                        panel_Most_Aid_Runners_In.Visible = false;
                        panel_Runners_Out.Visible = true;
                        tb_Add_Runner_Out.Visible = false;
                    }
                }
            }
            else
                lbl_Station_Name_notin_Station_List.Visible = true;
        }
        #endregion

        private void tb_Start_Time_DoubleClick(object sender, EventArgs e)
        {
            // send request to update the Start Time
            SendCommand(Commands.RequestStartTime, "");
        }

        #region Timers
        void Elapsed30secHandler(object source, ElapsedEventArgs e) // current time, Runner time at Station, connecting
        {
            // this event happens every 30 seconds.
            // its main purpose is to update the time of a runner at a station
            // and to remove the runner from the RunnerAtStation list 2 minutes after he has left the station
            //
            // It will also be used to: 1. look for a change in the Connected_to_Server flag
            //                          2. attempt to reconect to the Server if the Auto Reconnect checkbox is checked
            //                          3. APRS ??
            //

            // Current Time
// 7/20/16 - moved below            DateTime Now = DateTime.Now;
// 2/14/16            SetTime(Now.ToString("HH:mm"));

            // state will not change while 'Attempting to Connect'
//// 7/21/16            if (!WorkerObject.Attempting_to_Connect_to_Server)
//            {
//// 7/21/16                // test the worker thread connected state
////                if (Connected_to_Server != WorkerObject.Connected_to_Server)    // are they the same (no change) ?
//                if (Connected_to_Server != WorkerObject.Connected_to_DB)    // are they the same (no change) ?
//                {       // they are different, need to change
////                    if (Connected_to_Server = WorkerObject.Connected_to_Server)
//                    if (Connected_to_Server = WorkerObject.Connected_to_DB)
//                        ChangeState(Server_State.Connected);
//                    else
//                        ChangeState(Server_State.Error_Connecting);

//                    // change the flag
//                    Attempting_to_Connect_to_Server = false;
//                }

//                // test if failed to connect after a previous failure
//                if (Attempting_to_Connect_to_Server && (state == Server_State.Error_Connecting))
//                    Attempting_to_Connect_to_Server = false;

                // test if we should try to reconnect
//                if (!Connected_to_Server && Server_Mesh_Auto_Reconnect && !Attempting_to_Connect_to_Server)
                if (((WorkerObject.state == Aid_Worker.Server_State.Error_Connecting) || (WorkerObject.state == Aid_Worker.Server_State.Lost_Connection)) && Server_Mesh_Auto_Reconnect)
                    IP_Server_Connect();
                // 7/21/16            }

            #region Look at RunnersAtStation list
            // look at the RunnersAtStation list
            if (RunnersAtStation.Count != 0)
            {
                DateTime Now = DateTime.Now;
                bool ListChanged = false;
                for (int i = RunnersAtStation.Count - 1; i >= 0; i--)
                {
                    Aid_Runner runner = RunnersAtStation[i];

                    // is runner still at the station?
                    if (runner.TimeOut.Year == 1)
                    {       // runner still at station - calculate minutes at station
                        TimeSpan ts = Now - runner.TimeIn;
                        runner.Minutes = (uint)(ts.Minutes + ts.Hours * 60);
                        ListChanged = true;

                        // is there only a Runner In entry point?
                        if (One_Reader_Only)
                            if (ts.Minutes >= Remove_from_Runners_list_Minutes)
                            {
                                RunnersAtStation.Remove(runner);    // remove the runner from the list if he has been on it too long
                                ListChanged = true;
                            }
                    }
                    else
                    {       // runner has left - calculate minutes since leaving
                        TimeSpan ts = Now - runner.TimeOut;
                        //                    if (ts.Minutes > 2)
                        if (ts.Minutes > Remove_from_Runners_list_Minutes)
                        {
                            RunnersAtStation.Remove(runner);    // remove the runner from the list if he has been gone too long
                            ListChanged = true;
                        }
                    }
                }
                if (ListChanged)
                    Bind_RunnersAtStation();      // update the Runners At Station list display
            }
            #endregion
        }

        void Elapsed5secHandler(object source, ElapsedEventArgs e)  // incoming messages, Start time, File downloads
        {
            // this event happens every 5 seconds.
            //
            // the purpose for this timer event is to check for incoming messages & alerts,
            //      a change in the Start Time, and File downloads completing or needing to start
            //
            if (Incoming_Mess)
            {
                Incoming_Mess = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Add_In_MessageThread));
            }
            if (Incoming_Alrt)
            {
                Incoming_Alrt = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Add_AlertThread));
            }
            if (Start_Time_Rcvd)
            {
                Start_Time_Rcvd = false;
                SetTBtext(tb_Start_Time, Start_Time);
            }
            if (WorkerObject.Stations_Download_Complete)
            {
                WorkerObject.Stations_Download_Complete = false;
                MakeVisible(btn_Save_Downloaded_Station_File, true);
            }
            if (WorkerObject.Runners_Download_Request_Complete)
            {
                WorkerObject.Runners_Download_Request_Complete = false;
                // 8/3/16                ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_Runners));
                // changed 8/3/16
                if (WorkerObject.Runners_Not_Available)
                    MakeVisible(lbl_Aid_Runner_List_Not_available, true);
                else
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_Runners));
            }
            if (WorkerObject.DNS_Download_Request_Complete)
            {
                WorkerObject.DNS_Download_Request_Complete = false;
                // 8/3/16                ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_DNS));
                // changed 8/3/16
                if (WorkerObject.DNS_Not_Available)
                    MakeVisible(lbl_DNS_List_Not_available, true);
                else
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_DNS));
            }
            if (WorkerObject.DNF_Download_Request_Complete)
            {
                WorkerObject.DNF_Download_Request_Complete = false;
                // 8/3/16                ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_DNF));
                // changed 8/3/16
                if (WorkerObject.DNF_Not_Available)
                    MakeVisible(lbl_DNF_List_Not_available, true);
                else
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_DNF));
            }
            if (WorkerObject.Watch_Download_Request_Complete)
            {
                WorkerObject.Watch_Download_Request_Complete = false;
                // 8/3/16                ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_Watch));
                // changed 8/3/16
                if (WorkerObject.Watch_Not_Available)
                    MakeVisible(lbl_Aid_Watch_List_Not_available, true);
                else
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_Watch));
            }
            if (WorkerObject.Info_Download_Request_Complete)
            {
                WorkerObject.Info_Download_Request_Complete = false;
                // 8/3/16                ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_Info));
                // changed 8/3/16
                if (WorkerObject.Info_Not_Available)
                    MakeVisible(lbl_Info_Not_available, true);
                else
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Aid_Info));
            }
            if (WorkerObject.Issues_Download_Complete)
            {
                WorkerObject.Issues_Download_Complete = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Load_Issues));
            }
            switch (Current_InitAction)
            {
                case InitActions.LogPoints:
                    if (NumLogPts != 1)
                        SendCommand(Commands.LogPoints, NumLogPts.ToString());
                    if (Connection_Type == Connect_Medium.Ethernet)
                        Current_InitAction = InitActions.Runner;        // load all other things only if using Ethernet
                    else
                        Current_InitAction = InitActions.Done;      // otherwise, we are done
                    break;
                case InitActions.Runner:
                    if (!RunnerList_loading)
                    {
                        RunnerList_loading = true;
                        btn_Refresh_Aid_Runner_List_Click(null, null);
                    }
                    else
                    {
                        if (WorkerObject.Runners_Not_Available)
                            Current_InitAction = InitActions.DNS;   // change to request the DNS List
                    }
                    break;
                case InitActions.DNS:
                    if (!DNS_loading)
                    {
                        DNS_loading = true;
                        btn_DNS_Download_Click(null, null);
                    }
                    else
                    {
                        if (WorkerObject.DNS_Not_Available)
                            Current_InitAction = InitActions.DNF;   // change to request the DNF List
                    }
                    break;
                case InitActions.DNF:
                    if (!DNF_loading)
                    {
                        DNF_loading = true;
                        btn_Aid_DNF_Download_Click(null, null);
                    }
                    else
                    {
                        if (WorkerObject.DNF_Not_Available)
                            Current_InitAction = InitActions.Watch;     // change to request Watch List
                    }
                    break;
                case InitActions.Watch:
                    if (!Watch_loading)
                    {
                        Watch_loading = true;
                        btn_Watch_Download_Click(null, null);
                    }
                    else
                    {
                        if (WorkerObject.Watch_Not_Available)
                            Current_InitAction = InitActions.Info;      // change to request Info File
                    }
                    break;
                case InitActions.Info:
                    if (!Info_loading)
                    {
                        Info_loading = true;
                        btn_Download_Info_Click(null, null);
                    }
                    else
                    {
                        if (WorkerObject.Info_Not_Available)
                            Current_InitAction = InitActions.Done;      // all done with Initialization Actions
                    }                                                   // Issues are not automatically downloaded from DB
                    break;
            }
        }

        bool Prev_Cannot = false;
        void Elapsed_Aid_1secHandler(object source, ElapsedEventArgs e)  // Welcome message, Connected_to_Server, AGW socket - changed name 7/8/19
        {
            // this event happens every second.
            // its main purpose is to look for a change in the Connected_to_Server flag
            //
            // Also looks for the Welcome message from the server in the first 1 minute of 5 minutes after connecting
            // Also test if the AGWSocket is Registered
            //
            // It will also be used to delay 5 seconds after AGWPE finishes Initting to display the settings

            #region Old code
/***** combining the two
            // which method of connecting to the Central Database are we using?
            if (Connection_Type == Connect_Medium.Ethernet)
            {       // Ethernet
                #region Ethernet
                // while the worker thread is Attempting to Connect, there is nothing else to do
                if (!WorkerObject.Attempting_to_Connect_to_Server)
                {
                    // test the worker thread connected state
                    if (WorkerObject.Connected_to_Server)
                    {
                        if (!Connected_to_Server)       // test if just changed
                        {
                            ChangeState(Server_State.Connected);
                            //                            Welcome_Count = 21;     // start the 20 second counter
                            Welcome_Count = 31;     // start the 30 second counter
                        }

                        // test if in Connected_Active state
                        if (WorkerObject.state == Aid_Worker.Server_State.Connected_Active)
                            ChangeState(Server_State.Connected_Active);
                    }
                    else
                    {
                        ChangeState(Server_State.Error);
                        // should be done in Worker                    SetTBtext(tb_Server_Error_Message, WorkerObject.Server_Error_Message);
                    }

                    // test for the Welcome Message coming in
                    if (Welcome_Count != 0)
                    {
                        if ((WorkerObject.Welcome_Message != null) && (WorkerObject.Welcome_Message != ""))
                        {
                            SetTBtext(tb_Welcome_Message_received, WorkerObject.Welcome_Message);
                            SetTBtext(tb_Packet_Welcome_message, WorkerObject.Welcome_Message);
                            Welcome_Count = 0;
                            Current_InitAction = InitActions.LogPoints;     // send the number of Log Points
                        }
                        else
                        {
                            Welcome_Count--;
                            if (Welcome_Count == 0)
                            {
                                MessageBox.Show("Welcome Message from Central Database has not been\n     received in the first 30 seconds after connecting!", "No Welcome Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region APRS or Packet
                if (DBRole)     // is this the Central Database?  - no, will not be Central Database in the Aid Station Functions
                {       // yes - Database
                    if (((Connection_Type == Connect_Medium.APRS) || (Connection_Type == Connect_Medium.Packet)) && (DB_AGWSocket != null))
                    {       // AGWPE
                        // test if the AGWPE socket has finished Initting
                        if (AGW_Count != 0)
                        {
                            if (!DB_AGWSocket.InitInProcess)
                            {
                                AGW_Count--;
                                if (AGW_Count == 0)
                                {
                                    Get_AGWPE_Settings();
// removed 3/24/16                                    btn_Activate_Port_Monitor_Click(null, null);
                                }
                            }
                        }
                        else
                        {
                            // check for any Buttons to Push
                            if (Buttons_to_Push.Count != 0)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Button_PushThread));
                            }

                            // check the connection status
                            if (Prev_Cannot != Aid_AGWSocket.Cannot_Connect_to_DB)
                            {
                                if (!Prev_Cannot)
                                    ChangeState(Server_State.Cannot_Connect);
                                Prev_Cannot = Aid_AGWSocket.Cannot_Connect_to_DB;
                            }

                            // test if there is a new AGWPE Received packet available
                            if (NewAGWPEpacketRcvd)
                            {
                                NewAGWPEpacketRcvd = false;

                                switch (Connection_Type)
                                {
                                    case Connect_Medium.APRS:
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSRcvdThread), NewRcvdPacket);
                                        break;
                                    case Connect_Medium.Packet:
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketNodeThread), NewRcvdPacket);
                                        break;
                                }
                            }

                            // test if there is a new AGWPE Sent packet available
                            if (NewAGWPEpacketSent)
                            {
                                NewAGWPEpacketSent = false;

                                if (Connection_Type == Connect_Medium.APRS)
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSSentThread), NewSentPacket);
                                else
                                    if (Connection_Type == Connect_Medium.Packet)
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketNodeSentThread), NewRcvdPacket);
                            }
                        }
                    }
                }
                else
                {       // no - Aid Station
                    if (((Connection_Type == Connect_Medium.APRS) || (Connection_Type == Connect_Medium.Packet)) && (Aid_AGWSocket != null))
                    {       // AGWPE
                        // test if the AGWPE socket has finished Initting
                        if (AGW_Count != 0)
                        {
                            if (!Aid_AGWSocket.InitInProcess)
                            {
                                AGW_Count--;
                                if (AGW_Count == 0)
                                {
                                    Get_Aid_AGWPE_Settings();
// removed 3/16/24                                    btn_Activate_Port_Monitor_Click(null, null);
                                }
                            }
                        }
                        else
                        {
                            // check for any Buttons to Push
                            if (Buttons_to_Push.Count != 0)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Button_PushThread));
                            }

                            // check the connection status
                            if (Prev_Cannot != Aid_AGWSocket.Cannot_Connect_to_DB)
                            {
                                if (!Prev_Cannot)
                                    ChangeState(Server_State.Cannot_Connect);
                                Prev_Cannot = Aid_AGWSocket.Cannot_Connect_to_DB;
                            }

                            // check if we are connected to the Database
                            if (!cb_Connected_to_Database.Checked)
                            {
                                if (Aid_AGWSocket.Connected_to_Database)
                                {
                                    MakeChecked(cb_Connected_to_Database, true);
                                    MakeVisible(btn_AGWPE_Connect_DB, false);
                                }
                            }

                            // test if there is a new AGWPE Received packet available
                            if (NewAGWPEpacketRcvd)
                            {
                                NewAGWPEpacketRcvd = false;

                                switch (Connection_Type)
                                {
                                    case Connect_Medium.APRS:
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSRcvdThread), NewRcvdPacket);
                                        break;
                                    case Connect_Medium.Packet:
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketNodeThread), NewRcvdPacket);
                                        break;
                                }
                            }

                            // test if there is a new AGWPE Sent packet available
                            if (NewAGWPEpacketSent)
                            {
                                NewAGWPEpacketSent = false;

                                if (Connection_Type == Connect_Medium.APRS)
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSSentThread), NewSentPacket);
                                else
                                    if (Connection_Type == Connect_Medium.Packet)
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketNodeSentThread), NewRcvdPacket);
                            }
                            
                            // test for the Welcome Message coming in
                            if (Welcome_Count != 0)
                            {
                                if ((WorkerObject.Welcome_Message != null) && (WorkerObject.Welcome_Message != ""))
                                {
                                    SetTBtext(tb_Welcome_Message_received, WorkerObject.Welcome_Message);
                                    SetTBtext(tb_Packet_Welcome_message, WorkerObject.Welcome_Message);
                                    Welcome_Count = 0;
                                    Current_InitAction = InitActions.LogPoints;     // send the number of Log Points
                                }
                                else
                                {
                                    Welcome_Count--;
                                    if (Welcome_Count == 0)
                                    {
                                        MessageBox.Show("Welcome Message from Central Database has not been\n     received in the first 30 seconds after connecting!", "No Welcome Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
 ****/
            #endregion

            // check for any Buttons to Push
            if (Buttons_to_Push.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Button_PushThread));
            }

            // while the worker thread is Attempting to Connect, there is nothing else to do
            if (!WorkerObject.Attempting_to_Connect_to_Server)
            {
                // test the worker thread connected state
//                if (WorkerObject.Connected_to_Server)
                if (WorkerObject.Connected_to_DB)
                {
//// 7/21/16                    if (!Connected_to_Server)       // test if just changed
//                    {
//                        ChangeState(Server_State.Connected);
////                        if (Connection_Type != Connect_Medium.Ethernet)
////                            Welcome_Count = 301;    // start a 300 second counter = 5 minutes
////                        else
//////                            Welcome_Count = 31;     // start a 30 second counter
////                            Welcome_Count = 61;     // start a 1 minute counter
//                    }

                    // test if time to turn on the Welcome Count
// 8/18/16                    if ((WorkerObject.Connected_to_DB) && (Welcome_Count == -1))
                    if ((WorkerObject.Connected_to_DB) && (Welcome_Count < 0))    // expecting -1
                    {
                        // should enter this section only once - 4/4/17
//// 7/25/17                        MakeVisible(gb_Messaging, true);    // 4/4/17
//                        MakeVisible(btn_Download_StationFile_from_Central, true);   // 7/17/17
//                        MakeVisible(tb_Downloaded_Station_Info_File, true);            // 7/17/17
//                        MakeVisible(btn_Browse_Downloaded_Station_Info_File, true);    // 7/17/17
//                        MakeVisible(btn_Aid_Start_Runners_Out, true);                   // 7/21/17
                        if (Connection_Type != Connect_Medium.Ethernet)
                            Welcome_Count = 301;    // start a 300 second counter = 5 minutes
                        else
                            Welcome_Count = 61;     // start a 1 minute counter
                    }

// 7/21/16                    // test if in Connected_Active state
//                    if (WorkerObject.state == Aid_Worker.Server_State.Connected_Active)
//                        ChangeState(Server_State.Connected_Active);
                }
//                else
//                {
//// 7/18/16                    ChangeState(Server_State.Error_Connecting);
//                    // should be done in Worker                    SetTBtext(tb_Server_Error_Message, WorkerObject.Server_Error_Message);
//                }

                // test if just entered Connected and Active state - this will happen only once
                if (WorkerObject.Connected_and_Active && !Aid_ConnectedandActive)      // moved things from above 7/22/17
                {
//// 7/25/17                    MakeVisible(gb_Messaging, true);    // 4/4/17
//                    MakeVisible(btn_Download_StationFile_from_Central, true);   // 7/17/17
//                    MakeVisible(tb_Downloaded_Station_Info_File, true);            // 7/17/17
//                    MakeVisible(btn_Browse_Downloaded_Station_Info_File, true);    // 7/17/17
//                    MakeVisible(btn_Aid_Start_Runners_Out, true);                   // 7/21/17
//                    Aid_ConnectedandActive = true;
                    Connected_Active_Visible(true);
                }

                // test if Lost connection while Connected & Active - 7/25/17
                if (Aid_ConnectedandActive && !WorkerObject.Connected_and_Active)
                    Connected_Active_Visible(false);

                // test for the Welcome Message coming in
                if (Welcome_Count != 0)
                {
                    if ((WorkerObject.Welcome_Message != null) && (WorkerObject.Welcome_Message != ""))
                    {
                        SetTBtext(tb_Welcome_Message_received, WorkerObject.Welcome_Message);
                        SetTBtext(tb_Packet_Welcome_message, WorkerObject.Welcome_Message);
                        SetTBtext(tb_APRS_Welcome_message, WorkerObject.Welcome_Message);
                        Welcome_Count = 0;     // stop the count
                        if (Connection_Type == Connect_Medium.Ethernet)
                            Current_InitAction = InitActions.LogPoints;     // send the number of Log Points
                        else
                            Current_InitAction = InitActions.Done;      // Packet quits earlier
                    }
                    else
                    {
                        Welcome_Count--;
                        if (Welcome_Count == 0)
                        {
                            string time;
                            if (Connection_Type != Connect_Medium.Ethernet)
                                time = "5 minutes";
                            else
                                time = "1 minute";
                            MessageBox.Show("Welcome Message from Central Database has not been\n     received in the first " + time + " after connecting!", "No Welcome Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }

                if (((Connection_Type == Connect_Medium.APRS) || (Connection_Type == Connect_Medium.Packet)) && (Aid_AGWSocket != null))
                {       // AGWPE
                    // test if the AGWPE socket has finished Initting
                    if (AGW_Count != 0)
                    {
                        if (!Aid_AGWSocket.InitInProcess)
                        {
                            AGW_Count--;
                            if (AGW_Count == 0)
                            {
                                Get_Aid_AGWPE_Settings();
                                // removed 3/16/24                                    btn_Activate_Port_Monitor_Click(null, null);
                            }
                        }
                    }
                    else
                    {
                        //// check for any Buttons to Push
                        //if (Buttons_to_Push.Count != 0)
                        //{
                        //    ThreadPool.QueueUserWorkItem(new WaitCallback(Button_PushThread));
                        //}

//// 7/21/16 - check this                        // check the connection status
//                        if (Prev_Cannot != Aid_AGWSocket.Cannot_Connect_to_DB)
//                        {
//                            if (!Prev_Cannot)
//                                ChangeState(Server_State.Cannot_Connect);
//                            Prev_Cannot = Aid_AGWSocket.Cannot_Connect_to_DB;
//                        }

                        // check if we are connected to the Database
                        if (!cb_Connected_to_Database.Checked)
                        {
                            if (Aid_AGWSocket.Connected_to_Database)
                            {
                                MakeCBChecked(cb_Connected_to_Database, true);
                                MakeVisible(btn_AGWPE_Connect_DB, false);
                            }
                        }
                        else
                        {
                            if (!Aid_AGWSocket.Connected_to_Database)
                            {
                                MakeCBChecked(cb_Connected_to_Database, false);
                                SetCtlText(btn_AGWPE_Connect_DB, "Connect");
                                MakeVisible(btn_AGWPE_Connect_DB, true);
                            }
                        }

                        // test if there is a new AGWPE Received packet available
                        if (NewAGWPEpacketRcvd)
                        {
                            NewAGWPEpacketRcvd = false;

                            switch (Connection_Type)
                            {
                                case Connect_Medium.APRS:
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSRcvdThread), NewRcvdPacket);
                                    break;
                                case Connect_Medium.Packet:
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketNodeThread), NewRcvdPacket);
                                    break;
                            }
                        }

                        // test if there is a new AGWPE Sent packet available
                        if (NewAGWPEpacketSent)
                        {
                            NewAGWPEpacketSent = false;

                            if (Connection_Type == Connect_Medium.APRS)
                                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAPRSSentThread), NewSentPacket);
                            else
                                if (Connection_Type == Connect_Medium.Packet)
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPacketNodeSentThread), NewRcvdPacket);
                        }

                        //// test for the Welcome Message coming in
                        //if (Welcome_Count != 0)
                        //{
                        //    if ((WorkerObject.Welcome_Message != null) && (WorkerObject.Welcome_Message != ""))
                        //    {
                        //        SetTBtext(tb_Welcome_Message_received, WorkerObject.Welcome_Message);
                        //        SetTBtext(tb_Packet_Welcome_message, WorkerObject.Welcome_Message);
                        //        Welcome_Count = 0;
                        //        Current_InitAction = InitActions.LogPoints;     // send the number of Log Points
                        //    }
                        //    else
                        //    {
                        //        Welcome_Count--;
                        //        if (Welcome_Count == 0)
                        //        {
                        //            MessageBox.Show("Welcome Message from Central Database has not been\n     received in the first 30 seconds after connecting!", "No Welcome Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //        }
                        //    }
                        //}
                    }
                }
            }
        }

        void Connected_Active_Visible(bool visible)     // 7/25/17
        {
            if (visible)
            {
                MakeVisible(gb_Messaging, true);    // 4/4/17
                MakeVisible(btn_Download_StationFile_from_Central, true);   // 7/17/17
                MakeVisible(tb_Downloaded_Station_Info_File, true);            // 7/17/17
                MakeVisible(btn_Browse_Downloaded_Station_Info_File, true);    // 7/17/17
                MakeVisible(btn_Aid_Start_Runners_Out, true);                   // 7/21/17
                Aid_ConnectedandActive = true;
            }
            else
            {
                MakeVisible(gb_Messaging, false);    // 4/4/17
                MakeVisible(btn_Download_StationFile_from_Central, false);   // 7/17/17
                MakeVisible(tb_Downloaded_Station_Info_File, false);            // 7/17/17
                MakeVisible(btn_Browse_Downloaded_Station_Info_File, false);    // 7/17/17
                MakeVisible(btn_Aid_Start_Runners_Out, false);                   // 7/21/17
                Aid_ConnectedandActive = false;
            }
        }

        void Elapsed1minHandler(object source, ElapsedEventArgs e)  // send Runner data
        {
            // this event happens every 1 minute.
            // its main purpose is to send the new Runner data if there is any and if in Auto mode
            if (Auto_Send_Runner_Update)
            {
                Auto_Send_Elapsed_Minutes++;
                if (Auto_Send_Elapsed_Minutes >= Auto_Send_Minutes)
                {
                    btn_Send_Runners_Update_Click(null, null);
                    Auto_Send_Elapsed_Minutes = 0;
                }
            }

            // another purpose for this timer is to update the AGWPE Statistics, if AGWPE is running
            if (Aid_AGWSocket != null)
                DisplayAidAGWPEportStats(AGWPEPortStatistics);
        }

        private void APRSconnect30secTimerHandler(object sender, ElapsedEventArgs e)    // APRS waiting for DB to announce
        {
            // this happens because the Central DB did not respond
            // first test if Central DB did respond
            if (!WorkerObject.Connected_to_DB)
            {
                // wait another 2 minutes and then try again
                APRSconnect2minTimer.Start();
            }
        }

        private void APRSconnect2minTimerHandler(object sender, ElapsedEventArgs e)     // APRS continue waiting for DB to announce
        {
            if (!WorkerObject.Connected_to_DB)
            {
                // send ?AZRT? again
//                Aid_AGWSocket.TXdataUnproto(AGWPERadioPort, "BEACON", "?AZRT?");
                Aid_AGWSocket.TXdataUnproto(AGWPERadioPort, "?AZRT?");

                //// start the 30 second timer again
                //APRSconnect30secTimer.Start();
                // wait another 2 minutes and then try again
                APRSconnect2minTimer.Start();
            }
        }
        #endregion

        private void Button_PushThread(object info)
        {
            if (Buttons_to_Push.Count != 0)     // need it here too, for security reasons
            {
                // only pull out one button every second
                lock (Buttons_to_Push)
                {
                    switch (Buttons_to_Push.Dequeue())
                    {
                        case Button_to_Push.Aid_APRS_Connect:
                            if (APRSnetworkName != "")
                                btn_APRS_Connect_to_DB_Click(null, null);
                            break;
                        case Button_to_Push.Aid_AGW_Connect:
                            btn_AGWPE_Connect_DB_Click(null, null);
                            break;
                        case Button_to_Push.Aid_AGW_Start:
                            btn_Aid_AGWPE_Start_Refresh_Click(null, null);
                            break;
                        case Button_to_Push.Station_Info_File:      // 8/15/17
                            btn_Download_StationFile_from_Central_Click(null, null);      // 8/15/17
                            break;
                        case Button_to_Push.Bib_Only:      // 8/15/17
                            btn_Aid_Download_Bib_Only_from_DB_Click(null, null);      // 8/15/17
                            break;
                        case Button_to_Push.Runners_List:      // 8/15/17
                            btn_Refresh_Aid_Runner_List_Click(null, null);      // 8/15/17
                            break;
                        case Button_to_Push.DNS_List:      // 8/15/17
                            btn_DNS_Download_Click(null, null);      // 8/15/17
                            break;
                        case Button_to_Push.DNF_List:      // 8/15/17
                            btn_Aid_DNF_Download_Click(null, null);      // 8/15/17
                            break;
                        case Button_to_Push.Watch_List:      // 8/15/17
                            btn_Watch_Download_Click(null, null);      // 8/15/17
                            break;
                        case Button_to_Push.Info_List:      // 8/15/17
                            btn_Download_Info_Click(null, null);      // 8/15/17
                            break;
                    }
                }
            }
            else
            {
                int g = 4;
            }
        }

//        public void ChangeState(Server_State newstate)
//        {
//            // do not change if already at that state
//            if (state != newstate)
//            {
//                // set the new state
//                state = newstate;

//                // clear all the message labels
//                MakeVisible(lbl_Initting, false);
//                MakeVisible(lbl_Cannot_Connect, false);
//                MakeVisible(lbl_Attempting_Connection, false);
//                MakeVisible(lbl_Connected_Central_Database, false);
//                MakeVisible(lbl_Connected_Active, false);
//                MakeVisible(lbl_Error_Connecting, false);
//                Application.DoEvents();     // let the labels change

//                // set the new message label
//                switch (state)
//                {
//                    case Server_State.Not_Initted:
////                        MakeVisible(lbl_Cannot_Connect, true);
//                        MakeVisible(lbl_Initting, true);
//                        break;
////// 7/18/16                    case Server_State.Initted:
////                        MakeVisible(lbl_Cannot_Connect, true);
////                        break;
//                    case Server_State.Attempting_Connect:
//                        MakeVisible(lbl_Attempting_Connection, true);
//                        Attempting_to_Connect_to_Server = true;
//                        break;
//                    case Server_State.Connected:
//                        MakeVisible(lbl_Connected_Central_Database, true);
//                        MakeVisible(btn_Download_StationFile_from_Central, true);
//                        MakeVisible(tb_Downloaded_Station_Info_File, true);
//                        MakeVisible(btn_Browse_Downloaded_Station_Info_File, true);
//                        Connected_to_Server = true;
//                        break;
//                    case Server_State.Connected_Active:
//                        //                        MakeVisible(lbl_Connected_Central_Database, true);
//                        MakeVisible(lbl_Connected_Active, true);
//                        // Messages tab
//                        MakeVisible(lbl_Message_needs_Connect, false);
//                        MakeVisible(gb_Messaging, true);
//                        // settings tab
//                        MakeVisible(btn_Download_StationFile_from_Central, true);
//                        MakeVisible(btn_Connect_to_Mesh_Server, false);
//                        // Debug tab
//                        MakeVisible(lbl_FT_needs_CentralDB, false);
//                        MakeVisible(lbl_File_Transfer, true);
//                        MakeVisible(lbl_File_Transfer_Name, true);
//                        MakeVisible(tb_File_Transfer_filename, true);
//                        break;
//                    case Server_State.Error_Connecting:
//                        MakeVisible(lbl_Error_Connecting, true);
//                        MakeVisible(lbl_Connected_Central_Database, false);
//                        Connected_to_Server = false;
//                        Attempting_to_Connect_to_Server = false;
//                        MakeVisible(gb_Messaging, false);
//                        MakeVisible(btn_Download_StationFile_from_Central, false);
//                        MakeVisible(tb_Downloaded_Station_Info_File, false);
//                        MakeVisible(btn_Browse_Downloaded_Station_Info_File, false);
//                        MakeVisible(lbl_Message_needs_Connect, true);
//                        MakeVisible(btn_Connect_to_Mesh_Server, true);
//                        break;
//                    case Server_State.Cannot_Connect:
//                        MakeVisible(lbl_Cannot_Connect, true);
//                        break;
//                    default:
//                        break;
//                }
//                Application.DoEvents();
//            }
//        }

        private void Add_In_MessageThread(object info)
        {
            // tell the user a new Incoming Message has been received - added 8/14/17
            if ((Sounds_Directory != null) && (Messages_Sound != null))     // 8/18/17
            {
                SoundPlayer player = new SoundPlayer(Sounds_Directory + "\\" + Messages_Sound);
                player.Play();
            }

            Add_In_Message(Incoming_Message);
        }

        private void Add_AlertThread(object info)
        {
            // tell the user an Alert has been received - added 8/14/17
            if ((Sounds_Directory != null) && (Alerts_Sound != null))       // 8/18/17
            {
                SoundPlayer player = new SoundPlayer(Sounds_Directory + "\\" + Alerts_Sound);
                player.Play();
            }

            // look at the Alert to see if it is telling of DNS, DNF or Watch List being changed
            if (Incoming_Alert.Contains("Info File"))
            {
                DialogResult res = MessageBox.Show("The Central Database Info File has changed.\n\nDo you want to download the new file?", "Alert Received", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_Download_Info_Click(null, null);
            }
            if (Incoming_Alert.Contains("Runner List"))
            {
                DialogResult res = MessageBox.Show("The Central Database Runner List has changed.\n\nDo you want to download the new list?", "Alert Received", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_Refresh_Aid_Runner_List_Click(null, null);
            }
            if (Incoming_Alert.Contains("DNS List"))
            {
                DialogResult res = MessageBox.Show("The Central Database DNS List has changed.\n\nDo you want to download the new list?", "Alert Received", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_DNS_Download_Click(null, null);
            }
            if (Incoming_Alert.Contains("DNF List"))
            {
                DialogResult res = MessageBox.Show("The Central Database DNF List has changed.\n\nDo you want to download the new list?", "Alert Received", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_Aid_DNF_Download_Click(null, null);
            }
            if (Incoming_Alert.Contains("Watch List"))
            {
                DialogResult res = MessageBox.Show("The Central Database Watch List has changed.\n\nDo you want to download the new list?", "Alert Received", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_Watch_Download_Click(null, null);
            }
            if (Incoming_Alert.Contains("Issues File"))
            {
                DialogResult res = MessageBox.Show("The Central Database Issues File has changed.\n\nDo you want to download the new file?", "Alert Received", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    //                    btn_DNS_Download_Click(null, null);
                    btn_Get_Issues_Click(null, null);
            }
        }

        #region Enter Runners tab
        private void btn_Aid_Start_Runners_Out_Click(object sender, EventArgs e)    // added 7/21/17
        {
            foreach (RunnersList runner in RunnerList)
            {
                if (!Find_Runner_in_Aid_DNS(runner.BibNumber))
                {
                    Aid_Runner newrunner = new Aid_Runner();
                    newrunner.BibNumber = Convert.ToUInt32(runner.BibNumber);
                    newrunner.TimeOut = Convert.ToDateTime(Start_Time);
                    RunnersOut.Add(newrunner);
                }
            }
            Bind_RunnersOut();
            MakeVisible(btn_Send_Start_Runners_out_to_DB, true);
        }

        private void btn_Send_Start_Runners_out_to_DB_Click(object sender, EventArgs e)     // added 7/21/17
        {
            // make sure we are connected to the Database before we send
            if (WorkerObject.Connected_and_Active)
            {
                // make the Sending label visible and the Send Update button invisible
                MakeVisible(btn_Send_Start_Runners_out_to_DB, false);
                MakeVisible(lbl_Must_wait_til_Connected_Start, false);

                // now test if there are any Runners going out to send
                if (RunnersOut.Count != 0)
                {
                    foreach (Aid_Runner runner in RunnersOut)
                    {
                        SendCommand(Commands.RunnerOut, runner.BibNumber.ToString() + "," + runner.TimeOut.ToShortTimeString());
                        runner.Sent = true;
                    }
                    RunnersOut.Clear();
                    Bind_RunnersOut();
                }
            }
            else
            {   // not connected to Database, must wait to send
                MakeVisible(lbl_Must_wait_til_Connected_Start, true);
            }
        }

        private void cb_Auto_Send_Runner_CheckedChanged(object sender, EventArgs e)
        {
            // did this change becsause of reading from the Registry?
            if (!Init_Registry)
            {       // no - user clicked it
                if (cb_Auto_Send_Runner.Checked)
                    Save_Registry("Auto Send Update", "Yes");
                else
                    Save_Registry("Auto Send Update", "No");
            }

            // now change some other things
            if (cb_Auto_Send_Runner.Checked)
            {
                Auto_Send_Runner_Update = true;
                //lbl_Auto_Update.Visible = true;
                //lbl_Auto_minutes.Visible = true;
                //lb_Auto_Send_Minutes.Visible = true;
                MakeVisible(lbl_Auto_Update, true);
                MakeVisible(lbl_Auto_minutes, true);
                MakeVisible(lb_Auto_Send_Minutes, true);
            }
            else
            {
                Auto_Send_Runner_Update = false;
                //lbl_Auto_Update.Visible = false;
                //lb_Auto_Send_Minutes.Visible = false;
                //lbl_Auto_minutes.Visible = false;
                MakeVisible(lbl_Auto_Update, false);
                MakeVisible(lbl_Auto_minutes, false);
                MakeVisible(lb_Auto_Send_Minutes, false);
            }
        }

        private void lb_Auto_Send_Minutes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Auto_Send_Minutes = Convert.ToInt16(lb_Auto_Send_Minutes.SelectedItem);
            Save_Registry("Auto Send Time", lb_Auto_Send_Minutes.SelectedItem.ToString());
        }

        private void rb_One_Entry_Point_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_One_Entry_Point.Checked)
            {
                //panel_Runners_Out.Visible = false;
                //lbl_Remove_from_Runner_list_Pannel1.Visible = false;
                //lbl_Remove_Runners_from_Station.Visible = true;
                MakeVisible(panel_Runners_Out, false);
                MakeVisible(lbl_Remove_from_Runner_list_Pannel1, false);
                MakeVisible(lbl_Remove_Runners_from_Station, true);
                One_Reader_Only = true;
                rb_One_Log_Point.Checked = true;
                NumLogPts = 1;
                Save_Registry("Number of Log Points", "1");
            }
            else
            {
                //panel_Runners_Out.Visible = true;
                //lbl_Remove_Runners_from_Station.Visible = false;
                //lbl_Remove_from_Runner_list_Pannel1.Visible = true;
                MakeVisible(panel_Runners_Out, true);
                MakeVisible(lbl_Remove_from_Runner_list_Pannel1, true);
                MakeVisible(lbl_Remove_Runners_from_Station, false);
                One_Reader_Only = false;
                rb_Two_Log_Pts.Checked = true;
                NumLogPts = 2;
                Save_Registry("Number of Log Points", "2");
            }
            Send_LogPts();
        }

        private void lb_Remove_from_Station_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            Remove_from_Runners_list_Minutes = Convert.ToInt16(lb_Remove_from_Station_List.SelectedItem);
            Save_Registry("Remove from List", lb_Remove_from_Station_List.SelectedItem.ToString());
        }

        private void btn_Send_Runners_Update_Click(object sender, EventArgs e)
        {
            // make sure we are connected to the Database before we send
// 7/22/16            if (Connected_to_Server)
            if (WorkerObject.Connected_and_Active)
            {
                // make the Sending label visible and the Send Update button invisible
                MakeVisible(btn_Send_Runners_Update, false);
                MakeVisible(lbl_Must_Wait_til_Connected, false);
                // ???            lbl_Sending_Update.Visible = true;

                // first test if there are any Runners coming in to send
                if (RunnersIn.Count != 0)
                {
                    foreach (Aid_Runner runner in RunnersIn)
                    {
                        SendCommand(Commands.RunnerIn, runner.BibNumber.ToString() + "," + runner.TimeIn.ToShortTimeString());
                        runner.Sent = true;
                    }
                    RunnersIn.Clear();
                    Bind_RunnersIn();
                }

                // now test if there are any Runners going out to send
                if (RunnersOut.Count != 0)
                {
                    foreach (Aid_Runner runner in RunnersOut)
                    {
                        SendCommand(Commands.RunnerOut, runner.BibNumber.ToString() + "," + runner.TimeOut.ToShortTimeString());
                        runner.Sent = true;
                    }
                    RunnersOut.Clear();
                    Bind_RunnersOut();
                }
            }
            else
            {   // not connected to Database, must wait to send
                MakeVisible(lbl_Must_Wait_til_Connected, true);
            }
        }

        public void Bind_RunnersIn()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_RunnersIn.InvokeRequired)
            {
                Bind_RunnersIndel d = new Bind_RunnersIndel(Bind_RunnersIn);
                dgv_RunnersIn.Invoke(d, new object[] { });
            }
            else
            {
                dgv_RunnersIn.DataSource = null;
                dgv_RunnersIn.DataSource = RunnersIn;
                dgv_RunnersIn.Columns[0].Width = 40;
                dgv_RunnersIn.Columns[0].HeaderText = "#";
                dgv_RunnersIn.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnersIn.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                //                dgv_RunnersIn.Columns[1].Width = 64;
                dgv_RunnersIn.Columns[1].Width = 50;
                dgv_RunnersIn.Columns[1].HeaderText = "Time In";
                dgv_RunnersIn.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnersIn.Columns[1].DefaultCellStyle.Format = "HH:mm";
                dgv_RunnersIn.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_RunnersIn.Columns[2].Visible = false;
                dgv_RunnersIn.Columns[3].Visible = false;
                dgv_RunnersIn.Columns[4].Visible = false;
                dgv_RunnersIn.Columns[5].Visible = false;
                dgv_RunnersIn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnersIn.Update();
            }
        }

        public void Bind_RunnersOut()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_RunnersOut.InvokeRequired)
            {
                Bind_RunnersOutdel d = new Bind_RunnersOutdel(Bind_RunnersOut);
                dgv_RunnersOut.Invoke(d, new object[] { });
            }
            else
            {
                dgv_RunnersOut.DataSource = null;
                dgv_RunnersOut.DataSource = RunnersOut;
                dgv_RunnersOut.Columns[0].Width = 40;
                dgv_RunnersOut.Columns[0].HeaderText = "#";
                dgv_RunnersOut.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnersOut.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_RunnersOut.Columns[1].Visible = false;
                dgv_RunnersOut.Columns[2].Width = 56;
                dgv_RunnersOut.Columns[2].HeaderText = "Time Out";
                dgv_RunnersOut.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnersOut.Columns[2].DefaultCellStyle.Format = "HH:mm";
                dgv_RunnersOut.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_RunnersOut.Columns[3].Visible = false;
                dgv_RunnersOut.Columns[4].Visible = false;
                dgv_RunnersOut.Columns[5].Visible = false;
                dgv_RunnersOut.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_RunnersOut.Update();
            }
        }

        private void tb_Add_Runner_In_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_Runner_In_Click(null, null);
        }

        private void tb_Add_Runner_out_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_Runner_Out_Click(null, null);
        }

        private void tb_Add_Runner_In_TextChanged(object sender, EventArgs e)
        {
            if (tb_Add_Runner_In.Text != "")
            {
                MakeVisible(lbl_Aid_RunnerIn_Add, true);
                MakeVisible(btn_Add_Runner_In, true);
            }
            else
            {
                MakeVisible(lbl_Aid_RunnerIn_Add, false);
                MakeVisible(btn_Add_Runner_In, false);
            }
        }

        private void tb_Add_Runner_Out_TextChanged(object sender, EventArgs e)
        {
            if (tb_Add_Runner_Out.Text != "")
                MakeVisible(btn_Add_Runner_Out, true);
            else
                MakeVisible(btn_Add_Runner_Out, false);
        }

        private void btn_Add_Runner_In_Click(object sender, EventArgs e)
        {
            // verify there is an entry in the textbox
            if (tb_Add_Runner_In.Text != "")
            {
                // first check if this runner is on one of the lists
                if ((RunnerList.Count!=0) && !Find_Runner_in_Aid_RunnerList(tb_Add_Runner_In.Text))
                {
                    MessageBox.Show("This runner is NOT in the Runner List.\n\nThis runner will NOT be added.", "Invalid runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
// 4/2/16                else
                    if (Find_Runner_in_Aid_DNS(tb_Add_Runner_In.Text))
                    {
                        MessageBox.Show("Impossible!\n\nThis runner is in the DNS list.", "Invalid runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                        if (Find_Runner_in_Aid_DNF(tb_Add_Runner_In.Text))
                        {
                            MessageBox.Show("Impossible!\n\nThis runner is in the DNF list.", "Invalid runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                            if (Find_Runner_in_Aid_Watch(tb_Add_Runner_In.Text))
                            {
                                MessageBox.Show("This runner is in the Watch list.\n\nPlease update his status in that list.", "Watched runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }

                            // now add the runner
                            Aid_Runner runner = new Aid_Runner();
                            Runner_Times runnerT = new Runner_Times();  // 5/10/19
                            runner.BibNumber = Convert.ToUInt32(tb_Add_Runner_In.Text);
                            runnerT.BibNumber = runner.BibNumber.ToString();  // 5/10/19
// 5/10/19                            tb_Add_Runner_In.Text = "";
                            SetTBtext(tb_Add_Runner_In, "");    // 5/11/19
                            runner.TimeIn = DateTime.Now;
                            runnerT.TimeIn = runner.TimeIn;  // 5/10/19
                            runnerT.New = true;  // 5/10/19
                            Runners_thru_Station.Add(runnerT);  // 5/10/19
                            Runners_Thru_Changed = true;    // 5/11/19
                            RunnersIn.Add(runner);
                            Bind_RunnersIn();
// 5/10/19                            btn_Send_Runners_Update.Visible = true;
                            MakeVisible(btn_Send_Runners_Update, true); // 5/10/19
                            RunnersAtStation.Add(runner);
                            Bind_RunnersAtStation();
                            tb_Add_Runner_In.Focus();

                            // increment the Total # of Runners
                            Total_Number_of_Runners++;
// 5/11/19                            tb_Total_Number_Runners.Text = Total_Number_of_Runners.ToString();
                            SetTBtext(tb_Total_Number_Runners, Total_Number_of_Runners.ToString());     // 5/11/19
                        }
            }
        }

        private void btn_Add_Runner_Out_Click(object sender, EventArgs e)
        {
            // first check if this runner is in the Runner list or the Runners At Station list
            //if (!Find_Runner_in_RunnerList(tb_Add_Runner_Out.Text))
            //{
            //    MessageBox.Show("This runner is NOT in the Runner List.\n\nThis runner will NOT be added.", "Invalid runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
            //else
            //                if (!Find_Runner_in_RunnersAtStation(tb_Add_Runner_Out.Text))
            int index = Find_Runner_in_RunnersAtStation(tb_Add_Runner_Out.Text);
            if (index == -1)
            {
                MessageBox.Show("This runner is NOT in the Runners At Station List.\n\nThis runner will NOT be added.", "Invalid runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                Aid_Runner runner = new Aid_Runner();
                string bib = tb_Add_Runner_Out.Text;    // 5/11/19
// 5/11/19                runner.BibNumber = Convert.ToUInt32(tb_Add_Runner_Out.Text);
                runner.BibNumber = Convert.ToUInt32(bib);   // 5/11/19
// 5/11/19                tb_Add_Runner_Out.Text = "";
                SetTBtext(tb_Add_Runner_Out, "");   // 5/11/19
                runner.TimeOut = DateTime.Now;
                RunnersOut.Add(runner);
                Bind_RunnersOut();
                // 5/10/19                btn_Send_Runners_Update.Visible = true;
                MakeVisible(btn_Send_Runners_Update, true);  // 5/10/19

                // change entry in RunnersAtStation
                RunnersAtStation[index].TimeOut = runner.TimeOut;
                Bind_RunnersAtStation();
                tb_Add_Runner_Out.Focus();

                // add to Runners Thru Station  - 5/10/19
                int index2 = Find_Runner_in_RunnersThruStation(bib);
                if (index2 == -1)
                {
                    MessageBox.Show("This runner is NOT in the Runners Thru Station List.", "Invalid runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {       // 5/10/19
                    Runners_thru_Station[index2].TimeOut = runner.TimeOut;
                    Runners_thru_Station[index2].New = true;
                    Runners_Thru_Changed = true;
                }
            }
        }

        public void Bind_RunnersAtStation()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_Runners_at_Station.InvokeRequired)
            {
                SetDGVRASsourceDel d = new SetDGVRASsourceDel(Bind_RunnersAtStation);
                dgv_Runners_at_Station.Invoke(d, new object[] { });
            }
            else
            {
                dgv_Runners_at_Station.DataSource = null;
                dgv_Runners_at_Station.DataSource = RunnersAtStation;
                dgv_Runners_at_Station.RowHeadersVisible = false;
                dgv_Runners_at_Station.Columns[0].Width = 39;
                dgv_Runners_at_Station.Columns[0].HeaderText = "#";
                dgv_Runners_at_Station.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dgv_Runners_at_Station.Columns[1].Width = 50;
                if (NumLogPts < 2)
                {
                    dgv_Runners_at_Station.Columns[1].Width = 70;
                    dgv_Runners_at_Station.Columns[1].HeaderText = "Time In/Out";
                    dgv_Runners_at_Station.Columns[2].Visible = false;
                    dgv_Runners_at_Station.Columns[3].Visible = false;
                    dgv_Runners_at_Station.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgv_Runners_at_Station.Columns[5].Visible = false;
                }
                else
                {
                    dgv_Runners_at_Station.Columns[1].Width = 50;
                    dgv_Runners_at_Station.Columns[1].HeaderText = "Time In";
                    dgv_Runners_at_Station.Columns[2].Width = 56;
                    dgv_Runners_at_Station.Columns[2].HeaderText = "Time Out";
                    dgv_Runners_at_Station.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_Runners_at_Station.Columns[2].DefaultCellStyle.Format = "HH:mm";
                    dgv_Runners_at_Station.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgv_Runners_at_Station.Columns[3].Width = 50;
                }
                dgv_Runners_at_Station.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runners_at_Station.Columns[1].DefaultCellStyle.Format = "HH:mm";
                dgv_Runners_at_Station.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                //dgv_Runners_at_Station.Columns[2].Width = 56;
                //dgv_Runners_at_Station.Columns[2].HeaderText = "Time Out";
                //dgv_Runners_at_Station.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dgv_Runners_at_Station.Columns[2].DefaultCellStyle.Format = "HH:mm";
                //dgv_Runners_at_Station.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                //dgv_Runners_at_Station.Columns[3].Width = 50;
                dgv_Runners_at_Station.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runners_at_Station.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Runners_at_Station.Update();
            }
        }

        int Find_Runner_in_RunnersAtStation(string RunnerNumber)
        {
            uint RunnerNumb = Convert.ToUInt16(RunnerNumber);
            int index = RunnersAtStation.FindIndex(runner => runner.BibNumber == RunnerNumb);
            return index;
        }

        bool Is_Runner_in_RunnersAtStation(string RunnerNumber)
        {
            uint RunnerNumb = Convert.ToUInt16(RunnerNumber);
            int index = RunnersAtStation.FindIndex(runner => runner.BibNumber == RunnerNumb);
            if (index >= 0)
                return true;
            else
                return false;
        }

        int Find_Runner_in_RunnersThruStation(string RunnerNumber)
        {
//            uint RunnerNumb = Convert.ToUInt16(RunnerNumber);
//            int index = RunnersAtStation.FindIndex(runner => runner.BibNumber == RunnerNumb);
            int index = Runners_thru_Station.FindIndex(runner => runner.BibNumber == RunnerNumber);
            return index;
        }
        #endregion

        #region Reconciled Runners tab
        private void tabPage_Reconciled_Runners_Enter(object sender, EventArgs e)
        {
            // before displaying the Reconciled Runners DVG, need to add in any changes
            // test if the Runners list has changed
            if (Runner_List_Changed)
            {
                // clear the existing Reconciled data
                Reconciled_Runners.Clear();

                // add all Runners to the Reconciled data
                foreach (RunnersList runner in RunnerList)
                {
                    Runner_Times newrunner = new Runner_Times();
                    newrunner.BibNumber = runner.BibNumber;
                    newrunner.DNSorDNF = false;     // 5/8/19
                    Reconciled_Runners.Add(newrunner);
                }
            }

            // test if the DNS list has changed
            if (DNS_List_Changed || Runner_List_Changed)
            {
                // this will not handle runners removed from the DNS list

                // find each DNS runner in the Runner List
                for (int i = 0; i < lb_Aid_DNS.Items.Count; i++)
                {
                    int index = Reconciled_Runners.FindIndex(
                        delegate(Runner_Times newrunner)
                        {
                            return newrunner.BibNumber == lb_Aid_DNS.Items[i].ToString();
                        });
                    if (index != -1)
                    {
                        // set flag so it will become strikethrough
                        Reconciled_Runners[index].DNSorDNF = true;
                    }
                }

                // clear the flag
                DNS_List_Changed = false;
            }

            // test if the DNF list has changed
            if (DNF_List_Changed || Runner_List_Changed)
            {
                // this will not handle runners removed from the DNF list

                // find each DNSFrunner in the Runner List
                foreach (Aid_RunnerDNFWatch runner in Aid_DNFList)
                {
                    int index = Reconciled_Runners.FindIndex(
                        delegate(Runner_Times newrunner)
                        {
                            return newrunner.BibNumber == runner.BibNumber;
                        });
                    if (index != -1)
                    {
                        // set flag so it will become strikethrough
                        Reconciled_Runners[index].DNSorDNF = true;
                    }
                }

                // clear the flag
                DNF_List_Changed = false;
            }

            // now clear the Runner List flag
            Runner_List_Changed = false;

            // add in the Runners through this station
            if (Runners_Thru_Changed)
            {
                foreach (Runner_Times runner in Runners_thru_Station)
                {
                    if (runner.New)
                    {
                        // add data to the Reconciled list
                        int index = Reconciled_Runners.FindIndex(
                            delegate(Runner_Times newrunner)
                            {
                                return newrunner.BibNumber == runner.BibNumber;
                            });
                        if (index != -1)
                        {
                            // set the Ime In and Time out times
                            Reconciled_Runners[index].TimeIn = runner.TimeIn;
                            Reconciled_Runners[index].TimeOut = runner.TimeOut;
                        }


                        // clear the new flag for this runner
                        runner.New = false;
                    }
                }

                // clear the Runners through flag
                Runners_Thru_Changed = false;
            }

            // now display the Runner data
            Bind_Reconciled_Runners();

            // update the stats at the bottom
            SetTBtext(tb_RR_Total, RunnerListCount.ToString());
            int DNSplusDNF = Aid_DNFList.Count + lb_Aid_DNS.Items.Count;
            SetTBtext(tb_RR_Total_after_DNSDNF, DNSplusDNF.ToString());
            SetTBtext(tb_RR_Total_this, Runners_thru_Station.Count.ToString());
            SetTBtext(tb_RR_Expecting, (RunnerListCount - DNSplusDNF - Runners_thru_Station.Count).ToString());
        }
        
        public void Bind_Reconciled_Runners()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dgv_Reconcile.InvokeRequired)
            {
                SetDGVRASsourceDel d = new SetDGVRASsourceDel(Bind_Reconciled_Runners);
                dgv_Reconcile.Invoke(d, new object[] { });
            }
            else
            {
                int width, adjust;
                dgv_Reconcile.DataSource = null;
                dgv_Reconcile.DataSource = Reconciled_Runners;
                dgv_Reconcile.RowHeadersVisible = false;
                dgv_Reconcile.Columns[0].Width = 39;
                dgv_Reconcile.Columns[0].HeaderText = "#";
                dgv_Reconcile.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (NumLogPts < 2)
                {
                    dgv_Reconcile.Columns[1].Width = 70;
                    dgv_Reconcile.Columns[1].HeaderText = "Time In/Out";
                    dgv_Reconcile.Columns[2].Visible = false;
                    dgv_Reconcile.Columns[2].Width = 0;
                    width = 112;    // without scrollbar
                }
                else
                {
                    dgv_Reconcile.Columns[1].Width = 50;
                    dgv_Reconcile.Columns[1].HeaderText = "Time In";
                    dgv_Reconcile.Columns[2].Width = 56;
                    dgv_Reconcile.Columns[2].HeaderText = "Time Out";
                    dgv_Reconcile.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv_Reconcile.Columns[2].DefaultCellStyle.Format = "HH:mm";
                    dgv_Reconcile.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                    width = 148;    // without scrollbar
                }
                dgv_Reconcile.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Reconcile.Columns[1].DefaultCellStyle.Format = "HH:mm";
                dgv_Reconcile.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Reconcile.Columns[3].Visible = false;
                dgv_Reconcile.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // change the size of the DGV to match 1 or 2 Log Pts - 5/5/19
                // and to accommodate more than one screen ful of runners = 5/8/19
                if (RunnerListCount > 15)   // 5/8/19
                    adjust = 17;   // 5/8/19 - this is the width of the vertical scrollbar
                else
                    adjust = 0;   // 5/8/19
                dgv_Reconcile.ClientSize = new Size(width + adjust, 353);   // 5/5/19

                dgv_Reconcile.Update();
            }
        }

        private void dgv_Reconcile_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                // test for DNS/DNF
                if (Reconciled_Runners[e.RowIndex].DNSorDNF)
                {
                    Font Strike = new Font(dgv_Reconcile.DefaultCellStyle.Font, FontStyle.Strikeout);
                    dgv_Reconcile.Rows[e.RowIndex].DefaultCellStyle.Font = Strike;
                    dgv_Reconcile.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
                }

                // test if Through Station
                if (NumLogPts < 2)
                {       // Time In only
                    if (Reconciled_Runners[e.RowIndex].TimeIn.TimeOfDay.Ticks != 0)
                    {
                        dgv_Reconcile.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                }
                else
                {       // Tine In and Time out
                    if ((Reconciled_Runners[e.RowIndex].TimeIn.TimeOfDay.Ticks != 0) && (Reconciled_Runners[e.RowIndex].TimeOut.TimeOfDay.Ticks != 0))
                    {
                        dgv_Reconcile.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                }
            }
        }
        #endregion

        #region RFID Reading tab
        void AddRunner(string numbstr)         // add the new number only if it does not already exist
        {
            DateTime Now = DateTime.Now;
            int number = 0;

            // look up this string in the local database, to get the actual Runner number
            int index = RFIDAssignments.FindIndex(
                delegate(RFID rfid)
                {
                    return rfid.String == numbstr;
                });
            if (index != -1)
            {       // found RFID string in the list
                number = RFIDAssignments[index].RunnerNumber;
            }
            else
            {       // runner # not found
                NewRFIDNumber newn = new NewRFIDNumber();
                newn.BibNumber = numbstr;
                DialogResult res = newn.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    // get the new number
                    number = Convert.ToInt16(newn.RunnerNumber);

                    // check if need to save it
                    if (newn.SaveNewNumber)
                    {
                        // save it to the Assignments file
                        RFID rfid = new RFID();
                        rfid.RunnerNumber = number;
                        rfid.String = numbstr;
                        RFIDAssignments.Add(rfid);

                        // save the changed file
                        Save_RFID_Assignents(RFIDAssignments, tb_RFIDnumber_Assignment_file.Text);  // this only saves the file name, not the file contents !!!
                    }
                }
            }

            // find in the list of Runners that have already come through this station
            index = Runners.FindIndex(
                delegate(Aid_Runner runner)
                {
                    return runner.BibNumber == number;
                });
            if (index == -1)
            {
                // add to the list
                Aid_Runner newRunner = new Aid_Runner();
                newRunner.TimeIn = Now;
                newRunner.BibNumber = (uint)number;
                newRunner.Minutes = 0;
                newRunner.Sent = false;
                newRunner.RFIDnumber = numbstr;
                Runners.Add(newRunner);
                RunnersAtStation.Add(newRunner);    // also add to the Runners at this Station list

                // put in the Datagridview and update
                Bind_RunnersAtStation();      // also show in Runners at this Station tab

                // add to the Matching Runner # for Incoming
                AppendRXtext(tb_Match_incoming, number.ToString());

                // make the Send button and Auto Send checkbox visible
                Make_Send_Update_Visible();
            }
        }

        void Make_Send_Update_Visible()     // make the Send_Update button and Auto Send checkbox visible
        // if Connected to the Central Database
        {
//            if (Connected_to_Central_Database)
            if (WorkerObject.Connected_and_Active)
            {
                MakeVisible(btn_Send_Runners_Update, true);
                MakeVisible(cb_Auto_Send_Runner, true);
            }
        }

        void UpdateRunner(string numbstr)      // this adds the TimeOut to an existing Runner
        {
            DateTime Now = DateTime.Now;

            // find in the list
            int index = RunnersAtStation.FindIndex(
                delegate(Aid_Runner runner)
                {
                    return runner.RFIDnumber == numbstr;
                });
            if (index != -1)
            {
                // add the TimeOut time to the Runner
                Runners[index].TimeOut = Now;

                // update the Datagridview
                Bind_RunnersAtStation();

                // add to the Matching Runner # for Outgoing
                AppendRXtext(tb_Match_outgoing, Runners[index].BibNumber.ToString());

                // make the Send button and Auto Send checkbox visible
                Make_Send_Update_Visible();
            }
            else
            {       // this is a new number, not in the list
                MessageBox.Show("This RFID Number:  " + numbstr +
                                "\n\nhas been read at the Outgoing RFID reader, " +
                                "\n\nbut has not yet been read at the Incoming reader!", "Unknown RFID Number", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public void MakeCBChecked(CheckBox ctrl, bool checkd)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (ctrl.InvokeRequired)
            {
                MakeCheckeddel d = new MakeCheckeddel(MakeCBChecked);
                ctrl.Invoke(d, new object[] { ctrl, checkd });
            }
            else
            {
                ctrl.Checked = checkd;
                ctrl.Update();
            }
        }

        public void MakeRBChecked(RadioButton rb, bool checkd)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (rb.InvokeRequired)
            {
                MakeRBCheckeddel d = new MakeRBCheckeddel(MakeRBChecked);
                rb.Invoke(d, new object[] { rb, checkd });
            }
            else
            {
                rb.Checked = checkd;
                rb.Update();
            }
        }

        public void SetCtlText(Control ctrl, string str)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (ctrl.InvokeRequired)
            {
                SetCtlTextdel d = new SetCtlTextdel(SetCtlText);
                ctrl.Invoke(d, new object[] { ctrl, str });
            }
            else
            {
                ctrl.Text = str;
                ctrl.Update();
            }
        }
        #endregion

        #region Lookup Runner tab
        private void tb_RunnerNo_TextChanged(object sender, EventArgs e)
        {
            if (tb_RunnerNo.Text != "")
            {
                btn_Lookup.Visible = true;
                //                lbl_Fetching_Runner_Status.Visible = true;
            }
        }

        private void tb_RunnerNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Lookup_Click(null, null);
        }

        private void btn_Lookup_Click(object sender, EventArgs e)
        {
            // clear the textbox and dgv first
            // NO - need to see which runner           tb_RunnerNo.Clear();
            string RunnerNumber = tb_RunnerNo.Text;     // 7/24/17
            MakeVisible(lbl_Aid_Runner_in_DNF, false);      // 7/24/17 moved up here
            MakeVisible(lbl_Aid_Runner_in_DNS, false);      // 7/24/17
            MakeVisible(lbl_Aid_Runner_in_Watch, false);    // 7/24/17
            Aid_dgv_Runner_Status.DataSource = null;
            Aid_dgv_Runner_Status.Update();
            lbl_Runner_No_Status.Visible = false;
            lbl_Runner_notin_Official.Visible = false;

            // if the Central Database is not connected to, then just look in this Station's list
// 7/22/16            if (Connected_to_Server)
            if (WorkerObject.Connected_and_Active)
            {       // get from Central Database
                // first test if this Runner is in the Official Runner List
                if (RunnerList_Has_Entries)
// 7/24/17                    if (!Find_Runner_in_Aid_RunnerList(tb_RunnerNo.Text))
                    if (!Find_Runner_in_Aid_RunnerList(RunnerNumber))       // 7/24/17
                    {
                        lbl_Runner_notin_Official.Visible = true;
                        return;     // quit early
                    }

                // Now request from Central
// 7/24/17                SendCommand(Commands.RequestRunner, tb_RunnerNo.Text);
                SendCommand(Commands.RequestRunner, RunnerNumber);      // 7/24/17
                // NO - need to see which runner                tb_RunnerNo.Clear();
                btn_Lookup.Visible = false;
// 7/24/17                lbl_Fetching_Runner_Status.Visible = true;
                MakeVisible(lbl_Fetching_Runner_Status, true);      // 7/24/17

                // now wait for response from Central // this needs to be REMOVED !!!
                while (!Aid_Worker.Runner_Status_Received) // this needs to be REMOVED !!!
                {
                    Application.DoEvents(); // this needs to be REMOVED !!!
                    Application.DoEvents(); // this needs to be REMOVED !!!
                }
                lbl_Fetching_Runner_Status.Visible = false; // this needs to be REMOVED !!!
                if (Form1.RunnersStatus.Count == 0) // this needs to be REMOVED !!!
                    lbl_Runner_No_Status.Visible = true; // this needs to be REMOVED !!!
                Aid_Worker.Runner_Status_Received = false; // this needs to be REMOVED !!!
            }
            else
            {       // this station
                // look in this station's list
                int index = Runners.FindIndex(
                    delegate(Aid_Runner runner)
                    {
// 7/24/17                        return runner.BibNumber == Convert.ToUInt32(tb_RunnerNo.Text);
                        return runner.BibNumber == Convert.ToUInt32(RunnerNumber);      // 7/24/17
                    });
                // NO - need to see which runner                tb_RunnerNo.Clear();
                if (index == -1)
                {
// 7/24/17                    MessageBox.Show("Runner #:  " + tb_RunnerNo.Text + " was not found!", "Runner not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    MessageBox.Show("Runner #:  " + RunnerNumber + " was not found!", "Runner not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);    // 7/24/17
                    return;     // quit early
                }

                // populate the list
                RunnerStatus rs = new RunnerStatus();
                RunnersStatus.Clear();
                rs.Station = tb_Station_Name.Text;
                rs.TimeIn = Convert.ToDateTime(Runners[index].TimeIn).ToShortTimeString();
                rs.TimeOut = Convert.ToDateTime(Runners[index].TimeOut).ToShortTimeString();
                //                rs.TimeAtStation = Runners[index].t;
                //
                RunnersStatus.Add(rs);
            }

            // display the data
            Bind_Aid_Runner_Status(RunnersStatus);

            // also show if this runner is in one of the lists - added 7/24/17
//// 7/24/17 moved to top            MakeVisible(lbl_Aid_Runner_in_DNF, false);
//            MakeVisible(lbl_Aid_Runner_in_DNS, false);
//            MakeVisible(lbl_Aid_Runner_in_Watch, false);
            if (Find_Runner_in_Aid_DNS(RunnerNumber))
            {
                MakeVisible(lbl_Aid_Runner_in_DNS, true);
            }
            if (Find_Runner_in_Aid_DNF(RunnerNumber))
            {
                MakeVisible(lbl_Aid_Runner_in_DNF, true);
            }
            if (Find_Runner_in_Aid_Watch(RunnerNumber))
            {
                MakeVisible(lbl_Aid_Runner_in_Watch, true);
            }
        }

        public void Bind_Aid_Runner_Status(List<RunnerStatus> RunnersStatus)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (Aid_dgv_Runner_Status.InvokeRequired)
            {
                SetDGVsourceStatusDel d = new SetDGVsourceStatusDel(Bind_Aid_Runner_Status);
                Aid_dgv_Runner_Status.Invoke(d, new object[] { RunnersStatus });
            }
            else
            {
                Aid_dgv_Runner_Status.DataSource = null;
                Aid_dgv_Runner_Status.DataSource = RunnersStatus;
                Aid_dgv_Runner_Status.AllowUserToResizeColumns = true;
                Aid_dgv_Runner_Status.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //                Aid_dgv_Runner_Status.Columns[0].Width = 75;
                Aid_dgv_Runner_Status.Columns[0].Width = Station_DGV_Width;
                Aid_dgv_Runner_Status.Columns[0].HeaderText = "Station";
                Aid_dgv_Runner_Status.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_Status.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_Status.Columns[1].Width = 68;
                Aid_dgv_Runner_Status.Columns[1].HeaderText = "Time In";
                Aid_dgv_Runner_Status.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_Status.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_Status.Columns[2].Width = 75;
                Aid_dgv_Runner_Status.Columns[2].HeaderText = "Time Out";
                Aid_dgv_Runner_Status.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_Status.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_Status.Columns[3].Width = 66;
                Aid_dgv_Runner_Status.Columns[3].HeaderText = "Time at Station";
                Aid_dgv_Runner_Status.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_Status.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_Status.Columns[4].Width = 79;
                Aid_dgv_Runner_Status.Columns[4].HeaderText = "Time from Previous";
                Aid_dgv_Runner_Status.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_Status.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_Status.Columns[5].Width = 68;
                Aid_dgv_Runner_Status.Columns[5].HeaderText = "Time to Next";
                Aid_dgv_Runner_Status.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_Status.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_Status.Update();
            }
        }
        #endregion

        #region Messages tab
        //
        //  Incoming and Outgoing Messages files format:
        //
        //  message number,time,size
        //  </t
        //  {.. text ..}
        //  </-t
        //      next message
        //
        int Out_Message_Number = 1;
        int In_Message_Number = 1;

        private void tb_Outgoing_Message_TextChanged(object sender, EventArgs e)
        {
            if (tb_Outgoing_Message.Text == "")
                // 7/18/17                btn_Send_Outgoing_Message.Visible = false;
                MakeVisible(btn_Send_Outgoing_Message, false);
            else
                // 7/18/17                btn_Send_Outgoing_Message.Visible = true;
                MakeVisible(btn_Send_Outgoing_Message, true);
        }

        private void btn_Send_Outgoing_Message_Click(object sender, EventArgs e)
        {
            SendCommand(Commands.Message, tb_Outgoing_Message.Text + (Char)3);
            // 7/18/17            btn_Send_Outgoing_Message.Visible = false;
            MakeVisible(btn_Send_Outgoing_Message, false);
            Add_Out_Message(tb_Outgoing_Message.Text);
        }

        private void Add_In_Message(string message)
        {
            if (dgv_Incoming_Messages.InvokeRequired)
            {
                SetDGVInMessDel d = new SetDGVInMessDel(Add_In_Message);
                dgv_Incoming_Messages.Invoke(d, new object[] { message });
            }
            else
            {
                // add the new message to the list
                In_Message newmessage = new In_Message();
                newmessage.Received_Time = DateTime.Now;
                newmessage.Message_Number = In_Message_Number;
                In_Message_Number++;
                newmessage.Message_string = message;
                newmessage.Size = message.Length;
                Incoming_Messages.Add(newmessage);

                // When the 10th message is added, need to change the size and location of the DGV
                if (newmessage.Message_Number == 10)
                {
                    int adjust = 18;
                    dgv_Incoming_Messages.Width += adjust;
                    dgv_Incoming_Messages.Left -= (adjust / 2);
                }

                // update the datagridview
                //public class Message
                //{
                //    public int Message_Number { get; set; }
                //    public DateTime Received_Time { get; set; }
                //    public int Size { get; set; }
                //    public string Message_string { get; set; }
                //}
                dgv_Incoming_Messages.DataSource = null;
                dgv_Incoming_Messages.DataSource = Incoming_Messages;
                dgv_Incoming_Messages.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Incoming_Messages.Columns[0].Width = 25;     // Number
                dgv_Incoming_Messages.Columns[0].HeaderText = "#";
                dgv_Incoming_Messages.Columns[1].Width = 65;     // Received_Time
                dgv_Incoming_Messages.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Incoming_Messages.Columns[1].HeaderText = "Time";
                dgv_Incoming_Messages.Columns[1].DefaultCellStyle.Format = "HH:mm:ss";
                dgv_Incoming_Messages.Columns[2].Width = 30;     // Size
                dgv_Incoming_Messages.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_Incoming_Messages.Columns[2].HeaderText = "Size";
                dgv_Incoming_Messages.Columns[3].Visible = false;
                dgv_Incoming_Messages.CurrentCell = dgv_Incoming_Messages[0, Incoming_Messages.Count - 1];
                dgv_Incoming_Messages.CurrentCell.Selected = true;
                dgv_Incoming_Messages.Update();
// 7/18/17                dgv_Incoming_Messages_DoubleClick(null, null);

                // put the new message in the Selected message box - 7/18/17
                int Msg_Number = Incoming_Messages[dgv_Incoming_Messages.CurrentRow.Index].Message_Number;  // 7/18/17
                SetTBtext(tb_Aid_Selected_Message, Incoming_Messages[Msg_Number - 1].Message_string);       // 7/17/18

                // tell the user, ask if show now
                Select_Message_Tab();
            }
        }

        private void Select_Message_Tab()
        {
            if (tabControl_Main_Aid.InvokeRequired)
            {
                SelectTabDel d = new SelectTabDel(Select_Message_Tab);
                tabControl_Main_Aid.Invoke(d, new object[] { });
            }
            else
            {
                if (tabControl_Main_Aid.SelectedTab == tabPage_Aid_Messages)
                {
                    MessageBox.Show("New message received from Central Database.", "New message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //                    _last.Focus();
                }
                else
                {
                    DialogResult res = MessageBox.Show("New message received from Central Database.\n\nDisplay it now?", "New message", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (res == System.Windows.Forms.DialogResult.Yes)
                    {
                        tabControl_Main_Aid.SelectedTab = tabPage_Aid_Messages;
                        tabControl_Main_Aid.Update();
                    }
                    else
                        _last.Focus();
                }
            }
        }

        private void Add_Out_Message(string message)
        {
            // add the new message to the list
            Out_Message newmessage = new Out_Message();
            newmessage.Received_Time = DateTime.Now;
            newmessage.Message_Number = Out_Message_Number;
            Out_Message_Number++;
            tb_Message_Number.Text = Out_Message_Number.ToString();
            newmessage.Message_string = message;
            newmessage.Size = message.Length;
            tb_Outgoing_Message.Clear();
            Outgoing_Messages.Add(newmessage);

            // make the Send/Ack label visible
            MakeVisible(lbl_Send_Ackd, true);      // 7/17/17

            // When the 10th message is added, need to change the size and location of the DGV
            if (newmessage.Message_Number == 10)
            {
                int adjust = 18;
                dgv_Outgoing_Messages.Width += adjust;
                dgv_Outgoing_Messages.Left -= (adjust / 2);
            }

            // update the datagridview
            //public class Out_Message
            //{
            //    public int Message_Number { get; set; }
            //    public DateTime Received_Time { get; set; }
            //    public int Size { get; set; }
            //    public bool Sent { get; set; }
            //    public bool Acknowledged { get; set; }
            //    public string Message_string { get; set; }
            //}
            dgv_Outgoing_Messages.DataSource = null;
            dgv_Outgoing_Messages.DataSource = Outgoing_Messages;
            dgv_Outgoing_Messages.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages.Columns[0].Width = 25;     // Number
            dgv_Outgoing_Messages.Columns[0].HeaderText = "#";
            dgv_Outgoing_Messages.Columns[1].Width = 65;     // Received_Time
            dgv_Outgoing_Messages.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages.Columns[1].HeaderText = "Time";
            dgv_Outgoing_Messages.Columns[1].DefaultCellStyle.Format = "HH:mm:ss";
            dgv_Outgoing_Messages.Columns[2].Width = 30;     // Size
            dgv_Outgoing_Messages.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages.Columns[2].HeaderText = "Size";
            dgv_Outgoing_Messages.Columns[3].Width = 30;     // Sent
            dgv_Outgoing_Messages.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages.Columns[4].Width = 32;     // Acknowledged
            dgv_Outgoing_Messages.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Outgoing_Messages.Columns[4].HeaderText = "Ackd";
            dgv_Outgoing_Messages.Columns[5].Visible = false;
            dgv_Outgoing_Messages.CurrentCell = dgv_Outgoing_Messages[0, Outgoing_Messages.Count - 1];  // 7/18/17
            dgv_Outgoing_Messages.CurrentCell.Selected = true;          // 7/18/17
            dgv_Outgoing_Messages.Update();

            // put the new message in the Selected message box - 7/18/17
            int Msg_Number = Outgoing_Messages[dgv_Outgoing_Messages.CurrentRow.Index].Message_Number;      // 7/18/17
            SetTBtext(tb_Aid_Selected_Message, Outgoing_Messages[Msg_Number - 1].Message_string);       // 7/18/17
        }

        private void dgv_Incoming_Messages_SelectionChanged(object sender, EventArgs e)
        {
            // has the dgv been populated?
            if (dgv_Incoming_Messages.CurrentRow != null)
            {
                // unhighlight the Outgoing dgv
                dgv_Outgoing_Messages.ClearSelection();
                dgv_Outgoing_Messages.Update();

                // highlight the new one
                // this creates Stack overflow                dgv_Incoming_Messages.CurrentCell.Selected = true;

                // determine position in the list
                int Msg_Number = Incoming_Messages[dgv_Incoming_Messages.CurrentRow.Index].Message_Number;

                // get the message from the Incoming message list
                tb_Aid_Selected_Message.Text = Incoming_Messages[Msg_Number - 1].Message_string;
            }
        }

        private void dgv_Outgoing_Messages_SelectionChanged(object sender, EventArgs e)
        {
            // has the dgv been populated?
            if (dgv_Outgoing_Messages.CurrentRow != null)
            {
                // unhighlight the Incoming dgv
                dgv_Incoming_Messages.ClearSelection();
                dgv_Incoming_Messages.Update();

                // highlight the new one
                // this creates Stack overflow                dgv_Outgoing_Messages.CurrentCell.Selected = true;

                // determine position in the list
                int Msg_Number = Outgoing_Messages[dgv_Outgoing_Messages.CurrentRow.Index].Message_Number;

                // get the message from the Outgoing message list
                tb_Aid_Selected_Message.Text = Outgoing_Messages[Msg_Number - 1].Message_string;
            }
        }

//// 7/18/17        private void dgv_Incoming_Messages_DoubleClick(object sender, EventArgs e)
//        {
//            // unhighlight the Outgoing dgv
//            dgv_Outgoing_Messages.ClearSelection();
//            dgv_Outgoing_Messages.Update();

//            // determine position in the list
//            int Msg_Number = Incoming_Messages[dgv_Incoming_Messages.CurrentRow.Index].Message_Number;

//            // get the message from the Outgoing Messages file
//            //            tb_Selected_Message.Text = Retrieve_Message(Outgoing_Message_Filename, Msg_Number);

//            // get the message from the Incoming message list
//            tb_Selected_Message.Text = Incoming_Messages[Msg_Number - 1].Message_string;
//        }

//// 7/18/17        private void dgv_Outgoing_Messages_DoubleClick(object sender, EventArgs e)
//        {
//            // unhighlight the Incoming dgv
//            dgv_Incoming_Messages.ClearSelection();
//            dgv_Incoming_Messages.Update();

//            // determine position in the list
//            int Msg_Number = Outgoing_Messages[dgv_Outgoing_Messages.CurrentRow.Index].Message_Number;

//            // get the message from the Outgoing Messages file
//            //            tb_Selected_Message.Text = Retrieve_Message(Outgoing_Message_Filename, Msg_Number);

//            // get the message from the Outgoing message list
//            tb_Selected_Message.Text = Outgoing_Messages[Msg_Number - 1].Message_string;
//        }

        private string Retrieve_Message(string Message_Filename, int Msg_Number)
        {
            StreamReader reader = StreamReader.Null;
            FileStream fs;
            string line;
            string msgtext = string.Empty;
            char[] splitter = new char[] { ',' };
            string[] Parts = null;

            // open the file
            fs = new FileStream(Message_Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            reader = new StreamReader(fs);

            // search for the message number
            bool not_numb = true;
            while (not_numb)
            {
                line = reader.ReadLine();   // read the message number, time and size line
                Parts = line.Split(splitter);
                if (Convert.ToInt16(Parts[0]) == Msg_Number)
                    not_numb = false;
                else
                {       // move to next message number
                    while (line != "</-t")
                        line = reader.ReadLine();
                }
            }

            // read in the message
            line = reader.ReadLine();   // read the start of text line
            if (line != "</t")
            {
                int x = 4;  // error - set breakpoint here
            }
            bool text = true;
            while (text)
            {
                line = reader.ReadLine();   // read next line
                if (line == "</-t")         // test if end of text
                    text = false;           // stop looking
                else
                    msgtext += line + Environment.NewLine;        // add to text to return
            }
            if (Convert.ToInt16(Parts[2]) != (msgtext.Length - 2))
            {
                int w = 2;  // error - set breakpoint
            }
            reader.Close();

            // return the text
            return msgtext;
        }

        #region CellEnter removed 7/18/17
        //        private void dgv_Incoming_Messages_CellEnter(object sender, DataGridViewCellEventArgs e)
        //        {
        //            // highlight Bib number cell
        //            Point rowcol = dgv_Incoming_Messages.CurrentCellAddress;
        //// 7/17/17            if (rowcol.X != 0)
        //// 7/17/17            {
        //                this.BeginInvoke(new MethodInvoker(() =>
        //                {
        //// 7/17/17                    Move_To_InnMess_Bib_Cell(rowcol.Y);
        //                    dgv_Incoming_Messages.CurrentCell = dgv_Incoming_Messages.Rows[rowcol.Y].Cells[0];     // 7/17/17
        //                    dgv_Incoming_Messages.CurrentCell.Selected = true;      // 7/17/17
        //                }));
        //// 7/17/17            }
        //        }

        //// 7/17/17        private void Move_To_InnMess_Bib_Cell(int index)
        //// 7/17/17        {
        //// 7/17/17            dgv_Incoming_Messages.CurrentCell = dgv_Incoming_Messages.Rows[index].Cells[0];
        //// 7/17/17        }

        //        private void dgv_Outgoing_Messages_CellEnter(object sender, DataGridViewCellEventArgs e)
        //        {
        //            // highlight Bib number cell
        //            Point rowcol = dgv_Outgoing_Messages.CurrentCellAddress;
        //// 7/17/17            if (rowcol.X != 0)
        //// 7/17/17            {
        //                this.BeginInvoke(new MethodInvoker(() =>
        //                {
        //// 7/17/17                    Move_To_OutMess_Bib_Cell(rowcol.Y);
        //                    dgv_Outgoing_Messages.CurrentCell = dgv_Outgoing_Messages.Rows[rowcol.Y].Cells[0];     // 7/17/17
        //                    dgv_Outgoing_Messages.CurrentCell.Selected = true;      // 7/17/17
        //                }));
        //// 7/17/17            }
        //        }

        //        // 7/17/17        private void Move_To_OutMess_Bib_Cell(int index)
        //        // 7/17/17        {
        //        // 7/17/17            dgv_Outgoing_Messages.CurrentCell = dgv_Outgoing_Messages.Rows[index].Cells[0];
        //        // 7/17/17        }
        #endregion

        private void dgv_Outgoing_Messages_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)  // 7/18/17 
        {
            if (e.RowIndex != -1)
                dgv_Outgoing_Messages.CurrentCell = dgv_Outgoing_Messages[0, e.RowIndex];
        }

        private void dgv_Incoming_Messages_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)  // 7/18/17 
        {
            if (e.RowIndex != -1)
                dgv_Incoming_Messages.CurrentCell = dgv_Incoming_Messages[0, e.RowIndex];
        }

        #endregion

        #region Settings tab
        #region Station Name Textbox
        private void tb_Station_Name_Settings_TextChanged(object sender, EventArgs e)
        {
            if ((tb_Station_Name_Settings.Text == "") || (tb_Station_Name_Settings.Text == "Not yet identified"))
                tb_Station_Name_Settings.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Station_Name_Settings.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Station_Name_Settings_Leave(object sender, EventArgs e)
        {
            if (tb_Station_Name.Text == "")
                lbl_Station_Name_notin_Station_List.Visible = false;    // also turn off this label
            else
                Test_Station_Name();
            tb_Station_Name.Text = tb_Station_Name_Settings.Text;
            Save_Registry("Station Name", tb_Station_Name_Settings.Text);
        }

        private void tb_Station_Name_Settings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tb_Station_Name_Settings_Leave(null, null);
            }
        }
        #endregion

        private void btn_Aid_Sounds_Click(object sender, EventArgs e)   // 8/14/17
        {
            Sounds sounds = new Sounds();
            sounds.Sounds_Directory = Sounds_Directory;
            sounds.Connections = Connections_Sound;
            sounds.File_Download = File_Download_Sound;
            sounds.Alerts = Alerts_Sound;
            sounds.Messages = Messages_Sound;
            DialogResult res = sounds.ShowDialog();
            if (res == DialogResult.OK)
            {
                Sounds_Directory = sounds.Sounds_Directory;
                Connections_Sound = sounds.Connections;
                File_Download_Sound = sounds.File_Download;
                Alerts_Sound = sounds.Alerts;
                Messages_Sound = sounds.Messages;
            }
        }

        private void cb_Use_RFID_Readers_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Use_RFID_Readers.Checked)
            {
                MakeVisible(gb_RFID_Readers, true);
                MakeVisible(btn_Load_RFID_Assignments, true);
                MakeVisible(btn_Browse_RFIDnumber_Assignment_file, true);
                MakeVisible(tb_RFIDnumber_Assignment_file, true);
                MakeVisible(lbl_RFIDAssign_local, true);
            }
            else
            {
                MakeVisible(gb_RFID_Readers, false);
                MakeVisible(btn_Load_RFID_Assignments, false);
                MakeVisible(btn_Browse_RFIDnumber_Assignment_file, false);
                MakeVisible(tb_RFIDnumber_Assignment_file, false);
                MakeVisible(lbl_RFIDAssign_local, false);
            }
        }

        private void rb_One_Log_Point_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_One_Log_Point.Checked)
            {
                rb_One_Entry_Point.Checked = true;
                One_Reader_Only = true;
                Save_Registry("Number of Log Points", "1");
                MakeVisible(lbl_Incoming_Reader_port, false);
                MakeVisible(lbl_Outgoing_Reader_port, false);
                MakeVisible(cmb_Outgoing, false);
                MakeVisible(lbl_One_Reader_port, true);
            }
            else
            {
                rb_In_and_Out_Entry.Checked = true;
                One_Reader_Only = false;
                Save_Registry("Number of Log Points", "2");
                MakeVisible(lbl_Incoming_Reader_port, true);
                MakeVisible(lbl_Outgoing_Reader_port, true);
                MakeVisible(cmb_Outgoing, true);
                MakeVisible(lbl_One_Reader_port, false);
            }
            // 7/21/16            if ((state != Server_State.Not_Initted) && (state != Server_State.Error_Connecting))
            // 7/21/16                Send_LogPts();  // except during initialization
            // 8/18/16            if (WorkerObject.state == Aid_Worker.Server_State.Connected_Active)
            // 8/18/16                Send_LogPts();  // only send while Connected and Active
        }

        #region Ethernet Subtab
        private void btn_Ping_Central_Click(object sender, EventArgs e)
        {
            // make sure an IP address has been specified
            if (tb_Aid_Mesh_IP_address.Text == "")
            {
                MessageBox.Show("Must enter IP address", "Missing IP address", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                //// first save the textboxes
                //Save_Registry("Mesh IP Address", tb_Aid_Mesh_IP_address.Text);
                //Save_Registry("Mesh Server Port #", tb_Aid_Server_Port_Number.Text);

                if (NetworkInterface.GetIsNetworkAvailable())       // make sure there is a Network available to connect to
                {
                    //    // change the color and text of the 'Attempting Connection' button
                    //    button_Att.Text = "Attempting connection";
                    //    button_Att.BackColor = Color.FromArgb(192,255,192);
                    //    button_Att.Update();

                    //    // turn off the Cannot button
                    //    button_Cannot.Visible = false;
                    //    button_Cannot.Update();
                    //    Application.DoEvents();     // give time for the button to change

                    //    // try to ping the ARMS unit first
                    //    if (ping_test(IP_Addr))
                    //        Connect_ARMS();

                    // claer the error message displayed
                    tb_Aid_Server_Error_Message.Text = "";

                    Ping pingSender = new Ping();

                    // Set options for transmission:
                    // The data can go through 64 gateways or routers
                    // before it is destroyed, and the data packet
                    // cannot be fragmented.
                    PingOptions options = new PingOptions(64, true);

                    // Create a buffer of 32 bytes of data to be transmitted.
                    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);

                    //                    // Wait 5 seconds for a reply.
                    // Wait 10 seconds for a reply.
                    //                    int timeout = 5000;
                    int timeout = 10000;

                    // set up an exception message capture area
                    System.Net.NetworkInformation.PingException except = new System.Net.NetworkInformation.PingException("Message");

                    // now try to ping
                    try
                    {
                        PingReply reply = pingSender.Send(tb_Aid_Mesh_IP_address.Text, timeout, buffer, options);
                        if (reply.Status == IPStatus.Success)
                        {
                            lbl_Central_Ping_Successful.Visible = true;
                            //                            lbl_Connected_Central_Database.Visible = true;
                            //                            btn_Ping_Central.Visible = false;
                            //                            btn_Load_RFID_Assignments.Visible = true;
                            //                            tb_RFIDnumber_Assignment_file.Visible = true;
                            //                            btn_Browse_RFIDnumber_Assignment_file.Visible = true;
                            return;
                        }
                    }
                    catch
                    {
                        String reason = string.Empty;
                        if (except.InnerException != null)
                        {
                            reason = except.InnerException.Message;
                        }
                        MessageBox.Show("Got an exception:" + reason);
                    }

                    // if not successful, tell the user
                    MessageBox.Show("Ping failed for this IP address!", "Ping unsuccessful", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    btn_Load_RFID_Assignments.Visible = false;
                    tb_RFIDnumber_Assignment_file.Visible = false;
                    btn_Browse_RFIDnumber_Assignment_file.Visible = false;
                    return;
                }
                else
                {
                    MessageBox.Show("No Network adapter or Network is available on this machine!", "No Newtork", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btn_Connect_to_Mesh_Server_Click(object sender, EventArgs e)
        {
            //// first save the textboxes
            //Save_Registry("Mesh IP Address", tb_Aid_Mesh_IP_address.Text);
            //Save_Registry("Mesh Server Port #", tb_Aid_Server_Port_Number.Text);

            // now attempt to connect
            IP_Server_Connect();
        }

        #region IP Address Textbox
        private void tb_Aid_Mesh_IP_address_TextChanged(object sender, EventArgs e)
        {
            if (tb_Aid_Mesh_IP_address.Text == "")
            {
                tb_Aid_Mesh_IP_address.BackColor = Color.FromArgb(255, 128, 128);
                Aid_IP_Good = false;
            }
            else
            {
                tb_Aid_Mesh_IP_address.BackColor = Color.FromKnownColor(KnownColor.Window);
                Aid_IP_Good = true;
            }
        }

        private void tb_Aid_Mesh_IP_address_Leave(object sender, EventArgs e)
        {
            //if (tb_Aid_Mesh_IP_address.Text == "")
            //    Aid_IP_Good = false;
            //else
            //    Aid_IP_Good = true;
            Save_Registry("Mesh IP Address", tb_Aid_Mesh_IP_address.Text);
            Test_IP_Port();
        }

        private void tb_Aid_Mesh_IP_address_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Aid_Mesh_IP_address_Leave(null, null);
        }
        #endregion

        private void Test_IP_Port()
        {
            if ((tb_Aid_Mesh_IP_address.Text != "") && (tb_Aid_Server_Port_Number.Text != ""))
                IP_Server_Connect();
        }

        #region Server Port # Textbox
        private void tb_Aid_Server_Port_Number_TextChanged(object sender, EventArgs e)
        {
            if (tb_Aid_Server_Port_Number.Text == "")
            {
                tb_Aid_Server_Port_Number.BackColor = Color.FromArgb(255, 128, 128);
                Aid_Server_Port_Good = false;
            }
            else
            {
                tb_Aid_Server_Port_Number.BackColor = Color.FromKnownColor(KnownColor.Window);
                Aid_Server_Port_Good = true;
            }
        }

        private void tb_Aid_Server_Port_Number_Leave(object sender, EventArgs e)
        {
            //if (tb_Aid_Server_Port_Number.Text == "")
            //    Aid_Server_Port_Good = false;
            //else
            //    Aid_Server_Port_Good = true;
            Save_Registry("Mesh Server Port #", tb_Aid_Server_Port_Number.Text);
            Test_IP_Port();
        }

        private void tb_Aid_Server_Port_Number_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Aid_Server_Port_Number_Leave(null, null);
        }
        #endregion

        private void cb_Server_Auto_Reconnect_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Server_Auto_Reconnect.Checked)
            {
                Server_Mesh_Auto_Reconnect = true;
                if (!Init_Registry)
                    Save_Registry("Mesh Auto Reconnect", "Yes");
            }
            else
            {
                Server_Mesh_Auto_Reconnect = false;
                if (!Init_Registry)
                    Save_Registry("Mesh Auto Reconnect", "No");
            }
        }

        private void lbl_14001_Click(object sender, EventArgs e)
        {
            string str = lbl_14001.Text;
            str = str.Remove(0, 1);     // remove the leading '('
            str = str.Remove(str.Length - 1, 1);    // remove the trailing ')'
// 3/29/19            tb_Aid_Server_Port_Number.Text = str;
            SetTBtext(tb_Aid_Server_Port_Number, str);  // 3/29/19
            tb_Aid_Server_Port_Number_Leave(null, null);    // 3/29/19
        }
        #endregion

        #region APRS Subtab
        #region Textboxes
        #region Latitude Textbox
        private void tb_APRS_Latitude_TextChanged(object sender, EventArgs e)
        {
            if (tb_APRS_Latitude.Text == "")
                tb_APRS_Latitude.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_APRS_Latitude.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_APRS_Latitude_Leave(object sender, EventArgs e)
        {
            if (tb_APRS_Latitude.Text == "")
                Latitude = string.Empty;
            else
                Latitude = tb_APRS_Latitude.Text;
            Save_Registry("Latitude", Latitude);
        }

        private void tb_APRS_Latitude_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_APRS_Latitude_Leave(null, null);
        }
        #endregion

        #region Longitude Textbox
        private void tb_APRS_Longitude_TextChanged(object sender, EventArgs e)
        {
            if (tb_APRS_Longitude.Text == "")
                tb_APRS_Longitude.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_APRS_Longitude.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_APRS_Longitude_Leave(object sender, EventArgs e)
        {
            if (tb_APRS_Longitude.Text == "")
                Longitude = string.Empty;
            else
                Longitude = tb_APRS_Longitude.Text;
            Save_Registry("Longitude", tb_APRS_Longitude.Text);
        }

        private void tb_APRS_Longitude_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_APRS_Longitude_Leave(null, null);
        }
        #endregion

        #region Network Textbox
        private void tb_APRS_Networkname_TextChanged(object sender, EventArgs e)
        {
            if (tb_APRS_Networkname.Text == "")
            {
                tb_APRS_Networkname.BackColor = Color.FromArgb(255, 128, 128);
//                tb_APRS_Networkname.Text = "(9 chars max)";
                MakeVisible(btn_APRS_Connect_to_DB, false);
            }
            else
            {
                tb_APRS_Networkname.Text = tb_APRS_Networkname.Text.ToUpper();
                tb_APRS_Networkname.SelectionStart = tb_APRS_Networkname.Text.Length;
                tb_APRS_Networkname.BackColor = Color.FromKnownColor(KnownColor.Window);
                MakeVisible(btn_APRS_Connect_to_DB, true);
            }
        }

        private void tb_APRS_Networkname_Leave(object sender, EventArgs e)
        {
            if (tb_APRS_Networkname.Text == "")
                APRSnetworkName = string.Empty;
            else
                APRSnetworkName = tb_APRS_Networkname.Text;
            Save_Registry("APRS Network Name", APRSnetworkName);
        }

        private void tb_APRS_Networkname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_APRS_Networkname_Leave(null, null);
        }
        #endregion
        #endregion

        #region List Download groupbox
        private void chk_Runner_List_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Runner_List.Checked)
            {
                Save_Registry("Load APRS Runners", "Yes");
                APRS_Load_Runner = true;
            }
            else
            {
                Save_Registry("Load APRS Runners", "No");
                APRS_Load_Runner = false;
            }
        }
 
        private void chk_DNS_List_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_DNS_List.Checked)
            {
                Save_Registry("Load APRS DNS", "Yes");
                APRS_Load_DNS = true;
            }
            else
            {
                Save_Registry("Load APRS DNS", "No");
                APRS_Load_DNS = false;
            }
        }
 
        private void chk_DNF_List_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_DNF_List.Checked)
            {
                Save_Registry("Load APRS DNF", "Yes");
                APRS_Load_DNF = true;
            }
            else
            {
                Save_Registry("Load APRS DNF", "No");
                APRS_Load_DNF = false;
            }
        }
 
        private void chk_Watch_List_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Watch_List.Checked)
            {
                Save_Registry("Load APRS Watch", "Yes");
                APRS_Load_Watch = true;
            }
            else
            {
                Save_Registry("Load APRS Watch", "No");
                APRS_Load_Watch = false;
            }
        }
 
        private void chk_Info_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Info.Checked)
            {
                Save_Registry("Load APRS Info", "Yes");
                APRS_Load_Info = true;
            }
            else
            {
                Save_Registry("Load APRS Info", "No");
                APRS_Load_Info = false;
            }
        }
        #endregion

        private void btn_APRS_Connect_to_DB_Click(object sender, EventArgs e)
        {
            // make sure connection has been made to the AGWPE server
            if (!WorkerObject.Connected_to_AGWServer)
            {
                MessageBox.Show("        AGWPE is not running yet!");
                return;
            }
            else
            {
                // make sure Network name has been specified
                if (tb_APRS_Networkname.Text == "")
                {
                    MessageBox.Show("Cannot use APRS to Connect to Central Database!\n\n                   Missing Network name!");
                    return;
                }
                else
                {
                    MakeVisible(btn_APRS_Disconnect_DB, true);
                    APRSconnect30secTimer.Stop();
                    APRSconnect2minTimer.Stop();

                    // send ?AZRT?
                    //                Aid_AGWSocket.TXdataUnproto(AGWPERadioPort, "BEACON", "?AZRT?");
                    //                Aid_AGWSocket.TXdataUnproto(AGWPERadioPort, APRSnetworkName, "?AZRT?");
                    Aid_AGWSocket.TXdataUnproto(AGWPERadioPort, "?AZRT?");

                    // start the 30 second timer
                    APRSconnect30secTimer.Start();
                }
            }
        }
        #endregion

        #region Packet Node Subtab
        private void rb_Connect_Packet_Direct_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Connect_Packet_Direct.Checked)
            {
                Packet_Connect_Mode = Packet_Connect_Method.Direct;
                Save_Registry("Packet Connect", "Direct");
                VIAstring = null;
            }
        }

        private void rb_Connect_Packet_Via_String_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Connect_Packet_Via_String.Checked)
            {
                Packet_Connect_Mode = Packet_Connect_Method.ViaString;
                Save_Registry("Packet Connect", "Via");
                VIAstring = tb_AGWPE_ViaString.Text;
// ???                Aid_AGWSocket.InitViaString();    // recalculate VIACount
            }
        }

        private void rb_Connect_Packet_Via_Node_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Connect_Packet_Via_Node.Checked)
            {
                Packet_Connect_Mode = Packet_Connect_Method.Node;
//                tb_Packet_Node_Callsign.Visible = true;
                MakeVisible(tb_Packet_Node_Callsign, true);
                Save_Registry("Packet Connect", "Node");
                VIAstring = null;
            }
            else
            {
                tb_Packet_Node_Callsign.Visible = false;
            }
//            Connected_to_Packet_Node = true;
        }

        private void btn_AGWPE_Connect_DB_Click(object sender, EventArgs e)
        {
            MakeVisible(btn_Aid_AGWPE_Disconnect, true);

            //switch (Connection_Type)
            //{
            //    case Connect_Medium.APRS:
            //        Send_APRS_Message(APRSnetworkName, tb_Station_Name.Text + " Activated");
            //        break;
            //    case Connect_Medium.Packet:
                    Aid_AGWSocket.Connect_to_Database();
            //        break;
            //}
        }

        private void btn_Aid_AGWPE_Disconnect_Click(object sender, EventArgs e)
        {
            AddtoLogFile("Attempting to Disconnect from Central Database");
            Aid_AGWSocket.Disconnect();
            while (Aid_AGWSocket.Connected_to_Database)
            {
                //                Thread.Sleep(1000);     // wait 1 sec.
                Thread.Sleep(5000);     // wait 5 sec.      // cannot Disconnect if already Disconnected - how do I tell?
                Aid_AGWSocket.Disconnect();
            }
            btn_Aid_AGWPE_Disconnect.Visible = false;
        }

        #region Packet Node Callsign Textbox
        private void tb_Packet_Node_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_Packet_Node_Callsign.Text == "")
            //{
            //    PacketNodeCallsign = string.Empty;
                tb_Packet_Node_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            //}
            else
            //{
            //    PacketNodeCallsign = tb_Packet_Node_Callsign.Text;
                tb_Packet_Node_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
            //}
            //Save_Registry("Packet Node Callsign", PacketNodeCallsign);
        }

        private void tb_Packet_Node_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_Packet_Node_Callsign.Text == "")
            //{
                PacketNodeCallsign = string.Empty;
            //    tb_Packet_Node_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            //}
            else
            //{
                PacketNodeCallsign = tb_Packet_Node_Callsign.Text;
            //    tb_Packet_Node_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
            //}
            Save_Registry("Packet Node Callsign", PacketNodeCallsign);
        }

        private void tb_Packet_Node_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Packet_Node_Callsign_Leave(null, null);
        }
        #endregion
        #endregion

        #region AGWPE Subtab
        #region buttons
        private void btn_Aid_AGWPE_Start_Refresh_Click(object sender, EventArgs e)
        {
            // first save the textboxes
            Save_Registry("AGWPE Radio Port", AGWPERadioPort.ToString());

//            // calculate the VIA string count again
//            Aid_AGWSocket.InitViaString();

            // now verify the callsigns have been entered
//            if ((tb_AidStation_FCC_Callsign.Text == "") || (tb_Database_FCC_Callsign.Text == ""))
            if (tb_AidStation_FCC_Callsign.Text == "")
            {
//                MessageBox.Show("Station or Database callsign is missing!", "Missing callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MessageBox.Show("Station callsign is missing!", "Missing callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // make the button invisible
            MakeVisible(btn_Aid_AGWPE_Start_Refresh, false);

            // now we can connect
            if (!Aid_AGWSocket.Connected_to_AGWserver)
            {
                if (Connection_Type == Connect_Medium.Packet)
                    Aid_AGWSocket.InitAGWPE(true);
                else
                    Aid_AGWSocket.InitAGWPE(false);
                while (Aid_AGWSocket.InitInProcess)
                    Application.DoEvents();       // wait for the Init process to finish
                Thread.Sleep(4000);     // wait four seconds, before getting the new settings
            }
            Get_Aid_AGWPE_Settings();
        }

        public static void Connect_to_AGWPE()
        {
            if (!Aid_AGWSocket.Connected_to_AGWserver)
            {
                if (Connection_Type == Connect_Medium.Packet)
                    Aid_AGWSocket.InitAGWPE(true);
                else
                    Aid_AGWSocket.InitAGWPE(false);
                while (Aid_AGWSocket.InitInProcess)
                    Application.DoEvents();       // wait for the Init process to finish
                Thread.Sleep(4000);     // wait four seconds, before getting the new settings
            }
        }

        bool MonitorSent = false;
        private void btn_Activate_Port_Monitor_Click(object sender, EventArgs e)
        {
            // make the button invisible
            //            btn_Activate_Port_Monitor.Visible = false;
            MakeVisible(btn_Activate_Port_Monitor, false);

            // make sure we do this only once
            if (!MonitorSent)
            {
                // read it
                GetPort();

                //// don't want to Monitor port                if (Form1.AGWPERadioPort == -1)
                //                    MessageBox.Show("AGWPE RadioPort = -1");
                //                else
                //                    Aid_AGWSocket.StartMonitoring(AGWPERadioPort);  // use it

                MonitorSent = true;
            }
        }

        //private void chk_AGWPE_Registered_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (chk_AGWPE_Registered.Checked)
        //        btn_AGWPE_Connect_DB_Click(null, null);
        //}
        #endregion

        private void chk_Use_Station_Tactical_Callsign_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Use_Station_Tactical_Callsign.Checked)
            {
                MakeVisible(tb_AidStation_Tactical_Callsign, true);
                MakeVisible(chk_Station_Tactical_Callsign_registered_with_AGWPE, true);
                MakeVisible(tb_AidStation_Tactical_Beacon_Text, true);
                MakeVisible(lbl_AidStation_Tactical_Beacon_Text, true);
                if (!Init_Registry)
                    Save_Registry("Use Station Tactical", "Yes");
            }
            else
            {
                MakeVisible(tb_AidStation_Tactical_Callsign, false);
                MakeVisible(chk_Station_Tactical_Callsign_registered_with_AGWPE, false);
                MakeVisible(tb_AidStation_Tactical_Beacon_Text, false);
                MakeVisible(lbl_AidStation_Tactical_Beacon_Text, false);
                if (!Init_Registry)
                    Save_Registry("Use Station Tactical", "No");
            }
        }

        private void chk_Use_Database_Tactical_Callsign_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Use_Database_Tactical_Callsign.Checked)
            {
                MakeVisible(tb_Database_Tactical_Callsign, true);
                MakeVisible(chk_Database_Tactical_Callsign_registered_with_AGWPE, true);
                if (!Init_Registry)
                    Save_Registry("Use Database Tactical", "Yes");
            }
            else
            {
                MakeVisible(tb_Database_Tactical_Callsign, false);
                MakeVisible(chk_Database_Tactical_Callsign_registered_with_AGWPE, false);
                if (!Init_Registry)
                    Save_Registry("Use Database Tactical", "No");
            }
        }

        private void cb_Aid_AGWPE_Connected_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Aid_AGWPE_Connected.Checked)
            {
//                btn_Aid_AGWPE_Start_Refresh.Visible = true;
                MakeVisible(btn_Aid_AGWPE_Start_Refresh, true);
//                btn_Aid_AGWPE_Start_Refresh.Text = "Refresh";
                SetCtlText(btn_Aid_AGWPE_Start_Refresh, "Refresh");
//                btn_Aid_AGWPE_Start_Refresh.BackColor = Color.FromArgb(255, 192, 192);
                SetCtlBackColor(btn_Aid_AGWPE_Start_Refresh, Color.FromArgb(255, 192, 192));
            }
            else
            {
                //                cb_Callsign_Registered.Checked = false;
//                btn_Aid_AGWPE_Start_Refresh.Text = "Start";
                SetCtlText(btn_Aid_AGWPE_Start_Refresh, "Start");
//                btn_Aid_AGWPE_Start_Refresh.BackColor = Color.FromArgb(128, 255, 128);
                SetCtlBackColor(btn_Aid_AGWPE_Start_Refresh, Color.FromArgb(128, 255, 128));
            }
        }

        #region Textboxes
        #region AGWPE Server Name Textbox
        private void tb_Aid_AGWPEServer_TextChanged(object sender, EventArgs e)
        {
            if (tb_Aid_AGWPEServer.Text == "")
                tb_Aid_AGWPEServer.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Aid_AGWPEServer.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Aid_AGWPEServer_Leave(object sender, EventArgs e)
        {
            if (tb_Aid_AGWPEServer.Text == "")
                AGWPEServerName = string.Empty;
            else
                AGWPEServerName = tb_Aid_AGWPEServer.Text;
            Save_Registry("AGWPE Server Name", AGWPEServerName);
        }

        private void tb_Aid_AGWPEServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Aid_AGWPEServer_Leave(null, null);
        }
        #endregion

        #region AGWPE Server Port Textbox
        private void tb_Aid_AGWPEPort_TextChanged(object sender, EventArgs e)
        {
            if (tb_Aid_AGWPEPort.Text == "")
            {
//                AGWPEServerPort = string.Empty;
                tb_Aid_AGWPEPort.BackColor = Color.FromArgb(255, 128, 128);
            }
            else
            {
  //              AGWPEServerPort = tb_Aid_AGWPEPort.Text;
                tb_Aid_AGWPEPort.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void tb_Aid_AGWPEPort_Leave(object sender, EventArgs e)
        {
            if (tb_Aid_AGWPEPort.Text == "")
                AGWPEServerPort = string.Empty;
            else
                AGWPEServerPort = tb_Aid_AGWPEPort.Text;
            Save_Registry("AGWPE Server Port", AGWPEServerPort);
        }

        private void tb_Aid_AGWPEPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Aid_AGWPEPort_Leave(null, null);
        }
        #endregion

        #region AGWPE VIAString Textbox
        private void tb_AGWPE_ViaString_TextChanged(object sender, EventArgs e)
        {
            if (tb_AGWPE_ViaString.Text == "")
                tb_AGWPE_ViaString.BackColor = Color.FromArgb(255, 128, 128);
            else
            {
                tb_AGWPE_ViaString.Text = tb_AGWPE_ViaString.Text.ToUpper();
                tb_AGWPE_ViaString.SelectionStart = tb_AGWPE_ViaString.Text.Length;
                tb_AGWPE_ViaString.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void tb_AGWPE_ViaString_Leave(object sender, EventArgs e)
        {
            if (tb_AGWPE_ViaString.Text == "")
                VIAstring = string.Empty;
            else
                VIAstring = tb_AGWPE_ViaString.Text;
            Save_Registry("VIA string", VIAstring);
        }

        private void tb_AGWPE_ViaString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_AGWPE_ViaString_Leave(null, null);
        }
        #endregion

        #region Station FCC Callsign Textbox
        private void tb_AidStation_FCC_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_AidStation_FCC_Callsign.Text == "")
                tb_AidStation_FCC_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            else
            {
                tb_AidStation_FCC_Callsign.Text = tb_AidStation_FCC_Callsign.Text.ToUpper();
                tb_AidStation_FCC_Callsign.SelectionStart = tb_AidStation_FCC_Callsign.Text.Length;
                tb_AidStation_FCC_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void tb_AidStation_FCC_Callsign_Enter(object sender, EventArgs e)
        {
            if (tb_AidStation_FCC_Callsign.Text == "(9 chars max)")
                tb_AidStation_FCC_Callsign.Text = "";
        }

        private void tb_AidStation_FCC_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_AidStation_FCC_Callsign.Text == "")
                StationFCCCallsign = string.Empty;
            else
                StationFCCCallsign = tb_AidStation_FCC_Callsign.Text;
            Save_Registry("Station FCC Callsign", StationFCCCallsign);
        }

        private void tb_AidStation_FCC_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_AidStation_FCC_Callsign_Leave(null, null);
        }
        #endregion

        #region Station Tactical Callsign Textbox
        private void tb_AidStation_Tactical_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_AidStation_Tactical_Callsign.Text == "")
            {
                tb_AidStation_Tactical_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            }
            else
            {
                tb_AidStation_Tactical_Callsign.Text = tb_AidStation_Tactical_Callsign.Text.ToUpper();
                tb_AidStation_Tactical_Callsign.SelectionStart = tb_AidStation_Tactical_Callsign.Text.Length;
                tb_AidStation_Tactical_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void tb_AidStation_Tactical_Callsign_Enter(object sender, EventArgs e)
        {
            if (tb_AidStation_Tactical_Callsign.Text == "(9 chars max)")
                tb_AidStation_Tactical_Callsign.Text = "";
        }

        private void tb_AidStation_Tactical_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_AidStation_Tactical_Callsign.Text == "")
                StationTacticalCallsign = string.Empty;
            else
                StationTacticalCallsign = tb_AidStation_Tactical_Callsign.Text;
            Save_Registry("Station Tactical Callsign", StationTacticalCallsign);
        }

        private void tb_AidStation_Tactical_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_AidStation_Tactical_Callsign_Leave(null, null);
        }
        #endregion

        #region Station Tactical Beacon Textbox
        private void tb_AidStation_Tactical_Beacon_Text_TextChanged(object sender, EventArgs e)
        {
            if (tb_AidStation_Tactical_Beacon_Text.Text == "")
                tb_AidStation_Tactical_Beacon_Text.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_AidStation_Tactical_Beacon_Text.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_AidStation_Tactical_Beacon_Text_Enter(object sender, EventArgs e)
        {
            if (tb_AidStation_Tactical_Beacon_Text.Text == "(include FCC Callsign)")
                tb_AidStation_Tactical_Beacon_Text.Text = "";
        }

        private void tb_AidStation_Tactical_Beacon_Text_Leave(object sender, EventArgs e)
        {
            
            if (tb_AidStation_Tactical_Beacon_Text.Text == "")
                TacticalBeaconText = string.Empty;
            else
                TacticalBeaconText = tb_AidStation_Tactical_Beacon_Text.Text;
            Save_Registry("Station Tactical Beacon Text", TacticalBeaconText);
        }

        private void tb_AidStation_Tactical_Beacon_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_AidStation_Tactical_Beacon_Text_Leave(null, null);
        }
        #endregion

        #region Database FCC Callsign Textbox
        private void tb_Database_FCC_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_Database_FCC_Callsign.Text == "")
                tb_Database_FCC_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            else
            {
                tb_Database_FCC_Callsign.Text = tb_Database_FCC_Callsign.Text.ToUpper();
                tb_Database_FCC_Callsign.SelectionStart = tb_Database_FCC_Callsign.Text.Length;
                tb_Database_FCC_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void tb_Database_FCC_Callsign_Enter(object sender, EventArgs e)
        {
            if (tb_Database_FCC_Callsign.Text == "(9 chars max)")
                tb_Database_FCC_Callsign.Text = "";
        }

        private void tb_Database_FCC_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_Database_FCC_Callsign.Text == "")
                DatabaseFCCCallsign = string.Empty;
            else
                DatabaseFCCCallsign = tb_Database_FCC_Callsign.Text;
            Save_Registry("Database FCC Callsign", DatabaseFCCCallsign);
        }

        private void tb_Database_FCC_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Database_FCC_Callsign_Leave(null, null);
        }
        #endregion

        #region Database Tactical Callsign Textbox
        private void tb_Database_Tactical_Callsign_TextChanged(object sender, EventArgs e)
        {
            if (tb_Database_Tactical_Callsign.Text == "")
                tb_Database_Tactical_Callsign.BackColor = Color.FromArgb(255, 128, 128);
            else
            {
                tb_Database_Tactical_Callsign.Text = tb_Database_Tactical_Callsign.Text.ToUpper();
                tb_Database_Tactical_Callsign.SelectionStart = tb_Database_Tactical_Callsign.Text.Length;
                tb_Database_Tactical_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void tb_Database_Tactical_Callsign_Enter(object sender, EventArgs e)
        {
            if (tb_Database_Tactical_Callsign.Text == "(9 chars max)")
                tb_Database_Tactical_Callsign.Text = "";
        }

        private void tb_Database_Tactical_Callsign_Leave(object sender, EventArgs e)
        {
            if (tb_Database_Tactical_Callsign.Text == "")
                DatabaseTacticalCallsign = string.Empty;
            else
                DatabaseTacticalCallsign = tb_Database_Tactical_Callsign.Text;
            Save_Registry("Database Tactical Callsign", DatabaseTacticalCallsign);
        }

        private void tb_Database_Tactical_Callsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tb_Database_Tactical_Callsign_Leave(null, null);
        }
        #endregion
        #endregion

        private void lbl_Aid_localhost_Click(object sender, EventArgs e)
        {
            string str = lbl_localhost.Text;
            str = str.Remove(0, 1);     // remove the leading '('
            str = str.Remove(str.Length - 1, 1);    // remove the trailing ')'
            tb_Aid_AGWPEServer.Text = str;
        }

        private void lbl_Aid_8000_Click(object sender, EventArgs e)
        {
            string str = lbl_8000.Text;
            str = str.Remove(0, 1);     // remove the leading '('
            str = str.Remove(str.Length - 1, 1);    // remove the trailing ')'
            tb_Aid_AGWPEPort.Text = str;
        }

        private void Get_Aid_AGWPE_Settings()
        {
            MakeVisible(btn_Aid_AGWPE_Start_Refresh, true);

            // test if connected to the AGW server
            if (Aid_AGWSocket.Connected_to_AGWserver)
            {
                MakeCBChecked(chk_Aid_AGWPE_Connected, true);
                MakeVisible(btn_AGWPE_Connect_DB, true);
                SetTBtext(tb_Aid_Num_AGWPE_Radio_Ports, Aid_AGWSocket.Ports.Num.ToString());
                if (Form1.Aid_AGWSocket.Ports.Num != 0)
                {
                    Add_Aid_Portnames();
                }
            }
            else
            {
                MakeCBChecked(chk_Aid_AGWPE_Connected, false);
                MakeVisible(btn_AGWPE_Connect_DB, false);
            }
            //SetTBtext(tb_Aid_AGWPEServer, AGWPEServerName);
            //SetTBtext(tb_Aid_AGWPEPort, AGWPEServerPort);
            SetTBtext(tb_Aid_AGWPE_Version, Aid_AGWSocket.Version);

            // test if connected to the Database
//            MakeChecked(chk_AGWPE_Registered, Aid_AGWSocket.Registered);
            MakeCBChecked(chk_Station_Callsign_registered_with_AGWPE, Aid_AGWSocket.Registered);
            MakeCBChecked(chk_Station_Tactical_Callsign_registered_with_AGWPE, Aid_AGWSocket.Tactical_Registered);
            if (Aid_AGWSocket.Connected_to_Database)
            {
            }
            else
            {
                // try to connect again
            }
            //SetTBtext(tb_AidStation_FCC_Callsign, StationFCCCallsign);
            //SetTBtext(tb_AidStation_Tactical_Callsign, StationTacticalCallsign);
            //SetTBtext(tb_Database_FCC_Callsign, DatabaseFCCCallsign);
            //SetTBtext(tb_Database_Tactical_Callsign, DatabaseTacticalCallsign);
            //SetTBtext(tb_AGWPE_ViaString, VIAstring);

            // update the selected Radio port
            lb_Aid_AGWPE_Radio_Ports_SelectedIndexChanged(null, null);
        }

        private void lb_Aid_AGWPE_Radio_Ports_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetPort();

            // make the Activate button visible
// removed 3/24/16            MakeVisible(btn_Activate_Port_Monitor, true);
        }

        private void GetPort()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lb_Aid_AGWPE_Radio_Ports.InvokeRequired)
            {
                GetAGWPE_PortDel d = new GetAGWPE_PortDel(GetPort);
                lb_Aid_AGWPE_Radio_Ports.Invoke(d, new object[] { });
            }
            else
            {
                AGWPERadioPort = lb_Aid_AGWPE_Radio_Ports.SelectedIndex;
            }
        }

        public void Add_Aid_Portnames()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lb_Aid_AGWPE_Radio_Ports.InvokeRequired)
            {
                Add_Portnamesdel d = new Add_Portnamesdel(Add_Aid_Portnames);
                lb_Aid_AGWPE_Radio_Ports.Invoke(d, new object[] { });
            }
            else
            {
                lb_Aid_AGWPE_Radio_Ports.Items.Clear();
                for (int i = 0; i < Form1.Aid_AGWSocket.Ports.Num; i++)
                {
                    string portname = Form1.Aid_AGWSocket.Ports.Pdata[i].PortName;
                    lb_Aid_AGWPE_Radio_Ports.Items.Add(portname);
                }
                // highlight the selected port, if it is valid
                if (AGWPERadioPort < lb_Aid_AGWPE_Radio_Ports.Items.Count)
                    lb_Aid_AGWPE_Radio_Ports.SelectedIndex = AGWPERadioPort;
                lb_Aid_AGWPE_Radio_Ports.Update();
            }
        }
        #endregion

        #region AGWPE Statistics subtab
        /*
         * 
         * The format of the 'g' frame would be:
 
        Field             Length           Meaning
        AGWPE Port        1 Bytes        Port being queried
                                        0x00 Port1
                                        0x01 Port2 ….
        Reserved        3 Bytes        0x00 0x00 0x00
        DataKind        1 Byte        ‘g’ (ASCII 0x67)
        Reserved        1 Byte        0x00
        PID             1 Byte        0x00
        Reserved        1 Byte        0x00
        CallFrom        10 Bytes        10 0x00
        CallTo        10 Bytes        10 0x00
        DataLen        4 Bytes        12
        User (Reserved)        4 Bytes        0
  
        12 bytes of data would follow (as indicated by the DataLen field) containing the following information about the particular port referenced by the header’s AGWPEPort field :
  
        Offset (Byte or Characters) into the Data Area        Meaning
        +00                 On air baud rate (0=1200/1=2400/2=4800/3=9600…)
        +01                 Traffic level (if 0xFF the port is not in autoupdate mode)
        +02                 TX Delay
        +03                 TX Tail
        +04                 Persist
        +05                 SlotTime
        +06                 MaxFrame
        +07                 How Many connections are active on this port
        +08 LSB Low Word    HowManyBytes (received in the last 2 minutes) as a 32 bits
        +09 MSB Low Word     (4 bytes) integer. Updated every two minutes.
        +10 LSB High Word
        +11 MSB High Word
         * 
         */

        void DisplayAidAGWPEportStats(AGWPEPortStat Stats)
        {
            string statstr;
            switch (Stats.BaudRate)
            {
                case 0:
                    statstr = "1200";
                    break;
                case 1:
                    statstr = "2400";
                    break;
                case 2:
                    statstr = "4800";
                    break;
                case 3:
                    statstr = "9600";
                    break;
                default:
                    statstr = "none";
                    break;
            }
            SetTBtext(tb_Port_Baudrate, statstr);
            SetTBtext(tb_Port_TrafficLevel, Stats.TrafficLevel.ToString());
            SetTBtext(tb_Port_TxDelay, Stats.TxDelay.ToString());
            SetTBtext(tb_Port_TxTail, Stats.TxTail.ToString());
            SetTBtext(tb_Port_Persist, Stats.Persist.ToString());
            SetTBtext(tb_Port_SlotTime, Stats.SlotTime.ToString());
            SetTBtext(tb_Port_MaxFrame, Stats.MaxFrame.ToString());
            SetTBtext(tb_Port_NumConnections, Stats.NumConnections.ToString());
            SetTBtext(tb_Port_NumBytesReceived, Stats.NumBytesReceived.ToString());
            SetTBtext(tb_Port_NumPendPortFrames, Stats.NumPendingPortFrames.ToString());
            SetTBtext(tb_Port_NumPendConnectFrames, Stats.NumPendingConnectionFrames.ToString());
        }

        private void btn_Aid_AGWPEStats_Refresh_Click(object sender, EventArgs e)
        {
            DisplayAidAGWPEportStats(AGWPEPortStatistics);
        }
        #endregion

        #region Connection Medium groupbox
        private void btn_Change_Medium_Click(object sender, EventArgs e)
        {
            // start to prepare the message for the user
            string existing = string.Empty;
            switch (Connection_Type)
            {
                case Connect_Medium.Ethernet:
                    existing = "Ethernet/MESH";
                    break;
                case Connect_Medium.Packet:
                    existing = "Packet";
                    break;
                case Connect_Medium.APRS:
                    existing = "APRS";
                    break;
                case Connect_Medium.Cellphone:
                    existing = "Cellphone";
                    break;
            }

            Connect_Medium New_Connection_Type = Connect_Medium.Cellphone;
            // determine which radio button is selected
            if (rb_Use_Ethernet.Checked)
                New_Connection_Type = Connect_Medium.Ethernet;
            else
                if (rb_Use_Packet.Checked)
                    New_Connection_Type = Connect_Medium.Packet;
                else
                    if (rb_Use_APRS.Checked)
                        New_Connection_Type = Connect_Medium.APRS;
                    else
                        if (rb_Use_Cellphone.Checked)
                            New_Connection_Type = Connect_Medium.Cellphone;

            // finish preparing the message for the user
            string New = string.Empty;
            switch (New_Connection_Type)
            {
                case Connect_Medium.Ethernet:
                    New = "Ethernet/MESH";
                    break;
                case Connect_Medium.Packet:
                    New = "Packet";
                    break;
                case Connect_Medium.APRS:
                    New = "APRS";
                    break;
                case Connect_Medium.Cellphone:
                    New = "Cellphone";
                    break;
            }

            // put them all together
            string message = "You are about to change      " + Environment.NewLine + Environment.NewLine;
            message += "from " + existing + " to " + New + "    " + Environment.NewLine + Environment.NewLine;
            message += "             Continue?";

            // ask the question
            DialogResult result = MessageBox.Show(message, "Change Medium?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {

                // To Do:
                // terminate previous worker, closing any connections
                // start new worker
                // if AGWPE not loaded, ask

                // Start AGWPE if it is available and needed - first test if the socket is already created
                if ((Connection_Type == Connect_Medium.Packet) || (Connection_Type == Connect_Medium.APRS))
                {
                    Aid_AGWSocket = new Aid_AGWSocket(rtb_DB_Packet_Packets, DBRole);
                    Aid_AGWSocket.Connect_Button = btn_AGWPE_Connect_DB;
                    Aid_AGWSocket.Disconnect_Button = btn_Aid_AGWPE_Disconnect;
                }

                // attempt to connect

            }


            //// this will be saved for later:

            //            // disconnect the previous connection mode

            //            // determine which radio button is selected
            //            if (rb_Use_APRS.Checked)
            //                Connection_Type = Connect_Medium.APRS;
            //            if (rb_Use_Packet.Checked)
            //                Connection_Type = Connect_Medium.Packet;
            //            if (rb_Use_Ethernet.Checked)
            //                Connection_Type = Connect_Medium.Ethernet;
            //            if (rb_Use_Cellphone.Checked)
            //                Connection_Type = Connect_Medium.Cellphone;

            //            // Start AGWPE if it is available and needed
            //            if ((Connection_Type == Connect_Medium.Packet) || (Connection_Type == Connect_Medium.APRS))
            //            {
            //                AGWSocket = new AGWSocket(rtb_Packet_Node_Packets);
            //                AGWSocket.Connect_Button = btn_AGWPE_Connect_DB;
            //            }

            //            // make the Connection mode label visible, put the needed tabpages into the tabcontrol and start the connection
            //            Labels_TabPages_Connections();

            btn_Change_Medium.Visible = false;
        }

        private void rb_Aid_APRS_CheckedChanged(object sender, EventArgs e)
        {
            btn_Change_Medium.Visible = true;
        }

        private void rb_Aid_Packet_CheckedChanged(object sender, EventArgs e)
        {
            btn_Change_Medium.Visible = true;
        }

        private void rb_Ethernet_CheckedChanged(object sender, EventArgs e)
        {
            btn_Change_Medium.Visible = true;
        }

        private void rb_Cellphone_CheckedChanged(object sender, EventArgs e)
        {
            btn_Change_Medium.Visible = true;
        }
        #endregion

        #region Databases groupbox
        private void tb_RFIDnumber_Assignment_file_TextChanged(object sender, EventArgs e)
        {
            if (tb_RFIDnumber_Assignment_file.Text == "")
                tb_RFIDnumber_Assignment_file.BackColor = Color.FromArgb(255, 128, 128);
            else
            {
                tb_RFIDnumber_Assignment_file.BackColor = Color.FromKnownColor(KnownColor.Window);

                // load the file for lookup
            }
        }

        private void tb_Root_Directory_TextChanged(object sender, EventArgs e)
        {
            if (tb_Data_Directory.Text == "")
                tb_Data_Directory.BackColor = Color.FromArgb(255, 128, 128);
            else
            {
                tb_Data_Directory.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        private void btn_Load_RFID_Assignments_Click(object sender, EventArgs e)
        {
            if (tb_RFIDnumber_Assignment_file.Text != "")
            {
                // save the new name
                Save_Registry("RFID Assignments File", tb_RFIDnumber_Assignment_file.Text);
            }

            // load the file data
            Load_Aid_RFID_Assignments(tb_RFIDnumber_Assignment_file.Text);

            //// put the data into the DataGridView
            //if (Stations.Count != 0)
            //{
            //    Bind_DGV();
            //}
        }

        private void btn_Browse_RFIDnumber_Assignment_file_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tb_RFIDnumber_Assignment_file.Text = ofd.FileName;
            }
        }

        private void btn_Browse_Root_Directory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog findFolder = new FolderBrowserDialog();
            findFolder.RootFolder = Environment.SpecialFolder.MyDocuments;
            findFolder.ShowNewFolderButton = true;

            if (findFolder.ShowDialog() == DialogResult.OK)
            {
                tb_Data_Directory.Text = findFolder.SelectedPath;
            }
        }

        bool Load_Aid_RFID_Assignments(string FileName)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            StreamReader reader;

            // do this only if the FileName is not empty
            if (FileName != "")
            {
                try
                {
                    reader = File.OpenText(FileName);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                RFIDAssignments.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        RFID rfid = new RFID();
                        rfid.String = Parts[0].TrimStart(front);
                        rfid.RunnerNumber = Convert.ToInt16(Parts[1]);
                        RFIDAssignments.Add(rfid);
                    }
                }
                lbl_RFID_Assignments_file_Loaded.Visible = true;
            }
            return true;
        }

        bool Save_Aid_RFID_Assignents(List<RFID> rfid, string FileName)
        {
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (rfid.Count == 0)
            {
                MessageBox.Show("Station List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                                        FileName +
                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                else
                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save each station
            foreach (RFID rfidd in rfid)
            {
                // Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station, Next station,
                // distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)
                string line = rfidd.String + ",";
                line += rfidd.RunnerNumber.ToString();
                writer.WriteLine(line);
            }
            writer.Close();

            return true;
        }

        private void btn_Load_RFID_Assignment_from_Central_Click(object sender, EventArgs e)
        {
            SendCommand(Commands.RequestRFIDAssignments, "");

            // now wait for response from Central
        }

        private void btn_Save_Root_Directory_Click(object sender, EventArgs e)
        {
            Save_Registry("Program Root Directory", tb_Data_Directory.Text);
        }
        #endregion
        #endregion

        #region Interfaces tab
        //private void rb_Use_Mesh_Network_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (rb_Use_Mesh_Network.Checked)
        //    {
        //        gb_Mesh_Network.Visible = true;
        //        gb_AGWPE.Visible = false;
        //    }
        //    else
        //    {
        //        gb_Mesh_Network.Visible = false;
        //        gb_AGWPE.Visible = true;
        //        rb_Use_Packet.Checked = true;
        //    }
        //}

        #region RFID Readers groupbox
        #endregion

        //        #region AGWPE tab
        //        private void btn_AGWPE_Start_Refresh_Click(object sender, EventArgs e)
        //        {
        //            btn_AGWPE_Start_Refresh.Visible = false;
        //            btn_AGWPE_Start_Refresh.Update();

        //            // first save the textboxes
        //            Save_Registry("AGWPE Radio Port", AGWPERadioPort.ToString());
        //            Save_Registry("AGWPE Server Name", AGWPEServerName);
        //            Save_Registry("AGWPE Server Port", AGWPEServerPort);
        //            Save_Registry("Station Callsign", StationCallsign);
        //            Save_Registry("Database Callsign", DatabaseCallsign);
        //            Save_Registry("VIA string", VIAstring);

        //            // now verify the callsigns have been entered
        //            if ((tb_AGWPE_Station_Callsign.Text == "") || (tb_AGWPE_DB_Callsign.Text == ""))
        //            {
        //                MessageBox.Show("Station or Database callsign is missing!", "Missing callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //                return;
        //            }

        //            // now we can connect
        //            if (!AGWSocket.Connected_to_AGWserver)
        //            {
        //                AGWSocket.InitAGWPE();
        //                while (AGWSocket.InitInProcess)
        //                    ;       // wait for the Init process to finish
        //                Thread.Sleep(4000);     // wait four seconds, before getting the new settings
        //            }
        //            Get_AGWPE_Settings();
        //        }

        //        private void btn_AGWPE_Connect_DB_Click(object sender, EventArgs e)
        //        {
        //            AGWSocket.Connect_to_Database();    // if this button is clicked while we are the process
        //                                                // of trying to Connect, then it will disconnect
        //        }

        //        private void cb_AGWPE_Connected_CheckedChanged(object sender, EventArgs e)
        //        {
        //            if (cb_AGWPE_Connected.Checked)
        //            {
        //                btn_AGWPE_Start_Refresh.Visible = true;
        //                btn_AGWPE_Start_Refresh.Text = "Refresh";
        //                btn_AGWPE_Start_Refresh.BackColor = Color.FromArgb(255, 192, 192);
        //            }
        //            else
        //            {
        //                //                cb_Callsign_Registered.Checked = false;
        //                btn_AGWPE_Start_Refresh.Text = "Start";
        //                btn_AGWPE_Start_Refresh.BackColor = Color.FromArgb(128, 255, 128);
        //            }
        //        }

        //        private void tb_AGWPEServer_TextChanged(object sender, EventArgs e)
        //        {
        //            if (tb_AGWPEServer.Text == "")
        //            {
        //                AGWPEServerName = string.Empty;
        //                tb_AGWPEServer.BackColor = Color.FromArgb(255, 128, 128);
        //            }
        //            else
        //            {
        //                AGWPEServerName = tb_AGWPEServer.Text;
        //                tb_AGWPEServer.BackColor = Color.FromKnownColor(KnownColor.Window);
        //            }
        //        }

        //        private void tb_AGWPEPort_TextChanged(object sender, EventArgs e)
        //        {
        //            if (tb_AGWPEPort.Text == "")
        //            {
        //                AGWPEServerPort = string.Empty;
        //                tb_AGWPEPort.BackColor = Color.FromArgb(255, 128, 128);
        //            }
        //            else
        //            {
        //                AGWPEServerPort = tb_AGWPEPort.Text;
        //                tb_AGWPEPort.BackColor = Color.FromKnownColor(KnownColor.Window);
        //            }
        //        }

        //        private void tb_AGWPE_Station_Callsign_TextChanged(object sender, EventArgs e)
        //        {
        //            if (tb_AGWPE_Station_Callsign.Text == "")
        //            {
        //                StationCallsign = string.Empty;
        //                tb_AGWPE_Station_Callsign.BackColor = Color.FromArgb(255, 128, 128);
        //            }
        //            else
        //            {
        //                StationCallsign = tb_AGWPE_Station_Callsign.Text;
        //                tb_AGWPE_Station_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        //            }
        //        }

        //        private void tb_AGWPE_DB_Callsign_TextChanged(object sender, EventArgs e)
        //        {
        //            if (tb_AGWPE_DB_Callsign.Text == "")
        //            {
        //                DatabaseCallsign = string.Empty;
        //                tb_AGWPE_DB_Callsign.BackColor = Color.FromArgb(255, 128, 128);
        //            }
        //            else
        //            {
        //                DatabaseCallsign = tb_AGWPE_DB_Callsign.Text;
        //                tb_AGWPE_DB_Callsign.BackColor = Color.FromKnownColor(KnownColor.Window);
        //            }
        //        }

        //        private void tb_AGWPE_ViaString_TextChanged(object sender, EventArgs e)
        //        {
        //            if (tb_AGWPE_ViaString.Text == "")
        //            {
        //                VIAstring = string.Empty;
        //                tb_AGWPE_ViaString.BackColor = Color.FromArgb(255, 128, 128);
        //            }
        //            else
        //            {
        //                VIAstring = tb_AGWPE_ViaString.Text;
        //                tb_AGWPE_ViaString.BackColor = Color.FromKnownColor(KnownColor.Window);
        //            }
        //        }

        //        private void Get_AGWPE_Settings()
        //        {
        ////            btn_AGWPE_Start_Refresh.Visible = true;
        //            MakeVisible(btn_AGWPE_Start_Refresh, true);
        //            //tb_AGWPERadioports.Clear();
        //            //rb_AGWPE_Port1.Checked = false;
        //            //rb_AGWPE_Port2.Checked = false;
        //            //rb_AGWPE_Port3.Checked = false;
        //            //rb_AGWPE_Port4.Checked = false;

        //            // test if connected to the AGW server
        //            if (Form1.AGWSocket.Connected_to_AGWserver)
        //            {
        ////                cb_AGWPE_Connected.Checked = true;
        //                MakeChecked(cb_AGWPE_Connected, true);
        ////                btn_AGWPE_Connect_DB.Visible = true;
        //                MakeVisible(btn_AGWPE_Connect_DB, true);
        ////                tb_Num_AGWPE_Radio_Ports.Text = AGWSocket.Ports.Num.ToString();
        //                SetTBtext(tb_Num_AGWPE_Radio_Ports, AGWSocket.Ports.Num.ToString());
        //                if (Form1.AGWSocket.Ports.Num != 0)
        //                {
        //                    Add_Portnames();
        ////                    for (int i = 0; i < Form1.AGWSocket.Ports.Num; i++)
        ////                    {
        ////                        string portname = Form1.AGWSocket.Ports.Pdata[i].PortName;
        ////                        //tb_AGWPERadioports.Text += portname + Environment.NewLine;
        //////                        lb_AGWPE_Radio_Ports.Items.Add(portname);
        ////                        Add_Portname(portname);
        ////                    }
        ////                    //switch (AGWPERadioPort)
        ////                    //{
        ////                    //    case 0:
        ////                    //        rb_AGWPE_Port1.Checked = true;
        ////                    //        break;
        ////                    //    case 1:
        ////                    //        rb_AGWPE_Port2.Checked = true;
        ////                    //        break;
        ////                    //    case 2:
        ////                    //        rb_AGWPE_Port3.Checked = true;
        ////                    //        break;
        ////                    //    case 3:
        ////                    //        rb_AGWPE_Port4.Checked = true;
        ////                    //        break;
        ////                    //}
        ////                    lb_AGWPE_Radio_Ports.SelectedIndex = AGWPERadioPort;
        //                }
        //            }
        //            else
        //            {
        ////                cb_AGWPE_Connected.Checked = false;
        //                MakeChecked(cb_AGWPE_Connected, false);
        ////                btn_AGWPE_Connect_DB.Visible = false;
        //                MakeVisible(btn_AGWPE_Connect_DB, false);
        //            }
        ////            tb_AGWPEServer.Text = Form1.AGWPEServerName;    // also show the Server name and Server Port
        //            SetTBtext(tb_AGWPEServer, Form1.AGWPEServerName);
        ////            tb_AGWPEPort.Text = Form1.AGWPEServerPort;
        //            SetTBtext(tb_AGWPEPort, Form1.AGWPEServerPort);
        ////            tb_AGWPE_Version.Text = Form1.AGWSocket.Version;
        //            SetTBtext(tb_AGWPE_Version, Form1.AGWSocket.Version);

        //            // test if connected to the Database
        ////            chk_AGWPE_Registered.Checked = AGWSocket.Registered;
        //            MakeChecked(chk_AGWPE_Registered, AGWSocket.Registered);
        //            if (Form1.AGWSocket.Connected_to_Database)
        //            {
        //            }
        //            else
        //            {
        //                // try to connect again
        //            }
        ////            tb_AGWPE_Station_Callsign.Text = Form1.StationCallsign;
        //            SetTBtext(tb_AGWPE_Station_Callsign, Form1.StationCallsign);
        ////            tb_AGWPE_DB_Callsign.Text = Form1.DatabaseCallsign;
        //            SetTBtext(tb_AGWPE_DB_Callsign, Form1.DatabaseCallsign);
        ////            tb_AGWPE_ViaString.Text = Form1.VIAstring;
        //            SetTBtext(tb_AGWPE_ViaString, Form1.VIAstring);
        //        }

        //        private void lb_AGWPE_Radio_Ports_SelectedIndexChanged(object sender, EventArgs e)
        //        {
        //            AGWPERadioPort = lb_AGWPE_Radio_Ports.SelectedIndex;
        //        }

        //        //private void rb_AGWPE_Port1_CheckedChanged(object sender, EventArgs e)
        //        //{
        //        //    if (AGWSocket.Connected_to_AGWserver)
        //        //    {
        //        //        if (rb_AGWPE_Port1.Checked)
        //        //            AGWPERadioPort = 0;
        //        //    }
        //        //    else
        //        //    {
        //        //        rb_AGWPE_Port1.Checked = false;
        //        //        rb_AGWPE_Port2.Checked = false;
        //        //        rb_AGWPE_Port3.Checked = false;
        //        //        rb_AGWPE_Port4.Checked = false;
        //        //    }
        //        //}

        //        //private void rb_AGWPE_Port2_CheckedChanged(object sender, EventArgs e)
        //        //{
        //        //    if (AGWSocket.Connected_to_AGWserver)
        //        //    {
        //        //        if (rb_AGWPE_Port2.Checked)
        //        //            AGWPERadioPort = 1;
        //        //    }
        //        //    else
        //        //    {
        //        //        rb_AGWPE_Port1.Checked = false;
        //        //        rb_AGWPE_Port2.Checked = false;
        //        //        rb_AGWPE_Port3.Checked = false;
        //        //        rb_AGWPE_Port4.Checked = false;
        //        //    }
        //        //}

        //        //private void rb_AGWPE_Port3_CheckedChanged(object sender, EventArgs e)
        //        //{
        //        //    if (AGWSocket.Connected_to_AGWserver)
        //        //    {
        //        //        if (rb_AGWPE_Port3.Checked)
        //        //            AGWPERadioPort = 2;
        //        //    }
        //        //    else
        //        //    {
        //        //        rb_AGWPE_Port1.Checked = false;
        //        //        rb_AGWPE_Port2.Checked = false;
        //        //        rb_AGWPE_Port3.Checked = false;
        //        //        rb_AGWPE_Port4.Checked = false;
        //        //    }
        //        //}

        //        //private void rb_AGWPE_Port4_CheckedChanged(object sender, EventArgs e)
        //        //{
        //        //    if (AGWSocket.Connected_to_AGWserver)
        //        //    {
        //        //        if (rb_AGWPE_Port4.Checked)
        //        //            AGWPERadioPort = 3;
        //        //    }
        //        //    else
        //        //    {
        //        //        rb_AGWPE_Port1.Checked = false;
        //        //        rb_AGWPE_Port2.Checked = false;
        //        //        rb_AGWPE_Port3.Checked = false;
        //        //        rb_AGWPE_Port4.Checked = false;
        //        //    }
        //        //}

        //        private void btn_AGWPE_Test_Click(object sender, EventArgs e)
        //        {
        ////            AGWSocket.AGWSend(2, "Test string");

        //            string str = "Test string";
        //            AGWSocket.TXHEADER Hed = new AGWSocket.TXHEADER();
        //            Hed.Port = AGWPERadioPort;
        ////            Hed.DataKind = (long)('V');	// Transmit data Unproto Via
        //            Hed.DataKind = (long)('v');	// test Connect Via - did not work
        //            Hed.CallFrom = Form1.StationCallsign;
        //            Hed.CallTo = Form1.DatabaseCallsign;
        ////            Hed.Data = str;
        //            Hed.Data = "";  // test with no Data - it worked!
        //            AGWSocket.SendAGWTXFrame(Hed);
        //        }
        //        #endregion
        #endregion

        #region APRS & Packet node Packets tabs
        private void rb_Aid_APRS_AllPackets_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Aid_APRS_AllPackets.Checked)
            {
                MakeVisible(lbl_Aid_Num_AllPackets, true);
                MakeVisible(tb_NumAPRSlines_Aid, true);
                MakeVisible(lbl_Aid_NumAPRSnetwork, false);
                MakeVisible(tb_NumAPRSnetwork_Aid, false);
            }
            else
            {
                MakeVisible(lbl_Aid_NumAPRSnetwork, true);
                MakeVisible(tb_NumAPRSnetwork_Aid, true);
                MakeVisible(lbl_Aid_Num_AllPackets, false);
                MakeVisible(tb_NumAPRSlines_Aid, false);
            }
        }

        private void ProcessAPRSRcvdThread(object info)
        {
            string packet = (string)info;
            char[] EndTrim = { '\r', '\n', '\0' };
            string line = packet.TrimEnd(EndTrim);
            line = line.TrimStart(EndTrim);
            string[] Parts = NewAGWPErawPacket.Split(new char[] { ' ', '\r' });
            //string line;

            //line = Parts[2] + ">" + Parts[4];	    // move in Fm and To callsigns
            //if (Parts[5] == "Via")
            //{
            //    line += "," + Parts[6];
            //}

            //// add in the date
            //line += date + Parts[10].Substring(2, 5) + "]:";

            //// add in the rest of the data
            //line += szData.Substring(szData.IndexOf('\r') + 1);


            // increment line count
            NumAPRSlines++;
            SetTBtext(tb_NumAPRSlines_Aid, NumAPRSlines.ToString());

            // do we show all packets?
            if (rb_DB_All_APRS_Packets.Checked)
            {
//                AppendRXtext(tb_APRS_Packets_Rcvd, line + Environment.NewLine);
                AppendRtbRXtext(rtb_APRS_Packets_Received_DB, line + Environment.NewLine);
            }
            else
            {
                // test if it is a network packet
                if (Parts[4] == APRSnetworkName)
                {
                    NumAPRSnetworklines++;
                    SetTBtext(tb_NumAPRSnetwork_DB, NumAPRSnetworklines.ToString());
//                    AppendRXtext(tb_APRS_Packets_Rcvd, line + Environment.NewLine);
                    AppendRtbRXtext(rtb_APRS_Packets_Received_DB, line + Environment.NewLine);
                }
            }

            // now process only the network packets
            if (Parts[2] == APRSnetworkName)
            {
            }
        }

        private void ProcessAPRSSentThread(object info)
        {
            char[] EndTrim = { '\r', '\n', '\0' };
            string packet = (string)info;
            string line = packet.TrimEnd(EndTrim);
            line = line.TrimStart(EndTrim);
// 5/30/17            AppendRXtext(tb_APRS_Packets_Sent, line + Environment.NewLine);
        }

        private void ProcessPacketNodeThread(object info)
        {
            string packet = (string)info;
            //            AddRichText(Packet_Node_Packets, "Attempting to connect to AGWPE" + Environment.NewLine, Color.Green);
        }

        private void ProcessPacketNodeSentThread(object info)
        {
            string packet = (string)info;
            AddRichText(rtb_DB_Packet_Packets, "Attempting to connect to AGWPE" + Environment.NewLine, Color.Red);
        }

        private void Send_APRS_Message(string To, string message)
        {
            // verify that the 3 callsign textboxes have text entered
            if ((chk_Use_Station_Tactical_Callsign.Checked && (tb_AidStation_Tactical_Callsign.Text != "")) && (tb_AidStation_FCC_Callsign.Text != "") && (tb_Database_FCC_Callsign.Text != ""))
            {
                // prepare the message
                // do differently if ID packet
                string packet;
                if (To == "ID")
                {
                    packet = StationTacticalCallsign + ">" + To + ":" + tb_Station_Name.Text + "," + tb_AidStation_FCC_Callsign.Text;
                }
                else
                {
                    packet = StationTacticalCallsign + ">" + To + ":" + DatabaseFCCCallsign.PadRight(9) + ":" + message + "{" + APRS_Message_Number.ToString();
                }

                // send the message through AGWPE
                Aid_AGWSocket.AGWSend(AGWPERadioPort, packet);

                // increment the Message Number and the TX count
                APRS_Message_Number++;
                APRS_TX_Count++;
//                Elapsed10minTimer.Start();  // make sure the timer is running
            }
            else
            {
                // tell the user that a callsign is missing
                MessageBox.Show("Missing at least one callsign required to transmit!", "Missing callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Send_Packet_Message(string To, string message)
        {
            // verify that the 3 callsign textboxes have text entered
            //            if ((tb_APRS_Tactical_Callsign.Text != "") && (tb_APRS_AidStation_Callsign.Text != "") && (tb_APRS_Database_Callsign.Text != ""))
            if ((tb_AidStation_FCC_Callsign.Text != "") && (tb_Database_FCC_Callsign.Text != ""))
            {
                // prepare the message
                // do differently if ID packet
                string packet;
                //if (To == "ID")
                //{
                //    packet = TacticalCallsign + ">" + To + ":" + tb_Station_Name.Text + "," + tb_APRS_AidStation_Callsign.Text;
                //}
                //else
                //{
                packet = StationTacticalCallsign + ">" + To + ":" + DatabaseFCCCallsign.PadRight(9) + ":" + message + "{" + APRS_Message_Number.ToString();
                //}

                // send the message through AGWPE
                Aid_AGWSocket.AGWSend(AGWPERadioPort, packet);

                // increment the Message Number and the TX count
                //APRS_Message_Number++;
                //APRS_TX_Count++;
                //Elapsed10minTimer.Start();  // make sure the timer is running
            }
            else
            {
                // tell the user that a callsign is missing
                MessageBox.Show("Missing at least one callsign required to transmit!", "Missing callsign", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        #region Stations tab
        // Function to read in the Station data file (.csv or .txt suffix)
        bool Load_Aid_Stations(string FileName)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            StreamReader reader;

            try
            {
                reader = File.OpenText(FileName);
            }
            catch
            {
                MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            Aid_Stations.Clear();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                Parts = line.Split(splitter);
                if (!Parts[0].StartsWith("*"))
                {
                    Aid_Station station = new Aid_Station();
                    station.Name = Parts[0].TrimStart(front);
                    station.Number = Convert.ToInt16(Parts[1]);
                    station.Latitude = Convert.ToDouble(Parts[2]);
                    station.Longitude = Convert.ToDouble(Parts[3]);
                    station.Previous = Parts[4].TrimStart(front);
                    station.DistPrev = Convert.ToDouble(Parts[5]);
                    station.Next = Parts[6].TrimStart(front);
                    station.DistNext = Convert.ToDouble(Parts[7]);
                    station.Difficulty = Convert.ToDouble(Parts[8]);
                    string access = Parts[9].TrimStart(front);
                    if ((access == "Y") || (access == "y") || (access == "yes") || (access == "Yes") || (access == "YES"))
                    {
                        station.Accessible = true;
                    }
                    station.Number_of_Log_Points = Convert.ToInt16(Parts[10]);
                    if (Parts[11] != "")
                        station.First_Runner = Convert.ToDateTime(Parts[11]);
                    if (Parts[12] != "")
                        station.Cuttoff_Time = Convert.ToDateTime(Parts[12]);
                    Aid_Stations.Add(station);
                }
            }
            lbl_Stations_file_Loaded.Visible = true;

            // make the Station saving impossible
            Make_Saving_Aid_Possible(false);

            return true;
        }

        bool Save_Aid_Stations(List<Aid_Station> Stations, string FileName)
        {
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // test if the List is empty
            if (Stations.Count == 0)
            {
                MessageBox.Show("Station List is empty", "List empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                                        FileName +
                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                else
                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "* The file used to store the Station data could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (13 items)" + Environment.NewLine +
                            "* Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station," + Environment.NewLine +
                            "* Next station, distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)," + Environment.NewLine +
                            "* # of Log Points, First runner expected Time, Cutoff Time" + Environment.NewLine;

            // save each station
            foreach (Aid_Station station in Stations)
            {
                // Station name, Aid Station number, Latitude, Longitude, Previous station, distance from Previous station, Next station,
                // distance to Next station, difficulty factor to Next station, Crew accessible (Y/N)
                string line = station.Name + ",";
                line += station.Number + ",";
                line += station.Latitude.ToString() + ",";
                line += station.Longitude.ToString() + ",";
                line += station.Previous + ",";
                line += station.DistPrev.ToString() + ",";
                line += station.Next + ",";
                line += station.DistNext.ToString() + ",";
                line += station.Difficulty.ToString() + ",";
                if (station.Accessible)
                    line += "Yes";
                else
                    line += "No";
                line += station.Number_of_Log_Points.ToString() + ",";
                line += station.First_Runner.ToString() + ",";
                line += station.Cuttoff_Time.ToString();
                writer.WriteLine(line);
            }
            writer.Close();

            // make the Station saving impossible
            Make_Saving_Aid_Possible(false);

            return true;
        }

        private void tb_Aid_Station_Info_Filename_TextChanged(object sender, EventArgs e)
        {
            if (tb_Aid_Station_Info_Filename.Text == "")
                tb_Aid_Station_Info_Filename.BackColor = Color.FromArgb(255, 128, 128);
            else
                tb_Aid_Station_Info_Filename.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Downloaded_Station_Info_File_TextChanged(object sender, EventArgs e)
        {
            if (tb_Downloaded_Station_Info_File.Text == "")
            {
                tb_Downloaded_Station_Info_File.BackColor = Color.FromArgb(255, 128, 128);
                //                btn_Save_Downloaded_Station_File.Visible = false;
            }
            else
            {
                tb_Downloaded_Station_Info_File.BackColor = Color.FromKnownColor(KnownColor.Window);
                //                btn_Save_Downloaded_Station_File.Visible = true;
            }
        }

        private void Bind_Aid_Station_DGV()
        {
            Aid_dgv_Stations.DataSource = null;
            Aid_dgv_Stations.DataSource = Aid_Stations;
            Aid_dgv_Stations.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[0].Width = Station_DGV_Width;     // Name
            Aid_dgv_Stations.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[0].HeaderText = "Station Name";
            Aid_dgv_Stations.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[1].Width = 20;     // Number
            Aid_dgv_Stations.Columns[1].HeaderText = "#";
            if (Show_LatLong)
            {
                Aid_dgv_Stations.Columns[2].Width = 50;     // Latitude
                Aid_dgv_Stations.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Stations.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                Aid_dgv_Stations.Columns[3].Width = 55;     // Longitude
                Aid_dgv_Stations.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Stations.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                Aid_dgv_Stations.Width = 742;   // 3/27/19
            }
            else
            {
                Aid_dgv_Stations.Columns[2].Visible = false;
                Aid_dgv_Stations.Columns[3].Visible = false;
                Aid_dgv_Stations.Width = 637;   // 3/27/19
            }
            Aid_dgv_Stations.Columns[4].Width = Station_DGV_Width;     // Previous
            Aid_dgv_Stations.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[4].HeaderText = "Previous Station";
            Aid_dgv_Stations.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[5].Width = 51;
            Aid_dgv_Stations.Columns[5].HeaderText = "Dist. to Previous";
            Aid_dgv_Stations.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[6].Width = Station_DGV_Width;     // Next
            Aid_dgv_Stations.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[6].HeaderText = "Next Station";
            Aid_dgv_Stations.Columns[6].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[7].Width = 51;
            Aid_dgv_Stations.Columns[7].HeaderText = "Distance to Next";
            Aid_dgv_Stations.Columns[7].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[7].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[8].Width = 52;     // Difficulty
            Aid_dgv_Stations.Columns[9].Width = 63;     // Accessible
            Aid_dgv_Stations.Columns[10].Width = 48;    // Number of Log points (1 or 2)
            Aid_dgv_Stations.Columns[10].HeaderText = "# of Log pts";
            Aid_dgv_Stations.Columns[10].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[10].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[11].HeaderText = "First Runner Expected";
            Aid_dgv_Stations.Columns[11].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[11].DefaultCellStyle.Format = "HH:mm";
            Aid_dgv_Stations.Columns[11].Width = 70;
            Aid_dgv_Stations.Columns[11].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Columns[12].HeaderText = "Cutoff Time";
            Aid_dgv_Stations.Columns[12].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Aid_dgv_Stations.Columns[12].DefaultCellStyle.Format = "HH:mm";
            Aid_dgv_Stations.Columns[12].Width = 54;
            Aid_dgv_Stations.Columns[12].SortMode = DataGridViewColumnSortMode.NotSortable;
            Aid_dgv_Stations.Update();

            // total the miles
            double miles = 0;
            for (int i = 0; i < Aid_Stations.Count; i++)
            {
                miles += Aid_Stations[i].DistNext;
            }
            tb_Aid_Total_Miles.Text = miles.ToString("F2");
            tb_Number_Aid_Stations.Text = Aid_Stations.Count.ToString();    // and display the number of stations
        }

        private void Make_Saving_Aid_Possible(bool state)
        {
            btn_Save_Aid_Station_Changes.Visible = state;
            tb_Save_Aid_Station_Info_Filename.Visible = state;
            tb_Save_Aid_Station_Info_Filename.Text = tb_Station_Info_Filename.Text;
            btn_Browse_Aid_Save_Stations_Info_Filename.Visible = state;
        }

        private void chk_Make_Aid_Editable_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_Make_Aid_Editable.Checked)
            {
                Aid_dgv_Stations.ReadOnly = false;
                btn_Add_Aid_Station.Enabled = true;
                btn_Clear_Aid_Station_List.Enabled = true;
//                btn_Move_Aid_Station_Down.Visible = true;
//                btn_Move_Aid_Station_Up.Visible = true;
                MakeVisible(btn_Move_Aid_Station_Down, true);
                MakeVisible(btn_Move_Aid_Station_Up, true);
            }
            else
            {
                Aid_dgv_Stations.ReadOnly = true;
                btn_Add_Aid_Station.Enabled = false;
                btn_Clear_Aid_Station_List.Enabled = false;
//                btn_Move_Aid_Station_Down.Visible = false;
//                btn_Move_Aid_Station_Up.Visible = false;
                MakeVisible(btn_Move_Aid_Station_Down, false);
                MakeVisible(btn_Move_Aid_Station_Up, false);
            }
        }

        private void selectAsThisStationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // determine position in the list
            int index = Aid_dgv_Stations.CurrentRow.Index;

            // get the name
            Aid_Station temp = Aid_Stations[index];
            tb_Station_Name.Text = temp.Name;
            tb_Station_Name_Settings.Text = temp.Name;
            Station_Name = temp.Name;

            // save in the Registry
            Save_Registry("Station Name", Station_Name);

            // turn off the Not Found label
            Test_Station_Name();

            // 7/15/17
            MessageBox.Show("Close this program and restart it,\n\nto have this Station name change take affect!", "Station name change", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void chk_Aid_Show_LatLong_CheckedChanged(object sender, EventArgs e)
        {
// 3/27/19 - moved down            //if (chk_Aid_Show_LatLong.Checked)
            //{
            //    Show_LatLong = true;
            //    Save_Registry("Show Lat/Long", "Yes");
            //}
            //else
            //{
            //    Show_LatLong = false;
            //    Save_Registry("Show Lat/Long", "No");
            //}
            if (!Init_Registry)
            {   // 3/27/19 - moved down here
                if (chk_Aid_Show_LatLong.Checked)
                {
                    Show_LatLong = true;
                    Save_Registry("Show Lat/Long", "Yes");
                }
                else
                {
                    Show_LatLong = false;
                    Save_Registry("Show Lat/Long", "No");
                }
                Bind_Aid_Station_DGV();
            }
        }

        #region Buttons
        private void btn_Browse_Aid_Station_Info_Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tb_Aid_Station_Info_Filename.Text = ofd.FileName;
            }
        }

        private void btn_Load_Aid_Station_Info_Filename_Click(object sender, EventArgs e)
        {
            if (tb_Aid_Station_Info_Filename.Text != "")
            {
                // save the new name
                Save_Registry("Stations Info File", tb_Aid_Station_Info_Filename.Text);

                // load the file data
                Load_Aid_Stations(tb_Aid_Station_Info_Filename.Text);

                // put the data into the DataGridView
                if (Aid_Stations.Count != 0)
                {
                    Bind_Aid_Station_DGV();
                }

                // 8/4/17 - test the Station Name
                Test_Station_Name();    // 8/4/17
            }
            else    // 8/4/17
            {       // 8/4/17
                MessageBox.Show("Must enter file name before clicking Load button");    // 8/4/17
            }       // 8/4/17
        }

        private void btn_Clear_Aid_Station_List_Click(object sender, EventArgs e)
        {
            Aid_Stations.Clear();
            Aid_dgv_Stations.DataSource = null;
        }

        private void btn_Add_Aid_Station_Click(object sender, EventArgs e)
        {
            Aid_Station station = new Aid_Station();

            // make the Station saving possible
            Make_Saving_Aid_Possible(true);

            // put in some initial data
            if (Aid_Stations.Count != 0)
            {
                station.Previous = Aid_Stations[Stations.Count - 1].Name;
            }
            Aid_Stations.Add(station);
            Bind_Aid_Station_DGV();
        }

        private void btn_Move_Aid_Station_Up_Click(object sender, EventArgs e)
        {
            // determine position in the list
            int index = Aid_dgv_Stations.CurrentRow.Index;

            // move up only if it is not the top row
            if (index != 0)
            {
                // make the Station saving possible
                Make_Saving_Aid_Possible(true);

                // swap position in list
                Aid_Station temp = Aid_Stations[index];
                Aid_Stations[index] = Aid_Stations[index - 1];
                Aid_Stations[index - 1] = temp;

                // swap previous and next entries for three stations each
                if (index != 1)
                {
                    Aid_Stations[index - 1].Previous = Aid_Stations[index - 2].Name;
                    Aid_Stations[index - 2].Next = Aid_Stations[index - 1].Name;
                }
                else
                {
                    Aid_Stations[index - 1].Previous = "";
                }
                Aid_Stations[index].Previous = Aid_Stations[index - 1].Name;
                Aid_Stations[index - 1].Next = Aid_Stations[index].Name;
                if ((index + 1) != Aid_Stations.Count)
                {
                    Aid_Stations[index + 1].Previous = Aid_Stations[index].Name;
                    Aid_Stations[index].Next = Aid_Stations[index + 1].Name;
                }
                else
                {
                    Aid_Stations[index].Next = "";
                }

                // if distances and difficulty exist, change the value to negative, for three stations
                for (int i = index - 2; i <= index; i++)
                {
                    if (i < 0)
                        continue;
                    if (Aid_Stations[i].DistNext != 0)
                    {
                        Aid_Stations[i].DistNext = -(Aid_Stations[i].DistNext);
                    }
                    if (Aid_Stations[i].Difficulty != 0)
                    {
                        Aid_Stations[i].Difficulty = -(Aid_Stations[i].Difficulty);
                    }
                }

                // repaint the DGV
                Bind_Aid_Station_DGV();
            }
        }

        private void btn_Move_Aid_Station_Down_Click(object sender, EventArgs e)
        {
            // determine position in the list
            int index = Aid_dgv_Stations.CurrentRow.Index;

            // move down only if it is not the bottom row
            if ((index + 1) < Aid_Stations.Count)
            {
                // make the Station saving possible
                Make_Saving_Aid_Possible(true);

                // swap position in list
                Aid_Station temp = Aid_Stations[index];
                Aid_Stations[index] = Aid_Stations[index + 1];
                Aid_Stations[index + 1] = temp;

                // swap previous and next entries for three stations each
                if (index != 0)
                {
                    Aid_Stations[index].Previous = Aid_Stations[index - 1].Name;
                    Aid_Stations[index - 1].Next = Aid_Stations[index].Name;
                }
                else
                {
                    Aid_Stations[index].Previous = "";
                }
                Aid_Stations[index + 1].Previous = Aid_Stations[index].Name;
                Aid_Stations[index].Next = Aid_Stations[index + 1].Name;
                if ((index + 2) < Aid_Stations.Count)
                {
                    Aid_Stations[index + 2].Previous = Aid_Stations[index + 1].Name;
                    Aid_Stations[index + 1].Next = Aid_Stations[index + 2].Name;
                }
                else
                {
                    Aid_Stations[index + 1].Next = "";
                }

                // if distances and difficulty exist, change the value to negative, for three stations
                for (int i = index - 1; i <= index + 1; i++)
                {
                    if (i < 0)
                        continue;
                    if (Aid_Stations[i].DistNext != 0)
                    {
                        Aid_Stations[i].DistNext = -(Aid_Stations[i].DistNext);
                    }
                    if (Aid_Stations[i].Difficulty != 0)
                    {
                        Aid_Stations[i].Difficulty = -(Aid_Stations[i].Difficulty);
                    }
                }

                // repaint the DGV
                Bind_Aid_Station_DGV();
            }
        }

        private void btn_Save_Aid_Station_Changes_Click(object sender, EventArgs e)
        {
            Save_Aid_Stations(Aid_Stations, tb_Save_Aid_Station_Info_Filename.Text);
        }

        private void btn_Browse_Save_Aid_Stations_Info_Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tb_Save_Aid_Station_Info_Filename.Text = ofd.FileName;
            }
        }

        private void btn_Download_StationFile_from_Central_Click(object sender, EventArgs e)
        {
            if (tb_Downloaded_Station_Info_File.Text != "")
            {
                btn_Download_StationFile_from_Central.Text = "Downloading";
                WorkerObject.Download = btn_Download_StationFile_from_Central;
                WorkerObject.NeedStations = true;        // 8/9/17
                tb_Downloaded_Station_Info_File.Visible = true;
                btn_Browse_Downloaded_Station_Info_File.Visible = true;
                SendCommand(Commands.RequestStationInfo, "");
            }
            else
                MessageBox.Show("Need to enter a file name or use the Browse button", "Missing File name", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void btn_Save_Downloaded_Station_File_Click(object sender, EventArgs e)
        {
            // save the file
            // make sure the filename has been entered
            if (tb_Downloaded_Station_Info_File.Text == "")
            {
                MessageBox.Show("File name must be entered beofre clicking the Save button");
                return;
            }

            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            string FileName = tb_Downloaded_Station_Info_File.Text;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                                        FileName +
                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //                        return false;
                        return;
                    }
                }
                else
                    //                    return false;   // quit, do not overwrite existing file
                    return;
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //                    return false;
                    return;
                }
            }
            // save the file
            writer.Write(WorkerObject.Downloaded_Stations_Info);
            writer.Flush();
            writer.Close();
            fs.Close();

            // ask user if it should be used now
            DialogResult res = MessageBox.Show("Should this file be used now?", "Use the file?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                // make Loaded label invisible
                lbl_Aid_Stations_file_Loaded.Visible = false;
                lbl_Aid_Stations_file_Loaded.Update();

                // move file path to tb_Stations_Info_Filename
                tb_Aid_Station_Info_Filename.Text = tb_Downloaded_Station_Info_File.Text;
                tb_Save_Aid_Station_Info_Filename.Update();

                // click the Load button
                btn_Load_Aid_Station_Info_Filename_Click(null, null);

                // make Download textbox, Browse button and Save buttons invisible
                tb_Downloaded_Station_Info_File.Visible = false;
                tb_Downloaded_Station_Info_File.Update();
                btn_Browse_Downloaded_Station_Info_File.Visible = false;
                btn_Browse_Downloaded_Station_Info_File.Update();
                btn_Save_Downloaded_Station_File.Visible = false;
                btn_Save_Downloaded_Station_File.Update();
            }
        }

        private void btn_Browse_Downloaded_Station_Info_File_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tb_Downloaded_Station_Info_File.Text = ofd.FileName;
            }
        }
        #endregion
        #endregion

        #region Debug tab
        #region RFID Reading tab
        private void rb_Two_Readers_CheckedChanged(object sender, EventArgs e)
        {

        }
        #endregion
        #region File Transfer tab
        private void btn_Browse_File_Transfer_Click(object sender, EventArgs e)
        {

        }

        private void btn_Request_File_to_Transfer_Click(object sender, EventArgs e)
        {
            // send request to Central Database
        }

        private void tb_File_Transfer_filename_TextChanged(object sender, EventArgs e)
        {

        }

        private void tb_Save_File_Transfer_Filename_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion
        #endregion

        #region Lists/Info tab
        #region Runner List
        // user will click this button to Upload the Runner List file from the Central Database
        private void btn_Refresh_Aid_Runner_List_Click(object sender, EventArgs e)
        {
            SetCtlText(btn_Refresh_Aid_Runner_List, "Downloading");
            WorkerObject.Download = btn_Refresh_Aid_Runner_List;
            WorkerObject.NeedRunners = true;        // 8/9/17
            SendCommand(Commands.RequestRunnerList, "");
            MakeVisible(lbl_Aid_Runner_List_Not_available, false);
        }

        // 8/10/17 added
        private void btn_Aid_Download_Bib_Only_from_DB_Click(object sender, EventArgs e)
        {
            SetCtlText(btn_Aid_Download_Bib_Only_from_DB, "Downloading");
            WorkerObject.Download = btn_Aid_Download_Bib_Only_from_DB;
            WorkerObject.NeedBibs = true;
            SendCommand(Commands.RequestBibList, "");
            MakeVisible(lbl_Aid_Runner_List_Not_available, false);
        }

        private void Load_Aid_Runners(object info)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(WorkerObject.Downloaded_Runner_List);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            Load_Aid_Runners(reader);

// 5/1/19            if (Connection_Type == Connect_Medium.Ethernet)
            if ((Connection_Type == Connect_Medium.Ethernet) || (APRS_Load_DNS & (Connection_Type == Connect_Medium.APRS)))
                Current_InitAction = InitActions.DNS;   // change to request the DNS List

            // next section added 5/1/19 - to accommodate the checkboxes for APRS loading
            else
                if (Connection_Type == Connect_Medium.APRS)
                {
                    if (APRS_Load_DNS)
                        Current_InitAction = InitActions.DNS;
                    else
                        if (APRS_Load_DNF)
                            Current_InitAction = InitActions.DNF;
                        else
                            if (APRS_Load_Watch)
                                Current_InitAction = InitActions.Watch;
                            else
                                if (APRS_Load_Info)
                                    Current_InitAction = InitActions.Info;
                }
        }

        public void Load_Aid_Runners(StreamReader reader)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (Aid_dgv_Runner_List.InvokeRequired)
            {
                LoadRunnersdel d = new LoadRunnersdel(Load_Aid_Runners);
                Aid_dgv_Runner_List.Invoke(d, new object[] { reader });
            }
            else
            {
                string line;
                string[] Parts;
                char[] splitter = new char[] { ',' };
                char[] front = new char[] { ' ' };

                RunnerList.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        RunnersList runner = new RunnersList();
                        runner.BibNumber = Parts[0];
                        if (Parts.Length > 1)       // 7/21/17
                            runner.Name = Parts[1];
                        RunnerList.Add(runner);
                    }
                }
                Aid_dgv_Runner_List.ReadOnly = false;
                Bind_Aid_RunnerList_DGV();
                RunnerListCount = RunnerList.Count;
//                tb_Aid_Official_Number_Runners.Text = RunnerList.Count.ToString();    // 5/7/19
                SetTBtext(tb_Aid_Official_Number_Runners, RunnerListCount.ToString());  // 5/7/19
                if (RunnerList.Count == 0)
                    RunnerList_Has_Entries = false;
                else
                {
                    RunnerList_Has_Entries = true;
                    Runner_List_Changed = true;
                }

                // close the file
                reader.Close();
            }
        }

        private void Bind_Aid_RunnerList_DGV()
        {
            Aid_dgv_Runner_List.DataSource = null;
            if (RunnerList.Count != 0)
            {
                Aid_dgv_Runner_List.DataSource = RunnerList;
                Aid_dgv_Runner_List.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_List.Columns[0].Width = 48;     // Bib number
                Aid_dgv_Runner_List.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_List.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                Aid_dgv_Runner_List.Columns[0].HeaderText = "Bib #";
                Aid_dgv_Runner_List.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Name
                Aid_dgv_Runner_List.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Runner_List.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Runner_List.Columns[1].HeaderText = "Name";
                Aid_dgv_Runner_List.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                foreach (DataGridViewRow row in Aid_dgv_Runner_List.Rows)
                {
                    row.ReadOnly = true;
                }
            }
            Aid_dgv_Runner_List.Update();
        }

        bool Find_Runner_in_Aid_RunnerList(string RunnerNumber)
        {
            int index = RunnerList.FindIndex(runner => runner.BibNumber == RunnerNumber);
            if (index >= 0)
                return true;
            else
                return false;
        }
        #endregion

        #region DNS listbox
        // user will click this button to Upload a DNS file from the Central Database
        private void btn_DNS_Download_Click(object sender, EventArgs e)
        {
            SetCtlText(btn_DNS_Download, "Downloading");
            WorkerObject.Download = btn_DNS_Download;
            WorkerObject.NeedDNS = true;        // 8/9/17
            SendCommand(Commands.RequestDNSlist, "");
            MakeVisible(lbl_DNS_List_Not_available, false);
        }

        private void Load_Aid_DNS(object info)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(WorkerObject.Downloaded_DNS_List);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            Load_Aid_DNS(reader);

// 5/1/19            if (Connection_Type == Connect_Medium.Ethernet)
            if ((Connection_Type == Connect_Medium.Ethernet) || (APRS_Load_DNF & (Connection_Type == Connect_Medium.APRS)))
                Current_InitAction = InitActions.DNF;   // change to request the DNF List

            // next section added 5/1/19 - to accommodate the checkboxes for APRS loading
            else
                if (Connection_Type == Connect_Medium.APRS)
                {
                    if (APRS_Load_DNF)
                        Current_InitAction = InitActions.DNF;
                    else
                        if (APRS_Load_Watch)
                            Current_InitAction = InitActions.Watch;
                        else
                            if (APRS_Load_Info)
                                Current_InitAction = InitActions.Info;
                }
        }

        public void Load_Aid_DNS(StreamReader reader)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lb_Aid_DNS.InvokeRequired)
            {
                LoadDNSdel d = new LoadDNSdel(Load_Aid_DNS);
                lb_Aid_DNS.Invoke(d, new object[] { reader });
            }
            else
            {
                string line;
                string[] Parts;
                char[] splitter = new char[] { ',' };
                char[] front = new char[] { ' ' };

                // read each item, adding to Listbox
                lb_Aid_DNS.Items.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        for (int i = 0; i < Parts.Length; i++)
                        {
                            lb_Aid_DNS.Items.Add(Parts[i]);
                        }
                    }
                }

                // sort it
// 7/24/17                lb_Aid_DNS.Sorted = true;
                SortListboxNumeric(lb_Aid_DNS);         // 7/24/17
                tb_Number_Aid_DNS_runners.Text = lb_Aid_DNS.Items.Count.ToString();

                // close the file
                reader.Close();
            }
        }

        bool Find_Runner_in_Aid_DNS(string RunnerNumber)
        {
            return lb_Aid_DNS.Items.Contains(RunnerNumber);
        }
        #endregion

        #region DNF dgv
        List<Aid_RunnerDNFWatch> Aid_DNFList_original = new List<Aid_RunnerDNFWatch>();

        private void tabPage_Aid_DNF_Enter(object sender, EventArgs e)
        {
            DNF_List_Changed = false;
            if (Aid_DNFList != null)
            {
                foreach (Aid_RunnerDNFWatch runner in Aid_DNFList)
                {
                    Aid_DNFList_original.Add(runner);
                }
            }
        }

        private void tabPage_Aid_DNF_Leave(object sender, EventArgs e)
        {
            if (DNF_List_Changed)
            {
                DialogResult res = MessageBox.Show("DNF List has changed!\n\nDo you want to save the changes?", "DNF List changed", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_DNF_Send_Changes_Click(null, null);
                else
                {
                    // change the list back to the original
                    foreach (Aid_RunnerDNFWatch runner in Aid_DNFList_original)
                    {
                        Aid_DNFList.Add(runner);
                    }
                }
            }
        }

        // user will click this button to Upload a DNF file from the Central Database
        private void btn_Aid_DNF_Download_Click(object sender, EventArgs e)
        {
            SetCtlText(btn_DNF_Download, "Downloading");
            WorkerObject.Download = btn_DNF_Download;
            WorkerObject.NeedDNF = true;        // 8/9/17
            SendCommand(Commands.RequestDNFlist, "");
            MakeVisible(lbl_DNF_List_Not_available, false);
        }

        private void Load_Aid_DNF(object info)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(WorkerObject.Downloaded_DNF_List);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            Load_Aid_DNF(reader);

            if (Connection_Type == Connect_Medium.Ethernet)
                Current_InitAction = InitActions.Watch;     // change to request Watch List

            // next section added 5/1/19 - to accommodate the checkboxes for APRS loading
            else
                if (Connection_Type == Connect_Medium.APRS)
                {
                    if (APRS_Load_Watch)
                        Current_InitAction = InitActions.Watch;
                    else
                        if (APRS_Load_Info)
                            Current_InitAction = InitActions.Info;
                }
        }

        public void Load_Aid_DNF(StreamReader reader)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (Aid_dgv_DNF.InvokeRequired)
            {
                LoadDNFdel d = new LoadDNFdel(Load_Aid_DNF);
                Aid_dgv_DNF.Invoke(d, new object[] { reader });
            }
            else
            {
                string line;
                string[] Parts;
                char[] splitter = new char[] { ',' };
                char[] front = new char[] { ' ' };

                Aid_DNFList.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        Aid_RunnerDNFWatch runner = new Aid_RunnerDNFWatch();
                        runner.BibNumber = Parts[0];
                        runner.Station = Parts[1];
                        runner.Time = Parts[2];
                        runner.Note = Parts[3];
                        Aid_DNFList.Add(runner);
                    }
                }
                Aid_dgv_DNF.ReadOnly = false;
                Bind_Aid_DNF_DGV();
                btn_Aid_DNF_Edit.Visible = true;
                tb_Number_Aid_DNF_runners.Text = Aid_DNFList.Count.ToString();

                // close the file
                reader.Close();

                // set the flag - 5/8/19
                DNF_List_Changed = true;    // 5/8/19
            }
        }

        private void Bind_Aid_DNF_DGV()
        {
            Aid_dgv_DNF.DataSource = null;
            if (Aid_DNFList.Count != 0)
            {
                MakeVisible(lbl_DNF_List_Not_available, false);     // 7/13/17
                Aid_dgv_DNF.DataSource = Aid_DNFList;
                Aid_dgv_DNF.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_DNF.Columns[0].Width = 48;     // Bib number
                Aid_dgv_DNF.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_DNF.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                Aid_dgv_DNF.Columns[0].HeaderText = "Bib #";
                Aid_dgv_DNF.Columns[1].Width = Station_DGV_Width;     // Station
                Aid_dgv_DNF.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_DNF.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_DNF.Columns[1].HeaderText = "Station";
                Aid_dgv_DNF.Columns[2].Width = 54;    // Time
                Aid_dgv_DNF.Columns[2].HeaderText = "Time";
                //                dgv_DNF.Columns[3].Width = 539;     // Notes
                Aid_dgv_DNF.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Notes
                Aid_dgv_DNF.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_DNF.Columns[3].HeaderText = "Notes";
                Aid_dgv_DNF.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                foreach (DataGridViewRow row in Aid_dgv_DNF.Rows)
                {
                    row.ReadOnly = true;
                }
            }
            else
                MakeVisible(lbl_DNF_List_Not_available, true);     // 7/13/17
            Aid_dgv_DNF.Update();
        }

        private void btn_Add_Aid_DNF_Click(object sender, EventArgs e)
        {
            if (tb_Aid_DNF_Add.Text != "")
            {
                Aid_RunnerDNFWatch runnerD = new Aid_RunnerDNFWatch();
                runnerD.BibNumber = tb_Aid_DNF_Add.Text;
                runnerD.Station = Station_Name;
                runnerD.Time = DateTime.Now.ToShortTimeString();
                runnerD.SentToDB = false;
                Aid_DNFList.Add(runnerD);
                tb_Aid_DNF_Add.Text = "";
                Aid_DNFList.Sort(
                    delegate(Aid_RunnerDNFWatch l1, Aid_RunnerDNFWatch l2)
                    {
                        return l1.BibNumber.CompareTo(l2.BibNumber);
                    }
                    );
                Bind_Aid_DNF_DGV();
                btn_DNF_Send_Changes.Visible = true;
                DNF_List_Changed = true;
                Editting_DNF = true;
                tb_Number_Aid_DNF_runners.Text = Aid_DNFList.Count.ToString();

                // move cursor to the Notes cell
                int index = Aid_DNFList.FindIndex(runner => runner.BibNumber == runnerD.BibNumber);
                if (index >= 0)
                {
                    Aid_dgv_DNF.CurrentCell = Aid_dgv_DNF.Rows[index].Cells[3];
                    Aid_dgv_DNF.CurrentCell.ReadOnly = false;
                    if (!Aid_dgv_DNF.BeginEdit(true))
                    {
                        int r = 4;
                    }
                }
            }
            else
            {
                MessageBox.Show("Enter a runner number in the textbox first!", " Missing Bib #", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tb_Aid_DNF_Add_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_Aid_DNF_Click(null, null);
        }

        private void btn_Aid_DNF_Edit_Click(object sender, EventArgs e)
        {
            if (Aid_DNFList.Count != 0)
            {
                //// determine which runner has been selected
                //DNF_Edit_index = dgv_DNF.CurrentRow.Index;

                //// continue if an item has been selected
                //if (DNF_Edit_index == -1)
                //    return;

                //// now which field is being editted?  (0 = number, 1 = station, 2 = time, 3 = notes)
                //Point rowcol = dgv_DNF.CurrentCellAddress;

                //// proceed only if a cell is selected
                //if (rowcol.X == -1)
                //    return;

                //// start editting
                //dgv_DNF.ReadOnly = false;
                //dgv_DNF.BeginEdit(true);
                //Editting_DNF = true;

                btn_DNF_Send_Changes.Visible = true;
                DNF_List_Changed = true;
                Editting_DNF = true;

                // move to the Notes cell and change the SendToDB flag
                Aid_dgv_DNF.CurrentCell = Aid_dgv_DNF.Rows[Aid_dgv_DNF.CurrentCellAddress.Y].Cells[3];
                Aid_dgv_DNF.Rows[Aid_dgv_DNF.CurrentCellAddress.Y].Cells[4].Value = false;
                Aid_dgv_DNF.CurrentCell.ReadOnly = false;
                if (!Aid_dgv_DNF.BeginEdit(true))
                {
                    int r = 4;
                }
            }
        }

        private void Aid_dgv_DNF_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // highlight Bib number cell when not editting
            if (!Editting_DNF)
            {
                Point rowcol = Aid_dgv_DNF.CurrentCellAddress;
                if (rowcol.X != 0)
                {
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        Move_To_Aid_DNF_Bib_Cell(rowcol.Y);
                    }));
                }
            }
        }

        private void Move_To_Aid_DNF_Bib_Cell(int index)
        {
            Aid_dgv_DNF.CurrentCell = Aid_dgv_DNF.Rows[index].Cells[0];
        }

        private void Aid_dgv_DNF_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = Aid_dgv_DNF[e.ColumnIndex, e.RowIndex];

            Editting_DNF = false;

            // verify we have just editted the Notes field
            if (e.ColumnIndex != 3)
                return;
            else
            {
                if (cell.Value == null)
                {
                    MessageBox.Show("The Notes field is empty.\n\nThis entry will be deleted!", "Empty Notes field", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        RemoveRunnerFromAidDNFList(e.RowIndex);
                    }));
                }
                else
                {
                    Aid_DNFList[e.RowIndex].Note = cell.Value.ToString();
                    btn_DNF_Send_Changes.Visible = true;
                    //                    btn_Cancel_Last_Watch.Visible = true;
                }
            }
        }

        private void RemoveRunnerFromAidDNFList(int index)
        {
            Aid_DNFList.RemoveAt(index);
            Bind_Aid_DNF_DGV();
            tb_Number_Aid_DNF_runners.Text = Aid_DNFList.Count.ToString();
        }

        private void Aid_dgv_DNF_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl tb = e.Control as DataGridViewTextBoxEditingControl;
                tb.KeyDown -= Aid_dgv_DNF_KeyDown;
                tb.KeyDown += new KeyEventHandler(Aid_dgv_DNF_KeyDown);
            }
        }

        private void Aid_dgv_DNF_KeyDown(object sender, KeyEventArgs e)
        {
            btn_DNF_Send_Changes.Visible = true;
        }

        private void btn_DNF_Send_Changes_Click(object sender, EventArgs e)
        {
// 7/22/16            if (Connected_to_Server)
            if (WorkerObject.Connected_and_Active)
            {
                foreach (Aid_RunnerDNFWatch runner in Aid_DNFList)
                {
                    if (!runner.SentToDB)
                    {
                        // send this runner now
                        SendCommand(Commands.SendDNFRunner, runner.BibNumber + "," + runner.Station + "," + runner.Time + "," + runner.Note);
                        runner.SentToDB = true;     // clear his flag
                    }
                }
                //Editting_Watch = false;
                //dgv_Watch.ReadOnly = true;
                //btn_Watch_Send_Changes.Visible = false;
                //btn_Cancel_Last_Watch.Visible = false;
                //Watch_List_Changed = false;
                Editting_DNF = false;
                Aid_dgv_DNF.ReadOnly = true;
                btn_DNF_Send_Changes.Visible = false;
                DNF_List_Changed = false;
            }
            else
            {
                MessageBox.Show("Not connected to Central Database\n\nCannot send these DNF list changes!", "Not connected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        bool Find_Runner_in_Aid_DNF(string RunnerNumber)
        {
            int index = Aid_DNFList.FindIndex(runner => runner.BibNumber == RunnerNumber);
            if (index >= 0)
                return true;
            else
                return false;
        }
        #endregion

        #region Watch dgv
        List<Aid_RunnerDNFWatch> Aid_WatchList_original = new List<Aid_RunnerDNFWatch>();

        private void tabPage_Aid_Watch_Enter(object sender, EventArgs e)
        {
            Watch_List_Changed = false;
            foreach (Aid_RunnerDNFWatch runner in Aid_WatchList)
            {
                Aid_WatchList_original.Add(runner);
            }
        }

        private void tabPage_Aid_Watch_Leave(object sender, EventArgs e)
        {
            if (Watch_List_Changed)
            {
                DialogResult res = MessageBox.Show("Watch List has changed!\n\nDo you want to save the changes?", "Watch List changed", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    btn_Aid_Watch_Send_Changes_Click(null, null);
                else
                {
                    // change the list back to the original
                    foreach (Aid_RunnerDNFWatch runner in Aid_WatchList_original)
                    {
                        Aid_WatchList.Add(runner);
                    }
                }
            }

        }

        // user will click this button to Download a Watch file from the Central Database
        private void btn_Watch_Download_Click(object sender, EventArgs e)
        {
            SetCtlText(btn_Watch_Download, "Downloading");
            WorkerObject.Download = btn_Watch_Download;
            WorkerObject.NeedWatch = true;        // 8/9/17
            SendCommand(Commands.RequestWatchlist, "");
            MakeVisible(lbl_Aid_Watch_List_Not_available, false);
        }

        private void Load_Aid_Watch(object info)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(WorkerObject.Downloaded_Watch_List);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            Load_Aid_Watch(reader);

            if (Connection_Type == Connect_Medium.Ethernet)
                Current_InitAction = InitActions.Info;     // change to request Info File

            // next section added 5/1/19 - to accommodate the checkboxes for APRS loading
            else
                if (Connection_Type == Connect_Medium.APRS)
                {
                    if (APRS_Load_Info)
                        Current_InitAction = InitActions.Info;
                }
        }

        public void Load_Aid_Watch(StreamReader reader)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (Aid_dgv_Watch.InvokeRequired)
            {
                LoadWatchdel d = new LoadWatchdel(Load_Aid_Watch);
                Aid_dgv_Watch.Invoke(d, new object[] { reader });
            }
            else
            {
                string line;
                string[] Parts;
                char[] splitter = new char[] { ',' };
                char[] front = new char[] { ' ' };

                Aid_WatchList.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        Aid_RunnerDNFWatch runner = new Aid_RunnerDNFWatch();
                        runner.BibNumber = Parts[0];
                        runner.Station = Parts[1];
                        runner.Time = Parts[2];
                        runner.Note = Parts[3];
                        runner.SentToDB = true; // 8/28/15
                        Aid_WatchList.Add(runner);
                    }
                }
                Aid_dgv_Watch.ReadOnly = false;
                Bind_Aid_Watch_DGV();
                tb_Number_Aid_Watch_runners.Text = Aid_WatchList.Count.ToString();
                if (Aid_WatchList.Count == 0)
                    WatchList_Has_Entries = false;
                else
                    WatchList_Has_Entries = true;

                // close the file
                reader.Close();
            }
        }

        private void Bind_Aid_Watch_DGV()
        {
            Aid_dgv_Watch.DataSource = null;
            if (Aid_WatchList.Count != 0)
            {
                MakeVisible(lbl_Aid_Watch_List_Not_available, false);     // 7/13/17
                Aid_dgv_Watch.DataSource = Aid_WatchList;
                Aid_dgv_Watch.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Watch.Columns[0].Width = 48;     // Bib number
                Aid_dgv_Watch.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Watch.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                Aid_dgv_Watch.Columns[0].HeaderText = "Bib #";
                Aid_dgv_Watch.Columns[1].Width = Station_DGV_Width;     // Station
                Aid_dgv_Watch.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Watch.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Watch.Columns[1].HeaderText = "Station";
                Aid_dgv_Watch.Columns[2].Width = 54;    // Time
                Aid_dgv_Watch.Columns[2].HeaderText = "Time";
                Aid_dgv_Watch.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Watch.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                //                dgv_Watch.Columns[3].Width = 527;     // Notes
                Aid_dgv_Watch.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Notes
                Aid_dgv_Watch.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Watch.Columns[3].HeaderText = "Notes";
                Aid_dgv_Watch.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                Aid_dgv_Watch.Columns[4].Visible = false;   // do not show SentToDB
                foreach (DataGridViewRow row in Aid_dgv_Watch.Rows)
                {
                    row.ReadOnly = true;
                }
            }
            else
                MakeVisible(lbl_Aid_Watch_List_Not_available, true);     // 7/13/17
            Aid_dgv_Watch.Update();
        }

        private void btn_Add_Aid_Watch_Click(object sender, EventArgs e)
        {
            if (tb_Aid_Watch_Add_Runner.Text != "")
            {
                Aid_RunnerDNFWatch runnerw = new Aid_RunnerDNFWatch();
                runnerw.BibNumber = tb_Aid_Watch_Add_Runner.Text;
                runnerw.Station = Station_Name;
                runnerw.Time = DateTime.Now.ToShortTimeString();
                runnerw.SentToDB = false;
                Aid_WatchList.Add(runnerw);
                tb_Aid_Watch_Add_Runner.Text = "";
                Aid_WatchList.Sort(
                    delegate(Aid_RunnerDNFWatch l1, Aid_RunnerDNFWatch l2)
                    {
                        return l1.BibNumber.CompareTo(l2.BibNumber);
                    }
                    );
                Bind_Aid_Watch_DGV();
                btn_Aid_Watch_Send_Changes.Visible = true;
                btn_Cancel_Last_Aid_Watch.Visible = true;
                Watch_List_Changed = true;
                Editting_Watch = true;
                tb_Number_Aid_Watch_runners.Text = Aid_WatchList.Count.ToString();

                // move cursor to the Notes cell
                int index = Aid_WatchList.FindIndex(runner => runner.BibNumber == runnerw.BibNumber);
                if (index >= 0)
                {
                    Aid_dgv_Watch.CurrentCell = Aid_dgv_Watch.Rows[index].Cells[3];
                    Aid_dgv_Watch.CurrentCell.ReadOnly = false;
                    //                    dgv_Watch.BeginEdit(true);
                    if (!Aid_dgv_Watch.BeginEdit(true))
                    {
                        int r = 4;
                    }
                }
            }
            else
            {
                MessageBox.Show("Enter a runner number in the textbox first!", " Missing Bib #", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tb_Watch_Add_Runner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_Add_Aid_Watch_Click(null, null);
        }

        private void btn_Aid_Watch_New_Notes_Click(object sender, EventArgs e)
        {
            tb_Aid_Watch_Add_Runner.Text = Aid_dgv_Watch.CurrentCell.Value.ToString();
            btn_Add_Aid_Watch_Click(null, null);
        }

        private void Aid_dgv_Watch_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // highlight Bib number cell when not editting
            if (!Editting_Watch)
            {
                Point rowcol = Aid_dgv_Watch.CurrentCellAddress;
                if (rowcol.X != 0)
                {
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        Move_To_Aid_Watch_Bib_Cell(rowcol.Y);
                    }));
                }
            }
        }

        private void Move_To_Aid_Watch_Bib_Cell(int index)
        {
            Aid_dgv_Watch.CurrentCell = Aid_dgv_Watch.Rows[index].Cells[0];
        }

        private void Aid_dgv_Watch_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = Aid_dgv_Watch[e.ColumnIndex, e.RowIndex];

            Editting_Watch = false;

            // verify we have just editted the Notes field
            if (e.ColumnIndex != 3)
                return;
            else
            {
                if (cell.Value == null)
                {
                    MessageBox.Show("The Notes field is empty.\n\nThis entry will be deleted!", "Empty Notes field", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        RemoveRunnerFromAidWatchList(e.RowIndex);
                    }));
                }
                else
                {
                    Aid_WatchList[e.RowIndex].Note = cell.Value.ToString();
                    btn_Aid_Watch_Send_Changes.Visible = true;
                    btn_Cancel_Last_Aid_Watch.Visible = true;
                }
            }
        }

        private void RemoveRunnerFromAidWatchList(int index)
        {
            Aid_WatchList.RemoveAt(index);
            Bind_Aid_Watch_DGV();
            tb_Number_Aid_Watch_runners.Text = Aid_WatchList.Count.ToString();
        }

        private void Aid_dgv_Watch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            btn_Aid_Watch_New_Notes.Visible = true;
            Aid_dgv_Watch.CurrentCell.ReadOnly = true;
        }

        private void Aid_dgv_Watch_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl tb = e.Control as DataGridViewTextBoxEditingControl;
                tb.KeyDown -= Aid_dgv_Watch_KeyDown;
                tb.KeyDown += new KeyEventHandler(Aid_dgv_Watch_KeyDown);
            }
        }

        private void Aid_dgv_Watch_KeyDown(object sender, KeyEventArgs e)
        {
            btn_Aid_Watch_Send_Changes.Visible = true;
        }

        private void btn_Aid_Watch_Send_Changes_Click(object sender, EventArgs e)
        {
// 7/22/16            if (Connected_to_Server)
            if (WorkerObject.Connected_and_Active)
            {
                foreach (Aid_RunnerDNFWatch runner in Aid_WatchList)
                {
                    if (!runner.SentToDB)
                    {
                        // send this runner now
                        SendCommand(Commands.SendWatchRunner, runner.BibNumber + "," + runner.Station + "," + runner.Time + "," + runner.Note);
                        runner.SentToDB = true;     // clear his flag
                    }
                }
                Editting_Watch = false;
                Aid_dgv_Watch.ReadOnly = true;
                btn_Aid_Watch_Send_Changes.Visible = false;
                btn_Cancel_Last_Aid_Watch.Visible = false;
                Watch_List_Changed = false;
            }
            else
            {
                MessageBox.Show("Not connected to Central Database\n\nCannot send these Watch list changes!", "Not connected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        bool Find_Runner_in_Aid_Watch(string RunnerNumber)
        {
            int index = Aid_WatchList.FindIndex(runner => runner.BibNumber == RunnerNumber);
            if (index >= 0)
                return true;
            else
                return false;
        }
        #endregion

        #region Info File
        private void Load_Aid_Info(object info)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(WorkerObject.Downloaded_Info);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            Load_Aid_Info(reader);

            Current_InitAction = InitActions.Done;     // done with Initialiation Actions
        }

        public void Load_Aid_Info(StreamReader reader)
        {
            string line;
            string[] Parts;
            string[] splitter = new string[] { ": " };

            // read each item, extracting the information
            Loading_Info = true;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                Parts = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                if (!Parts[0].StartsWith("*"))
                {
                    switch (Parts[0])
                    {
                        case "Race Name":
                            SetTBtext(tb_Aid_RaceName, Parts[1]);
                            break;
                        case "Race Location":
                            SetTBtext(tb_Aid_RaceLocation, Parts[1]);
                            break;
                        case "Sponsor":
                            SetTBtext(tb_Aid_RaceSponsor, Parts[1]);
                            break;
                        case "Race Date":
                            SetTBtext(tb_Aid_Race_Date, Parts[1]);
                            break;
                        case "Start Time":
                            Start_Time = Parts[1];
                            SetTBtext(tb_Start_Time, Start_Time);
                            SetTBtext(tb_Aid_Official_Start_Time, Start_Time);
                            break;
                        case "# of Runners":
                            SetTBtext(tb_Aid_NumberofRunners, Parts[1]);
                            break;
                        case "Contact person":
                            SetTBtext(tb_Aid_ContactName, Parts[1]);
                            break;
                        case "Contact phone":
                            SetTBtext(tb_Aid_ContactPhone, Parts[1]);
                            break;
                        case "Packet Frequency":
                            SetTBtext(tb_Aid_Info_Packet_Frequency, Parts[1]);
                            break;
                    }
                }
            }
            Loading_Info = false;

            // close the file
            reader.Close();
        }

        private bool Load_Aid_Info(string path, bool suppress_error_msg)
        {
            //string line;
            //string[] Parts;
            //string[] splitter = new string[] { ": " };
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

                Load_Aid_Info(reader);
            }
            return true;
        }

        private bool Load_Aid_Info(string path)
        {
            return Load_Aid_Info(path, false);
        }

        private void btn_Download_Info_Click(object sender, EventArgs e)
        {
            SetCtlText(btn_Download_Info, "Downloading");
            WorkerObject.Download = btn_Download_Info;
            WorkerObject.NeedInfo = true;        // 8/9/17
            SendCommand(Commands.RequestInfo, "");
            MakeVisible(lbl_Info_Not_available, false);
        }
        #endregion
        #endregion

        #region Issues tab
        bool Adding_Issue = false;
        bool Loading_Issues = false;

        private void Load_Issues(object info)
        {       // this is the function called when downloading the Issues from the Database
            byte[] byteArray = Encoding.ASCII.GetBytes(WorkerObject.Downloaded_Issues);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            Load_Issues(reader);
            Save_Issues();          // save a copy locally
        }

        public void Load_Issues(StreamReader reader)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (Aid_dgv_Issues.InvokeRequired)
            {
                LoadIssuedel d = new LoadIssuedel(Load_Issues);
                Aid_dgv_Issues.Invoke(d, new object[] { reader });
            }
            else
            {
                string line;
                string[] Parts;
                char[] splitter = new char[] { '|' };

                Loading_Issues = true;
                ASIssues.Clear();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Parts = line.Split(splitter);
                    if (!Parts[0].StartsWith("*"))
                    {
                        Aid_Issue issue = new Aid_Issue();
                        issue.EntryDate = Parts[0];
                        issue.ResolveDate = Parts[1];
                        issue.EntryPerson = Parts[2];
                        issue.Station = Parts[3];
                        if (Parts[4] == "B")
                            issue.Broken = true;
                        else
                            issue.Broken = false;
                        if (Parts[4] == "E")
                            issue.Enhancement = true;
                        else
                            issue.Enhancement = false;
                        issue.Description = Parts[5];
                        ASIssues.Add(issue);
                    }
                }
                Loading_Issues = false;
                Bind_Issues_DGV();
                Original_Issues_Count = ASIssues.Count;

                // close the file
                reader.Close();
            }
        }

        private bool Load_Issues(string path, bool suppress_error_msg)
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

                Load_Issues(reader);
            }
            return true;
        }

        private bool Save_Issues()
        {
            string FileName = IssuesFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                //                DialogResult result = MessageBox.Show("The Save file:\n\n" +
                //                                        FileName +
                //                                        "\n\nAlready exists  Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                //                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // verify the file is good by trying to open it
                    try
                    {
                        //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                //                else
                //                    return false;   // quit, do not overwrite existing file
            }
            else
            {       // new file
                // verify the file is good by trying to open it
                try
                {
                    fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            // save the header to the file
            string header = "*" + Environment.NewLine +
                            "* The file used to store the Issues List could be an xml or csv file.  I will choose to use a csv file." + Environment.NewLine +
                            "* The file can have a .csv or .txt suffix on its file name." + Environment.NewLine +
                            "* The format for this csv file will be thus:  (5 items, separated by the '|' character)" + Environment.NewLine +
                            "* Entry Date, Resolve Date, Entry Person, Type (\"B\" or \"E\"), Description" + Environment.NewLine +
                            "*" + Environment.NewLine;
            writer.Write(header);

            // save each item in the Issues List
            foreach (Aid_Issue issue in ASIssues)
            {
                string line = issue.EntryDate + "|";
                line += issue.ResolveDate + "|";
                line += issue.EntryPerson + "|";
                line += issue.Station + "|";
                if (issue.Broken)
                    line += "B|";
                if (issue.Enhancement)
                    line += "E|";
                line += issue.Description;
                writer.WriteLine(line);
            }
            writer.Close();

            return true;
        }

        private bool Add_Issue_to_File(string EntryPerson, string Type, string Description) // Type = "B" or "E"
        {
            string FileName = IssuesFilePath;
            StreamWriter writer = StreamWriter.Null;
            FileStream fs;
            FileInfo fi = new FileInfo(FileName);

            // determine if this is a new file or existing
            if (fi.Exists)
            {       // existing file - ask if overwrite
                // verify the file is good by trying to open it
                try
                {
                    //                        fs = new FileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    //                    fs = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                    fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(fs);
                }
                catch
                {
                    MessageBox.Show("Selected file:\n\n" + FileName + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            else
            {       // new file - tell the user the file does not exist
                MessageBox.Show("Selected file:\n\n" + FileName + "\n\ndoes not exist!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // append this one item to the Issues List file
            string line = DateTime.Now.ToString("MM/dd/yy") + "||";      // no Resolve Date entered
            //            string line = Environment.NewLine;
            //            line += DateTime.Now.ToString("MM/dd/yy") + "||";      // no Resolve Date entered
            line += EntryPerson + "|";
            line += Station_Name + "|";
            line += Type + "|";
            line += Description;
            writer.WriteLine(line);
            writer.Close();

            return true;
        }

        private bool Send_Issues_before_CLosing()
        {
            return true;
        }

        private void btn_Get_Issues_Click(object sender, EventArgs e)
        {
//            if (Connected_to_Central_Database)
            if (WorkerObject.Connected_and_Active)
            {
                SetCtlText(btn_Get_Issues, "Downloading");
                WorkerObject.Download = btn_Get_Issues;
                SendCommand(Commands.RequestIssues, "");
            }
            else
                MessageBox.Show("Must be connected to Central Database to download", "Not connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_Add_New_Issue_Click(object sender, EventArgs e)
        {
            // has the info already been entered?
            if (!Adding_Issue)
            {
                // clear the tecxtboxes and radio buttons
                tb_Issue_Name.Clear();
                tb_Issue_Issue.Clear();
// 7/26/17                rb_Issues_Enhancement.Checked = false;
                MakeRBChecked(rb_Issues_Enhancement, false);    // 7/26/17
// 7/26/17                rb_Issues_Broken.Checked = false;
                MakeRBChecked(rb_Issues_Broken, false);     // 7/26/17

                // make the labels and textboxes visible
// 7/26/17                panel_Issues.Visible = true;
                MakeVisible(panel_Issues, true);    // 7/26/17
// 7/26/17                btn_Add_New_Issue.Enabled = false;
                MakeEnabled(btn_Add_New_Issue, false);  // 7/26/17
// 7/26/17                btn_Issues_Cancel_Add.Visible = true;
                MakeVisible(btn_Issues_Cancel_Add, true);   // 7/26/17
                Application.DoEvents();

                // tell user to enter info and click it again
                MessageBox.Show("Enter name, issue info and Type,\n\nThen click the Add button again.", "Click again", MessageBoxButtons.OK, MessageBoxIcon.Hand);

                // change the flag
                Adding_Issue = true;
            }
            else
            {
                // test if the two textboxes have data in them.
                if ((tb_Issue_Issue.Text == "") || (tb_Issue_Name.Text == ""))
                {
                    // tell user to enter info and click it again
                    MessageBox.Show("Enter name and issue info,\n\nThen click the Add button again.", "Click again", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // add to the list
                    Aid_Issue issue = new Aid_Issue();
                    issue.EntryPerson = tb_Issue_Name.Text;
                    issue.Station = Station_Name;
                    issue.Description = tb_Issue_Issue.Text;
                    if (rb_Issues_Broken.Checked)
                        issue.Broken = true;
                    if (rb_Issues_Enhancement.Checked)
                        issue.Enhancement = true;
                    //                    issue.EntryDate = DateTime.Now.ToShortDateString("MM/dd/yy");
                    issue.EntryDate = DateTime.Now.ToString("MM/dd/yy");
                    ASIssues.Add(issue);
                    Bind_Issues_DGV();

                    // add to the local file
                    if (IssuesLoaded)
                    {
                        // add just one line
                        string type = string.Empty;
                        if (rb_Issues_Broken.Checked)
                            type = "B";
                        if (rb_Issues_Enhancement.Checked)
                            type = "E";
                        Add_Issue_to_File(tb_Issue_Name.Text, type, tb_Issue_Issue.Text);
                    }
                    else
                    {   // no file loaded on start up - need to create the file
                        Save_Issues();
                    }

                    // make Send to DB button visible
                    btn_Send_Issue_to_DB.Visible = true;
                    btn_Send_Issue_to_DB.Update();

                    // make the labels and textboxes invisible
                    panel_Issues.Visible = false;
                    btn_Issues_Cancel_Add.Visible = false;
                    Application.DoEvents();

                    // change the flag
                    Adding_Issue = false;
                }
            }
        }

        private void btn_Send_Issue_to_DB_Click(object sender, EventArgs e)
        {
//            if (Connected_to_Central_Database)
            if (WorkerObject.Connected_and_Active)
            {
                // send to the Central Database
                WorkerObject.Download = btn_Send_Issue_to_DB;

            // send all the new issues
            for (int i = Original_Issues_Count; i < ASIssues.Count; i++)
            {
                string line = ASIssues[i].EntryDate + "|";
                line += ASIssues[i].ResolveDate + "|";
                line += ASIssues[i].EntryPerson + "|";
                line += ASIssues[i].Station + "|";
                if (ASIssues[i].Broken)
                    line += "B|";
                if (ASIssues[i].Enhancement)
                    line += "E|";
                line += ASIssues[i].Description;
                SendCommand(Commands.SendIssue, line);
            }

            // make the button invisible
            btn_Send_Issue_to_DB.Visible = false;
            btn_Send_Issue_to_DB.Update();
            }
            else
                MessageBox.Show("Must be connected to Central Database to send", "Not connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_Issues_Cancel_Add_Click(object sender, EventArgs e)
        {
// 7/26/17            panel_Issues.Visible = false;
// 7/26/17            btn_Issues_Cancel_Add.Visible = false;
// 7/26/17            btn_Add_New_Issue.Enabled = true;
            MakeVisible(panel_Issues, false);    // 7/26/17
            MakeVisible(btn_Issues_Cancel_Add, false);   // 7/26/17
            MakeEnabled(btn_Add_New_Issue, true);  // 7/26/17
            Adding_Issue = false;
        }

        private void rb_Issues_Broken_CheckedChanged(object sender, EventArgs e)
        {
            Test_Add_Issue();
        }

        private void rb_Issues_Enhancement_CheckedChanged(object sender, EventArgs e)
        {
            Test_Add_Issue();
        }

        private void tb_Issue_Name_TextChanged(object sender, EventArgs e)
        {
            if (tb_Issue_Name.Text == "")
                tb_Issue_Name.BackColor = Color.FromArgb(255, 224, 192);
            else
                tb_Issue_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
            Test_Add_Issue();
        }

        private void tb_Issue_Issue_TextChanged(object sender, EventArgs e)
        {
            if (tb_Issue_Issue.Text == "")
                tb_Issue_Issue.BackColor = Color.FromArgb(255, 224, 192);
            else
                tb_Issue_Issue.BackColor = Color.FromKnownColor(KnownColor.Window);
            Test_Add_Issue();
        }

        private void Test_Add_Issue()
        {
            if ((tb_Issue_Issue.Text == "") || (tb_Issue_Name.Text == "") || ((!rb_Issues_Enhancement.Checked) && (!rb_Issues_Broken.Checked)))
// 7/26/17                btn_Add_New_Issue.Enabled = false;
                MakeEnabled(btn_Add_New_Issue, false);  // 7/26/17
            else
// 7/26/17                btn_Add_New_Issue.Enabled = true;
                MakeEnabled(btn_Add_New_Issue, true);  // 7/26/17
        }

        private void Bind_Issues_DGV()
        {
            Aid_dgv_Issues.DataSource = null;
            if (ASIssues.Count != 0)
            {
                Aid_dgv_Issues.DataSource = ASIssues;
                Aid_dgv_Issues.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[0].Width = 55;     // Entry Date
                Aid_dgv_Issues.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                Aid_dgv_Issues.Columns[0].HeaderText = "Entry Date";
                Aid_dgv_Issues.Columns[1].Width = 55;     // Resolve Date
                Aid_dgv_Issues.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Issues.Columns[1].HeaderText = "Resolve Date";
                Aid_dgv_Issues.Columns[2].Width = 98;    // Entry Person
                Aid_dgv_Issues.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Issues.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                Aid_dgv_Issues.Columns[2].HeaderText = "Entry Person";
                Aid_dgv_Issues.Columns[3].Width = Station_DGV_Width;     // Station
                Aid_dgv_Issues.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Issues.Columns[3].HeaderText = "Station";
                Aid_dgv_Issues.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[4].Width = 43;     // Broken type
                Aid_dgv_Issues.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Issues.Columns[4].HeaderText = "Broken";
                Aid_dgv_Issues.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[5].Width = 73;     // Enhancement type
                Aid_dgv_Issues.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;   // this helps center the header text
                Aid_dgv_Issues.Columns[5].HeaderText = "Enhancement";
                Aid_dgv_Issues.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //                dgv_Issues.Columns[5].Width = 539;     // Description
                Aid_dgv_Issues.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // Description
                Aid_dgv_Issues.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Aid_dgv_Issues.Columns[6].HeaderText = "Description";
                Aid_dgv_Issues.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            Aid_dgv_Issues.Update();
        }
        #endregion

        #region Open/Close ports
        private Boolean OpenInPort(int port)
        {
            try
            {
                IncomingSerialPort.PortName = ports[port].COMport;
                IncomingSerialPort.BaudRate = 9600;
                IncomingSerialPort.Parity = Parity.None;
                IncomingSerialPort.DataBits = 8;
                IncomingSerialPort.StopBits = StopBits.One;
                IncomingSerialPort.ReadTimeout = 1000;
                IncomingSerialPort.WriteTimeout = 500;
                IncomingSerialPort.Handshake = System.IO.Ports.Handshake.RequestToSend;
                IncomingSerialPort.NewLine = "\r";
                IncomingSerialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            // start the Event handler for the received bytes
            IncomingSerialPort.DataReceived += new SerialDataReceivedEventHandler(IncomingSerialData);
            return true;
        }

        private Boolean OpenOutPort(int port)
        {
            try
            {
                OutgoingSerialPort.PortName = ports[port].COMport;
                OutgoingSerialPort.BaudRate = 9600;
                OutgoingSerialPort.Parity = Parity.None;
                OutgoingSerialPort.DataBits = 8;
                OutgoingSerialPort.StopBits = StopBits.One;
                OutgoingSerialPort.ReadTimeout = 1000;
                OutgoingSerialPort.WriteTimeout = 500;
                OutgoingSerialPort.Handshake = System.IO.Ports.Handshake.RequestToSend;
                OutgoingSerialPort.NewLine = "\r";
                OutgoingSerialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            // start the Event handler for the received bytes
            OutgoingSerialPort.DataReceived += new SerialDataReceivedEventHandler(OutgoingSerialData);
            return true;
        }

        public void IncomingSerialData(object source, SerialDataReceivedEventArgs e)
        {
            int len = IncomingSerialPort.BytesToRead;
            char[] bytesin = new char[len];
            IncomingSerialPort.Read(bytesin, 0, len);
            string str = new string(bytesin);
            AppendToIncoming(str);
        }

        private void AppendToIncoming(string str)
        {
            char[] TrimChars = { (char)0x2, (char)0x3, '\n', '\r' };

            // add to existing chars
            IncomingNumber += str;

            // test if the End-of-string char has been received
            if (IncomingNumber.Contains(EndOfRFIDstring))
            {
                // create a new string containing just the RFID string, leaving extra chars
                string just = IncomingNumber.Substring(0, IncomingNumber.IndexOf(EndOfRFIDstring) + 1);
                IncomingNumber = IncomingNumber.Replace(just, "");

                // remove formatting chars
                just = just.Trim(TrimChars);
                AppendRXtext(tb_Incoming_Data_Received, just);

                // add runner number to the list and put in TimeIn and Matching Runner # for Incoming
                //                int result;
                //                bool good = int.TryParse(just, out result);
                //                if (good)
                //                    AddRunner((uint)result);
                AddRunner(just);
            }
        }

        public void OutgoingSerialData(object source, SerialDataReceivedEventArgs e)
        {
            int len = OutgoingSerialPort.BytesToRead;
            char[] bytesin = new char[len];
            OutgoingSerialPort.Read(bytesin, 0, len);
            string str = new string(bytesin);
            AppendToOutgoing(str);
        }

        private void AppendToOutgoing(string str)
        {
            char[] TrimChars = { (char)0x2, (char)0x3, '\n', '\r' };

            // add to existing chars
            OutgoingNumber += str;

            // test if the End-of-string char has been received
            if (OutgoingNumber.Contains(EndOfRFIDstring))
            {
                // create a new string containing just the RFID string, leaving extra chars
                string just = OutgoingNumber.Substring(0, OutgoingNumber.IndexOf(EndOfRFIDstring) + 1);
                OutgoingNumber = OutgoingNumber.Replace(just, "");

                // remove formatting chars
                just = just.Trim(TrimChars);
                AppendRXtext(tb_Outgoing_Data_Received, just);

                // add TimeOut to existing runner
                //int result;
                //bool good = int.TryParse(just, out result);
                //if (good)
                //    UpdateRunner((uint)result);
                UpdateRunner(just);
            }
        }

        public void AppendRXtext(TextBox tb, string p)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (tb.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(AppendRXtext);
                tb.Invoke(d, new object[] { tb, p });
            }
            else
            {
                tb.AppendText(p);
                tb.Update();
            }
        }

        public void AppendRtbRXtext(RichTextBox rtb, string p)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (rtb.InvokeRequired)
            {
                AppendTextRtbCallback d = new AppendTextRtbCallback(AppendRtbRXtext);
                rtb.Invoke(d, new object[] { rtb, p });
            }
            else
            {
                rtb.AppendText(p);
                rtb.Update();
            }
        }
        #endregion
        #endregion
    }
}
