using ITSWebMgmt.Computer;
using ITSWebMgmt.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Web;

namespace ITSWebMgmt.Controllers.Computer
{
    public class ComputerController
    {
        public string ResourceID;
        public string adpath;
        public string ComputerName = "ITS\\AAU804396";
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private SCCMcache SCCMcache;
        public string ConfigPC = "Unknown";
        public string ConfigExtra = "False";
        //TODO getsTestUpdates not used
        public string Domain;
        public ManagementObjectCollection RAM { get => SCCMcache.RAM; private set { } }
        public ManagementObjectCollection LogicalDisk { get => SCCMcache.LogicalDisk; private set { } }
        public ManagementObjectCollection BIOS { get => SCCMcache.BIOS; private set { } }
        public ManagementObjectCollection VideoController { get => SCCMcache.VideoController; private set { } }
        public ManagementObjectCollection Processor { get => SCCMcache.Processor; private set { } }
        public ManagementObjectCollection Disk { get => SCCMcache.Disk; private set { } }
        public ManagementObjectCollection Software { get => SCCMcache.Software; private set { } }
        public ManagementObjectCollection Computer { get => SCCMcache.Computer; private set { } }
        public ManagementObjectCollection Antivirus { get => SCCMcache.Antivirus; private set { } }
        public ManagementObjectCollection System { get => SCCMcache.System; private set { } }
        public ManagementObjectCollection Collection { get => SCCMcache.Collection; private set { } }
        
        public ComputerController(string computername)
        {
            //XXX this is not safe computerName is a use attibute, they might be able to change the value of this
            SCCMcache = new SCCMcache();
            getDomain();
            ResourceID = getSCCMResourceIDFromComputerName();
            SCCMcache.ResourceID = ResourceID;
        }

        public string getSCCMResourceIDFromComputerName()
        {
            string resourceID = "";
            //XXX use ad path to get right object in sccm, also dont get obsolite
            foreach (ManagementObject o in SCCMcache.getResourceIDFromComputerName(ComputerName))
            {
                resourceID = o.Properties["ResourceID"].Value.ToString();
                break;
            }

            return resourceID;
        }

        private string getDomain()
        {
            var tmpName = ComputerName;

            if (tmpName.Contains("\\"))
            {
                var tmp = tmpName.Split('\\');
                ComputerName = tmp[1];

                if (!tmp[0].Equals("aau", StringComparison.CurrentCultureIgnoreCase))
                {
                    Domain = tmp[0] + ".aau.dk";
                }
                else
                {
                    Domain = "aau.dk";
                }
            }

            if (Domain == null)
            {
                var de = new DirectoryEntry("GC://aau.dk");
                string filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);

                var search = new DirectorySearcher(de);
                search.Filter = filter;
                search.PropertiesToLoad.Add("distinguishedName");

                var r = search.FindOne();

                if (r == null)
                { //Computer not found

                    return null;
                }

                var distinguishedName = r.Properties["distinguishedName"][0].ToString();
                var split = distinguishedName.Split(',');

                var len = split.GetLength(0);
                Domain = (split[len - 3] + "." + split[len - 2] + "." + split[len - 1]).Replace("DC=", "");
            }

            return Domain;
        }

        public SearchResult GetSearch(string username)
        {
            var de2 = new DirectoryEntry("LDAP://" + Domain);
            var search2 = new DirectorySearcher(de2);

            search2.PropertiesToLoad.Add("cn");

            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search2.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");
            search2.PropertiesToLoad.Add("memberOf");

            search2.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);
            var resultLocal = search2.FindOne();

            if (resultLocal == null)
            { //Computer not found
                return null;
            }

            adpath = resultLocal.Properties["ADsPath"][0].ToString();

            logger.Info("User {0} requesed info about computer {1}", username, adpath);

