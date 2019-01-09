using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Helpers
{
    public class DatabaseGetter
    {
        public static ManagementScope ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");

        public static ManagementObjectCollection getResults(WqlObjectQuery wqlq)
        {
            var searcher = new ManagementObjectSearcher(ms, wqlq);
            return searcher.Get();
        }

        public static bool HasValues(ManagementObjectCollection results)
        {
            try
            {
                var t2 = results.Count;
                return results.Count != 0;
            }
            catch (ManagementException e) { }

            return false;
        }

        public static string CreateVerticalTableFromDatabase(WqlObjectQuery wqlq, List<string> keys, string errorMessage) => CreateVerticalTableFromDatabase(wqlq, keys, keys, errorMessage);

        public static string CreateVerticalTableFromDatabase(WqlObjectQuery wqlq, List<string> keys, List<string> names, string errorMessage)
        {
            HTMLTableHelper tableHelper = new HTMLTableHelper(2);
            var sb = new StringBuilder();
            var results = getResults(wqlq);

            if (HasValues(results))
            {
                int i = 0;
                var o = results.OfType<ManagementObject>().FirstOrDefault();

                foreach (var p in keys)
                {
                    var property = o.Properties[p];
                    if (o.Properties[p].Name == "Size" || o.Properties[p].Name == "FreeSpace")
                    {
                        tableHelper.AddRow(new string[] { names[i], (int.Parse(o.Properties[p].Value.ToString()) / 1024).ToString() });
                    }
                    else if (property.Type.ToString() == "DateTime")
                    {
                        tableHelper.AddRow(new string[] { names[i], DateTimeConverter.Convert(o.Properties[p].Value.ToString()) });
                    }
                    else
                    {
                        tableHelper.AddRow(new string[] { names[i], o.Properties[p].Value.ToString() });
                    }
                    i++;
                }
            }
            else
            {
                return errorMessage;
            }

            return tableHelper.GetTable();
        }

        public static Tuple<string, string> CreateTableAndRawFromDatabase(WqlObjectQuery wqlq, List<string> keys, string errorMessage)
        {
            HTMLTableHelper tableHelper = new HTMLTableHelper(2);
            var sb = new StringBuilder();

            var results = getResults(wqlq);

            if (HasValues(results))
            {
                foreach (ManagementObject o in results) //Has one!
                {
                    //OperatingSystemNameandVersion = Microsoft Windows NT Workstation 6.1

                    foreach (var property in o.Properties)
                    {
                        string key = property.Name;
                        object value = property.Value;

                        if (keys.Contains(key))
                        {
                            if (value != null)
                            {
                                if (value.GetType().Equals(typeof(string[])))
                                {
                                    string joinedValues = string.Join(", ", (string[])value);
                                    tableHelper.AddRow(new string[] { key, joinedValues });

                                }
                                else if (property.Type.ToString() == "DateTime")
                                {
                                    tableHelper.AddRow(new string[] { key, DateTimeConverter.Convert(value.ToString()) });
                                }
                                else
                                {
                                    tableHelper.AddRow(new string[] { key, value.ToString() });
                                }
                            }
                            else
                            {
                                tableHelper.AddRow(new string[] { key, "" });
                            }
                        }

                        int i = 0;
                        string[] arry = null;
                        if (value != null && value.GetType().IsArray)
                        {
                            if (value is string[])
                            {
                                arry = (string[])value;
                            }
                            else
                            {
                                arry = new string[] { "none-string value" }; //XXX get the byte value
                            }
                            foreach (string f in arry)
                            {
                                sb.Append(string.Format("{0}[{2}]: {1}<br />", key, f, i));
                                i++;
                            }
                        }
                        else
                        {
                            sb.Append(string.Format("{0}: {1}<br />", key, property.Value));
                        }

                    }

                }
            }
            else
            {
                sb.Append(errorMessage);
            }

            return Tuple.Create(tableHelper.GetTable(), sb.ToString());
        }

        public static string CreateTableFromDatabase(WqlObjectQuery wqlq, List<string> keys, string errorMessage) => CreateTableFromDatabase(wqlq, keys, keys, errorMessage);

        public static string CreateTableFromDatabase(WqlObjectQuery wqlq, List<string> keys, List<string> names, string errorMessage)
        {
            var results = getResults(wqlq);

            if (HasValues(results))
            {
                HTMLTableHelper tableHelper = new HTMLTableHelper(names.ToArray());

                foreach (ManagementObject o in results) //Has one!
                {
                    List<string> properties = new List<string>();
                    foreach (var p in keys)
                    {
                        string temp = o.Properties[p].Value.ToString();
                        if (o.Properties[p].Type.ToString() == "DateTime")
                        {
                            temp = DateTimeConverter.Convert(temp);
                        }
                        properties.Add(temp);
                        
                    }
                    tableHelper.AddRow(properties.ToArray());
                }

                return tableHelper.GetTable();
            }
            else
            {
                return errorMessage;
            }
        }
    }
}