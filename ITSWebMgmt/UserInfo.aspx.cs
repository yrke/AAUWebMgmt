using SimpleImpersonation;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        string username = "USERNAME";
        public string UserName
        {
            get { return username; }
            set {username = value;}
        }

        protected void toggle_userprofile(object sender, EventArgs e)
        {

            DirectorySearcher search = new DirectorySearcher();
            search.Filter = String.Format("(cn={0})", "KYRKE"); //XXX This need to be unique!!
            search.PropertiesToLoad.Add("userPrincipalName");
            search.PropertiesToLoad.Add("profilePath");
            SearchResult result = search.FindOne();


            //Set value
            DirectoryEntry de = result.GetDirectoryEntry();
            de.Properties["TelephoneNumber"].Clear();
            //de.Properties["employeeNumber"].Value = "123456789";
            //de.CommitChanges();
        }


        protected string globalSearch(string email)
        {

            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            string filter = string.Format("(proxyaddresses=SMTP:{0})", email);


            DirectorySearcher search = new DirectorySearcher(de, filter);
            search.PropertiesToLoad.Add("userPrincipalName");
            SearchResult r = search.FindOne();


            if (r != null){ 
                return r.Properties["userPrincipalName"][0].ToString(); //XXX handle if result is 0 (null exception)
            } else {
                return null;
            }
        }

        protected void lookupUser(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            
            UserName = UserNameBox.Text;
            UserNameLabel.Text = UserName;


            var upn = globalSearch(UserName);
            if (upn == null)
            {
                builder.Append("User Not found");
                ResultLabel.Text = builder.ToString();
                return;
            }
            //builder.Append("Found UPN:" +  upn);
            var dc = upn.Split('@')[1];
            DirectoryEntry de = new DirectoryEntry("LDAP://"+dc);         

            var elements = "cn,userAccountControl,sAMAccountName,userPrincipalName,aauStudentID,aauStaffID,aauUserClassification,aauUserStatus,displayName,department,proxyAddresses,badPwdCount,badPasswordTime,lockoutTime,departmentNumber,homeDirectory,homeDrive,lastLogon,memberOf,profilePath,msDS-UserPasswordExpiryTimeComputed,msDS-User-Account-Control-Computed";
            //var elements = "sAMAccountName,userPrincipalName,aauStaffID";

            var listElements = elements.Split(',');

            DirectorySearcher search = new DirectorySearcher(de);
            search.PropertiesToLoad.AddRange(listElements);
            search.Filter = String.Format("(userPrincipalName={0})", upn);
            
            SearchResult result = search.FindOne();

            if (result != null)
            {
                
                //builder.Append((result.Properties["aauStaffID"][0])).ToString();

                foreach (string k in listElements)
                {
                    builder.Append(k);

                    var a = result.Properties[k];
                    if (a != null && a.Count > 0)
                    {
                        foreach (var j in a)
                        {
                            var value = (j.ToString());
                            builder.Append("  &nbsp;    &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;             : " + value);
                            builder.Append("<br />");
                        }
                    }
                    else { 
                        builder.Append("<br />");
                    }
                }

            }
            else
            {
                builder.Append("No user found");
            }
            ResultLabel.Text = builder.ToString();

        }

        protected void Page_Load(object sender, EventArgs e)
        {



        }
    }
}