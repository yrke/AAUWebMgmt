using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class ComputerInfo : System.Web.UI.Page
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        protected string ComputerName = "ITS\\AAU804396";

        public void Page_Init(object o, EventArgs e)
        {
            
        }

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
                    ComputerName = HttpUtility.HtmlEncode(computername);
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

                DateTime expiredate = (DateTime.Now).AddHours(8);
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
            buildSCCMAntivirus(resourceID);

            buildGroupsSegments(resultLocal.GetDirectoryEntry());
           
            ResultDiv.Visible = true;


            if (!checkComputerOU(adpath))
            {
                MoveComputerOUdiv.Visible = true;
            }

        }


        protected void buildBasicInfo(DirectoryEntry de)
        {
            
            if (de.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
               
                DateTime? expireDate = ADHelpers.convertADTimeToDateTime(de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value);
                labelPwdExpireDate.Text = expireDate.ToString();
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }


            //Managed By
            labelManagedBy.Text = "none";

            if (de.Properties.Contains("managedBy"))
            {
                string managerVal = de.Properties["managedBy"].Value.ToString();

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelpers.DistinguishedNameToUPN(managerVal);
                    labelManagedBy.Text = email;
                }
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
                Func<string, string, string> appendColor = (string x, string color) => { return "<font color=\"" + color + "\">" + x + "</font>"; };

                string passwordWithColor = "";
                foreach (char c in passwordRetuned)
                {
                    var color = "green";
                    if (char.IsNumber(c))
                    {
                        color = "blue";
                    }

                    passwordWithColor += appendColor(c.ToString(), color);

                }

                ResultLabel.Text = "<code>" + passwordWithColor + "</code><br /> Password will expire in 8 hours";
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

            string resourceID=null;
            foreach (ManagementObject o in DatabaseGetter.getResults(new WqlObjectQuery("select ResourceID from SMS_CM_RES_COLL_SMS00001 where name like '" + computername + "'")))
            {
                resourceID = o.Properties["ResourceID"].Value.ToString();
                break;
            }
            return resourceID;
        }

        private void buildGroupsSegments(DirectoryEntry result)
        {
            //XXX is memeber of an attribute
            Helpers.GroupTableGenerator.BuildGroupsSegments("memberOf", result, groupssegmentLabel, groupsAllsegmentLabel);
        }


        protected void buildSCCMInventory(string resourceID)
        {
            // labelSCCMInventory
            // SELECT * FROM SMS_G_System_COMPUTER_SYSTEM WHERE ResourceID=16780075
            List<string> interestingKeys = new List<string>() {"Manufacturer", "Model", "SystemType", "Roles"};

            #region Software Info class
            /*
            [DisplayName("Installed Software"), dynamic: ToInstance, provider("ExtnProv")]
            class SMS_G_System_INSTALLED_SOFTWARE : SMS_G_System_Current
            {
                [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
                string ARPDisplayName;
                string ChannelCode;
                string ChannelID;
                string CM_DSLID;
                string EvidenceSource;
                datetime InstallDate;
                uint32 InstallDirectoryValidation;
                string InstalledLocation;
                string InstallSource;
                uint32 InstallType;
                uint32 Language;
                string LocalPackage;
                string MPC;
                uint32 OsComponent;
                string PackageCode;
                string ProductID;
                string ProductName;
                string ProductVersion;
                string Publisher;
                string RegisteredUser;
                string ServicePack;
                string SoftwareCode;
                string SoftwarePropertiesHash;
                string SoftwarePropertiesHashEx;
                string UninstallString;
                string UpgradeCode;
                uint32 VersionMajor;
                uint32 VersionMinor;
            };
            */
            #endregion

            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_COMPUTER_SYSTEM WHERE ResourceID=" + resourceID);
            var wqlqSoftware = new WqlObjectQuery("SELECT * FROM SMS_G_System_INSTALLED_SOFTWARE WHERE ResourceID=" + resourceID);

            var tableAndList = DatabaseGetter.CreateTableAndRawFromDatabase(wqlq, interestingKeys, "No inventory data");
            labelSSCMInventoryTable.Text = tableAndList.Item1; //Table
            labelSCCMCollecionsSoftware.Text = DatabaseGetter.CreateTableFromDatabase(wqlqSoftware,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found");
            labelSCCMInventory.Text += tableAndList.Item2; //List
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
           
            ManagementObject obj = new ManagementObject();
            var results = DatabaseGetter.getResults(new WqlObjectQuery("SELECT * FROM SMS_FullCollectionMembership WHERE ResourceID=" + resourceID));

            string configPC = "Unknown";
            string configExtra = "False";
            string getsTestUpdates = "False";

            HTMLTableHelper groupTableHelper = new HTMLTableHelper(new string[] { "Collection Name" });
            if (DatabaseGetter.HasValues(results))
            {
                foreach (ManagementObject o in results)
                {
                    
                    //o.Properties["ResourceID"].Value.ToString();
                    var collectionID = o.Properties["CollectionID"].Value.ToString();

                    if (collectionID.Equals("AA100015"))
                    {
                        configPC = "AAU7 PC";
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
                        configPC = "Administrativ7 PC";
                    }
                    else if (collectionID.Equals("AA1001BD"))
                    {
                        configPC = "Administrativ10 PC";
                        configExtra = "True"; // Hardcode AAU10 is bitlocker enabled
                    }
                    else if (collectionID.Equals("AA10009C"))
                    {
                        configPC = "Imported";
                    }

                    if (collectionID.Equals("AA1000B8"))
                    {
                        configExtra = "True";
                    }

                    var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
                    ManagementPath path = new ManagementPath(pathString);

                    obj.Path = path;
                    obj.Get();
                    
                    groupTableHelper.AddRow(new string[] { obj["Name"].ToString() });
                }
            }
            else
            {
                sb.Append("Computer not found i SCCM");
            }

            labelBasicInfoPCConfig.Text = configPC;
            labelBasicInfoExtraConfig.Text = configExtra;

            //Basal Info
            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_R_System WHERE ResourceId=" + resourceID);
            List<string> interestingKeys = new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build", "Config" };
            var tableAndList = DatabaseGetter.CreateTableAndRawFromDatabase(wqlq, interestingKeys, "Computer not found i SCCM");

            labelSCCMComputers.Text = sb.ToString() + groupTableHelper.GetTable();
            labelSCCMCollecionsTable.Text = tableAndList.Item1; //Table
            labelSCCMCollections.Text = tableAndList.Item2; //List
        }

        protected void buildSCCMAntivirus(string resourceID)
        {
            #region Antivirus Info class
            /*             
                instance of SMS_G_System_Threats
                {
                    ActionSuccess = TRUE;
                    ActionTime = "20181206092351.150000+***";
                    Category = "";
                    CategoryID = 27;
                    CleaningAction = 2;
                    DetectionID = "{04155F79-EB84-4828-9CEC-AC0749C4EDA6}";
                    DetectionSource = 3;
                    DetectionTime = "20181206092345.703000+***";
                    ErrorCode = -2142207965;
                    ExecutionStatus = 1;
                *    Path = "file:_C:\\OneDriveTemp\\S-1-5-21-1950982312-1110734968-986239597-1661\\2ce71f67d80e4f848ce20184ec987e52-8c19be29fc224df0aa8af25df55650dd-33f2835be60c42e1812107e28db565e3-d250bdce6b36dbf4e5459435ccabee25f9b05e46.temp";
                *    PendingActions = 0;
                *    Process = "C:\\Users\\lat\\AppData\\Local\\Microsoft\\OneDrive\\OneDrive.exe";
                    ProductVersion = "4.18.1810.5";
                    ResourceID = 16787705;
                    Severity = "";
                *    SeverityID = 5;
                    ThreatID = "227086";
                *    ThreatName = "PUA:Win32/Reimage";
                *    UserName = "ET\\lat";
                };
            */
            #endregion

            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_Threats WHERE ResourceID=" + resourceID);
            //DetectionID is requaired for UserName (SELECT * FROM SMS_G_System_Threats WHERE DetectionID='{04155F79-EB84-4828-9CEC-AC0749C4EDA6}' AND ResourceID=16787705)
            //Only few computers with data, one them is AAU112782
            labelSCCMAV.Text = DatabaseGetter.CreateTableFromDatabase(wqlq,
                new List<string>() { "ThreatName", "PendingActions", "Process", "SeverityID", "Path" },
                "Antivirus information not found");
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