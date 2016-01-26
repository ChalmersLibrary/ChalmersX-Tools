using ChalmersxTools.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public class TemperatureMeasurementToolViewModel : LtiViewModelBase
    {
        public TemperatureMeasurementSubmission Submission { get; set; }
        public List<MeasurementAndCoordinate> Measurements { get; set; }
    }
}