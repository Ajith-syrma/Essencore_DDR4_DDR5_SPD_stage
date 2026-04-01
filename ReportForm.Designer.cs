namespace SPD_Stage
{
    partial class ReportForm
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
            label1 = new Label();
            txt_serialno = new TextBox();
            btn_exit = new Button();
            lbl_result = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Poppins", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.MenuHighlight;
            label1.Location = new Point(18, 53);
            label1.Name = "label1";
            label1.Size = new Size(180, 37);
            label1.TabIndex = 0;
            label1.Text = "Serial Number :";
            // 
            // txt_serialno
            // 
            txt_serialno.Font = new Font("Poppins", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txt_serialno.Location = new Point(202, 55);
            txt_serialno.Name = "txt_serialno";
            txt_serialno.Size = new Size(249, 36);
            txt_serialno.TabIndex = 1;
            txt_serialno.KeyDown += txt_serialnumber_KeyDown;
            // 
            // btn_exit
            // 
            btn_exit.BackColor = Color.Red;
            btn_exit.Location = new Point(501, 1);
            btn_exit.Name = "btn_exit";
            btn_exit.Size = new Size(26, 26);
            btn_exit.TabIndex = 2;
            btn_exit.Text = "X";
            btn_exit.UseVisualStyleBackColor = false;
            btn_exit.Click += btn_exit_Click;
            // 
            // lbl_result
            // 
            lbl_result.AutoSize = true;
            lbl_result.Font = new Font("Poppins", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_result.Location = new Point(182, 103);
            lbl_result.Name = "lbl_result";
            lbl_result.Size = new Size(27, 34);
            lbl_result.TabIndex = 3;
            lbl_result.Text = "...";
            lbl_result.Visible = false;
            // 
            // ReportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(527, 146);
            ControlBox = false;
            Controls.Add(lbl_result);
            Controls.Add(btn_exit);
            Controls.Add(txt_serialno);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Location = new Point(1316, 8);
            Name = "ReportForm";
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txt_serialno;
        private Button btn_exit;
        private Label lbl_result;
    }
}