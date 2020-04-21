using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;

namespace RunnerTracker
{
    public partial class Sounds : Form
    {
        // Updates:
        //  8/14/17 - creation
        //

        #region Variables and Declarations
        bool Init = true;
        public string Sounds_Directory { get; set;}
        public string Connections { get; set;}
        public string File_Download { get; set;}
        public string Alerts { get; set;}
        public string Messages { get; set;}
        #endregion

        public Sounds()
        {
            InitializeComponent();
        }

        #region Buttons
        private void btn_Browse_Sounds_Directory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog findFolder = new FolderBrowserDialog();
            findFolder.RootFolder = Environment.SpecialFolder.Windows;
            findFolder.ShowNewFolderButton = true;

            if (findFolder.ShowDialog() == DialogResult.OK)
            {
                tb_Sounds_Directory.Text = findFolder.SelectedPath;
            }
        }

        private void btn_Save_Sounds_Directory_Click(object sender, EventArgs e)
        {
            Sounds_Directory = tb_Sounds_Directory.Text;
            Form1.Save_Registry("Sounds Directory", Sounds_Directory);
            btn_Save_Sounds_Directory.Visible = false;
        }

        private void btn_Test_Connections_Click(object sender, EventArgs e)
        {
            if (Connections != null)
            {
                SoundPlayer player = new SoundPlayer(Sounds_Directory + "\\" + Connections);
                player.Play();
            }
        }

        private void btn_Test_File_Download_Click(object sender, EventArgs e)
        {
            if (File_Download != null)
            {
                SoundPlayer player = new SoundPlayer(Sounds_Directory + "\\" + File_Download);
                player.Play();
            }
        }

        private void btn_Test_Alerts_Click(object sender, EventArgs e)
        {
            if (Alerts != null)
            {
                SoundPlayer player = new SoundPlayer(Sounds_Directory + "\\" + Alerts);
                player.Play();
            }
        }

        private void btn_Test_Messages_Click(object sender, EventArgs e)
        {
            if (Messages != null)
            {
                SoundPlayer player = new SoundPlayer(Sounds_Directory + "\\" + Messages);
                player.Play();
            }
        }

        private void btn_Save_Connections_Click(object sender, EventArgs e)
        {
            Form1.Save_Registry("Connections Sound", lb_Connections.SelectedItem.ToString());
        }

        private void btn_Save_File_Downloads_Click(object sender, EventArgs e)
        {
            Form1.Save_Registry("File Download Sound", File_Download);
        }

        private void btn_Save_Alerts_Click(object sender, EventArgs e)
        {
            Form1.Save_Registry("Alerts Sound", Alerts);
        }

        private void btn_Save_Messages_Click(object sender, EventArgs e)
        {
            Form1.Save_Registry("Messages Sound", Messages);
        }
        #endregion

        #region Listboxes
        private void lb_Connections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Init)
            {
                if (lb_Connections.SelectedIndex != -1)
                {
                    btn_Save_Connections.Visible = true;
                    Connections = lb_Connections.SelectedItem.ToString();
                    btn_Test_Connections.Visible = true;
                }
                else
                {
                    btn_Save_Connections.Visible = false;
                    Connections = null;
                    btn_Test_Connections.Visible = false;
                }
            }
        }

        private void lb_FilesDownload_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Init)
            {
                if (lb_FilesDownload.SelectedIndex != -1)
                {
                    btn_Save_File_Downloads.Visible = true;
                    File_Download = lb_FilesDownload.SelectedItem.ToString();
                    btn_Test_File_Download.Visible = true;
                }
                else
                {
                    btn_Save_File_Downloads.Visible = false;
                    File_Download = null;
                    btn_Test_File_Download.Visible = false;
                }
            }
        }

        private void lb_Alerts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Init)
            {
                if (lb_Alerts.SelectedIndex != -1)
                {
                    btn_Save_Alerts.Visible = true;
                    Alerts = lb_Alerts.SelectedItem.ToString();
                    btn_Test_Alerts.Visible = true;
                }
                else
                {
                    btn_Save_Alerts.Visible = false;
                    Alerts = null;
                    btn_Test_Alerts.Visible = false;
                }
            }
        }

        private void lb_Messages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Init)
            {
                if (lb_Messages.SelectedIndex != -1)
                {
                    btn_Save_Messages.Visible = true;
                    Messages = lb_Messages.SelectedItem.ToString();
                    btn_Test_Messages.Visible = true;
                }
                else
                {
                    btn_Save_Messages.Visible = false;
                    Messages = null;
                    btn_Test_Messages.Visible = false;
                }
            }
        }
        #endregion

        private void tb_Sounds_Directory_TextChanged(object sender, EventArgs e)
        {
            if (!Init)
            {
                btn_Save_Sounds_Directory.Visible = true;
            }
        }

        private void btn_Done_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void Sounds_Load(object sender, EventArgs e)
        {
            if (Sounds_Directory != null)
            {
                tb_Sounds_Directory.Text = Sounds_Directory;

                // fill in the listboxes
                string[] filepaths = Directory.GetFiles(Sounds_Directory, "*.wav", SearchOption.TopDirectoryOnly);
                foreach (string filepath in filepaths)
                {
                    string name = filepath.Substring(Sounds_Directory.Length + 1);
                    lb_Connections.Items.Add(name);
                    lb_FilesDownload.Items.Add(name);
                    lb_Alerts.Items.Add(name);
                    lb_Messages.Items.Add(name);
                }

                // select the one for each listbox
                lb_Connections.SelectedItem = Connections;
                lb_FilesDownload.SelectedItem = File_Download;
                lb_Alerts.SelectedItem = Alerts;
                lb_Messages.SelectedItem = Messages;
//                Init = false;
            }
            Init = false;

        }
    }
}
