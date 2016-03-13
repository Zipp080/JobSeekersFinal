using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JobSeekersFinal.Models;

namespace JobSeekersFinal.DataSources
{
    public static class ApplicantData
    {
        private static Dictionary<string, Applicant> _inProgressApps = new Dictionary<string, Applicant>();



        public static Applicant GetApplicantData(string emailAddress)
        {
            Applicant app;
            if (_inProgressApps.TryGetValue(emailAddress.ToUpper(), out app))
                return app;
            else
                return null;
        }

        public static void StoreApplicantData(string emailAddress, Applicant application)
        {
            _inProgressApps[emailAddress.ToUpper()] = application;
        }
    }
}