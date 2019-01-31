using ITSWebMgmt.Caches;
using System;

namespace ITSWebMgmt.Controllers
{
    public class GroupController : Controller<GroupADcache>
    {
        public string Description { get => ADcache.getPropertyAsString("description"); }
        public string Info { get => ADcache.getPropertyAsString("info"); }
        public string Name { get => ADcache.getPropertyAsString("name"); }
        public string ManagedBy { get => ADcache.getPropertyAsString("managedBy"); }
        public string GroupType { get => ADcache.getPropertyAsString("groupType"); }

        public GroupController(string adpath)
        {
            ADcache = new GroupADcache(adpath);
        }

        public bool isGroup()
        {
            ///XXX we expect a group check its a group
            return ADcache.DE.SchemaEntry.Name.Equals("group", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}