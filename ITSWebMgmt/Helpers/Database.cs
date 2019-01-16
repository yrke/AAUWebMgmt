using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Helpers
{
    public class Database
    {
        public static ManagementScope ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");

        public static ManagementObjectCollection getResults(WqlObjectQuery wqlq)
        {
            var searcher = new ManagementObjectSearcher(ms, wqlq);
            return searcher.Get();
        }

        public static bool HasValues(ManagementObjectCollection results)
        {
            try
            {
                var t2 = results.Count;
                return results.Count != 0;
            }
            catch (ManagementException e) { }

            return false;
        }
    }
}