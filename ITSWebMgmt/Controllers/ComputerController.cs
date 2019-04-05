using System;
using System.Collections.Generic;
using System.Linq;
using ITSWebMgmt.Caches;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Management;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using System.Net;
using ITSWebMgmt.Functions;
using Microsoft.Extensions.Caching.Memory;

namespace ITSWebMgmt.Controllers
{
    public class ComputerController : WebMgmtController<ComputerADcache>
    {
        public IActionResult Index(string computername)
        {
            if (computername != null)
            {
                if (!_cache.TryGetValue(computername, out ComputerModel))
                {
                    ComputerModel = new ComputerModel(this, computername);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(computername, ComputerModel, cacheEntryOptions);
                }
            }
            else
            {
                ComputerModel = new ComputerModel(this, computername);
            }

            return View(ComputerModel);
        }

        private IMemoryCache _cache;
        public ComputerModel ComputerModel;
        public string ResourceID;
        public string ConfigPC = "Unknown";
        public string ConfigExtra = "False";

        public ComputerController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult LoadTab(string tabName, string computername)
        {
            ComputerModel = _cache.Get<ComputerModel>(computername);
            switch (tabName)
            {
                case "groups":
                    ComputerModel.buildGroupsSegments();
                    break;
                case "sccmInventory":
                    ComputerModel.buildSCCMInventory();
                    break;
                case "sccmAV":
                    ComputerModel.buildSCCMAntivirus();
                    break;
                case "sccmHW":
                    ComputerModel.biuldSCCMHardware();
                    return PartialView("SCCMHW", ComputerModel);
                case "rawdata":
                    ComputerModel.buildRaw();
                    return PartialView("Raw", ComputerModel);
            }
            return PartialView(tabName, ComputerModel);
        }

        internal bool computerIsInRightOU(string dn)
        {
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray();

            int count = ou.Count();

            //Check root is people
            if ((ou[0]).Equals("OU=Clients", StringComparison.CurrentCultureIgnoreCase))
            {
                //Computer should be in OU Clients
                return true;
            }

            return false;
        }

        public static string getLocalAdminPassword(string adpath)
        {
            if (string.IsNullOrEmpty(adpath))
            { //Error no session
                return null;
            }

            DirectoryEntry de = new DirectoryEntry(adpath);

            //XXX if expire time is smaller than 4 hours, you can use this to add time to the password (eg 3h to expire will become 4), never allow a password expire to be larger than the old value

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

        public void moveOU(string user, string adpath)
        {
            if (!checkComputerOU(adpath))
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

        public bool checkComputerOU(string adpath)
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

        public List<string> setConfig(ManagementObjectCollection Collection)
        {
            if (Database.HasValues(Collection))
            {
                List<string> namesInCollection = new List<string>();
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

                    namesInCollection.Add(obj["Name"].ToString());
                }
                return namesInCollection;
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

        public void EnableBitlockerEncryption(string adpath)
        {
            string[] adpathsplit = adpath.Split('/');
            string computerName = (adpathsplit[adpathsplit.Length - 1].Split(','))[0].Replace("CN=", "");

            var collectionID = "AA1000B8"; //Enabled Bitlocker Encryption Collection ID
            addComputerToCollection(ResourceID, collectionID);
        }
    }
}