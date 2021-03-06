﻿using System;
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
using ChalmersxTools.Models.Database;
using ChalmersxTools.Models.View;

namespace ChalmersxTools.Controllers
{
    public class ToolController : Controller
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

                    // Try to get the LTI request from Request.
                    try
                    {
                        Request.CheckForRequiredLtiParameters();

                        var newLtiRequest = new LtiRequest(null);
                        newLtiRequest.ParseRequest(Request);

                        tool = _unityContainer.Resolve<ITool>(newLtiRequest.ConsumerKey);

                        if (!tool.IsAuthorized(Request, newLtiRequest.Signature))
                        {
                            throw new Exception("Unauthorized.");
                        }

                        // Update the LTI request in the session.
                        if (session.Valid)
                        {
                            sessionManager.UpdateLtiRequest(session, newLtiRequest);
                        }
                        else
                        {
                            session.LtiRequest = newLtiRequest;
                        }
                    }
                    catch
                    {
                        if (!session.Valid)
                        {
                            throw;
                        }
                    }

                    if (session.Valid)
                    {
                        sessionManager.RefreshSession(session);
                    }
                    else
                    {
                        session.UserHostAddress = Request.UserHostAddress;
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
            catch (Exception e)
            {
                _log.Error("Failed to get tool: " + e.Message);
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
                    session = sessionManager.GetAndRefreshSession(Guid.Parse(ltiSessionId), Request.UserHostAddress);

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
                _log.Error("Failed to get data: " + e.Message);
                res = new HttpNotFoundResult("Failed to download data: " + e.Message);
            }

            return res;
        }

        [HttpGet]
        public ActionResult GetVisualization(string ltiSessionId)
        {
            ActionResult res = new HttpNotFoundResult("Failed to get visualization: unknown error");

            try
            {
                LtiSession session = null;

                using (var sessionManager = _unityContainer.Resolve<ISessionManager>())
                {
                    session = sessionManager.GetAndRefreshSession(Guid.Parse(ltiSessionId), Request.UserHostAddress);

                    if (!session.Valid)
                    {
                        throw new Exception("Unauthorized.");
                    }
                    else
                    {
                        var tool = _unityContainer.Resolve<ITool>(session.ConsumerKey)
                            .SetSessionManager(sessionManager)
                            .SetSession(session);

                        var visualizationData = tool.HandleVisualizationRequest();

                        res = View(visualizationData.ViewIdentifier, visualizationData.Model);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to get visualization: " + e.Message);
                res = new HttpNotFoundResult("Failed to get visualization: " + e.Message);
            }

            return res;
        }
    }
}