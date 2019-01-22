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
        public GroupADcache(string adpath)
        {
            this.adpath = adpath;
            DE = new DirectoryEntry(adpath);
            var search = new DirectorySearcher(DE);

            propertyNames = new List<string>{ "memberOf", "member", "description", "info", "name", "managedBy", "groupType" };
            foreach (string p in propertyNames)
            {
                search.PropertiesToLoad.Add(p);
            }

            result = search.FindOne();

            foreach (string p in propertyNames)
            {
                addProperty(p, DE.Properties[p].Value);
            }
        }

        public string Path { get => DE.Path; }
    }
}