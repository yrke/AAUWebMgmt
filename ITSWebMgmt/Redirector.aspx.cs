using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class Redirector : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {

                var adpath = Request.QueryString["adpath"];
                if (adpath != null)
                {
                    if (!adpath.StartsWith("LDAP://"))
                    {
                        adpath = "LDAP://" + adpath;
                    }
                    DirectoryEntry de = new DirectoryEntry(adpath);

                    var type = de.SchemaEntry.Name;

                    if (type.Equals("user"))
                    {

                        
                        string param = "?" + "username=" + de.Properties["userPrincipalName"].Value.ToString();
                        Response.Redirect("~/UserInfo.aspx" + param);

                    } else if(type.Equals("computer")) {
                        //http://localhost:52430/computerInfo.aspx?computername=its\kyrke-l03
                        
                        var ldapSplit = adpath.Replace("LDAP://","").Split(',');
                        var name = ldapSplit[0].Replace("CN=", "");
                        var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                        string param = "?" + "computername=" + domain + "\\" + name;
                        Response.Redirect("~/ComputerInfo.aspx" + param);
                    }
                    else if (type.Equals("group"))
                    {
                        string param = "?" + "grouppath="+adpath;
                        Response.Redirect("~/GroupsInfo.aspx" + param );
                    }
                }

            }
            else
            {
                //Is Postback
            }

        }
    }
}