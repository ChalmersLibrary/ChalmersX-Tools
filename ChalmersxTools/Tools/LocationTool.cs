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
    public class LocationTool : SimpleDataStorageToolBase
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string CONSUMER_KEY = "ChalmersxLocationTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        override protected string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";
            var presentations = GetAllLocationSubmissionsForCourseRun();
            data += "name,presentation,location,latitude,longitude\n";
            foreach (var presentation in presentations)
            {
                data += "\"" + presentation.Name.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
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
                _sessionManager.DbContext.LocationSubmissions.Add(new LocationSubmission()
                {
                    UserId = _session.UserId,
                    ContextId = _session.ContextId,
                    Name = request.Form["name"].ToString(),
                    LocationName = System.Web.HttpUtility.HtmlEncode(request.Form["location"].ToString()),
                    LocationLat = Double.Parse(request.Form["latitude"].ToString(), CultureInfo.InvariantCulture),
                    LocationLong = Double.Parse(request.Form["longitude"].ToString(), CultureInfo.InvariantCulture)
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
                _log.Error("Failed to create student presentation: " + e.Message);
                throw new Exception("Failed to create student presentation.", e);
            }
            return res;
        }

        protected override string Edit(HttpRequestBase request)
        {
            var res = "";

            try
            {
                LocationSubmission existingLocationSubmission =
                    (from sp in _sessionManager.DbContext.LocationSubmissions
                     where sp.UserId == _session.UserId &&
                     sp.ContextId == _session.ContextId
                     select sp).SingleOrDefault();

                existingLocationSubmission.Name = request.Form["name"].ToString();
                existingLocationSubmission.LocationName = request.Form["location"].ToString();
                existingLocationSubmission.LocationLat = Double.Parse(request.Form["latitude"].ToString(), CultureInfo.InvariantCulture);
                existingLocationSubmission.LocationLong = Double.Parse(request.Form["longitude"].ToString(), CultureInfo.InvariantCulture);

                _sessionManager.DbContext.SaveChanges();
            }
            catch (Exception e)
            {
                _log.Error("Failed to edit existing student presentation: " + e.Message);
                throw new Exception("Failed to edit existing student presentation.", e);
            }

            return res;
        }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            int numberOfSubmissions = 0;

            try
            {
                numberOfSubmissions = (from o in _sessionManager.DbContext.LocationSubmissions
                                       where o.ContextId == _session.ContextId
                                       select o).ToList().Count;
            }
            catch (Exception e)
            {
                _log.Error("Failed to get all previous submissions: " + e.Message);
                throw new Exception("Failed to get all previous submissions.", e);
            }

            return new ViewIdentifierAndModel("~/Views/LocationToolView.cshtml",
                new LocationToolViewModel()
                {
                    CurrentLocationSubmission = GetCurrentLocationSubmission(),
                    Locations = GetTitleTextAndCoordinateList(),
                    Roles = _session.LtiRequest.Roles,
                    LtiSessionId = _session.Id.ToString(),
                    ResponseMessage = message,
                    NumberOfSubmissions = numberOfSubmissions
                });
        }

        #region Private methods

        private LocationSubmission GetCurrentLocationSubmission()
        {
            LocationSubmission res = null;

            try
            {
                res = (from sp in _sessionManager.DbContext.LocationSubmissions
                       where sp.ContextId == _session.ContextId &&
                           sp.UserId == _session.UserId
                       select sp).SingleOrDefault();
            }
            catch (Exception e)
            {
                _log.Error("Failed to fetch current student presentation: " + e.Message);
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
                var LocationSubmissions = (from sp in _sessionManager.DbContext.LocationSubmissions
                                            where sp.ContextId == _session.ContextId
                                            select sp).ToList();
                var coordinates = new HashSet<Coordinate>();
                foreach (var sp in LocationSubmissions)
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
                        Text = "",
                        Coordinate = newCoord
                    });
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to fetch all title, text and coordinate for course run: " + e.Message);
                throw new Exception("Failed to fetch all title, text and coordinate for course run.", e);
            }

            return res;
        }

        private List<LocationSubmission> GetAllLocationSubmissionsForCourseRun()
        {
            var res = new List<LocationSubmission>();

            try
            {
                res = (from sp in _sessionManager.DbContext.LocationSubmissions
                       where
                           sp.ContextId == _session.ContextId
                       select sp).ToList();
            }
            catch (Exception e)
            {
                _log.Error("Failed to get all student presentations for course run: " + e.Message);
                throw new Exception("Failed to get all student presentations for course run.", e);
            }

            return res;
        }

        #endregion
    }
}