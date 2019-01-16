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
        private string adpath;
        public string Domain;

        public object AdmPwdExpirationTime;
        public ADcache(string computerName, string userName)
        {
            var de = new DirectoryEntry("LDAP://" + Domain);
            var search = new DirectorySearcher(de);

            search.PropertiesToLoad.Add("cn");
            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");
            search.PropertiesToLoad.Add("memberOf");

            search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", computerName);
            var resultLocal = search.FindOne();

            if (resultLocal == null)
            { //Computer not found
                
            }

            addProperty("cn", de.Properties["cn"].Value);
            addProperty("memberOf", de.Properties["memberOf"].Value);
            addProperty("ms-Mcs-AdmPwdExpirationTime", de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value);

            adpath = resultLocal.Properties["ADsPath"][0].ToString();

            logger.Info("User {0} requesed info about computer {1}", userName, adpath);
        }

        public object getProperty(string property)
        {
            return properties[property];
        }

        public void addProperty(string property, object value)
        {
            properties.Add(property, value);
        }

        private string getDomain(string ComputerName)
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