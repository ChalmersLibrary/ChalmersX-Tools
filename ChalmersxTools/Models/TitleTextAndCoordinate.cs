using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class TitleTextAndCoordinate
    {
        public TitleTextAndCoordinate()
        {
            Coordinate = new Coordinate();
        }

        public string Title { get; set; }
        public string Text { get; set; }
        public Coordinate Coordinate { get; set; }
    }
}