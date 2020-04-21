using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RunnerTracker
{
    public partial class SelectRoleForm : Form
    {
        //
        // Updates:
        //  July 14, 2017 - changed (c) 2016 to 2017
        //                  changed tb_StationName to lb_StationName
        //                  filled new listbox with contents of Stations.txt, if it exists
        //  July 27, 2017 - added MakeVisible, MakeRBChecked, SetTBtext and GetTBtext delegates and changed code to use them
        //  March 12, 2019 - changed copyright date to 2019
        //                   added cancel buttons for Aid and DB
        //

        #region Variables
        public string DataDirectory;
        public string RDFilename;
        private string RDprevFilename;
        public Form1.Connect_Medium Connection_Type;
        public string StationName
        {
            get { return tb_StationName.Text; }
            set
            {
                tb_StationName.Text = value;
                btn_OK_DB.Focus();     // this didn't work
            }
        }
        public string Stations_Info_Filename;   // 7/14/17
        public bool UsingRFID
        {
            get { return cb_Using_RFID.Checked; }
            set { cb_Using_RFID.Checked = value; }
        }
        public int NumLogPts { get; set; }
        public bool ConnectViaEthernet
        {
            get { return chk_Ethernet.Checked; }
            set { chk_Ethernet.Checked = value; }
        }
        public bool ConnectViaPacket
        {
            get { return chk_Packet.Checked; }
            set { chk_Packet.Checked = value; }
        }
        public bool ConnectViaAPRS
        {
            get { return chk_APRS.Checked; }
            set { chk_APRS.Checked = value; }
        }
        public bool DatabaseRole;
        #endregion

        public SelectRoleForm(string Directory, Form1.Connect_Medium CT, int NLP, bool Database)
        {
            InitializeComponent();

            DataDirectory = Directory;
// 7/27/17            tb_Directory_Name.Text = DataDirectory;
// 7/27/17            tb_Directory_Name_Aid.Text = DataDirectory;
            SetTBtext(tb_Directory_Name, DataDirectory);    // 7/27/17
            SetTBtext(tb_Directory_Name_Aid, DataDirectory);    // 7/27/17

            // set the Connection type for the Aid Station
            Connection_Type = CT;
            switch (CT)
            {
                case Form1.Connect_Medium.APRS:
// 7/27/17                    rb_APRS.Checked = true;
                    MakeRBChecked(rb_APRS, true);   // 7/27/17
                    break;
                case Form1.Connect_Medium.Cellphone:
// 7/27/17                    rb_Cellphone.Checked = true;
                    MakeRBChecked(rb_Cellphone, true);   // 7/27/17
                    break;
                case Form1.Connect_Medium.Ethernet:
// 7/27/17                    rb_Ethernet.Checked = true;
                    MakeRBChecked(rb_Ethernet, true);   // 7/27/17
                    break;
                case Form1.Connect_Medium.Packet:
// 7/27/17                    rb_Packet.Checked = true;
                    MakeRBChecked(rb_Packet, true);   // 7/27/17
                    break;
            }

            // set the Number of Log Points for the Aid Station
            NumLogPts = NLP;
            if (NumLogPts == 1)
            {
// 7/27/17                rb_One_Log_Point.Checked = true;
// 7/27/17                rb_2_Log_Points.Checked = false;
                MakeRBChecked(rb_One_Log_Point, true);   // 7/27/17
                MakeRBChecked(rb_2_Log_Points, false);   // 7/27/17
            }
            else
            {
// 7/27/17                rb_One_Log_Point.Checked = false;
// 7/27/17                rb_2_Log_Points.Checked = true;
                MakeRBChecked(rb_One_Log_Point, false);   // 7/27/17
                MakeRBChecked(rb_2_Log_Points, true);   // 7/27/17
            }

            // choose whether this Role is for Aid Station or Database
            DatabaseRole = Database;
            if (Database)
            {
// 7/27/17                rb_Database.Checked = true;
// 7/27/17                panel_Aid.Visible = false;
// 7/27/17                panel_DB.Visible = true;
                MakeRBChecked(rb_Database, true);   // 7/27/17
                MakeVisible(panel_Aid, false);      // 7/27/17
                MakeVisible(panel_DB, true);      // 7/27/17
                btn_OK_DB.Focus();
            }
            else
            {
// 7/27/17                rb_AidStation.Checked = true;
// 7/27/17                panel_Aid.Visible = true;
// 7/27/17                panel_DB.Visible = false;
                MakeRBChecked(rb_AidStation, true);   // 7/27/17
                MakeVisible(panel_Aid, true);      // 7/27/17
                MakeVisible(panel_DB, false);      // 7/27/17
                btn_OK_Aid.Focus();
            }
        }

        private void rb_AidStation_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_AidStation.Checked)
            {
// 7/27/17                panel_Aid.Visible = true;
// 7/27/17                panel_DB.Visible = false;
                MakeVisible(panel_Aid, true);   // 7/27/17
                MakeVisible(panel_DB, false);   // 7/27/17
                DatabaseRole = false;
            }
            else
            {
// 7/27/17                panel_Aid.Visible = false;
// 7/27/17                panel_DB.Visible = true;
                MakeVisible(panel_Aid, false);   // 7/27/17
                MakeVisible(panel_DB, true);   // 7/27/17
                DatabaseRole = true;
            }
        }

        #region Delegates
        delegate void MakeVisibledel(Control cntrl, bool visible);      // 7/27/17
        delegate void MakeRBCheckeddel(RadioButton rb, bool checkd);      // 7/27/17
        delegate void SetTextdel(TextBox tb, string str);      // 7/27/17
        delegate string GetTBtextdel(TextBox tb);      // 7/27/17
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
                return tb.Text;
            }
            return null;
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
        #endregion

        #region Aid Panel Functions
        private void btn_OK_Aid_Click(object sender, EventArgs e)
        {
            DialogResult res;

            // user must enter a Data Directory path, quit if not
// 7/27/17            if (tb_Directory_Name_Aid.Text != "")
            if (GetTBtext(tb_Directory_Name_Aid) != "")     // 7/27/17
            {
// 7/27/17                DataDirectory = tb_Directory_Name_Aid.Text;
                DataDirectory = GetTBtext(tb_Directory_Name_Aid);   // 7/27/17

                // verify that this directory is good
                if (System.IO.Directory.Exists(DataDirectory))
                {
                    DatabaseRole = false;
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                {
                    res = MessageBox.Show("This Data Directory does not exist!", "Nonexistant directory", MessageBoxButtons.RetryCancel, MessageBoxIcon.Asterisk);
                    if (res == System.Windows.Forms.DialogResult.Cancel)
                    {
                        MessageBox.Show("Without a Data Directory to store this event's data,\n\n           This program will now close.", "Missing path", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    }
                }
            }
            else
            {
                res = MessageBox.Show("You must enter this event's Data Directory path to continue!", "Missing path", MessageBoxButtons.RetryCancel, MessageBoxIcon.Asterisk);
                if (res == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Without a Data Directory to store this event's data,\n\n             This program will now close.", "Missing path", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    DialogResult = System.Windows.Forms.DialogResult.Cancel;
                }
            }
        }

        private void btn_Aid_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void rb_APRS_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_APRS.Checked)
                Connection_Type = Form1.Connect_Medium.APRS;
        }

        private void rb_Packet_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Packet.Checked)
                Connection_Type = Form1.Connect_Medium.Packet;
        }

        private void rb_Ethernet_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Ethernet.Checked)
                Connection_Type = Form1.Connect_Medium.Ethernet;
        }

        private void rb_Cellphone_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Cellphone.Checked)
                Connection_Type = Form1.Connect_Medium.Cellphone;
        }

        private void rb_One_Log_Point_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_One_Log_Point.Checked)
                NumLogPts = 1;
            else
                NumLogPts = 2;
        }

        private void btn_Browse_Directory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.SelectedPath = DataDirectory;
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                DataDirectory = folderBrowserDialog1.SelectedPath;
// 7/27/17                tb_Directory_Name_Aid.Text = DataDirectory;
                SetTBtext(tb_Directory_Name_Aid, DataDirectory);    // 7/27/17
            }
        }

        private void tb_StationName_Aid_TextChanged(object sender, EventArgs e)
        {
            if (tb_StationName.Text == "Not yet identified")
                tb_StationName.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_StationName.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Directory_Name_Aid_TextChanged(object sender, EventArgs e)
        {
            if (tb_Directory_Name_Aid.Text == "")
                tb_Directory_Name_Aid.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_Directory_Name_Aid.BackColor = Color.FromKnownColor(KnownColor.Window);
        }
        #endregion

        #region DB Panel Functions
        private void tb_Directory_Name_TextChanged(object sender, EventArgs e)
        {
            if (tb_Directory_Name.Text == "")
                tb_Directory_Name.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_Directory_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void btn_Browse_DB_Directory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.SelectedPath = DataDirectory;
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                DataDirectory = folderBrowserDialog1.SelectedPath;
// 7/27/17                tb_Directory_Name.Text = DataDirectory;
                SetTBtext(tb_Directory_Name, DataDirectory);    // 7/27/17
            }
        }

        private void btn_OK_DB_Click(object sender, EventArgs e)
        {
            DialogResult res;

            // verify at least one Connection Type is Expected
            if (!chk_APRS.Checked && !chk_Ethernet.Checked && !chk_Packet.Checked)
            {
                MessageBox.Show("You must select at least one Connection Type", "No Connection Type Expected");
                return;     // quit early, do not return to From1
            }

            // user must enter a Data Directory path, quit if not
// 7/27/17            if (tb_Directory_Name.Text != "")
            if (GetTBtext(tb_Directory_Name) != "") // 7/27/17
            {
// 7/27/17                DataDirectory = tb_Directory_Name.Text;
                DataDirectory = GetTBtext(tb_Directory_Name);   // 7/27/17
// old method                RDFilename = DataDirectory + "\\RunnerData.txt";
// 8/8/16                RDprevFilename = DataDirectory + "\\RunnerData.prev.txt";

                // verify that this directory is good
                if (System.IO.Directory.Exists(DataDirectory))
                {
                    // check if a Runner data file already exists
// old method                    string FileName = DataDirectory + "\\RunnerData.txt";
// 8/8/16                    FileInfo fi = new FileInfo(FileName);
// 8/8/16                    if (fi.Exists)
                    if (Directory.GetFiles(DataDirectory, "*.xml").Length != 0)
                    {       // existing files
// 7/27/17                        lbl_File_Exists.Visible = true;
// 7/27/17                        btn_No.Visible = true;
// 7/27/17                        btn_Yes.Visible = true;
                        MakeVisible(lbl_File_Exists, true); // 7/27/17
                        MakeVisible(btn_No, true); // 7/27/17
                        MakeVisible(btn_Yes, true); // 7/27/17
                    }
                    else
                    {
                        // No existing Runner data file - all done
                        RDFilename = "";
                        DialogResult = System.Windows.Forms.DialogResult.OK;
                    }
                }
                else
                {
                    res = MessageBox.Show("This Data Directory does not exist!", "Nonexistant directory", MessageBoxButtons.RetryCancel, MessageBoxIcon.Asterisk);
                    if (res == System.Windows.Forms.DialogResult.Cancel)
                    {
                        MessageBox.Show("Without a Data Directory to store this event's data,\n\n           This program will now close.", "Missing path", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    }
                }
            }
            else
            {
                res = MessageBox.Show("You must enter this event's Data Directory path to continue!", "Missing path", MessageBoxButtons.RetryCancel, MessageBoxIcon.Asterisk);
                if (res == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Without a Data Directory to store this event's data,\n\n           This program will now close.", "Missing path", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    DialogResult = System.Windows.Forms.DialogResult.Cancel;
                }
            }
        }

        private void btn_DB_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btn_Yes_Click(object sender, EventArgs e)
        {
            //// old method            // load the Runner data file
            //// 8/8/16            char[] splitter = new char[] { ',' };
            //            char[] front = new char[] { ' ' };
            //            StreamReader reader;

            //            // open the file
            //            // do this only if the FileName is not empty
            //            if (RDFilename != "")
            //            {
            //                try
            //                {
            //                    reader = File.OpenText(RDFilename);
            //                }
            //                catch
            //                {
            //                    MessageBox.Show("Selected file:\n\n" + RDFilename + "\n\nis not accessible!             If it has not been loaded yet, then all is OK.", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            ////                    return false;
            //                }

            //                // read each item, adding to the Runner Dictionary
            //                Form1.RunnerDictionary.Clear();
            ////                while (!reader.EndOfStream)
            //                {
            ////                    line = reader.ReadLine();
            ////                    Parts = line.Split(splitter);
            //  //                  if (!Parts[0].StartsWith("*"))
            //                    {
            //    //                    for (int i = 0; i < Parts.Length; i++)
            //                        {
            ////                            lb_DNS.Items.Add(Parts[i]);
            //                        }
            //                    }
            //                }

            //                // close the file
            //    //            reader.Close();
            //            }

            // display available files, so user can select one
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml files (*.xml)|*.xml";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                RDFilename = ofd.FileName;
                // ask the user if he wants to create a copy of the file before using it
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
                        File.Copy(RDFilename, newfilename);
                }

                // read each item, adding to the Runner Dictionary
                Form1.RunnerDictionary.Clear();
                //                while (!reader.EndOfStream)
                {
                    //                    line = reader.ReadLine();
                    //                    Parts = line.Split(splitter);
                    //                  if (!Parts[0].StartsWith("*"))
                    {
                        //                    for (int i = 0; i < Parts.Length; i++)
                        {
                            //                            lb_DNS.Items.Add(Parts[i]);
                        }
                    }
                }

            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btn_No_Click(object sender, EventArgs e)
        {
            // don't use the existing Runner Data file

// old method            // give the existing Runner data file a new name, so it is preserved
//// 8/8/16            // first check if the new name already exists.  If it does, tell the user and quit.
//            if (File.Exists(RDprevFilename))
//            {       // backup file already exists, tell user
//                DialogResult res = MessageBox.Show("A backup file by the name of:\n\n" + RDprevFilename + "\n\nalready exists.  Do you want to overwrite?", "Backup file exists", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
//                if (res == System.Windows.Forms.DialogResult.No)
//                    DialogResult = System.Windows.Forms.DialogResult.OK;
//                else
//                {
//                    File.Delete(RDprevFilename);   // delete the existing backup file
//                    File.Move(RDFilename, RDprevFilename);     // rename the existing data file to the backup file name
//                    DialogResult = System.Windows.Forms.DialogResult.OK;
//                }
//            }
//            else
//            {
//                File.Move(RDFilename, RDprevFilename);     // rename the existing data file to the backup file name
//                DialogResult = System.Windows.Forms.DialogResult.OK;
//            }

                DialogResult = System.Windows.Forms.DialogResult.OK;
        }
        #endregion

        #region Code added for the Stations listbox - 7/14/17
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

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                Parts = line.Split(splitter);
                if (!Parts[0].StartsWith("*"))
                {
                    lb_StationName.Items.Add(Parts[0].TrimStart(front));
                    lb_StationName.BackColor = Color.FromKnownColor(KnownColor.Window);
                }
            }
            return true;
        }

        private void SelectRoleForm_Shown(object sender, EventArgs e)   // added 7/14/17
        {
            // will put filling the Stations listbox here - added 7/14/17
            if (Stations_Info_Filename != "")
            {
                // load the Station listbox
                Load_Stations(Stations_Info_Filename);
                lb_StationName.SelectedItem = StationName;  // if it is "Not yet identified", nothing will be selected.
            }
            else
            {
                int y = 2;
                if (StationName != "")
                {
                    lb_StationName.Items.Add(StationName);
                    lb_StationName.SelectedItem = StationName;
                }
            }
        }

        private void lb_StationName_SelectedIndexChanged(object sender, EventArgs e)    // added 7/14/17
        {
// 7/27/17            tb_StationName.Text = lb_StationName.SelectedItem.ToString();
            SetTBtext(tb_StationName, lb_StationName.SelectedItem.ToString());      // 7/27/17
        }
        #endregion
    }
}
