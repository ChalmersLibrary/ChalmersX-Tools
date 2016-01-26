using ChalmersxTools.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public class EarthMassToolViewModel : LtiViewModelBase
    {
        public EarthMassSubmission Submission { get; set; }
        public double EarthMassAverage { get; set; }
        public int NumberOfSubmissions { get; set; }
    }
}