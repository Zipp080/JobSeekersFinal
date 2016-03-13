using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JobSeekersFinal.Models
{
    public class VM_Personality
    {
        public VM_Personality(Dictionary<string, List<Tuple<int, string>>> quizQuestions)
        {
            var orderedQuestions = quizQuestions.OrderBy(x => x.Key).ToList();
            Q1 = new Tuple<string, List<Tuple<int, string>>>(orderedQuestions[0].Key, orderedQuestions[0].Value);
            Q2 = new Tuple<string, List<Tuple<int, string>>>(orderedQuestions[1].Key, orderedQuestions[1].Value);
            Q3 = new Tuple<string, List<Tuple<int, string>>>(orderedQuestions[2].Key, orderedQuestions[2].Value);
            Q4 = new Tuple<string, List<Tuple<int, string>>>(orderedQuestions[3].Key, orderedQuestions[3].Value);
            Q5 = new Tuple<string, List<Tuple<int, string>>>(orderedQuestions[4].Key, orderedQuestions[4].Value);
        }

        public Tuple<string, List<Tuple<int, string>>> Q1;
        public Tuple<string, List<Tuple<int, string>>> Q2;
        public Tuple<string, List<Tuple<int, string>>> Q3;
        public Tuple<string, List<Tuple<int, string>>> Q4;
        public Tuple<string, List<Tuple<int, string>>> Q5;
    }
}