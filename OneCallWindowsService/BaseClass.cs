using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Collections;
using System.Web.SessionState;
using System.Web.Configuration;
using System.Web.Security;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Net.Mail;
using System.Web.UI;


namespace OneCallWindowsService { 
    public class BaseClass: System.Web.UI.Page
    {
        public int LoginAccountID;
        public string CanViewReporting;
        public string OrganizationName;
        public static DBUtility dbU;
        public string[] UserRoles;
        public DataSet UserRolesDesc;
        public Hashtable AppSettingsCollection = new Hashtable();
        public string Instance = "";

        public string UserDB = "";

        public static string Mode = ConfigurationManager.AppSettings["Mode"].ToString().ToUpper();

        public static string ftpusername = ConfigurationManager.AppSettings["onecallFTPUsername"].ToString().ToUpper();

        public static int transferingbytes;

        public BaseClass()
        {

            if (HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("LOCALHOST") >= 0)
            {
                UserDB = "conStr_ProdeoBeta";
                Instance = "LOCALHOST";
            }
            else if (HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("SATDB1") >= 0)
            {
                UserDB = "conStr_ProdeoBeta";
                Instance = "DEV";
            }
            else //if (HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("SATAPP1")  >= 0 or HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("SATAPP2")>=0)
            {
                UserDB = "conStr_Prodeo";
                Instance = "PROD";
            }
                       
           
            dbU = new DBUtility(UserDB, DBUtility.ConnectionStringType.Configured);

           

        }



        public void CreateCSVFile(DataSet DS)
        {
            DataTable dt;
            if (DS != null && DS.Tables[0] != null)
            {
                dt = DS.Tables[0];
                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in dt.Rows)
                {
                   
                    IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                    sb.AppendLine(string.Join(",", fields));

                }

                byte[] data = new ASCIIEncoding().GetBytes(sb.ToString());

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ContentType = "APPLICATION/OCTET-STREAM";
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + "Export" + DateTime.Now.ToString("d").Replace("\\", "").Replace("/", "") + ".csv");
                HttpContext.Current.Response.OutputStream.Write(data, 0, data.Length);

                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.Close();
            }
        }

        public void ImportDataFromExcel(string excelfileWithpath, string ssqltable, string table)
        {
            string myexceldataquery = "select * from [" + table + "$]";

            try
            {
                //create our connection strings
                string sexcelconnectionstring = @"provider=microsoft.jet.oledb.4.0;data source=" + excelfileWithpath +
            ";extended properties=" + "\"excel 8.0;hdr=yes;\"";
                string ssqlconnectionstring = dbU.ConnectionString;
                //Erase any previous data from destination table


                //SqlParameter[] sqlParams_ = new SqlParameter[]
                //        {                   
                //           DBUtility.GetInParameter("@TableName", ssqltable) ,
                //        };
                string sclearsql = "delete from " + ssqltable;
                dbU.ExecuteNonQuery(sclearsql);

                // bulk copy data from the excel file into sql table
                OleDbConnection oledbconn = new OleDbConnection(sexcelconnectionstring);
                OleDbCommand oledbcmd = new OleDbCommand(myexceldataquery, oledbconn);
                oledbconn.Open();
                OleDbDataReader dr = oledbcmd.ExecuteReader();
                SqlBulkCopy bulkcopy = new SqlBulkCopy(ssqlconnectionstring);
                bulkcopy.DestinationTableName = ssqltable;
                while (dr.Read())
                {
                    bulkcopy.WriteToServer(dr);
                }

                oledbconn.Close();
            }
            catch (Exception ex)
            {
                //handle exception
            }
        }

        public static void UploadFileToFTP(string ServerFile, string LocalFile)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        public static void SendMail(string Subject, string ToAddresses, string FromAddress, string Body)
        {

            try
            {
              //  if (VGlobal.Mode == "PROD")
                {
                    Body += " This is an automated message.Please do not reply.";
                    MailMessage mailObj = new MailMessage(FromAddress, ToAddresses, Subject, Body);

                    SmtpClient SMTPServer = new SmtpClient(ConfigurationManager.AppSettings["SMTP"],25);
                    SMTPServer.Send(mailObj);
                }
            }
            catch { }

        }
    }

    public static class WebControlExtender
    {
        public static Control FindControlRecursive(this Control root, string id)
        {
            if (root.ID == id)
            {
                return root;
            }

            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }
    }
}