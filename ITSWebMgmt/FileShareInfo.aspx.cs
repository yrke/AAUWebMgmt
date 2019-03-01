using ITSWebMgmt.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class FileShareInfo : System.Web.UI.Page
    {
        private FileshareController fileshare;

        protected void Page_Load(object sender, EventArgs e)
        {
            String groupPath = Request.QueryString["grouppath"];

            fileshare = new FileshareController(groupPath);

            groupssegmentLabel.Text = fileshare.MemberTable;
            groupsAllsegmentLabel.Text = fileshare.TransitiveMemberTable;
            groupofsegmentLabel.Text = fileshare.MemberOfTable;
            groupofAllsegmentLabel.Text = fileshare.TransitiveMemberOfTable;
        }
    }
}