using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ITSWebMgmt.Helpers
{
    public class TableGenerator
    {
        private static string CreateGroupTable(List<string> groupsAsList)
        {
            HTMLTableHelper groupTableHelper = new HTMLTableHelper(new string[] { "Domain", "Name" });

            foreach (string adpath in groupsAsList)
            {
                var split = adpath.Split(',');
                var groupname = split[0].Replace("CN=", "");
                var domain = split.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
                var linkToGroup = String.Format("<a href=\"/GroupsInfo.aspx?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + adpath), groupname);
                groupTableHelper.AddRow(new string[] { domain, linkToGroup });

            }

            return groupTableHelper.GetTable();
        }

        private static string buildgroupssegmentLabel(String[] groupsList)
        {
            var groupsAsList = groupsList.ToList();

            /*Func<string[], string, bool> startsWith = delegate (string[] prefix, string value)
            {
                return prefix.Any<string>(x => value.StartsWith(x));
            };
            string[] prefixMBX_ACL = { "CN=MBX_", "CN=ACL_" };
            Func<string, bool> startsWithMBXorACL = (string value) => startsWith(prefixMBX_ACL, value);*/

            bool StartsWith(string[] prefix, string value) => prefix.Any(value.StartsWith);
            string[] prefixMBX_ACL = { "CN=MBX_", "CN=ACL_" };
            bool startsWithMBXorACL(string value) => StartsWith(prefixMBX_ACL, value);

            //Sort MBX and ACL Last
            groupsAsList.Sort((a, b) =>
            {
                if (startsWithMBXorACL(a) && startsWithMBXorACL(b))
                {
                    return a.CompareTo(b);
                }
                else if (startsWithMBXorACL(a))
                {
                    return 1;
                }
                else if (startsWithMBXorACL(b))
                {
                    return -1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            });

            return CreateGroupTable(groupsAsList);
        }
        
        public static Tuple<String[], String[]> BuildGroupsSegments(string[] groupsList, string[] groupsListTransitive, Label groupssegmentLabel, Label groupsAllsegmentLabel)
        {
            // Names of items in tuple is c# 7 feature: (String[] groupListConvert, String[] groupsListAllConverted)

            groupssegmentLabel.Text = buildgroupssegmentLabel(groupsList);
            groupsAllsegmentLabel.Text = buildgroupssegmentLabel(groupsListTransitive);

            return Tuple.Create(groupsList, groupsListTransitive);
        }

        public static string buildRawTable(List<PropertyValueCollection> properties)
        {
            //builder.Append((result.Properties["aauStaffID"][0])).ToString();
            var builder = new StringBuilder();

            builder.Append("<table class=\"ui celled structured table\"><thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>");

            foreach (PropertyValueCollection a in properties)
            {
                builder.Append("<tr>");

                //Don't display admin password in raw data
                if (a != null && a.Count > 0 && a.PropertyName != "ms-Mcs-AdmPwd")
                {
                    builder.Append("<td rowspan=\"" + a.Count + "\">" + a.PropertyName + "</td>");

                    var v = a[0];
                    if (v.GetType().Equals(typeof(DateTime)))
                    {
                        v = DateTimeConverter.Convert((DateTime)v);
                    }

                    if (a.Count == 1)
                    {
                        builder.Append("<td>" + v + "</td></tr>");
                    }
                    else
                    {
                        builder.Append("<td>" + v + "</td>");
                        for (int i = 1; i < a.Count; i++)
                        {
                            v = a[i];
                            if (v.GetType().Equals(typeof(DateTime)))
                            {
                                v = DateTimeConverter.Convert((DateTime)v);
                            }
                            builder.Append("<tr><td>" + v + "</td></tr>");
                        }
                    }
                }
                else
                {
                    builder.Append("<td></td></tr>");
                }
            }

            builder.Append("</tbody></table>");

            return builder.ToString();
        }

        public static string CreateVerticalTableFromDatabase(ManagementObjectCollection results, List<string> keys, string errorMessage) => CreateVerticalTableFromDatabase(results, keys, keys, errorMessage);

        public static string CreateVerticalTableFromDatabase(ManagementObjectCollection results, List<string> keys, List<string> names, string errorMessage)
        {
            HTMLTableHelper tableHelper = new HTMLTableHelper(2);
            var sb = new StringBuilder();

            if (Database.HasValues(results))
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

        public static Tuple<string, string> CreateTableAndRawFromDatabase(ManagementObjectCollection results, List<string> keys, string errorMessage)
        {
            HTMLTableHelper tableHelper = new HTMLTableHelper(2);
            var sb = new StringBuilder();

            if (Database.HasValues(results))
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

        public static string CreateTableFromDatabase(ManagementObjectCollection results, List<string> keys, string errorMessage) => CreateTableFromDatabase(results, keys, keys, errorMessage);

        public static string CreateTableFromDatabase(ManagementObjectCollection results, List<string> keys, List<string> names, string errorMessage)
        {
            if (Database.HasValues(results))
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