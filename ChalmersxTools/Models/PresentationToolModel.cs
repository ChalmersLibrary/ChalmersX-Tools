using LtiLibrary.Core.Lti1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class PresentationToolModel
    {
        public StudentPresentation CurrentStudentPresentation { get; set; }
        public List<TitleTextAndCoordinate> Presentations { get; set; }
        public string Roles { get; set; }
        public string LtiSessionId { get; set; }
        public string LtiRequest { get; set; }
    }
}