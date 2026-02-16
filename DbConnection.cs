using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace K1_Stages
{

    class DbConnection
    {
        SqlConnection SFCS_db = new SqlConnection(ConfigurationManager.AppSettings["SFCS"].ToString());
        SqlConnection Barcode_db = new SqlConnection(ConfigurationManager.AppSettings["BARCODE"].ToString());
        SqlConnection Essencore_db = new SqlConnection(ConfigurationManager.AppSettings["ESSENCORE"].ToString());
        SqlCommand cmd;
        SqlDataAdapter adapter;
        SqlDataReader reader;
        DataTable dt;
        public string DbConnect(string serial, string model, string capacity, string station,
                                string testTime, string status)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("pro_update_result_stat", Essencore_db))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Stage", "K3");
                    cmd.Parameters.AddWithValue("@serial_no", serial);
                    cmd.Parameters.AddWithValue("@model", model);
                    cmd.Parameters.AddWithValue("@capacity", capacity);
                    cmd.Parameters.AddWithValue("@station", station);
                    cmd.Parameters.AddWithValue("@test_time", testTime);
                    cmd.Parameters.AddWithValue("@status", status);

                    Barcode_db.Open();
                    object reader = cmd.ExecuteScalar();
                    Barcode_db.Close();

                    return reader?.ToString(); // handles null safely
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message); // ✅ show error
                return string.Empty;
            }
        }


        public string pcb_status(string serial, string model, string result)
        {
            try
            {
                cmd = new SqlCommand("pro_update_result_stat", SFCS_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@serial", serial);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@result", result);
                //cmd.Parameters.AddWithValue("@Work_Orderno", Work_Orderno);
                SFCS_db.Open();
                var reader = cmd.ExecuteScalar();
                SFCS_db.Close();
                return reader.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
                MessageBox.Show("Error : " + ex.Message.ToString());
            }
        }

        public string get_stage_valid(string serial, string model)
        {
            try
            {
                //cmd = new SqlCommand("pro_get_stage_val", con);
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@serial", serial);
                //cmd.Parameters.AddWithValue("@model", model);



                ////cmd.Parameters.AddWithValue("@result", result);
                ////cmd.Parameters.AddWithValue("@Work_Orderno", Work_Orderno);

                //con.Open();
                //var reader = cmd.ExecuteScalar();
                //con.Close();
                //return reader.ToString();
                return "Pass";
            }
            catch (Exception ex)
            {
                return string.Empty;
                MessageBox.Show("Error : " + ex.Message.ToString());
            }
        }


        public string get_serial_dup(string serial, string model)
        {
            try
            {
                cmd = new SqlCommand("pro_get_serial_dup", Barcode_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@serial_no", serial);
                // cmd.Parameters.AddWithValue("@model", model);
                //cmd.Parameters.AddWithValue("@result", result);
                //cmd.Parameters.AddWithValue("@Work_Orderno", Work_Orderno);
                Barcode_db.Open();
                var reader = cmd.ExecuteScalar();
                Barcode_db.Close();
                return reader.ToString();
                //return "Pass";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message.ToString());
                return string.Empty;
            }
        }
        public string getfgname(string serialno)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("pro_getfg_name", Barcode_db))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@serial_no", serialno);
                    Barcode_db.Open();
                    object reader = cmd.ExecuteScalar();
                    Barcode_db.Close();

                    return reader?.ToString(); // handles null safely
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message); // ✅ show error
                return string.Empty;
            }

        }

        public string getmodel(string fg)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("pro_getmodel_name", Barcode_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fg", fg);
                Barcode_db.Open();
                var result = cmd.ExecuteScalar();
                Barcode_db.Close();

                return result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
                return string.Empty;
            }

        }

        public string getgentype(string fg)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("pro_getgentype", Barcode_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fg", fg);
                Barcode_db.Open();
                var result = cmd.ExecuteScalar();
                Barcode_db.Close();

                return result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
                return string.Empty;
            }

        }
        public List<Fgdetails> getfgdetails(string fgno)
        {
            var list = new List<Fgdetails>();
            try
            {

                Fgdetails objBar;
                cmd = new SqlCommand("pro_getfg_details", Barcode_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fg_name", fgno);
                adapter = new SqlDataAdapter(cmd);
                dt = new DataTable();
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    objBar = new Fgdetails();
                    objBar.Firmware = Convert.ToString(dr["Firmware"]);
                    objBar.Model = Convert.ToString(dr["model"]);
                    objBar.Disksize = Convert.ToString(dr["disksize"]);
                    list.Add(objBar);
                }
                return list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", "Database not connected");
                return list;
            }
        }

        public List<appFgdetails> get_app_Name(string fg_name, string stage_name)
        {
            try
            {

                var lstappfg = new List<appFgdetails>();
                cmd = new SqlCommand("pro_get_app_Name", Essencore_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fg_name", fg_name);
                cmd.Parameters.AddWithValue("@stage_name", stage_name);
                adapter = new SqlDataAdapter(cmd);
                dt = new DataTable();
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    var objappfg = new appFgdetails();
                    objappfg.app_name = Convert.ToString(dr["app_name"]);
                    objappfg.app_path = Convert.ToString(dr["app_path"]);
                    objappfg.app_logpath = Convert.ToString(dr["app_logpath"]);
                    objappfg.fg_model = Convert.ToString(dr["fg_model"]);
                    lstappfg.Add(objappfg);
                }
                return lstappfg;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", "Database not connected");
                return new List<appFgdetails>();
            }
        }

        public void uploadresult(List<QcDataRecord> records)
        {
            try
            {


                foreach (var record in records)
                {
                    using (SqlCommand cmd = new SqlCommand("pro_InsertQcData", Essencore_db))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@empid", record.emp_id);
                        cmd.Parameters.AddWithValue("@emp_name", record.emp_name);
                        cmd.Parameters.AddWithValue("@Fg_no", record.Fg_no);
                        cmd.Parameters.AddWithValue("@SerialNumber", record.SerialNumber);
                        cmd.Parameters.AddWithValue("@Firmware", record.Firmware);
                        cmd.Parameters.AddWithValue("@Temperature", record.Temperature);
                        cmd.Parameters.AddWithValue("@HealthStatus", record.HealthStatus);
                        cmd.Parameters.AddWithValue("@ModelNumber", record.ModelNumber);
                        cmd.Parameters.AddWithValue("@DriveLetter", record.DriveLetter);
                        cmd.Parameters.AddWithValue("@DiskSize", record.DiskSize);
                        cmd.Parameters.AddWithValue("@boardstatus", record.boardstatus);
                        Barcode_db.Open();
                        cmd.ExecuteNonQuery();
                        Barcode_db.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error uploading data: " + ex.Message);
            }
        }






        //public List<BarcodeDetails> GetBarcodeDetails(int labelid)
        //{
        //    var list = new List<BarcodeDetails>();
        //    try
        //    {

        //        BarcodeDetails objBar;
        //        cmd = new SqlCommand("pro_getPrintedValue", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@labelmasterid", labelid);
        //        adapter = new SqlDataAdapter(cmd);
        //        dt = new DataTable();
        //        adapter.Fill(dt);
        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            objBar = new BarcodeDetails();
        //            objBar.CustomerSerialNo = Convert.ToString(dr["CustomerSerialNo"]);
        //            objBar.PCBSerialNo = Convert.ToString(dr["PCBSerialNo"]);
        //            objBar.ProductNo = Convert.ToString(dr["productno"]);
        //            list.Add(objBar);
        //        }
        //        return list;
        //    }
        //    catch (Exception ex)
        //    {
        //        return list;
        //        MessageBox.Show("Error", "Database not connected");
        //    }
        //}

        //public List<labeltype> GetLabels()
        //{
        //    var lstType = new List<labeltype>();
        //    try
        //    {

        //        labeltype objType = new labeltype();
        //        cmd = new SqlCommand("getLabelType", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter = new SqlDataAdapter(cmd);
        //        dt = new DataTable();
        //        adapter.Fill(dt);
        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            objType = new labeltype();
        //            objType.labelmasterid = Convert.ToInt32(dr["labelmasterid"]);
        //            objType.labelname = Convert.ToString(dr["labeltype"]);
        //            lstType.Add(objType);
        //        }
        //        return lstType;
        //    }
        //    catch (Exception ex)
        //    {
        //        return lstType;
        //        MessageBox.Show("Error :" + ex.Message.ToString());
        //    }

        //}


        public string getpcbserialno(string serial)
        {
            try
            {
                cmd = new SqlCommand("pro_get_pcbserialno", Barcode_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@serial", serial);
                Barcode_db.Open();
                var reader = cmd.ExecuteScalar();
                Barcode_db.Close();
                return reader.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
                MessageBox.Show("Error : " + ex.Message.ToString());
            }
        }

        public List<string> getcapacity(string stageval, string product_Model)
        {
            var listcapacity = new List<string>();
            try
            {

                cmd = new SqlCommand("pro_get_capacity_details", Essencore_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stage", stageval);
                cmd.Parameters.AddWithValue("@Product_Type", product_Model);
                adapter = new SqlDataAdapter(cmd);
                dt = new DataTable();
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        listcapacity.Add(dr["capacity"].ToString());
                    }
                }
                return listcapacity;
            }
            catch (Exception ex)
            {
                return listcapacity;
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public string GetFilePathdetail(string stage, string capacity)
        {

            try
            {
                cmd = new SqlCommand("pro_getfilepath_k_stages", Essencore_db);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stage", stage);
                cmd.Parameters.AddWithValue("@Fg_Name", capacity);
                Essencore_db.Open();
                var reader = cmd.ExecuteScalar();
                Essencore_db.Close();
                return reader.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
                MessageBox.Show("Error : " + ex.Message.ToString());
            }
        }
    }
}
