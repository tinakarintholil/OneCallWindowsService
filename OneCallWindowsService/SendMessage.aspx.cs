using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using log4net;
using OneCallWindowsService.com.onecallnow.api;
using Tamir.SharpSsh;







namespace OneCallWindowsService
{
    public partial class SendMessage :BaseClass
    {
        #region MyRegion
        protected string CurrentDate;
        public DataSet ds = null;
        public string directoryPath = "";// ConfigurationManager.AppSettings["OneCallDirectory"].ToString();

        public string localfilepath = "";




        string todayFirst = DateTime.Now.ToString("yyyyMMddHHmmss");
        string today = "";
        private static ILog logger = log4net.LogManager.GetLogger("ErrorLog");

        public  OneCallNow_v3 t = null;
        public OCNReturn loginRet = null;
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                

                if (VGlobal.Mode == "DEV")
                {

                    directoryPath = ConfigurationManager.AppSettings["OneCallDirectoryDev"].ToString();
                    localfilepath = ConfigurationManager.AppSettings["localfilepathDev"].ToString();
                }
                else
                {
                    directoryPath = ConfigurationManager.AppSettings["OneCallDirectoryProd"].ToString();
                    localfilepath = ConfigurationManager.AppSettings["localfilepathProd"].ToString();
                }



                lblMessage.Text = "";
                lblResponseMessage.Text = "";


                //Instantiate your OCN service class
                t = new OneCallNow_v3();
                loginRet = t.Login(1, false, 335781, true, "5788");
                var result = t.GetSavedMessages(loginRet.LoginToken);

                if (result.ErrorCode == CommonOCNErrorCode.NoError)
                {
                    if (!Page.IsPostBack)
                    {
                        ddl.DataSource = result.MessageList;
                        ddl.DataValueField = "Code";
                        ddl.DataTextField = "Name";
                        ddl.DataBind();
                        ddl.Items.Insert(0, new ListItem("-Select-", ""));



                        ddlResponse.DataSource = result.MessageList;
                        ddlResponse.DataValueField = "Code";
                        ddlResponse.DataTextField = "Name";
                        ddlResponse.DataBind();
                        ddlResponse.Items.Insert(0, new ListItem("-Select-", ""));

                        

                        
                        

                    }

                }



                WindowsIdentity current = WindowsIdentity.GetCurrent();

                // First find if user is logged in-use HttpContext.Current.User.Identity.Name to get the authenticated username always.
                //HttpContext.Current.Request.LogonUserIdentity.Name-will give the name of user logged in the machine.
                //For anonymous requests Page.User is blank, while Environment.User will be NETWORK SERVICE (or ASPNET on IIS5).
                //For autenticated requests Page.user is not the one that is running the current thread.Its the Environment.User .

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    string username = HttpContext.Current.User.Identity.Name.Substring(HttpContext.Current.User.Identity.Name.IndexOf("\\")+1);
                  
                    // Finds user name and says Hi
                    //   lblWelcome.Text = "Hi, " + (HttpContext.Current.User.Identity.Name) ;
                }
                else
                {
                    //  string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Substring(System.Security.Principal.WindowsIdentity.GetCurrent().Name.IndexOf("\\")+1);
                    // It is anonymous user, say hi to guest


                    //  lblWelcome.Text = "Hi, "+ HttpContext.Current.Request.LogonUserIdentity.Name + "<br/>"+ System.Security.Principal.WindowsIdentity.GetCurrent().Name+ "<br/>" + System.Environment.UserName;
                }


