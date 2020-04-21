namespace RunnerTracker
{
    partial class ImportRunnerData
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_Source_File_Name = new System.Windows.Forms.TextBox();
            this.tb_Destination_File_Name = new System.Windows.Forms.TextBox();
            this.btn_Browse_Destination_File = new System.Windows.Forms.Button();
            this.btn_Source_File = new System.Windows.Forms.Button();
            this.tb_Data_Start_Line = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lb_Col1 = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lb_Col3 = new System.Windows.Forms.ListBox();
            this.lb_Col2 = new System.Windows.Forms.ListBox();
            this.lb_Col4 = new System.Windows.Forms.ListBox();
            this.lb_Col5 = new System.Windows.Forms.ListBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tb_Num_Columns = new System.Windows.Forms.TextBox();
            this.btn_Import = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(220, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 40);
            this.label1.TabIndex = 0;
            this.label1.Text = "Import Runner Data File\r\n  to Create Runner List";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 308);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Source File:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 346);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Destination File:";
            // 
            // tb_Source_File_Name
            // 
            this.tb_Source_File_Name.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tb_Source_File_Name.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.tb_Source_File_Name.Location = new System.Drawing.Point(100, 305);
            this.tb_Source_File_Name.Name = "tb_Source_File_Name";
            this.tb_Source_File_Name.Size = new System.Drawing.Size(458, 20);
            this.tb_Source_File_Name.TabIndex = 3;
            this.tb_Source_File_Name.TextChanged += new System.EventHandler(this.tb_Source_File_Name_TextChanged);
            // 
            // tb_Destination_File_Name
            // 
            this.tb_Destination_File_Name.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tb_Destination_File_Name.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.tb_Destination_File_Name.Location = new System.Drawing.Point(100, 343);
            this.tb_Destination_File_Name.Name = "tb_Destination_File_Name";
            this.tb_Destination_File_Name.Size = new System.Drawing.Size(458, 20);
            this.tb_Destination_File_Name.TabIndex = 4;
            this.tb_Destination_File_Name.TextChanged += new System.EventHandler(this.tb_Destination_File_Name_TextChanged);
            // 
            // btn_Browse_Destination_File
            // 
            this.btn_Browse_Destination_File.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Browse_Destination_File.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btn_Browse_Destination_File.Location = new System.Drawing.Point(564, 341);
            this.btn_Browse_Destination_File.Name = "btn_Browse_Destination_File";
            this.btn_Browse_Destination_File.Size = new System.Drawing.Size(28, 23);
            this.btn_Browse_Destination_File.TabIndex = 6;
            this.btn_Browse_Destination_File.Text = "...";
            this.btn_Browse_Destination_File.UseVisualStyleBackColor = false;
            this.btn_Browse_Destination_File.Click += new System.EventHandler(this.btn_Browse_Destination_File_Click);
            // 
            // btn_Source_File
            // 
            this.btn_Source_File.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Source_File.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btn_Source_File.Location = new System.Drawing.Point(564, 303);
            this.btn_Source_File.Name = "btn_Source_File";
            this.btn_Source_File.Size = new System.Drawing.Size(28, 23);
            this.btn_Source_File.TabIndex = 7;
            this.btn_Source_File.Text = "...";
            this.btn_Source_File.UseVisualStyleBackColor = false;
            this.btn_Source_File.Click += new System.EventHandler(this.btn_Source_File_Click);
            // 
            // tb_Data_Start_Line
            // 
            this.tb_Data_Start_Line.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.tb_Data_Start_Line.Location = new System.Drawing.Point(161, 94);
            this.tb_Data_Start_Line.Name = "tb_Data_Start_Line";
            this.tb_Data_Start_Line.Size = new System.Drawing.Size(22, 20);
            this.tb_Data_Start_Line.TabIndex = 8;
            this.tb_Data_Start_Line.TextChanged += new System.EventHandler(this.tb_Data_Start_Line_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(56, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Data starts on line #:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Green;
            this.label5.Location = new System.Drawing.Point(42, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(282, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Select the data type for each column of interest:";
            // 
            // lb_Col1
            // 
            this.lb_Col1.FormattingEnabled = true;
            this.lb_Col1.Items.AddRange(new object[] {
            "Bib number",
            "First name",
            "Last name",
            "Full name"});
            this.lb_Col1.Location = new System.Drawing.Point(55, 180);
            this.lb_Col1.Name = "lb_Col1";
            this.lb_Col1.Size = new System.Drawing.Size(65, 56);
            this.lb_Col1.TabIndex = 11;
            this.lb_Col1.SelectedIndexChanged += new System.EventHandler(this.lb_Col1_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(56, 161);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Column 1:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(480, 161);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Column 5:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(374, 161);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Column 4:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(268, 161);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "Column 3:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(162, 161);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Column 2:";
            // 
            // lb_Col3
            // 
            this.lb_Col3.FormattingEnabled = true;
            this.lb_Col3.Items.AddRange(new object[] {
            "Bib number",
            "First name",
            "Last name",
            "Full name",
            "Age",
            "Gender",
            "Address"});
            this.lb_Col3.Location = new System.Drawing.Point(267, 180);
            this.lb_Col3.Name = "lb_Col3";
            this.lb_Col3.Size = new System.Drawing.Size(65, 95);
            this.lb_Col3.TabIndex = 19;
            this.lb_Col3.SelectedIndexChanged += new System.EventHandler(this.lb_Col3_SelectedIndexChanged);
            // 
            // lb_Col2
            // 
            this.lb_Col2.FormattingEnabled = true;
            this.lb_Col2.Items.AddRange(new object[] {
            "Bib number",
            "First name",
            "Last name",
            "Full name"});
            this.lb_Col2.Location = new System.Drawing.Point(161, 180);
            this.lb_Col2.Name = "lb_Col2";
            this.lb_Col2.Size = new System.Drawing.Size(65, 56);
            this.lb_Col2.TabIndex = 20;
            this.lb_Col2.SelectedIndexChanged += new System.EventHandler(this.lb_Col2_SelectedIndexChanged);
            // 
            // lb_Col4
            // 
            this.lb_Col4.FormattingEnabled = true;
            this.lb_Col4.Items.AddRange(new object[] {
            "Bib number",
            "First name",
            "Last name",
            "Full name",
            "Age",
            "Gender",
            "Address"});
            this.lb_Col4.Location = new System.Drawing.Point(373, 180);
            this.lb_Col4.Name = "lb_Col4";
            this.lb_Col4.Size = new System.Drawing.Size(65, 95);
            this.lb_Col4.TabIndex = 21;
            this.lb_Col4.SelectedIndexChanged += new System.EventHandler(this.lb_Col4_SelectedIndexChanged);
            // 
            // lb_Col5
            // 
            this.lb_Col5.FormattingEnabled = true;
            this.lb_Col5.Items.AddRange(new object[] {
            "Bib number",
            "First name",
            "Last name",
            "Full name",
            "Age",
            "Gender",
            "Address"});
            this.lb_Col5.Location = new System.Drawing.Point(479, 180);
            this.lb_Col5.Name = "lb_Col5";
            this.lb_Col5.Size = new System.Drawing.Size(65, 95);
            this.lb_Col5.TabIndex = 22;
            this.lb_Col5.SelectedIndexChanged += new System.EventHandler(this.lb_Col5_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(56, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(169, 13);
            this.label11.TabIndex = 23;
            this.label11.Text = "Number of Columns in Source File:";
            // 
            // tb_Num_Columns
            // 
            this.tb_Num_Columns.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.tb_Num_Columns.Location = new System.Drawing.Point(225, 74);
            this.tb_Num_Columns.Name = "tb_Num_Columns";
            this.tb_Num_Columns.Size = new System.Drawing.Size(22, 20);
            this.tb_Num_Columns.TabIndex = 24;
            this.tb_Num_Columns.TextChanged += new System.EventHandler(this.tb_Num_Columns_TextChanged);
            // 
            // btn_Import
            // 
            this.btn_Import.AutoSize = true;
            this.btn_Import.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btn_Import.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Import.Location = new System.Drawing.Point(393, 89);
            this.btn_Import.Name = "btn_Import";
            this.btn_Import.Size = new System.Drawing.Size(75, 26);
            this.btn_Import.TabIndex = 25;
            this.btn_Import.Text = "Import";
            this.btn_Import.UseVisualStyleBackColor = false;
            this.btn_Import.Click += new System.EventHandler(this.btn_Import_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.AutoSize = true;
            this.btn_Cancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btn_Cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Cancel.Location = new System.Drawing.Point(493, 89);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 26);
            this.btn_Cancel.TabIndex = 26;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = false;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // ImportRunnerData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 384);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Import);
            this.Controls.Add(this.tb_Num_Columns);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lb_Col5);
            this.Controls.Add(this.lb_Col4);
            this.Controls.Add(this.lb_Col2);
            this.Controls.Add(this.lb_Col3);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lb_Col1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_Data_Start_Line);
            this.Controls.Add(this.btn_Source_File);
            this.Controls.Add(this.btn_Browse_Destination_File);
            this.Controls.Add(this.tb_Destination_File_Name);
            this.Controls.Add(this.tb_Source_File_Name);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ImportRunnerData";
            this.Text = "ImportRunnerData";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_Source_File_Name;
        private System.Windows.Forms.TextBox tb_Destination_File_Name;
        private System.Windows.Forms.Button btn_Browse_Destination_File;
        private System.Windows.Forms.Button btn_Source_File;
        private System.Windows.Forms.TextBox tb_Data_Start_Line;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lb_Col1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ListBox lb_Col3;
        private System.Windows.Forms.ListBox lb_Col2;
        private System.Windows.Forms.ListBox lb_Col4;
        private System.Windows.Forms.ListBox lb_Col5;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tb_Num_Columns;
        private System.Windows.Forms.Button btn_Import;
        private System.Windows.Forms.Button btn_Cancel;
    }
}