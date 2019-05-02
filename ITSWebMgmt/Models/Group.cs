using ITSWebMgmt.Caches;
using ITSWebMgmt.Controllers;
using ITSWebMgmt.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Models
{
    public class Group : WebMgmtModel<GroupADcache>
    {
        public string Description { get => ADcache.getProperty("description"); }
        public string Info { get => ADcache.getProperty("info"); }
        public string Name { get => ADcache.getProperty("name"); }
        public string ADManagedBy { get => ADcache.getProperty("managedBy"); }
        public string GroupType { get => ADcache.getProperty("groupType").ToString(); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName").ToString(); }
        public GroupController group;
        public string Title;
        public string FileshareInfo;
        public string Domain;
        public string ManagedBy;
        public string SecurityGroup;
        public string GroupScope;
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

            if (GroupController.isFileShare(DistinguishedName))
            {
                // TODO move to different view
                /*string[] tables = group.GetFileshareTables();
                GroupSegment = tables[0];
                GroupsAllSegment = tables[1];
                GroupOfSegment = tables[2];
                GroupsOfAllSegment = tables[3];

                FileshareInfo = "Contains information from the other fileshares with the other accesses<br>";
                Title = "Fileshare Info";*/
            }
            Title = "Group Info";
        }

        private void buildRaw()
        {
            Raw = TableGenerator.buildRawTable(getAllProperties());
        }

        private void buildBasicInfo()
        {
            var sb = new StringBuilder();

            var dom = Path.Split(',').Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
            Domain = dom;

            string managedByString = "none";
            if (ADManagedBy != "")
            {
                var manager = ADManagedBy;

                var ldapSplit = manager.Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                managedByString = string.Format("<a href=\"/Home/Redirector?adpath={0}\">{1}</a>", HttpUtility.HtmlEncode("LDAP://" + manager), domain + "\\" + name);
            }
            ManagedBy = managedByString;

            //IsDistributionGroup?
            //ManamgedBy

            var isDistgrp = false;
            string groupType = ""; //domain Local, Global, Universal 

            var gt = GroupType;
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

            //TODO: IsRequrceGroup (is exchange, fileshare or other resource type group?)

        }
    }
}