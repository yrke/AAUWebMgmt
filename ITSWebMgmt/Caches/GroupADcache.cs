using System.Collections.Generic;

namespace ITSWebMgmt.Caches
{
    public class GroupADcache : ADcache
    {
        public GroupADcache(string adpath) : base(adpath, new List<string> { "memberOf", "member", "description", "info", "name", "managedBy", "groupType" })
        {
        }
    }
}