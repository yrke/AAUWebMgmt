using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ITSWebMgmt.Connectors
{
    public class SCSMConnector
    {
        string webserviceURL = "https://service.aau.dk";
        //string webserviceURL = "http://scsm-tms1.srv.aau.dk";

        public string userID = "";

        static readonly string idForConvertedToSR = "d283d1f2-5660-d28e-f0a3-225f621394a9";

        protected async Task<string> getAuthKey()
        {

            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/Authorization/GetToken");
            request.Method = "POST";
            request.ContentType = "text/json";

            string domain = ConfigurationManager.AppSettings["cred:scsm:domain"];
            string username = ConfigurationManager.AppSettings["cred:scsm:username"];
            string secret = ConfigurationManager.AppSettings["cred:scsm:password"];

            if (domain == null || username == null || secret == null)
            {
                return null;
            }

            //string json = "{\"Username\": \"srv\\\\svc_webmgmt-scsm\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";
            //string json = "{\"Username\": \"its\\\\kyrke\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";
            //string json = "{\"Username\": \"its\\\\svc_webmgmt-scsm\",\"Domain\": \"its\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";

            //FOrmat string json = "{\"Username\": \"its\\\\svc_webmgmt-scsm3\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";
            string json = "{\"Username\": \""+domain +"\\\\"+username+"\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";

            var requestSteam = new StreamWriter(request.GetRequestStream());
            requestSteam.Write(json);
            requestSteam.Flush();
            requestSteam.Close();

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            //responseLbl.Text = responseText;
            return responseText.Replace("\"", "");
        }

        protected string doAction(string userjson)
        {
            //Print the user info! 
            var sb = new StringBuilder();

            if (userjson == null)
            {
                sb.Append("User not found i SCSM");
                return sb.ToString();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = Int32.MaxValue;
            dynamic json = js.Deserialize<dynamic>(userjson);

            sb.Append("<br /><br />" + string.Format("<a href=\"{0}{1}\">Servie Portal User Info</a>", "https://service.aau.dk/user/UserRelatedInfoById/", userID) + "<br/>");


            //Open Cases
            sb.Append("<h1>Open Requests</h1><br />");
            bool isOpenFilter(string status) => !("Closed".Equals(status));
            sb.Append(PrintTableOfCases((object)json, isOpenFilter));

            //Closed Cases
            sb.Append("<br /><br /><h3>Closed Requests</h3>");
            bool isClosedFilter(string status) => "Closed".Equals(status);
            sb.Append(PrintTableOfCases((object)json, isClosedFilter));


            //sb.Append("<br /><br/>IsAssignedToUser<br />");
            //foreach (dynamic s in json["IsAssignedToUser"])
            //for (int i = 0; i < json["IsAssignedToUser"].Length; i++)
            //{
            //    sb.Append(json["IsAssignedToUser"][i]["Id"] + " -" + json["IsAssignedToUser"][i]["DisplayName"] + " - " + json["IsAssignedToUser"][i]["Status"]["Name"] + "<br/>");
            //}

            return sb.ToString();
        }

        private static StringBuilder PrintTableOfCases(object jsonO, Func<string, bool> filter)
        {
            dynamic json = jsonO;
            var sb = new StringBuilder();
            var helper = new HTMLTableHelper(4);
            sb.Append(helper.printStart());
            sb.Append(helper.printRow(new string[] { "ID", "Title", "Status", "Last Change" }, true));

            for (int i = 0; i < json["MyRequest"].Length; i++)
            {
                if (filter(json["MyRequest"][i]["Status"]["Name"]))
                {
                    string id = json["MyRequest"][i]["Id"];
                    string link;
                    if (id.StartsWith("IR"))
                    {
                        link = "https://service.aau.dk/Incident/Edit/" + id;

                        //Filter away if its closed as converted to SR
                        if ((idForConvertedToSR.Equals(json["MyRequest"][i]["ResolutionCategory"]["Id"])))
                        {
                            continue;
                        }
                    }
                    else //if (id.StartsWith("SR"))
                    {
                        link = "https://service.aau.dk/ServiceRequest/Edit/" + id;
                    }
                    string sID = "<a href=\"" + link + "\" target=\"_blank\">" + json["MyRequest"][i]["Id"] + "</a><br/>";
                    string sTitle = json["MyRequest"][i]["Title"];
                    string sStatus = json["MyRequest"][i]["Status"]["Name"];
                    string tmp = json["MyRequest"][i]["LastModified"];
                    string sLastChange = Convert.ToDateTime(tmp).ToString("yyyy-MM-dd HH:mm");

                    sb.Append(helper.printRow(new String[] { sID, sTitle, sStatus, sLastChange }));
                }
            }

            sb.Append(helper.printEnd());
            return sb;
        }


        //returns json string for uuid
        protected async Task<string> lookupUserByUUID(string uuid, string authkey)
        {

            //WebRequest request = WebRequest.Create(webserviceURL+"api/V3/User/GetUserRelatedInfoByUserId/?userid=352b43f6-9ff4-a36f-0342-6ce1ae283e37");
            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/User/GetUserRelatedInfoByUserId/?userid=" + uuid);
            request.Method = "Get";
            //request.ContentType = "text/json";
            request.Headers.Add("Authorization", "Token " + authkey);
            request.ContentType = "application/json; text/json";

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            //string sMatchUPN = ",\\\"UPN\\\":\\\"(.)*\\\",";

            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = Int32.MaxValue;
            dynamic jsonString = js.Deserialize<dynamic>(responseText);

            //dynamic json = js.Deserialize<dynamic>((string)jsonString);
            return jsonString;

        }


        protected async Task<string> getUserUUIDByUPN(string upn, string authkey)
        {
            var userName = upn.Split('@')[0];
            return await getUserUUIDByUPN(upn, userName, authkey);
        }

        //Takes a upn and retuns the users uuid
        protected async Task<string> getUserUUIDByUPN(string upn, string displayName, string authkey)
        {
            //Get username from UPN


            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/User/GetUserList?userFilter=" + upn);
            request.Method = "Get";
            request.ContentType = "text/json";
            request.ContentType = "application/json; charset=utf-8";


            request.Headers.Add("Authorization", "Token " + authkey);

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic json = js.Deserialize<dynamic>(responseText);

            StringBuilder sb = new StringBuilder();
            string userjson = null;

            //TODO: Don't await for each item, make all requests and await, then look over data
            foreach (dynamic obj in json)
            {
                //sb.Append((string)obj["Id"]);
                userjson = await lookupUserByUUID((string)obj["Id"], authkey);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.MaxJsonLength = Int32.MaxValue;
                dynamic jsonString = jss.Deserialize<dynamic>(userjson);
                userID = (string)obj["Id"];

                if (upn.Equals((string)jsonString["UPN"], StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }
            ;
            return userjson;

        }


        /*protected async Task<string> getIncidentTest(string IRid, string authkey)
        {

            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/Projection/GetProjection?id=" + IRid + " +&typeProjectionId=2d460edd-d5db-bc8c-5be7-45b050cba652");
            request.Method = "Get";
            request.ContentType = "text/json";
            request.ContentType = "application/json; charset=utf-8";


            request.Headers.Add("Authorization", "Token " + authkey);

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic json = js.Deserialize<dynamic>(responseText);


            return responseText;

        }*/
        /*
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        */
        /*protected async void LoadTestData(object sender, EventArgs e)
        {
            string authkey = await getAuthKey();
            //string uuid = getUserUUIDByUPN("kyrke@its.aau.dk", authkey);
            //string s = doAction(uuid);
            //string s = await lookupUserByUUID("008f492b-df58-6e9c-47c5-bd4ae81028af", authkey);
            string s = await getIncidentTest("IR408927", authkey);

            //responseLbl.Text = s;

        }*/

        public async Task<string> getActiveIncidents(string upn, string displayName)
        {
            string authkey = await getAuthKey();

            if (authkey == null)
            {
                return "No creds for SCSM";
            }

            string uuid = await getUserUUIDByUPN(upn, displayName, authkey);
            return doAction(uuid);
        }
    }
}