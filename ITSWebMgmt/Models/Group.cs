using ITSWebMgmt.Controllers;
using ITSWebMgmt.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Models
{
    public class Group
    {
        public GroupController group;
        public string Title;
        public string FileshareInfo;
        public string Domain;
        public string ManagedBy;
        public string SecurityGroup;
        public string GroupScope;
        public string Description;
        public string Info;
        public string GroupSegment;
        public string GroupsAllSegment;
        public string GroupOfSegment;
        public string GroupsOfAllSegment;
        public string Raw;

        public Group(GroupController controller)
        {
            group = controller;
        }

        public void buildResult()
        {
            buildBasicInfo();
            buildRaw();

            if (GroupController.isFileShare(group.DistinguishedName))
            {
                string[] tables = group.GetFileshareTables();
                GroupSegment = tables[0];
                GroupsAllSegment = tables[1];
                GroupOfSegment = tables[2];
                GroupsOfAllSegment = tables[3];

                FileshareInfo = "Contains information from the other fileshares with the other accesses<br>";
                Title = "Fileshare Info";
            }
            else
            {
                GroupSegment = TableGenerator.BuildgroupssegmentLabel(group.getGroups("member"));
                GroupsAllSegment = TableGenerator.BuildgroupssegmentLabel(group.getGroupsTransitive("member"));
                GroupOfSegment = TableGenerator.BuildgroupssegmentLabel(group.getGroups("memberOf"));
                GroupsOfAllSegment = TableGenerator.BuildgroupssegmentLabel(group.getGroupsTransitive("memberOf"));
                Title = "Group Info";
            }
        }

        private void buildRaw()
        {
            Raw = TableGenerator.buildRawTable(group.getAllProperties());
        }

        private void buildBasicInfo()
        {
            var sb = new StringBuilder();

            var dom = group.Path.Split(',').Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
            Domain = dom;

            string managedByString = "none";
            if (group.ManagedBy != "")
            {
                var manager = group.ManagedBy;

                var ldapSplit = manager.Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                managedByString = string.Format("<a href=\"/Redirector.aspx?adpath={0}\">{1}</a>", HttpUtility.HtmlEncode("LDAP://" + manager), domain + "\\" + name);

            }
            ManagedBy = managedByString;

            //IsDistributionGroup?
            //ManamgedBy

            var isDistgrp = false;
            string groupType = ""; //domain Local, Global, Universal 

            var gt = group.GroupType;
            switch (gt)
            {
                case "2":
                    isDistgrp = true;
                    groupType = "Global";
                    break;
                case "4":
                    groupType = "Domain local";
                    isDistgrp = true;
                    break;
                case "8":
                    groupType = "Universal";
                    isDistgrp = true;
                    break;
                case "-2147483646":
                    groupType = "Global";
                    break;
                case "-2147483644":
                    groupType = "Domain local";
                    break;
                case "-2147483640":
                    groupType = "Universal";
                    break;
            }

            SecurityGroup = (!isDistgrp).ToString();
            GroupScope = groupType;

            if (group.Description != null)
            {
                Description = group.Description;
            }

            //I found a object that had a attrib that was info not description?
            if (group.Info != null)
            {
                Info = group.Info;
            }

            //TODO: IsRequrceGroup (is exchange, fileshare or other resource type group?)

        }
    }
}