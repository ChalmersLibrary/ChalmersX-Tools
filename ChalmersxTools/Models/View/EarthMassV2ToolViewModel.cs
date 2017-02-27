using ChalmersxTools.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public class EarthMassV2ToolViewModel : LtiViewModelBase
    {
        public EarthMassV2Submission Submission { get; set; }
        public double EarthMassAverage { get; set; }
        public List<double> Measurements { get; set; }
    }
}