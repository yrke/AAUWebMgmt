using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;
using System.Management;
using Microsoft.Exchange.WebServices.Data;
using ITSWebMgmt.Functions;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Controllers;
using ITSWebMgmt.WebMgmtErrors;

namespace ITSWebMgmt
{
    public partial class UserInfo : System.Web.UI.Page
    {
        private UserController user;
        protected string UserName = "kyrke@its.aau.dk";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ResultDiv.Visible = false;
                errordiv.Visible = false;
                user = new UserController();

                string search = Request.QueryString["search"];
                if (search != null)
                {
                    UserName = HttpUtility.HtmlEncode(search);
                    lookupUser(search.Trim());
                    return;
                }

                string username = Request.QueryString["username"];
                if (username != null)
                {
                    UserName = HttpUtility.HtmlEncode(username);
                    buildUserLookupFromUsername(username);
                    return;
                }

                string phoneNr = Request.QueryString["phone"];
                if (phoneNr != null)
                {
                    UserName = HttpUtility.HtmlEncode(phoneNr);
                    buildUserLookupFromPhone(phoneNr);
                    return;
                }
            }
            else
            {
                string adpath = (string)Session["adpath"];
                user = new UserController();
                user.adpath = adpath;
            }
        }

        protected void lookupUser(string username)
        {
            int val;
            if (username.Length == 4 && int.TryParse(username, out val))
            {
                buildUserLookupFromPhone(username);
            }
            else
            {
                UserNameLabel.Text = username;
                buildUserLookupFromUsername(username);
            }
        }

        protected void buildUserLookupFromPhone(string phone)
        {
            var adpath = user.doPhoneSearch(phone);
            buildUserLookupFromAdpath(adpath);

        }
        protected void buildUserLookupFromUsername(string username)
        {
            string adpath = user.getADPathFromUsername(username);
            buildUserLookupFromAdpath(adpath);
        }

        protected void buildUserLookupFromAdpath(string adpath)
        {

            if (adpath == null)
            {
                buildUserNotFound();
            }
            else
            {
                user.adpath = adpath;
                Session["adpath"] = user.adpath;
                buildUserLookup(adpath);
            }

        }

        protected void buildExchangeLabel(string[] groupsList, bool isTransitiv)
        {
            string transitiv = "";

            if (!isTransitiv)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
            }
            
            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Select Exchange groups and convert to list of ExchangeMailboxGroup
            var exchangeMailboxGroupList = groupsList.Where<string>(group => (group.StartsWith("CN=MBX_"))).Select(x => new ExchangeMailboxGroup(x));       

            foreach (ExchangeMailboxGroup e in exchangeMailboxGroupList)
            {
                var type = e.Type;
                var domain = e.Domain;
                var nameFormated = string.Format("<a href=\"/GroupsInfo.aspx?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + e.RawValue), e.Name);
                var access = e.Access;
                helper.AddRow(new string[] { type, domain, nameFormated, access });
            }

            lblexchange.Text = transitiv + helper.GetTable();
        }

        protected void buildFilesharessegmentLabel(string[] groupsList, bool isTransitiv)
        {
            string transitiv = "";

            if (!isTransitiv)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
            }

            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Filter fileshare groups and convert to Fileshare Objects
            var fileshareList = groupsList.Where<string>((string value)=> {
                var split = value.Split(',');
                var oupath = split.Where<string>(s => s.StartsWith("OU=")).ToArray<string>();
                int count = oupath.Count();

                return ((count == 3 && oupath[count - 1].Equals("OU=Groups") && oupath[count - 2].Equals("OU=Resource Access")));
            }).Select(x => new Fileshare(x));

            foreach (Fileshare f in fileshareList)
            {
                var nameWithLink = string.Format("<a href=\"/GroupsInfo.aspx?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + f.Fileshareraw), f.Name);
                helper.AddRow(new string[] { f.Type, f.Domain, nameWithLink, f.Access });
            }
        
            filesharessegmentLabel.Text = transitiv + helper.GetTable();
        }

        protected void button_toggle_userprofile(object sender, EventArgs e)
        {

            user.toggle_userprofile();
            
            //Set value
            //DirectoryEntry de = result.GetDirectoryEntry();
            //de.Properties["TelephoneNumber"].Clear();
            //de.Properties["employeeNumber"].Value = "123456789";
            //de.CommitChanges();
        }

        protected void fixUserOUButton(object sender, EventArgs e)
        {
            user.fixUserOu();
        }

        protected void buildUserNotFound()
        {
            var builder = new StringBuilder();

            builder.Append("User Not found");
            ResultErrorLabel.Text = builder.ToString();
            ResultDiv.Visible = false;
            errordiv.Visible = true;
        }

        protected async void buildUserLookup(string adpath)
        {
            if (adpath != null)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Async
                var task1 = buildBasicInfoSegment();
                var task2 = BuildSCSMSegment();

                //Build GUI
                labelRawdata.Text = TableGenerator.buildRawTable(user.getAllProperties());

                buildComputerInformation();
                buildWarningSegment();
                buildGroupsSegments();
                buildCalAgenda();
                buildLoginscript();
                buildPrint(); // XXX make async? 

                try { 
                await System.Threading.Tasks.Task.WhenAll(task1, task2);
                } catch(InvalidCastException e)
                {
                    System.Console.Write(e.StackTrace);
                }

                //Save user in session
                ResultDiv.Visible = true;
                errordiv.Visible = false;

                watch.Stop();
                System.Diagnostics.Debug.WriteLine("buildUserLookup took: " + watch.ElapsedMilliseconds);
            }
        }


        private async System.Threading.Tasks.Task  BuildSCSMSegment()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var scsmtest = new SCSMConnector();
            divServiceManager.Text = await scsmtest.getActiveIncidents(user.UserPrincipalName, user.DisplayName);
            var userID = scsmtest.userID;
            Session["scsmuserID"] = userID;
            Session["scsmuserUPN"] = user.UserPrincipalName;
            watch.Stop();
            System.Diagnostics.Debug.WriteLine("BuildSCSMSegment took: " + watch.ElapsedMilliseconds);
        }

        private async System.Threading.Tasks.Task BuildSCSMSegment(DirectoryEntry result)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var scsmtest = new SCSMConnector();
            divServiceManager.Text = await scsmtest.getActiveIncidents((string)result.Properties["userPrincipalName"][0], (string)result.Properties["displayName"][0]);
            var userID = scsmtest.userID;
            Session["scsmuserID"] = userID;
            Session["scsmuserUPN"] = (string)result.Properties["userPrincipalName"][0];
            watch.Stop();
            System.Diagnostics.Debug.WriteLine("BuildSCSMSegment took: " + watch.ElapsedMilliseconds);
        }

        private void buildGroupsSegments()
        {
            var temp = TableGenerator.BuildGroupsSegments(user.getGroups("memberOf"), user.getGroupsTransitive("memberOf"), groupssegmentLabel, groupsAllsegmentLabel);
            var groupListConvert = temp.Item1;
            var groupsListAllConverted = temp.Item2;

            if (groupsListAllConverted.Length > 0)
            {
                buildExchangeLabel(groupsListAllConverted, true);
                buildFilesharessegmentLabel(groupsListAllConverted, true);
            }
            else
            {   //If we dont have transitive data
                buildExchangeLabel(groupListConvert, false);
                buildFilesharessegmentLabel(groupListConvert, false);
            }
        }

        private async System.Threading.Tasks.Task buildBasicInfoSegment()
        {
            //Fills in basic user info
            displayName.Text = user.DisplayName;

            //lblbasicInfoOfficePDS
            if (user.AAUStaffID != null)
            {
                string empID = user.AAUStaffID;

                var pds = new PDSConnector(empID);
                lblbasicInfoDepartmentPDS.Text = pds.Department;
                lblbasicInfoOfficePDS.Text = pds.OfficeAddress;
            }

            //Other fileds
            var attrDisplayName = "UserName, AAU-ID, AAU-UUID, UserStatus, StaffID, StudentID, UserClassification, Telephone, LastLogon (approx.)";
            var attrArry = user.getUserInfo();
            var dispArry = attrDisplayName.Split(',');
            string[] dateFields = { "lastLogon", "badPasswordTime" };

            var sb = new StringBuilder();
            for (int i = 0; i < attrArry.Length; i++)
            {
                string k = attrArry[i];
                sb.Append("<tr>");

                sb.Append(string.Format("<td>{0}</td>", dispArry[i].Trim()));

                if (k != null)
                {
                    sb.Append(string.Format("<td>{0}</td>", k));
                }
                else
                {
                    sb.Append("<td></td>");
                }

                sb.Append("</tr>");
            }

            //Email
            string email = "";
            foreach (string s in user.ProxyAddresses) {
                if (s.StartsWith("SMTP:", StringComparison.CurrentCultureIgnoreCase)){
                    var tmp2 = s.ToLower().Replace("smtp:", "");
                    email += string.Format("<a href=\"mailto:{0}\">{0}</a><br/>", tmp2);
                }
            }
            sb.Append($"<tr><td>E-mails</td><td>{email}</td></tr>");

            //TODO
            const int UF_LOCKOUT = 0x0010;
            int userFlags = (int)user.UserAccountControlComputed;

            //basicInfoPasswordExpired.Text = "False";

            if ((userFlags & UF_LOCKOUT) == UF_LOCKOUT)
            {
            //    basicInfoPasswordExpired.Text = "True";
            }

            if (user.UserPasswordExpiryTimeComputed == null)
            {
                basicInfoPasswordExpireDate.Text = "Never";
            }
            else
            {
                basicInfoPasswordExpireDate.Text = user.UserPasswordExpiryTimeComputed;
            }

            labelBasicInfoTable.Text = sb.ToString();

            var admdb = new ADMdbConnector();

            string upn = user.UserPrincipalName;

            string firstName = user.GivenName;
            string lastName = user.SN;

            var tmp = upn.Split('@');
            var domain = tmp[1].Split('.')[0];

            //Make lookup in ADMdb
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //basicInfoAdmDBExpireDate.Text = await admdb.loadUserExpiredate(domain, tmp[0], firstName, lastName);
            //watch.Stop();
            //System.Diagnostics.Debug.WriteLine("ADMdb Lookup took: " + watch.ElapsedMilliseconds);

            //Has roaming
            labelBasicInfoRomaing.Text = "false";
            if (user.Profilepath != null)
            {
                labelBasicInfoRomaing.Text = "true";
            }

            //Password Expire date "PasswordExpirationDate"
        }
        private void buildCalAgenda()
        {
            var sb = new StringBuilder();
            // Display available meeting times.

            DateTime now = DateTime.Now;
            foreach (AttendeeAvailability availability in user.getFreeBusyResults().AttendeesAvailability)
            {

                foreach (CalendarEvent calendarItem in availability.CalendarEvents)
                {
                    if (calendarItem.FreeBusyStatus != LegacyFreeBusyStatus.Free)
                    {

                        bool isNow = false;
                        if (now > calendarItem.StartTime && calendarItem.EndTime > now)
                        {
                            sb.Append("<b>");
                            isNow = true;
                        }
                        sb.Append(string.Format("{0}-{1}: {2}<br/>", calendarItem.StartTime.ToString("HH:mm"), calendarItem.EndTime.ToString("HH:mm"), calendarItem.FreeBusyStatus));

                        if (isNow)
                        {
                            sb.Append("</b>");
                        }
                    }
                }
            }

            lblcalAgenda.Text = sb.ToString();
        }

        private void buildComputerInformation()
        {
            try
            {
                string upn = user.UserPrincipalName;
                string[] upnsplit = upn.Split('@');
                string domain = upnsplit[1].Split('.')[0];

                string userName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

                var helper = new HTMLTableHelper(new string[] { "Computername", "AAU Fjernsupport" });

                foreach (ManagementObject o in user.getUserMachineRelationshipFromUserName(userName))
                {
                    var machinename = o.Properties["ResourceName"].Value.ToString();
                    var name = "<a href=\"/computerInfo.aspx?computername=" + machinename + "\">" + machinename + "</a><br />";
                    var fjernsupport = "<a href=\"https://support.its.aau.dk/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&client.hostname=" + machinename + "\">Start</a>";
                    helper.AddRow(new string[] { name, fjernsupport });
                }
                divComputerInformation.Text = "<h4>Links til computerinfo kan være til maskiner i et forkert domæne, da info omkring computer domæne ikke er tilgængelig i denne søgning</h4>" + helper.GetTable();
            }
            catch(System.UnauthorizedAccessException e)
            {
                divComputerInformation.Text = "Service user does not have SCCM access.";
            }
        }

        private void buildWarningSegment()
        {
            List<WebMgmtError> errors = new List<WebMgmtError>
            {
                new UserDisabled(user),
                new UserLockedDiv(user),
                new PasswordExpired(user),
                new MissingAAUAttr(user),
                new NotStandardOU(user)
            };

            var errorList = new WebMgmtErrorList(errors);
            ErrorCountMessageLabel.Text = errorList.getErrorCountMessage();
            ErrorMessagesLabel.Text = errorList.ErrorMessages;

            if (user.userIsInRightOU())
            {
                divFixuserOU.Visible = false;
            }
            //Password is expired and warning before expire (same timeline as windows displays warning)
        }

        protected void unlockUserAccountButton_Click(object sender, EventArgs e)
        {
            user.unlockUserAccount();
        }

        protected void createNewIRSR_Click(object sender, EventArgs e)
        {
            string userID = (string)Session["scsmuserID"];
            string upn = (string)Session["scsmuserUPN"];

            Response.Redirect("/CreateWorkItem.aspx?userID=" + userID + "&userDisplayName=" + upn);
        }

        protected void buildPrint()
        {
            PrintConnector printConnector = new PrintConnector(user.Guid.ToString());

            lblPrint.Text = printConnector.doStuff();
        }

        protected void buildLoginscript()
        {
            menuLoginScript.Visible = false;

            var loginscripthelper = new Loginscript();

            var script = loginscripthelper.getLoginScript(user.ScriptPath, user.Path);

            if (script != null) {
                menuLoginScript.Visible = true;
                labelLoginscript.Text = loginscripthelper.parseAndDisplayLoginScript(script);
            }
        }
    }
}