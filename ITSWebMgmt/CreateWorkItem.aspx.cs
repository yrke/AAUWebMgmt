using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class CreateWorkItem : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            if (!IsPostBack)
            {
                var userID = Request.QueryString["userID"];
                var userDisplayName = Request.QueryString["userDisplayName"];

                tb_affectedUser.Text = userDisplayName;



            }

        }

        protected void createRedirectCode(string userID, string displayName, string title, string description, string submiturl)
        {
            
            Response.Clear();
            string jsondata = "{\"Title\":\"" + title + "\",\"Description\":\"" + description + "\",\"RequestedWorkItem\":{\"BaseId\":\"" + userID + "\",\"DisplayName\":\"" + displayName + "\",}}";
            var sb = new StringBuilder();

            sb.Append("<html><head>");
            //sb.Append("<script src=\"https://cdnjs.cloudflare.com/ajax/libs/jquery/3.0.0-alpha1/jquery.min.js\" ></script>");
            sb.Append("</head>");
            sb.Append(@"<body onload='document.forms[""form""].submit()'>");

            sb.Append("<form name='form' target='_blank' method='post' action='" + submiturl + "'>");
            
            sb.Append("<input hidden='true' name='vm' value='"+jsondata+"'/>");
            sb.Append("<input type='submit' value='Recreate IR/SR (if someting whent wrong)' />");
                    
            sb.Append("</form>");
            sb.Append("</body></html>");
            Response.Write(sb.ToString());
            Response.End();


        }


        protected void createForm(string url){
                createRedirectCode(Request.QueryString["userID"], tb_affectedUser.Text, tb_title.Text, tb_description.Text, url);
        
        }
        protected void createSR_OnClick(object sender, EventArgs e)
        {
            createForm("https://service.aau.dk/ServiceRequest/New/");
        }

        protected void createIR_OnClick(object sender, EventArgs e)
        {

            createForm("https://service.aau.dk/Incident/New/");
        
        }
    }
}