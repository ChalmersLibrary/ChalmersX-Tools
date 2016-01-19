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

namespace ChalmersxTools.Controllers
{
    public class PresentationToolController : Controller
    {
        private readonly string SECRET_KEY = ConfigurationManager.AppSettings["ltiConsumerSecret"];

        private IUnityContainer _unityContainer;

        public PresentationToolController(IUnityContainer unityContainer)
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
                    session = sessionManager.TryToExtractSessionFromRequest(Request);

                    if (!session.Valid)
                    {
                        session.LtiRequest = GetLtiRequestFromHttpRequest();
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

                    if (Request.Form["action"] == "create")
                    {
                        CreateStudentPresentation(sessionManager.DbContext, session);
                    }

                    if (Request.Form["action"] == "edit")
                    {
                        EditStudentPresentation(sessionManager.DbContext, session);
                    }

                    resultingView = View(new PresentationToolModel()
                    {
                        CurrentStudentPresentation = GetCurrentStudentPresentation(sessionManager.DbContext, session),
                        Presentations = GetTitleTextAndCoordinateList(sessionManager.DbContext, session),
                        Roles = session.LtiRequest.Roles,
                        LtiSessionId = session.Id.ToString()
                    });
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

            string data = "", courseOrg = "", courseId = "", courseRun = "";

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
                        courseOrg = session.CourseOrg;
                        courseId = session.CourseId;
                        courseRun = session.CourseRun;
                        var presentations = GetAllStudentPresentationsForCourseRun(sessionManager.DbContext, session);
                        data += "name,presentation,location,latitude,longitude\n";
                        foreach (var presentation in presentations)
                        {
                            data += "\"" + presentation.Name.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                                presentation.Presentation.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                                presentation.LocationName.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                                presentation.LocationLat.ToString() + "\",\"" +
                                presentation.LocationLong.ToString() + "\"\n";
                        }

                        res = File(new System.Text.UTF8Encoding().GetBytes(data), "text/csv", courseOrg + "-" + courseId + "-" + courseRun + "-presentations.csv");
                    }
                }
            }
            catch (Exception e)
            {
                res = new HttpNotFoundResult("Failed to download data: " + e.Message);
            }

            return res;
        }

        #region Private methods

        private LtiRequest GetLtiRequestFromHttpRequest()
        {
            LtiRequest res = null;

            // Try to get the LTI request from Request.
            Request.CheckForRequiredLtiParameters();

            res = new LtiRequest(null);
            res.ParseRequest(Request);

            if (!res.ConsumerKey.Equals("12345"))
            {
                throw new Exception("Invalid consumer key.");
            }

            var oauthSignature = Request.GenerateOAuthSignature(SECRET_KEY);
            if (!oauthSignature.Equals(res.Signature))
            {
                throw new Exception("Invalid signature.");
            }

            return res;
        }

        private void CreateStudentPresentation(LearningToolServerDbContext dbContext, LtiSession session)
        {
            try
            {
                dbContext.StudentPresentations.Add(new StudentPresentation()
                {
                    UserId = session.UserId,
                    CourseOrg = session.CourseOrg,
                    CourseId = session.CourseId,
                    CourseRun = session.CourseRun,
                    Name = Request.Form["name"].ToString(),
                    LocationName = Request.Form["location"].ToString(),
                    LocationLat = Double.Parse(Request.Form["latitude"].ToString(), CultureInfo.InvariantCulture),
                    LocationLong = Double.Parse(Request.Form["longitude"].ToString(), CultureInfo.InvariantCulture),
                    Presentation = Request.Form["presentation"].ToString()
                });

                dbContext.SaveChanges();

                OutcomesClient.PostScore(
                    session.LtiRequest.LisOutcomeServiceUrl,
                    session.LtiRequest.ConsumerKey,
                    SECRET_KEY,
                    session.LtiRequest.LisResultSourcedId,
                    1.0);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create student presentation.", e);
            }
        }

        private void EditStudentPresentation(LearningToolServerDbContext dbContext, LtiSession session)
        {
            try
            {
                StudentPresentation existingStudentPresentation =
                    (from sp in dbContext.StudentPresentations
                     where sp.UserId == session.UserId &&
                     sp.CourseOrg == session.CourseOrg &&
                     sp.CourseId == session.CourseId &&
                     sp.CourseRun == session.CourseRun
                     select sp).SingleOrDefault();

                existingStudentPresentation.Name = Request.Form["name"].ToString();
                existingStudentPresentation.LocationName = Request.Form["location"].ToString();
                existingStudentPresentation.LocationLat = Double.Parse(Request.Form["latitude"].ToString(), CultureInfo.InvariantCulture);
                existingStudentPresentation.LocationLong = Double.Parse(Request.Form["longitude"].ToString(), CultureInfo.InvariantCulture);
                existingStudentPresentation.Presentation = Request.Form["presentation"].ToString();

                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }
        }

        private StudentPresentation GetCurrentStudentPresentation(LearningToolServerDbContext dbContext, LtiSession session)
        {
            StudentPresentation res = null;

            try
            {
                res = (from sp in dbContext.StudentPresentations
                       where sp.CourseOrg == session.CourseOrg &&
                           sp.CourseId == session.CourseId &&
                           sp.CourseRun == session.CourseRun &&
                           sp.UserId == session.UserId
                       select sp).SingleOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch current student presentation.", e);
            }
            
            return res;
        }

        private List<TitleTextAndCoordinate> GetTitleTextAndCoordinateList(LearningToolServerDbContext dbContext, LtiSession session)
        {
            var res = new List<TitleTextAndCoordinate>();

            try
            {
                var rand = new Random();
                var studentPresentations = (from sp in dbContext.StudentPresentations
                                            where sp.CourseOrg == session.CourseOrg &&
                                                sp.CourseId == session.CourseId &&
                                                sp.CourseRun == session.CourseRun
                                            select sp).ToList();
                var coordinates = new HashSet<Coordinate>();
                foreach (var sp in studentPresentations)
                {
                    var newCoord = new Coordinate(sp.LocationLat, sp.LocationLong);
                    while (coordinates.Contains(newCoord))
                    {
                        newCoord.Latitude += (rand.Next(0, 3) - 1) * 0.0001;
                        newCoord.Longitude += (rand.Next(0, 3) - 1) * 0.0001;
                    }
                    coordinates.Add(newCoord);

                    res.Add(new TitleTextAndCoordinate()
                    {
                        Title = sp.Name,
                        Text = sp.Presentation,
                        Coordinate = newCoord
                    });
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch all student presentations for course run.", e);
            }

            return res;
        }

        private List<StudentPresentation> GetAllStudentPresentationsForCourseRun(LearningToolServerDbContext dbContext, LtiSession session)
        {
            var res = new List<StudentPresentation>();

            try
            {
                res = (from sp in dbContext.StudentPresentations where
                            sp.CourseOrg == session.CourseOrg &&
                            sp.CourseId == session.CourseId &&
                            sp.CourseRun == session.CourseRun
                            select sp).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get all student presentations for course run.", e);
            }

            return res;
        }

        #endregion
    }
}