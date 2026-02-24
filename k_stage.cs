using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic.Logging;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Application = FlaUI.Core.Application;
using Font = System.Drawing.Font;
using Point = System.Drawing.Point;
namespace K1_Stages
{
    public partial class k_stage : Form
    {
        #region variable decl,dll,appconf
        //Declaration from  App config 
        DbConnection dbConnection = new DbConnection();
        SqlConnection SFCS_db = new SqlConnection(ConfigurationManager.AppSettings["SFCS"].ToString());



        //public string CONFIG_NETWORKPATH = ConfigurationManager.AppSettings["NETWORKPATH"];
        private string CONFIG_ISONLINE = ConfigurationManager.AppSettings["ISONLINE"];
        //DLL imports for window manipulation and DPI awareness
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        //[DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        // Variables for barcode scanning and form positioning
        private StringBuilder barcodeData = new StringBuilder();
        // Constants for ShowWindow function
        private const int SW_MINIMIZE = 6;
        private const int SW_MAXIMIZE = 3;
        // Variables for application state and data handling
        public string board_status = "";
        private string currentText = null;
        private string Model_name = null;
        private string Disk_size = null;
        private string Health_status = null;
        private string Temp_status = null;
        private string Serial_no = null;
        private string Firmware_val = null;
        private string diskletter = null;
        private int Temp_val = 0;
        private string Health_stat = null;
        private int Health_value = 0;
        // Variables for SFCS 
        string errordesc = "";
        public string[] nextidinfo = { "", "" };
        public string[] reworkidinfo = { "24", "Rework" };
        public string[] infosfromprint = { "", "", "" }; // FG, Sno, WO
        public string[] infosfromboard = { "", "" };// WO,RW
        public string yearinfofromprint = "";
        public bool boardonline = true;
        public bool boardfail = true;
        public int boardfailcount = 0;
        public string[] infologindetails = { "", "" };
        // Timers for monitoring application state and file changes
        private System.Windows.Forms.Timer monitorTimer;
        private System.Windows.Forms.Timer fileMonitorTimer;
        public string fileNameg4 = DateTime.Now.ToString("yyyyMMdd");
        public string ssdmpFilePath = string.Empty;
        public string ssdmpG3 = string.Empty;
        public string ssdmpG4 = string.Empty;
        private DateTime lastSsdmpReadTime = DateTime.MinValue;
        private DateTime _lastFileWriteTime = DateTime.MinValue;
        private bool fileMonitorStarted = false;
        private string lastHash;
        // FlaUI Window reference
        private FlaUI.Core.AutomationElements.Window mainWindow;
        // Variables for K3 Stage and CDI
        public string emp_id = "";
        public string emp_name = "";
        public string capacity = "";
        public string stage = "";
        public string Gentype = "";
        private string Filename;
        public string Log_filePath;
        public string product_Model;
        private string App_Name;
        private string App_Path;
        private string Fg;
        public string Pcb_Serialno = "";
        public string customer_Fg = "";
        private string Firmware_Name;
        private bool isHandlingChange = false;
        bool suppressCapacityEvent = false;
        #endregion

        #region UI_components
        public k_stage(string stage_name, string Prduct_model, string App_N, string App_Pth, string fg, string emp_id, string emp_name, string f_name, string App_logpath)
        {
            InitializeComponent();
            //variable initialization from parameters and configuration
            stage = stage_name;
            product_Model = Prduct_model;
            App_Name = App_N;
            App_Path = App_Pth;
            Fg = fg;
            Firmware_Name = f_name;
            Log_filePath = App_logpath;
            //set form properties and event handlers
            this.MaximizeBox = false;
            lbl_filepathvalue.Enabled = false;
            lbl_startinfo.Enabled = false;
            this.MinimizeBox = false;
            lblemp_id.Text = emp_id;
            lbl_date.Text = DateOnly.FromDateTime(DateTime.Now).ToString();
            this.FormClosing += Form1_close;
            this.Shown += Form1_Shown;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;


            //Start check sfcs 
            startchecksfcs();

            k1_stage_test(stage_name, Prduct_model, App_N, fg, emp_id, emp_name, f_name);
            this.Show();
            this.TopMost = true;
            this.Activate();
            this.BringToFront();
            this.TopMost = true;


        }
        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int HTCAPTION = 0x2;

            if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
                return; // block dragging

            base.WndProc(ref m);
        }


        private void Txt_SN_KeyDown(object sender, KeyEventArgs e)       
        {
            if (e.KeyCode == Keys.Enter)
            {
                lbl_result.BackColor = SystemColors.ActiveCaption;
                lbl_result.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
                lbl_result.Location = new Point(493, 179);
                lbl_result.Name = "lbl_result";
                lbl_result.Size = new Size(456, 274);
                lbl_result.TabIndex = 23;
                lbl_result.Text = "TEST STATUS";
                

                var scannedData = "";

                if (product_Model == "M.2")
                {
                    scannedData = txt_SN.Text.Trim().ToUpper();
                }
                else if (product_Model == "SSD_SATA")
                {
                    //scannedData = dbConnection.getpcbserialno(txt_SN.Text.Trim().ToUpper());
                    scannedData = txt_SN.Text.Trim().ToUpper();
                }
                else
                {
                    scannedData = "1";
                }

                if (!scannedData.StartsWith("ECH"))
                {
                    Pcb_Serialno = string.Empty;
                    customer_Fg = string.Empty;
                    var scanned_dtval = dbConnection.scanned_dtval(scannedData, product_Model);
                    foreach (var fg_detail in scanned_dtval)
                    {
                        Pcb_Serialno = fg_detail.PCB_SNO;
                        customer_Fg = fg_detail.Fg_no;
                    }

                    
                    if (!string.IsNullOrEmpty(customer_Fg) && customer_Fg  == Fg)
                    {
                        if (!string.IsNullOrEmpty(Pcb_Serialno) || Pcb_Serialno! == "0")
                        {
                           
                            if (Check_Curr_Stage(Pcb_Serialno, lbl_app_id.Text, lblstagename.Text, boardonline))
                            {
                                move_Formto_right();
                                MessageBox.Show("Please scan the serial Number on the OUT0 of the MP Tool");
                            }
                            else
                            {
                                move_Formto_left();
                                txt_SN.Clear();
                                MessageBox.Show("Please scan the Next serial Number on the SFCS Application");
                            }

                        }
                        else
                        {
                            lbl_result.BackColor = Color.Yellow;
                            lbl_result.ForeColor = Color.Black;
                            lbl_result.Text = "No data found for the provided customer Serial Number";
                            txt_SN.Clear();
                        }

                    }

                    else if (customer_Fg == "0")
                    {
                        lbl_result.BackColor = Color.Yellow;
                        lbl_result.ForeColor = Color.Black;
                        lbl_result.Text = "No data found for the provided customer Serial Number";
                        txt_SN.Clear();
                    }
                    else if (string.IsNullOrEmpty(customer_Fg))
                    {
                        lbl_result.BackColor = Color.Yellow;
                        lbl_result.ForeColor = Color.Black;
                        lbl_result.Text = "Network error could not access server Please contact IT Administrator";
                        txt_SN.Clear();
                    }
                    else
                    {
                        lbl_result.BackColor = Color.Yellow;
                        lbl_result.ForeColor = Color.Black;
                        lbl_result.Text = "Scanned FG-Serial and Selected Fg different";
                        txt_SN.Clear();

                    }


                }
                else
                {
                    lbl_result.BackColor = Color.Yellow;
                    lbl_result.ForeColor = Color.Black;
                    lbl_result.Text = "Please scan Customer Serial Number";
                    txt_SN.Clear();

                }

            }
            else
            {
                // Append the keystroke to the barcode data
                barcodeData.Append((char)e.KeyValue);
            }
        }

        private void move_Formto_center()
        {
            StartPosition = FormStartPosition.Manual;

            Screen screen = Screen.FromControl(this);
            Location = new Point(
                screen.WorkingArea.Left + (screen.WorkingArea.Width - Width) / 2,
                screen.WorkingArea.Top + (screen.WorkingArea.Height - Height) / 2
            );
        }



        private void move_Formto_left()
        {
            StartPosition = FormStartPosition.Manual;

            Screen screen = Screen.FromControl(this);
            Location = new Point(
                screen.WorkingArea.Left,
                screen.WorkingArea.Top + (screen.WorkingArea.Height - Height) / 3
            );


        }
        private void move_Formto_right()
        {

            StartPosition = FormStartPosition.Manual;

            Screen screen = Screen.FromControl(this);
            Location = new Point(
                screen.WorkingArea.Right - Width,
                screen.WorkingArea.Top
            );

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (App_Name == "SM2268XT2_MPTool.exe")
            {
                move_Formto_center();
            }

            else if (App_Name == "SSDMP.exe")
            {
                move_Formto_left();
            }

            else
            {
                move_Formto_center();
            }

        }


