using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class SCSMTest : System.Web.UI.Page
    {
        string webserviceURL = "https://service.aau.dk";
        //string webserviceURL = "http://scsm-tms1.srv.aau.dk";

        public string userID = "";

        protected string getAuthKey()
        {

            WebRequest request = WebRequest.Create(webserviceURL+"/api/V3/Authorization/GetToken");
            request.Method = "POST";
            request.ContentType = "text/json";


            string secret = File.ReadAllText(@"C:\webmgmtlog\webmgmtsecret.txt");
            //string json = "{\"Username\": \"srv\\\\svc_webmgmt-scsm\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";
            //string json = "{\"Username\": \"its\\\\kyrke\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";
            string json = "{\"Username\": \"its\\\\svc_webmgmt-scsm3\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";
            //string json = "{\"Username\": \"its\\\\svc_webmgmt-scsm\",\"Domain\": \"its\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";


            var requestSteam = new StreamWriter(request.GetRequestStream());
            requestSteam.Write(json);
            requestSteam.Flush();
            requestSteam.Close();
            
            var response = request.GetResponse();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();
           
            //responseLbl.Text = responseText;
            return responseText.Replace("\"","");
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

            
            sb.Append("<h1>Open Requests</h1><br />");

            for (int i = 0; i < json["MyRequest"].Length; i++)
            {
                if (!"Closed".Equals(json["MyRequest"][i]["Status"]["Name"])) {
                    string id = json["MyRequest"][i]["Id"];
                    string link;
                    if (id.StartsWith("IR"))
                    {
                        link = "https://service.aau.dk/Incident/Edit/" + id;
                    }
                    else //if (id.StartsWith("SR"))
                    {
                        link = "https://service.aau.dk/ServiceRequest/Edit/" + id;
                    }

                    sb.Append("<a href=\"" + link + "\" target=\"_blank\">" + json["MyRequest"][i]["Id"] + " - " + json["MyRequest"][i]["Title"] + " - " + json["MyRequest"][i]["Status"]["Name"] + "</a><br/>");
                }
            }

            sb.Append("<br /><br /><h3>Closed Requests</h3>");
            for (int i = 0; i < json["MyRequest"].Length; i++)
            {
                if ("Closed".Equals(json["MyRequest"][i]["Status"]["Name"]))
                {
                    string id = json["MyRequest"][i]["Id"];
                    string link;
                    if (id.StartsWith("IR"))
                    {
                        link = "https://service.aau.dk/Incident/Edit/" + id;
                    }
                    else //if (id.StartsWith("SR"))
                    {
                        link = "https://service.aau.dk/ServiceRequest/Edit/" + id;
                    }

                    sb.Append("<a href=\"" + link + "\" target=\"_blank\">" + json["MyRequest"][i]["Id"] + " - " + json["MyRequest"][i]["Title"] + " - " + json["MyRequest"][i]["Status"]["Name"] + "</a><br/>");
                }
            }
            
            //sb.Append("<br /><br/>IsAssignedToUser<br />");
            //foreach (dynamic s in json["IsAssignedToUser"])
            //for (int i = 0; i < json["IsAssignedToUser"].Length; i++)
            //{
            //    sb.Append(json["IsAssignedToUser"][i]["Id"] + " -" + json["IsAssignedToUser"][i]["DisplayName"] + " - " + json["IsAssignedToUser"][i]["Status"]["Name"] + "<br/>");
            //}

            return sb.ToString();
        }


        //returns json string for uuid
        protected string lookupUserByUUID(string uuid, string authkey) {

            //WebRequest request = WebRequest.Create(webserviceURL+"api/V3/User/GetUserRelatedInfoByUserId/?userid=352b43f6-9ff4-a36f-0342-6ce1ae283e37");
            WebRequest request = WebRequest.Create(webserviceURL+"/api/V3/User/GetUserRelatedInfoByUserId/?userid="+uuid);
            request.Method = "Get";
            //request.ContentType = "text/json";
            request.Headers.Add("Authorization", "Token " + authkey);
            request.ContentType = "application/json; text/json";

            var response = request.GetResponse();
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

        
        protected string getUserUUIDByUPN(string upn, string authkey) {
            var userName = upn.Split('@')[0];
            return getUserUUIDByUPN(upn, userName, authkey);
        }

        //Takes a upn and retuns the users uuid
        protected string getUserUUIDByUPN(string upn, string displayName, string authkey)
        {
            //Get username from UPN
            

            WebRequest request = WebRequest.Create(webserviceURL+"/api/V3/User/GetUserList?userFilter="+upn);
            request.Method = "Get";
            request.ContentType = "text/json";
            request.ContentType = "application/json; charset=utf-8";


            request.Headers.Add("Authorization", "Token " + authkey);

            var response = request.GetResponse();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic json = js.Deserialize<dynamic>(responseText);

            StringBuilder sb = new StringBuilder();
            string userjson = null;
            foreach (dynamic obj in json)
            {
                //sb.Append((string)obj["Id"]);
                userjson = lookupUserByUUID((string)obj["Id"], authkey);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.MaxJsonLength = Int32.MaxValue;
                dynamic jsonString = jss.Deserialize<dynamic>(userjson);
                userID = (string)obj["Id"];

                if (upn.Equals((string)jsonString["UPN"], StringComparison.CurrentCultureIgnoreCase)) {
                    break;
                }
            }
            ;
            return userjson;

        }

        protected void Page_Load(object sender, EventArgs e)
        {

            
            

        }

        protected void button_Click(object sender, EventArgs e)
        {
            string authkey = getAuthKey();
            //string uuid = getUserUUIDByUPN("kyrke@its.aau.dk", authkey);
            //string s = doAction(uuid);
            string s = lookupUserByUUID("008f492b-df58-6e9c-47c5-bd4ae81028af", authkey);
            
            responseLbl.Text = s;

        }

        public string getActiveIncidents(string upn, string displayName){
            string authkey = getAuthKey();
            string uuid = getUserUUIDByUPN(upn, displayName, authkey);
            return doAction(uuid);
        }
    }
}