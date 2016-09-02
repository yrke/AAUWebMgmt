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
                    buildlookupComputer(computername.Trim());
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
        
        protected void buildlookupComputer(string computername)
        {
            
            
            Session["adpath"] = null;
            var tmpName = computername;


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

            String adpath = resultLocal.Properties["ADsPath"][0].ToString();
            Session["adpath"] = adpath;

            logger.Info("User {0} requesed info about computer {1}", System.Web.HttpContext.Current.User.Identity.Name, adpath);



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
            

            var rawbuilder = new RawADGridGenerator();
            ResultLabelRaw.Text = rawbuilder.buildRawSegment(resultLocal.GetDirectoryEntry());

            buildBasicInfo(resultLocal.GetDirectoryEntry());

            var resourceID = getSCCMResourceIDFromComputerName(computername); //XXX use ad path to get right object in sccm, also dont get obsolite
            //XXX check resourceID 
            buildSCCMInfo(resourceID);
            buildSCCMInventory(resourceID);


            buildGroupsSegments(resultLocal.GetDirectoryEntry());
           
            ResultDiv.Visible = true;


            if (!checkComputerOU(adpath))
            {
                MoveComputerOUdiv.Visible = true;
            }

        }

        public DateTime? convertADTimeToDateTime(object adsLargeInteger)
        {

            var highPart = (Int32)adsLargeInteger.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, adsLargeInteger, null);
            var lowPart = (Int32)adsLargeInteger.GetType().InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, adsLargeInteger, null);
            var result = highPart * ((Int64)UInt32.MaxValue + 1) + lowPart;

            if (result == 9223372032559808511)
            {
                return null;
            }

            return DateTime.FromFileTime(result);
        }

        protected void buildBasicInfo(DirectoryEntry de)
        {
            
            if (de.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
               
                DateTime? expireDate = convertADTimeToDateTime(de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value);
                labelPwdExpireDate.Text = expireDate.ToString();
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }


            

            //builder.Append((string)Session["adpath"]);

            if (de.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
                ResultGetPassword.Visible = true;
                ResultGetPassword2.Visible = true;
            }
            

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



        private void buildGroupsSegments(DirectoryEntry result)
        {
            //XXX is memeber of an attribute
            var groupsList = result.Properties["memberOf"];
            var b = groupsList.Cast<string>();
            var groupListConvert = b.ToArray<string>();

            var sb = new StringBuilder();

            foreach (string adpath in groupsList)
            {
         
                var split = adpath.Split(',');
                var groupname = split[0].Replace("CN=", "");

                sb.Append(String.Format("<a href=\"/GroupsInfo.aspx?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + adpath), groupname));
         
            }


            groupssegmentLabel.Text = sb.ToString();
         




        }


        protected void buildSCCMInventory(string resourceID)
        {
            // labelSCCMInventory
            // SELECT * FROM SMS_G_System_COMPUTER_SYSTEM WHERE ResourceID=16780075

            var sb = new StringBuilder();

            var ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");
            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_COMPUTER_SYSTEM WHERE ResourceID=" + resourceID);
            var searcher = new ManagementObjectSearcher(ms, wqlq);

            ManagementObject obj = new ManagementObject();
            var results = searcher.Get();


            var o = results.OfType<ManagementObject>().FirstOrDefault();
                


            foreach (var property in o.Properties)
            {
                string key = property.Name;
                object value = property.Value;

                int i = 0;
                if (value != null && value.GetType().IsArray)
                {
                    var arry = (string[])value;
                    foreach (string f in arry)
                    {
                        sb.Append(string.Format("{0}[{2}]: {1}<br />", key, f, i));
                        i++;
                    }
                }
                else
                {
                    sb.Append(string.Format("{0}: {1}<br />", key, property.Value));
                }

            }

            labelSCCMInventory.Text = sb.ToString();

        }
        protected void buildSCCMInfo(string resourceID)
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


            var ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");
            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_FullCollectionMembership WHERE ResourceID=" + resourceID);
            var searcher = new ManagementObjectSearcher(ms, wqlq);

            ManagementObject obj = new ManagementObject();
            var results = searcher.Get();

            bool hasValues=false;
            try
            {
                var t = results.Count;
                hasValues=true;
            }catch (ManagementException e){}

            string configPC = "Unknown";
            string configExtra = "False";
            string getsTestUpdates = "False";

            if (hasValues)
            {
                foreach (ManagementObject o in results)
                {
                    //o.Properties["ResourceID"].Value.ToString();
                    var collectionID = o.Properties["CollectionID"].Value.ToString();

                    if (collectionID.Equals("AA100015"))
                    {
                        configPC = "AAU PC";
                    }else if (collectionID.Equals("AA100087")){
                        configPC = "AAU8 PC";
                    }
                    else if (collectionID.Equals("AA1000BC"))
                    {
                        configPC = "AAU10 PC";
                        configExtra = "True"; // Hardcode AAU10 is bitlocker enabled
                    }
                    else if (collectionID.Equals("AA100027"))
                    {
                        configPC = "Administrativ PC";
                    }
                    else if (collectionID.Equals("AA10009C"))
                    {
                        configPC = "Imported";
                    }

                    if (collectionID.Equals("AA1000B8"))
                    {
                        configExtra = "True";
                    }

                    if (collectionID.Equals("AA100069") || collectionID.Equals("AA100066") || collectionID.Equals("AA100065") || collectionID.Equals("AA100064") || collectionID.Equals("AA100063") || collectionID.Equals("AA100083"))
                    {
                        getsTestUpdates = "True";
                    }

                    var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
                    ManagementPath path = new ManagementPath(pathString);

                    obj.Path = path;
                    obj.Get();

                    sb.Append(string.Format("{0}<br/>", obj["Name"]));
                }
            }
            else
            {
                sb.Append("Computer not found i SCCM");
            }

            labelBasicInfoPCConfig.Text = configPC;
            labelBasicInfoExtraConfig.Text = configExtra;
            labelBasicInfoTestUpdates.Text = getsTestUpdates;

            //Basal Info
            var wqlqSystem = new WqlObjectQuery("SELECT * FROM SMS_R_System WHERE ResourceId=" + resourceID);
            var searcherSystem = new ManagementObjectSearcher(ms, wqlqSystem);

            ManagementObject objSystem = new ManagementObject();
            var resultsSystem = searcherSystem.Get();

            hasValues = false;
            try
            {
                var t2 = resultsSystem.Count;
                hasValues = true;
            }
            catch (ManagementException e) {}

            sb.Append("<h3>Computer Details</h3>");

            if (hasValues)
            {

                
                foreach (ManagementObject o in resultsSystem) //Has one!
                {
                    //OperatingSystemNameandVersion = Microsoft Windows NT Workstation 6.1

                    foreach (var property in o.Properties)
                    {
                        string key = property.Name;
                        object value = property.Value;

                        int i = 0;
                        string[] arry = null;
                        if (value != null && value.GetType().IsArray)
                        {
                            if (value is string[]){ 
                                arry = (string[])value;
                            } else {
                                arry = new string[]{ "none-string value" }; //XXX get the byte value
                            }
                            foreach (string f in arry)
                            {
                                sb.Append(string.Format("{0}[{2}]: {1}<br />", key, f, i));
                                i++;
                            }
                        }
                        else
                        {
                            sb.Append(string.Format("{0}: {1}<br />", key, property.Value));
                        }
                        
                    }

                }
            }
            else
            {
                sb.Append("Computer not found i SCCM");
            }


            labelSCCMCollections.Text = sb.ToString();
        }


        protected void addComputerToCollection(string resourceID, string collectionID)
        {
              /*  Set collection = SWbemServices.Get ("SMS_Collection.CollectionID=""" & CollID &"""")
  
                  Set CollectionRule = SWbemServices.Get("SMS_CollectionRuleDirect").SpawnInstance_()
                  CollectionRule.ResourceClassName = "SMS_R_System"
                  CollectionRule.RuleName = "Static-"&ResourceID
                  CollectionRule.ResourceID = ResourceID
                  collection.AddMembershipRule CollectionRule*/

            //o.Properties["ResourceID"].Value.ToString();

            var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
            ManagementPath path = new ManagementPath(pathString);
            ManagementObject obj = new ManagementObject(path);

            ManagementClass ruleClass = new ManagementClass("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_CollectionRuleDirect");

            ManagementObject rule = ruleClass.CreateInstance();

            rule["RuleName"] = "Static-"+ resourceID;
            rule["ResourceClassName"] = "SMS_R_System";
            rule["ResourceID"] = resourceID;

            obj.InvokeMethod("AddMembershipRule", new object[] { rule });

        }
 
        
        protected void buttonEnableBitlockerEncryption_Click(object sender, EventArgs e)
        {
            string adpath = (string)Session["adpath"];
            string[] adpathsplit = adpath.Split('/');
            string computerName = (adpathsplit[adpathsplit.Length-1].Split(','))[0].Replace("CN=", "");

            var resourceID = getSCCMResourceIDFromComputerName(computerName);
            var collectionID = "AA1000B8"; //Enabled Bitlocker Encryption Collection ID
            addComputerToCollection(resourceID, collectionID);

        }



    }
}