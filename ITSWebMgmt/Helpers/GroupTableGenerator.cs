using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Helpers
{
    public class GroupTableGenerator
    {
        public static string CreateGroupTable(List<string> groupsAsList)
        {
            HTMLTableHelper groupTableHelper = new HTMLTableHelper(2);
            var groupStringBuilder = new StringBuilder();

            groupStringBuilder.Append(groupTableHelper.printStart());
            groupStringBuilder.Append(groupTableHelper.printRow(new string[] { "Domain", "Name" }, true));

            foreach (string adpath in groupsAsList)
            {
                var split = adpath.Split(',');
                var groupname = split[0].Replace("CN=", "");
                var domain = split.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
                var linkToGroup = String.Format("<a href=\"/GroupsInfo.aspx?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + adpath), groupname);
                groupStringBuilder.Append(groupTableHelper.printRow(new string[] { domain, linkToGroup }));

            }

            groupStringBuilder.Append(groupTableHelper.printEnd());
            return groupStringBuilder.ToString();
        }
    }
}