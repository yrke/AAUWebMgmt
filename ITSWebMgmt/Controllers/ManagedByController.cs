using ITSWebMgmt.Caches;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace ITSWebMgmt.Controllers
{
    public class ManagedByController : Controller
    {
        public string ErrorMessage;

        [HttpPost]
        public ActionResult SaveEditManagedBy([FromBody]string email)
        {
            SaveManagedBy(email);

            if (ErrorMessage == "")
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                return Json(new { success = true });
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, errorMessage = ErrorMessage });
            }
        }

        public void SaveManagedBy(string email)
        {
            try
            {
                UserController uc = new UserController(null);//TODO Test if it still works after update on usercontroller
                uc.UserModel.adpath = uc.globalSearch(email);
                if (uc.UserModel.DistinguishedName.Contains("CN="))
                {
                    new GroupADcache(uc.UserModel.adpath).saveProperty("managedBy", uc.UserModel.DistinguishedName);
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