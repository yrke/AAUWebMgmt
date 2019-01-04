using System;
using System.Collections.Generic;
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
                return true;
            }
            catch (ManagementException e) { }

            return false;
        }

        public static Tuple<string, string> CreateTableAndRawFromDatabase(WqlObjectQuery wqlq, List<string> keys, string errorMessage)
        {
            HTMLTableHelper tableHelper = new HTMLTableHelper(2);
            var tableStringBuilder = new StringBuilder();
            var sb = new StringBuilder();

            var results = getResults(wqlq);
            var mo = results.OfType<ManagementObject>().FirstOrDefault();

            tableStringBuilder.Append(tableHelper.printStart());

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
                                    tableStringBuilder.Append(tableHelper.printRow(new string[] { key, joinedValues }));

                                }
                                else
                                {
                                    tableStringBuilder.Append(tableHelper.printRow(new string[] { key, value.ToString() }));
                                }
                            }
                            else
                            {
                                tableStringBuilder.Append(tableHelper.printRow(new string[] { key, "" }));
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

            tableStringBuilder.Append(tableHelper.printEnd());
            return Tuple.Create(tableStringBuilder.ToString(), sb.ToString());
        }

        public static string CreateTableFromDatabase(WqlObjectQuery wqlq, List<string> keys, string errorMessage) => CreateTableFromDatabase(wqlq, keys, keys, errorMessage);

        public static string CreateTableFromDatabase(WqlObjectQuery wqlq, List<string> keys, List<string> names, string errorMessage)
        {
            var results = getResults(wqlq);
            var sb = new StringBuilder();

            if (HasValues(results))
            {
                HTMLTableHelper SWTableHelper = new HTMLTableHelper(keys.Count());
                var SWsb = new StringBuilder();
                SWsb.Append(SWTableHelper.printStart());
                SWsb.Append(SWTableHelper.printRow(names.ToArray(), true));

                foreach (ManagementObject o in results) //Has one!
                {
                    List<string> properties = new List<string>();
                    foreach (var p in keys)
                    {
                        properties.Add(o.Properties[p].Value.ToString());
                    }
                    SWsb.Append(SWTableHelper.printRow(properties.ToArray()));
                }

                SWsb.Append(SWTableHelper.printEnd());
                sb.Append(SWsb);
            }
            else
            {
                sb.Append(errorMessage);
            }

            return sb.ToString();
        }
    }
}