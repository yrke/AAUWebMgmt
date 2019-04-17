using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using System.DirectoryServices;

namespace ITSWebMgmt.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public void Redirector(string adpath)
        {
            if (adpath != null)
            {
                if (!adpath.StartsWith("LDAP://"))
                {
                    adpath = "LDAP://" + adpath;
                }
                DirectoryEntry de = new DirectoryEntry(adpath);

                var type = de.SchemaEntry.Name;

                if (type.Equals("user"))
                {


                    string param = "?" + "username=" + de.Properties["userPrincipalName"].Value.ToString();
                    Response.Redirect("~/User" + param);

                }
                else if (type.Equals("computer"))
                {
                    var ldapSplit = adpath.Replace("LDAP://", "").Split(',');
                    var name = ldapSplit[0].Replace("CN=", "");
                    var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                    string param = "?" + "computername=" + domain + "\\" + name;
                    Response.Redirect("~/Computer" + param);
                }
                else if (type.Equals("group"))
                {
                    string param = "?" + "grouppath=" + adpath;
                    Response.Redirect("~/Group" + param);
                }
            }
        }
    }
}
