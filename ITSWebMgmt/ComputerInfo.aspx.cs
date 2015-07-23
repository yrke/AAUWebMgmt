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
    public partial class ComputerInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["adpath"] == null)
            {
               // ResultGetPassword.Enabled = false;
            }

        }


        protected String getLocalAdminPassword(String adobject)
        {
            DirectoryEntry de = new DirectoryEntry(adobject);
            DirectorySearcher search = new DirectorySearcher(de);
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            SearchResult r = search.FindOne();

            if (r != null)
            {
                return r.Properties["ms-Mcs-AdmPwd"][0].ToString();
            }
            else { 
                return null;
            }
        }

        protected void lookupComputer(object sender, EventArgs e)
        {

            var computerName = ComputerNameInput.Text;

            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            string filter = string.Format("(&(objectClass=computer)(cn={0}))", computerName);
            //string filter = string.Format("(name={0})", computerName);


            DirectorySearcher search = new DirectorySearcher(de);
            //DirectorySearcher search = new DirectorySearcher(filter);
            search.Filter = filter;
            search.PropertiesToLoad.Add("distinguishedName");
            

            SearchResult r = search.FindOne();

            var distinguishedName = r.Properties["distinguishedName"][0].ToString();
            var split = distinguishedName.Split(',');

            var len = split.GetLength(0);
            var domain = (split[len-3] + "." + split[len-2] + "." + split[len-1]).Replace("DC=","");


            de = new DirectoryEntry("LDAP://"+domain);
            //de.Username = "its\\kyrke";
            //de.Password = "";
            search.PropertiesToLoad.Add("cn");

            search = new DirectorySearcher(de);
            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            search.PropertiesToLoad.Add("ms-Mcs-AdmPwdExpirationTime");
            search.PropertiesToLoad.Add("memberOf");

            search.Filter = string.Format("(&(objectClass=computer)(distinguishedName={0}))", distinguishedName);

            var builder = new StringBuilder();
            r = search.FindOne();
            if (r != null)
            {

                //builder.Append((result.Properties["aauStaffID"][0])).ToString();

                foreach (string k in search.PropertiesToLoad)
                {
                    builder.Append(k);

                    var a = r.Properties[k];
                    if (a != null && a.Count > 0)
                    {
                        foreach (var j in a){
                            var value = (j.ToString());
                            builder.Append(" : " + value);
                        }
                    }
                    builder.Append("<br />");
                }

            }

            long rawDate = (long)r.Properties["ms-Mcs-AdmPwdExpirationTime"][0];
            DateTime expireDate = DateTime.FromFileTime(rawDate);
            builder.Append(expireDate);


            String adpath = r.Properties["ADsPath"][0].ToString();

            Session["adpath"] = adpath;

            builder.Append((string)Session["adpath"]);

            ResultLabel.Text = builder.ToString();
        }

        protected void ResultGetPassword_Click(object sender, EventArgs e)
        {
            String adpath = (string)Session["adpath"];

            var passwordRetuned = this.getLocalAdminPassword(adpath);

            ResultLabel.Text = "Fisk" + passwordRetuned;
        
        }

        
    }
}