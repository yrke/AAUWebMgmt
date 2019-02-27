using ITSWebMgmt.Controllers;
using ITSWebMgmt.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace ITSWebMgmt
{
    public partial class GroupsInfo : System.Web.UI.Page
    {
        private GroupController group;

        protected void Page_Load(object sender, EventArgs e)
        {
            ResultDiv.Visible = false;

            if (!IsPostBack)
            {
                //GroupPath the AD path to an object
                //GroupName the (domina) + group name (now we need to search for the gorup)
                //Default user an AD path to the user to be default to add/remove from a group
                //Action (action to preform)
                //Return to (only used on action to) page to return to after action
                String groupPath = Request.QueryString["grouppath"];
                String groupName = Request.QueryString["groupname"];
                String defaultUser = Request.QueryString["defaultuser"];

                if (groupPath != null)
                {
                    group = new GroupController(groupPath);

                    if (group.isGroup())
                    {
                        buildResult();
                    }
                }
            }
            else //Is postback
            {
            }
        }

        protected void sumbit_Click(object sender, EventArgs e)
        {
            //Search button i pressed, do stuff
        }

        protected void buildResult()
        {
            buildBasicInfo();
            buildMembers();
            buildMemberOf();
            buildRaw();

            ResultDiv.Visible = true;
        }

        private void buildRaw()
        {
            labelRawData.Text = TableGenerator.buildRawTable(group.getAllProperties());
        }

        private void buildMembers()
        {
            TableGenerator.BuildGroupsSegments(group.getGroups("member"), group.getGroupsTransitive("member"), groupssegmentLabel, groupsAllsegmentLabel);
        }
        private void buildMemberOf()
        {
            TableGenerator.BuildGroupsSegments(group.getGroups("memberOf"), group.getGroupsTransitive("memberOf"), groupofsegmentLabel, groupofAllsegmentLabel);
        }

        private void buildBasicInfo()
        {
            var sb = new StringBuilder();


            sb.Append("<table class=\"ui definition table\">");

            sb.Append("<tr><td>");
            sb.Append("Name");
            sb.Append("</td><td>");
            sb.Append(group.Name);
            sb.Append("</td></tr>");

            sb.Append("<tr><td>");
            sb.Append("Domain");
            sb.Append("</td><td>");
            var dom = group.Path.Split(',').Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
            sb.Append(dom);
            sb.Append("</td></tr>");

            string managedByString = "none";
            if (group.ManagedBy != "")
            {
                var manager = group.ManagedBy;

                var ldapSplit = manager.Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                managedByString = string.Format("<a href=\"/Redirector.aspx?adpath={0}\">{1}</a>", HttpUtility.HtmlEncode("LDAP://"+manager), domain + "\\" + name);

            }
            sb.Append("<tr><td>");
            sb.Append("Managed By");
            sb.Append("</td><td>");
            sb.Append(managedByString);
            sb.Append("</td></tr>");
            
            //IsDistributionGroup?
            //ManamgedBy

            var isDistgrp = false;
            string groupType = ""; //domain Local, Global, Universal 

            var gt = group.GroupType;
            switch (gt)
            {
                case "2":
                    isDistgrp=true;
                    groupType="Global";
                    break;
                case "4":
                    groupType="Domain local";
                    isDistgrp=true;
                    break;
                case "8":
                    groupType="Universal";
                    isDistgrp=true;
                    break;
                case "-2147483646":
                    groupType="Global";
                    break;
                case "-2147483644":
                    groupType="Domain local";
                    break;
                case "-2147483640":
                    groupType="Universal";
                    break;
            }

            sb.Append("<tr><td>");
            sb.Append("Is Security group");
            sb.Append("</td><td>");
            sb.Append((!isDistgrp).ToString());
            sb.Append("</td></tr>");

            sb.Append("<tr><td>");
            sb.Append("Group Scope");
            sb.Append("</td><td>");
            sb.Append(groupType);
            sb.Append("</td></tr>");

            if (group.Description != null)
            {
                sb.Append("<tr><td>");
                sb.Append("Description");
                sb.Append("</td><td>");
                sb.Append(group.Description);
                sb.Append("</td></tr>");
            }

            //I found a object that had a attrib that was info not description?
            if (group.Info != null)
            {
                sb.Append("<tr><td>");
                sb.Append("Info");
                sb.Append("</td><td>");
                sb.Append(group.Info);
                sb.Append("</td></tr>");
            }
            
            //TODO: IsRequrceGroup (is exchange, fileshare or other resource type group?)
            


            sb.Append("</table>");
            lblBasicInfo.Text = sb.ToString();
        }


    }
}