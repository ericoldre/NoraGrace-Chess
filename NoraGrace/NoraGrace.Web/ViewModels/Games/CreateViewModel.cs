using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoraGrace.Web.ViewModels.Games
{
    public class CreateViewModel
    {
        
        [System.ComponentModel.DataAnnotations.Required]
        public string White { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Black { get; set; }

        
    }
}