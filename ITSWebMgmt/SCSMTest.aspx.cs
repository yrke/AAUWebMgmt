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

        protected string getAuthKey()
        {

            WebRequest request = WebRequest.Create("https://service.aau.dk/api/V3/Authorization/GetToken");
            request.Method = "POST";
            request.ContentType = "text/json";

            string json = "{\"Username\": \"its\\\\kyrke\",\"Password\": \"" + passwordTextBx.Text + "\",\"LanguageCode\": \"ENU\"}";

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

        protected void doAction(string authkey)
        {
            //string authkey = "YIP7Av2/OPNVVvKudsS0WMNcQaOa9GJowu9pUdTuhiQVwy7zHJ/9AoK+A21mU0yzpHdAq1gbxARoajONDuETf3CTW77QAMh5xGJ14RXAXWvCHTZDhIrf0NF5+CGhYI+ZQAxvytq+MxoHqVxHnPojvQ8yxvCpGDCHpJq8jz31ktLYW8ldhN1uLLVT974J2Uc9Lmh4bW5axWGf4laCvMX9gqat3ikRocuYIbNXY4j0kGCxGOmxHdXy71eU7HjFueUnuC0BugfvCRhCqx9d0FQGWIUAii3KOWOO7MSZmG+2dEIq5q0J6zOUb6fdxf4k91aytyI0VcPHBwgeU7AHkR65NMlerTGQN/I4U0vW/ANEZS8/l82hFQRJMAdDkQgZ0tMpybO45fmN0oESW8dHGC7638PJlgD5bohljf4f7llZGQzP7j1NaKNEysf2ZO7zl6CkYFzbDeuYyi4575WpPQj0FwfCvRe2I9tiw+oHJVj+p2WOV0EoBU1HdnFLrJ89/nPXBTrWiCLTs7ppzO7f5HZMjgxzEEQXfBVuYq5wDyhNUGg=";
            WebRequest request = WebRequest.Create("https://service.aau.dk/api/V3/User/GetUserList?userFilter=kyrke");
            request.Method = "Get";
            request.ContentType = "text/json";
            request.ContentType = "application/json; charset=utf-8";

            
            request.Headers.Add("Authorization", "Token "+authkey);

            var response = request.GetResponse();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();
            
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic json = js.Deserialize<dynamic>(responseText);

            StringBuilder sb = new StringBuilder();
            foreach (dynamic obj in json)
            {
                //sb.Append((string)obj["Id"]);
                sb.Append(lookupUserByUUID((string)obj["Id"], authkey));
            }


            responseLbl.Text = sb.ToString();

        }

        protected string lookupUserByUUID(string uuid,string authkey) {
        
            //WebRequest request = WebRequest.Create("https://service.aau.dk/api/V3/User/GetUserRelatedInfoByUserId/?userid=352b43f6-9ff4-a36f-0342-6ce1ae283e37");
            WebRequest request = WebRequest.Create("https://service.aau.dk/api/V3/User/GetUserRelatedInfoByUserId/?userid="+uuid);
            request.Method = "Get";
            request.ContentType = "text/json";
            request.Headers.Add("Authorization", "Token " + authkey);
            request.ContentType = "application/json; text/json";

            var response = request.GetResponse();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            //string sMatchUPN = ",\\\"UPN\\\":\\\"(.)*\\\",";

            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic jsonString = js.Deserialize<dynamic>(responseText);

            dynamic json = js.Deserialize<dynamic>((string)jsonString);
            return json["UPN"];
            
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            
            

        }

        protected void button_Click(object sender, EventArgs e)
        {
            string authkey = getAuthKey();
            doAction(authkey);
        }
    }
}