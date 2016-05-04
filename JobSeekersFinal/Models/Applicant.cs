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
            if (data.ContainsKey("FirstName"))
                FirstName = data["FirstName"].ToString();

            if (data.ContainsKey("MiddleName"))
                MiddleName = data["MiddleName"].ToString();

            if (data.ContainsKey("LastName"))
                LastName = data["LastName"].ToString();

            if (data.ContainsKey("Street"))
                Address = data["Street"].ToString();
            else if (data.ContainsKey("Address"))
                Address = data["Address"].ToString();

            if (data.ContainsKey("City"))
                City = data["City"].ToString();

            if (data.ContainsKey("State"))
                State = data["State"].ToString();

            if (data.ContainsKey("Zip"))
                Zip = Convert.ToInt32(data["Zip"]);
            else if (data.ContainsKey("Zipcode"))
                Zip = Convert.ToInt32(data["Zipcode"]);

            if (data.ContainsKey("Phone"))
                Phone = data["Phone"].ToString();

            if (data.ContainsKey("Email"))
                Email = data["Email"].ToString();

            if (data.ContainsKey("Positions"))
                DesiredPositions = data["Positions"].ToString();
            else if (data.ContainsKey("Title"))
                DesiredPositions = data["Title"].ToString();

            if (data.ContainsKey("Skills"))
                Skills = data["Skills"].ToString();

            if (data.ContainsKey("Resume"))
                ResumePath = data["Resume"].ToString();
           
        }



        public void SetAnswers(int[] answers, int quizPageNum)
        {
            if (Answers == null) Answers = new int[20];
            int offset = (quizPageNum - 1) * 5;
            answers.CopyTo(Answers, offset);
        }
    }

}