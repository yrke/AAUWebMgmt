using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Caches
{
    public abstract class ADcache
    {
        protected Dictionary<string, object> properties = new Dictionary<string, object>();
        public List<string> propertyNames;
        public DirectoryEntry DE;
        public SearchResult result;
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public string adpath;

        public List<PropertyValueCollection> getAllProperties()
        {
            List<PropertyValueCollection> propertyValueCollections = new List<PropertyValueCollection>();
            foreach (string k in DE.Properties.PropertyNames)
            {
                propertyValueCollections.Add(DE.Properties[k]);
            }
            return propertyValueCollections;
        }

        public object getProperty(string property)
        {
            if (properties.ContainsKey(property))
            {
                return properties[property];
            }
            return null;
        }

        public string getPropertyAsString(string property)
        {
            if (properties.ContainsKey(property))
            {
                var temp = getProperty(property);
                return temp != null ? temp.ToString() : null;
            }
            return null;
        }

        public void addProperty(string property, object value)
        {
            properties.Add(property, value);
        }

        public string[] getGroups(string name)
        {
            return result.Properties[name].Cast<string>().ToArray();
        }

        public string[] getGroupsTransitive(string name)
        {
            string attName = $"msds-{name}Transitive";
            result.GetDirectoryEntry().RefreshCache(attName.Split(','));
            var temp = result.GetDirectoryEntry();
            //TODO Does cannot find attName in DE or result
            return result.GetDirectoryEntry().Properties[attName].Cast<string>().ToArray();
        }
    }
}