using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.DirectoryServices;

namespace ITSWebMgmt
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var builder = new StringBuilder();




            string userName = "kyrke@its.aau.dk";
            DirectorySearcher search = new DirectorySearcher();
            //search.Filter = String.Format("(userPrincipalName={0})", userName);
            search.PropertiesToLoad.Add("cn");
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");


            search.Filter = String.Format("(cn={0})", "AAU804396");


            //search.PropertiesToLoad.Add("samaccountname");
            //search.PropertiesToLoad.Add("givenname");
            //search.PropertiesToLoad.Add("sn");
            //search.PropertiesToLoad.Add("memberOf");
            search.PropertiesToLoad.Add("canonicalName");
            search.PropertiesToLoad.Add("employeeNumber");

            SearchResult result = search.FindOne();

            if (result == null)
            {
                builder.Append("Computer not found");
            }
            else
            {
                //StringBuilder groupsList = new StringBuilder();

                /*int groupCount = result.Properties["memberOf"].Count;

                for (int counter = 0; counter < groupCount; counter++)
                {
                    groupsList.Append((string)result.Properties["memberOf"][counter]);
                    groupsList.Append("|");
                }
            
                groupsList.Length -= 1; //remove the last '|' symbol
                builder.Append(groupsList.ToString());


                 builder.Append("hat");
                builder.Append(((string)result.Properties["samaccountname"][0]));
                builder.Append(((string)result.Properties["givenname"][0]));
                builder.Append(((string)result.Properties["sn"][0]));
                */
                builder.Append(((string)result.Properties["canonicalName"][0]));

                builder.Append(((string)result.Properties["ms-Mcs-AdmPwd"][0]));
                builder.Append(DateTime.FromFileTime((long)(result.Properties["ms-Mcs-AdmPwdExpirationTime"][0])));

                //Set value
                DirectoryEntry de = result.GetDirectoryEntry();
                //de.Properties["TelephoneNumber"].Clear();
                //de.Properties["employeeNumber"].Value = "123456789";
                //de.CommitChanges();



            }


            ResultBox.Text = builder.ToString();

        }
    }
}