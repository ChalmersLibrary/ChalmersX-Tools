using LtiLibrary.Core.Common;
using LtiLibrary.Core.Outcomes.v1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Lti
{
    public class LtiLibraryOutcomesClient
    {
        public BasicResult PostScore(string serviceUrl, string consumerKey, string consumerSecret, string lisResultSourcedId, double? score)
        {
            return OutcomesClient.PostScore(serviceUrl, consumerKey, consumerSecret, lisResultSourcedId, score);
        }
    }
}