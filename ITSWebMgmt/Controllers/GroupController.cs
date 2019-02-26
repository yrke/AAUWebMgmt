using ITSWebMgmt.Caches;
using System;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Controllers
{
    public class GroupController : Controller<GroupADcache>
    {
        public string Description { get => ADcache.getProperty("description"); }
        public string Info { get => ADcache.getProperty("info"); }
        public string Name { get => ADcache.getProperty("name"); }
        public string ManagedBy { get => ADcache.getProperty("managedBy"); }
        public string GroupType { get => ADcache.getProperty("groupType").ToString(); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName").ToString(); }

        public GroupController(string adpath)
        {
            ADcache = new GroupADcache(adpath);
        }

        public bool isGroup()
        {
            ///XXX we expect a group check its a group
            return ADcache.DE.SchemaEntry.Name.Equals("group", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool isFileShare(string value)
        {
            var split = value.Split(',');
            var oupath = split.Where(s => s.StartsWith("OU=")).ToArray();
            int count = oupath.Count();

            return ((count == 3 && oupath[count - 1].Equals("OU=Groups") && oupath[count - 2].Equals("OU=Resource Access")));
        }
    }
}