using ITSWebMgmt.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace ITSWebMgmt.Controllers
{
    public class ManagedByChanger
    {
        public ADcache cache;
        public string ErrorMessage;

        public ManagedByChanger(ADcache ADcache)
        {
            cache = ADcache;
        }

        public void SaveEditManagedBy(string email)
        {
            try
            {
                UserController uc = new UserController();
                uc.adpath = uc.globalSearch(email);
                if (uc.DistinguishedName.Contains("CN="))
                {
                    cache.saveProperty("managedBy", uc.DistinguishedName);
                    ErrorMessage = "";
                }
                else
                {
                    ErrorMessage = "Error in email";
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }
    }
}