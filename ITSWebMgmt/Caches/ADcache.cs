using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Computer
{
    public class ADcache
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public string adpath;
        public string ComputerName = "ITS\\AAU804396";
        public bool ComputerFound = false;
        public string Domain;
        private DirectoryEntry DE;

        public object AdmPwdExpirationTime;
        public ADcache(string computerName, string userName)
        {
            ComputerName = computerName;
            var de = new DirectoryEntry("LDAP://" + getDomain());
            var search = new DirectorySearcher(de);

            search.PropertiesToLoad.Add("cn");
            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");
            search.PropertiesToLoad.Add("memberOf");

            search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);
            var resultLocal = search.FindOne();

            if (resultLocal == null)
            { //Computer not found
                return;
            }

            ComputerFound = true;

            saveDataFromDataBase(de, resultLocal);

            adpath = adpath = resultLocal.Properties["ADsPath"][0].ToString();

            DE = de;

            logger.Info("User {0} requesed info about computer {1}", userName, adpath);
        }

        public List<PropertyValueCollection> getAllProperties()
        {
            List<PropertyValueCollection> propertyValueCollections = new List<PropertyValueCollection>();
            foreach (string k in DE.Properties.PropertyNames)
            {
                propertyValueCollections.Add(DE.Properties[k]);
            }
            return propertyValueCollections;
        }

        public string[] getGroups(string name)
        {
            return DE.Properties[name].Cast<string>().ToArray();
        }

        public string[] getGroupsTransitive(string name)
        {
            string attName = $"msds-{name}Transitive";
            DE.RefreshCache(attName.Split(','));
            return DE.Properties[attName].Cast<string>().ToArray();
        }

        private void saveDataFromDataBase(DirectoryEntry de, SearchResult resultLocal)
        {
            addProperty("managedBy", de.Properties["managedBy"].Value);
            addProperty("cn", resultLocal.Properties["cn"]);
            addProperty("memberOf", resultLocal.Properties["memberOf"]);
            addProperty("ms-Mcs-AdmPwdExpirationTime", de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value);
        }

        public object getProperty(string property)
        {
            if (properties.ContainsKey(property))
            {
                return properties[property];
            }
            return null;
        }

        public void addProperty(string property, object value)
        {
            properties.Add(property, value);
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
    }
}