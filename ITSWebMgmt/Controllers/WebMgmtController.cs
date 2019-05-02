using ITSWebMgmt.Caches;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ITSWebMgmt.Controllers
{
    public abstract class WebMgmtController : Controller
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
