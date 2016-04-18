using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JobSeekersFinal.Models;
using JobSeekersFinal.DataSources;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JobSeekersFinal.Controllers
{
    public class HomeController : Controller
    {

        private static readonly string _connString = ConfigurationManager.ConnectionStrings["appDb"].ConnectionString;
        private const int _appAuthType = 1;
        private const int _employerAuthType = 2;

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Dashboard(int authType, string email)
        {
            if (authType == _employerAuthType)
                return View("Dashboard", GetApplicants());
            
            using (var sql = new DataService(_connString))
            {
                var applicant = sql.GetRecords($"Select * from applicants where upper(Email) = upper('{email}')");
                var app = new Applicant();
                app.SetData(applicant.FirstOrDefault());
                return View("ApplicantProfile", app);
            }
        }

        private Applicant[] GetApplicants()
        {
            using (var sql = new DataService(_connString))
            {
                List<Applicant> applicants = new List<Applicant>();
                var apps = sql.GetRecords("SELECT au.*, app.LastName, app.FirstName, app.Title FROM Applicants app JOIN Auth au on app.email = au.email where au.type = 1");
                foreach (var app in apps)
                {
                    var appToAdd = new Applicant();
                    appToAdd.SetData(app);
                    applicants.Add(appToAdd);
                }
                return applicants.ToArray();
            }
        }

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

        public ActionResult ApplicantProfile()
        {

            return View();
        }

        private enum ScoreCategory
        {
            Power,
            Inspirational,
            Balance,
            Analytical
        }

        public JsonResult DashboardProfile(string email)
        {
            using (var sql = new DataService(_connString))
            {
                var appRecord = sql.GetRecords($"SELECT au.*, app.Id as appId, app.LastName, app.FirstName, app.MiddleName, app.Title, app.Skills FROM Applicants app JOIN Auth au on app.email = au.email where upper(au.email) = upper('{email}')").FirstOrDefault();
                if (appRecord == null)
                {
                    return JsonAllowGet(new
                    {
                        Result = false
                    });
                }
                else
                {
                    var answerResults = sql.GetRecords($"SELECT ans.* FROM AnswerScore ans join ApplicantsAnswers aa on ans.AnswerId = aa.AnswerId join applicants a on aa.applicantid = a.id where a.id = {Convert.ToInt32(appRecord["appId"])}");

                    return JsonAllowGet(new
                    {
                        Result = true,
                        Name = $"{appRecord["FirstName"]} {appRecord["MiddleName"]} {appRecord["LastName"]}",
                        Title = appRecord["Title"].ToString(),
                        Address = $"{appRecord["Address"]}, {appRecord["City"]}, {appRecord["State"]} {appRecord["Zipcode"]}",
                        Phone = appRecord["Phone"].ToString(),
                        Email = email,
                        Skills = appRecord["Skills"].ToString(),
                        Power = answerResults.Where(r => Convert.ToInt32(r["CategoryID"]) == (int)ScoreCategory.Power).Sum(r => Convert.ToInt32(r["Score"])),
                        Inspirational = answerResults.Where(r => Convert.ToInt32(r["CategoryID"]) == (int)ScoreCategory.Inspirational).Sum(r => Convert.ToInt32(r["Score"])),
                        Balance = answerResults.Where(r => Convert.ToInt32(r["CategoryID"]) == (int)ScoreCategory.Balance).Sum(r => Convert.ToInt32(r["Score"])),
                        Analytical = answerResults.Where(r => Convert.ToInt32(r["CategoryID"]) == (int)ScoreCategory.Analytical).Sum(r => Convert.ToInt32(r["Score"]))
                    });
                }
            }
        }

        public ActionResult HowDoesItWork()
        {

            return View();
        }

       

        public ActionResult AccountLogin()
        {
            return View();
        }

        public ActionResult Login(string email, string password)
        {            
            using (var sql = new DataService(_connString))
            {
                bool success = sql.VerifyAccount(email, password);
                if (success)
                {
                    return JsonAllowGet(new { Result = success, AuthType = sql.GetAuthType(email) });
                }
                else
                {
                    return JsonAllowGet(new { Result = success });
                }
            }
        }


        public ActionResult Sales()
        {

            return View();
        }
        public ActionResult EmployerHelp()
        {

            return View();
        }

        /// <summary>
        /// Presents applicant data entry forms.
        /// </summary>
        /// <returns></returns>
        public ActionResult NewApplicantProfile(string email)
        {
            ApplicantData.StoreApplicantData(email.ToUpper(), new Applicant());
            ViewBag.EmailAddress = email.Replace("@", "%40");
            return View();
        }

        /// <summary>
        /// Delivers SignUp prompt to user.
        /// </summary>
        /// <returns></returns>
        public ActionResult Signup()
        {
            return View();
        }

        /// <summary>
        /// Presents question section 1, saves applicant data (Name/Address/etc).
        /// </summary>
        /// <returns></returns>
        public ActionResult Personality1()
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.QueryString[0]);
            var newApp = ApplicantData.GetApplicantData(jsonData["Email"].ToString());
            newApp.SetData(jsonData);

            using (var sql = new DataService(_connString))
            {
                sql.SaveApplicantData(newApp);
                sql.SaveAuthData(newApp);
                VM_Personality questions = new VM_Personality(sql.GetQuizSection(1));
                return PartialView("Personality1", questions);
            }
        }

        /// <summary>
        /// Presents question section 2, saves question section 1.
        /// </summary>
        /// <returns></returns>
        public ActionResult Personality2()
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.QueryString[0]);
            using (var sql = new DataService(_connString))
            {
                sql.SaveQuizSection(ConvertJArrayToIntArray((JArray)jsonData["answerArray"]), jsonData["Email"].ToString(), 1);
                VM_Personality questions = new VM_Personality(sql.GetQuizSection(2));
                return PartialView("Personality2", questions);
            }
        }

        /// <summary>
        /// Presents question section 3, saves question section 2.
        /// </summary>
        /// <returns></returns>
        public ActionResult Personality3()
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.QueryString[0]);
            using (var sql = new DataService(_connString))
            {
                sql.SaveQuizSection(ConvertJArrayToIntArray((JArray)jsonData["answerArray"]), jsonData["email"].ToString(), 2);
                VM_Personality questions = new VM_Personality(sql.GetQuizSection(3));
                return PartialView("Personality3", questions);
            }
        }

        /// <summary>
        /// Presents question section 4, saves question section 3.
        /// </summary>
        /// <returns></returns>
        public ActionResult Personality4()
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.QueryString[0]);
            using (var sql = new DataService(_connString))
            {
                sql.SaveQuizSection(ConvertJArrayToIntArray((JArray)jsonData["answerArray"]), jsonData["email"].ToString(), 3);
                VM_Personality questions = new VM_Personality(sql.GetQuizSection(4));
                return PartialView("Personality4", questions);
            }
        }

        /// <summary>
        /// NEEDS UPDATE: Last page seen after saving last quiz questions.  Saves question section 4
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplicationComplete()
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.QueryString[0]);
            using (var sql = new DataService(_connString))
            {
                sql.SaveQuizSection(ConvertJArrayToIntArray((JArray)jsonData["answerArray"]), jsonData["email"].ToString(), 4);
                return View();
            }            
        }

        /// <summary>
        /// Insert new applicant into Auth and Applicants table.
        /// </summary>
        /// <param name="email">applicant email</param>
        /// <param name="password">applicant password</param>
        /// <returns></returns>
        public ActionResult AddNewAccount(string email, string password)
        {
            var queryParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.QueryString[0]);
            using (var sql = new DataSources.DataService(_connString))
            {
                try
                {
                    if (sql.CreateAccount(queryParams["email"], queryParams["password"]))
                    {
                        return Json(new { Success = true, Message = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Success = false, Message = "Account creation failed, please check your information and try again." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new { Success = false, Message = e.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private int[] ConvertJArrayToIntArray(JArray convertValues)
        {
            List<int> x = new List<int>();
            foreach (var val in convertValues)
            {
                x.Add(Convert.ToInt32(val));
            }
            return x.ToArray();
        }


        private JsonResult JsonAllowGet(object json)
        {
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
