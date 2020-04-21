namespace RunnerTracker
{
    partial class NewNumber
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
            this.lbl_RFID_number = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_Bib_number = new System.Windows.Forms.TextBox();
            this.btn_Create = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_Save_Newnumber = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(28, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "This RFID number:";
            // 
            // lbl_RFID_number
            // 
            this.lbl_RFID_number.AutoSize = true;
            this.lbl_RFID_number.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_RFID_number.ForeColor = System.Drawing.Color.Red;
            this.lbl_RFID_number.Location = new System.Drawing.Point(160, 25);
            this.lbl_RFID_number.Name = "lbl_RFID_number";
            this.lbl_RFID_number.Size = new System.Drawing.Size(104, 16);
            this.lbl_RFID_number.TabIndex = 1;
            this.lbl_RFID_number.Text = "223456789222";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label3.Location = new System.Drawing.Point(63, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "is not found in the local database!";
            // 
            // tb_Bib_number
            // 
            this.tb_Bib_number.Location = new System.Drawing.Point(188, 131);
            this.tb_Bib_number.Name = "tb_Bib_number";
            this.tb_Bib_number.Size = new System.Drawing.Size(45, 20);
            this.tb_Bib_number.TabIndex = 3;
            this.tb_Bib_number.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btn_Create
            // 
            this.btn_Create.Location = new System.Drawing.Point(53, 197);
            this.btn_Create.Name = "btn_Create";
            this.btn_Create.Size = new System.Drawing.Size(75, 23);
            this.btn_Create.TabIndex = 4;
            this.btn_Create.Text = "Create entry";
            this.btn_Create.UseVisualStyleBackColor = true;
            this.btn_Create.Click += new System.EventHandler(this.btn_Create_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(164, 197);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 5;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(244, 39);
            this.label4.TabIndex = 6;
            this.label4.Text = "A new entry in the local database can be created\r\nby entering the Runner\'s Bib nu" +
                "mber in the textbox\r\n      below and clicking the \'Create entry\' button.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(60, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(126, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "New Runner Bib number:";
            // 
            // cb_Save_Newnumber
            // 
            this.cb_Save_Newnumber.AutoSize = true;
            this.cb_Save_Newnumber.Location = new System.Drawing.Point(76, 163);
            this.cb_Save_Newnumber.Name = "cb_Save_Newnumber";
            this.cb_Save_Newnumber.Size = new System.Drawing.Size(141, 17);
            this.cb_Save_Newnumber.TabIndex = 8;
            this.cb_Save_Newnumber.Text = "Save to Assignments file";
            this.cb_Save_Newnumber.UseVisualStyleBackColor = true;
            // 
            // NewNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 244);
            this.Controls.Add(this.cb_Save_Newnumber);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Create);
            this.Controls.Add(this.tb_Bib_number);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbl_RFID_number);
            this.Controls.Add(this.label1);
            this.Name = "NewNumber";
            this.Text = "No such RFID number";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_RFID_number;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_Bib_number;
        private System.Windows.Forms.Button btn_Create;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cb_Save_Newnumber;
    }
}