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
    public partial class ImportRunnerData : Form
    {
        // variables
        int Bib = -1, First = -1, Last = -1, Full = -1;

        public ImportRunnerData()
        {
            InitializeComponent();
        }

        private void tb_Num_Columns_TextChanged(object sender, EventArgs e)
        {
            if (tb_Num_Columns.Text == "")
                tb_Num_Columns.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_Num_Columns.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Data_Start_Line_TextChanged(object sender, EventArgs e)
        {
            if (tb_Data_Start_Line.Text == "")
                tb_Data_Start_Line.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_Data_Start_Line.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Source_File_Name_TextChanged(object sender, EventArgs e)
        {
            if (tb_Source_File_Name.Text == "")
                tb_Source_File_Name.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_Source_File_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void tb_Destination_File_Name_TextChanged(object sender, EventArgs e)
        {
            if (tb_Destination_File_Name.Text == "")
                tb_Destination_File_Name.BackColor = Color.FromArgb(255, 255, 128);
            else
                tb_Destination_File_Name.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        private void btn_Source_File_Click(object sender, EventArgs e)
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
                tb_Source_File_Name.Text = folderPath;
            }
        }

        private void btn_Browse_Destination_File_Click(object sender, EventArgs e)
        {
            string folderPath = "";     // set this to the previous value

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
//            ofd.CheckFileExists = true;
            ofd.CheckFileExists = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                folderPath = ofd.FileName;
                tb_Destination_File_Name.Text = folderPath;
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btn_Import_Click(object sender, EventArgs e)
        {
            string line;
            string[] Parts;
            char[] splitter = new char[] { ',' };
            char[] front = new char[] { ' ' };
            int linenum = 1;
            StreamReader reader;
            StreamWriter writer;

            // verify that all 4 textboxes have data
            if ((tb_Num_Columns.Text == "") || (tb_Data_Start_Line.Text == "") || (tb_Source_File_Name.Text == "") || (tb_Destination_File_Name.Text == ""))
            {
                MessageBox.Show("Missing some required entry!", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit now
            }

            // verify that a Bib number column has been designated
            if (Bib == -1)
            {
                MessageBox.Show("Bib number has not been designated!", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit now
            }

            // if Full name is not designated, then need First and Last
            if (Full == -1)
            {
                if ((First == -1) || (Last == -1))
                {
                    MessageBox.Show("Proper name has not been designated!", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;     // quit now
                }
            }

            // open source file
            try
            {
                reader = File.OpenText(tb_Source_File_Name.Text);
            }
            catch
            {
                MessageBox.Show("Selected file:\n\n" + tb_Source_File_Name.Text + "\n\nis not accessible!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit now
            }

            // open destination file
            try
            {
                writer = File.CreateText(tb_Destination_File_Name.Text);
            }
            catch
            {
                MessageBox.Show("Selected file:\n\n" + tb_Destination_File_Name.Text + "\n\ncould not be created!", "Invalid file?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;     // quit now
            }

            // copy data from source to destination
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (linenum == Convert.ToInt16(tb_Data_Start_Line.Text))
                {       // found start of data in file
                    string newline;
                    Parts = line.Split(splitter);
                    if (Full != -1)     // was Full name designated?
                        newline = Parts[Bib] + "," + Parts[Full];   // yes, use Full name
                    else
                        newline = Parts[Bib] + "," + Parts[First] + " " + Parts[Last];  // no, combine First and Last to make Full name
                    writer.WriteLine(newline);
                }
                else
                {       // have not found start of data in file yet
                    linenum++;
                }
            }

            // close files
            reader.Close();
            writer.Close();

            // quit
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void lb_Col1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectItem(1, lb_Col1.SelectedIndex);
        }

        private void lb_Col2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectItem(2, lb_Col2.SelectedIndex);
        }

        private void lb_Col3_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectItem(3, lb_Col3.SelectedIndex);
        }

        private void lb_Col4_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectItem(4, lb_Col4.SelectedIndex);
        }

        private void lb_Col5_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectItem(5, lb_Col5.SelectedIndex);
        }

        void SelectItem(int column, int selected)
        {
            bool ret = true;

            switch (selected)
            {
                case 0:     // Bib number
                    if (Bib != -1)
                    {
                        MessageBox.Show("Bib number has already been assigned to another column!", "Illegal assignment", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        ret = false;
                    }
                    else
                        Bib = column-1;
                    break;
                case 1:     // First name
                    if (First != -1)
                    {
                        MessageBox.Show("First name has already been assigned to another column!", "Illegal assignment", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        ret = false;
                    }
                    else
                        First = column-1;
                    break;
                case 2:     // Last name
                    if (Last != -1)
                    {
                        MessageBox.Show("Last name has already been assigned to another column!", "Illegal assignment", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        ret = false;
                    }
                    else
                        Last = column-1;
                    break;
                case 3:     // Full name
                    if (Full != -1)
                    {
                        MessageBox.Show("Full name has already been assigned to another column!", "Illegal assignment", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        ret = false;
                    }
                    else
                        Full = column-1;
                    break;
            }

            // was there a pre-assignment?
            if (ret)
                return;
            else
            {       // yes - the desired column selection was already selected in another column, need to deselect this column again
                switch (column)
                {
                    case 1:
                        lb_Col1.SelectedIndex = -1;
                        break;
                    case 2:
                        lb_Col2.SelectedIndex = -1;
                        break;
                    case 3:
                        lb_Col3.SelectedIndex = -1;
                        break;
                    case 4:
                        lb_Col4.SelectedIndex = -1;
                        break;
                    case 5:
                        lb_Col5.SelectedIndex = -1;
                        break;
                }
            }
        }
    }
}
