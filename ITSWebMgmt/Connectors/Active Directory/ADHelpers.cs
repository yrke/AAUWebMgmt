using System;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Connectors.Active_Directory
{
    public class ADHelpers
    {

       //XXX these functions can add/remove any user from any group. Dont let i be a user parameter. 
       //Wrap these to know groups. 

        private static void addADuserToGroupUNSAFE(string userADpath, string groupADPath) 
        {
            
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupADPath);
                dirEntry.Properties["member"].Add(userADpath);
                dirEntry.CommitChanges();
                dirEntry.Close();
            
        }

        private static void removeADuserFromGroupUNSAFE(string userADpath, string groupADPath)
        {
            
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupADPath);
                dirEntry.Properties["member"].Remove(userADpath);
                dirEntry.CommitChanges();
                dirEntry.Close();
                
        }

        static string[] safegroups = {
             
            "CN=kyrketestgroup,OU=Manual,OU=Groups,DC=its,DC=aau,DC=dk",
        };
        

        private static bool isGroupSafe(String groupADPath)
        {

            foreach (string s in safegroups)
            {
                if (s.Equals(groupADPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;

        }

        public static void addADUserToGroup(string userADpath, string groupADPath)
        {
            if (!isGroupSafe(groupADPath))
            {
                throw new Exception("Not a safe group");
            }

            addADuserToGroupUNSAFE(userADpath, groupADPath);

        }

        public static void remoteADUserFromGroup(string userADpath, string groupADPath)
        {
            if (!isGroupSafe(groupADPath))
            {
                throw new Exception("Not a safe group");
            }
            removeADuserFromGroupUNSAFE(userADpath, groupADPath);
        }

        public static string DistinguishedNameToUPN(string dn)
        {
            //format CN=kyrke,OU=test,OU=Staff,OU=People,DC=its,DC=aau,DC=dk 
            //to kyrke@its.aau.dk

            string[] dnSplit = dn.Split(',');
            string cn = dnSplit[0].ToLower().Replace("cn=", "");
            string domain = String.Join(".", dnSplit.Where(x => x.ToLower().StartsWith("dc=")).Select(x => x.ToLower().Replace("dc=", "")));

            return $"{cn}@{domain}";

        }

    }
}