        private void Form1_close(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void k1_stage_test(string stage_val, string Prduct_model,
                                        string Appl_Name, string fg_no, string employe_id, string employee_name, string f)
        {

            stage = stage_val;
            capacity = fg_no;
            emp_id = employe_id;
            emp_name = employee_name;
            Filename = f;
            Fill_Response_Data("K1 Stage Test");

            string modelname = dbConnection.getmodel(capacity);
            Gentype = dbConnection.getgentype(capacity);
            //ssdmpFilePath = Gentype == "Gen4x4" ? ssdmpG4 : ssdmpFilePathG3;
            ssdmpFilePath = Log_filePath;
            writestatusMessage("Start button clicked", "Application_start");

            Thread.Sleep(1000);
            Spd_Automation(stage, capacity, Filename, emp_id, emp_name, modelname, Gentype, Appl_Name);

            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
            //this.TopMost = false;


        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region modeltypehandlerK3
        public string Spd_Automation(string stageName, string capacity, string Filepath, string emp_id, string emp_name, string model, string Gentype, string Applic_Name)
        {
            try
            {

                if (Applic_Name == "SSDMP.exe")
                {
                    // Initialize and start the SSDMP file monitor timer
                    fileMonitorTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 5000 // Check every 5 seconds
                    };
                    fileMonitorTimer.Tick += FileMonitorTimer_Tick;
                    fileMonitorTimer.Start();
                    StartAppWithCleanup(App_Path);
                    var app = Application.Launch(App_Path);
                    app.WaitWhileMainHandleIsMissing();

                    //UI Automation SSDMP Tool 
                    using var automation = new UIA3Automation();
                    mainWindow = app.GetMainWindow(automation);
                    if (mainWindow == null || !mainWindow.Name.StartsWith("SSDMP"))
                    {
                        MessageBox.Show("Main window not found or name mismatch for Gen3.");
                        writestatusMessage("SSDMP Window not found", "Window not found");
                        return null;
                    }
                    Fill_Response_Data("SSDMP MP Tool Started");
                    writestatusMessage("SSDMP Window found", "Window found");
                    var allButtons = mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Button)).ToList();
                    var allcombo = mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox)).ToList();

                    // Click the "Load" button (AutomationId: "1168")
                    var loadButton = allButtons.FirstOrDefault(b => b?.AutomationId == "1168");
                    loadButton?.WaitUntilClickable(TimeSpan.FromSeconds(2));
                    loadButton?.AsButton().Invoke();

