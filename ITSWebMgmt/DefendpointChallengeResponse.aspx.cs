using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ITSWebMgmt
{
    public partial class DefendpointChallengeResponse : System.Web.UI.Page
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (IsPostBack)
            {
                

                Process si = new Process();
                si.StartInfo.FileName = "C:\\webmgmtlog\\PGChallengeResponse.exe";
                si.StartInfo.UseShellExecute = false;

                string key = ConfigurationManager.AppSettings["other:defendpoint:crkey"];

                string challange = challanageInput.Value.Replace("\"", "");

                //XXX: Allow spaces sperators and rember X version for persistent response
                Regex regex = new Regex("[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]");

                if (regex.IsMatch(challange)) {
                    string argument = string.Format(@"""{0}"" ""{1}"" ""{2}""", challange, "once".Replace("\"", ""), key.Replace("\"", ""));

                    si.StartInfo.Arguments = argument;
                    si.StartInfo.RedirectStandardOutput = true;
                    si.Start();
                    string output = si.StandardOutput.ReadToEnd();
                    si.Close();

                    //Check if valid response
                    if (output.Contains("Generated Response:"))
                    {
                        logger.Info("User {0} generated challange with reason {1}", System.Web.HttpContext.Current.User.Identity.Name, reasonInput.Value);
                        resultLbl2.Text = "<br/>Response Code: " + output.Replace("Generated Response: ", "");

                    }
                    else
                    {
                        resultLbl2.Text = "<br/>Error generating code - Error in command line values";
                    }

                }
                else
                {
                    resultLbl2.Text = "<br/>Error generating code - Invalid challange code";
                }













            }

        }


        protected void generateResponse_OnClick(object sender, EventArgs e)
        {
            resultLbl2.Text = "fisk";
        }
    }
}