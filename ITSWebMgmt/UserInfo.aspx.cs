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
            
            String phoneNr = Request.QueryString["phone"];
            if (phoneNr != null) {
                //Got a phonenumber to lookup
                string res = doPhoneSearch(phoneNr);
                if (res != null) { 

                    string url = Request.Url.AbsolutePath;
                    string updatedQueryString = "?" + "username=" + res;
                    Response.Redirect(url + updatedQueryString);
                } else
                {
                    //TODO: No user show error
                }

            }

        }

        //Searhces on a phone numer (internal or external), and returns a upn (later ADsPath) to a use or null if not found
         protected string doPhoneSearch(string numberIn)
         {
             string number = numberIn;
             //If number is a shot internal number, expand it :)
             if (number.Length == 4)
             {
                 // format is 3452
                 number = String.Format("+459940{0}", number);

             }
             else if (number.Length == 6)
             {
                 //format is +453452 
                 number = String.Format("+459940{0}", number.Replace("+45",""));

             }
             else if (number.Length == 8)
             {
                 //format is 99403442
                 number = String.Format("+45{0}", number);

             } // else format is ok

            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            string filter = string.Format("(&(objectCategory=person)(telephoneNumber={0}))", number);

            //logger.Debug(filter);

            DirectorySearcher search = new DirectorySearcher(de, filter);
            search.PropertiesToLoad.Add("userPrincipalName");
            SearchResult r = search.FindOne();


            if (r != null){
                //return r.Properties["ADsPath"][0].ToString(); //XXX handle if result is 0 (null exception)
                return r.Properties["userPrincipalName"][0].ToString(); 
            } else {
                return null;
            }

         }

         protected void buildgroupssegmentLabel(String[] groupsList)
         {
             StringBuilder sb = new StringBuilder();

             foreach (string adpath in groupsList) {
                 //DirectoryEntry de = new DirectoryEntry("LDAP://"+adpath);
                 
                 //string groupname = de.Properties["cn"][0].ToString();
                 //TODO:
                 //UserPrincipal user = UserPrincipal.FindByIdentity(new PrincipalContext (ContextType.Domain, "mydomain.com"), IdentityType.SamAccountName, "username");
                 //foreach (GroupPrincipal group in user.GetGroups())

                 var split = adpath.Split(',');
                 var groupname = split[0].Replace("CN=","");

                 sb.Append(groupname + "<br/>");
                 
             }


             groupssegmentLabel.Text = sb.ToString();
         
         }

         protected void buildFilesharessegmentLabel(String[] groupsList)
         {
             StringBuilder sb = new StringBuilder();
             foreach (string group in groupsList)
             {
                 
                 var split = group.Split(',');
                 var oupath = split.Where<string>(s => s.StartsWith("OU=")).ToArray<string>() ;
                 int count = oupath.Count();

                 if (count == 3 && oupath[count-1].Equals("OU=Groups") && oupath[count - 2].Equals("OU=Resource Access"))
                 {
                     //This is a group access group
                     var groupname = split[0].Replace("CN=", "");
                     sb.Append(groupname + "<br/>"); 
                 }
             }
             filesharessegmentLabel.Text = sb.ToString();

         }

         protected bool fixUserOu(String adpath)
         {
             
             DirectoryEntry de = new DirectoryEntry(adpath);

             String dn = (string)de.Properties["distinguishedName"][0];
             String[] dnarray = dn.Split(',');

             String[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray<String>();
             
             int count = ou.Count();

             if (count < 2)
             {
                 //This cant be in people/{staff,student,guest}
             }
             //Check root is people
             if (!(ou[count - 1]).Equals("ou=people", StringComparison.CurrentCultureIgnoreCase))
             {
                 //Error user is not placed in people!!!!! Cant move the user (might not be a real user or admin or computer)
                 return false;
             }  
             String[] okplaces = new String[3]{"ou=staff","ou=guests","ou=students"};
             if (!okplaces.Contains(ou[count - 2], StringComparer.OrdinalIgnoreCase))
             {
                 //Error user is not in out staff, people or student, what is gowing on here?
                 return false;
             }
             if (count > 2)
             {
                 //User is not placed in people/{staff,student,guest}, but in a sub ou, we need to do somthing!
                 //from above check we know the path is people/{staff,student,guest}, lets generate new OU

                 //Format ldap://DOMAIN/pathtoOU
                 //return false; //XX Return false here?

                 String[] adpathsplit = adpath.ToLower().Replace("ldap://", "").Split('/');
                 String protocol = "LDAP://";
                 String domain = adpathsplit[0];
                 String[] dcpath = (adpathsplit[1].Split(',')).Where<string>(s => s.StartsWith("DC=", StringComparison.CurrentCultureIgnoreCase)).ToArray<String>();

                 String newOU = String.Format("{0},{1}", ou[count - 2], ou[count - 1]);
                 String newPath = String.Format("{0}{1}/{2},{3}", protocol, domain, newOU, String.Join(",", dcpath));

                 logger.Info("user " +System.Web.HttpContext.Current.User.Identity.Name+" changed OU on user to: "+newPath + " from "+adpath+".");

                 var newLocaltion = new DirectoryEntry(newPath);
                 de.MoveTo(newLocaltion);

                 return true;
             }
             //We don't need to do anything, user is placed in the right ou! (we think, can still be in wrong ou fx a guest changed to staff, we cant check that here) 
             logger.Debug("no need to change user {0} out, all is good", adpath);    
             return true;
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
        protected void fixUserOUButton(object sender, EventArgs e)
        {
            fixUserOu((string)Session["adpath"]);
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

                var groupsList = result.Properties["memberOf"];
                var b = groupsList.Cast<string>();
                var groupListConvert = b.ToArray<string>();
                buildgroupssegmentLabel(groupListConvert);
                buildFilesharessegmentLabel(groupListConvert);

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