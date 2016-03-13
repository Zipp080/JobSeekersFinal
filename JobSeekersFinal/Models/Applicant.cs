using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JobSeekersFinal.Models
{
    public class Applicant
    {
        public string FirstName { get; set; } = null;
        public string MiddleName { get; set; } = null;
        public string LastName { get; set; } = null;

        public string Address { get; set; } = null;
        public string City { get; set; } = null;
        public string State { get; set; } = null;
        public int Zip { get; set; } = 0;
        public string Phone { get; set; } = null;
        public string Email { get; set; } = null;

        public string DesiredPositions { get; set; } = null;
        public string Skills { get; set; } = null;

        public string ResumePath { get; set; } = null;

        public int[] Answers { get; set; } = null;

        public void SetData(Dictionary<string, object> data)
        {
            FirstName = data["firstName"].ToString();
            MiddleName = data["middleName"].ToString();
            LastName = data["lastName"].ToString();
            Address = data["street"].ToString();
            City = data["city"].ToString();
            State = data["state"].ToString();
            Zip = Convert.ToInt32(data["zip"]);
            Phone = data["phone"].ToString();
            Email = data["email"].ToString();
            DesiredPositions = data["positions"].ToString();
            Skills = data["skills"].ToString();
            //ResumePath = data["resumePath"].ToString();
        }

        public void SetAnswers(int[] answers, int quizPageNum)
        {
            if (Answers == null) Answers = new int[20];
            int offset = (quizPageNum - 1) * 5;
            answers.CopyTo(Answers, offset);
        }
    }

}