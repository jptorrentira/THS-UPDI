using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShuttleService.Data;
using ShuttleService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using ReportService;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Collections.Specialized;
using System.Text;

using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using Newtonsoft.Json;


using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace ShuttleService.Controllers
{
    public class ReportsController : Controller
    {
        public string _dbName = "";
        public string _dbServer = "";

        // public string _dbName = "ShuttleReservationDB";//test
        // public string _dbServer = "192.168.70.102";//test
        // public string _dbName = "ShuttleReservationDB";
        // public string _dbServer = "192.168.70.102";
        //public string _dbName = "THS_TEST2";//calaca dev
        //public string _dbServer = "192.168.30.156";//calaca dev

        //public string _dbName = "TransportHubDB";//calaca live
        //public string _dbServer = "192.168.30.156";//calaca dev
        //private readonly ApplicationDbContext _context;

        public ReportsController()
        {
            //to set the dbserver
            MailMessage mail = new MailMessage();
            if (System.Environment.MachineName == "ANDROMEDA" )
            {
                _dbName = "TransportHubDB";//calaca live
               _dbServer = "192.168.30.156";//calaca live
            }
            else if ( System.Environment.MachineName == "PEGASUS")
            {
                _dbName = "TransportHubDBCopyOfLive";//calaca dev
               _dbServer = "192.168.30.156";//calaca dev
            }
            //else if (System.Environment.MachineName == "SODIUM2" || System.Environment.MachineName == "WSD2095") //JPT commented code 
            else if (System.Environment.MachineName == "SODIUM2" || System.Environment.MachineName == "WSN1263") //JPT additional code
            {
                _dbName = "ShuttleReservationDB_UPDI"; // UPDI Test
                _dbServer = "192.168.70.231"; // UPDI Test

            }
            else
            {
                if (System.Environment.MachineName == "BRAN")
                {
                    _dbName = "ShuttleReservationDB_LiveCopy";//test
                    _dbServer = "192.168.70.102";//test
                }
                else
                {
                    _dbName = "ShuttleReservationDB_UPDI"; //UPDI Live
                    _dbServer = "192.168.70.102"; //UPDI Live
                }
                
            }
            //end
        }




        //private void ResetContextState() => _context.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);

        public async Task<IActionResult> Index()
        {
            return View();
        }




        public async Task<IActionResult> Manifest(string t)
        {
            //var webRoot = _env.WebRootPath;
            ReportService.Report _report = new Report
            {
                FileName = "ShuttleManifest.rdl",
                FolderName = "SSRS",
                //Default Directory = \\192.168.70.165\inetpub\wwwroot\jrpt\Report
                //Directory = webRoot + @"\Report\"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };

            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //FOR SQL QUERY
            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "Trips",
                SQLQuery = "SELECT * FROM Trip " +
                           "   LEFT OUTER JOIN VehicleLists " +
                           "     ON Trip.VehicleListId = VehicleLists.Id " +
                           "   INNER JOIN Drivers " +
                           "     ON Trip.DriverId = Drivers.Id where Trip.TripControlNo = '" + t + "' "
            };
            reportDataSets.Add(SQLQueryDataset);

            //FOR SQL QUERY
            ReportService.ReportDataSet SQLQueryDataset1 = new ReportDataSet
            {
                DataSetName = "ShuttlePassengers",
                SQLQuery = "SELECT ROW_NUMBER() OVER(ORDER BY ShuttlePassengers.FirstName ASC) as [RowNum],ShuttlePassengers.FirstName,ShuttlePassengers.LastName,ShuttlePassengers.Remarks AS [ShuttlePassengers Remarks],ShuttlePassengers.TripTypeId,Trip.TripControlNo," +
                            "ShuttlePassengers.ShuttleId,ShuttlePassengers.ReservedDatetime,ShuttlePassengers.ContactNo " +
                            " FROM " +
                             " ShuttlePassengers " +
                             " LEFT OUTER JOIN Trip " +
                             "   ON ShuttlePassengers.ShuttleId = Trip.Id where ShuttlePassengers.TripTypeId in (1,2) and Trip.TripControlNo = '" + t + "' "
            };
            reportDataSets.Add(SQLQueryDataset1);

            //FOR SQL QUERY
            ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            {
                DataSetName = "ShuttlePassengersPM",
                SQLQuery = "SELECT ROW_NUMBER() OVER(ORDER BY ShuttlePassengers.FirstName ASC) as [RowNum],ShuttlePassengers.FirstName,ShuttlePassengers.LastName,ShuttlePassengers.Remarks AS [ShuttlePassengers Remarks],ShuttlePassengers.TripTypeId,Trip.TripControlNo," +
                            "ShuttlePassengers.ShuttleId,ShuttlePassengers.ReservedDatetime,ShuttlePassengers.ContactNo " +
                            " FROM " +
                             " ShuttlePassengers " +
                             " LEFT OUTER JOIN Trip " +
                             "   ON ShuttlePassengers.ShuttleId = Trip.Id where ShuttlePassengers.TripTypeId in (1,3) and Trip.TripControlNo = '" + t + "' "
            };
            reportDataSets.Add(SQLQueryDataset2);



            ////FOR STORED PROCEDURE
            //ReportService.ReportDataSet _dataset = new ReportDataSet
            //{
            //    DataSetName = "DataSet1",
            //    StoredProcedureCommandType = ReportService.CommandType.Text,
            //    StoredProcedureCommandText = "spUserList"

            //};
            //_dataset.StoredProcedureParameters = new StoredProcedureParameter[1];

            //StoredProcedureParameter paramData = new StoredProcedureParameter
            //{
            //    SpPramName = "@Id",
            //    SpPramDataType = DbType.String,
            //    SpPramValue = "8c2357bd-1c24-46bc-a492-d531e9315874"
            //};
            //_dataset.StoredProcedureParameters[0] = paramData;

            //reportDataSets.Add(_dataset);

            //PDF
            var client = new ReportService.Service1Client();
            byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
            return File(bytes, "application/pdf");

            //EXCEL
            //var client = new ReportService.Service1Client();
            //byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(),_report,database);
            //return File(bytes, "application/vnd.ms-excel",  "Filename.xls");
        }




        public async Task<IActionResult> ReportForGS(string t, string Name, string Email, string SenderEmail, string Message, string Subject, string Method)
        {
            //var webRoot = _env.WebRootPath;
            ReportService.Report _report = new Report
            {
                FileName = "GSReport.rdl",
                FolderName = "SSRS",
                //Default Directory = \\192.168.70.165\inetpub\wwwroot\jrpt\Report
                //Directory = webRoot + @"\Report\"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };

            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //FOR SQL QUERY
            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "Trips",
                SQLQuery = "SELECT * FROM Trip " +
                           "   LEFT OUTER JOIN VehicleLists " +
                           "     ON Trip.VehicleListId = VehicleLists.Id " +
                           "   INNER JOIN Drivers " +
                           "     ON Trip.DriverId = Drivers.Id where Trip.TripControlNo = '" + t + "' "
            };
            reportDataSets.Add(SQLQueryDataset);

            //FOR SQL QUERY
            ReportService.ReportDataSet SQLQueryDataset1 = new ReportDataSet
            {
                DataSetName = "ShuttlePassengers",
                SQLQuery = "SELECT ROW_NUMBER() OVER(ORDER BY ShuttlePassengers.FirstName ASC) as [RowNum],ShuttlePassengers.*,ChargingCompanys.*,Trip.TripControlNo  " +
                        " FROM " +
                        " ShuttlePassengers "+
                        " INNER JOIN ChargingCompanys "+
                        "  ON ShuttlePassengers.ChargingCompanyId = ChargingCompanys.Id "+
                        "   INNER JOIN Trip "+
                        "  ON ShuttlePassengers.ShuttleId = Trip.Id " +
                        "  where ShuttlePassengers.TripTypeId in (1,2) and Trip.TripControlNo = '" + t + "' "
            };
            reportDataSets.Add(SQLQueryDataset1);

            ////FOR SQL QUERY
            ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            {
                DataSetName = "ShuttlePassengersPM",
                SQLQuery = "SELECT ROW_NUMBER() OVER(ORDER BY ShuttlePassengers.FirstName ASC) as [RowNum],ShuttlePassengers.*,ChargingCompanys.*,Trip.TripControlNo  " +
                        " FROM " +
                        " ShuttlePassengers " +
                        " INNER JOIN ChargingCompanys " +
                        "  ON ShuttlePassengers.ChargingCompanyId = ChargingCompanys.Id " +
                        "   INNER JOIN Trip " +
                        "  ON ShuttlePassengers.ShuttleId = Trip.Id " +
                        "  where ShuttlePassengers.TripTypeId in (1,3) and Trip.TripControlNo = '" + t + "' "
            };
            reportDataSets.Add(SQLQueryDataset2);



            ////FOR STORED PROCEDURE
            //ReportService.ReportDataSet _dataset = new ReportDataSet
            //{
            //    DataSetName = "DataSet1",
            //    StoredProcedureCommandType = ReportService.CommandType.Text,
            //    StoredProcedureCommandText = "spUserList"

            //};
            //_dataset.StoredProcedureParameters = new StoredProcedureParameter[1];

            //StoredProcedureParameter paramData = new StoredProcedureParameter
            //{
            //    SpPramName = "@Id",
            //    SpPramDataType = DbType.String,
            //    SpPramValue = "8c2357bd-1c24-46bc-a492-d531e9315874"
            //};
            //_dataset.StoredProcedureParameters[0] = paramData;

            //reportDataSets.Add(_dataset);

            //PDF
            var client = new ReportService.Service1Client();
            byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);

            if(Method == "email") {
                var EmailSend = SendEmailWithAttachment(Name, Email, SenderEmail, Message, Subject, bytes);
                return Ok(EmailSend);
            }
            else
            {
                return File(bytes, "application/pdf");
            }

            //return File(bytes, "application/pdf");

            //EXCEL
            //var client = new ReportService.Service1Client();
            //byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(),_report,database);
            //return File(bytes, "application/vnd.ms-excel",  "Filename.xls");


        }


        //public string SendEmail(string Name, string Email, string Message, string Subject)
        //{

        //    try
        //    {
        //        // Credentials
        //        var credentials = new NetworkCredential("smcdacon\\helpdeskadmin", "Str@wb3rry##");
        //        // Mail message
        //        var mail = new MailMessage()
        //        {
        //            From = new MailAddress("webhelpdeskadmin@semirarampc.com"),
        //            Subject = Subject,
        //            Body = Message
        //        };
        //        mail.IsBodyHtml = true;
        //        mail.To.Add(new MailAddress(Email));
        //        // Smtp client
        //        var client = new SmtpClient()
        //        {
        //            Port = 587,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Host = "mail.hoaccess.com",
        //            EnableSsl = true,
        //            Credentials = credentials
        //        };
        //        client.Send(mail);
        //        return "Email Sent Successfully!";
        //    }
        //    catch (System.Exception e)
        //    {
        //        return e.Message;
        //    }

        //}

        //public string SendEmail(string Name, string Email, string Message, string Subject)
        //{

        //    try
        //    {
        //        MailMessage mail = new MailMessage();
        //        SmtpClient SmtpServer = new SmtpClient("mail.hoaccess.com");
        //        mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com", "SHUTTLE SERVICE RESERVATION SYSTEM");

        //        //var getEmails = _context.EmailRecipients.Where(e=> e.Status == 1 && e.Group == "");


        //        mail.To.Add(Email);

        //        string senderMail = _context.Employees.FirstOrDefault(e => e.Id.Equals(Convert.ToInt32(HttpContext.Session.GetString("Session_employeeId")))).CompanyEmail;
        //        if (IsValidEmail(senderMail))
        //        {
        //            mail.CC.Add(senderMail);
        //        }

        //        mail.Subject = Subject;
        //        mail.Body = Message;



        //        mail.IsBodyHtml = true;
        //        SmtpServer.Send(mail);
                
        //        return "1";
        //    }
        //    catch (System.Exception e)
        //    {
        //        return e.Message;
        //    }

        //}


        public string SendEmailWithAttachment(string Name, string Email, string SenderEmail, string Message, string Subject, byte[] _byte)
        {

            try
            {
                Attachment att = new Attachment(new MemoryStream(_byte), Name + ".pdf");
                //MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer;
                var smtp = new SmtpClient();// SmtpServer;
                MailMessage mail = new MailMessage();
                if (System.Environment.MachineName == "ANDROMEDA" || System.Environment.MachineName == "PEGASUS")
                {
                    smtp.Host = "mail.cpcaccess.com";
                    smtp.Credentials = new System.Net.NetworkCredential("webhelpdeskadmin@semcalaca.com", "System@1");
                    smtp.Port = 587;
                    smtp.EnableSsl = false;
                    //SmtpServer = new SmtpClient("");
                    mail.From = new MailAddress("webhelpdeskadmin@semcalaca.com", "Transport Hub System");
                }
                else if (System.Environment.MachineName == "ANDROMEDA" || System.Environment.MachineName == "PEGASUS")
                {
                    smtp.Host = "mail.smpcaccess.com";
                    //SmtpServer = new SmtpClient("mail.hoaccess.com");
                    smtp.Credentials = new System.Net.NetworkCredential("semiraramining\\minesitehelpdesk", "P@ss~w0rd2025");
                    smtp.Port = 587;
                    smtp.EnableSsl = false;
                    mail.From = new MailAddress("minesitehelpdesk@semirarampcsite.com", "Transport Hub System");
                }
                else
                {
                    smtp.Host = "relay.smcdacon.com";
                    //smtp.Host = "mail.hoaccess.com";
                    //SmtpServer = new SmtpClient("mail.hoaccess.com");
                    smtp.Credentials = new System.Net.NetworkCredential("smcdacon\\webhelpdeskadmin", "Str@wb3rry##");
                    smtp.Port = 25;
                    smtp.EnableSsl = false;
                    mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com", "Transport Hub System");
                }

                string[] tempEmails = Email.Split(',');

                foreach (string email in tempEmails) {
                    if (IsValidEmail(email))
                    {
                        mail.To.Add(email);
                    }
                }


                if (IsValidEmail(SenderEmail))
                {
                    mail.CC.Add(SenderEmail);
                }

                mail.CC.Add("lbdatuin@semirarampc.com");
                mail.CC.Add("jfsalvador@semirarampc.com");


                mail.Subject = Subject;
                mail.Body = Message;
                mail.Attachments.Add(att);



                mail.IsBodyHtml = true;
                smtp.Send(mail);

                return "1";
            }
            catch (System.Exception e)

            {
                //logs

                //ResetContextState();
                //Log _logs2 = new Log();
                //_logs2.Process = "EMAIL ERROR: " + e.Message;
                //_logs2.ProcessedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                //_logs2.ProcessedDate = DateTime.Now;
                //_context.Add(_logs2);
                //_context.SaveChanges();
                //end logs

                return e.Message;
            }

        }

        public string SendEmail(string Name, string Email, string SenderEmail, string Message, string Subject)
        {

            try
            {
                var smtp = new SmtpClient();// SmtpServer;
                MailMessage mail = new MailMessage();
                if (System.Environment.MachineName == "ANDROMEDA" || System.Environment.MachineName == "PEGASUS")
                {
                    smtp.Host = "mail.cpcaccess.com";
                    smtp.Credentials = new System.Net.NetworkCredential("webhelpdeskadmin@semcalaca.com", "System@1");
                    smtp.Port = 587;
                    smtp.EnableSsl = false;
                    //SmtpServer = new SmtpClient("");
                    mail.From = new MailAddress("webhelpdeskadmin@semcalaca.com", "Transport Hub System");
                }
                else if (System.Environment.MachineName == "CRONUS" || System.Environment.MachineName == "HYACINTHUSX")
                {
                    smtp.Host = "mail.smpcaccess.com";
                    //SmtpServer = new SmtpClient("mail.hoaccess.com");
                    smtp.Credentials = new System.Net.NetworkCredential("semiraramining\\minesitehelpdesk", "P@ss~w0rd2025");
                    smtp.Port = 587;
                    smtp.EnableSsl = false;
                    mail.From = new MailAddress("minesitehelpdesk@semirarampcsite.com", "Transport Hub System");
                }
                else
                {
                    //smtp.Host = "relay.smcdacon.com";
                    ////SmtpServer = new SmtpClient("mail.hoaccess.com");
                    //smtp.Credentials = new System.Net.NetworkCredential("smcdacon\\webhelpdeskadmin", "Str@wb3rry##");
                    //smtp.Port = 25;
                    //smtp.EnableSsl = false;
                    //mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com", "Transport Hub System");

                    smtp.Host = "relay.smcdacon.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = false;
                    mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com", "Transport Hub System");
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = true;
                    smtp.Timeout = 30000;

                }

                string[] tempEmails = Email.Split(',');

                foreach (string email in tempEmails)
                {
                    if (IsValidEmail(email))
                    {
                        mail.To.Add(email);
                    }
                }

                if (IsValidEmail(SenderEmail))
                {
                    mail.CC.Add(SenderEmail);
                }

                mail.CC.Add("lbdatuin@semirarampc.com");
                mail.CC.Add("jfsalvador@semirarampc.com");

                mail.Subject = Subject;
                mail.Body = Message;
                mail.IsBodyHtml = true;
                smtp.Send(mail);

                return "1";
            }
            catch (System.Exception e)

            {
                //logs

                //ResetContextState();
                //Log _logs2 = new Log();
                //_logs2.Process = "EMAIL ERROR: " + e.Message;
                //_logs2.ProcessedBy = HttpContext.Session.GetString("Session_userDomainWithName");
                //_logs2.ProcessedDate = DateTime.Now;
                //_context.Add(_logs2);
                //_context.SaveChanges();
                //end logs

                return e.Message;
            }

        }

        public async Task<bool> SendEmailAsync(string Name, string Email, string SenderEmail, string Message, string Subject)
        {
            //("", _Email, _senderEmail, _message, "Shuttle Service");
            try
            {
                //string systemLink = "<a href='http://sodium2/SMPCHousing'>SMPC Housing System</a>";

                using (var mail = new MailMessage())
                {


                    mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com");



                    // ✅ Add CC (user email if provided)
                    if (!string.IsNullOrWhiteSpace(Email))
                        mail.CC.Add(Email);

                    // ✅ Add multiple recipients properly
                    mail.CC.Add("lbdatuin@semirarampc.com");
                    mail.CC.Add("jfsalvador@semirarampc.com");

                    // ✅ Email subject and body
                    mail.Subject = Subject;
                    mail.Body = Message;
                    mail.IsBodyHtml = true;

                    // ✅ SMTP configuration (per company if needed)
                    //string smtpServer = "mail.hoaccess.com";
                    string smtpServer = "relay.smcdacon.com";
                    int smtpPort = 25;
                    bool enableSsl = false;
                    string smtpUser = "webhelpdeskadmin@semirarampc.com";
                    string smtpPass = "Str@wb3rry##"; // ⚠️ Move this to appsettings or config file



                    using (var smtp = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                        smtp.EnableSsl = enableSsl;

                        // ✅ Send asynchronously
                        await smtp.SendMailAsync(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Optionally log exception here
                // e.g. _logger.LogError(ex, "Failed to send SysOwner email for {name}", name);
                return false;
            }
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }



        public string CallSMSAPIOld(string MobileNo, string Message, string ReferenceNo)
        {
            //return "200";

            if (MobileNo == "" || MobileNo == null) // added by ebe in case that the requestor has no mobile number 05-30-2020
            {
                return "200";
            }

            //06092020
         
                if (MobileNo.Length == 11 && MobileNo.Substring(0, 2) == "09")
                {
                
                }
                 else { return "200"; }
            //end 06092020

            string url = "https://192.168.70.74/smsapi/api/sms/ssrs";

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["MobileNo"] = MobileNo;
                data["Message"] = Message;
                data["ReferenceNo"] = ReferenceNo;


                var response = wb.UploadValues(url, "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                return responseInString;
            }

        }

        public async Task<string> CallSMSAPI(string MobileNo, string Message, string ReferenceNo)
        {
            if (System.Environment.MachineName == "SODIUM2" || System.Environment.MachineName == "WSN1263")
            {
                Message = "***THIS IS A TEST. PLEASE IGNORE.***" + Environment.NewLine + Message;
            }

            if (ReferenceNo == "TRIP")
            {
                if(System.Environment.MachineName == "ANDROMEDA" || System.Environment.MachineName == "PEGASUS")
                {
                    ReferenceNo = ReferenceNo + "TRIP-C";
                }

                Message = Message + Environment.NewLine + Environment.NewLine +  "This is system-generated text. Please do not reply"  ;
            }

           
            if (MobileNo == "" || MobileNo == null) // added by ebe in case that the requestor has no mobile number 05-30-2020
            {
                return "200";
            }

            //06092020

            if (MobileNo.Length == 11 && MobileNo.Substring(0, 2) == "09")
            {

            }
            else { return "200"; }
            //end 06092020

            string url = "https://192.168.70.74/smsapi/api/sms/ssrs";

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["MobileNo"] = MobileNo;
                data["Message"] = Message;
                data["ReferenceNo"] = ReferenceNo;

                System.Uri uri = new System.Uri(url);


                var response = await wb.UploadValuesTaskAsync(uri, "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                return responseInString;
            }

        }


        public string SendSMS(string msg, string phonenumber)
        {
            //return "200";

            string returnMessage = "";
            if (phonenumber == "" || phonenumber == null) { // added by ebe in case that the requestor has no mobile number 05-30-2020
                return "200";
            }

            //06092020
            var firsttemp = phonenumber.Split(',');
            string newPhoneNumber = "";
            string tempNum = "";
            foreach (string firsttemp_ in firsttemp) {
                tempNum = "";
                tempNum = firsttemp_.Replace(" ", string.Empty);

                if (tempNum.Length == 11 && tempNum.Substring(0, 2) == "09") { 
                    newPhoneNumber += tempNum + ",";
                }
            }
            newPhoneNumber = newPhoneNumber.TrimEnd(','); // 06152020
            //end 06092020

            if (System.Environment.MachineName == "SODIUM2" || System.Environment.MachineName == "WSN1263")
            {
                msg = "***THIS IS A TEST. PLEASE IGNORE.***" + Environment.NewLine + msg;
            }

            var param = newPhoneNumber.Split(',');
            var sms = new
            {
                message = msg,
                PhoneNumbers = param,
            };

            string xstring = JsonConvert.SerializeObject(sms);
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(30);
            HttpResponseMessage response = new HttpResponseMessage();

            string urlconverted = "https://192.168.70.74/smsapi/api/smsweb2?cmd=" + xstring;
            response = client.GetAsync("https://192.168.70.74/smsapi/api/smsweb2?cmd=" + xstring).Result;

            if (response.IsSuccessStatusCode)
            {
                returnMessage = "200";
            }
            return returnMessage;
        }



        [Authorize(Policy = "RequireAdminAGSRole")]
        public async Task<IActionResult> DriverMonthlyReport()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _start = HttpContext.Request.Query["_start"];
            string _end = HttpContext.Request.Query["_end"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);


            ReportService.Report _report = new Report
            {
                FileName = "MonthlyReport.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "vDriverTripCounts",
            //    SQLQuery = $" Exec GetDriverCounts_New @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            //{
            //    DataSetName = "GetTripVehicleCounts",
            //    SQLQuery = $" Exec [GetTripVehicleCounts_DriverVehicle_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset3 = new ReportDataSet
            //{
            //    DataSetName = "GetDriverCounts_DriverVehicle_Monthly",
            //    SQLQuery = $" Exec [GetDriverCounts_DriverVehicle_Monthly_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "vDriverTripCounts",
                SQLQuery = " Exec GetDriverCounts @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            {
                DataSetName = "GetTripVehicleCounts",
                SQLQuery = " Exec [GetTripVehicleCounts_DriverVehicle] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset3 = new ReportDataSet
            {
                DataSetName = "GetDriverCounts_DriverVehicle_Monthly",
                SQLQuery = " Exec [GetDriverCounts_DriverVehicle_Monthly] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterQuery",
                SQLQuery = $" select '{_start}' as start_date, '{_end}' as end_date "
            };
            reportDataSets.Add(SQLQueryDataset4);

            reportDataSets.Add(SQLQueryDataset);
            reportDataSets.Add(SQLQueryDataset2);
            reportDataSets.Add(SQLQueryDataset3);
            //reportDataSets.Add(SQLQueryDataset4);
            


            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "DriverVehicleMonthlyReports.xls");
            }
        }

        [Authorize(Policy = "RequireAdminAGSRole")]

        public async Task<IActionResult> ShuttleMonthlyReport()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _start = HttpContext.Request.Query["_start"];
            string _end = HttpContext.Request.Query["_end"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);


            ReportService.Report _report = new Report
            {
                FileName = "MonthlyReportShuttle.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "vDriverTripCounts",
            //    SQLQuery = $" Exec GetShuttleCounts_New @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            //{
            //    DataSetName = "GetTripVehicleCounts",
            //    SQLQuery = $" Exec [GetTripVehicleCounts_Shuttle_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset3 = new ReportDataSet
            //{
            //    DataSetName = "GetDriverCounts_DriverVehicle_Monthly",
            //    SQLQuery = $" Exec [GetShuttleCounts_Monthly_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "vDriverTripCounts",
                SQLQuery = " Exec GetShuttleCounts @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            {
                DataSetName = "GetTripVehicleCounts",
                SQLQuery = " Exec [GetTripVehicleCounts_Shuttle] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset3 = new ReportDataSet
            {
                DataSetName = "GetDriverCounts_DriverVehicle_Monthly",
                SQLQuery = " Exec [GetShuttleCounts_Monthly] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterQuery",
                SQLQuery = " select '" + _start + "' as start_date, '" + _end + "' as end_date "
            };
            reportDataSets.Add(SQLQueryDataset4);

            reportDataSets.Add(SQLQueryDataset);
            reportDataSets.Add(SQLQueryDataset2);
            reportDataSets.Add(SQLQueryDataset3);



            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "ShuttleMonthlyReports.xls");
            }
        }


        [Authorize(Policy = "RequireAdminAGSRole")]

        public async Task<IActionResult> SummaryMonthlyReport()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _start = HttpContext.Request.Query["_start"];
            string _end = HttpContext.Request.Query["_end"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);


            ReportService.Report _report = new Report
            {
                FileName = "MonthlySummary.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "GetCounts_Monthly",
            //    SQLQuery = $" Exec GetCounts_Monthly_New @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            //{
            //    DataSetName = "GetTripVehicleCounts",
            //    SQLQuery = $" Exec [GetTripVehicleCounts_Union_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "GetCounts_Monthly",
                SQLQuery = " Exec GetCounts_Monthly @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            {
                DataSetName = "GetTripVehicleCounts",
                SQLQuery = " Exec [GetTripVehicleCounts_Union] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };
            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterQuery",
                SQLQuery = " select '" + _start + "' as start_date, '" + _end + "' as end_date "
            };
            reportDataSets.Add(SQLQueryDataset4);

            reportDataSets.Add(SQLQueryDataset);
            reportDataSets.Add(SQLQueryDataset2);



            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "SummaryMonthlyReports.xls");
            }
        }


        [Authorize(Policy = "RequireAdminAGSRole")]

        public async Task<IActionResult> ChargingReport()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _start = HttpContext.Request.Query["_start"];
            string _end = HttpContext.Request.Query["_end"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);


            ReportService.Report _report = new Report
            {
                FileName = "ChargingReport.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "GetTripChargingCompanies_Union",
            //    SQLQuery = $" Exec GetTripChargingCompanies_Union_New @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset1 = new ReportDataSet
            //{
            //    DataSetName = "GetTripChargingDepartmentByCompanies_Union_Ranked1",
            //    SQLQuery = $" Exec [GetTripChargingDepartmentByCompanies_Union_Ranked1_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            //{
            //    DataSetName = "GetTripChargingDepartmentByCompanies_Union_Ranked2",
            //    SQLQuery = $" Exec [GetTripChargingDepartmentByCompanies_Union_Ranked2_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            //ReportService.ReportDataSet SQLQueryDataset3 = new ReportDataSet
            //{
            //    DataSetName = "GetTripChargingDepartmentByCompanies_Union_Ranked3",
            //    SQLQuery = $" Exec [GetTripChargingDepartmentByCompanies_Union_Ranked3_New] @StartDate = '{_start}', @EndDate = '{_end}', @CompanyGroupId = {_companyGroupId}"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "GetTripChargingCompanies_Union",
                SQLQuery = " Exec GetTripChargingCompanies_Union @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset1 = new ReportDataSet
            {
                DataSetName = "GetTripChargingDepartmentByCompanies_Union_Ranked1",
                SQLQuery = " Exec [GetTripChargingDepartmentByCompanies_Union_Ranked1] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset2 = new ReportDataSet
            {
                DataSetName = "GetTripChargingDepartmentByCompanies_Union_Ranked2",
                SQLQuery = " Exec [GetTripChargingDepartmentByCompanies_Union_Ranked2] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset3 = new ReportDataSet
            {
                DataSetName = "GetTripChargingDepartmentByCompanies_Union_Ranked3",
                SQLQuery = " Exec [GetTripChargingDepartmentByCompanies_Union_Ranked3] @StartDate = '" + _start + "', @EndDate = '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterQuery",
                SQLQuery = " select '" + _start + "' as start_date, '" + _end + "' as end_date "
            };
            reportDataSets.Add(SQLQueryDataset4);
            reportDataSets.Add(SQLQueryDataset);
            reportDataSets.Add(SQLQueryDataset1);
            reportDataSets.Add(SQLQueryDataset2);
            reportDataSets.Add(SQLQueryDataset3);



            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "ChargingReports.xls");
            }
        }


        //OutoftownTrip

        [Authorize(Policy = "RequireAdminAGSRole")]

        public async Task<IActionResult> OutoftownTrip()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _start = HttpContext.Request.Query["_start"];
            string _end = HttpContext.Request.Query["_end"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);

            ReportService.Report _report = new Report
            {
                FileName = "Outoftown.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "vOutsideMMTrips",
            //    SQLQuery = $" select * from vOutsideMMTrips_New where TRY_CAST(TripDate AS DATE) >= '{_start}' and TRY_CAST(TripDate AS DATE) <= '{_end}' and CompanyGroupId = {_companyGroupId}"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "vOutsideMMTrips",
                SQLQuery = " select * from vOutsideMMTrips where TripDate >= '" + _start + "' and TripDate <= '" + _end + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterQuery",
                SQLQuery = " select '" + _start + "' as start_date, '" + _end + "' as end_date "
            };
            reportDataSets.Add(SQLQueryDataset4);


            reportDataSets.Add(SQLQueryDataset);



            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "OutofTown.xls");
            }
        }



        //Survey Report Per Transactions
        [Authorize(Policy = "RequireAdminAGSRole")]

        public async Task<IActionResult> SurveyPerTransaction()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _q = HttpContext.Request.Query["_q"];
            string _y = HttpContext.Request.Query["_y"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);


            ReportService.Report _report = new Report
            {
                FileName = "SurveyPerTransaction.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "vSurveyResults",
            //    SQLQuery = $" select * from vSurveyResults_New where QuarterNo = '{_q}' and YearNo = '{_y}' and CompanyGroupId = '{_companyGroupId}'"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "vSurveyResults",
                SQLQuery = " select * from vSurveyResults where QuarterNo = '" + _q + "' and YearNo = '" + _y + "'  "
            };

            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterSurveyQuery",
                SQLQuery = " select '" + _q + "' as QuarterNo, '" + _y + "' as YearNo, count(*) as TotalRespondents from vSurveyResults where QuarterNo = '" + _q + "' and YearNo = '" + _y + "'  "
            };

            reportDataSets.Add(SQLQueryDataset4);
            reportDataSets.Add(SQLQueryDataset);



            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "SurveyPerTransaction.xls");
            }
        }

        //Survey Report Per Transactions
        [Authorize(Policy = "RequireAdminAGSRole")]

        public async Task<IActionResult> SurveyPerDriver()
        {
            int t = Int32.Parse(HttpContext.Request.Query["t"]);
            //int i = Int32.Parse(HttpContext.Request.Query["i"]);
            string _q = HttpContext.Request.Query["_q"];
            string _y = HttpContext.Request.Query["_y"];
            int _companyGroupId = Int32.Parse(HttpContext.Request.Query["_companyGroupId"]);


            ReportService.Report _report = new Report
            {
                FileName = "SurveyPerDriver.rdlc",
                FolderName = "SSRS"
            };

            ReportService.Database database = new Database
            {
                DbServer = _dbServer,
                DbName = _dbName,
                DbUser = "ict",
                DbPwd = "ict@ictdept"
            };


            List<ReportService.ReportDataSet> reportDataSets = new List<ReportDataSet>();

            //ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            //{
            //    DataSetName = "GetSurveyResultsPerDriver",
            //    SQLQuery = $" Exec [GetSurveyResultsPerDriver_New] @Quarter = '{_q}', @Year = '{_y}', @CompanyGroupId = {_companyGroupId}"
            //};

            ReportService.ReportDataSet SQLQueryDataset = new ReportDataSet
            {
                DataSetName = "GetSurveyResultsPerDriver",
                SQLQuery = " Exec [GetSurveyResultsPerDriver] @Quarter = '" + _q + "', @Year = '" + _y + "'"
            };

            ReportService.ReportDataSet SQLQueryDataset4 = new ReportDataSet
            {
                DataSetName = "filterSurveyQuery",
                SQLQuery = " select '" + _q + "' as QuarterNo, '" + _y + "' as YearNo, count(*) as TotalRespondents from vSurveyResults where QuarterNo = '" + _q + "' and YearNo = '" + _y + "'  "
            };

            reportDataSets.Add(SQLQueryDataset4);
            reportDataSets.Add(SQLQueryDataset);



            if (t == 0)
            {
                //PDF
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GeneratePDFAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/pdf");
            }
            else
            {
                //EXCEL
                var client = new ReportService.Service1Client();
                byte[] bytes = await client.GenerateExcelAsync(reportDataSets.ToArray(), _report, database);
                return File(bytes, "application/vnd.ms-excel", "SurveyPerDriver.xls");
            }
        }


        public string getLink()
        {

            string hostLink = "";

            if (System.Environment.MachineName == "SODIUM2")
            {
                hostLink = "http://sodium2/TransportHub";
            }
            else if (System.Environment.MachineName == "ALUMINUM")
            {
                hostLink = "https://ictsystems.semirarampc.com:8443/TransportHub";
            }
            else if (System.Environment.MachineName == "CALIFORNIUM")
            {
                hostLink = "https://www.semirarampc.com:8443/TransportHub";
            }
            else if (System.Environment.MachineName == "ANDROMEDA")
            {
                hostLink = "http://192.168.30.182/TransportHubCPC";
            }
            else if (System.Environment.MachineName == "PEGASUS")
            {
                hostLink = "http://pegasus.semcalaca.com/TransportHubCPC";
            }
            else
            {
                hostLink = "https://localhost:44390";
            }

            return hostLink;
        }


        public string stringHasher(string keyword)
        {


            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: keyword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            hashed = hashed.Replace("+", "1");
            hashed = hashed.Replace("&", "2");
            hashed = hashed.Replace("=", "3");

            return hashed;
        }

        //JPT additional code 09062024
        public async Task<string> SendSmsAsync(string mobileNumbers, string message, string referenceNo, string systemName)
        {
            // Modify message if MachineName matches specific values
            if (System.Environment.MachineName == "SODIUM2" || System.Environment.MachineName == "WSN1263")
            {
                message += Environment.NewLine + "***THIS IS A TEST. PLEASE IGNORE.***";
            }

            // API URL
            //string apiUrl = "http://192.168.70.74/smsWebApi/api/SmsApi/send"; //Old API using GSM Modem
            //string apiUrl = "http://sodium2/SMARTSMS/api/SendSmsApi/send"; //Test New API using Smart 
            string apiUrl = "http://aluminum/SMARTSMS_SMPC/api/SendSmsApi/send"; //Live New API using Smart 

            // Split the mobile numbers by comma
            string[] mobileArr = mobileNumbers.Split(',');

            // Remove duplicates and empty or whitespace entries
            mobileArr = mobileArr.Distinct()
                                 .Where(m => !string.IsNullOrWhiteSpace(m))
                                 .ToArray();

            string smsStatus = string.Empty;

            // Create HttpClient for sending API requests
            using (HttpClient client = new HttpClient())
            {
                foreach (string mobileNum in mobileArr)
                {
                    // Validate the format for 11 digits starting with "09" or 12 digits starting with "+63"
                    if ((mobileNum.Length == 11 && mobileNum.StartsWith("09")) || (mobileNum.Length == 12 && mobileNum.StartsWith("63")) ||
                    (mobileNum.Length == 13 && mobileNum.StartsWith("+63")))
                    {
                        //For Single Message
                        //var postData = new
                        //{
                        //    Message = message,
                        //    DestinationNumbers = mobileNum,
                        //    ReferenceNo = referenceNo,
                        //    SystemName = systemName
                        //};

                        //For Multiple Message
                        var postData = new[]
                        {
                            new
                            {
                                Message = message,
                                Destination = mobileNum,
                                ReferenceNo = referenceNo,
                                SystemName = systemName
                            }
                        };

                        // Serialize the data to JSON
                        string jsonContent = JsonConvert.SerializeObject(postData);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        try
                        {
                            // Send the POST request
                            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                            // Check if the request was successful
                            if (response.IsSuccessStatusCode)
                            {
                                // Handle successful response
                                string responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"SMS sent successfully to {mobileNum}: {responseContent}");
                                smsStatus = "SMS sent successfully. ";
                            }
                            else
                            {
                                // Handle error response
                                string errorContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"Error sending SMS to {mobileNum}: {errorContent}");
                                smsStatus = "SMS not sent due to bad connection. ";
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            // Handle exceptions such as network errors
                            Console.WriteLine($"Request error while sending to {mobileNum}: {e.Message}");
                            smsStatus = "SMS not sent due to bad connection. ";
                        }
                    }
                }
            }

            return smsStatus;
        }
        //JPT end additional code 09062024


    }
}