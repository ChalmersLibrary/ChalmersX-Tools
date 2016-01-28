using ChalmersxTools.Database;
using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using ChalmersxTools.Models.View;
using ChalmersxTools.Sessions;
using LtiLibrary.Core.Outcomes.v1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChalmersxTools.Tools
{
    public class PresentationTool : SimpleDataStorageToolBase
    {
        public static readonly string CONSUMER_KEY = "ChalmersxPresentationTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        override protected string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";
            var presentations = GetAllStudentPresentationsForCourseRun();
            data += "name,presentation,location,latitude,longitude\n";
            foreach (var presentation in presentations)
            {
                data += "\"" + presentation.Name.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                    presentation.Presentation.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                    presentation.LocationName.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                    presentation.LocationLat.ToString() + "\",\"" +
                    presentation.LocationLong.ToString() + "\"\n";
            }
            return new CsvFileData(_session.ContextId + "-presentations.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";
            try
            {
                _sessionManager.DbContext.StudentPresentations.Add(new StudentPresentation()
                {
                    UserId = _session.UserId,
                    ContextId = _session.ContextId,
                    Name = request.Form["name"].ToString(),
                    LocationName = request.Form["location"].ToString(),
                    LocationLat = Double.Parse(request.Form["latitude"].ToString(), CultureInfo.InvariantCulture),
                    LocationLong = Double.Parse(request.Form["longitude"].ToString(), CultureInfo.InvariantCulture),
                    Presentation = request.Form["presentation"].ToString()
                });

                _sessionManager.DbContext.SaveChanges();

                OutcomesClient.PostScore(
                    _session.LtiRequest.LisOutcomeServiceUrl,
                    _session.LtiRequest.ConsumerKey,
                    ConsumerSecret,
                    _session.LtiRequest.LisResultSourcedId,
                    1.0);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create student presentation.", e);
            }
            return res;
        }

        protected override string Edit(HttpRequestBase request)
        {
            var res = "";

            try
            {
                StudentPresentation existingStudentPresentation =
                    (from sp in _sessionManager.DbContext.StudentPresentations
                     where sp.UserId == _session.UserId &&
                     sp.ContextId == _session.ContextId
                     select sp).SingleOrDefault();

                existingStudentPresentation.Name = request.Form["name"].ToString();
                existingStudentPresentation.LocationName = request.Form["location"].ToString();
                existingStudentPresentation.LocationLat = Double.Parse(request.Form["latitude"].ToString(), CultureInfo.InvariantCulture);
                existingStudentPresentation.LocationLong = Double.Parse(request.Form["longitude"].ToString(), CultureInfo.InvariantCulture);
                existingStudentPresentation.Presentation = request.Form["presentation"].ToString();

                _sessionManager.DbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }

            return res;
        }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            return new ViewIdentifierAndModel("~/Views/PresentationToolView.cshtml",
                new PresentationToolViewModel()
                {
                    CurrentStudentPresentation = GetCurrentStudentPresentation(),
                    Presentations = GetTitleTextAndCoordinateList(),
                    Roles = _session.LtiRequest.Roles,
                    LtiSessionId = _session.Id.ToString(),
                    ResponseMessage = message
                });
        }

        #region Private methods

        private StudentPresentation GetCurrentStudentPresentation()
        {
            StudentPresentation res = null;

            try
            {
                res = (from sp in _sessionManager.DbContext.StudentPresentations
                       where sp.ContextId == _session.ContextId &&
                           sp.UserId == _session.UserId
                       select sp).SingleOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch current student presentation.", e);
            }

            return res;
        }

        private List<TitleTextAndCoordinate> GetTitleTextAndCoordinateList()
        {
            var res = new List<TitleTextAndCoordinate>();

            try
            {
                var rand = new Random();
                var studentPresentations = (from sp in _sessionManager.DbContext.StudentPresentations
                                            where sp.ContextId == _session.ContextId
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

        private List<StudentPresentation> GetAllStudentPresentationsForCourseRun()
        {
            var res = new List<StudentPresentation>();

            try
            {
                res = (from sp in _sessionManager.DbContext.StudentPresentations
                       where
                           sp.ContextId == _session.ContextId
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