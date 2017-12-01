using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class DefendpointChallengeResponse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (IsPostBack)
            {

                Process si = new Process();
                si.StartInfo.FileName = "C:\\webmgmtlog\\PGChallengeResponse.exe";
                si.StartInfo.UseShellExecute = false;

                string key = ConfigurationManager.AppSettings["other:defendpoint:crkey"];

                string argument = string.Format(@"""{0}"" ""{1}"" ""{2}""", challanageInput.Value, "once", "key");

                si.StartInfo.Arguments = argument;
                si.StartInfo.RedirectStandardOutput = true;
                si.Start();
                string output = si.StandardOutput.ReadToEnd();
                si.Close();

                //Check if valid response
                if (output.Contains("Generated Response:"))
                {

                    resultLbl2.Text = "<br/>Response Code: "+ output.Replace("Generated Response: ", "");

                }
                else
                {
                    resultLbl2.Text = "<br/>Error generating code";
                }











                


                
            }

        }


        protected void generateResponse_OnClick(object sender, EventArgs e)
        {
            resultLbl2.Text = "fisk";
        }
    }
}