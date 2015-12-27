using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class PDStest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        string department;
        string streetAddress;
        string extendedAddress;
        string postalCode;
        string locality;
        string countryName;


        public string Department
        {
            get { return department; }
        }

        public string OfficeAddress
        {
            get { return streetAddress + " (" + extendedAddress + ")"; }
        } 

        PDStest(string empID)
        {
            string toReturn = "";

            //string empID = "115928";
            string url = "http://personprofil.aau.dk/" + empID + "?lang=en";

            WebRequest request = WebRequest.Create(url);

            var response = request.GetResponse();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = streamReader.ReadToEnd();

            if (responseText.Contains("Profile not found."))
            {
                toReturn = "Not Found";
            }
            else
            {

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseText);

                var adr = doc.DocumentNode.SelectSingleNode("//*[@id=\"visitkort\"]/div/div/address");



                department = adr.SelectSingleNode("span[1]").InnerText;
                streetAddress = adr.SelectSingleNode("span[2]").InnerText;
                extendedAddress = adr.SelectSingleNode("span[3]").InnerText;
                postalCode = adr.SelectSingleNode("span[4]").InnerText;
                locality = adr.SelectSingleNode("span[5]").InnerText;
                countryName = adr.SelectSingleNode("span[6]").InnerText;


            }

        }



    }
}