                    // Wait and find "Open" dialog window under main window
                    var loadK1Window = Retry.WhileNull(() =>
                        mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Window)
                            .And(cf.ByName("Open"))),
                        TimeSpan.FromSeconds(5)).Result;

                    if (loadK1Window != null)
                    {
                        // Find "File name:" edit box in Open dialog
                        var filePathEdit = loadK1Window.FindFirstDescendant(cf =>
                            cf.ByControlType(ControlType.Edit)
                            .And(cf.ByName("File name:")));

                        string filepathGen3 = Filepath;

                        if (filePathEdit != null)
                        {
                            var fpTextBox = filePathEdit.AsTextBox();
                            fpTextBox.Text = "";
                            System.Threading.Thread.Sleep(100);
                            fpTextBox.Text = filepathGen3;
                        }
                        Fill_Response_Data($"Firmware :{filepathGen3}");

                        // Click "Open" button (AutomationId: "1")
                        var openButton = loadK1Window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                            .FirstOrDefault(e => e.AutomationId == "1");

                        openButton?.WaitUntilClickable(TimeSpan.FromSeconds(2));
                        openButton?.AsButton().Invoke();
                        System.Threading.Thread.Sleep(500);

                        // Select "Device" in ComboBox (AutomationId: "2112")
                        var devicestationid = mainWindow.Name == "SSDMP 2.82.0.0_20240318_R_X64" ? "1040" : "2108";
                        var MP_stationid = mainWindow.Name == "SSDMP 2.82.0.0_20240318_R_X64" ? "1043" : "2112";

                        // Select "Device" in ComboBox (AutomationId: "2112")
                        var comboBoxdevice = mainWindow.FindFirstDescendant(cf =>
                            cf.ByControlType(ControlType.ComboBox).And(cf.ByAutomationId(devicestationid)));
                        var cmbdevicecurval = comboBoxdevice.AsComboBox();

                        var cmbdeviceval = cmbdevicecurval.AsTextBox().Text;

                        if (cmbdeviceval != "RTS5766DL")
                        {
                            var combodevice = comboBoxdevice?.AsComboBox();
                            combodevice?.Expand();
                            System.Threading.Thread.Sleep(500);

                            var itemToSelectdevice = combodevice?.Items?.FirstOrDefault(i => i.Text == "RTS5766DL");
                            itemToSelectdevice?.Select();
                            writestatusMessage("RTS5766DL option selected", "Device selected");
                            System.Threading.Thread.Sleep(50);
                        }



                        // Select "RDT" in ComboBox (AutomationId: "2112")
                        var comboBoxElement = mainWindow.FindFirstDescendant(cf =>
                            cf.ByControlType(ControlType.ComboBox).And(cf.ByAutomationId(MP_stationid)));

                        var cmbk1curval = comboBoxElement.AsComboBox();

                        var cmbk1val = cmbk1curval.AsTextBox().Text;

                        var k1val = stageName == "K1" ? "RDT" :
                                    stageName == "K2" ? "MP" :
                                    stageName == "K3" ? "K3" : "0";

                        //if (cmbk1val != k1val)
                        //{
                        //    var comboBox = comboBoxElement?.AsComboBox();
                        //    comboBox?.Expand();
                        //    System.Threading.Thread.Sleep(500);


                        //    var itemToSelect = comboBox?.Items?.FirstOrDefault(i => i.Text == k1val);
                        //    itemToSelect?.Select();
                        //    System.Threading.Thread.Sleep(50);
                        //    Mouse.Click();
                        //    writestatusMessage($"stage name is {stageName} so {k1val} is selected", "Stage selected");
                        //    System.Threading.Thread.Sleep(50);
                        //}
                        var comboBox = comboBoxElement?.AsComboBox();

                        if (comboBox == null)
                        {
                            writestatusMessage("ComboBox not found or is null.", "Error");
                            return string.Empty;
                        }

                        comboBox.Expand();
                        System.Threading.Thread.Sleep(300); // Slight delay to allow items to appear

                        var itemToSelect = comboBox.Items?.FirstOrDefault(i => i.Text.Equals(k1val, StringComparison.OrdinalIgnoreCase));

                        if (itemToSelect != null)
                        {
                            itemToSelect.Select();
                            System.Threading.Thread.Sleep(100);
                            itemToSelect.Click();
                            writestatusMessage($"Stage name is {stageName}, so '{k1val}' is selected.", "Stage selected");
                            Fill_Response_Data($"Stage name is {stageName}, so '{k1val}' is selected.");
                        }
                        else
                        {
                            writestatusMessage($"Item '{k1val}' not found in ComboBox for stage '{stageName}'.", "Selection Failed");
                        }

                        // Edit textboxes for capacity and model (AutomationIds "1021" and "1025")
                        var textboxMem = mainWindow.FindFirstDescendant(cf =>
                            cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("1021")));
                        var textboxMod = mainWindow.FindFirstDescendant(cf =>
                            cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("1025")));

                        if (textboxMem != null && textboxMod != null)
                        {
                            var memTextBox = textboxMem.AsTextBox();
                            var modTextBox = textboxMod.AsTextBox();

                            string memCurrentVal = memTextBox.Text;
                            string modCurrentVal = modTextBox.Text;

                            string expectedValue = filepathGen3.Contains("256GB") ? "256" :
                                                   filepathGen3.Contains("512GB") ? "512" :
                                                   filepathGen3.Contains("1TB") ? "1024" : "0";

                            if (memCurrentVal != expectedValue)
                            {
                                memTextBox.Text = "";
                                System.Threading.Thread.Sleep(100);
                                memTextBox.Enter(expectedValue);
                                writestatusMessage($"Capacity {expectedValue} is selected", "capacity selected");
                                Fill_Response_Data($"Capacity {expectedValue} is selected");
                            }

                            modTextBox.Text = model;
                            writestatusMessage($"The model value is{model}", "Model selected");
                            Fill_Response_Data($"The model value is{model}");

                        }
                    }

                    return "Completed";
                }
                else if (Applic_Name == "SM2268XT2_MPTool.exe")
                {
                    // Initialize and start the SM2268XT2_MPTool file monitor timer
                    fileMonitorTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 5000 // Check every 5 seconds
                    };
                    fileMonitorTimer.Tick += FileMonitorTimer_Tick;
                    fileMonitorTimer.Start();
                    StartAppWithCleanup(App_Path);
                    var app = Application.Launch(App_Path);
                    app.WaitWhileMainHandleIsMissing();

                    //UI Automation for SM2268XT2_MPTool 

                    using var automation = new UIA3Automation();
                    mainWindow = app.GetMainWindow(automation);
                    if (mainWindow == null || !mainWindow.Name.StartsWith("SM2268XT2 MPTool"))
                    {
                        MessageBox.Show("Main window not found or name mismatch for Gen4.");
                        writestatusMessage("SM2268XT2 MPTool Window not found", "Window not found");
                        return null;
                    }
                    Fill_Response_Data("SM2268XT2 MPTool Started");
                    Thread.Sleep(1000);
                    var parameterTab = mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.TabItem).And(cf.ByName("Parameter")));
                    if (parameterTab != null)
                    {
                        parameterTab.Click();  // Works for Win32 tabs

                    }
                    else
                    {
                        MessageBox.Show("Parameter tab not found");
                        writestatusMessage("Parameter tab not found", "Tab not found");
                    }
                    Thread.Sleep(500);

                    var fileloadbtn = mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("657")));

                    if (fileloadbtn != null)
                    {
                        fileloadbtn.WaitUntilClickable(TimeSpan.FromSeconds(2));
                        fileloadbtn.Click();
                    }
                    else
                    {
                        MessageBox.Show("File open button not found");
                        writestatusMessage("File open button not found", "Button not found");
                    }

                    // Wait and find "Open" dialog window under main window
                    var loadK1Window = Retry.WhileNull(() => mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Window).And(cf.ByName("Open"))), TimeSpan.FromSeconds(5)).Result;

                    if (loadK1Window != null)
                    {
                        // Find "File name:" edit box in Open dialog
                        var filePathEdit = loadK1Window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit).And(cf.ByName("File name:")));

                        string filepathGen4 = Filepath;

                        if (filePathEdit != null)
                        {
                            var fpTextBox = filePathEdit.AsTextBox();
                            fpTextBox.Text = "";
                            System.Threading.Thread.Sleep(100);
                            fpTextBox.Text = filepathGen4;
                        }


                        else
                        {
                            MessageBox.Show("File path in File load dialog box not found");
                            writestatusMessage("File path in File load dialog box not found", "Text box Not found");

                        }
                        Fill_Response_Data($"Firmware :{filepathGen4}");


                        var openButton = loadK1Window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("1")));

                        openButton?.WaitUntilClickable(TimeSpan.FromSeconds(2));
                        openButton?.AsButton().Invoke();
                        System.Threading.Thread.Sleep(500); ;

                        var TestTab = mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.TabItem).And(cf.ByName("Test"))
                                                );
                        if (TestTab != null)
                        {
                            TestTab.Click();  // Works for Win32 tabs
                        }

                        else
                        {
                            MessageBox.Show("File open btn in File load dialog box not found");
                            writestatusMessage("File open btn in File load dialog box not found", "Button Not found");
                        }
                        Thread.Sleep(2000);


                    }
                    return "Completed";
                }
                else
                {
                    //// This is for SSD_SATA Mp_Tools Need to check UI Automation but still can lauch the App
                    //// Initialize and start the SSDMP file monitor timer
                    //fileMonitorTimer = new System.Windows.Forms.Timer
                    //{
                    //    Interval = 5000 // Check every 5 seconds
                    //};
                    //fileMonitorTimer.Tick += FileMonitorTimer_Tick;
                    //fileMonitorTimer.Start();
                    //StartAppWithCleanup(App_Path);
                    //var app = Application.Launch(App_Path);
                    //app.WaitWhileMainHandleIsMissing();
                    //return null;
                    MessageBox.Show("No Application for the selected MP Tool");
                    return "Completed";


                }

            }



            catch (Exception ex)
            {
                if (ex.Message.Contains("An error occurred trying to start process"))
                {
                    //MessageBox.Show("Open the Application in Administartor");
                    lbl_result.Text = "Open the Application in Administartor";
                    lbl_result.ForeColor = Color.Black;
                    lbl_result.BackColor = Color.Yellow;
                    txt_SN.Enabled = false;

                    return null;
                }
                else
                {
                    lbl_result.Text = "Error in Automation \n" +
                                      "Restart the Application";
                    lbl_result.ForeColor = Color.Black;
                    lbl_result.BackColor = Color.Yellow;
                    txt_SN.Enabled = false;
                    return null;
                }

            }
        }
        #endregion

        #region Filehandling
        public static void StartAppWithCleanup(string exePath)
        {
            string processName = Path.GetFileNameWithoutExtension(exePath);
            var runningProcesses = Process.GetProcessesByName(processName);
            foreach (var process in runningProcesses)
            {
                try
                {
                    // Step 2: Kill it
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Closed existing instance of {processName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not close {processName}: {ex.Message}");
                }
            }
        }


        private void FileMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {

                if (!File.Exists(ssdmpFilePath))
                    return;

                var currentWriteTime = File.GetLastWriteTime(ssdmpFilePath);



                // Only process the file if it's been modified since last time
                if (currentWriteTime > _lastFileWriteTime)
                {

                    try
                    {
                        string Currenthash = GetFileHash(ssdmpFilePath);
                        if (Currenthash != lastHash)
                        {
                            lastHash = Currenthash;

                            EvaluateLastValidDUT(ssdmpFilePath, emp_id, emp_name, Gentype, capacity, stage, App_Name, App_Path, Firmware_Name, this);

                        }
                        else
                        {
                            Console.WriteLine("File timestamp changed, but content is the same.");
                            writestatusMessage($"File timestamp changed, but content is the same", "File Timestamp");
                            return;
                        }

                        _lastFileWriteTime = currentWriteTime;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("File in use — will retry...");

                    }


                    string content = File.ReadAllText(ssdmpFilePath);
                    Console.WriteLine("File changed at " + currentWriteTime);
                    Console.WriteLine("Content:\n" + content);

                    // Add your file processing logic here
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading file: " + ex.Message);
            }
        }
        static string GetFileHash(string path)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

        }


        public static void EvaluateLastValidDUT(string filePath, string emp_id, string emp_name,
                                              string Gtype, string fgno, string stage_N, string app_name, string app_path,
                                              string firmware_name, k_stage currentStage)
        {


            bool boardfail = true;
            string status = string.Empty;
            string testTime = string.Empty;
            string model = string.Empty;
            string serial = string.Empty;
            string capacity = string.Empty;
            string station = string.Empty;
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Log file not found.");
                return;
            }
            if (app_name == "SM2268XT2_MPTool.exe")
            {
                string lastLine = File.ReadLines(filePath).LastOrDefault();
                string pattern = @"^(\S+)\s+(\S+)\s+(\S+\s+\S+)\s+(\S+\s+\S+)\s+(\S+)\s+(\S+)\s+(.+)$";
                var m = Regex.Match(lastLine, pattern);

                if (!m.Success)
                    return;
                capacity = fgno; station = "K1";
                string port = m.Groups[2].Value;
                string start = m.Groups[3].Value;
                string end = m.Groups[4].Value;
                serial = m.Groups[5].Value.Trim();
                model = m.Groups[6].Value.Trim();
                status = m.Groups[7].Value.Trim();

                DateTime startTime = DateTime.Parse(start);
                DateTime endTime = DateTime.Parse(end);

                double durationSeconds = (endTime - startTime).TotalSeconds;
                testTime = durationSeconds.ToString();

                Console.WriteLine($"Serial: {serial}, Status: {status}, Duration: {durationSeconds}s");
            }
            else
            {
                string content = File.ReadAllText(filePath);
                var matches = Regex.Matches(content, @"\*{8}\[DUT \d\].+?\*{8}([\s\S]*?)(?=\*{8}\[DUT|\Z)");

                if (matches.Count == 0)
                {
                    Console.WriteLine("No DUT blocks found.");
                    return;
                }

                string lastValidBlock = null;
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    var block = matches[i].Groups[1].Value;

                    if (block.Contains("Test time"))
                    {
                        lastValidBlock = block;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(lastValidBlock))
                {
                    Console.WriteLine("No valid DUT blocks with result data found.");
                    return;
                }
                string pattern = @"\b(Pass|Fail)\b(?:\s+\([^\)]+\))?\s+Test time:";
                status = Regex.Match(lastValidBlock, pattern).Groups[1].Value.Trim();
                testTime = Regex.Match(lastValidBlock, @"Test time:\s*(.+)").Groups[1].Value.Trim();
                model = Regex.Match(lastValidBlock, @"Model name:\s*(.+)").Groups[1].Value.Trim();
                serial = Regex.Match(lastValidBlock, @"Serial number:\s*(.+)").Groups[1].Value.Trim();
                capacity = Regex.Match(lastValidBlock, @"Capacity:\s*(.+)").Groups[1].Value.Trim();
                station = Regex.Match(lastValidBlock, @"Station:\s*(.+)").Groups[1].Value.Trim();
            }

            DbConnection dbconnect = new DbConnection();
            var ser_len = serial.Length;
            if ((ser_len == 14 || ser_len == 17) && (serial.StartsWith("ES") || (serial.StartsWith("EN"))))
            {
                //var stage_count = dbconnect.get_serial_dup(serial, model);
                //bool checkstage = currentStage.Check_Curr_Stage(serial, currentStage.lbl_app_id.Text, currentStage.lblstagename.Text, currentStage.boardonline);

                if (serial.Trim()== currentStage.txt_SN.Text.Trim())
                {
                    //    if (stage_count == "0" || stage_count == "Status not found")
                    //{
                    //dbconnect.DbConnect1(serial, model, capacity, station, testTime, status);
                    dbconnect.DbConnect2(serial, model, capacity, station, testTime, status,currentStage.Pcb_Serialno);

                    var displayColor = status.Equals("Pass", StringComparison.OrdinalIgnoreCase) ? Color.Green : Color.Red;
                    // Debug output
                    //System.Diagnostics.Debug.WriteLine("---- Last Valid DUT Result ----");
                    //System.Diagnostics.Debug.WriteLine($"status   : {status}");
                    //System.Diagnostics.Debug.WriteLine($"test_time : {testTime}");
                    //System.Diagnostics.Debug.WriteLine($"model    : {model}");
                    //System.Diagnostics.Debug.WriteLine($"serial_no  : {serial}");
                    //System.Diagnostics.Debug.WriteLine($"capacity : {capacity}");
                    //System.Diagnostics.Debug.WriteLine($"station  : {station}");

                    currentStage.Fill_Response_Data("----K_STAGE-Log File Data---");
                    currentStage.Fill_Response_Data($"status   : {status}");
                    currentStage.Fill_Response_Data($"test_time : {testTime}");
                    currentStage.Fill_Response_Data($"model    : {model}");
                    currentStage.Fill_Response_Data($"serial_no  : {serial}");
                    currentStage.Fill_Response_Data($"capacity : {capacity}");
                    currentStage.Fill_Response_Data($"station  : {station}");


                    if (status.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                    {
                        //var qc = new k_stage(stage_N, Gtype,app_name,app_path,fgno,emp_id,emp_name,firmware_name, filePath);
                        boardfail = false;
                        currentStage.SQL_Upload(serial, boardfail, "Passed at K1");
                        currentStage.lbl_result.Text = $"Serial no: {serial} is Pass in K1 stage";
                        currentStage.lbl_result.BackColor = Color.Green;
                        currentStage.lbl_result.ForeColor = Color.White;
                        currentStage.txt_SN.Clear();
                        currentStage.Fill_Response_Data($"Serial no: {serial} is Pass in K1 stage");
                        popup.ShowDialogMessage($"{serial} -- {status} at K1", displayColor, Color.White, false);
                        writestatusMessage($"(Serial No: {serial}-- Capacity: {capacity} is Pass in K1", "Board status");
                        currentStage.Fill_Response_Data($"(Serial No: {serial}-- Capacity: {capacity} is Pass in K1");

                        if (currentStage.App_Name == "SM2268XT2_MPTool.exe")
                        {
                            currentStage.move_Formto_center();
                        }
                        else
                        {
                            currentStage.move_Formto_left();
                        }

                            //currentStage.qcstage_validation(serial, capacity, emp_id, emp_name);
                            return;
                    }
                    else
                    {
                        boardfail = true;
                        currentStage.SQL_Upload(serial, boardfail, "Failed at K1");
                        currentStage.lbl_result.Text = $"Serial no: {serial} is FAIL in K1 stage";
                        currentStage.lbl_result.BackColor = Color.Red;
                        currentStage.lbl_result.ForeColor = Color.White;
                        currentStage.txt_SN.Clear();
                        currentStage.Fill_Response_Data($"Serial no: {serial} is FAIL in K1 stage");
                        popup.ShowDialogMessage($"{serial} -- {status} at K1", displayColor, Color.White, false);
                        writestatusMessage($"(Serial No: {serial}-- Capacity: {capacity} is fail in K1", "Board status");
                        //var instance = new k_stage(stage_N, Gtype, app_name, app_path, fgno, emp_id, emp_name, filePath, firmware_name);

                        if (currentStage.App_Name == "SM2268XT2_MPTool.exe")
                        {
                            currentStage.move_Formto_center();
                        }
                        else
                        {
                            currentStage.move_Formto_left();
                        }

                    }

                }
                //else if (stage_count == "1")
                //{
                //    // var displayColor =
                //    popup.ShowDialogMessage($"{serial} --is already pass at K3 Please scan the next serial Number", Color.Red, Color.White, false);
                //    writestatusMessage($"Serial No: {serial}-- Capacity: {capacity} is already pass at k3", "Board status");
                //    currentStage.Fill_Response_Data($"Serial No: {serial}-- Capacity: {capacity} is already pass at k3");
                //    return;
                //}
                //else if (stage_count == "2")
                //{
                //    // var displayColor = Color.Red;
                //    popup.ShowDialogMessage($"{serial} --is already fail at K3 Please scan the next serial Number", Color.Red, Color.White, false);
                //    writestatusMessage($"Serial No: {serial}-- Capacity: {capacity} is already fail at k3", "Board status");
                //    currentStage.Fill_Response_Data($"Serial No: {serial}-- Capacity: {capacity} is already fail at k3");
                //    return;
                //}
                else
                {
                    writestatusMessage($"Serial No: {serial}-- Capacity: {capacity} status not found", "Stage Mismatch");
                    currentStage.Fill_Response_Data($"Serial No: {serial}-- Capacity: {capacity} SerialNumber Mismatch");
                    currentStage.lbl_result.Text = $"Serial No: {serial}-- Capacity: {capacity} SerialNumber Mismatch\n" +
                                                   $"Please check the scanned serial Number";
                    currentStage.lbl_result.ForeColor=Color.White;
                    currentStage.lbl_result.BackColor=Color.Red;
                    currentStage.move_Formto_left();
                    currentStage.txt_SN.Clear();
                    return;
                }

            }
            else
            {

                writestatusMessage($"(Serial No: {serial}-- Capacity: {capacity} is fail -Serial Number not scanned Properly", "Board status");
                popup.ShowDialogMessage($"Please scan the correct serial Number", Color.Red, Color.White, false);
                currentStage.Fill_Response_Data($"(Serial No: {serial}-- Capacity: {capacity} is fail -Serial Number not scanned Properly");
                currentStage.lbl_result.Text = $"Please scan the correct serial Number";
                currentStage.lbl_result.ForeColor = Color.White;
                currentStage.lbl_result.BackColor = Color.Red;
                currentStage.move_Formto_left();
                currentStage.txt_SN.Clear();
                return;
            }


        }

        #endregion 

        private void Fill_Response_Data(string datarep)
        {
            txt_live_stat.AppendText(
                DateTime.Now.ToString("HH:mm:ss.fff") + " : " + datarep + "\n");

            txt_live_stat.SelectionStart = txt_live_stat.TextLength;
            txt_live_stat.ScrollToCaret();

            //base.Update();

            //System.Windows.Forms.Application.DoEvents();
        }


        #region QC_Valdiation


        //public void qcstage_validation(string serialno, string capacity, string emp_id, string emp_name)
        //{
        //    writestatusMessage($"Serial No: {serialno}-- Capacity: {capacity} is pass IN K3 and in Qc stage", "Board status");
        //    Fill_Response_Data($"Serial No: {serialno}-- Capacity: {capacity} is pass in K3 and in Qc stage");
        //    var fg_num = dbConnection.getfgname(serialno);
        //    fg_num = "ECH3ESN00077";
        //    if ((!string.IsNullOrEmpty(fg_num)))
        //    {
        //        writestatusMessage($"Serialno :{serialno}-- Fgno :{fg_num}", "FgName-SerialNumber");
        //        var fg_details_list = dbConnection.getfgdetails(fg_num);
        //        foreach (var fg_detail in fg_details_list)
        //        {
        //            var Model = fg_detail.Model;
        //            var Firmware = fg_detail.Firmware;
        //            var Disksize = fg_detail.Disksize;
        //            writestatusMessage($"Serialno :{serialno}-- Fgno :{fg_num}--Model :{Model}--Firmware :{Firmware}--Disksize :{Disksize}", "Fg details");
        //            Fill_Response_Data("----------Values for reference from the database---------");
        //            Fill_Response_Data($"Serialno :{serialno}-- Fgno :{fg_num}--Model :{Model}--Firmware :{Firmware}--Disksize :{Disksize}");
        //            processScannedText(Model, Firmware, Disksize, serialno, fg_num, emp_id, emp_name);

        //        }


        //    }
        //    else
        //    {

        //        MessageBox.Show("No Fg details Found for the provided Serial number");
        //        return;
        //    }


        //}
        private bool serialnopass = true;
        private bool firwarepass = true;
        private bool modelnopass = true;
        private bool healthstatpass = true;
        private bool temperaturepass = true;
        private bool driveletterpass = true;
        private bool diskSizePass = true;
        private bool ignoreTextChange = false;



        //public void processScannedText(string Modeldb, string firmwaredb, string Disksizedb, string serialnodb, string fgnodb,
        //                               string emp_iddb, string emp_namedb)
        //{
        //    SetProcessDPIAware();

        //    string exePath = ConfigurationManager.AppSettings["exePath"];
        //    if (!File.Exists(exePath))
        //    {
        //        MessageBox.Show("Application file not found at path: " + exePath);
        //        writestatusMessage($"CrystalDisk App path not found", "Application Not found");
        //        return;
        //    }
        //    string processName = Path.GetFileNameWithoutExtension(exePath);
        //    string savePath = ConfigurationManager.AppSettings["savePath"];

        //    if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        //    {
        //        writestatusMessage($"CrystalDisk file path not found", "File Not found");
        //        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        //    }

        //    try
        //    {
        //        Process crystalprocess = null;
        //        ManualResetEvent loadingFormReady = new ManualResetEvent(false);
        //        LoadingForm loadingForm = null;

        //        Thread loadingThread = new Thread(() =>
        //        {
        //            loadingForm = new LoadingForm();
        //            loadingForm.Load += (s, e) => loadingFormReady.Set();  // Signal that form is ready
        //            System.Windows.Forms.Application.Run(loadingForm);
        //        });
        //        loadingThread.SetApartmentState(ApartmentState.STA);
        //        loadingThread.Start();

        //        // Wait for the loading form to be fully loaded
        //        loadingFormReady.WaitOne();
        //        Fill_Response_Data("Loading QC Application");


        //        try
        //        {
        //            var existingProcess = Process.GetProcessesByName(processName).FirstOrDefault();

        //            if (existingProcess == null)
        //            {
        //                crystalprocess = Process.Start(exePath);

        //                // Wait for window (up to 30s)
        //                int timeout = 30000;
        //                int waited = 0;
        //                int interval = 500;


        //                while (crystalprocess.MainWindowHandle == IntPtr.Zero && waited < timeout)
        //                {
        //                    Thread.Sleep(interval);
        //                    waited += interval;
        //                    crystalprocess.Refresh();

        //                }

        //                if (crystalprocess.MainWindowHandle == IntPtr.Zero)
        //                {
        //                    writestatusMessage("CrystalDiskInfo UI window not ready", "Timeout waiting for window");
        //                    return;
        //                }
        //            }
        //            else
        //            {
        //                crystalprocess = existingProcess;
        //            }
        //        }
        //        finally
        //        {

        //            // loadingForm.Invoke(new System.Action(() => loadingForm.Close()));
        //            //  CLOSE LOADING UI

        //            if (loadingForm != null && loadingForm.IsHandleCreated)
        //            {
        //                loadingForm.Invoke(new System.Action(() =>
        //                {
        //                    loadingForm.Close();
        //                }));
        //            }


        //        }

        //        //  Continue with the rest of your code
        //        using (var automation = new UIA3Automation())
        //        {
        //            var desktop = automation.GetDesktop();
        //            Thread.Sleep(500);

        //            var processes = Process.GetProcesses()
        //                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowTitle.Contains("CrystalDiskInfo"));

        //            foreach (var process in processes)
        //            {
        //                try
        //                {
        //                    var app = FlaUI.Core.Application.Attach(process);
        //                    var mainWindow = app.GetMainWindow(automation);

        //                    if (mainWindow == null)
        //                    {
        //                        writestatusMessage($"CrystalDisk App window not found", "App window Not found");
        //                        continue;
        //                    }

        //                    writestatusMessage($"CrystalDisk App window  found", "App window  found");

        //                    // Maximize and focus the window
        //                    ShowWindow(mainWindow.Properties.NativeWindowHandle.ValueOrDefault, SW_MAXIMIZE);
        //                    mainWindow.Focus();
        //                    Thread.Sleep(500);

        //                    // Optionally click the disk button with AutomationId "1021"
        //                    Keyboard.Type(VirtualKeyShort.F6);
        //                    writestatusMessage("F6 button clicked for refresh", "F6 button click");
        //                    Thread.Sleep(5000); // Let it refresh

        //                    var targetButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("1021"))?.AsButton();
        //                    if (targetButton == null)
        //                    {
        //                        Console.WriteLine("Button Not found");
        //                        writestatusMessage("Disk button Not found for Essencore disk", "Disk button not found for click");

        //                    }

        //                    targetButton.Invoke();
        //                    Thread.Sleep(700);
        //                    writestatusMessage("Disk button clicked for Essencore disk", "Disk button click");
        //                    var editElement_Mod = mainWindow.FindFirstDescendant(cf =>
        //                                          cf.ByAutomationId("1012").And(cf.ByControlType(ControlType.Edit)));

        //                    if (editElement_Mod == null)
        //                    {
        //                        Console.WriteLine("Edit control not found.");
        //                        writestatusMessage("Model label not found", "Model label not found");
        //                        return;
        //                    }
        //                    writestatusMessage("Model label  found", "Model label  found");

        //                    // Try ValuePattern
        //                    var valuePattern = editElement_Mod.Patterns.Value.PatternOrDefault;
        //                    if (valuePattern != null)
        //                    {
        //                        currentText = valuePattern.Value;
        //                        Console.WriteLine($"Edit control text: {currentText}");
        //                        writestatusMessage($"Model text: {currentText}", "Model label found");
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("ValuePattern is not supported for this Edit control.");

        //                    }

        //                    var Model_val = currentText;
        //                    if (Model_val != null)
        //                    {


        //                        // Regex: captures everything before the size and then the size itself
        //                        var match1 = Regex.Match(Model_val, @"^(.*)\s(\d+(?:\.\d+)?)\s(GB|MB)$");

        //                        if (match1.Success)
        //                        {
        //                            Model_name = match1.Groups[1].Value.Trim();       // "ESSENCORE NVME GEN3 SSD"
        //                            Disk_size = match1.Groups[2].Value + " GB";       // "256.0 GB"
        //                            writestatusMessage($"Model name: {Model_name} DiskSize {Disk_size}", "Model value found");
        //                            Console.WriteLine($"Name: {Model_name}");
        //                            Console.WriteLine($"Size: {Disk_size}");
        //                        }
        //                        else
        //                        {
        //                            Console.WriteLine("No match.");
        //                        }


        //                    }
        //                    var health_stat_btn = mainWindow.FindFirstDescendant(cf =>
        //                                cf.ByAutomationId("1047").And(cf.ByControlType(ControlType.Button)));
        //                    var temp_stat_btn = mainWindow.FindFirstDescendant(cf =>
        //                                cf.ByAutomationId("1049").And(cf.ByControlType(ControlType.Button)));

        //                    if (health_stat_btn == null && temp_stat_btn == null)
        //                    {
        //                        Console.WriteLine("health status and temp_status not found.");
        //                        writestatusMessage("health status and temp_status not found", "Heath & Temp status not found");
        //                        return;
        //                    }

        //                    Health_status = health_stat_btn.Name.ToString();
        //                    Temp_status = temp_stat_btn.Name.ToString();


        //                    Match Temp_match = Regex.Match(Temp_status, @"\d+");

        //                    if (Temp_match.Success)
        //                    {
        //                        Temp_val = int.Parse(Temp_match.Value);
        //                        Console.WriteLine(Temp_val); // Output: 36
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("No number found.");
        //                    }

        //                    var Firmware_element = mainWindow.FindFirstDescendant(cf =>
        //                        cf.ByAutomationId("1014").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Document)));
        //                    var serialno_element = mainWindow.FindFirstDescendant(cf =>
        //                        cf.ByAutomationId("1015").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Document)));
        //                    var disk_letr_element = mainWindow.FindFirstDescendant(cf =>
        //                        cf.ByAutomationId("1031").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Document)));

        //                    if (Firmware_element == null && serialno_element == null && disk_letr_element == null)
        //                    {
        //                        Console.WriteLine("Firmware & Serialno & Disk letter control not found.");
        //                        return;
        //                    }

        //                    // 2. Try getting Text using TextPattern
        //                    var txt_Pattern_Firmware = Firmware_element.Patterns.Text.PatternOrDefault;
        //                    var txt_Pattern_Serialno = serialno_element.Patterns.Text.PatternOrDefault;
        //                    var txt_Pattern_Diskltr = disk_letr_element.Patterns.Text.PatternOrDefault;
        //                    if (txt_Pattern_Firmware == null && txt_Pattern_Serialno == null && txt_Pattern_Diskltr == null)
        //                    {
        //                        Console.WriteLine("Serial no & Firmware & Diskletter not found.");
        //                        return;
        //                    }
        //                    Serial_no = txt_Pattern_Serialno.DocumentRange.GetText(-1);
        //                    Firmware_val = txt_Pattern_Firmware.DocumentRange.GetText(-1);
        //                    diskletter = txt_Pattern_Diskltr.DocumentRange.GetText(-1);
        //                    writestatusMessage($"Serial_nos: {Serial_no}--Firmware_val:{Firmware_val}--Diskletter :{diskletter} ", "Heath & Temp status found");

        //                    var Health_match = Regex.Match(Health_status, @"^(.*)\s+(\d+)\s*%?$", RegexOptions.Singleline);

        //                    if (Health_match.Success)
        //                    {
        //                        Health_stat = Health_match.Groups[1].Value.Trim();
        //                        Health_value = int.Parse(Health_match.Groups[2].Value);
        //                        Console.WriteLine($"Status: {Health_stat}");
        //                        Console.WriteLine($"Value: {Health_value}");
        //                        writestatusMessage($"Health status: {Health_stat}--Temp_status:{Health_value}", "Heath & Temp status found");
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Pattern not matched.");
        //                    }

        //                    bool serialnopass = false, firwarepass = false, modelnopass = false,
        //                    temperaturepass = false, healthstatpass = false, driveletterpass = false, diskSizePass = false;

        //                    if (!string.IsNullOrWhiteSpace(diskletter))
        //                    {

        //                        driveletterpass = false;
        //                    }
        //                    else
        //                    {

        //                        diskletter = "Null";
        //                        driveletterpass = true;
        //                    }
        //                    serialnopass = Serial_no.Trim() == serialnodb;
        //                    firwarepass = Firmware_val.Trim() == firmwaredb;
        //                    modelnopass = Model_name.Equals(Modeldb.Trim(), StringComparison.OrdinalIgnoreCase);
        //                    temperaturepass = (Temp_val > 25) && (Temp_val < 70);
        //                    healthstatpass = Health_stat.Equals("Good", StringComparison.OrdinalIgnoreCase) && Health_value > 80;
        //                    diskSizePass = Disk_size.Equals(Disksizedb.Trim());

        //                    Fill_Response_Data($"Serial_no    :{Serial_no.Trim()}--------{serialnopass}");
        //                    Fill_Response_Data($"Firmware_no  :{Firmware_val.Trim()}-----{firwarepass}");
        //                    Fill_Response_Data($"Model Name   :{Model_name.Trim()}-------{modelnopass}");
        //                    Fill_Response_Data($"Temperature  :{Temp_val.ToString()}-----{temperaturepass}");
        //                    Fill_Response_Data($"Health Status:{Health_stat.ToString()}--{healthstatpass}");
        //                    Fill_Response_Data($"Disk Size    :{Disk_size.ToString()}--{diskSizePass}");




        //                    bool allPass = serialnopass && firwarepass && modelnopass &&
        //                          temperaturepass && healthstatpass && driveletterpass && diskSizePass;


        //                    if (allPass)
        //                    {

        //                        board_status = "PASS";
        //                        writestatusMessage($"Serial no: {Serial_no} is Pass", "Final QC status");
        //                        Fill_Response_Data($"Serial no: {Serial_no} is Pass at K3 and CDI Stage");
        //                        SQL_Upload(Serial_no, false, "Passed at K3&QC");
        //                        popup.ShowDialogMessage($"{Serial_no} -- {board_status} at QC Testing", Color.Green, Color.White, true);
        //                        lbl_result.Text = $"Serial no: {Serial_no} is Pass in K3-CDI stage";
        //                        lbl_result.BackColor = Color.Green;
        //                        lbl_result.ForeColor = Color.White;
        //                        move_Formto_left();

        //                    }
        //                    else
        //                    {
        //                        board_status = "FAIL";
        //                        writestatusMessage($"Serial no: {Serial_no} is Fail", "Final QC status");
        //                        Fill_Response_Data($"Serial no: {Serial_no} is Fail at CDI Stage");
        //                        SQL_Upload(Serial_no, true, "Failed at QC");
        //                        popup.ShowDialogMessage($"{Serial_no} -- {board_status} at QC Testing", Color.Red, Color.White, false);
        //                        lbl_result.Text = $"Serial no: {Serial_no} is fail in CDI stage";
        //                        lbl_result.BackColor = Color.Red;
        //                        lbl_result.ForeColor = Color.White;
        //                        move_Formto_left();
        //                    }

        //                    var record = new QcDataRecord
        //                    {

        //                        emp_id = emp_iddb,
        //                        emp_name = emp_namedb,
        //                        Fg_no = fgnodb,
        //                        SerialNumber = Serial_no,
        //                        Firmware = Firmware_val,
        //                        Temperature = Temp_val,
        //                        HealthStatus = Health_status,
        //                        ModelNumber = Model_name,
        //                        DriveLetter = diskletter,
        //                        DiskSize = Disk_size,
        //                        boardstatus = board_status
        //                    };


        //                    List<QcDataRecord> records = new List<QcDataRecord> { record };

        //                    // Pass the list to the upload function
        //                    dbConnection.uploadresult(records);


        //                    try
        //                    {
        //                        if (crystalprocess != null && !crystalprocess.HasExited)
        //                        {
        //                            crystalprocess.Kill();
        //                            crystalprocess.WaitForExit();
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        MessageBox.Show("Failed to close CrystalDiskInfo process: " + ex.Message);
        //                    }

        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show("Error:" + ex.Message);
        //                }

        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        MessageBox.Show(" Error: " + ex.Message);
        //    }
        //}


        private string GetClipboardTextSafe()
        {
            string clipboardText = string.Empty;
            var thread = new Thread(() =>
            {
                try
                {
                    clipboardText = Clipboard.GetText();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Clipboard Error: " + ex.Message);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return clipboardText;
        }
        private void ClearClipboardSafe()
        {
            var thread = new Thread(() =>
            {
                try
                {
                    bool cleared = false;
                    for (int i = 0; i < 3 && !cleared; i++)
                    {
                        Clipboard.Clear();
                        if (string.IsNullOrWhiteSpace(Clipboard.GetText()))
                        {
                            cleared = true;
                            break;
                        }
                        Thread.Sleep(300);
                    }

                    // Use BeginInvoke to prevent deadlock
                    this.BeginInvoke(new System.Action(() =>
                    {
                        if (!cleared)
                            MessageBox.Show(" Clipboard could not be cleared after retries.");
                        else
                            MessageBox.Show(" Clipboard cleared successfully.");
                    }));
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new System.Action(() =>
                    {
                        MessageBox.Show("Clipboard Clear Error: " + ex.Message);
                    }));
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(); // no .Join() to avoid deadlock
        }


        private List<string> tempImageFiles = new List<string>();
        private void SaveImageSafe(Bitmap bmp, string baseFileName)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    $"{baseFileName}_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                tempImageFiles.Add(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($" Failed to save image {baseFileName}: {ex.Message}");
            }
        }


        private void pic_status_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm();

            reportForm.ShowDialog();

        }

        #endregion

        #region Errorlogs
        public static void writestatusMessage(string Status_message, string functionName)
        {
            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Syrma_Training_Errors" + "\\" + DateTime.Now.ToString("dd-MM-yyyy");

            if (!Directory.Exists(systemPath))
            {
                Directory.CreateDirectory(systemPath);
            }

            string StatusLog = String.Format(@"{0}\{1}.txt", systemPath, "StatusLogK3QC");
            using (StreamWriter statLogs = new StreamWriter(StatusLog, true))
            {
                statLogs.WriteLine("--------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                statLogs.WriteLine("---------------------------------------------------" + DateTime.Now + "----------------------------------------------" + Environment.NewLine);
                statLogs.WriteLine(Environment.NewLine + "-----" + functionName + "-----");
                statLogs.WriteLine(Status_message + Environment.NewLine);
                statLogs.Close();
            }
        }

        #endregion




        #region SFCS_DATA
        //public bool Check_Curr_Stage(string serialno, string app_id, string stage, bool boardonline)
        //{
        //    try
        //    {
        //        if (boardonline)
        //        {
        //            SFCS_db.Close();
        //            using (SqlCommand cmd = new SqlCommand(
        //                "SELECT * FROM PCBA_NextStage(NOLOCK) WHERE PCBA_Id = '" + serialno + "'", SFCS_db))
        //            {
        //                if (SFCS_db.State == ConnectionState.Closed)
        //                    SFCS_db.Open();

        //                using (SqlDataReader sdr = cmd.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        infosfromboard[0] = sdr["Work_order_no"].ToString();
        //                        infosfromboard[1] = sdr["Rework_count"].ToString();

        //                        Fill_Response_Data("Board Waiting ID : " + sdr["Next_Stage_id"].ToString());
        //                        Fill_Response_Data("Board Workorder : " + infosfromboard[0]);
        //                        Fill_Response_Data("Board RW : " + infosfromboard[1]);
        //                        lbl_wo.Text = infosfromboard[0];
        //                        lbl_try.Text= infosfromboard[1];
        //                        Get_Completed_Qty();
        //                        if (sdr["Next_Stage_id"].ToString() == app_id)
        //                        {
        //                            //SFCS_db.Close();
        //                            return true;
        //                        }
        //                        else
        //                        {
        //                            errordesc = "Stage Mismatch for this PCB : " + serialno + ".\n" +
        //                                        "Expected is : " + sdr["Next_Stage_id"] + "|" + sdr["Next_Stage_name"] + ".\n" +
        //                                        "Actual is : " + app_id + "|" + stage + ".";
        //                            lbl_result.Text = errordesc;
        //                            lbl_result.BackColor = Color.Red;
        //                            lbl_result.ForeColor = Color.White;
        //                            //SFCS_db.Close();
        //                            return false;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        errordesc = "No Data for this PCB : " + serialno + " in SFCS Master Table.";
        //                        lbl_result.Text = errordesc;
        //                        lbl_result.BackColor = Color.Red;
        //                        lbl_result.ForeColor = Color.White;
        //                        //SFCS_db.Close();
        //                        return false;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        errordesc = "Exception. " + ex.Message;
        //        lbl_result.Text = errordesc;
        //        lbl_result.BackColor = Color.Red;
        //        lbl_result.ForeColor = Color.White;
        //        return false;
        //    }
        //}


        //--------------------Version without retry count 
        //public bool Check_Curr_Stage(string serialno, string app_id, string stage, bool boardonline)
        //{
        //    if (!boardonline)
        //        return true;

        //    try
        //    {
        //        // Ensure connection is closed before starting
        //        if (SFCS_db.State == ConnectionState.Open)
        //            SFCS_db.Close();

        //        using (SqlCommand cmd = new SqlCommand(
        //            "SELECT Work_order_no, Rework_count, Next_Stage_id, Next_Stage_name " +
        //            "FROM PCBA_NextStage(NOLOCK) WHERE PCBA_Id = @serialno", SFCS_db))
        //        {
        //            cmd.Parameters.Add("@serialno", SqlDbType.VarChar).Value = serialno;

        //            SFCS_db.Open();

        //            using (SqlDataReader sdr = cmd.ExecuteReader())
        //            {
        //                if (!sdr.Read())
        //                {
        //                    lbl_result.Text = "No Data for this PCB : " + serialno;
        //                    lbl_result.BackColor = Color.Red;
        //                    lbl_result.ForeColor = Color.White;
        //                    return false;
        //                }

        //                string workOrder = sdr["Work_order_no"].ToString();
        //                string reworkCount = sdr["Rework_count"].ToString();
        //                string nextStageId = sdr["Next_Stage_id"].ToString();
        //                string nextStageName = sdr["Next_Stage_name"].ToString();

        //                infosfromboard[0] = workOrder;
        //                infosfromboard[1] = reworkCount;

        //                Fill_Response_Data("Board Waiting ID : " + nextStageId);
        //                Fill_Response_Data("Board Workorder : " + workOrder);
        //                Fill_Response_Data("Board RW : " + reworkCount);

        //                lbl_wo.Text = workOrder;
        //                lbl_try.Text = reworkCount;

        //                Get_Completed_Qty();

        //                if (nextStageId == app_id)
        //                {
        //                    return true;
        //                }

        //                lbl_result.Text =
        //                    $"Stage Mismatch for PCB {serialno}\n" +
        //                    $"Expected: {nextStageId} | {nextStageName}\n" +
        //                    $"Actual: {app_id} | {stage}";

        //                lbl_result.BackColor = Color.Red;
        //                lbl_result.ForeColor = Color.White;

        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lbl_result.Text = "Exception: " + ex.Message;
        //        lbl_result.BackColor = Color.Red;
        //        lbl_result.ForeColor = Color.White;
        //        return false;
        //    }
        //}
        //------------------------------

        public bool Check_Curr_Stage(string serialno, string app_id, string stage, bool boardonline)
        {
            if (!boardonline)
                return true;

            try
            {
                if (SFCS_db.State == ConnectionState.Open)
                    SFCS_db.Close();

                string workOrder = string.Empty;
                string reworkCount = string.Empty;
                string nextStageId = string.Empty;
                string nextStageName = string.Empty;

                // -----------------------------
                // 1️⃣ Get Next Stage Information
                // -----------------------------
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Work_order_no, Rework_count, Next_Stage_id, Next_Stage_name " +
                    "FROM PCBA_NextStage (NOLOCK) WHERE PCBA_Id = @serialno", SFCS_db))
                {
                    cmd.Parameters.Add("@serialno", SqlDbType.VarChar).Value = serialno;

                    SFCS_db.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (!sdr.Read())
                        {
                            lbl_result.Text = "No Data for this PCB : " + serialno;
                            lbl_result.BackColor = Color.Red;
                            lbl_result.ForeColor = Color.White;
                            return false;
                        }

                        workOrder = sdr["Work_order_no"]?.ToString();
                        reworkCount = sdr["Rework_count"]?.ToString();
                        nextStageId = sdr["Next_Stage_id"]?.ToString();
                        nextStageName = sdr["Next_Stage_name"]?.ToString();
                    }

                    SFCS_db.Close();
                }

                infosfromboard[0] = workOrder;
                infosfromboard[1] = reworkCount;

                Fill_Response_Data("Board Waiting ID : " + nextStageId);
                Fill_Response_Data("Board Workorder : " + workOrder);
                Fill_Response_Data("Board RW : " + reworkCount);

                lbl_wo.Text = workOrder;
                lbl_try.Text = reworkCount;

                Get_Completed_Qty();

                // -----------------------------
                // 2️⃣ Stage Match Check
                // -----------------------------
                if (nextStageId == app_id)
                {
                    boardfailcount = 0;

                    using (SqlCommand countCmd = new SqlCommand(
                        "SELECT COUNT(TEST) FROM TESTINGFAILCOUNTCHECK_ESSENCORE (NOLOCK) " +
                        "WHERE PCBAID = @pcbaid AND WORKORDER = @wo AND RW = @rw AND TEST = @test",
                        SFCS_db))
                    {
                        countCmd.Parameters.Add("@pcbaid", SqlDbType.VarChar).Value = serialno;
                        countCmd.Parameters.Add("@wo", SqlDbType.VarChar).Value = workOrder;
                        countCmd.Parameters.Add("@rw", SqlDbType.Int).Value =
                            string.IsNullOrEmpty(reworkCount) ? 0 : Convert.ToInt32(reworkCount);
                        countCmd.Parameters.Add("@test", SqlDbType.VarChar).Value = stage;

                        SFCS_db.Open();

                        object result = countCmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                            boardfailcount = (int)Convert.ToInt64(result);

                        SFCS_db.Close();
                    }

                    lbl_try.Text = (boardfailcount + 1).ToString();
                    this.Refresh();

                    return true;
                }
                else
                {
                    lbl_result.Text =
                        "Stage Mismatch for PCB : " + serialno + Environment.NewLine +
                        "Expected : " + nextStageId + " | " + nextStageName + Environment.NewLine +
                        "Actual : " + app_id + " | " + stage;

                    lbl_result.BackColor = Color.Red;
                    lbl_result.ForeColor = Color.White;

                    return false;
                }
            }
            catch (Exception ex)
            {
                if (SFCS_db.State == ConnectionState.Open)
                    SFCS_db.Close();

                lbl_result.Text = "Exception: " + ex.Message;
                lbl_result.BackColor = Color.Red;
                lbl_result.ForeColor = Color.White;

                return false;
            }
        }




        private void SQL_Upload(string Sno, bool boardfail, string Result_Remarks)
        {


            try
            {
                SFCS_db.Close();

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO DASHBOARD_ESSENCOREDATAS VALUES (" +
                    "'" + lbl_app_id.Text + "'," +
                    "'" + lblstagename.Text + "'," +
                    "'" + infosfromboard[0] + "'," +
                    "'" + Pcb_Serialno + "'," +
                    "'" + Sno + "'," +
                    "'" + (boardfail ? "FAIL" : "PASS") + "'," +
                    "'" + errordesc + "'," +
                    "'" + infosfromboard[1] + "'," +
                    "CONCAT(FORMAT(CURRENT_TIMESTAMP,'HH'),'-',FORMAT(DATEADD(HOUR,1,CURRENT_TIMESTAMP),'HH'))," +
                    "CASE " +
                    "WHEN (FORMAT(CURRENT_TIMESTAMP,'HH:mm:ss') >= CONVERT(datetime,'08:00:00',103)) AND (FORMAT(CURRENT_TIMESTAMP,'HH:mm:ss') < CONVERT(datetime,'16:00:00',103)) THEN 'SHIFT-A' " +
                    "WHEN (FORMAT(CURRENT_TIMESTAMP,'HH:mm:ss') >= CONVERT(datetime,'16:00:00',103)) AND (FORMAT(CURRENT_TIMESTAMP,'HH:mm:ss') < CONVERT(datetime,'23:59:59',103)) THEN 'SHIFT-B' " +
                    "ELSE 'SHIFT-C' END," +
                    "FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy')," +
                    "'" + lblemp_id.Text + "'," +
                    "FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy HH:mm:ss')," +
                    "HOST_NAME(),'','','')",
                    SFCS_db);

                if (SFCS_db.State == ConnectionState.Closed)
                    SFCS_db.Open();

                cmd.ExecuteNonQuery();
                SFCS_db.Close();
                Fill_Response_Data("SQL Report Success. - SFCS Dashboard");
            }
            catch (Exception ex)
            {
                Update_Error_in_Server("Exception", "ERR-SQL-02", ex.Message.ToString(),
                    "SFCS Dashboard", "PCBA:" + Pcb_Serialno + ",Workorder:" + lblemp_id.Text + ",CustomerNo:" + Sno + ".");
                lbl_result.Text += "SFCS Dashboard Failed.";
                lbl_result.BackColor = Color.Red;
                lbl_result.ForeColor = Color.Yellow;
                Fill_Response_Data("SQL Report Failed. - SFCS Dashboard");
            }

            for (int tryupdate = 1; tryupdate <= 3; tryupdate++)
            {

                //If(boardfail, If(boardfailcount > 1, reworkidinfo(0), lbl_app_id.Text), nextidinfo(0))
                // "Next_Stage_Id = '" + (boardfail ? reworkidinfo[0] : nextidinfo[0]) + "', " +
                // "Next_Stage_Name = '" + (boardfail ? reworkidinfo[1] : nextidinfo[1]) + "', " +
                try
                {
                    SFCS_db.Close();

                    SqlCommand cmd = new SqlCommand(
                        "UPDATE PCBA_NextStage SET " +

                        "Next_Stage_Id = '" +(boardfail? (boardfailcount > 1 ? reworkidinfo[0] : lbl_app_id.Text): nextidinfo[0]) + "', " +
                        "Next_Stage_Name = '" + (boardfail ? (boardfailcount > 1 ? reworkidinfo[1] : lblstagename.Text) : nextidinfo[1]) + "', " +
                        "Previous_Stage = '" + lblstagename.Text + "', " +
                        "Update_timestamp = FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy HH:mm:ss.ffff'), " +
                        "Update_Machine_id = HOST_NAME(), " +
                        "Update_Emp_id = '" + lblemp_id.Text + "' " +
                        "WHERE PCBA_Id = '" + Pcb_Serialno + "'",
                        SFCS_db);

                    if (SFCS_db.State == ConnectionState.Closed)
                        SFCS_db.Open();

                    cmd.ExecuteNonQuery();
                    SFCS_db.Close();
                    Fill_Response_Data("Next Stage : " + (boardfail ? (boardfailcount > 1 ? reworkidinfo[1] : lblstagename.Text) : nextidinfo[1]));
                    Fill_Response_Data("SFCS Next Stage Update Success.");
                    break;
                }
                catch (Exception ex)
                {
                    Update_Error_in_Server("Exception", "ERR-SQL-03", ex.Message.ToString(),
                        "SFCS Nextstage Failed", "PCBA:" + Pcb_Serialno + ",Workorder:" + infosfromboard[0] + ",CustomerNo:" + Sno + ".");
                    lbl_result.Text += "SFCS Nextstage Failed.";
                    lbl_result.BackColor = Color.Red;
                    lbl_result.ForeColor = Color.Yellow;
                    Fill_Response_Data("SFCS Next Stage Update Failed.");
                }
            }

            try
            {
                SFCS_db.Close();

                SqlCommand cmd = new SqlCommand("INSERT INTO FCT VALUES('CHN1','" + lblstagename.Text + "','" + Sno + "','" + (boardfail ? "FAIL" : "PASS") + "','" + errordesc + "',FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy HH:mm:ss.ffff'),'" + lblemp_id.Text + "',HOST_NAME(),'" + infosfromboard[1] + "','" + infosfromboard[0] + "','')", SFCS_db);
                if (SFCS_db.State == ConnectionState.Closed)
                    SFCS_db.Open();
                cmd.ExecuteNonQuery();
                SFCS_db.Close();
                Fill_Response_Data("SFCS FCT Success.");
            }
            catch (Exception ex)
            {
                Update_Error_in_Server("Exception", "ERR-SQL-04", ex.Message.ToString(),
                    "SFCS FCT Failed", "PCBA:" + Pcb_Serialno + ",Workorder:" + infosfromboard[0] + ",CustomerNo:" + Sno + ".");
                lbl_result.Text += "SFCS FCT Failed.";
                lbl_result.BackColor = Color.Red;
                lbl_result.ForeColor = Color.Yellow;
                Fill_Response_Data("SFCS FCT Update Failed.");
            }

            if (boardfail)
            {
                try
                {
                    if (SFCS_db.State == ConnectionState.Open)
                        SFCS_db.Close();
                 

                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO TESTINGFAILCOUNTCHECK_ESSENCORE VALUES ('" +
                        infosfromboard[0] + "','" +
                        lblstagename.Text + "','" +
                        Pcb_Serialno + "','" +
                        infosfromboard[1] + "','" +
                        (boardfail ? "FAIL" : "PASS") + "','" +
                        errordesc + "','Try : " +
                        lbl_try.Text + "'," +
                        "FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy HH:mm:ss'),'" +
                        infologindetails[0] + "'," +
                        "SUBSTRING(HOST_NAME(),1,50),'','','')",
                        SFCS_db);

                    if (SFCS_db.State == ConnectionState.Closed)
                        SFCS_db.Open();

                    cmd.ExecuteNonQuery();
                    SFCS_db.Close();

                    Fill_Response_Data("SFCS Fail Count Update Success.");
                }
                catch (Exception ex)
                {
                    Update_Error_in_Server(
                        "Exception",
                        "ERR-SQL-05",
                        ex.Message.ToString(),
                        "SFCS Fail Count Failed",
                        "CustomerNo:" + Sno +
                        ",Workorder:" + infosfromboard[0] +
                        ",PCBA:" + Pcb_Serialno + ".");

                    lbl_result.Text += "SFCS Fail Count Update Failed.";
                    lbl_result.BackColor = Color.Red;
                    lbl_result.ForeColor = Color.Yellow;

                    Fill_Response_Data("SFCS Fail Count Update Failed.");
                }
            }


        }

        private void Update_Error_in_Server(string errortype, string errorcode, string errordesc, string errorloc, string errorremarks)
        {
            string inqry = string.Empty;
            try
            {
                SFCS_db.Close();

                inqry = "INSERT INTO EXCEPTIONLOGS_MEMORY VALUES ('" +
                        errortype + "','" +
                        lbl_app_id.Text + "','" +
                        lblstagename.Text + "','" +
                        lblappver.Text + "','" +
                        errorcode + "','" +
                        errordesc.Replace("'", "@") + "','" +
                        errorloc.Replace("'", "@") + "','" +
                        errorremarks.Replace("'", "@") + "'," +
                        "FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy')," +
                        "FORMAT(CURRENT_TIMESTAMP,'dd-MM-yyyy HH:mm:ss.fff')," +
                        "HOST_NAME(),'" + lblemp_id.Text + "','','')";

                using (SqlCommand cmd = new SqlCommand(inqry, SFCS_db))
                {
                    if (SFCS_db.State == ConnectionState.Closed)
                        SFCS_db.Open();

                    cmd.ExecuteNonQuery();
                    SFCS_db.Close();
                }
            }
            catch (Exception ex)
            {
                Fill_Response_Data("Exception in Updating Error in Server.");
                Fill_Response_Data(ex.Message.ToString());
            }

        }

        private void Get_Completed_Qty()
        {
            string inqry = string.Empty;

            try
            {
                if (lbl_wo.Text == "NA")
                {
                    lbl_qty.Text = "0";
                    return;
                }

                SFCS_db.Close();

                inqry = "SELECT COUNT(DISTINCT PCBID) AS PCBCOUNT " +
                        "FROM DASHBOARD_ESSENCOREDATAS " +
                        "WHERE STAGENAME = '" + lblstagename.Text + "' " +
                        "AND WORKORDER = '" + lbl_wo.Text + "'";

                using (SqlCommand cmd = new SqlCommand(inqry, SFCS_db))
                {
                    if (SFCS_db.State == ConnectionState.Closed)
                        SFCS_db.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            lbl_qty.Text = Convert.ToInt64(sdr["PCBCOUNT"]).ToString();
                        }
                        else
                        {
                            lbl_qty.Text = "0";
                        }
                    }

                    SFCS_db.Close();
                }
            }
            catch (Exception ex)
            {
                lbl_qty.Text = "0";
                // optional logging
                // Fill_Response_Data(ex.Message);
            }

            //this.Refresh();
        }

        private void startchecksfcs()
        {
            try
            {
                if (SFCS_db.State == ConnectionState.Open)
                    SFCS_db.Close();

                SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM RoutingStages WHERE FG = '" + Fg + "'",
                    SFCS_db);

                if (SFCS_db.State == ConnectionState.Closed)
                    SFCS_db.Open();

                SqlDataReader sdr = cmd.ExecuteReader();

                if (sdr.Read())
                {
                    try
                    {
                        string[] stages = sdr["Stages"].ToString().Split(',');
                        int index = Array.IndexOf(stages, lbl_app_id.Text);

                        if (index >= 0 && index < stages.Length - 1)
                            nextidinfo[0] = stages[index + 1];
                    }
                    catch (Exception)
                    {
                        // ignored (same as VB empty catch)
                    }
                }

                sdr.Close();
                SFCS_db.Close();

                string cmd1 = "SELECT App_ID, Application_Name FROM App_ver WHERE App_ID = '" + nextidinfo[0] + "'";
                SqlDataAdapter da1 = new SqlDataAdapter(cmd1, SFCS_db);
                DataSet ds1 = new DataSet();
                da1.Fill(ds1, "app_name");

                if (ds1.Tables[0].Rows.Count > 0)
                {
                    nextidinfo[1] = ds1.Tables[0].Rows[0][1].ToString();
                }

                SFCS_db.Close();

                Fill_Response_Data("SFCS Next Stage : " + nextidinfo[0] + "|" + nextidinfo[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(),
                                "Exception in Fetching Next Stage",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                return;   // equivalent to Exit Sub
            }
        }




        #endregion
    }
}

