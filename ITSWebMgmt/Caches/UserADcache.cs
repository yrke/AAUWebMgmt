using System.Collections.Generic;

namespace ITSWebMgmt.Caches
{
    public class UserADcache : ADcache
    {
        public UserADcache(string adpath) : base(adpath, new List<string>
        {
            "memberOf",
            "member",
            "userPrincipalName",
            "displayName",
            "proxyAddresses",
            "givenName",
            "sn",
            "profilepath",
            "aauStaffID",
            "aauStudentID",
            "aauUserClassification",
            "aauUserStatus",
            "userAccountControl",
            "scriptPath",
            "IsAccountLocked",
            "aauAAUID",
            "aauUUID",
            "telephoneNumber",
            "lastLogon",
            "distinguishedName"
        })
        {
            string[] test = { "msDS-User-Account-Control-Computed", "msDS-UserPasswordExpiryTimeComputed" };
            DE.RefreshCache(test);
            for (int i = 0; i < 2; i++)
            {
                addProperty(test[i], DE.Properties[test[i]].Value);
            }
        }
    }
}