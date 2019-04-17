using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ITSWebMgmt.Caches;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class DefendpointChallengeResponseController : WebMgmtController
    {
        public IActionResult Index(string challanageInput, string reasonInput)
        {
            string result = "";
            if (challanageInput != null)
            {
                Process si = new Process();
                si.StartInfo.FileName = "C:\\webmgmtlog\\PGChallengeResponse.exe";
                si.StartInfo.UseShellExecute = false;

                string key = ConfigurationManager.AppSettings["other:defendpoint:crkey"];

                string challange = challanageInput.Replace("\"", "");

                //XXX: Allow spaces sperators and rember X version for persistent response
                Regex regex = new Regex("[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]");

                if (regex.IsMatch(challange))
                {
                    string argument = string.Format(@"""{0}"" ""{1}"" ""{2}""", challange, "once".Replace("\"", ""), key.Replace("\"", ""));

                    si.StartInfo.Arguments = argument;
                    si.StartInfo.RedirectStandardOutput = true;
                    si.Start();
                    string output = si.StandardOutput.ReadToEnd();
                    si.Close();

                    //Check if valid response
                    if (output.Contains("Generated Response:"))
                    {
                        logger.Info("User {0} generated challange with reason {1}", ControllerContext.HttpContext.User.Identity.Name, reasonInput);
                        result = "<br/>Response Code: " + output.Replace("Generated Response: ", "");

                    }
                    else
                    {
                        result = "<br/>Error generating code - Error in command line values";
                    }
                }
                else
                {
                    result = "<br/>Error generating code - Invalid challange code";
                }
            }

            return View("Index", new DefendpointChallengeResponse(result));
        }
    }
}