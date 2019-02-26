using System.Collections.Generic;

namespace ITSWebMgmt.Caches
{
    public class GroupADcache : ADcache
    {
        public GroupADcache(string adpath) : base(adpath, new List<Property>
        {
            new Property("memberOf", typeof(object[])),
            new Property("member", typeof(object[])), //Check this
            new Property("description", typeof(string)),
            new Property("info", typeof(string)),
            new Property("name", typeof(string)),
            new Property("managedBy", typeof(string)),
            new Property("groupType", typeof(int)),
            new Property("distinguishedName", typeof(string)),
        }, null)
        { }
    }
}