using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ITSWebMgmt.Helpers
{
    public class GroupTableGenerator
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
        
        public static Tuple<String[], String[]> BuildGroupsSegments(string name, DirectoryEntry result, Label groupssegmentLabel, Label groupsAllsegmentLabel)
        {
            // Names of items in tuple is c# 7 feature: (String[] groupListConvert, String[] groupsListAllConverted)
            var groupsList = result.Properties[name];
            string attName = $"msds-{name}Transitive";
            result.RefreshCache(attName.Split(','));

            var b = groupsList.Cast<string>();
            var groupListConvert = b.ToArray<string>();

            var groupsListAll = result.Properties[attName];
            var groupsListAllConverted = groupsListAll.Cast<string>().ToArray();

            groupssegmentLabel.Text = buildgroupssegmentLabel(groupListConvert);
            groupsAllsegmentLabel.Text = buildgroupssegmentLabel(groupsListAllConverted);

            return Tuple.Create(groupListConvert, groupsListAllConverted);
        }
    }
}