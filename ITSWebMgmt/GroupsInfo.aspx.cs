using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
            var sb = new StringBuilder();


            var memberlist = group.Properties["memberOf"];

            foreach (var pvs in memberlist)
            {
                //TODO on hover display full AD name
                var ldapSplit = pvs.ToString().Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=","");

                sb.Append(string.Format("<a href=\"/GroupsInfo.aspx?grouppath={1}\">{0}</a><br/>", domain +"\\"+name, HttpUtility.HtmlEncode("LDAP://" + pvs.ToString())));
            }


            lblMemberOf.Text = sb.ToString();
        }

        private void buildMembers(DirectoryEntry group)
        {
            var sb = new StringBuilder();


            var memberlist = group.Properties["member"];

            foreach (var pvs in memberlist)
            {
                var ldapSplit = pvs.ToString().Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                sb.Append(string.Format("<a href=\"/Redirector.aspx?adpath={1}\">{0}</a><br/>", domain + "\\" + name, HttpUtility.HtmlEncode("LDAP://" + pvs.ToString())));
            }


            lblMembers.Text = sb.ToString();
        }

        private void buildBasicInfo(DirectoryEntry group)
        {
            return;
        }


    }
}