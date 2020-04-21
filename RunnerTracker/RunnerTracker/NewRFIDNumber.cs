using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RunnerTracker
{
    public partial class NewRFIDNumber : Form
    {
        public string BibNumber;
        public string RunnerNumber;
        public bool SaveNewNumber;

        public NewRFIDNumber()
        {
            InitializeComponent();

            lbl_RFID_number.Text = BibNumber;
            SaveNewNumber = false;
        }

        private void btn_Create_Click(object sender, EventArgs e)
        {
            // verify an entry has been made in the new Runner's number
            if (tb_Bib_number.Text != "")
            {
                // verify that it is a number
                int result;
                bool good = int.TryParse(tb_Bib_number.Text, out result);
                if (good)
                {
                    RunnerNumber = result.ToString();
                    SaveNewNumber = cb_Save_Newnumber.Checked;
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                }
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
