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
    public partial class SelectStationForm : Form
    {
        public string Station_Name = string.Empty;
        int dgv_Height;

        public SelectStationForm()
        {
            InitializeComponent();

            // Bind the Stations DGV
            dgv_Stations.DataSource = null;
            dgv_Stations.DataSource = Form1.Stations;
            dgv_Stations.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Stations.Columns[0].Width = 75;
            dgv_Stations.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_Stations.Columns[0].HeaderText = "Station Name";
            dgv_Stations.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_Stations.Columns[1].Visible = false;     // Number
            dgv_Stations.Columns[2].Visible = false;     // Latitude
            dgv_Stations.Columns[3].Visible = false;     // Longitude
            dgv_Stations.Columns[4].Visible = false;     // Previous
            dgv_Stations.Columns[5].Visible = false;     // Distance to Previous
            dgv_Stations.Columns[6].Visible = false;     // Next
            dgv_Stations.Columns[7].Visible = false;     // Distance to Next
            dgv_Stations.Columns[8].Visible = false;     // Difficulty
            dgv_Stations.Columns[9].Visible = false;     // Accessible
            dgv_Stations.Columns[10].Visible = false;    // Number of Log points (1 or 2)
            dgv_Stations.Columns[11].Visible = false;    // # of Log pts
            dgv_Stations.Columns[12].Visible = false;    // First Runner Expected
            dgv_Stations.Columns[13].Visible = false;    // Cutoff Time"
            dgv_Stations.Columns[14].Visible = false;
            dgv_Stations.Columns[15].Visible = false;
            dgv_Stations.Columns[16].Visible = false;
            dgv_Stations.Columns[17].Visible = false;
            dgv_Stations.Columns[18].Visible = false;
            dgv_Stations.Columns[19].Visible = false;
            dgv_Stations.Columns[20].Visible = false;
            dgv_Stations.Columns[21].Visible = false;

            // change the size of the DGV to hold only the Stations - 4/22/19
            dgv_Height = (Form1.Num_Stations-1) * dgv_Stations.Rows[0].Height + dgv_Stations.ColumnHeadersHeight + 2;
//            dgv_Stations.ClientSize = new Size(78, (Form1.Num_Stations-1) * dgv_Stations.Rows[0].Height + dgv_Stations.ColumnHeadersHeight + 2);
            dgv_Stations.ClientSize = new Size(78, dgv_Height);
            dgv_Stations.Update();

            // adjust the height of the entire form
            if (dgv_Height > 100)   // 3 stations = 69, 4 stations = 91, 5 stations = 113
                this.Height += dgv_Height - 125;
            else
                this.Height -= 125 - dgv_Height;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            // save the station name
            Station_Name = (string)dgv_Stations.CurrentCell.Value;

            // quit
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
