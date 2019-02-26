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
                    
                    if (GroupController.isFileShare(group.DistinguishedName))
                    {
                        //View as fileshare
                        Response.Redirect(string.Format("/FileShareInfo.aspx?grouppath={0}\">{1}", HttpUtility.UrlEncode("LDAP://" + groupPath), groupName));
                        //TODO create view for fileshare
                    }
                    Session["group"] = group;
                    if (group.isGroup())
                    {
                        buildResult();
                    }
                }
            }
            else //Is postback
            {
                group = (GroupController)Session["group"];
                buildResult();
            }
        }

        protected void sumbit_Click(object sender, EventArgs e)
        {
            //Search button i pressed, do stuff
        }

        protected void EditManagedBy_Click(object sender, EventArgs e)
        {
            labelManagedByText.Text = labelManagedBy.Text;
            tuggleVisibility();
        }

        protected void SaveEditManagedBy_Click(object sender, EventArgs e)
        {
            ManagedByChanger managedByChanger = new ManagedByChanger(group.ADcache);
            managedByChanger.SaveEditManagedBy(labelManagedByText.Text);
            labelManagedByError.Text = managedByChanger.ErrorMessage;
            if (managedByChanger.ErrorMessage == "")
            {
                tuggleVisibility();
                labelManagedBy.Text = labelManagedByText.Text;
            }
        }

        private void tuggleVisibility()
        {
            labelManagedBy.Visible = !labelManagedBy.Visible;
            EditManagedByButton.Visible = !EditManagedByButton.Visible;
            labelManagedByText.Visible = !labelManagedByText.Visible;
            SaveEditManagedByButton.Visible = !SaveEditManagedByButton.Visible;
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


            labelName.Text = group.Name;
            var dom = group.Path.Split(',').Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
            labelDomain.Text = dom;

            string managedByString = "none";
            if (group.ManagedBy != "")
            {
                var manager = group.ManagedBy;

                var ldapSplit = manager.Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                managedByString = string.Format("<a href=\"/Redirector.aspx?adpath={0}\">{1}</a>", HttpUtility.HtmlEncode("LDAP://"+manager), domain + "\\" + name);

            }
            labelManagedBy.Text = managedByString;

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

            labelSecurityGroup.Text = (!isDistgrp).ToString();
            labelGroupScope.Text = groupType;

            if (group.Description != null)
            {
                labelGroupDescription.Text = group.Description;
            }

            //I found a object that had a attrib that was info not description?
            if (group.Info != null)
            {
                labelGroupInfo.Text = group.Info;
            }
            
            //TODO: IsRequrceGroup (is exchange, fileshare or other resource type group?)
            
        }
    }
}