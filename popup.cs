using System;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SPD_Stage
{
    public partial class popup : Form
    {
        private System.Windows.Forms.Timer autoCloseTimer;
        public bool autoclose;
        public popup(string message, Color backColor, Color textColor, bool autoClose)
        {
            InitializeComponent();

            this.Text = "Test Status";
            this.BackColor = backColor;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Size = new Size(800, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;

            // Message Label
            Label lbl = new Label();
            lbl.Text = message;
            lbl.ForeColor = textColor;
            lbl.BackColor = backColor;
            lbl.Location = new Point(150, 60);
            lbl.Size = new Size(500, 80);
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Stencil", 14, FontStyle.Bold);
            this.Controls.Add(lbl);

            // Exit Button
            Button btn_exit = new Button();
            btn_exit.Location = new Point(320, 200);
            btn_exit.Size = new Size(112, 34);
            btn_exit.Text = "Exit";
            btn_exit.UseVisualStyleBackColor = true;
            btn_exit.Click += btn_exit_Click;
            btn_exit.Hide();
            this.Controls.Add(btn_exit);

            if (!autoClose)
            {
                btn_exit.Show();
            }
            else
            {
                btn_exit.Hide();
            }
            // Initialize and start auto-close timer
            if (autoClose)
            {
                autoCloseTimer = new Timer();
                autoCloseTimer.Interval = 3000; // 5 seconds
                autoCloseTimer.Tick += AutoCloseTimer_Tick;
                autoCloseTimer.Start();
            }
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            autoCloseTimer.Stop(); // stop the timer to prevent multiple triggers
            this.Close();          // close the form
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Static method to show the modal message form
        public static void ShowDialogMessage(string message, Color backColor, Color textColor, bool autoClose)
        {
            using (var form = new popup(message, backColor, textColor, autoClose))
            {
                form.TopMost = true; 
                form.ShowDialog();  
            }
        }

    }
}
