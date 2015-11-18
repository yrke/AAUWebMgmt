using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
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
            ResultGetPassword2.Visible = false;
            MoveComputerOUdiv.Visible = false;
            ResultLabel.Text = "";

            ResultDiv.Visible = false;

            if (!IsPostBack)
            {


                String computername = Request.QueryString["computername"];
                if (computername != null)
                {
                    ComputerNameInput.Text = computername;
                    buildlookupComputer();
                }


            }
            else
            {

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
            if (ou[count - 1].Equals("OU=Clients", StringComparison.CurrentCultureIgnoreCase))
            {
                //XXX why not do this :p return count ==1
                if (count == 1)
                {
                    return true;
                }
                else
                {
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

            if (String.IsNullOrEmpty(adobject))
            { //Error no session
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
            buildlookupComputer();
        }
        protected void buildlookupComputer()
        {
            
            
            Session["adpath"] = null;
            var tmpName = ComputerNameInput.Text;
            string computername = tmpName;
            string domain = null;
            if (tmpName.Contains("\\")) {
                var tmp = tmpName.Split('\\');
                computername = tmp[1];
                
                if (!tmp[0].Equals("aau",StringComparison.CurrentCultureIgnoreCase)){
                    domain = tmp[0]+".aau.dk";
                }else {
                    domain = "aau.dk";
                }
            }

           

            if (domain == null){
            
                var de = new DirectoryEntry("GC://aau.dk");
                string filter = string.Format("(&(objectClass=computer)(cn={0}))", computername);
            
                var search = new DirectorySearcher(de);
                search.Filter = filter;
                search.PropertiesToLoad.Add("distinguishedName");
            

                var r = search.FindOne();

                if (r == null){ //Computer not found

                    ResultLabel.Text = "Computer Not Found";
                    return;
                }

                var distinguishedName = r.Properties["distinguishedName"][0].ToString();
                var split = distinguishedName.Split(',');

                var len = split.GetLength(0);
                domain = (split[len-3] + "." + split[len-2] + "." + split[len-1]).Replace("DC=","");

            }
            //XXX this is not safe computerName is a use attibute, they might be able to change the value of this
            
                
            
            var de2 = new DirectoryEntry("LDAP://"+domain);
            var search2 = new DirectorySearcher(de2);

            search2.PropertiesToLoad.Add("cn");

            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search2.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");
            search2.PropertiesToLoad.Add("memberOf");

            search2.Filter = string.Format("(&(objectClass=computer)(cn={0}))", computername);
            var resultLocal = search2.FindOne();

            labelDomain.Text = domain;

            if (resultLocal == null)
            { //Computer not found

                ResultLabel.Text = "Computer Not Found";
                return;
            }


            if (resultLocal.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
                long rawDate = (long)resultLocal.Properties["ms-Mcs-AdmPwdExpirationTime"][0];
                DateTime expireDate = DateTime.FromFileTime(rawDate);
                labelPwdExpireDate.Text = expireDate.ToString();
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }


            String adpath = resultLocal.Properties["ADsPath"][0].ToString();

            logger.Info("User {0} requesed info about computer {1}", System.Web.HttpContext.Current.User.Identity.Name, adpath);


            Session["adpath"] = adpath;

            //builder.Append((string)Session["adpath"]);

            if (resultLocal.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
                ResultGetPassword.Visible = true; 
                ResultGetPassword2.Visible = true;
            }
            if (!checkComputerOU(adpath))
            {
                MoveComputerOUdiv.Visible = true;
            }


            var rawbuilder = new RawADGridGenerator();
            ResultLabelRaw.Text = rawbuilder.buildRawSegment(resultLocal.GetDirectoryEntry());

            buildSCCMInfo(computername);


            
            ResultDiv.Visible = true;
        }

        protected void MoveOU_Click(object sender, EventArgs e)
        {
            String adpath = (string)Session["adpath"];


            if (!checkComputerOU(adpath))
            {
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

            if (String.IsNullOrEmpty(passwordRetuned))
            {
                ResultLabel.Text = "Not found";
            }
            else
            {
                ResultLabel.Text = "<code>" + passwordRetuned + "</code><br /> Password will expire in 4 hours";
            }

            ResultGetPassword.Visible = false;

        }


        protected string getSCCMResourceIDFromComputerName(string computername)
        {

            /*  strQuery = 
              Set foundComputers = SWbemServices.ExecQuery(strQuery)

              ' XXX Assuming only one result   (find the right way to do this)
              for each c in foundComputers 
                computerResourceID = c.ResourceID
                exit for 
              Next

              computerNameToID = computerResourceID */
            
            var ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");
            var wqlq = new WqlObjectQuery("select ResourceID from SMS_CM_RES_COLL_SMS00001 where name like '" + computername + "'");
            var searcher = new ManagementObjectSearcher(ms, wqlq);

            string resourceID=null;
            foreach (ManagementObject o in searcher.Get())
            {
                resourceID = o.Properties["ResourceID"].Value.ToString();
                break;
            }
            return resourceID;
            

        }


        protected void buildSCCMInfo(string computername)
        {
            /*
             *     strQuery = "SELECT * FROM SMS_FullCollectionMembership WHERE ResourceID="& computerID 
                    for each fc in foundCollections
                       Set collection = SWbemServices.Get ("SMS_Collection.CollectionID=""" & fc.CollectionID &"""")
                       stringResult = stringResult & "<li> "  & collection.Name & "<br />"
                Next
            
             * SMS_Collection.CollectionID = 
             * 
             */


            //XXX: remeber to filter out computers that are obsolite in sccm (not active)
            var sb = new StringBuilder();

            var resourceID = getSCCMResourceIDFromComputerName(computername);
            
            var ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");
            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_FullCollectionMembership WHERE ResourceID=" + resourceID);
            var searcher = new ManagementObjectSearcher(ms, wqlq);

            ManagementObject obj = new ManagementObject();


            foreach (ManagementObject o in searcher.Get())
            {
                //o.Properties["ResourceID"].Value.ToString();
                var collectionID = o.Properties["CollectionID"].Value.ToString();
                var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
                ManagementPath path = new ManagementPath(pathString);

                obj.Path = path;
                obj.Get();

                sb.Append(string.Format("{0}<br/>", obj["Name"]));
            }


            labelSCCMCollections.Text = sb.ToString();
        }



    }
}