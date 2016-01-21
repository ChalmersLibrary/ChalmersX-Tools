using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using ChalmersxTools.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ChalmersxTools.Tools
{
    public interface ITool
    {
        string ConsumerKey { get; }
        bool IsAuthorized(HttpRequestBase request, string consumerSecret);
        ITool SetSessionManager(ISessionManager sessionManager);
        ITool SetSession(LtiSession session);
        ViewIdentifierAndModel HandleRequest(HttpRequestBase request);
        CsvFileData HandleDataRequest();
    }
}
