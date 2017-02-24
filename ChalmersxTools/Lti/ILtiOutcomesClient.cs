using LtiLibrary.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChalmersxTools.Lti
{
    public interface ILtiOutcomesClient
    {
        BasicResult PostScore(string serviceUrl, string consumerKey, string consumerSecret, string lisResultSourcedId, double? score);
    }
}
