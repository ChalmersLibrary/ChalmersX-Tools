using ChalmersxTools.Database;
using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using LtiLibrary.Core.Lti1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChalmersxTools.Sessions
{
    public interface ISessionManager : IDisposable
    {
        LearningToolServerDbContext DbContext { get; }

        LtiSession GetAndRefreshSession(Guid ltiSessionId, string userHostAddress);
        LtiSession TryToExtractSessionFromRequest(HttpRequestBase request);
        LtiSession CreateValidSession(LtiSession session);
        void RefreshSession(LtiSession session);
        void UpdateLtiRequest(LtiSession session, LtiRequest ltiRequest);
    }
}
