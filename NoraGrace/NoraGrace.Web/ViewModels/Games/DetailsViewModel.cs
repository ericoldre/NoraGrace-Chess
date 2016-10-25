using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoraGrace.Web.ViewModels.Games
{
    public class DetailsViewModel
    {
        public NoraGrace.Web.Model.GameInfo GameInfo { get; set; }
        public string test { get; set; }

        public string White { get; set; }
        public string Black { get; set; }
    }
}