using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class ViewIdentifierAndModel
    {
        public string ViewIdentifier { get; set; }
        public object Model { get; set; }

        public ViewIdentifierAndModel(string viewIdentifier, object model)
        {
            ViewIdentifier = viewIdentifier;
            Model = model;
        }
    }
}