using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChalmersxTools.Config
{
    public class DefaultConfig : IConfig
    {
        public string LtiConsumerSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["ltiConsumerSecret"];
            }
        }

        public string OpenWeatherMapApiKey
        {
            get
            {
                return ConfigurationManager.AppSettings["openWeatherMapApiKey"];
            }
        }
    }
}
