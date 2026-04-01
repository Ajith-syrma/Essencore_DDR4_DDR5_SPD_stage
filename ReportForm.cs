using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace SPD_Stage
{
    public partial class ReportForm : Form
    {
        public string conn_str1 = "Data Source=192.168.1.146;Initial Catalog=Barcode;User ID=sa;Password=syrma@123;MultipleActiveResultSets=true";

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void PanelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        public ReportForm()
        {
            InitializeComponent();
            this.MouseDown += new MouseEventHandler(PanelHeader_MouseDown);
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
          this.Close();
        }
        public void txt_serialnumber_KeyDown(object sender, KeyEventArgs e)
        {
            lbl_result.Visible= false;
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    string serialnumber = txt_serialno.Text;
                    if (serialnumber.Length <= 17 && serialnumber.StartsWith("E"))
                    {
                        using (SqlConnection con = new SqlConnection(conn_str1))
                        {
                            SqlCommand cmd = new SqlCommand("pro_getBordstatus", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@serial_no", serialnumber);
                            con.Open();
                            var result = cmd.ExecuteScalar();
                            con.Close();

                            if (result != null && !string.IsNullOrEmpty(result.ToString()))
                            {
                                lbl_result.Visible = true;
                                lbl_result.Text =$"'{serialnumber}' is "+result.ToString();
                                lbl_result.ForeColor = result.ToString() == "PASS" ? Color.Green : Color.Red;
                                // this.BackColor = result.ToString() == "PASS" ? Color.Green : Color.Red;
                                // txt_serialnumber.BackColor= result.ToString()=="PASS"?Color.Green : Color.Red;
                                //label1.BackColor= result.ToString()=="PASS"?Color.Green : Color.Red;
                                txt_serialno.Clear();                               
                            }
                            else
                            {
                                lbl_result.Visible = true;
                                lbl_result.Text = "Not Found";
                                lbl_result.ForeColor = Color.Red;
                              //  MessageBox.Show("The provided serial number did not pass the K3 test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txt_serialno.Clear();
                                lbl_result.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter the valid serial number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txt_serialno.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Please enter the valid serial number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_serialno.Clear();
                }
            }

        }

    }
}
