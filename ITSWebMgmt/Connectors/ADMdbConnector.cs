using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.Connectors
{
    public class ADMdbConnector
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        Dictionary<string, string> admdburl = new Dictionary<string, string>() {
            {"its","https://tools.ist.aau.dk/its/i88/adm_db/adm.pl"},
            {"adm","https://tools.ist.aau.dk/adm/i93/adm_db/adm.pl"},
            {"aub","https://tools.ist.aau.dk/aub/i96/adm_db/adm.pl"},
            {"student","https://tools.ist.aau.dk/student/i99/adm_db/adm.pl"},
            {"staff","https://tools.ist.aau.dk/staff/i95/adm_db/adm.pl"},
            {"id","https://tools.ist.aau.dk/id/i92/adm_db/adm.pl"},
            {"sbi","https://tools.ist.aau.dk/sbi/i91/adm_db/adm.pl" },
            {"dcm","https://tools.ist.aau.dk/dcm/i89/adm_db/adm.pl" },
            {"admt","https://tools.ist.aau.dk/admt/i87/adm_db/adm.pl" },
            {"ist","https://tools.ist.aau.dk/ist/i98/develadm_db/adm.pl" },
            {"aau-it","https://tools.ist.aau.dk/aau-it/i90/adm_db/adm.pl"},
            {"civil","https://tools.ist.aau.dk/civil/i6/adm_db/adm.pl" },
            {"create","https://tools.ist.aau.dk/create/i7/adm_db/adm.pl" },
            {"es","https://tools.ist.aau.dk/es/i8/adm_db/adm.pl" },
            {"learning","https://tools.ist.aau.dk/learning/i10/adm_db/adm.pl" },
            {"nano","https://tools.ist.aau.dk/nano/i13/adm_db/adm.pl" },
            {"et","https://tools.ist.aau.dk/et/i14/adm_db/adm.pl" },
            {"m-tech","https://tools.ist.aau.dk/m-tech/i94/adm_db/adm.pl"},
            {"cs","https://tools.ist.aau.dk/cs/i16/adm_db/adm.pl" },
            {"math","https://tools.ist.aau.dk/math/i17/adm_db/adm.pl"} ,
            {"plan","https://tools.ist.aau.dk/plan/i20/adm_db/adm.pl" },
            {"bio","https://tools.ist.aau.dk/bio/i18/adm_db/adm.pl" },
            {"hst","https://tools.ist.aau.dk/hst/i21/adm_db/adm.pl" }


        };
        Dictionary<string, string> month = new Dictionary<string, string>(){
            {"jan","01"},
            {"feb","02"},
            {"mar","03"},
            {"apr","04"},
            {"maj","05"},
            {"may","05"}, //Why is admdb using danish month here, we keep the enligsh name for concistency
            {"jun","06"},
            {"jul","07"},
            {"aug","08"},
            {"sep","09"},
            {"oct","10"},
            {"nov","11"},
            {"dec","12"}
        };

        public async Task<string> loadUserExpiredate(string domain, string username, string firstName, string lastName)
        {
            var url = admdburl[domain.ToLower()]; //Throws exception on wrong domain

            //Get password for ADMdb
            string wsusername = ConfigurationManager.AppSettings["cred:admdb:username"];
            string wspassword = ConfigurationManager.AppSettings["cred:admdb:password"];

            if (wsusername == null || wspassword == null)
            {
                return "missing username or password to admdb";
            }

            WebRequest request = WebRequest.Create(url);

            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(wsusername + ":" + wspassword));
            request.Headers.Add("Authorization", "Basic " + encoded);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var outgoingQueryString = HttpUtility.ParseQueryString(String.Empty, System.Text.Encoding.GetEncoding("Windows-1252"));
            //outgoingQueryString.Add("field1", "value1");
            outgoingQueryString.Add("osnames.username", username);
            outgoingQueryString.Add("persons.firstname", firstName);
            outgoingQueryString.Add("persons.lastname", lastName);
            outgoingQueryString.Add("Submit", "Search");
            outgoingQueryString.Add("page", "SearchStatus");

            string postdata = outgoingQueryString.ToString();
            postdata = ITSWebMgmt.Helpers.HTMLEncodingHelper.convertUTF8encodingToWindows1252(postdata);

            var requestSteam = new StreamWriter(request.GetRequestStream());
            requestSteam.Write(postdata);
            requestSteam.Flush();
            requestSteam.Close();

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            if (responseText.Contains("Found no entries that matches the specified search parameters"))
            {
                return "Ingen aktiv ramme";

            }
            else
            {

                try
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(responseText);

                    var selectElements = doc.DocumentNode.Descendants("select");

                    var selectEmployeesEndDay = selectElements.Where(node => node.GetAttributeValue("name", "").EndsWith(".end_day")).First();
                    var selectEmployeesEndMonth = selectElements.Where(node => node.GetAttributeValue("name", "").EndsWith(".end_month")).First();
                    var selectEmployeesEndYear = selectElements.Where(node => node.GetAttributeValue("name", "").EndsWith(".end_year")).First();


                    string endday = selectEmployeesEndDay.Element("option").GetAttributeValue("value", "");
                    string endMonth = selectEmployeesEndMonth.Element("option").GetAttributeValue("value", "");
                    string endYear = selectEmployeesEndYear.Element("option").GetAttributeValue("value", "");

                    return "" + endYear + "-" + month[endMonth.ToLower()] + "-" + endday;
                }
                catch (Exception)
                {
                    logger.Debug("User not found on key username: " + domain + "\\" + username);
                    return "UserInfo Error on Lookup";
                }

            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            /*var response = loadUserExpiredate("its", "kyrke", "Kenneth Yrke", "Jørgensen");
            if (response != null) {
                labelResult.Text = response;
            }
            else
            {
                labelResult.Text = "user expired";
            }*/

        }
    }
}