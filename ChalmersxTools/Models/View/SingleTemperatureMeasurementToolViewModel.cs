using ChalmersxTools.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public class SingleTemperatureMeasurementToolViewModel : LtiViewModelBase
    {
        public SingleTemperatureMeasurementSubmission Submission { get; set; }
        public List<MeasurementWithTimeAndPlace> Measurements { get; set; }
    }
}