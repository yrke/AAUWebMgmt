using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace ITSWebMgmt
{
    public partial class test : System.Web.UI.Page
    {



        /// <summary>
        /// Checks if a person/group of people are free for a specified time
        /// </summary>
        /// <param name="People">The people to check on</param>
        /// <param name="StartTime">The start time</param>
        /// <param name="EndTime">The end time</param>
        /// <param name="Interval">The interval in minutes at which to check (the minimum is 10 minutes)</param>
        /// <param name="UserName">The username of the person checking</param>
        /// <param name="Password">The password of the person checking</param>
        /// <param name="TimeZoneDifference">Time zone difference</param>
        /// <param name="Server">Server name</param>
        /// <returns>returns an array of bytes stating whether someone is free or not, a 1 means they are not free during that time and a 0 means they are free</returns>
        public static byte[] GetFreeBusyData(StringDictionary People, DateTime StartTime, DateTime EndTime, int Interval,
            string UserName, string Password, string Server, string TimeZoneDifference)
        {
            if (People.Count < 1)
                return null;
            if (StartTime >= EndTime)
                return null;
            string EmailAddresses = "";

            foreach (string Person in People.Values)
            {
                EmailAddresses += "&u=SMTP:" + Person;
            }

            string Url = Server + "/public/?cmd=freebusy&start=" + StartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + TimeZoneDifference + "&end=" + EndTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + TimeZoneDifference + "&interval=" + Interval.ToString() + EmailAddresses;

            System.Net.CredentialCache MyCredentialCache = new System.Net.CredentialCache();
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
            {
                MyCredentialCache.Add(new System.Uri(Url),
                                    "NTLM",
                                    new System.Net.NetworkCredential(UserName, Password, "")
                                    );
            }
            else
            {
                MyCredentialCache.Add(new System.Uri(Url),
                                       "Negotiate",
                                       (System.Net.NetworkCredential)CredentialCache.DefaultCredentials);
            }

            System.Net.HttpWebRequest Request = (System.Net.HttpWebRequest)HttpWebRequest.Create(Url);
            Request.Credentials = MyCredentialCache;
            Request.Method = "GET";
            Request.ContentType = "text/xml; encoding='utf-8'";
            Request.UserAgent = "Mozilla/4.0(compatible;MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; InfoPath.1)";
            Request.KeepAlive = true;
            Request.AllowAutoRedirect = false;

            System.Net.WebResponse Response = (HttpWebResponse)Request.GetResponse();
            byte[] bytes = null;
            using (System.IO.Stream ResponseStream = Response.GetResponseStream())
            {
                XmlDocument ResponseXmlDoc = new XmlDocument();
                ResponseXmlDoc.Load(ResponseStream);
                XmlNodeList DisplayNameNodes = ResponseXmlDoc.GetElementsByTagName("a:item");
                if (DisplayNameNodes.Count > 0)
                {
                    for (int i = 0; i < DisplayNameNodes.Count; i++)
                    {
                        XmlNodeList Nodes = DisplayNameNodes[i].ChildNodes;
                        foreach (XmlNode Node in Nodes)
                        {
                            if (Node.Name == "a:fbdata")
                            {
                                bytes = new byte[Node.InnerText.Length];
                                for (int x = 0; x < Node.InnerText.Length; ++x)
                                {
                                    bytes[x] = byte.Parse(Node.InnerText[x].ToString());
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Could not get free/busy data from the exchange server");
                }
                // Clean up.
                ResponseStream.Close();
                Response.Close();
            }
            return bytes;
        }



        protected void Page_Load(object sender, EventArgs e)
        {

            
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
            service.UseDefaultCredentials = true; // Use domain account for connecting 
            //service.Credentials = new WebCredentials("user1@contoso.com", "password"); // used if we need to enter a password, but for now we are using domain credentials
            //service.AutodiscoverUrl("kyrke@its.aau.dk");  //XXX we should use the service user for webmgmt!
            service.Url = new System.Uri("https://mail.aau.dk/EWS/exchange.asmx");

            List<AttendeeInfo> attendees = new List<AttendeeInfo>();

            attendees.Add(new AttendeeInfo()
            {
                SmtpAddress = "kyrke@its.aau.dk",
                AttendeeType = MeetingAttendeeType.Organizer
            });


            // Specify availability options.
            AvailabilityOptions myOptions = new AvailabilityOptions();

            myOptions.MeetingDuration = 30;
            myOptions.RequestedFreeBusyView = FreeBusyViewType.FreeBusy;

            // Return a set of free/busy times.
            DateTime dayBegin = DateTime.Now.Date;
            var window = new TimeWindow(dayBegin, dayBegin.AddDays(1));
            GetUserAvailabilityResults freeBusyResults = service.GetUserAvailability(attendees,
                                                                                 window,
                                                                                     AvailabilityData.FreeBusy,
                                                                                     myOptions);

            var sb = new StringBuilder();
            // Display available meeting times.
            sb.Append(string.Format("<h2>Agenda</h2>"));

            DateTime now = DateTime.Now;
            foreach (AttendeeAvailability availability in freeBusyResults.AttendeesAvailability)
            {
               
                foreach (CalendarEvent calendarItem in availability.CalendarEvents)
                {
                    if (calendarItem.FreeBusyStatus!=LegacyFreeBusyStatus.Free) {

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

            
            lblResult.Text = sb.ToString();
        }

        protected void onSubmit_Click(object sender, EventArgs e)
        {




        }
    }
}