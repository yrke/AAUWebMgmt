using System;
using System.DirectoryServices;
using ITSWebMgmt.Functions;
using ITSWebMgmt.Connectors.Active_Directory;

namespace ITSWebMgmt
{
    public partial class test : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            var group = "CN=kyrketestgroup,OU=Manual,OU=Groups,DC=its,DC=aau,DC=dk";
            var user = "CN=kyrke,OU=test,OU=Staff,OU=People,DC=its,DC=aau,DC=dk";

            //ADHelpers.addADUserToGroup(user, group);
            //ADHelpers.remoteADUserFromGroup(user, group);

            /*var userinfo = new UserInfo();

            //userinfo.get
            var adpath = "LDAP://adm.aau.dk/CN=XXXXXXXXX,OU=Staff,OU=People,DC=adm,DC=aau,DC=dk";

            var userDE = new DirectoryEntry(adpath);

            var a = new Loginscript();

            var script = a.getLoginScript(userDE);

            lblResult.Text = a.parseAndDisplayLoginScript(script);


            */

            /*PrincipalContext pc = new PrincipalContext(ContextType.Domain);
            UserPrincipal user = UserPrincipal.FindByIdentity(pc, "kyrke@its.aau.dk");
            var groups = user.GetGroups();// or user.GetUserGroups() 
            var sb = new StringBuilder();
            var groupsCount = 0;
            foreach (var g in groups)
            {
                sb.Append(g.ToString()+"<br />");groupsCount++;

            }
            sb.Append("<br/>" + groupsCount);
            
             */



            /* 
             var sb = new StringBuilder();

             DirectoryEntry deBase = new DirectoryEntry("GC://aau.dk");


             DirectorySearcher dsLookFor = new DirectorySearcher(deBase);
             dsLookFor.Filter = "(member:1.2.840.113556.1.4.1941:=CN=kyrke,OU=test,OU=staff,OU=people,DC=its,DC=aau,DC=dk)";
             dsLookFor.SearchScope = SearchScope.Subtree;
             dsLookFor.PropertiesToLoad.Add("cn");

             SearchResultCollection srcGroups = dsLookFor.FindAll();
             */
            /* Just to know if user is explicitly in group
             */
            /*foreach (SearchResult srcGroup in srcGroups)
            {
                sb.Append(string.Format("{0}<br/>", srcGroup.Path));*/

            /*foreach (string property in srcGroup.Properties.PropertyNames)
            {
                sb.Append(string.Format("\t{0} : {1} ", property, srcGroup.Properties[property][0]));
            }

            DirectoryEntry aGroup = new DirectoryEntry(srcGroup.Path);
            DirectorySearcher dsLookForAMermber = new DirectorySearcher(aGroup);
            dsLookForAMermber.Filter = "(member=CN=kyrke,OU=test,OU=staff,OU=people,DC=its,DC=aau,DC=dk)";
            dsLookForAMermber.SearchScope = SearchScope.Base;
            dsLookForAMermber.PropertiesToLoad.Add("cn");

            SearchResultCollection memberInGroup = dsLookForAMermber.FindAll();
            sb.Append(string.Format("Find the user {0}", memberInGroup.Count));
            */
            /*}

            

              lblResult.Text = sb.ToString();*/

        }

        protected void onSubmit_Click(object sender, EventArgs e)
        {




        }
    }
}