using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ITSWebMgmt.Controllers
{
    public class FileshareController
    {
        public string MemberTable = "";
        public string TransitiveMemberTable = "";
        public string MemberOfTable = "";
        public string TransitiveMemberOfTable = "";

        public FileshareController(string groupPath)
        {
            //TODO Things to show in basic info: Type fileshare/department and Domain plan/its/adm

            HTMLTableHelper[] groupTableHelper = new HTMLTableHelper[4];
            for (int i = 0; i < 4; i++)
            {
                groupTableHelper[i] = new HTMLTableHelper(new string[] { "Name", "Type", "Access" });
            }

            List<string> accessNames = new List<string> { "Full", "Modify", "Read", "Edit", "Contribute" };
            foreach (string accessName in accessNames)
            {
                string temp = Regex.Replace(groupPath, @"_[a-zA-Z]*,OU", $"_{accessName},OU");
                GroupController group = null;
                try
                {
                    group = new GroupController(temp);
                }
                catch (Exception)
                {
                }

                if (group != null)
                {
                    List<string>[] adpaths = new List<string>[] { group.getGroups("member"), group.getGroupsTransitive("member"), group.getGroups("memberOf"), group.getGroupsTransitive("memberOf") };
                    for (int i = 0; i < 4; i++)
                    {
                        TableGenerator.createGroupTableRows(adpaths[i], groupTableHelper[i], accessName);
                    }
                }
            }

            MemberTable = groupTableHelper[0].GetTable();
            TransitiveMemberTable = groupTableHelper[1].GetTable();
            MemberOfTable = groupTableHelper[2].GetTable();
            TransitiveMemberOfTable = groupTableHelper[3].GetTable();
        }
    }
}