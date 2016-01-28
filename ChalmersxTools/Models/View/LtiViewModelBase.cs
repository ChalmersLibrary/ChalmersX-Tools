using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public abstract class LtiViewModelBase
    {
        public string LtiSessionId { get; set; }
        public string Roles { get; set; }
        public string ResponseMessage { get; set; }
        public int NumberOfSubmissions { get; set; }
    }
}