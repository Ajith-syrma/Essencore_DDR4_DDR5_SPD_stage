using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SPD_Stage
{
    public partial class spd_load : Form
    {

        DbConnection dbConnection = new DbConnection();
        //private readonly string Logsbkppath = ConfigurationManager.AppSettings["BackupfilePath"];
        private readonly string CONFIG_MASTERFILEPATH = ConfigurationManager.AppSettings["MASTERFILEPATH"];
        private readonly string DDR4_App_Path = ConfigurationManager.AppSettings["DDR4_APP_Path"];
        private readonly string DDR5_App_Path = ConfigurationManager.AppSettings["DDR5_APP_Path"];
        private readonly string DDR4_App_LogPath = ConfigurationManager.AppSettings["DDR4_APP_LogPath"];
        private readonly string DDR5_App_LogPath = ConfigurationManager.AppSettings["DDR5_APP_LogPath"];
        private string App_Name = "Ezspdset.exe";    
        private string App_Path = "";
        private string Master_FilePath = "";    

        private string Logsbkppath = "";
        private string pass_filename;
        private string fail_filename;
        private string Pass_LogFilePath = "";
        private string Fail_LogFilePath = "";




        private string ssdmpFilePathG4;
        public string filedtfmt = DateTime.Now.ToString("MM-dd-yyyy");
        public string ssdmpFilePath = string.Empty;
        public string ssdmpG3 = string.Empty;
        public string ssdmpG4 = string.Empty;
        public string emp_id = "";
        public string emp_name = "";
        public string Gentype = "";

        
        private string App_LogPath;
        public string filepath = string.Empty;
        bool suppressCapacityEvent = false;



        public spd_load()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            //lbl_filepathvalue.Enabled = false;
            //lbl_startinfo.Enabled = false;
            //cmb_stage.Text = "K3";
            cmb_stage.Visible = false;

            //this.Shown += K1_load_Shown; // Attach Shown event

            //Filevalidation();
            //fg_load();

        }


        private bool Filevalidation()
        {
         

            try
            {

                pass_filename = $"PASS_SN_log_{filedtfmt}.txt";
                fail_filename = $"FAIL_SN_log_{filedtfmt}.txt";
                

                if (cmb_prdctModel.Text == "DDR4")
                {

                    Pass_LogFilePath = Path.Combine($"{DDR4_App_LogPath}\\PASS", pass_filename);
                    Fail_LogFilePath = Path.Combine($"{DDR4_App_LogPath}\\FAIL", fail_filename);
                    App_Path = Path.Combine(DDR4_App_Path,App_Name);
                    Logsbkppath=Path.Combine($"{DDR4_App_Path}\\LogBackup");
                    Master_FilePath= Path.Combine($"{CONFIG_MASTERFILEPATH}", $"{cmb_capacity.Text}.spp4");

                    if (!Directory.Exists(Logsbkppath))
                    {
                        System.IO.Directory.CreateDirectory(Logsbkppath);
                    }
                    if (!File.Exists(Master_FilePath))
                    {
                        MessageBox.Show($"{Master_FilePath}--Not Found\n  Please Check the network or File Path", "File Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (File.Exists(Pass_LogFilePath))
                    {
                        File.Copy(Pass_LogFilePath, Logsbkppath + "\\" + $"LogFile_{System.DateTime.Now.ToString("HH_mm_ss")}_{pass_filename}");
                        Thread.Sleep(1000);
                        File.WriteAllText(Pass_LogFilePath, string.Empty);
                    }

                    if (File.Exists(Fail_LogFilePath))
                    {
                        File.Copy(Fail_LogFilePath, Logsbkppath + "\\" + $"LogFile_{System.DateTime.Now.ToString("HH_mm_ss")}_{fail_filename}");
                        Thread.Sleep(1000);
                        File.WriteAllText(Fail_LogFilePath, string.Empty);
                    }

                    return true; // validation passed
                }
                else if (cmb_prdctModel.Text == "DDR5")
                {

                    Pass_LogFilePath = Path.Combine($"{DDR5_App_LogPath}\\PASS", pass_filename);
                    Fail_LogFilePath = Path.Combine($"{DDR5_App_LogPath}\\FAIL", fail_filename);
                    App_Path = Path.Combine(DDR5_App_Path, App_Name);
                    Logsbkppath = Path.Combine($"{DDR5_App_Path}\\LogBackup");
                    Master_FilePath = Path.Combine($"{CONFIG_MASTERFILEPATH}", $"{cmb_capacity.Text}.spp5");

                    if (!Directory.Exists(Logsbkppath))
                    {
                        System.IO.Directory.CreateDirectory(Logsbkppath);
                    }
                    if (!File.Exists(Master_FilePath))
                    {
                        MessageBox.Show($"{Master_FilePath}--Not Found\n  Please Check the network or File Path", "File Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (File.Exists(Pass_LogFilePath))
                    {
                        File.Copy(Pass_LogFilePath, Logsbkppath + "\\" + $"LogFile_{System.DateTime.Now.ToString("HH_mm_ss")}_{pass_filename}");
                        Thread.Sleep(1000);
                        File.WriteAllText(Pass_LogFilePath, string.Empty);
                    }

                    if (File.Exists(Fail_LogFilePath))
                    {
                        File.Copy(Fail_LogFilePath, Logsbkppath + "\\" + $"LogFile_{System.DateTime.Now.ToString("HH_mm_ss")}_{fail_filename}");
                        Thread.Sleep(1000);
                        File.WriteAllText(Fail_LogFilePath, string.Empty);
                    }

                    return true; // validation passed
                }
                else
                {
                    MessageBox.Show("Please check the Gen Type for the Selected FG", "Data Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; // signal failure
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // validation passed
            }

        }

        private void cmb_prdctModel_SelectedValueChanged(object sender, EventArgs e)
        {

            cmb_prdctModel.Enabled = false;
            try
            {

                string stageval = lbl_stage_load.Text;
                var listNos = dbConnection.getcapacity(stageval, cmb_prdctModel.Text);
                suppressCapacityEvent = true;
                if (listNos != null && listNos.Count > 0)
                {

                    cmb_capacity.DataSource = listNos;

                }
                else
                {
                    cmb_capacity.DataSource = null;
                }
                suppressCapacityEvent = false;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }



        private void btn_start_Click(object sender, EventArgs e)
        {

            string stage = lbl_stage_load.Text;
            string Prduct_model = cmb_prdctModel.Text;
            string fg = cmb_capacity.Text;
            string emp_id = txt_empid.Text;
            string emp_name = txt_emp_name.Text;
            

            
            if (stage != "select stage" && Prduct_model != "Select model" && !string.IsNullOrEmpty(fg) && (!string.IsNullOrEmpty(emp_id)) && !string.IsNullOrEmpty(emp_name))
            {
                bool valid = Filevalidation();
                if (valid)
                {

                    spd_stage k1form = new spd_stage(stage, Prduct_model, App_Name, App_Path, fg, emp_id, emp_name, Master_FilePath, Pass_LogFilePath,Fail_LogFilePath);
                    k1form.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("File Validation Failed. Please check the file paths and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please provide all the inputs");
                cmb_capacity.Enabled = true;
                cmb_stage.Enabled = true;
            }

        }


        private void btn_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void K1_load_Load(object sender, EventArgs e)
        {

        }

        private void lbl_app_id_Click(object sender, EventArgs e)
        {

        }
    }
}
