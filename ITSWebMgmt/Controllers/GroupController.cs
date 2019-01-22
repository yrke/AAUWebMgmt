using ITSWebMgmt.Caches;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Controllers
{
    public class GroupController
    {
        private GroupADcache ADcache;

        public string[] getGroups(string name) => ADcache.getGroups(name);
        public string[] getGroupsTransitive(string name) => ADcache.getGroupsTransitive(name);
        public List<PropertyValueCollection> getAllProperties() => ADcache.getAllProperties();

        public string Description { get => ADcache.getPropertyAsString("description"); }
        public string Info { get => ADcache.getPropertyAsString("info"); }
        public string Name { get => ADcache.getPropertyAsString("name"); }
        public string ManagedBy { get => ADcache.getPropertyAsString("managedBy"); }
        public string GroupType { get => ADcache.getPropertyAsString("groupType"); }
        public string Path { get => ADcache.Path; }

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