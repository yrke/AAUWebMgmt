using ITSWebMgmt.Caches;
using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Controllers
{
    public class Controller<T> where T : ADcache
    {
        protected T ADcache;
        public string Path { get => ADcache.Path; }
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public virtual string adpath { get => ADcache.adpath; set { ADcache.adpath = value; } }
        public string[] getGroups(string name) => ADcache.getGroups(name);
        public string[] getGroupsTransitive(string name) => ADcache.getGroupsTransitive(name);
        public List<PropertyValueCollection> getAllProperties() => ADcache.getAllProperties();
    }
}