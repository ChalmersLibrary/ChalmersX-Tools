using ChalmersxTools.Models;
using ChalmersxTools.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LtiLibrary.AspNet.Extensions;
using ChalmersxTools.Models.Database;

namespace ChalmersxTools.Tools
{
    public abstract class ToolBase : ITool
    {
        protected ISessionManager _sessionManager;
        protected LtiSession _session;

        public abstract string ConsumerKey { get; }
        virtual protected string ConsumerSecret { get { return ""; } }

        public bool IsAuthorized(HttpRequestBase request, string consumerSecret)
        {
            var oauthSignature = request.GenerateOAuthSignature(ConsumerSecret);
            return oauthSignature.Equals(consumerSecret);
        }

        public ITool SetSessionManager(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
            return this; // For call chaining
        }

        public ITool SetSession(LtiSession session)
        {
            _session = session;
            return this; // For call chaining
        }

        public abstract ViewIdentifierAndModel HandleRequest(HttpRequestBase request);
        public abstract CsvFileData HandleDataRequest();
    }
}