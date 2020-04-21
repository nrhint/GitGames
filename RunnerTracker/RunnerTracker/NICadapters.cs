using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace RunnerTracker
{
    public partial class NICadapters : Form
    {
        #region Variables
        NetworkInterface[] FoundAdapters;
        int selected = -1;
        public string IP_Address = string.Empty;
        OperationalStatus status;
        public class Adapter
        {
            public string Interface_Type { get; set; }
            public string IP_Address { get; set; }
            public string Mask { get; set; }
            public string Gateway_Address { get; set; }
            public string Status { get; set; }
            public int NI_index { get; set; }
        }
        List<Adapter> Found_Adapters = new List<Adapter>();
        int i, j;
        #endregion

        public NICadapters(NetworkInterface[] adapters)
        {
            InitializeComponent();

            // save a copy of the adapters array
            FoundAdapters = adapters;

            // put the adapter info in the Found_Adapters List
            for (i=0; i<adapters.Length; i++)
            {
                IPInterfaceProperties properties1 = adapters[i].GetIPProperties();
                Adapter adapter = new Adapter();
                adapter.NI_index = i;
                adapter.Interface_Type = adapters[i].NetworkInterfaceType.ToString();
                status = adapters[i].OperationalStatus;
                if (status == OperationalStatus.Up)
                {
                    j = 0;
                    while (properties1.UnicastAddresses[j].Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                        j++;
                    adapter.IP_Address = properties1.UnicastAddresses[j].Address.ToString();
                    adapter.Mask = properties1.UnicastAddresses[j].IPv4Mask.ToString();
                    if (properties1.GatewayAddresses.Count != 0)
                        adapter.Gateway_Address = properties1.GatewayAddresses[0].Address.ToString();
                }
                //else
                //{
                //    if (status == OperationalStatus.Down)
                //        adapter.IP_Address = "Media disconnected";
                //    else
                //        adapter.IP_Address = adapters[i].OperationalStatus.ToString(); ;
                //}

                switch (status)
                {
                    case OperationalStatus.Up:
                        adapter.Status = "Up";
                        break;
                    case OperationalStatus.Down:
                        adapter.Status = "Down";
                        break;
                    default:
                        break;
                }

                if (adapters[i].NetworkInterfaceType != NetworkInterfaceType.Loopback && adapters[i].NetworkInterfaceType != NetworkInterfaceType.Tunnel)   // add only if not these types
                    Found_Adapters.Add(adapter);
            }

            // display in the DGV
            Bind_DGV();
        }

        delegate void BindDGVDel();
        private void Bind_DGV()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (DGV_Adapters.InvokeRequired)
            {
                BindDGVDel d = new BindDGVDel(Bind_DGV);
                DGV_Adapters.Invoke(d, new object[] { });
            }
            else
            {
                DGV_Adapters.DataSource = null;
                DGV_Adapters.DataSource = Found_Adapters;
                DGV_Adapters.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DGV_Adapters.Columns[0].Width = 90;     // Name
                DGV_Adapters.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DGV_Adapters.Columns[0].HeaderText = "Interface Type";
                DGV_Adapters.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                DGV_Adapters.Columns[1].Width = 110;     // IP Address
                DGV_Adapters.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DGV_Adapters.Columns[1].HeaderText = "IP Address";
                DGV_Adapters.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                DGV_Adapters.Columns[2].Width = 80;     // Mask
                DGV_Adapters.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DGV_Adapters.Columns[2].HeaderText = "Mask";
                DGV_Adapters.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                DGV_Adapters.Columns[3].Width = 100;     // Gateway Address
                DGV_Adapters.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DGV_Adapters.Columns[3].HeaderText = "Gateway Address";
                DGV_Adapters.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                DGV_Adapters.Columns[4].Width = 60;     // Status
                DGV_Adapters.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DGV_Adapters.Columns[4].HeaderText = "Status";
                DGV_Adapters.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
                DGV_Adapters.Columns[5].Visible = false;        // the NetworkInterface index
                DGV_Adapters.ClientSize = new Size(DGV_Adapters.Columns.GetColumnsWidth(DataGridViewElementStates.None) + 3 - 100, Found_Adapters.Count * DGV_Adapters.Rows[0].Height + DGV_Adapters.ColumnHeadersHeight);
                DGV_Adapters.Update();
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            // determine which row has been selected
            int rowcount = DGV_Adapters.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (rowcount == 0)
            {
                MessageBox.Show("You must select an adapter\n\nor click the Cancel button!", "Select an Adapter", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            int row = DGV_Adapters.CurrentCell.RowIndex;

            // verify that this adapter has an IP address and Gateway address
            IP_Address = Form1.Test_IP_GW(FoundAdapters[Found_Adapters[row].NI_index]);
            if ((IP_Address == "") || (IP_Address.Contains("not available")))
            {
                MessageBox.Show("    This adapter is missing an IP or Gateway Address\n\nPlease select a different one or click the Cancel button", "Missing address", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // verify that this adapter has a status of 'Up'
            if (Found_Adapters[row].Status != "Up")
                MessageBox.Show("            This adapter's Status is not 'Up'\n\nPlease select a different one or click the Cancel button", "Missing address", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void DGV_Adapters_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DGV_Adapters.ClearSelection();      // this deselects the first row on loading
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Abort;
        }
    }
}
