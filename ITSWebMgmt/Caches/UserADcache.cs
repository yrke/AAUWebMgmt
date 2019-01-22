using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

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
            "msDS-User-Account-Control-Computed",
            "msDS-UserPasswordExpiryTimeComputed",
            "givenName",
            "sn",
            "profilepath",
            "aauStaffID",
            "aauStudentID",
            "aauUserClassification",
            "aauUserStatus",
            "userAccountControl",
            "scriptPath"
        }){}
    }
}