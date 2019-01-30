using ITSWebMgmt.Caches;
using ITSWebMgmt.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.WebMgmtErrors
{
    enum Severity { Info, Warning, Error };

    public abstract class WebMgmtError<T> where T : ADcache
    {
        public string Heading;
        public string Description;
        public abstract bool HaveError(Controller<T> controller);
        public int Severity;
    }
}