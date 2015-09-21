using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        string username = "USERNAME";
        public string UserName
        {
            get { return username; }
            set {username = value;}
        }

         protected void Page_Load(object sender, EventArgs e)
        {
            String username = Request.QueryString["username"];
            if (username != null)
            {
                UserNameBox.Text = username;
            }
            ResultDiv.Visible = false;
            
        }

        


        protected void toggle_userprofile(String adpath)
        {
            
            DirectoryEntry de = new DirectoryEntry(adpath);

            //String profilepath = (string)(de.Properties["profilePath"])[0];

            
            if (de.Properties.Contains("profilepath"))
            {
                de.Properties["profilePath"].Clear();
                de.CommitChanges();
            }
            else
            {
                String upn = ((string)de.Properties["userPrincipalName"][0]);
                var tmp = upn.Split('@');

                string path = String.Format("\\\\{0}\\profiles\\{1}", tmp[1], tmp[0]);

                de.Properties["profilePath"].Add(path);
                de.CommitChanges();
            }
        }

        protected void button_toggle_userprofile(object sender, EventArgs e)
        {
            String adpath = (String)Session["adpath"];

            
            //XXX log what the new value of profile is :)
            logger.Info("User {0} toggled romaing profile for user  {1}", System.Web.HttpContext.Current.User.Identity.Name, adpath);

            
            toggle_userprofile(adpath);


            //Set value
            //DirectoryEntry de = result.GetDirectoryEntry();
            //de.Properties["TelephoneNumber"].Clear();
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
            
            //XXX, this is a use input, might not be save us use in log 
            logger.Info("User {0} lookedup user {1}", System.Web.HttpContext.Current.User.Identity.Name, UserName);

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

                builder.Append("<table><tr><th>k</th><th>v</th></tr>");
                
                foreach (string k in listElements)
                {
                    builder.Append("<tr>");
                    builder.Append("<td>"+k+"</td>");

                    var a = result.Properties[k];
                    if (a != null && a.Count > 0)
                    {
                        if (a.Count == 1) {
                            var v = a[0];
                            builder.Append("<td>"+v+"</td></tr>");
                        } else {

                            var v = a[0];
                            builder.Append("<td>"+v+"</td>");
                            for (int i = 1; i < a.Count; i++ ){
                                v = a[i];
                                builder.Append("<tr><td></td><td>"+v+"</td></tr>");
                            }

                        }

                    } else {
                        builder.Append("<td></td></tr>");
                    }

                }
                
                builder.Append("</table>");

                displayName.Text = result.Properties["displayName"][0].ToString();

            }
            else
            {
                builder.Append("No user found");
            }
            ResultLabel.Text = builder.ToString();

            //Save user in session
            String adpath = result.Properties["ADsPath"][0].ToString();
            Session["adpath"] = adpath;

            ResultDiv.Visible = true;

        }

        
    }
}