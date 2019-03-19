using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt.Controls
{
    public partial class GroupMembersView : System.Web.UI.UserControl
    {

        public List<string> groups;
        public List<string> groupsTransitive;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (groups != null && groupsTransitive != null) { 
                TableGenerator.BuildGroupsSegments(groups, groupsTransitive, groupssegmentLabel, groupsAllsegmentLabel);
            }

        }
    }
}