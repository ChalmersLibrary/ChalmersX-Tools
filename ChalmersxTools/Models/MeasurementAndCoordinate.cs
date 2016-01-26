using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class MeasurementAndCoordinate
    {
        public MeasurementAndCoordinate() {
            Measurement1 = new double();
            Measurement2 = new double();
            Coordinate = new Coordinate();
        }
        public double? Measurement1 { get; set; }
        public double? Measurement2 { get; set; }

        public Coordinate Coordinate { get; set; }
    }
}