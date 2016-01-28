using ChalmersxTools.Models;
using ChalmersxTools.Models.View;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LtiLibrary.Core.Outcomes.v1;
using ChalmersxTools.Models.Database;
using System.Globalization;

namespace ChalmersxTools.Tools
{
    public class TemperatureMeasurementTool : SimpleDataStorageToolBase
    {
        public static readonly string CONSUMER_KEY = "ChalmersxTemperatureMeasurementTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        override protected string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            return new ViewIdentifierAndModel("~/Views/TemperatureMeasurementToolView.cshtml",
                new TemperatureMeasurementToolViewModel()
                {
                    Submission = GetSubmissionForCurrentStudent(),
                    Measurements = GetMeasurements(),  
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles,
                    ResponseMessage = message
                });
        }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";

            var submissions = GetAllSubmissionsForCourseRun();
            data += "latitude,longitude,measurement1,measurement2\n";
            foreach (var submission in submissions)
            {
                data += "\"" + submission.Position.Latitude + "\",\"" +
                    submission.Position.Longitude + "\",\"" +
                    submission.Measurement1 + "\",\"" +
                    submission.Measurement2 + "\"\n";
            }

            return new CsvFileData(_session.ContextId + "-temperature-measurements.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";

            try
            {
                double lat, lng;
                double? mes1 = null, mes2 = null;
                double mesout;
                if (Double.TryParse(request.Form["measurement1"], out mesout))
                {
                    mes1 = mesout;
                }
                if (Double.TryParse(request.Form["measurement2"], out mesout))
                {
                    mes2 = mesout;
                }

                if (!Double.TryParse(request.Form["lat"].ToString(), out lat)) {
                    res = "<span style='color: red;'>Failed to parse latitude.</span>";
                } else if (!Double.TryParse(request.Form["long"].ToString(), out lng)) {
                    res = "<span style='color: red;'>Failed to parse longitude.</span>";
                } else if (mes1 == null && mes2 == null)
                {
                    res = "<span style='color: red;'>You have to insert at least one measurement.</span>";
                } else { 
                    var newSubmission = _sessionManager.DbContext.TemperatureMeasurementSubmissions.Add(new TemperatureMeasurementSubmission()
                    {
                        UserId = _session.UserId,
                        ContextId = _session.ContextId,
                        Position = new Coordinate(lat, lng),
                        Measurement1 = mes1,
                        Measurement2 = mes2
                    });

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(newSubmission);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create submission.", e);
            }

            return res;

        }

        protected override string Edit(HttpRequestBase request)
        {
            var res = "";

            try
            {
                double lat, lng;
                double? mes1 = null, mes2 = null;
                double mesout;
                if (Double.TryParse(request.Form["measurement1"], out mesout))
                {
                    mes1 = mesout;
                }
                if (Double.TryParse(request.Form["measurement2"], out mesout))
                {
                    mes2 = mesout;
                }

                if (!Double.TryParse(request.Form["lat"].ToString(), out lat))
                {
                    res = "<span style='color: red;'>Failed to parse latitude.</span>";
                }
                else if (!Double.TryParse(request.Form["long"].ToString(), out lng))
                {
                    res = "<span style='color: red;'>Failed to parse longitude.</span>";
                }
                else if (mes1 == null && mes2 == null)
                {
                    res = "<span style='color: red;'>You have to insert at least one measurement.</span>";
                }
                else
                {

                    TemperatureMeasurementSubmission existing =
                    (from o in _sessionManager.DbContext.TemperatureMeasurementSubmissions
                     where o.UserId == _session.UserId &&
                     o.ContextId == _session.ContextId
                     select o).SingleOrDefault();

                    existing.Position = new Coordinate(lat, lng);
                    existing.Measurement1 = mes1;
                    existing.Measurement2 = mes2;

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(existing);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }

            return res;
        }

        #region Private methods

        private List<MeasurementAndCoordinate> GetMeasurements()
        {
            var res = new List<MeasurementAndCoordinate>();
            try {
                var measurements = (from o in _sessionManager.DbContext.TemperatureMeasurementSubmissions
                                    where o.ContextId == _session.ContextId
                                    select o).ToList();
                foreach (var m in measurements)
                {
                    res.Add(new MeasurementAndCoordinate()
                    {
                        Measurement1 = m.Measurement1,
                        Measurement2 = m.Measurement2,
                        Coordinate = m.Position
                    });
                }
            }
            catch (Exception e) {
                throw new Exception("Failed to fetch all measurements for course run.", e);
            }

            return res;
        }

        private string SubmitScore(TemperatureMeasurementSubmission submission)
        {
            var res = "";

            if (submission.Position != null)
            {
                if (submission.Measurement1 != null && submission.Measurement2 != null)
                {
                    OutcomesClient.PostScore(
                        _session.LtiRequest.LisOutcomeServiceUrl,
                        _session.LtiRequest.ConsumerKey,
                        ConsumerSecret,
                        _session.LtiRequest.LisResultSourcedId,
                        1.0);
                    res = "<span style='color: green;'>Your measurements are saved</span>";
                } else if (submission.Measurement1 == null || submission.Measurement2 == null)
                {
                    OutcomesClient.PostScore(
                        _session.LtiRequest.LisOutcomeServiceUrl,
                        _session.LtiRequest.ConsumerKey,
                        ConsumerSecret,
                        _session.LtiRequest.LisResultSourcedId,
                        1.0);
                    res = "<span style='color: green;'>Your measurement is saved</span>";
                }
            }
            else
            {
                res = "You have to enter your coordinates";
            }

            return res;
        }

        private TemperatureMeasurementSubmission GetSubmissionForCurrentStudent()
        {
            TemperatureMeasurementSubmission res = null;

            try
            {
                res = (from o in _sessionManager.DbContext.TemperatureMeasurementSubmissions
                       where o.ContextId == _session.ContextId &&
                           o.UserId == _session.UserId
                       select o).SingleOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch current submission.", e);
            }

            return res;
        }

        private List<TemperatureMeasurementSubmission> GetAllSubmissionsForCourseRun()
        {
            var res = new List<TemperatureMeasurementSubmission>();

            try
            {
                res = (from o in _sessionManager.DbContext.TemperatureMeasurementSubmissions
                       where
                           o.ContextId == _session.ContextId
                       select o).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get all submissions for course run.", e);
            }

            return res;
        }
        #endregion
    }
}