using System;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;

namespace ITSWebMgmt
{
    public partial class GroupsInfo : System.Web.UI.Page
    {
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
                    //Handle a group path
                    var groupDE = convertADpathToDirectoryEntry(groupPath);
                    if (groupDE != null) { 
                        buildResult(groupDE);
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

        protected DirectoryEntry convertADpathToDirectoryEntry(string adpath)
        {
            ///XXX we expect a group check its a group
            var groupDE = new DirectoryEntry(adpath);
            if (groupDE.SchemaEntry.Name.Equals("group", StringComparison.CurrentCultureIgnoreCase))
            {
                return groupDE;
            }
            else
            {
                return null;
            }
        }

        protected void buildResult(DirectoryEntry group)
        {
            buildBasicInfo(group);
            buildMembers(group);
            buildMemberOf(group);
            buildRaw(group);

            ResultDiv.Visible = true;

        }

        private void buildRaw(DirectoryEntry group)
        {
            var generator = new RawADGridGenerator();
            var result = generator.buildRawSegment(group);

            labelRawData.Text = result.ToString();
        
        }

        private void buildMemberOf(DirectoryEntry group)
        {
            Helpers.TableGenerator.BuildGroupsSegments("memberOf", group, groupofsegmentLabel, groupofAllsegmentLabel);
        }

        private void buildMembers(DirectoryEntry group)
        {
            Helpers.TableGenerator.BuildGroupsSegments("member", group, groupssegmentLabel, groupsAllsegmentLabel);
        }

        private void buildBasicInfo(DirectoryEntry group)
        {
            var sb = new StringBuilder();


            sb.Append("<table class=\"ui definition table\">");

            sb.Append("<tr><td>");
            sb.Append("Name");
            sb.Append("</td><td>");
            sb.Append(group.Properties["name"].Value.ToString());
            sb.Append("</td></tr>");

            sb.Append("<tr><td>");
            sb.Append("Domain");
            sb.Append("</td><td>");
            var dom = group.Path.Split(',').Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
            sb.Append(dom);
            sb.Append("</td></tr>");

            string managedByString = "none";
            if (group.Properties.Contains("managedBy"))
            {
                var manager = group.Properties["managedBy"].Value.ToString();

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

            var gt = group.Properties["groupType"].Value.ToString();
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

            if (group.Properties.Contains("description"))
            {
                sb.Append("<tr><td>");
                sb.Append("Description");
                sb.Append("</td><td>");
                sb.Append(group.Properties["description"].Value.ToString());
                sb.Append("</td></tr>");
            }

            //I found a object that had a attrib that was info not description?
            if (group.Properties.Contains("info"))
            {
                sb.Append("<tr><td>");
                sb.Append("Info");
                sb.Append("</td><td>");
                sb.Append(group.Properties["info"].Value.ToString());
                sb.Append("</td></tr>");
            }
            
            //TODO: IsRequrceGroup (is exchange, fileshare or other resource type group?)
            


            sb.Append("</table>");
            lblBasicInfo.Text = sb.ToString();
        }


    }
}