using ChalmersxTools.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.View
{
    public class EarthSpheresImageGalleryViewModel : LtiViewModelBase
    {
        public EarthSpheresImageGalleryViewModel()
        {
            GeosphereImages = new List<GeoImage>();
            AtmosphereImages = new List<GeoImage>();
            BiosphereImages = new List<GeoImage>();
            HydrosphereImages = new List<GeoImage>();
            CryosphereImages = new List<GeoImage>();
        }

        public List<GeoImage> GeosphereImages { get; set; }
        public List<GeoImage> AtmosphereImages { get; set; }
        public List<GeoImage> BiosphereImages { get; set; }
        public List<GeoImage> HydrosphereImages { get; set; }
        public List<GeoImage> CryosphereImages { get; set; }
    }
}