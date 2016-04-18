using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

using JobSeekersFinal.Models;

namespace JobSeekersFinal.DataSources
{
    public class DataService : IDisposable
    {
        public DataService(string connectionString)
            :this(new SqlConnection(connectionString))
        {
        }

        public DataService(SqlConnection connection)
        {
            _connection = connection;
        }
        
        public SqlConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// Retrieve a set of records from the DB with a custom Query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, object>> GetRecords(string query)
        {
            if (!OpenConnection())
                return new List<Dictionary<string, object>>();

            try
            {
                using (var cmd = new SqlCommand(query, _connection))
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    return ReadRecords(reader);
                }
            }
            catch (SqlException)
            {
                return new List<Dictionary<string, object>>();
            }
        }

        public int GetAuthType(string email)
        {
            if (!OpenConnection() || string.IsNullOrWhiteSpace(email))
                return -1;

            string query = "SELECT Type FROM Auth WHERE upper(email) = upper(@email)";
            using (var cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.Add(new SqlParameter("@email", email.ToUpper()));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        /// <summary>
        /// Confirm email/password match a database entry
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>true: user exists, false: user or password not found.</returns>
        public bool VerifyAccount(string email, string password)
        {
            if (!OpenConnection() || string.IsNullOrWhiteSpace(password))
                return false;

            string query = "SELECT Password FROM Auth WHERE upper(email) = upper(@email)";
            using (var cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.Add(new SqlParameter("@email", email.ToUpper()));
                string pw = cmd.ExecuteScalar().ToString().Trim();
                return string.Equals(pw, password);
            }
        }

        /// <summary>
        /// Insert Auth and Applicant records for new applicant
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CreateAccount(string email, string password)
        {
            if (!OpenConnection() || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            string testEmailQuery = "SELECT Password FROM Auth WHERE upper(email) = @email";
            string insertNewAccountQuery = "INSERT into Auth (email, [password]) " +
                                           "values (@email, @password); " +
                                           "INSERT into Applicants (email) values (@email)";

            try
            {
                using (var cmd = new SqlCommand(testEmailQuery, _connection))
                {
                    cmd.Parameters.Add(new SqlParameter("@email", email.ToUpper()));
                    if (cmd.ExecuteScalar() != null)
                        throw new Exception("Account with that email already exists in the database.");

                    using (var insertCmd = new SqlCommand(insertNewAccountQuery, _connection))
                    {
                        insertCmd.Parameters.Add(new SqlParameter("@email", email.ToUpper()));
                        insertCmd.Parameters.Add(new SqlParameter("@password", password));
                        return insertCmd.ExecuteNonQuery() == 2;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get the specified quiz section (1-4) presented to new users.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Dictionary<string, List<Tuple<int, string>>> GetQuizSection(int section)
        {
            if (!OpenConnection() || section < 0 || section > 4)
                return null;

            section = section * 5 - 4;

            string query = "SELECT q.QUESTION, A.ANSWER, A.ID " +
                           "FROM Question q " +
                           "JOIN Answer a " +
                           "ON q.id = a.QuestionID " +
                           "WHERE q.id between @section and (@section + 4)";

            using (var cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.Add(new SqlParameter("@section", section));
                IDataReader reader = cmd.ExecuteReader();
                var records = ReadRecords(reader);

                var returnVal = new Dictionary<string, List<Tuple<int, string>>>();
                foreach (var record in records)
                {
                    string q = record["QUESTION"].ToString();
                    string a = record["ANSWER"].ToString();
                    int aId = Convert.ToInt32(record["ID"]);
                    if (returnVal.ContainsKey(q))
                    {
                        returnVal[q].Add(new Tuple<int, string>(aId, a));
                    }
                    else
                    {
                        returnVal[q] = new List<Tuple<int, string>>();
                        returnVal[q].Add(new Tuple<int, string>(aId, a));
                    }
                }
                return returnVal;
            }
        }   
        
        /// <summary>
        /// Save a quiz section submitted by new users.
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="email"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public bool SaveQuizSection(int[] answers, string email, int section)
        {
            if (!OpenConnection())
                return false;

            if (answers.Length != 5 || answers.Any(n => n < 0))
                throw new Exception("Received answer save request without 5 answers");

            string appQuery = "SELECT ID FROM Applicants where upper(email) = @email";
            
            int? appID;
            using (var idCmd = new SqlCommand(appQuery, _connection))
            {
                idCmd.Parameters.Add(new SqlParameter("@email", email.ToUpper()));
                appID = (int?)idCmd.ExecuteScalar() ?? -1;
            }

            if (appID == -1)
                throw new Exception("Invalid Applicant email provided");

            string saveQuery = "INSERT into ApplicantsAnswers (answerid, applicantid) values " +
                               "(@a1, @appID)," +
                               "(@a2, @appID)," +
                               "(@a3, @appID)," +
                               "(@a4, @appID)," +
                               "(@a5, @appID)";

            // When this is the first question set, delete all the previous answers.
            if (section == 1)
                saveQuery = "DELETE FROM ApplicantsAnswers where applicantId = @appID; " + saveQuery;

            using (var insertCmd = new SqlCommand(saveQuery, _connection))
            {
                SqlParameter[] sqlParams =
                {
                    new SqlParameter("@appID", appID),
                    new SqlParameter("@a1", answers[0]),
                    new SqlParameter("@a2", answers[1]),
                    new SqlParameter("@a3", answers[2]),
                    new SqlParameter("@a4", answers[3]),
                    new SqlParameter("@a5", answers[4]),
                };
                insertCmd.Parameters.AddRange(sqlParams);

                // Should insert 5 values
                return insertCmd.ExecuteNonQuery() == 5;
            }
        }
        
        /// <summary>
        /// Save name/address/skills/etc from a new user.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool SaveApplicantData(Applicant app)
        {
            if (!OpenConnection())
                return false;

            string query = "UPDATE Applicants SET lastname = @lName, " +
                                                  "middlename = @mName, " +
                                                  "firstname = @fName, " +
                                                  "title = @title, " +
                                                  "skills = @skills " +
                                                //"createdate = @createdate" +
                                                 "WHERE upper(email) = @email";
            using (var cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddRange(new SqlParameter[]
                                        {
                                            new SqlParameter("@lName", app.LastName),
                                            new SqlParameter("@mName", app.MiddleName),
                                            new SqlParameter("@fName", app.FirstName),                                            
                                            new SqlParameter("@email", app.Email.ToUpper()),
                                            new SqlParameter("@title", app.DesiredPositions),
                                            new SqlParameter("@skills", app.Skills),
                                            //new SqlParameter("@createdate", app.CreateDate),
                                        });

                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public bool SaveAuthData(Applicant app)
        {
            if (!OpenConnection())
                return false;

            string query = "UPDATE Auth SET address = @address, " +
                                            "city = @city, " +
                                            "state = @state, " +
                                            "zipcode = @zip, " +
                                            "phone = @phone " +
                                            "WHERE upper(email) = @email";
            using (var cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddRange(new SqlParameter[]
                                        {
                                            new SqlParameter("@address", app.Address),
                                            new SqlParameter("@city", app.City),
                                            new SqlParameter("@state", app.State),
                                            new SqlParameter("@zip", app.Zip),
                                            new SqlParameter("@phone", app.Phone),
                                            new SqlParameter("@email", app.Email.ToUpper()),
                                        });

                return cmd.ExecuteNonQuery() == 1;
            }
        }



        private IEnumerable<Dictionary<string, object>> ReadRecords(IDataReader reader)
        {
            var records = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                var rec = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rec.Add(reader.GetName(i), reader.GetValue(i));
                }
                records.Add(rec);
            }

            return records;
        }
        
        private SqlConnection _connection;
        
        private bool OpenConnection()
        {
            if (_connection == null)
                return false;

            try
            {
                if ((_connection.State & (ConnectionState.Connecting | ConnectionState.Open)) > 0)
                    return true;

                _connection.Open();

                return true;
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is SqlException)
                    return false;
                throw e;
            }
        }
        
        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}