using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Caches
{
    public class Property
    {
        public string Name;
        public Type Type;
        public object Value;

        public Property(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    public abstract class ADcache
    {
        protected Dictionary<string, Property> properties = new Dictionary<string, Property>();
        public DirectoryEntry DE;
        public SearchResult result;
        public string Path { get => DE.Path; }
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public string adpath;
        private List<Type> types = new List<Type>();

        public ADcache() { }

        public ADcache(string adpath, List<Property> properties, List<Property> propertiesToRefresh)
        {
            this.adpath = adpath;
            DE = new DirectoryEntry(adpath);
            var search = new DirectorySearcher(DE);

            if (propertiesToRefresh != null)
            {
                List<string> propertiesNamesToRefresh = new List<string>();
                foreach (var p in propertiesToRefresh)
                {
                    propertiesNamesToRefresh.Add(p.Name);
                    properties.Add(p);
                }

                DE.RefreshCache(propertiesNamesToRefresh.ToArray());
            }

            foreach (var p in properties)
            {
                search.PropertiesToLoad.Add(p.Name);
            }

            result = search.FindOne();

            foreach (var p in properties)
            {
                var value = DE.Properties[p.Name].Value;
                //TODO Handle all null values here and give then default values
                if (value == null)
                {
                    if (p.Type.Equals(typeof(bool)))
                        value = false;
                    else if (p.Type.Equals(typeof(int)))
                        value = -1;
                    else if (p.Type.Equals(typeof(string)))
                        value = "";
                    else if (p.Type.Equals(typeof(object[])))
                        value = new string[]{""};
                }
                else
                {
                    //Print the type
                    Debug.WriteLine($"{p.Name}: {value.GetType().ToString()}, value: {value.ToString()}");

                    //Handle special types
                    if (value.GetType().Equals(typeof(object[])))
                    {
                        if (value.GetType().Equals(typeof(string)))
                        {
                            value = new string[] { value.ToString() };
                        }
                        else
                        {
                            value = ((object[])value).Cast<string>().ToArray<string>();
                        }
                    }
                    if (value.GetType().ToString() == "System.__ComObject")
                    {
                        value = DateTimeConverter.Convert(value);
                    }
                }

                p.Value = value;
                addProperty(p.Name, p);
            }
        }

        public List<PropertyValueCollection> getAllProperties()
        {
            List<PropertyValueCollection> propertyValueCollections = new List<PropertyValueCollection>();
            foreach (string k in DE.Properties.PropertyNames)
            {
                propertyValueCollections.Add(DE.Properties[k]);
            }
            return propertyValueCollections;
        }

        public dynamic getProperty(string property)
        {
            if (properties.ContainsKey(property))
            {
                return properties[property].Value;
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

        public string getPropertyAsDateString(string property)
        {
            if (properties.ContainsKey(property))
            {
                var temp = getProperty(property);
                if (temp.GetType().Equals(typeof(long)))
                {
                    return DateTimeConverter.Convert((long)temp);
                }
                return temp != null ? DateTimeConverter.Convert(temp.Value) : null;
            }
            return null;
        }

        public T? getPropertyAs<T>(string property) where T : struct
        {
            if (properties.ContainsKey(property))
            {
                var temp = getProperty(property);
                return temp != null ? (T)temp : (T?)null;
            }
            return null;
        }

        public void addProperty(string property, Property value)
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
            DE.RefreshCache(attName.Split(','));
            return DE.Properties[attName].Cast<string>().ToArray();
        }
    }
}