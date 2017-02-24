using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChalmersxTools.Config
{
    public interface IConfig
    {
        string LtiConsumerSecret { get; }
        string OpenWeatherMapApiKey { get; }
        string GoogleMapsApiKey { get; }
    }
}
