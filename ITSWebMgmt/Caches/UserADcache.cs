using System;
using System.Collections.Generic;

namespace ITSWebMgmt.Caches
{
    public class UserADcache : ADcache
    {
        public UserADcache(string adpath) : base(adpath, new List<Property>
        {
            new Property("memberOf", typeof(object[])),
            new Property("member", typeof(object[])), //Check this
            new Property("userPrincipalName", typeof(string)),
            new Property("displayName", typeof(string)),
            new Property("proxyAddresses", typeof(object[])),
            new Property("givenName", typeof(string)),
            new Property("sn", typeof(string)),
            new Property("profilepath", typeof(string)), //Check this
            new Property("aauStaffID", typeof(int)),
            new Property("aauStudentID", typeof(int)), //Check this
            new Property("aauUserClassification", typeof(string)),
            new Property("aauUserStatus", typeof(string)),
            new Property("userAccountControl", typeof(int)),
            new Property("scriptPath", typeof(string)), //Check this
            new Property("IsAccountLocked", typeof(bool)), //Check this
            new Property("aauAAUID", typeof(string)),
            new Property("aauUUID", typeof(string)),
            new Property("telephoneNumber", typeof(string)),
            new Property("lastLogon", typeof(object)), //System.__ComObject
            new Property("distinguishedName", typeof(string)),
            new Property("objectGUID", typeof(object[]))
        }, new List<Property>
        {
            new Property("msDS-User-Account-Control-Computed", typeof(int)),
            new Property("msDS-UserPasswordExpiryTimeComputed", typeof(object)) //System.__ComObject
        })
        { }
    }
}
