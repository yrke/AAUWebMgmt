using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ITSWebMgmt.Caches
{
    public class ComputerADcache : ADcache
    {
        public string ComputerName = "ITS\\AAU804396";
        public bool ComputerFound = false;
        public string Domain;
        public object AdmPwdExpirationTime;
        public ComputerADcache(string computerName, string userName) : base()
        {
            ComputerName = computerName;
            DE = new DirectoryEntry("LDAP://" + getDomain());
            var search = new DirectorySearcher(DE);

            search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);
            result = search.FindOne();

            if (result == null)
            { //Computer not found
                return;
            }

            ComputerFound = true;

            adpath = result.Properties["ADsPath"][0].ToString();
            DE = new DirectoryEntry(adpath);

            var PropertyNames = new List<string> { "memberOf", "cn", "ms-Mcs-AdmPwdExpirationTime", "managedBy" };
            //propertyNames.Add("ms-Mcs-AdmPwd");

            search = new DirectorySearcher(DE);
            foreach (string p in PropertyNames)
            {
                search.PropertiesToLoad.Add(p);
            }
            search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);
            result = search.FindOne();

            List<Property> properties = new List<Property>
            {
                new Property("managedBy", typeof(string)),
                new Property("cn", typeof(string)),
                new Property("memberOf", typeof(string)),
                new Property("ms-Mcs-AdmPwdExpirationTime", typeof(object)) //System.__ComObject
            };

            saveCache(properties, null);

            logger.Info("User {0} requesed info about computer {1}", userName, adpath);
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