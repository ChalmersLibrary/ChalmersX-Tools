﻿using ChalmersxTools.Database;
using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using LtiLibrary.Core.Lti1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ChalmersxTools.Sessions
{
    public class SessionManager : ISessionManager
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private LearningToolServerDbContext _dbContext;

        public SessionManager(LearningToolServerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public LearningToolServerDbContext DbContext
        {
            get { return _dbContext; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                    _dbContext = null;
                }
            }
        }

        public LtiSession TryToExtractSessionFromRequest(HttpRequestBase request)
        {
            LtiSession res = new LtiSession();

            if (request.Form["ltiSessionId"] != null && request.Form["ltiSessionId"].ToString() != "")
            {
                res = GetAndRefreshSession(Guid.Parse(request.Form["ltiSessionId"].ToString()), request.UserHostAddress);
            }

            return res;
        }

        public LtiSession GetAndRefreshSession(Guid ltiSessionId, string userHostAddress)
        {
            LtiSession res = new LtiSession();

            try
            {
                LtiSession existingLtiSession = null;

                existingLtiSession = (from s in _dbContext.LtiSessions
                                      where s.Id == ltiSessionId
                                      select s).SingleOrDefault();

                if (existingLtiSession != null && existingLtiSession.UserHostAddress == userHostAddress)
                {
                    if (existingLtiSession.Timestamp < DateTime.Now.AddDays(-1))
                    {
                        // Remove the session if it is older than a day.
                        _dbContext.LtiSessions.Remove(existingLtiSession);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        RefreshSession(existingLtiSession);
                        res = existingLtiSession;
                        res.Valid = true;
                        res.DeserializeLtiRequest();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to get session: " + e.Message);
                throw new Exception("Failed to get session.", e);
            }

            return res;
        }

        public LtiSession CreateValidSession(LtiSession session)
        {
            LtiSession res =
                (from s in _dbContext.LtiSessions
                 where s.ConsumerKey == session.LtiRequest.ConsumerKey &&
                 s.ContextId == session.LtiRequest.ContextId &&
                 s.UserId == session.LtiRequest.UserId
                 select s).SingleOrDefault();

            if (res != null)
            {
                _dbContext.LtiSessions.Remove(res);
                _dbContext.SaveChanges();
            }

            res = new LtiSession()
            {
                ConsumerKey = session.LtiRequest.ConsumerKey,
                ContextId = session.LtiRequest.ContextId,
                UserId = session.LtiRequest.UserId,
                Timestamp = DateTime.Now,
                LtiRequest = session.LtiRequest,
                UserHostAddress = session.UserHostAddress
            };
            res.SerializeLtiRequest();
            res = _dbContext.LtiSessions.Add(res);
            _dbContext.SaveChanges();

            if (res == null)
            {
                throw new Exception("Failed to fetch newly created LTI session.");
            }

            return res;
        }

        public void RefreshSession(LtiSession session)
        {
            session.Timestamp = DateTime.Now;
            _dbContext.SaveChanges();
        }

        public void UpdateLtiRequest(LtiSession session, LtiRequest ltiRequest)
        {
            session.LtiRequest = ltiRequest;
            _dbContext.SaveChanges();
        }
    }
}