                today = todayFirst.Substring(1);

            }
            catch(Exception ex)
            {
                logger.Debug("*************");
                logger.Error("Exception - \n" + ex.Message);
                logger.Debug("*************");

            }
          

            }

        private  bool IsAvailable(SqlConnection conn)
        {
            try
            {
                conn.Open();
                conn.Close();
            }
            catch (SqlException)
            {
                return false;
            }

            return true;
        }



        public bool WillFlowAcrossNetwork(WindowsIdentity w)
        {
            foreach (SecurityIdentifier s in w.Groups)
            {
                if (s.IsWellKnown(WellKnownSidType.InteractiveSid)) { return true; }
                if (s.IsWellKnown(WellKnownSidType.BatchSid)) { return true; }
                if (s.IsWellKnown(WellKnownSidType.ServiceSid)) { return true; }
            }

            return false;
        }


     

        //1.Create txtfile in onecall folder.
        //2.Move TXTfile created to ftp.
        //3.Move TXTfile to processed folder.
        //   DestinationSettings dest = new DestinationSettings();
        //   dest.DestinationSource = DestinationSource.FileOfDestinations;
        //   dest.FileSettings = new DestinationFileSettings();
        //   byte[] fileAsbytes = ConvertDataSetToByteArray(Ds_CallNow);

        ////   dest.FileSettings.Filename= "PRE335781M7.txt";
        //   dest.FileSettings.DestinationFileContents = fileAsbytes;
        //   var when = new WhenToSendSettings();
        //   when.StartDate = DateTime.Now;
        //   OCNMessageReturn messageReturn = new OCNMessageReturn();


        //   messageReturn=  t.SendASavedMessage(loginRet.LoginToken, "M7",dest, when);
        //   if (messageReturn.ErrorCode==CommonOCNErrorCode.NoError)
        //   {

        //       lblMessage.Text = "File Posting Successfull!!!";
        //   }
       
        protected void btnSubmit_Click(object sender, EventArgs e)
        {

           
            DataSet Ds_CallNow = null;
       
            

            String selectedBranchValue = ddl.SelectedValue.Substring(1);
            String selectedBranchName = ddl.SelectedItem.Text;
       
            int temp = 0;
            int BranchCode = 0;
            if (int.TryParse(selectedBranchValue, out temp))
             BranchCode= temp;
            else
              BranchCode = 0;

            string filename = "PRE335781M"+selectedBranchValue+".txt";
            string Error = "";
            int Filecounttotransfer = 0;
                        

               
                try
                {
                    SqlParameter[] sqlParams = new SqlParameter[]
                    {
                     
                        DBUtility.GetInParameter("@PrimaryDate",txtRouteDate.Text),
                    
                        DBUtility.GetInParameter("Code",BranchCode)

                   };

                    Ds_CallNow = dbU.ExecuteDataSet("CallEmailAheadDownloadOneCallNow", sqlParams);

                }

                catch (Exception ex)
                {
                  BaseClass.SendMail("ONE CALL Error:", "tkarintholil@savatree.com", "mailsend@savatree.com", ex.Message);

                  logger.Debug("*************");
                  logger.Error("Exception - \n" + ex.Message);
                  logger.Debug("*************");
                }
            //   string filenametocheck = "PRE*.txt";
               string filenametocheck = filename;
                bool errorcheck;

                if (!CheckIfFileExists(filenametocheck, directoryPath))
                {
                    if (Ds_CallNow != null && Ds_CallNow.Tables[0].Rows.Count > 0)
                    {
                        Filecounttotransfer = Ds_CallNow.Tables[0].Rows.Count;
                    //  string localfilepath = ConfigurationManager.AppSettings["localfilepath"].ToString() + "/" + filename;
                    //  bool errorcheck = WriteDataToFile(Ds_CallNow.Tables[0], localfilepath);


                       localfilepath = localfilepath + "/" + filename;
                       using (StreamWriter writer = new StreamWriter(localfilepath))
                        {
                        errorcheck = WriteDataTable(Ds_CallNow.Tables[0], writer, true);
                        }
                        if (!errorcheck)
                        {
                          MoveFileToFtp(filenametocheck);
                     
                                       


                        if (BaseClass.transferingbytes > 0)
                        {
                         //Change status of the records 
                            //if(VGlobal.Mode=="PROD")
                            //{
                                //Correct it in db-Check back again
                                UpdateStatusOfRecords(BranchCode, txtRouteDate.Text.Trim());
                          //  }

                            BaseClass.SendMail("ONE CALL Success:", "tkarintholil@savatree.com", "mailsend@savatree.com", "File posted.There were " + Filecounttotransfer.ToString() + "records to Call / Email for  "+ selectedBranchName);
                            lblMessage.Text = "File Posting Successfull!!!.Total Calls/Emails= " + Filecounttotransfer.ToString();

                        }
                        else
                        {

                            BaseClass.SendMail("ONE CALL Error:", "tkarintholil@savatree.com", "mailsend@savatree.com", "File Not posted  .");
                            lblMessage.Text = "File not posted .Please contact the administrator..";

                        }

                    }
                    else
                        {
                           lblMessage.Text = "Error Processing....Redo the process..";
                            
                        }

                        return;
                    }
                    lblMessage.Text = "0 Records To Send To One Call Today For "+ selectedBranchName;
                }
                else
                {

                    DeleteFile(filenametocheck);
                //  string localfilepath = ConfigurationManager.AppSettings["localfilepath"].ToString() + "/" + filename;

                    localfilepath = localfilepath + "/" + filename;
                    if (Ds_CallNow != null && Ds_CallNow.Tables[0].Rows.Count > 0)
                    {

                    Filecounttotransfer = Ds_CallNow.Tables[0].Rows.Count;
                    
                    using (StreamWriter writer = new StreamWriter(localfilepath))
                    {
                        errorcheck = WriteDataTable(Ds_CallNow.Tables[0], writer, true);
                    }

                    if (!errorcheck)
                    {
                        MoveFileToFtp(filenametocheck);

                        if (BaseClass.transferingbytes > 0)
                        {


                            //Change status of the records 
                           // if (VGlobal.Mode == "PROD")
                         //   {
                                //Correct it in db-Check back again-Here the branchcode=8 or 9 for Bedford branch.Need to get the real branchname from look up and update. 
                                UpdateStatusOfRecords(BranchCode, txtRouteDate.Text.Trim());
                         //   }
                            BaseClass.SendMail("ONE CALL Success:", "tkarintholil@savatree.com", "mailsend@savatree.com", "File posted. There were   " + Filecounttotransfer.ToString() + " records to Call/Email.");
                            lblMessage.Text = "File Posting Successfull!!!. Total Calls/Emails= " + Filecounttotransfer.ToString();
                        }
                        else
                        {
                            BaseClass.SendMail("ONE CALL Error:", "tkarintholil@savatree.com", "mailsend@savatree.com", "File Not posted.");
                            lblMessage.Text = "File Not Posted . Please contact the administrator..";

                        }
                    }
                    else
                    {
                        lblMessage.Text = "Error Processing....Redo the process..";

                    }

                    return;

                 }

                    else
                    {

                        lblMessage.Text = "0 Records To Send to One Call Today for "+ selectedBranchName;
                    }


                }
           
           
          
               
           
            

        }

        private void UpdateStatusOfRecords(int BranchCode, string selecteddate)
        {
            DataSet DsUpdate = null;
               try
                {
                    SqlParameter[] sqlParams = new SqlParameter[]
                    {
                        DBUtility.GetInParameter("@Code",BranchCode) ,
                        DBUtility.GetInParameter("@PrimaryDate",selecteddate)

                   };

                var count = dbU.ExecuteNonQuery("OneCallNowSent", sqlParams);
                
                }
            

            catch(Exception ex)
            {

            }
        }

        private void MoveFileToFtp(string filenameToSend)
        {
            string Error = "";
            Sftp oSftp = new Sftp("cancall.onecallnow.com", "ftp$MT1upl", "onecall");
            int _Port = 22;

            oSftp.OnTransferStart += new FileTransferEvent(oSftp_OnTransferStart);
            oSftp.OnTransferEnd += new FileTransferEvent(oSftp_OnTransferEnd);

            try
            {
                oSftp.Connect(_Port);           
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                BaseClass.SendMail("ONE CALL FTP ERROR:", "tkarintholil@savatree.com", "tkarintholil@savatree.com", Error);
                logger.Debug("*************");
                logger.Error("Exception - \n" + ex.Message);
                logger.Debug("*************");

            }

            // var fileList = new DirectoryInfo(directoryPath).GetFiles("PRE*.txt", SearchOption.AllDirectories);
            var fileList = new DirectoryInfo(directoryPath).GetFiles(filenameToSend, SearchOption.AllDirectories);
            foreach (FileInfo aFile in fileList)
            {
                string fullfilelocation = aFile.FullName;
          
                string filenamealone = aFile.Name;

               


             
                    try
                    {
                       

                        oSftp.Put(fullfilelocation, "/" + aFile.Name);
                    }
                    catch(Exception ex)
                    {

                        BaseClass.SendMail("ONE CALL FTP ERROR:", "tkarintholil@savatree.com", "tkarintholil@savatree.com", ex.Message);
                        logger.Debug("*************");
                        logger.Error("Exception - \n" + ex.Message);
                        logger.Debug("*************");
                    }
                     

            

            }

            if (oSftp.Connected)
                oSftp.Close();

                    

        }


        private static void oSftp_OnTransferStart(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
           // BaseClass.SendMail("ONE CALL FTP :", "tkarintholil@savatree.com", "mailsend@savatree.com",  "File Transfer Started...");
           // BaseClass.SendMail("ONE CALL FTP :", "tkarintholil@savatree.com","mailsend@savatree.com",  "Transferred Bytes: " + transferredBytes);
                  
        }

        private static void oSftp_OnTransferEnd(string src, string dst, int transferredBytes, int totalBytes, string message)
        {

        
            BaseClass.SendMail("ONE CALL FTP :", "tkarintholil@savatree.com", "mailsend@savatree.com", "Transferred Bytes: for " +DateTime.Today.ToString()+"--"+ transferredBytes);
       
            BaseClass.transferingbytes = transferredBytes;
           
        }


        //private void MoveFileToProcessedFolder()
        //{
        //    string Error = "";
        //    if (!System.IO.Directory.Exists(targetpath))
        //    {
        //        System.IO.Directory.CreateDirectory(targetpath);
        //    }

        //    try
        //    {
        //        var fileList = new DirectoryInfo(directoryPath).GetFiles("PRE*.txt", SearchOption.AllDirectories);
        //        foreach (FileInfo aFile in fileList)
        //        {

        //            string fullfilelocation = aFile.FullName;
        //            string filenamealone = aFile.Name;

        //            string destinationfile = System.IO.Path.Combine(targetpath, filenamealone);
        //            if (System.IO.File.Exists(destinationfile))
        //                try
        //                {
        //                    System.IO.File.Delete(destinationfile);
        //                }
        //                catch (System.IO.IOException ex)
        //                {
        //                    logger.Error(ex.Message);
        //                }

        //            System.IO.File.Move(fullfilelocation, destinationfile);



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Error = ex.Message;
        //        BaseClass.SendMail("ONE CALL ERROR At MoveFileToProcessedFolder:", "tkarintholil@savatree.com", "mailsend@savatree.com",  Error);
        //        logger.Error(ex.Message);
        //    }

        //}

        private  void DeleteFile(string FilenameToDelete)
        {
            string Error = "";
            string sourceDir = directoryPath;
            try
            {
                //   string[] picList = Directory.GetFiles(sourceDir, "PRE*.txt");
                string[] picList = Directory.GetFiles(sourceDir, FilenameToDelete);

                foreach (string f in picList)
                {
                    File.Delete(f);
                }
            }
            catch(Exception ex)
            {
                Error = ex.Message;
                BaseClass.SendMail("ONE CALL ERROR At DeleteFile:", "tkarintholil@savatree.com", "mailsend@savatree.com",  Error);
                logger.Error(ex.Message);
            }
       }


        public static bool WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            bool errorCheck = false;
            try
            {
                if (includeHeaders)
                {
                    IEnumerable<String> headerValues = sourceTable.Columns
                        .OfType<DataColumn>()
                        .Select(column =>(column.ColumnName));

                    writer.WriteLine(String.Join(",", headerValues));
                }

                IEnumerable<String> items = null;

                foreach (DataRow row in sourceTable.Rows)
                {
                    items = row.ItemArray.Select(o => (o?.ToString() ?? String.Empty));
                    writer.WriteLine(String.Join(",", items));
                }

                writer.Flush();
            }
            catch (System.IO.IOException e)
            {

                errorCheck = false;
                logger.Error(e.Message);


            }
            return errorCheck;
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }

        public static bool WriteDataToFile(DataTable submittedDataTable, string submittedFilePath)
        {
            int i = 0;
            StreamWriter sw = null;
            bool errorCheck=false;
            try
            {
                sw = new StreamWriter(submittedFilePath, false);



                foreach (DataRow row in submittedDataTable.Rows)
                {
                    object[] array = row.ItemArray;

                    for (i = 0; i < array.Length - 1; i++)
                    {
                        sw.Write(array[i].ToString() + ",");
                    }
                    sw.Write(array[i].ToString());
                    sw.WriteLine();

                }

                sw.Flush();
                sw.Close();

            }
            catch(System.IO.IOException e)
            {

                errorCheck = false;
                logger.Error(e.Message);


            }
            return errorCheck;

            
        }





        private static bool CheckIfFileExists(string fileName, string directory)
        {
            var exists = false;
            
            var fileNameToCheck = Path.Combine(directory, fileName);

            if (Directory.Exists(directory))
            {

                //check directory for file

                // if (Directory.EnumerateFiles(directory, "PRE*.txt", SearchOption.TopDirectoryOnly).Any())
                if (Directory.EnumerateFiles(directory, fileName, SearchOption.TopDirectoryOnly).Any())
                { exists = true; }
                    //check subdirectories for file
                    if (!exists)
                    {
                        foreach (var dir in Directory.GetDirectories(directory))
                        {
                            exists = CheckIfFileExists(fileName, dir);

                            if (exists) break;
                        }
                    }
               }
              
               
                return exists;
           
        }

        protected void btnReturn_Click(object sender, EventArgs e)
        {
            string username = "";
           
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                 username = HttpContext.Current.User.Identity.Name;
              
                // Finds user name and says Hi
                //   lblWelcome.Text = "Hi, " + (HttpContext.Current.User.Identity.Name) ;
            }


            try
            {

                bool FileInUse;
                int error = 0;
                MessageDetailReport DetailReportPhone = null;
                MessageDetailReport DetailReportEmail = null;
                MessageDetailReport DetailReportSMS = null;
                string selectedgroup = "";
                string storedProcFileName = "";

                string selectedBranchName = ddlResponse.SelectedItem.Text.Trim();
                string withConfirmOrnot = selectedBranchName.Substring(selectedBranchName.LastIndexOf("-") + 1).Trim();
                if (RadioButton1.Checked)
                {
                    selectedgroup = "Group1";
                    storedProcFileName = "OneCallNowOutcomeGroup1" + withConfirmOrnot;
                }
                else if (RadioButton2.Checked)
                {
                    selectedgroup = "Group2";
                    storedProcFileName = "OneCallNowOutcomeGroup2" + withConfirmOrnot;
                }
                else if (RadioButton3.Checked)
                {
                    selectedgroup = "Group3";
                    storedProcFileName = "OneCallNowOutcomeGroup3" + withConfirmOrnot;
                }
                else if (RadioButton4.Checked)
                {
                    selectedgroup = "Group4";
                    storedProcFileName = "OneCallNowOutcomeGroup4" + withConfirmOrnot;
                }
                else if (RadioButton5.Checked)
                {
                    selectedgroup = "Group5";
                    storedProcFileName = "OneCallNowOutcomeGroup5" + withConfirmOrnot;
                }
                else if (RadioButton6.Checked)
                {
                    selectedgroup = "Group6";
                    storedProcFileName = "OneCallNowOutcomeGroup6" + withConfirmOrnot;
                }
                else if (RadioButton7.Checked)
                {
                    selectedgroup = "Group7";
                    storedProcFileName = "OneCallNowOutcomeGroup7" + withConfirmOrnot;
                }
                else if (RadioButton8.Checked)
                {
                    selectedgroup = "Group8";
                    storedProcFileName = "OneCallNowOutcomeGroup8" + withConfirmOrnot;
                }


                string CSVFilePath = "";
                //responseDevFolderPath value = "C:\apr\Call Download Groups\"/>
                //responseProdFolderPath" value = "\\satd\apr\Call Download Groups\"/>

                try
                {

                    if (HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("LOCALHOST") >= 0)
                    {
                        CSVFilePath = ConfigurationManager.AppSettings["responseDevFolderPath"] + selectedgroup + withConfirmOrnot + "\\onecallresults.csv";



                        if (File.Exists(CSVFilePath))
                        {
                            FileInUse = IsFileInUse(CSVFilePath);
                            if (!FileInUse)
                            {
                                File.Delete(CSVFilePath);

                            }

                            else
                            {
                                lblResponseMessage.Text = "Please close the file-Onecallresult.csv opened and try again";
                                return;

                            }


                        }





                    }
                    else if (HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("SATDB1") >= 0)
                    {
                        CSVFilePath = ConfigurationManager.AppSettings["responseSatdb1FolderPath"] + selectedgroup + withConfirmOrnot + "\\onecallresults.csv";
                        if (File.Exists(CSVFilePath))
                        {

                            FileInUse = IsFileInUse(CSVFilePath);
                            if (!FileInUse)
                            {
                                File.Delete(CSVFilePath);

                            }

                            else
                            {
                                lblResponseMessage.Text = "Please close the file-Onecallresult.csv opened and try again";
                                return;

                            }
                        }
                    }
                    else
                    {

                        CSVFilePath = ConfigurationManager.AppSettings["responseProdFolderPath"] + selectedgroup + withConfirmOrnot + "\\onecallresults.csv";
                        if (File.Exists(CSVFilePath))
                        {

                            FileInUse = IsFileInUse(CSVFilePath);
                            if (!FileInUse)
                            {
                                File.Delete(CSVFilePath);

                            }

                            else
                            {
                                lblResponseMessage.Text = "Please close the file-Onecallresult.csv opened and try again";
                                return;

                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    logger.Debug("*************");
                    logger.Error("Exception - \n" + ex.Message);
                    logger.Debug("*************");

                }



                int groupId;
                try
                {
                    if (loginRet.ErrorCode == CommonOCNErrorCode.NoError)
                    {
                        OCNReportReturn reportReturn = null;
                        OCNReportReturn reportReturnEmail = null;
                        OCNReportReturn reportReturnPhone = null;
                        OCNReportReturn reportReturnSMS = null;


                        MessageSearchMethod msg = new MessageSearchMethod();


                        reportReturn = t.SearchMessageReports(loginRet.LoginToken, 0, false, MessageSearchMethod.Today, true, null, false, null, false, "", null);
                        //  reportReturn = t.SearchMessageReports(loginRet.LoginToken, 0, false, Enum.GetNames(typeof(MessageSearchMethod)).Equals(ddlFilterdayType.SelectedItem.Text), true, null, false, null, false, "", null);
                        if (reportReturn.SummaryReport != null)
                        {



                            MessageReportGroup[] msggrp = reportReturn.SummaryReport.ReportGroups;
                            List<int> grpId = new List<int>();
                            for (int i = 0; i < msggrp.Length; i++)
                            {

                                if (msggrp[i].MessageName.Contains(selectedBranchName))
                                {


                                    groupId = msggrp[i].MessageGroupID;

                                    grpId.Add(groupId);
                                }


                            }

                            if (grpId != null & grpId.Count > 0)
                            {

                                MessageType msgTypePhone = (MessageType)Enum.Parse(typeof(MessageType), MessageType.Phone.ToString());
                                MessageType msgTypeEmail = (MessageType)Enum.Parse(typeof(MessageType), MessageType.Email.ToString());
                                MessageType msgTypeSMS = (MessageType)Enum.Parse(typeof(MessageType), MessageType.SMS.ToString());
                                MessageReportFilterType filterType = (MessageReportFilterType)Enum.Parse(typeof(MessageReportFilterType), MessageReportFilterType.Name.ToString());
                                List<Delivery> deliveries = new List<Delivery>();
                                Paging paging = new Paging();
                                paging.Index = 1;
                                paging.Size = 200;
                                paging.ItemCount = 500;
                                paging.ItemCountSpecified = true;
                                paging.IndexSpecified = true;
                                paging.SizeSpecified = true;

                                List<ResponseFrom> responses = new List<ResponseFrom>();


                                for (int j = 0; j < grpId.Count; j++)

                                {
                                    reportReturnPhone = t.RetrieveMessageReportDetails(loginRet.LoginToken, grpId[j], true, msgTypePhone, true, filterType, true, "", paging);
                                    reportReturnEmail = t.RetrieveMessageReportDetails(loginRet.LoginToken, grpId[j], true, msgTypeEmail, true, filterType, true, "", paging);
                                    reportReturnSMS = t.RetrieveMessageReportDetails(loginRet.LoginToken, grpId[j], true, msgTypeSMS, true, filterType, true, "", paging);






                                    DetailReportPhone = reportReturnPhone.DetailReport;
                                    DetailReportEmail = reportReturnEmail.DetailReport;
                                    DetailReportSMS = reportReturnSMS.DetailReport;



                                    Delivery[] deliveryPhone = null;
                                    Delivery[] deliveryEmail = null;
                                    Delivery[] deliverySMS = null;

                                    deliveryPhone = DetailReportPhone.Deliveries;
                                    deliveryEmail = DetailReportEmail.Deliveries;
                                    deliverySMS = DetailReportSMS.Deliveries;

                                    //int lengthdeliveryPhone = deliveryPhone.Length;
                                    //int lengthdeliveryEmail = deliveryEmail.Length;
                                    //int lengthdeliverySMS = deliverySMS.Length;

                                    for (int k = 0; k < deliveryPhone.Length; k++)
                                    {
                                        ResponseFrom resp = new ResponseFrom();


                                        resp.name = deliveryPhone[k].Name;
                                        resp.ExternalID = deliveryPhone[k].ExternalID;
                                        resp.Destination = deliveryPhone[k].Destination;
                                        resp.CountryCode = deliveryPhone[k].CountryCode;
                                        resp.Description = deliveryPhone[k].Disposition;
                                        resp.Status = deliveryPhone[k].DeliveryStatus + " " + deliveryPhone[k].Disposition;
                                        resp.Response = deliveryPhone[k].PollResponse;
                                        resp.Duration = deliveryPhone[k].Duration;
                                        resp.Attempts = deliveryPhone[k].Attempts;
                                        resp.CreditsUsed = deliveryPhone[k].CallCreditsUsed;
                                        resp.FirstAttempt = deliveryPhone[k].FirstAttemptTime;
                                        resp.DeliveryTime = deliveryPhone[k].DeliveryTime;



                                        responses.Add(resp);
                                    }

                                    for (int l = 0; l < deliveryEmail.Length; l++)
                                    {
                                        ResponseFrom resp1 = new ResponseFrom();
                                        resp1.name = deliveryEmail[l].Name;
                                        resp1.ExternalID = deliveryEmail[l].ExternalID;
                                        resp1.Destination = deliveryEmail[l].Destination;
                                        resp1.CountryCode = deliveryEmail[l].CountryCode;
                                        resp1.Description = deliveryEmail[l].Disposition;
                                        resp1.Status = deliveryEmail[l].DeliveryStatus + " " + deliveryEmail[l].Disposition;
                                        resp1.Response = deliveryEmail[l].PollResponse;
                                        resp1.Duration = deliveryEmail[l].Duration;
                                        resp1.Attempts = deliveryEmail[l].Attempts;
                                        resp1.CreditsUsed = deliveryEmail[l].CallCreditsUsed;
                                        resp1.FirstAttempt = deliveryEmail[l].FirstAttemptTime;
                                        resp1.DeliveryTime = deliveryEmail[l].DeliveryTime;


                                        responses.Add(resp1);
                                    }

                                    for (int m = 0; m < deliverySMS.Length; m++)
                                    {
                                        ResponseFrom resp2 = new ResponseFrom();
                                        resp2.name = deliverySMS[m].Name;
                                        resp2.ExternalID = deliverySMS[m].ExternalID;
                                        resp2.Destination = deliverySMS[m].Destination;
                                        resp2.CountryCode = deliverySMS[m].CountryCode;
                                        resp2.Description = deliverySMS[m].Disposition;
                                        resp2.Status = deliverySMS[m].DeliveryStatus + " " + deliverySMS[m].Disposition;
                                        resp2.Response = deliverySMS[m].PollResponse;
                                        resp2.Duration = deliverySMS[m].Duration;
                                        resp2.Attempts = deliverySMS[m].Attempts;
                                        resp2.CreditsUsed = deliverySMS[m].CallCreditsUsed;
                                        resp2.FirstAttempt = deliverySMS[m].FirstAttemptTime;
                                        resp2.DeliveryTime = deliverySMS[m].DeliveryTime;

                                        responses.Add(resp2);
                                    }

                                }



                                //Creating csv file in the path based on group selected.


                                WriteCSV(responses, CSVFilePath);

                                if (HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToUpper().IndexOf("LOCALHOST") >= 0)
                                {
                                    //MOVE THE FILE TO SATDB1:Stored proc is looking in the path: fIRST DELETE tHE FILE IN SATDB1 LOCATION AND THE COPY FILE.
                                    string CSVFilePathOnSatdb1 = ConfigurationManager.AppSettings["responseSatdb1FolderPath"] + selectedgroup + withConfirmOrnot + "\\onecallresults.csv";
                                    if (File.Exists(CSVFilePathOnSatdb1))
                                        File.Delete(CSVFilePathOnSatdb1);
                                    File.Copy(CSVFilePath, CSVFilePathOnSatdb1);



                                }



                                try
                                {

                                    dbU.ExecuteDataSet(storedProcFileName);


                                }
                                catch (Exception ex)
                                {
                                    error = error + 1;
                                    logger.Debug("*************");
                                    logger.Error("Exception - \n" + ex.Message);
                                    logger.Debug("*************");

                                }
                                if (error > 0)
                                {
                                    lblResponseMessage.Text = "Response File Generated For " + selectedBranchName + ". But Prodeo not updated.The user is "+ username ;
                                }
                                else
                                {
                                    lblResponseMessage.Text = "Response File Generated For " + selectedBranchName;
                                }


                            }


                            else
                            {

                                lblResponseMessage.Text = "No Response File for " + selectedBranchName;
                                return;
                            }



                        }



                    }





                }


                catch (Exception ex)
                {

                    logger.Debug("*************");
                    logger.Error("Exception - \n" + ex.Message);
                    logger.Debug("*************");

                }
                finally
                {


                }
            }

            catch(Exception ex)
            {
                logger.Debug("*************");
                logger.Error("Exception - \n" + ex.Message);
                logger.Debug("*************");

            }
              

            }

      public static bool IsFileInUse(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    
                 }
                return false;
            }
            catch (IOException ex)
            {
                logger.Debug("*************");
                logger.Error("Exception - \n" + ex.Message);
                logger.Debug("*************");
                return true;
            }
        }





        public void WriteCSV<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(",", props.Select(p => p.GetValue(item, null))));
                }
            }
        }



        
    }

    // Name, [External ID], Destination, [Country Code], Description, Status, Response, Duration, Attempts, [Credits Used], [First Attempt], [Delivery Time]
    public class ResponseFrom
    {
        public string name { get; set; } //name
        public string ExternalID { get; set; }//[External ID]
        public string Destination { get; set; }//Destination
        public string CountryCode { get; set; }//[Country Code]
        public string Description { get; set; }
        public string Status { get; set; }//Status
        public string Response { get; set; } //Response
        public int Duration { get; set; }//Duration
        public int Attempts { get; set; }// Attempts
        public float CreditsUsed { get; set; }//[Credits Used]
        public string FirstAttempt { get; set; }//[First Attempt]
        public string DeliveryTime { get; set; } //[Delivery Time]

        

    }
}