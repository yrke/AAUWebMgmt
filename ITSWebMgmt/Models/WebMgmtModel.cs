using ITSWebMgmt.Caches;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ITSWebMgmt.Models
{
    public abstract class WebMgmtModel<T> where T : ADcache
    {
        public T ADcache;
        public string Path { get => ADcache.Path; }
        public virtual string adpath { get => ADcache.adpath; set { ADcache.adpath = value; } }
        public List<PropertyValueCollection> getAllProperties() => ADcache.getAllProperties();
    }
}
