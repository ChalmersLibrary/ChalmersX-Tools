using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class Coordinate
    {
        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Coordinate() : this(0, 0) { }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override bool Equals(object obj)
        {
            var c = obj as Coordinate;
            return c.Latitude == Latitude && c.Longitude == Longitude;
        }

        public override int GetHashCode()
        {
            // Time for magic!
            int hash = 17;
            hash = hash * 31 + Latitude.GetHashCode();
            hash = hash * 31 + Longitude.GetHashCode();
            return hash;
        }
    }
}