            return resultLocal;
        }

        public string getLocalAdminPassword(string adobject)
        {
            if (string.IsNullOrEmpty(adobject))
            { //Error no session
                return null;
            }

            DirectoryEntry de = new DirectoryEntry(adobject);

            //XXX if expire time i smaller than 4 hours, you can use this to add time to the password (eg 3h to expire will become 4), never allow a password expire to be larger than the old value

            if (de.Properties.Contains("ms-Mcs-AdmPwd"))
            {
                var f = (de.Properties["ms-Mcs-AdmPwd"][0]).ToString();

                DateTime expiredate = (DateTime.Now).AddHours(8);
                string value = expiredate.ToFileTime().ToString();
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

        public void moveOU(string user)
        {
            if (!checkComputerOU())
            {
                //OU is wrong lets calulate the right one
                string[] adpathsplit = adpath.ToLower().Replace("ldap://", "").Split('/');
                string protocol = "LDAP://";
                string Domain = adpathsplit[0];
                string[] dcpath = (adpathsplit[1].Split(',')).Where<string>(s => s.StartsWith("DC=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();

                string newOU = string.Format("OU=Clients");
                string newPath = string.Format("{0}{1}/{2},{3}", protocol, Domain, newOU, string.Join(",", dcpath));

                logger.Info("user " + user + " changed OU on user to: " + newPath + " from " + adpath + ".");
                moveComputerToOU(adpath, newPath);

            }
            else
            {
                logger.Debug("computer " + adpath + " is in the right ou");
            }
        }

        protected void moveComputerToOU(string adpath, string newOUpath)
        {
            //Important that LDAP:// is in upper case ! 
            DirectoryEntry de = new DirectoryEntry(adpath);
            var newLocaltion = new DirectoryEntry(newOUpath);
            de.MoveTo(newLocaltion);
            de.Close();
            newLocaltion.Close();
        }

        public bool checkComputerOU()
        {
            //Check OU and fix it if wrong (only for clients sub folders or new clients)
            //Return true if in right ou (or we think its the right ou, or dont know)
            //Return false if we need to move the ou.

            DirectoryEntry de = new DirectoryEntry(adpath);

            string dn = (string)de.Properties["distinguishedName"][0];
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();
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

        public string setConfig()
        {
            if (Database.HasValues(Collection))
            {
                foreach (ManagementObject o in Collection)
                {
                    //o.Properties["ResourceID"].Value.ToString();
                    var collectionID = o.Properties["CollectionID"].Value.ToString();

                    if (collectionID.Equals("AA100015"))
                    {
                        ConfigPC = "AAU7 PC";
                    }
                    else if (collectionID.Equals("AA100087"))
                    {
                        ConfigPC = "AAU8 PC";
                    }
                    else if (collectionID.Equals("AA1000BC"))
                    {
                        ConfigPC = "AAU10 PC";
                        ConfigExtra = "True"; // Hardcode AAU10 is bitlocker enabled
                    }
                    else if (collectionID.Equals("AA100027"))
                    {
                        ConfigPC = "Administrativ7 PC";
                    }
                    else if (collectionID.Equals("AA1001BD"))
                    {
                        ConfigPC = "Administrativ10 PC";
                        ConfigExtra = "True"; // Hardcode AAU10 is bitlocker enabled
                    }
                    else if (collectionID.Equals("AA10009C"))
                    {
                        ConfigPC = "Imported";
                    }

                    if (collectionID.Equals("AA1000B8"))
                    {
                        ConfigExtra = "True";
                    }

                    var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
                    ManagementPath path = new ManagementPath(pathString);
                    ManagementObject obj = new ManagementObject();

                    obj.Path = path;
                    obj.Get();

                    return obj["Name"].ToString();
                }
            }
            return null;
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

            rule["RuleName"] = "Static-" + resourceID;
            rule["ResourceClassName"] = "SMS_R_System";
            rule["ResourceID"] = resourceID;

            obj.InvokeMethod("AddMembershipRule", new object[] { rule });
        }

        public void EnableBitlockerEncryption()
        {
            string[] adpathsplit = adpath.Split('/');
            string computerName = (adpathsplit[adpathsplit.Length - 1].Split(','))[0].Replace("CN=", "");

            var collectionID = "AA1000B8"; //Enabled Bitlocker Encryption Collection ID
            addComputerToCollection(ResourceID, collectionID);
        }
    }
}