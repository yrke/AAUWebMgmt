using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ITSWebMgmt.Functions
{
    public class Loginscript
    {

        public Loginscript()
        {

        }

        //Return the loginscript of the user or null for none
        public string getLoginScript(DirectoryEntry user)
        {



            if (user.Properties.Contains("scriptPath"))
            {
                string scriptName = user.Properties["scriptPath"].Value.ToString();

                //Get DCpath
                //\\adm.aau.dk\SYSVOL\adm.aau.dk\scripts

                var split = user.Path.Split(',');
                var dc = split.Where<string>(s => s.StartsWith("DC=")).ToArray<string>();
                var domain = String.Join<string>(".", dc).Replace("DC=", "");
             
                var scriptpath = String.Format("\\\\{0}\\SYSVOL\\{0}\\scripts\\{1}", domain, scriptName);

                string scriptText = System.IO.File.ReadAllText(scriptpath);


                return scriptText;
               
            }  // Else no login script return null

            return null;
        }

        //Returns a html table displaying loginscript data and raw data parition
        public string parseAndDisplayLoginScript(string loginscript)
        {

            var sb = new StringBuilder();

            var lines = loginscript.Split('\n');

            //Remote comment 
            var linesNetUse = lines.Where<string>(s => s.StartsWith("net use", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();
            var linesNetUseNotDelete = linesNetUse.Where<string>(s => !s.ToLower().Contains(" /d")).ToArray<string>();

            var table = new HTMLTableHelper(2);

            sb.Append("<h3>Networkdrives</h3>");

            sb.Append(table.printStart());
            sb.Append(table.printRow(new string[] { "Drive Letter", "share" }, true));

            foreach (string line in linesNetUseNotDelete)
            {
                //net use S: \\adm.aau.dk\Fileshares\DRIFTSDB

                Regex pattern = new Regex(@"net use (?<drive>.): (?<share>.*)");
                Match match = pattern.Match(line.ToLower());

                string drive = match.Groups["drive"].Value;
                string share = match.Groups["share"].Value;


                sb.Append(table.printRow(new string[] { drive, share }, true));

                
            }

            sb.Append(table.printEnd());


            sb.Append("<h3>Raw<h3>");
            foreach (string line in lines)
            {

                sb.Append(String.Format("<code>{0}</code><br/>", line.Replace("\r", "")));

            }
            
            


            return sb.ToString();

        }





    }
}