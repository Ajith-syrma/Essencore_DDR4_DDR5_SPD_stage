using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SPD_Stage
{
    class Fgdetails
    {
        //  public int barcodeid { get; set; }
        public string Firmware { get; set; }
        public string Model { get; set; }
        public string Disksize { get; set; }
        // public string Bar_Description { get; set; }
        //public string ProductNo { get; set; }
        //public string CustomerSerialNo { get; set; }
        //public string PCBSerialNo { get; set; }
        ////public string DateandTime { get; set; }
        ////public string ShiftType { get; set; }
        //public int WeekDetails { get; set; }
        //public string Dublicate { get; set; }

    }
    class appFgdetails
    {
        //  public int barcodeid { get; set; }
        public string app_name { get; set; }
        public string fg_model { get; set; }
        public string app_path { get; set; }
        public string app_logpath { get; set; }
        //public string Disksize { get; set; }
        // public string Bar_Description { get; set; }
        //public string ProductNo { get; set; }
        //public string CustomerSerialNo { get; set; }
        //public string PCBSerialNo { get; set; }
        ////public string DateandTime { get; set; }
        ////public string ShiftType { get; set; }
        //public int WeekDetails { get; set; }
        //public string Dublicate { get; set; }

    }

    public class QcDataRecord
    {
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string Fg_no { get; set; }
        public string SerialNumber { get; set; }
        public string Firmware { get; set; }
        public int Temperature { get; set; }
        public string HealthStatus { get; set; }
        public string ModelNumber { get; set; }
        public string DriveLetter { get; set; }
        public string DiskSize { get; set; }
        public string boardstatus { get; set; }
    }

    class UserDetails
    {
        public string emp_id { get; set; }
    }

    class scanned_dt_val
    {
        public string PCB_SNO { get; set; }
        public string Fg_no { get; set; }  
    }
    class FileState
    {
        public DateTime LastWriteTime { get; set; }
        public string LastHash { get; set; }
    }
}
