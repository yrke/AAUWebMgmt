using ITSWebMgmt.Caches;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Functions;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.WebMgmtErrors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.Controllers
{
    public class UserController : WebMgmtController<UserADcache>
    {
        public IActionResult Index(string username)
        {
            if (username != null)
            {
                if (!_cache.TryGetValue(username, out UserModel))
                {
                    username = username.Trim();
                    UserModel = new UserModel(this, lookupUser(username));
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(username, UserModel, cacheEntryOptions);
                }
            }
            else
            {
                UserModel = new UserModel(this, username);
            }

            return View(UserModel);
        }

        private IMemoryCache _cache;
        public UserModel UserModel;

        public UserController(IMemoryCache cache)
        {
            _cache = cache;
        }

        protected string lookupUser(string username)
        {
            int val;
            if (username.Length == 4 && int.TryParse(username, out val))
            {
                return doPhoneSearch(username);
            }
            else
            {
                UserModel.UserName = username;
                return getADPathFromUsername(username);
            }
        }

        public string globalSearch(string email)
        {
            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            string filter = string.Format("(|(proxyaddresses=SMTP:{0})(userPrincipalName={0}))", email);

            DirectorySearcher search = new DirectorySearcher(de, filter);
            SearchResult r = search.FindOne();

            if (r != null)
            {
                //return r.Properties["userPrincipalName"][0].ToString(); //XXX handle if result is 0 (null exception)
                string adpath = r.Properties["ADsPath"][0].ToString();
                return adpath.Replace("aau.dk/", "").Replace("GC:", "LDAP:");
            }
            else
            {
                return null;
            }
        }

        //Searhces on a phone numer (internal or external), and returns a upn (later ADsPath) to a use or null if not found
        public string doPhoneSearch(string numberIn)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string number = numberIn;
            //If number is a shot internal number, expand it :)
            if (number.Length == 4)
            {
                // format is 3452
                number = string.Format("+459940{0}", number);

            }
            else if (number.Length == 6)
            {
                //format is +453452 
                number = string.Format("+459940{0}", number.Replace("+45", ""));

            }
            else if (number.Length == 8)
            {
                //format is 99403442
                number = string.Format("+45{0}", number);

            } // else format is ok

            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            //string filter = string.Format("(&(objectCategory=person)(telephoneNumber={0}))", number);
            string filter = string.Format("(&(objectCategory=person)(objectClass=user)(telephoneNumber={0}))", number);

            //logger.Debug(filter);

            DirectorySearcher search = new DirectorySearcher(de, filter);
            search.PropertiesToLoad.Add("userPrincipalName"); //Load something to speed up the object get?
            SearchResult r = search.FindOne();

            watch.Stop();
            System.Diagnostics.Debug.WriteLine("phonesearch took: " + watch.ElapsedMilliseconds);

            if (r != null)
            {
                //return r.Properties["ADsPath"][0].ToString(); //XXX handle if result is 0 (null exception)
                return r.Properties["ADsPath"][0].ToString().Replace("GC:", "LDAP:").Replace("aau.dk/", "");
            }
            else
            {
                return null;
            }
        }

        public bool userIsInRightOU()
        {
            string dn = UserModel.DistinguishedName;
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray();

            int count = ou.Count();
            if (count < 2)
            {
                return false;
            }
            //Check root is people
            if (!(ou[count - 1]).Equals("ou=people", StringComparison.CurrentCultureIgnoreCase))
            {
                //Error user is not placed in people!!!!! Cant move the user (might not be a real user or admin or computer)
                return false;
            }
            string[] okplaces = new string[3] { "ou=staff", "ou=guests", "ou=students" };
            if (!okplaces.Contains(ou[count - 2], StringComparer.OrdinalIgnoreCase))
            {
                //Error user is not in out staff, people or student, what is gowing on here?
                return false;
            }
            if (count > 2)
            {
                return false;
            }
            return true;
        }

        public bool fixUserOu()
        {
            if (userIsInRightOU()) { return false; }

            //See if it can be fixed!
            string dn = UserModel.DistinguishedName;
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray();

            int count = ou.Count();

            if (count < 2)
            {
                //This cant be in people/{staff,student,guest}
                return false;
            }
            //Check root is people
            if (!(ou[count - 1]).Equals("ou=people", StringComparison.CurrentCultureIgnoreCase))
            {
                //Error user is not placed in people!!!!! Cant move the user (might not be a real user or admin or computer)
                return false;
            }
            string[] okplaces = new string[3] { "ou=staff", "ou=guests", "ou=students" };
            if (!okplaces.Contains(ou[count - 2], StringComparer.OrdinalIgnoreCase))
            {
                //Error user is not in out staff, people or student, what is gowing on here?
                return false;
            }
            if (count > 2)
            {
                //User is not placed in people/{staff,student,guest}, but in a sub ou, we need to do somthing!
                //from above check we know the path is people/{staff,student,guest}, lets generate new OU

                //Format ldap://DOMAIN/pathtoOU
                //return false; //XX Return false here?

                string[] adpathsplit = adpath.ToLower().Replace("ldap://", "").Split('/');
                string protocol = "LDAP://";
                string domain = adpathsplit[0];
                string[] dcpath = (adpathsplit[0].Split(',')).Where<string>(s => s.StartsWith("dc=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();

                string newOU = string.Format("{0},{1}", ou[count - 2], ou[count - 1]);
                string newPath = string.Format("{0}{1}/{2},{3}", protocol, string.Join(".", dcpath).Replace("dc=", ""), newOU, string.Join(",", dcpath));

                logger.Info("user " + ControllerContext.HttpContext.User.Identity.Name + " changed OU on user to: " + newPath + " from " + adpath + ".");

                var newLocaltion = new DirectoryEntry(newPath);
                ADcache.DE.MoveTo(newLocaltion);

                return true;
            }
            //We don't need to do anything, user is placed in the right ou! (we think, can still be in wrong ou fx a guest changed to staff, we cant check that here) 
            logger.Debug("no need to change user {0} out, all is good", adpath);
            return true;
        }

        public string getADPathFromUsername(string username)
        {
            //XXX, this is a use input, might not be save us use in log 
            logger.Info("User {0} lookedup user {1}", ControllerContext.HttpContext.User.Identity.Name, username);

            if (username.Contains("\\"))
            {
                //Form is domain\useranme
                var tmp = username.Split('\\');
                if (!tmp[0].Equals("AAU", StringComparison.CurrentCultureIgnoreCase))
                {
                    username = string.Format("{0}@{1}.aau.dk", tmp[1], tmp[0]);
                }
                else //IS AAU domain 
                {
                    username = string.Format("{0}@{1}.dk", tmp[1], tmp[0]);
                }
            }

            var adpath = globalSearch(username);
            if (adpath == null)
            {
                //Show user not found
                return null;
            }
            else
            {
                //We got ADPATH lets build the GUI
                return adpath;
            }
        }

        public void unlockUserAccount()
        {
            logger.Info("User {0} unlocked useraccont {1}", ControllerContext.HttpContext.User.Identity.Name, adpath);
            
            ADcache.DE.Properties["LockOutTime"].Value = 0; //unlock account
            ADcache.DE.CommitChanges(); //may not be needed but adding it anyways
            ADcache.DE.Close();
        }

        public async Task<GetUserAvailabilityResults> getFreeBusyResultsAsync()
        {
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
            service.UseDefaultCredentials = true; // Use domain account for connecting 
            //service.Credentials = new WebCredentials("user1@contoso.com", "password"); // used if we need to enter a password, but for now we are using domain credentials
            //service.AutodiscoverUrl("kyrke@its.aau.dk");  //XXX we should use the service user for webmgmt!
            service.Url = new Uri("https://mail.aau.dk/EWS/exchange.asmx");

            List<AttendeeInfo> attendees = new List<AttendeeInfo>();

            attendees.Add(new AttendeeInfo()
            {
                SmtpAddress = UserModel.UserPrincipalName,
                AttendeeType = MeetingAttendeeType.Organizer
            });

            // Specify availability options.
            AvailabilityOptions myOptions = new AvailabilityOptions();

            myOptions.MeetingDuration = 30;
            myOptions.RequestedFreeBusyView = FreeBusyViewType.FreeBusy;

            // Return a set of free/busy times.
            DateTime dayBegin = DateTime.Now.Date;
            var window = new TimeWindow(dayBegin, dayBegin.AddDays(1));
            return await service.GetUserAvailability(attendees, window, AvailabilityData.FreeBusy, myOptions);
        }

        public void toggle_userprofile()
        {
            //XXX log what the new value of profile is :)
            logger.Info("User {0} toggled romaing profile for user  {1}", ControllerContext.HttpContext.User.Identity.Name, adpath);

            //string profilepath = (string)(ADcache.DE.Properties["profilePath"])[0];

            if (ADcache.DE.Properties.Contains("profilepath"))
            {
                ADcache.DE.Properties["profilePath"].Clear();
                ADcache.DE.CommitChanges();
            }
            else
            {
                string upn = UserModel.UserPrincipalName;
                var tmp = upn.Split('@');

                string path = string.Format("\\\\{0}\\profiles\\{1}", tmp[1], tmp[0]);

                ADcache.DE.Properties["profilePath"].Add(path);
                ADcache.DE.CommitChanges();
            }
        }

        protected void buildExchangeLabel(List<string> groupsList, bool isTransitiv)
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

            UserModel.Exchange = transitiv + helper.GetTable();
        }

        protected void buildFilesharessegmentLabel(List<string> groupsList, bool isTransitiv)
        {
            string transitiv = "";

            if (!isTransitiv)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
            }

            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Filter fileshare groups and convert to Fileshare Objects
            var fileshareList = groupsList.Where((string value) => {
                return GroupController.isFileShare(value);
            }).Select(x => new Fileshare(x));

            foreach (Fileshare f in fileshareList)
            {
                var nameWithLink = string.Format("<a href=\"/GroupsInfo.aspx?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + f.Fileshareraw), f.Name);
                helper.AddRow(new string[] { f.Type, f.Domain, nameWithLink, f.Access });
            }

            UserModel.Filesharessegment = transitiv + helper.GetTable();
        }

        protected void button_toggle_userprofile(object sender, EventArgs e)
        {

            toggle_userprofile();

            //Set value
            //DirectoryEntry de = result.GetDirectoryEntry();
            //de.Properties["TelephoneNumber"].Clear();
            //de.Properties["employeeNumber"].Value = "123456789";
            //de.CommitChanges();
        }

        protected void fixUserOUButton(object sender, EventArgs e)
        {
            fixUserOu();
        }

        public void buildUserNotFound()
        {
            var builder = new StringBuilder();

            builder.Append("User Not found");
            UserModel.ResultError = builder.ToString();
            UserModel.ShowResultDiv = false;
            UserModel.ShowErrorDiv = true;
        }

        public async void buildUserLookup(string adpath)
        {
            if (adpath != null)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Async
                var task1 = buildBasicInfoSegment();
                var task2 = BuildSCSMSegment();

                //Build GUI
                UserModel.Rawdata = TableGenerator.buildRawTable(UserModel.ADcache.getAllProperties());

                buildComputerInformation();
                buildWarningSegment();
                buildGroupsSegments();
                buildCalAgendaAsync();
                buildLoginscript();
                buildPrint(); // XXX make async? 

                //TODO: Fix bug and remove try catch
                try
                {
                    await System.Threading.Tasks.Task.WhenAll(task1, task2);
                }
                catch (InvalidCastException e)
                {
                    System.Console.Write(e.StackTrace);
                }

                //Save user in session
                UserModel.ShowResultDiv = true;
                UserModel.ShowErrorDiv = false;

                watch.Stop();
                System.Diagnostics.Debug.WriteLine("buildUserLookup took: " + watch.ElapsedMilliseconds);
            }
        }


        private async System.Threading.Tasks.Task BuildSCSMSegment()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var scsmtest = new SCSMConnector();
            UserModel.ServiceManager = await scsmtest.getActiveIncidents(UserModel.UserPrincipalName, UserModel.DisplayName);
            var userID = scsmtest.userID;
            /*Session["scsmuserID"] = userID;
            Session["scsmuserUPN"] = UserModel.UserPrincipalName;*/
            watch.Stop();
            System.Diagnostics.Debug.WriteLine("BuildSCSMSegment took: " + watch.ElapsedMilliseconds);
        }

        private async System.Threading.Tasks.Task BuildSCSMSegment(DirectoryEntry result)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var scsmtest = new SCSMConnector();
            UserModel.ServiceManager = await scsmtest.getActiveIncidents((string)result.Properties["userPrincipalName"][0], (string)result.Properties["displayName"][0]);
            var userID = scsmtest.userID;
            /*Session["scsmuserID"] = userID;
            Session["scsmuserUPN"] = (string)result.Properties["userPrincipalName"][0];*/
            watch.Stop();
            System.Diagnostics.Debug.WriteLine("BuildSCSMSegment took: " + watch.ElapsedMilliseconds);
        }

        private void buildGroupsSegments()
        {
            var groupList = UserModel.ADcache.getGroups("memberOf");
            var groupsListAll = UserModel.ADcache.getGroupsTransitive("memberOf");
            UserModel.GroupSegment = TableGenerator.BuildgroupssegmentLabel(groupList);
            UserModel.GroupsAllSegment = TableGenerator.BuildgroupssegmentLabel(groupsListAll);

            if (groupsListAll.Count > 0)
            {
                buildExchangeLabel(groupsListAll, true);
                buildFilesharessegmentLabel(groupsListAll, true);
            }
            else
            {   //If we dont have transitive data
                buildExchangeLabel(groupList, false);
                buildFilesharessegmentLabel(groupList, false);
            }
        }

        private async System.Threading.Tasks.Task buildBasicInfoSegment()
        {
            //lblbasicInfoOfficePDS
            if (UserModel.AAUStaffID != null)
            {
                string empID = UserModel.AAUStaffID;

                var pds = new PDSConnector(empID);
                UserModel.BasicInfoDepartmentPDS = pds.Department;
                UserModel.BasicInfoOfficePDS = pds.OfficeAddress;
            }

            //Other fileds
            var attrDisplayName = "UserName, AAU-ID, AAU-UUID, UserStatus, StaffID, StudentID, UserClassification, Telephone, LastLogon (approx.)";
            var attrArry = UserModel.getUserInfo();
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
            foreach (string s in UserModel.ProxyAddresses)
            {
                if (s.StartsWith("SMTP:", StringComparison.CurrentCultureIgnoreCase))
                {
                    var tmp2 = s.ToLower().Replace("smtp:", "");
                    email += string.Format("<a href=\"mailto:{0}\">{0}</a><br/>", tmp2);
                }
            }
            sb.Append($"<tr><td>E-mails</td><td>{email}</td></tr>");

            const int UF_LOCKOUT = 0x0010;
            int userFlags = UserModel.UserAccountControlComputed;

            UserModel.BasicInfoPasswordExpired = "False";

            if ((userFlags & UF_LOCKOUT) == UF_LOCKOUT)
            {
                UserModel.BasicInfoPasswordExpired = "True";
            }

            if (UserModel.UserPasswordExpiryTimeComputed == "")
            {
                UserModel.BasicInfoPasswordExpireDate = "Never";
            }
            else
            {
                UserModel.BasicInfoPasswordExpireDate = UserModel.UserPasswordExpiryTimeComputed;
            }

            UserModel.BasicInfoTable = sb.ToString();

            var admdb = new ADMdbConnector();

            string upn = UserModel.UserPrincipalName;

            string firstName = UserModel.GivenName;
            string lastName = UserModel.SN;

            var tmp = upn.Split('@');
            var domain = tmp[1].Split('.')[0];

            //Make lookup in ADMdb
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //basicInfoAdmDBExpireDate.Text = await admdb.loadUserExpiredate(domain, tmp[0], firstName, lastName);
            //watch.Stop();
            //System.Diagnostics.Debug.WriteLine("ADMdb Lookup took: " + watch.ElapsedMilliseconds);

            //Has roaming
            UserModel.BasicInfoRomaing = "false";
            if (UserModel.Profilepath != null)
            {
                UserModel.BasicInfoRomaing = "true";
            }

            //Password Expire date "PasswordExpirationDate"
        }
        private async System.Threading.Tasks.Task buildCalAgendaAsync()
        {
            var sb = new StringBuilder();
            // Display available meeting times.

            var temp = await getFreeBusyResultsAsync();

            DateTime now = DateTime.Now;
            foreach (AttendeeAvailability availability in temp.AttendeesAvailability)
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

            UserModel.CalAgenda = sb.ToString();
        }

        private void buildComputerInformation()
        {
            try
            {
                string upn = UserModel.UserPrincipalName;
                string[] upnsplit = upn.Split('@');
                string domain = upnsplit[1].Split('.')[0];

                string userName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

                var helper = new HTMLTableHelper(new string[] { "Computername", "AAU Fjernsupport" });

                foreach (ManagementObject o in UserModel.getUserMachineRelationshipFromUserName(userName))
                {
                    var machinename = o.Properties["ResourceName"].Value.ToString();
                    var name = "<a href=\"/computerInfo.aspx?computername=" + machinename + "\">" + machinename + "</a><br />";
                    var fjernsupport = "<a href=\"https://support.its.aau.dk/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&client.hostname=" + machinename + "\">Start</a>";
                    helper.AddRow(new string[] { name, fjernsupport });
                }
                UserModel.ComputerInformation = "<h4>Links til computerinfo kan være til maskiner i et forkert domæne, da info omkring computer domæne ikke er tilgængelig i denne søgning</h4>" + helper.GetTable();
            }
            catch (System.UnauthorizedAccessException e)
            {
                UserModel.ComputerInformation = "Service user does not have SCCM access.";
            }
        }

        private void buildWarningSegment()
        {
            List<WebMgmtError> errors = new List<WebMgmtError>
            {
                new UserDisabled(this),
                new UserLockedDiv(this),
                new PasswordExpired(this),
                new MissingAAUAttr(this),
                new NotStandardOU(this)
            };

            var errorList = new WebMgmtErrorList(errors);
            UserModel.ErrorCountMessage = errorList.getErrorCountMessage();
            UserModel.ErrorMessages = errorList.ErrorMessages;

            if (userIsInRightOU())
            {
                UserModel.ShowFixUserOU = false;
            }
            //Password is expired and warning before expire (same timeline as windows displays warning)
        }

        protected void unlockUserAccountButton_Click(object sender, EventArgs e)
        {
            unlockUserAccount();
        }

        protected void createNewIRSR_Click(object sender, EventArgs e)
        {
            /*string userID = (string)Session["scsmuserID"];
            string upn = (string)Session["scsmuserUPN"];

            Response.Redirect("/CreateWorkItem.aspx?userID=" + userID + "&userDisplayName=" + upn);*/
        }

        protected void buildPrint()
        {
            PrintConnector printConnector = new PrintConnector(UserModel.Guid.ToString());

            UserModel.Print = printConnector.doStuff();
        }

        protected void buildLoginscript()
        {
            UserModel.ShowResultDiv = false;

            var loginscripthelper = new Loginscript();

            var script = loginscripthelper.getLoginScript(UserModel.ScriptPath, UserModel.ADcache.Path);

            if (script != null)
            {
                UserModel.ShowResultDiv = true;
                UserModel.Loginscript = loginscripthelper.parseAndDisplayLoginScript(script);
            }
        }
    }
}