using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class MeasurementWithTimeAndPlace
    {
        public MeasurementWithTimeAndPlace()
        {
            Measurement = new double();
            Coordinate = new Coordinate();
            Time = new DateTime(1970, 1, 1);
        }

        public double? Measurement { get; set; }
        public Coordinate Coordinate { get; set; }
        public DateTime Time { get; set; }
    }
}