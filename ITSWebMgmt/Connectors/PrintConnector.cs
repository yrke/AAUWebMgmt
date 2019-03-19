using SimpleImpersonation;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace ITSWebMgmt.Connectors
{
    public class PrintConnector
    {

        string userGuid = "";

        public PrintConnector(string guid)
        {
            this.userGuid = guid;
        }


        public string doStuff()
        {

            StringBuilder sb = new StringBuilder();

            string domain = ConfigurationManager.AppSettings["cred:equitrac:domain"];
            string username = ConfigurationManager.AppSettings["cred:equitrac:username"];
            string secret = ConfigurationManager.AppSettings["cred:equitrac:password"];

            if (domain == null || username == null || secret == null )
            {
                return "No valid creds for Equitrac";
            }

            var credentials = new UserCredentials(domain, username, secret);
            Impersonation.RunAsUser(credentials, LogonType.NewCredentials, () =>
            {
                // do whatever you want as this user.

                //SqlConnection myConnection = new SqlConnection("Data Source = ad-sql1-i13.aau.dk\\sqlequitrac; Database = eqcas; Integrated Security=SSPI");
                SqlConnection myConnection = new SqlConnection("Data Source = AD-SQL2-MISC.AAU.DK; Database = eqcas; Integrated Security=SSPI");

                bool SQLSuccess = true;

                try
                {
                    myConnection.Open();
                }
                catch (SqlException)
                {
                    sb.Append("Error connection to equitrac database.");
                    SQLSuccess = false;
                }

                if (SQLSuccess)
                {
                    string adguid = "AD:{" + userGuid + "}";

                    string sqlcommand = @"
                    	SELECT 
                    		account.creation, 
                    		account.lastmodified, 
                    		account.state,
                    		account.freemoney,
                    		account.balance, 
		                    account.primarypin as AAUCardXerox,
		                    altp.primarypin as AAUCardKonica, 
                            dept.valtype as departmentThing
                    
                    	FROM cat_validation account
                    		LEFT OUTER JOIN cas_val_assoc      ass  ON ass.associd= account.id
                    		LEFT OUTER JOIN cat_validation     dept ON dept.id    = ass.mainid and dept.valtype = 'dpt'
                    		LEFT OUTER JOIN cas_user_ext       ur   ON ur.x_id    = account.id
                    		LEFT OUTER JOIN cas_user_class     uc   ON uc.id      = classid
                    		LEFT OUTER JOIN cas_primarypin_ext altp ON altp.x_id  = account.id
                    		LEFT JOIN cas_location             loc  ON loc.id     = account.locationid
                    		Where syncidentifier = @adguid;
                   ";

                    var command = new SqlCommand(sqlcommand, myConnection);
                    command.Parameters.AddWithValue("@adguid", adguid);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            //sb.Append("User: " + reader["AAUCardXerox"]);
                            string AAUCardXerox = reader["AAUCardXerox"] as string;
                            string AAUCardKonica = reader["AAUCardKonica"] as string;
                            string departmentThing = reader["departmentThing"] as string;
                            int state = reader.GetByte(reader.GetOrdinal("state"));

                            decimal free = reader.GetDecimal(reader.GetOrdinal("freemoney"));
                            decimal balance = reader.GetDecimal(reader.GetOrdinal("balance"));
                            decimal paid = balance - free;



                            //sb.Append("stuff:" + AAUCardXerox);
                            //sb.Append("stuff1:" + AAUCardKonica);
                            //sb.Append("stuff2:" + state);

                            bool cardok = true;

                            if (state != 1)
                            {
                                sb.Append("Error! Users is disabled in Equitrac<br/>");
                                cardok = false;
                            }
                            if (String.IsNullOrWhiteSpace(AAUCardKonica))
                            {
                                sb.Append("Error! Users is missing AAUCard information in Konica format <br/>");
                                cardok = false;
                            }
                            if (String.IsNullOrWhiteSpace(AAUCardXerox))
                            {
                                sb.Append("Error! Users is missing AAUCard information in Xerox format <br/>");
                                cardok = false;
                            }

                            if (cardok)
                            {
                                sb.Append("AAU Card OK <br/>");
                            }

                            if (String.IsNullOrEmpty(departmentThing))
                            {

                                sb.Append("<br/>");
                                sb.Append("Free Credits: " + free);
                                sb.Append("<br/>");
                                sb.Append("Paid Credits: " + paid);
                                sb.Append("<br/>");
                                sb.Append("Remaning Credits: " + balance);

                            }
                            else
                            {
                                sb.Append("User has \"free print\"");
                            }

                        }
                    }

                    myConnection.Close();
                }

                //ad:{8d0ef212-766e-44b8-8900-ead976e4f7cb} // kyrke
            });



            return sb.ToString();
        }












    }
}