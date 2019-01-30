using ITSWebMgmt.Controllers;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Caches
{
    public class GroupADcache : ADcache
    {
        public GroupADcache(string adpath) : base(adpath, new List<string> { "memberOf", "member", "description", "info", "name", "managedBy", "groupType" })
        {
        }
    }
}