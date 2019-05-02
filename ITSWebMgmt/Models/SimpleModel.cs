using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class SimpleModel
    {
        [Required]
        public string Text { get; set; }
    }
}
