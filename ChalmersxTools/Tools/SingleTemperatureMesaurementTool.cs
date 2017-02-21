using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using ChalmersxTools.Models.View;
using LtiLibrary.Core.Outcomes.v1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Tools
{
    public class SingleTemperatureMesaurementTool : SimpleDataStorageToolBase
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string CONSUMER_KEY = "ChalmersxSingleTemperatureMeasurementTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        override protected string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            int numberOfSubmissions = 0;

            try
            {
                numberOfSubmissions = (from o in _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions
                                       where o.ContextId == _session.ContextId
                                       select o).ToList().Count;
            }
            catch (Exception e)
            {
                _log.Error("Failed to get all previous submissions: " + e.Message);
                throw new Exception("Failed to get all previous submissions.", e);
            }

            return new ViewIdentifierAndModel("~/Views/SingleTemperatureMeasurementToolView.cshtml",
                new SingleTemperatureMeasurementToolViewModel()
                {
                    Submission = GetSubmissionForCurrentStudent(),
                    Measurements = GetMeasurements(),
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles,
                    ResponseMessage = message,
                    NumberOfSubmissions = numberOfSubmissions
                });
        }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";

            var submissions = GetAllSubmissionsForCourseRun();
            data += "latitude,longitude,measurement,time\n";
            foreach (var submission in submissions)
            {
                data += "\"" + submission.Position.Latitude + "\",\"" +
                    submission.Position.Longitude + "\",\"" +
                    submission.Measurement + "\",\"" +
                    submission.Time + "\"\n";
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
                double? measurement = null;
                double mesout;
                DateTime time;
                if (Double.TryParse(request.Form["measurement"], out mesout))
                {
                    measurement = mesout;
                }

                if (!Double.TryParse(request.Form["lat"].ToString(), out lat))
                {
                    res = "<span style='color: red;'>Failed to parse latitude.</span>";
                }
                else if (!Double.TryParse(request.Form["long"].ToString(), out lng))
                {
                    res = "<span style='color: red;'>Failed to parse longitude.</span>";
                }
                else if (!DateTime.TryParseExact(request.Form["time"], "HH:mm",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Globalization.DateTimeStyles.None,
                    out time))
                {
                    res = "<span style='color: red;'>Invalid time format.</span>";
                }
                else if (measurement == null)
                {
                    res = "<span style='color: red;'>You have to insert a measurement.</span>";
                }
                else if (measurement < -50 || measurement > 50)
                {
                    res = "<span style='color: red;'>Your measurements should be in Celsius and in the range of -50 to 50.</span>";
                }
                else
                {
                    var newSubmission = _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions.Add(new SingleTemperatureMeasurementSubmission()
                    {
                        UserId = _session.UserId,
                        ContextId = _session.ContextId,
                        Position = new Coordinate(lat, lng),
                        Measurement = measurement,
                        Time = time
                    });

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(newSubmission);
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to create submission: " + e.Message);
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
                double? measurement = null;
                double mesout;
                DateTime time;
                if (Double.TryParse(request.Form["measurement"], out mesout))
                {
                    measurement = mesout;
                }

                if (!Double.TryParse(request.Form["lat"].ToString(), out lat))
                {
                    res = "<span style='color: red;'>Failed to parse latitude.</span>";
                }
                else if (!Double.TryParse(request.Form["long"].ToString(), out lng))
                {
                    res = "<span style='color: red;'>Failed to parse longitude.</span>";
                }
                else if (!DateTime.TryParseExact(request.Form["time"], "HH:mm",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Globalization.DateTimeStyles.None,
                    out time))
                {
                    res = "<span style='color: red;'>Invalid time format.</span>";
                }
                else if (measurement == null)
                {
                    res = "<span style='color: red;'>You have to insert a measurement.</span>";
                }
                else if (measurement < -50 || measurement > 50)
                {
                    res = "<span style='color: red;'>Your measurements should be in Celsius and in the range of -50 to 50.</span>";
                }
                else
                {

                    SingleTemperatureMeasurementSubmission existing =
                    (from o in _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions
                     where o.UserId == _session.UserId &&
                     o.ContextId == _session.ContextId
                     select o).SingleOrDefault();

                    existing.Position = new Coordinate(lat, lng);
                    existing.Measurement = measurement;
                    existing.Time = time;

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(existing);
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to edit existing submission: " + e.Message);
                throw new Exception("Failed to edit existing submission.", e);
            }

            return res;
        }

        #region Private methods

        private List<MeasurementWithTimeAndPlace> GetMeasurements()
        {
            var res = new List<MeasurementWithTimeAndPlace>();
            try
            {
                var measurements = (from o in _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions
                                    where o.ContextId == _session.ContextId
                                    select o).ToList();
                foreach (var m in measurements)
                {
                    res.Add(new MeasurementWithTimeAndPlace()
                    {
                        Measurement = m.Measurement,
                        Time = m.Time,
                        Coordinate = m.Position
                    });
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to fetch all measurements for course run: " + e.Message);
                throw new Exception("Failed to fetch all measurements for course run.", e);
            }

            return res;
        }

        private string SubmitScore(SingleTemperatureMeasurementSubmission submission)
        {
            var res = "";

            if (submission.Position != null)
            {
                if (submission.Measurement != null)
                {
                    OutcomesClient.PostScore(
                        _session.LtiRequest.LisOutcomeServiceUrl,
                        _session.LtiRequest.ConsumerKey,
                        ConsumerSecret,
                        _session.LtiRequest.LisResultSourcedId,
                        1.0);
                    res = "<span style='color: green;'>Your measurement is saved.</span>";
                }
            }
            else
            {
                res = "You have to enter your coordinates";
            }

            return res;
        }

        private SingleTemperatureMeasurementSubmission GetSubmissionForCurrentStudent()
        {
            SingleTemperatureMeasurementSubmission res = null;

            try
            {
                res = (from o in _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions
                       where o.ContextId == _session.ContextId &&
                           o.UserId == _session.UserId
                       select o).SingleOrDefault();
            }
            catch (Exception e)
            {
                _log.Error("Failed to fetch current submission: " + e.Message);
                throw new Exception("Failed to fetch current submission.", e);
            }

            return res;
        }

        private List<SingleTemperatureMeasurementSubmission> GetAllSubmissionsForCourseRun()
        {
            var res = new List<SingleTemperatureMeasurementSubmission>();

            try
            {
                res = (from o in _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions
                       where
                           o.ContextId == _session.ContextId
                       select o).ToList();
            }
            catch (Exception e)
            {
                _log.Error("Failed to get all submissions for course run: " + e.Message);
                throw new Exception("Failed to get all submissions for course run.", e);
            }

            return res;
        }
        #endregion
    }
}