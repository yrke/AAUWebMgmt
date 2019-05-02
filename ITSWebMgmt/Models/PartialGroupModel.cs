using ITSWebMgmt.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class PartialGroupModel : WebMgmtModel<ADcache>
    {
        public List<string> GroupList;
        public List<string> GroupListAll;
        public string GroupSegment;
        public string GroupsAllSegment;
        public string AttributeName;
        public string Title;

        public List<string> getGroups(string name) => ADcache.getGroups(name);
        public List<string> getGroupsTransitive(string name) => ADcache.getGroupsTransitive(name);

        public PartialGroupModel(ADcache aDcache, string attributeName, string title = "Groups")
        {
            ADcache = aDcache;
            AttributeName = attributeName;
            Title = title;
        }
    }
}
