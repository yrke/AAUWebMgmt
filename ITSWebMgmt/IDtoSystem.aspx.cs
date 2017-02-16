using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class IDtoSystem : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string id = Request.QueryString["id"] ?? ""; // ?? "": return empty string if null value


            if (id.StartsWith("IR", StringComparison.CurrentCultureIgnoreCase)){
                //Is incident
                Response.Redirect("https://service.aau.dk/Incident/Edit/" + id);
            }
            else if (id.StartsWith("SR", StringComparison.CurrentCultureIgnoreCase)) {
                Response.Redirect("https://service.aau.dk/ServiceRequest/Edit/" + id);
            }
            else {
                result.Text = "<h1>Type not found</h1>";
            }










        }
    }
}