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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FrontDash()
        {
            return View();
        }

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

        /// <summary>
        /// Presents applicant data entry forms.
        /// </summary>
        /// <returns></returns>
        public ActionResult NewApplicant(string email)
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
            var newApp = ApplicantData.GetApplicantData(jsonData["email"].ToString());
            newApp.SetData(jsonData);

            using (var sql = new DataService(_connString))
            {
                sql.SaveApplicantData(newApp);
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
                sql.SaveQuizSection(ConvertJArrayToIntArray((JArray)jsonData["answerArray"]), jsonData["email"].ToString(), 1);
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
                return PartialView("AppComplete");
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
    }
}
