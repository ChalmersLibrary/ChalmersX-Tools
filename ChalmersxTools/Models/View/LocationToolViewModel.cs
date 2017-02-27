using ChalmersxTools.Models.Database;
using LtiLibrary.Core.Lti1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public class LocationToolViewModel : LtiViewModelBase
    {
        public LocationSubmission CurrentLocationSubmission { get; set; }
        public List<TitleTextAndCoordinate> Locations { get; set; }
        public string LtiRequest { get; set; }
    }
}