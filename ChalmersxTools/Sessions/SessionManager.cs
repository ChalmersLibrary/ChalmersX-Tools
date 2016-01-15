using ChalmersxTools.Database;
using ChalmersxTools.Models;
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
                var ltiSessionId = Guid.Parse(request.Form["ltiSessionId"].ToString());

                try
                {
                    LtiSession existingLtiSession = null;

                    existingLtiSession = (from s in _dbContext.LtiSessions
                         where s.Id == ltiSessionId
                         select s).SingleOrDefault();

                    if (existingLtiSession != null)
                    {
                        if (existingLtiSession.Timestamp < DateTime.Now.AddDays(-1))
                        {
                            // Remove the session if it is older than a day.
                            _dbContext.LtiSessions.Remove(existingLtiSession);
                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            res = existingLtiSession;
                            res.Valid = true;
                            res.DeserializeLtiRequest();
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to check for valid session ID.", e);
                }
            }

            return res;
        }

        public LtiSession CreateValidSession(LtiSession session)
        {
            LtiSession res =
                (from s in _dbContext.LtiSessions
                 where s.ConsumerKey == session.LtiRequest.ConsumerKey &&
                 s.CourseOrg == session.CourseOrg &&
                 s.CourseId == session.CourseId &&
                 s.CourseRun == session.CourseRun &&
                 s.UserId == session.LtiRequest.UserId
                 select s).SingleOrDefault();

            if (res != null)
            {
                res.DeserializeLtiRequest();

                if (res.Timestamp < DateTime.Now.AddDays(-1))
                {
                    // Remove the session if it is older than a day.
                    _dbContext.LtiSessions.Remove(res);
                    res = new LtiSession()
                    {
                        ConsumerKey = session.ConsumerKey,
                        CourseOrg = session.CourseOrg,
                        CourseId = session.CourseId,
                        CourseRun = session.CourseRun,
                        UserId = session.UserId,
                        Timestamp = DateTime.Now,
                        LtiRequest = session.LtiRequest
                    };
                    res.SerializeLtiRequest();
                    res = _dbContext.LtiSessions.Add(res);
                    _dbContext.SaveChanges();
                }
                else
                {
                    RefreshSession(res);
                }
            }
            else
            {
                // Create a new session ID if we didn't have a valid session ID.
                res = new LtiSession()
                {
                    ConsumerKey = session.LtiRequest.ConsumerKey,
                    CourseOrg = session.CourseOrg,
                    CourseId = session.CourseId,
                    CourseRun = session.CourseRun,
                    UserId = session.LtiRequest.UserId,
                    Timestamp = DateTime.Now,
                    LtiRequest = session.LtiRequest
                };
                res.SerializeLtiRequest();
                res = _dbContext.LtiSessions.Add(res);
                _dbContext.SaveChanges();

                if (res == null)
                {
                    throw new Exception("Failed to fetch newly created LTI session.");
                }
            }

            return res;
        }

        public void RefreshSession(LtiSession session)
        {
            session.Timestamp = DateTime.Now;
            _dbContext.SaveChanges();
        }
    }
}