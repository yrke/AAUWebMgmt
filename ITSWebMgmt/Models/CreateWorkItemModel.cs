using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class CreateWorkItemModel
    {
        public string AffectedUser { get; set; }
        public string Title { get; set; }
        public string Desription { get; set; }
    }
}
