using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LtiLibrary.AspNet.Extensions;
using LtiLibrary.Core.Lti1;
using LtiLibrary.Core.Common;
using ChalmersxTools.Models;
using LtiLibrary.Core.Outcomes.v1;
using ChalmersxTools.Database;
using System.Globalization;
using LtiLibrary.Core.Outcomes;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.IO;
using ChalmersxTools.Sessions;
using Microsoft.Practices.Unity;
using System.Configuration;
using ChalmersxTools.Tools;

namespace ChalmersxTools.Controllers
{
    public class ToolController : Controller
    {
        private IUnityContainer _unityContainer;

        public ToolController(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        [HttpPost]
        public ActionResult Index()
        {
            ViewResult resultingView = null;

            try
            {
                LtiSession session = null;

                using (var sessionManager = _unityContainer.Resolve<ISessionManager>())
                {
                    ITool tool = null;
                    session = sessionManager.TryToExtractSessionFromRequest(Request);

                    if (!session.Valid)
                    {
                        // Try to get the LTI request from Request.
                        Request.CheckForRequiredLtiParameters();

                        session.LtiRequest = new LtiRequest(null);
                        session.LtiRequest.ParseRequest(Request);

                        tool = _unityContainer.Resolve<ITool>(session.LtiRequest.ConsumerKey);

                        if (!tool.IsAuthorized(Request, session.LtiRequest.Signature))
                        {
                            throw new Exception("Unauthorized.");
                        }
                    }

                    // Get all the course run identifiers from context ID.
                    var contextIdList = session.LtiRequest.ContextId.Split('/');
                    session.CourseOrg = contextIdList[0];
                    session.CourseId = contextIdList[1];
                    session.CourseRun = contextIdList[2];

                    if (session.Valid)
                    {
                        sessionManager.RefreshSession(session);
                    }
                    else
                    {
                        session = sessionManager.CreateValidSession(session);
                    }

                    if (tool == null)
                    {
                        tool = _unityContainer.Resolve<ITool>(session.ConsumerKey);
                    }

                    tool.SetSessionManager(sessionManager).SetSession(session);

                    var data = tool.HandleRequest(Request);

                    resultingView = View(data.ViewIdentifier, data.Model);
                }
            }
            catch (LtiException e)
            {
                resultingView = View("~/Views/StdErrorView.cshtml", new StdError()
                {
                    Message = e.Message
                });
            }

            return resultingView;
        }

        [HttpGet]
        public ActionResult GetData(string ltiSessionId)
        {
            ActionResult res = new HttpNotFoundResult("Failed to download data: unknown error");

            try
            {
                LtiSession session = null;

                using (var sessionManager = _unityContainer.Resolve<ISessionManager>())
                {
                    session = sessionManager.GetAndRefreshSession(Guid.Parse(ltiSessionId));

                    if (!session.Valid)
                    {
                        throw new Exception("Unauthorized.");
                    }
                    else
                    {
                        var tool = _unityContainer.Resolve<ITool>(session.ConsumerKey)
                            .SetSessionManager(sessionManager)
                            .SetSession(session);

                        var data = tool.HandleDataRequest();

                        res = File(data.Data, CsvFileData.CONTENT_TYPE, data.Filename);
                    }
                }
            }
            catch (Exception e)
            {
                res = new HttpNotFoundResult("Failed to download data: " + e.Message);
            }

            return res;
        }
    }
}