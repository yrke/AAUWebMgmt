using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace ITSWebMgmt.Connectors
{
    public class PDSConnector
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            //var f = new PDStest("115928");

            //lblresult.Text = f.department;
        }


        string department = "";
        string streetAddress = "";
        string extendedAddress = "";
        string postalCode = "";
        string locality = "";
        string countryName = "";


        public string Department
        {
            get { return department; }
        }

        public string OfficeAddress
        {
            get { return streetAddress + " (" + extendedAddress.Trim() + ")"; }
        }
        public PDSConnector()
        {

        }
        public PDSConnector(string empID)
        {
            return; //The webside is down and the service is will be closed after april 2019. Do not try to connect to it
            //string empID = "115928";
            string url = "http://personprofil.aau.dk/" + empID + "?lang=en";

            WebRequest request = WebRequest.Create(url);

            var response = request.GetResponse();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            if (responseText.Contains("Profile not found."))
            {
                //"Not Found";
                extendedAddress = "Not found in PDS";
                department = "Not found in PDS";
                return;
            }
            else
            {

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseText);

                var adr = doc.DocumentNode.SelectSingleNode("//*[@id=\"visitkort\"]/div/div/address");


                try
                {
                    department = adr.SelectSingleNode("span[1]").InnerText;
                    streetAddress = adr.SelectSingleNode("span[2]").InnerText;
                    extendedAddress = adr.SelectSingleNode("span[3]").InnerText;
                    postalCode = adr.SelectSingleNode("span[4]").InnerText;
                    locality = adr.SelectSingleNode("span[5]").InnerText;
                    countryName = adr.SelectSingleNode("span[6]").InnerText;
                }
                catch (NullReferenceException)
                {
                    //Do nothing, just value missing, default value empty
                }


            }

        }



    }
}