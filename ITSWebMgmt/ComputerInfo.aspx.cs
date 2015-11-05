using NLog;
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
    public partial class ComputerInfo : System.Web.UI.Page
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            ResultGetPassword.Visible = false;
            MoveComputerOUdiv.Visible = false;
            String computername = Request.QueryString["computername"];
            if (computername != null)
            {
                ComputerNameInput.Text = computername;
            }
            
        }
        protected void moveComputerToOU(String adpath, String newOUpath)
        {
            //Important that LDAP:// is in upper case ! 
            DirectoryEntry de = new DirectoryEntry(adpath);
            var newLocaltion = new DirectoryEntry(newOUpath);
            de.MoveTo(newLocaltion);
            de.Close();
            newLocaltion.Close();

        }
        protected bool checkComputerOU(String adpath)
        {
            //Check OU and fix it if wrong (only for clients sub folders or new clients)
            //Return true if in right ou (or we think its the right ou, or dont know)
            //Return false if we need to move the ou.


            DirectoryEntry de = new DirectoryEntry(adpath);

            String dn = (string)de.Properties["distinguishedName"][0];
            String[] dnarray = dn.Split(',');

            String[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray<String>();
            int count = ou.Count();

            //Check if topou is clients (where is should be)
            if (ou[count -1].Equals("OU=Clients", StringComparison.CurrentCultureIgnoreCase)) {
                //XXX why not do this :p return count ==1
                if (count == 1)  {
                    return true;
                }else {
                    //Is in a sub ou for clients, we need to move it
                    return false;
                }
            }
            else
            {
                //Is now in Clients, is it in new computere => move to clients else we don't know where to place it (it might be a server)
                if (ou[count - 1].Equals("OU=New Computers", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            //return true;

        }

        protected String getLocalAdminPassword(String adobject)
        {

            if (String.IsNullOrEmpty(adobject)){ //Error no session
                return null;
            }

            DirectoryEntry de = new DirectoryEntry(adobject);

            //XXX if expire time i smaller than 4 hours, you can use this to add time to the password (eg 3h to expire will become 4), never allow a password expire to be larger than the old value

            if (de.Properties.Contains("ms-Mcs-AdmPwd"))
            {
                var f = (de.Properties["ms-Mcs-AdmPwd"][0]).ToString();

                DateTime expiredate = (DateTime.Now).AddHours(4);
                String value = expiredate.ToFileTime().ToString();
                de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value = value;
                de.CommitChanges();
             
                return f;

            }
            else
            {
                return null;
            }

            
            //DirectorySearcher search = new DirectorySearcher(de);
            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            //SearchResult r = search.FindOne();

            //if (r != null && r.Properties.Contains("ms-Mcs-AdmPwd"))
            //{
            //    return r.Properties["ms-Mcs-AdmPwd"][0].ToString();

                //DirectoryEntry  = r.GetDirectoryEntry();
                //de


            //}
            //else { 
            //    return null;
            //}
        }

        protected void lookupComputer(object sender, EventArgs e)
        {
            
            
            Session["adpath"] = null;
            var computerName = ComputerNameInput.Text;

            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            string filter = string.Format("(&(objectClass=computer)(cn={0}))", computerName);
            //string filter = string.Format("(name={0})", computerName);


            DirectorySearcher search = new DirectorySearcher(de);
            //DirectorySearcher search = new DirectorySearcher(filter);
            search.Filter = filter;
            search.PropertiesToLoad.Add("distinguishedName");
            

            SearchResult r = search.FindOne();

            if (r == null){ //Computer not found

                ResultLabel.Text = "Computer Not Found";
                return;
            }
            var distinguishedName = r.Properties["distinguishedName"][0].ToString();
            var split = distinguishedName.Split(',');

            var len = split.GetLength(0);
            var domain = (split[len-3] + "." + split[len-2] + "." + split[len-1]).Replace("DC=","");

            //XXX this is not safe computerName is a use attibute, they might be able to change the value of this
            logger.Info("User {0} requesed info about computer {1}", System.Web.HttpContext.Current.User.Identity.Name, (split[len - 3]).Replace("DC=", "") + "\\" + computerName);

            de = new DirectoryEntry("LDAP://"+domain);
            //de.Username = "its\\kyrke";
            //de.Password = "";
            search.PropertiesToLoad.Add("cn");

            search = new DirectorySearcher(de);
            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");
            search.PropertiesToLoad.Add("memberOf");

            search.Filter = string.Format("(&(objectClass=computer)(distinguishedName={0}))", distinguishedName);

            var builder = new StringBuilder();
            r = search.FindOne();
            if (r != null)
            {

                //builder.Append((result.Properties["aauStaffID"][0])).ToString();

                foreach (string k in search.PropertiesToLoad)
                {
                    builder.Append(k);

                    var a = r.Properties[k];
                    if (a != null && a.Count > 0)
                    {
                        foreach (var j in a){
                            var value = (j.ToString());
                            builder.Append(" : " + value);
                        }
                    }
                    builder.Append("<br />");
                }

            }

            //long rawDate = (long)r.Properties["ms-Mcs-AdmPwdExpirationTime"][0];
            //DateTime expireDate = DateTime.FromFileTime(rawDate);
            //builder.Append(expireDate);


            String adpath = r.Properties["ADsPath"][0].ToString();

            Session["adpath"] = adpath;

            //builder.Append((string)Session["adpath"]);

            ResultLabel.Text = builder.ToString();

            if (r.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            { 
                ResultGetPassword.Visible = true;
            }
            if (!checkComputerOU(adpath))
            {
                MoveComputerOUdiv.Visible = true;
            }
        }

        protected void MoveOU_Click(object sender, EventArgs e)
        {
            String adpath = (string)Session["adpath"];


            if (!checkComputerOU(adpath)) {
                //OU is wrong lets calulate the right one
                String[] adpathsplit = adpath.ToLower().Replace("ldap://", "").Split('/');
                String protocol = "LDAP://";
                String domain = adpathsplit[0];
                String[] dcpath = (adpathsplit[1].Split(',')).Where<string>(s => s.StartsWith("DC=", StringComparison.CurrentCultureIgnoreCase)).ToArray<String>();

                String newOU = String.Format("OU=Clients");
                String newPath = String.Format("{0}{1}/{2},{3}", protocol, domain, newOU, String.Join(",", dcpath));

                logger.Info("user " + System.Web.HttpContext.Current.User.Identity.Name + " changed OU on user to: " + newPath + " from " + adpath + ".");
                moveComputerToOU(adpath, newPath);

            }
            else
            {
                logger.Debug("computer " + adpath + " is in the right ou"); 
            }

            

        }

        protected void ResultGetPassword_Click(object sender, EventArgs e)
        {


            

            String adpath = (string)Session["adpath"];

            logger.Info("User {0} requesed localadmin password for computer {1}", System.Web.HttpContext.Current.User.Identity.Name, adpath);

            var passwordRetuned = this.getLocalAdminPassword(adpath);

            if (String.IsNullOrEmpty(passwordRetuned)) { 
                ResultLabel.Text = "Not found";
            }
            else
            {
                ResultLabel.Text = "<code>"+passwordRetuned + "</code><br /> Password will expire in 4 hours";
            }

            ResultGetPassword.Visible = false;
        
        }

        
    }
}