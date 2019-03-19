using System;
using System.Text;

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

            sb.Append(@"<br /><br /><a href=""#"" onclick=""window.history.go(-2); return false;""> Go back to UserInfo </a>");
                    
            sb.Append("</form>");
            sb.Append("</body></html>");
            Response.Write(sb.ToString());
            Response.End();


        }


        protected void createForm(string url){
            string descriptionConverted = tb_description.Text.Replace("\n", "\\n").Replace("\r", "\\r");
            createRedirectCode(Request.QueryString["userID"], tb_affectedUser.Text, tb_title.Text, descriptionConverted, url);        
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