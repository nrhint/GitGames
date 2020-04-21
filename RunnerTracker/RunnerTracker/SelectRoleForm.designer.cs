namespace RunnerTracker
{
    partial class SelectRoleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_Browse_Directory_DB = new System.Windows.Forms.Button();
            this.tb_Directory_Name = new System.Windows.Forms.TextBox();
            this.btn_OK_DB = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btn_Yes = new System.Windows.Forms.Button();
            this.btn_No = new System.Windows.Forms.Button();
            this.lbl_File_Exists = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel_DB = new System.Windows.Forms.Panel();
            this.gb_ExpectedTypes = new System.Windows.Forms.GroupBox();
            this.chk_APRS = new System.Windows.Forms.CheckBox();
            this.chk_Packet = new System.Windows.Forms.CheckBox();
            this.chk_Ethernet = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel_Aid = new System.Windows.Forms.Panel();
            this.lb_StationName = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.gb_Comm_Medium = new System.Windows.Forms.GroupBox();
            this.rb_APRS = new System.Windows.Forms.RadioButton();
            this.rb_Packet = new System.Windows.Forms.RadioButton();
            this.rb_Ethernet = new System.Windows.Forms.RadioButton();
            this.rb_Cellphone = new System.Windows.Forms.RadioButton();
            this.btn_OK_Aid = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tb_Directory_Name_Aid = new System.Windows.Forms.TextBox();
            this.btn_Browse_Directory_Aid = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.gb_LogPoints = new System.Windows.Forms.GroupBox();
            this.rb_One_Log_Point = new System.Windows.Forms.RadioButton();
            this.rb_2_Log_Points = new System.Windows.Forms.RadioButton();
            this.cb_Using_RFID = new System.Windows.Forms.CheckBox();
            this.tb_StationName = new System.Windows.Forms.TextBox();
            this.lbl_StationName = new System.Windows.Forms.Label();
            this.rb_AidStation = new System.Windows.Forms.RadioButton();
            this.rb_Database = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTip_Medium = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_StationName_label = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_StationName_tb = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_RFID = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_LogPoints = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_directory = new System.Windows.Forms.ToolTip(this.components);
            this.btn_DB_Cancel = new System.Windows.Forms.Button();
            this.btn_Aid_Cancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel_DB.SuspendLayout();
            this.gb_ExpectedTypes.SuspendLayout();
            this.panel_Aid.SuspendLayout();
            this.gb_Comm_Medium.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gb_LogPoints.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(62, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(374, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select the directory to store this event\'s data.";
            // 
            // btn_Browse_Directory_DB
            // 
            this.btn_Browse_Directory_DB.BackColor = System.Drawing.Color.MistyRose;
            this.btn_Browse_Directory_DB.Location = new System.Drawing.Point(465, 145);
            this.btn_Browse_Directory_DB.Name = "btn_Browse_Directory_DB";
            this.btn_Browse_Directory_DB.Size = new System.Drawing.Size(28, 23);
            this.btn_Browse_Directory_DB.TabIndex = 5;
            this.btn_Browse_Directory_DB.Text = "...";
            this.toolTip_directory.SetToolTip(this.btn_Browse_Directory_DB, "Select or create a direcctory where\r\n  this event\'s data can be stored.");
            this.btn_Browse_Directory_DB.UseVisualStyleBackColor = false;
            this.btn_Browse_Directory_DB.Click += new System.EventHandler(this.btn_Browse_DB_Directory_Click);
            // 
            // tb_Directory_Name
            // 
            this.tb_Directory_Name.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.tb_Directory_Name.Location = new System.Drawing.Point(9, 146);
            this.tb_Directory_Name.Name = "tb_Directory_Name";
            this.tb_Directory_Name.Size = new System.Drawing.Size(450, 20);
            this.tb_Directory_Name.TabIndex = 4;
            this.toolTip_directory.SetToolTip(this.tb_Directory_Name, "Select or create a direcctory where\r\n  this event\'s data can be stored.");
            this.tb_Directory_Name.TextChanged += new System.EventHandler(this.tb_Directory_Name_TextChanged);
            // 
            // btn_OK_DB
            // 
            this.btn_OK_DB.BackColor = System.Drawing.Color.DarkSalmon;
            this.btn_OK_DB.Location = new System.Drawing.Point(164, 186);
            this.btn_OK_DB.Name = "btn_OK_DB";
            this.btn_OK_DB.Size = new System.Drawing.Size(75, 23);
            this.btn_OK_DB.TabIndex = 3;
            this.btn_OK_DB.Text = "OK";
            this.btn_OK_DB.UseVisualStyleBackColor = false;
            this.btn_OK_DB.Click += new System.EventHandler(this.btn_OK_DB_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(164, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(175, 25);
            this.label3.TabIndex = 6;
            this.label3.Text = "Runner Tracker";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::RunnerTracker.Properties.Resources.Runner;
            this.pictureBox1.Location = new System.Drawing.Point(201, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(96, 96);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // btn_Yes
            // 
            this.btn_Yes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btn_Yes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Yes.Location = new System.Drawing.Point(169, 290);
            this.btn_Yes.Name = "btn_Yes";
            this.btn_Yes.Size = new System.Drawing.Size(45, 23);
            this.btn_Yes.TabIndex = 8;
            this.btn_Yes.Text = "Yes";
            this.btn_Yes.UseVisualStyleBackColor = false;
            this.btn_Yes.Visible = false;
            this.btn_Yes.Click += new System.EventHandler(this.btn_Yes_Click);
            // 
            // btn_No
            // 
            this.btn_No.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btn_No.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_No.Location = new System.Drawing.Point(284, 290);
            this.btn_No.Name = "btn_No";
            this.btn_No.Size = new System.Drawing.Size(45, 23);
            this.btn_No.TabIndex = 9;
            this.btn_No.Text = "No";
            this.btn_No.UseVisualStyleBackColor = false;
            this.btn_No.Visible = false;
            this.btn_No.Click += new System.EventHandler(this.btn_No_Click);
            // 
            // lbl_File_Exists
            // 
            this.lbl_File_Exists.AutoSize = true;
            this.lbl_File_Exists.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_File_Exists.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.lbl_File_Exists.Location = new System.Drawing.Point(62, 234);
            this.lbl_File_Exists.Name = "lbl_File_Exists";
            this.lbl_File_Exists.Size = new System.Drawing.Size(379, 40);
            this.lbl_File_Exists.TabIndex = 10;
            this.lbl_File_Exists.Text = "One or more Runner data files exists in this directory!\r\n     Do you want to use " +
    "one of these existing files?";
            this.lbl_File_Exists.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(199, 142);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "(c) 2019 Mesa Micro";
            // 
            // panel_DB
            // 
            this.panel_DB.BackColor = System.Drawing.Color.PeachPuff;
            this.panel_DB.Controls.Add(this.btn_DB_Cancel);
            this.panel_DB.Controls.Add(this.gb_ExpectedTypes);
            this.panel_DB.Controls.Add(this.label6);
            this.panel_DB.Controls.Add(this.btn_OK_DB);
            this.panel_DB.Controls.Add(this.label1);
            this.panel_DB.Controls.Add(this.lbl_File_Exists);
            this.panel_DB.Controls.Add(this.btn_Browse_Directory_DB);
            this.panel_DB.Controls.Add(this.btn_No);
            this.panel_DB.Controls.Add(this.tb_Directory_Name);
            this.panel_DB.Controls.Add(this.btn_Yes);
            this.panel_DB.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_DB.Location = new System.Drawing.Point(0, 209);
            this.panel_DB.Name = "panel_DB";
            this.panel_DB.Size = new System.Drawing.Size(502, 343);
            this.panel_DB.TabIndex = 12;
            this.panel_DB.Visible = false;
            // 
            // gb_ExpectedTypes
            // 
            this.gb_ExpectedTypes.BackColor = System.Drawing.Color.SeaShell;
            this.gb_ExpectedTypes.Controls.Add(this.chk_APRS);
            this.gb_ExpectedTypes.Controls.Add(this.chk_Packet);
            this.gb_ExpectedTypes.Controls.Add(this.chk_Ethernet);
            this.gb_ExpectedTypes.Location = new System.Drawing.Point(100, 52);
            this.gb_ExpectedTypes.Name = "gb_ExpectedTypes";
            this.gb_ExpectedTypes.Size = new System.Drawing.Size(303, 50);
            this.gb_ExpectedTypes.TabIndex = 12;
            this.gb_ExpectedTypes.TabStop = false;
            this.gb_ExpectedTypes.Text = "Connection Types Expected";
            // 
            // chk_APRS
            // 
            this.chk_APRS.AutoSize = true;
            this.chk_APRS.Location = new System.Drawing.Point(238, 21);
            this.chk_APRS.Name = "chk_APRS";
            this.chk_APRS.Size = new System.Drawing.Size(55, 17);
            this.chk_APRS.TabIndex = 2;
            this.chk_APRS.Text = "APRS";
            this.chk_APRS.UseVisualStyleBackColor = true;
            // 
            // chk_Packet
            // 
            this.chk_Packet.AutoSize = true;
            this.chk_Packet.Location = new System.Drawing.Point(153, 21);
            this.chk_Packet.Name = "chk_Packet";
            this.chk_Packet.Size = new System.Drawing.Size(60, 17);
            this.chk_Packet.TabIndex = 1;
            this.chk_Packet.Text = "Packet";
            this.chk_Packet.UseVisualStyleBackColor = true;
            // 
            // chk_Ethernet
            // 
            this.chk_Ethernet.AutoSize = true;
            this.chk_Ethernet.Location = new System.Drawing.Point(25, 21);
            this.chk_Ethernet.Name = "chk_Ethernet";
            this.chk_Ethernet.Size = new System.Drawing.Size(106, 17);
            this.chk_Ethernet.TabIndex = 0;
            this.chk_Ethernet.Text = "Ethernet (MESH)";
            this.chk_Ethernet.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Purple;
            this.label6.Location = new System.Drawing.Point(124, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(254, 24);
            this.label6.TabIndex = 11;
            this.label6.Text = "Central Database Settings:";
            // 
            // panel_Aid
            // 
            this.panel_Aid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panel_Aid.Controls.Add(this.btn_Aid_Cancel);
            this.panel_Aid.Controls.Add(this.lb_StationName);
            this.panel_Aid.Controls.Add(this.label7);
            this.panel_Aid.Controls.Add(this.gb_Comm_Medium);
            this.panel_Aid.Controls.Add(this.btn_OK_Aid);
            this.panel_Aid.Controls.Add(this.panel1);
            this.panel_Aid.Controls.Add(this.gb_LogPoints);
            this.panel_Aid.Controls.Add(this.cb_Using_RFID);
            this.panel_Aid.Controls.Add(this.tb_StationName);
            this.panel_Aid.Controls.Add(this.lbl_StationName);
            this.panel_Aid.Location = new System.Drawing.Point(0, 209);
            this.panel_Aid.Name = "panel_Aid";
            this.panel_Aid.Size = new System.Drawing.Size(502, 343);
            this.panel_Aid.TabIndex = 13;
            this.panel_Aid.Visible = false;
            // 
            // lb_StationName
            // 
            this.lb_StationName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.lb_StationName.FormattingEnabled = true;
            this.lb_StationName.Location = new System.Drawing.Point(196, 45);
            this.lb_StationName.Name = "lb_StationName";
            this.lb_StationName.Size = new System.Drawing.Size(200, 17);
            this.lb_StationName.TabIndex = 30;
            this.lb_StationName.SelectedIndexChanged += new System.EventHandler(this.lb_StationName_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Purple;
            this.label7.Location = new System.Drawing.Point(153, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(196, 24);
            this.label7.TabIndex = 29;
            this.label7.Text = "Aid Station Settings:";
            // 
            // gb_Comm_Medium
            // 
            this.gb_Comm_Medium.BackColor = System.Drawing.Color.AntiqueWhite;
            this.gb_Comm_Medium.Controls.Add(this.rb_APRS);
            this.gb_Comm_Medium.Controls.Add(this.rb_Packet);
            this.gb_Comm_Medium.Controls.Add(this.rb_Ethernet);
            this.gb_Comm_Medium.Controls.Add(this.rb_Cellphone);
            this.gb_Comm_Medium.Location = new System.Drawing.Point(99, 73);
            this.gb_Comm_Medium.Name = "gb_Comm_Medium";
            this.gb_Comm_Medium.Size = new System.Drawing.Size(305, 93);
            this.gb_Comm_Medium.TabIndex = 28;
            this.gb_Comm_Medium.TabStop = false;
            this.gb_Comm_Medium.Text = "Select Communication Medium";
            this.toolTip_Medium.SetToolTip(this.gb_Comm_Medium, "How will the data be transferred to\r\n  and from the Central Database?");
            // 
            // rb_APRS
            // 
            this.rb_APRS.AutoSize = true;
            this.rb_APRS.Location = new System.Drawing.Point(95, 19);
            this.rb_APRS.Name = "rb_APRS";
            this.rb_APRS.Size = new System.Drawing.Size(54, 17);
            this.rb_APRS.TabIndex = 14;
            this.rb_APRS.TabStop = true;
            this.rb_APRS.Text = "APRS";
            this.rb_APRS.UseVisualStyleBackColor = true;
            this.rb_APRS.CheckedChanged += new System.EventHandler(this.rb_APRS_CheckedChanged);
            // 
            // rb_Packet
            // 
            this.rb_Packet.AutoSize = true;
            this.rb_Packet.Location = new System.Drawing.Point(95, 37);
            this.rb_Packet.Name = "rb_Packet";
            this.rb_Packet.Size = new System.Drawing.Size(59, 17);
            this.rb_Packet.TabIndex = 15;
            this.rb_Packet.TabStop = true;
            this.rb_Packet.Text = "Packet";
            this.rb_Packet.UseVisualStyleBackColor = true;
            this.rb_Packet.CheckedChanged += new System.EventHandler(this.rb_Packet_CheckedChanged);
            // 
            // rb_Ethernet
            // 
            this.rb_Ethernet.AutoSize = true;
            this.rb_Ethernet.Location = new System.Drawing.Point(95, 55);
            this.rb_Ethernet.Name = "rb_Ethernet";
            this.rb_Ethernet.Size = new System.Drawing.Size(105, 17);
            this.rb_Ethernet.TabIndex = 16;
            this.rb_Ethernet.TabStop = true;
            this.rb_Ethernet.Text = "Ethernet (MESH)";
            this.rb_Ethernet.UseVisualStyleBackColor = true;
            this.rb_Ethernet.CheckedChanged += new System.EventHandler(this.rb_Ethernet_CheckedChanged);
            // 
            // rb_Cellphone
            // 
            this.rb_Cellphone.AutoSize = true;
            this.rb_Cellphone.Enabled = false;
            this.rb_Cellphone.Location = new System.Drawing.Point(95, 73);
            this.rb_Cellphone.Name = "rb_Cellphone";
            this.rb_Cellphone.Size = new System.Drawing.Size(115, 17);
            this.rb_Cellphone.TabIndex = 17;
            this.rb_Cellphone.TabStop = true;
            this.rb_Cellphone.Text = "Cell phone (texting)";
            this.rb_Cellphone.UseVisualStyleBackColor = true;
            this.rb_Cellphone.Visible = false;
            this.rb_Cellphone.CheckedChanged += new System.EventHandler(this.rb_Cellphone_CheckedChanged);
            // 
            // btn_OK_Aid
            // 
            this.btn_OK_Aid.BackColor = System.Drawing.Color.DarkSalmon;
            this.btn_OK_Aid.Location = new System.Drawing.Point(163, 305);
            this.btn_OK_Aid.Name = "btn_OK_Aid";
            this.btn_OK_Aid.Size = new System.Drawing.Size(75, 23);
            this.btn_OK_Aid.TabIndex = 22;
            this.btn_OK_Aid.Text = "OK";
            this.btn_OK_Aid.UseVisualStyleBackColor = false;
            this.btn_OK_Aid.Click += new System.EventHandler(this.btn_OK_Aid_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(220)))), ((int)(((byte)(90)))));
            this.panel1.Controls.Add(this.tb_Directory_Name_Aid);
            this.panel1.Controls.Add(this.btn_Browse_Directory_Aid);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Location = new System.Drawing.Point(0, 218);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(502, 76);
            this.panel1.TabIndex = 23;
            this.toolTip_directory.SetToolTip(this.panel1, "Select or create a direcctory where\r\n  this event\'s data can be stored.");
            // 
            // tb_Directory_Name_Aid
            // 
            this.tb_Directory_Name_Aid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.tb_Directory_Name_Aid.Location = new System.Drawing.Point(9, 42);
            this.tb_Directory_Name_Aid.Name = "tb_Directory_Name_Aid";
            this.tb_Directory_Name_Aid.Size = new System.Drawing.Size(450, 20);
            this.tb_Directory_Name_Aid.TabIndex = 23;
            this.tb_Directory_Name_Aid.TextChanged += new System.EventHandler(this.tb_Directory_Name_Aid_TextChanged);
            // 
            // btn_Browse_Directory_Aid
            // 
            this.btn_Browse_Directory_Aid.BackColor = System.Drawing.Color.MistyRose;
            this.btn_Browse_Directory_Aid.Location = new System.Drawing.Point(465, 41);
            this.btn_Browse_Directory_Aid.Name = "btn_Browse_Directory_Aid";
            this.btn_Browse_Directory_Aid.Size = new System.Drawing.Size(28, 23);
            this.btn_Browse_Directory_Aid.TabIndex = 24;
            this.btn_Browse_Directory_Aid.Text = "...";
            this.btn_Browse_Directory_Aid.UseVisualStyleBackColor = false;
            this.btn_Browse_Directory_Aid.Click += new System.EventHandler(this.btn_Browse_Directory_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(64, 11);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(374, 20);
            this.label5.TabIndex = 20;
            this.label5.Text = "Select the directory to store this event\'s data.";
            // 
            // gb_LogPoints
            // 
            this.gb_LogPoints.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.gb_LogPoints.Controls.Add(this.rb_One_Log_Point);
            this.gb_LogPoints.Controls.Add(this.rb_2_Log_Points);
            this.gb_LogPoints.Location = new System.Drawing.Point(272, 172);
            this.gb_LogPoints.Name = "gb_LogPoints";
            this.gb_LogPoints.Size = new System.Drawing.Size(116, 38);
            this.gb_LogPoints.TabIndex = 27;
            this.gb_LogPoints.TabStop = false;
            this.gb_LogPoints.Text = "Log Points";
            this.toolTip_LogPoints.SetToolTip(this.gb_LogPoints, "Does this station log runners at\r\nIn and Out, or just upon Arrival?");
            // 
            // rb_One_Log_Point
            // 
            this.rb_One_Log_Point.AutoSize = true;
            this.rb_One_Log_Point.Location = new System.Drawing.Point(24, 16);
            this.rb_One_Log_Point.Name = "rb_One_Log_Point";
            this.rb_One_Log_Point.Size = new System.Drawing.Size(31, 17);
            this.rb_One_Log_Point.TabIndex = 2;
            this.rb_One_Log_Point.TabStop = true;
            this.rb_One_Log_Point.Text = "1";
            this.rb_One_Log_Point.UseVisualStyleBackColor = true;
            this.rb_One_Log_Point.CheckedChanged += new System.EventHandler(this.rb_One_Log_Point_CheckedChanged);
            // 
            // rb_2_Log_Points
            // 
            this.rb_2_Log_Points.AutoSize = true;
            this.rb_2_Log_Points.Location = new System.Drawing.Point(71, 16);
            this.rb_2_Log_Points.Name = "rb_2_Log_Points";
            this.rb_2_Log_Points.Size = new System.Drawing.Size(31, 17);
            this.rb_2_Log_Points.TabIndex = 3;
            this.rb_2_Log_Points.TabStop = true;
            this.rb_2_Log_Points.Text = "2";
            this.rb_2_Log_Points.UseVisualStyleBackColor = true;
            // 
            // cb_Using_RFID
            // 
            this.cb_Using_RFID.AutoSize = true;
            this.cb_Using_RFID.Location = new System.Drawing.Point(115, 182);
            this.cb_Using_RFID.Name = "cb_Using_RFID";
            this.cb_Using_RFID.Size = new System.Drawing.Size(104, 17);
            this.cb_Using_RFID.TabIndex = 26;
            this.cb_Using_RFID.Text = "Using RFID tags";
            this.toolTip_RFID.SetToolTip(this.cb_Using_RFID, "Is this event using RFID bibs/tags?");
            this.cb_Using_RFID.UseVisualStyleBackColor = true;
            // 
            // tb_StationName
            // 
            this.tb_StationName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.tb_StationName.Location = new System.Drawing.Point(27, 19);
            this.tb_StationName.Name = "tb_StationName";
            this.tb_StationName.Size = new System.Drawing.Size(116, 20);
            this.tb_StationName.TabIndex = 25;
            this.tb_StationName.Text = "Not yet identified";
            this.toolTip_StationName_tb.SetToolTip(this.tb_StationName, "Enter or verify name for this Aid Station");
            this.tb_StationName.Visible = false;
            this.tb_StationName.TextChanged += new System.EventHandler(this.tb_StationName_Aid_TextChanged);
            // 
            // lbl_StationName
            // 
            this.lbl_StationName.AutoSize = true;
            this.lbl_StationName.Location = new System.Drawing.Point(106, 47);
            this.lbl_StationName.Name = "lbl_StationName";
            this.lbl_StationName.Size = new System.Drawing.Size(90, 13);
            this.lbl_StationName.TabIndex = 24;
            this.lbl_StationName.Text = "Aid Station name:";
            this.toolTip_StationName_label.SetToolTip(this.lbl_StationName, "Enter or verify name for this Aid Station");
            // 
            // rb_AidStation
            // 
            this.rb_AidStation.AutoSize = true;
            this.rb_AidStation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rb_AidStation.Location = new System.Drawing.Point(180, 172);
            this.rb_AidStation.Name = "rb_AidStation";
            this.rb_AidStation.Size = new System.Drawing.Size(101, 20);
            this.rb_AidStation.TabIndex = 14;
            this.rb_AidStation.TabStop = true;
            this.rb_AidStation.Text = "Aid Station";
            this.rb_AidStation.UseVisualStyleBackColor = true;
            this.rb_AidStation.CheckedChanged += new System.EventHandler(this.rb_AidStation_CheckedChanged);
            // 
            // rb_Database
            // 
            this.rb_Database.AutoSize = true;
            this.rb_Database.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rb_Database.Location = new System.Drawing.Point(293, 172);
            this.rb_Database.Name = "rb_Database";
            this.rb_Database.Size = new System.Drawing.Size(147, 20);
            this.rb_Database.TabIndex = 15;
            this.rb_Database.TabStop = true;
            this.rb_Database.Text = "Central Database";
            this.rb_Database.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Fuchsia;
            this.label4.Location = new System.Drawing.Point(63, 171);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 22);
            this.label4.TabIndex = 16;
            this.label4.Text = "Select Role:";
            // 
            // btn_DB_Cancel
            // 
            this.btn_DB_Cancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btn_DB_Cancel.Location = new System.Drawing.Point(264, 186);
            this.btn_DB_Cancel.Name = "btn_DB_Cancel";
            this.btn_DB_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_DB_Cancel.TabIndex = 13;
            this.btn_DB_Cancel.Text = "Cancel";
            this.btn_DB_Cancel.UseVisualStyleBackColor = false;
            this.btn_DB_Cancel.Click += new System.EventHandler(this.btn_DB_Cancel_Click);
            // 
            // btn_Aid_Cancel
            // 
            this.btn_Aid_Cancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btn_Aid_Cancel.Location = new System.Drawing.Point(265, 305);
            this.btn_Aid_Cancel.Name = "btn_Aid_Cancel";
            this.btn_Aid_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Aid_Cancel.TabIndex = 31;
            this.btn_Aid_Cancel.Text = "Cancel";
            this.btn_Aid_Cancel.UseVisualStyleBackColor = false;
            this.btn_Aid_Cancel.Click += new System.EventHandler(this.btn_Aid_Cancel_Click);
            // 
            // SelectRoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(502, 552);
            this.ControlBox = false;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.rb_Database);
            this.Controls.Add(this.rb_AidStation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel_Aid);
            this.Controls.Add(this.panel_DB);
            this.Name = "SelectRoleForm";
            this.ShowIcon = false;
            this.Shown += new System.EventHandler(this.SelectRoleForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel_DB.ResumeLayout(false);
            this.panel_DB.PerformLayout();
            this.gb_ExpectedTypes.ResumeLayout(false);
            this.gb_ExpectedTypes.PerformLayout();
            this.panel_Aid.ResumeLayout(false);
            this.panel_Aid.PerformLayout();
            this.gb_Comm_Medium.ResumeLayout(false);
            this.gb_Comm_Medium.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gb_LogPoints.ResumeLayout(false);
            this.gb_LogPoints.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Browse_Directory_DB;
        private System.Windows.Forms.TextBox tb_Directory_Name;
        private System.Windows.Forms.Button btn_OK_DB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btn_Yes;
        private System.Windows.Forms.Button btn_No;
        private System.Windows.Forms.Label lbl_File_Exists;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel_DB;
        private System.Windows.Forms.Panel panel_Aid;
        private System.Windows.Forms.GroupBox gb_Comm_Medium;
        private System.Windows.Forms.RadioButton rb_APRS;
        private System.Windows.Forms.RadioButton rb_Packet;
        private System.Windows.Forms.RadioButton rb_Ethernet;
        private System.Windows.Forms.RadioButton rb_Cellphone;
        private System.Windows.Forms.Button btn_OK_Aid;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tb_Directory_Name_Aid;
        private System.Windows.Forms.Button btn_Browse_Directory_Aid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox gb_LogPoints;
        private System.Windows.Forms.RadioButton rb_One_Log_Point;
        private System.Windows.Forms.RadioButton rb_2_Log_Points;
        private System.Windows.Forms.CheckBox cb_Using_RFID;
        private System.Windows.Forms.TextBox tb_StationName;
        private System.Windows.Forms.Label lbl_StationName;
        private System.Windows.Forms.RadioButton rb_AidStation;
        private System.Windows.Forms.RadioButton rb_Database;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolTip toolTip_Medium;
        private System.Windows.Forms.ToolTip toolTip_StationName_label;
        private System.Windows.Forms.ToolTip toolTip_StationName_tb;
        private System.Windows.Forms.ToolTip toolTip_RFID;
        private System.Windows.Forms.ToolTip toolTip_LogPoints;
        private System.Windows.Forms.ToolTip toolTip_directory;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox gb_ExpectedTypes;
        private System.Windows.Forms.CheckBox chk_APRS;
        private System.Windows.Forms.CheckBox chk_Packet;
        private System.Windows.Forms.CheckBox chk_Ethernet;
        private System.Windows.Forms.ListBox lb_StationName;
        private System.Windows.Forms.Button btn_DB_Cancel;
        private System.Windows.Forms.Button btn_Aid_Cancel;
    }
}