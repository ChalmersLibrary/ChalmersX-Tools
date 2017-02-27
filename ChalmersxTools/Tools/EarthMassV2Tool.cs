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
    public class EarthMassV2Tool : SimpleDataStorageToolBase
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string CONSUMER_KEY = "ChalmersxEarthMassV2Tool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        protected override string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";
            data += "meanGravityAcceleration,massOfEarth,location,latitude,longitude\n";
            foreach (var submission in GetAllData())
            {
                data += "\"" + submission.MeanGravityAcceleration.ToString() + "\",\"" +
                    submission.TotalEarthMass.ToString() + "\",\"" + 
                    submission.Location + "\",\"" + 
                    submission.Position.Latitude.ToString() + "\",\"" + 
                    submission.Position.Longitude.ToString() + "\"\n";
            }
            return new CsvFileData(_session.ContextId + "-earth-mass.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";

            try
            {
                double meanGravityAcceleration = 0, earthMass = 0, latitude = 0, longitude = 0;

                var latitudeParsed = Double.TryParse(request.Form["latitude"], out latitude);
                var longitudeParsed = Double.TryParse(request.Form["longitude"], out longitude);

                if (!Double.TryParse(request.Form["meanGravityAcceleration"].ToString(), out meanGravityAcceleration))
                {
                    res = "<span style='color: red;'>Failed to parse mean gravity acceleration.</span>";
                }
                else if (!Double.TryParse(request.Form["earthMass"].ToString(), out earthMass))
                {
                    res = "<span style='color: red;'>Failed to parse earth mass.</span>";
                }
                else if (request.Form["location"] == "" || !latitudeParsed || !longitudeParsed)
                {
                    res = "<span style='color: red;'>Failed to parse location.</span>";
                }
                else
                {
                    var newSubmission = _sessionManager.DbContext.EarthMassV2Submissions.Add(new EarthMassV2Submission()
                    {
                        UserId = _session.UserId,
                        ContextId = _session.ContextId,
                        MeanGravityAcceleration = meanGravityAcceleration,
                        TotalEarthMass = earthMass,
                        Location = request.Form["location"],
                        Position = new Coordinate(latitude, longitude)
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
                double meanGravityAcceleration = 0, earthMass = 0, latitude = 0, longitude = 0;

                var latitudeParsed = Double.TryParse(request.Form["latitude"], out latitude);
                var longitudeParsed = Double.TryParse(request.Form["longitude"], out longitude);

                if (!Double.TryParse(request.Form["meanGravityAcceleration"].ToString(), out meanGravityAcceleration))
                {
                    res = "<span style='color: red;'>Failed to parse mean gravity acceleration.</span>";
                }
                else if (!Double.TryParse(request.Form["earthMass"].ToString(), out earthMass))
                {
                    res = "<span style='color: red;'>Failed to parse earth mass.</span>";
                }
                else if (request.Form["location"] == "" || !latitudeParsed || !longitudeParsed)
                {
                    res = "<span style='color: red;'>Failed to parse location.</span>";
                }
                else
                {
                    EarthMassV2Submission existing =
                        (from o in _sessionManager.DbContext.EarthMassV2Submissions
                         where o.UserId == _session.UserId &&
                         o.ContextId == _session.ContextId
                         select o).SingleOrDefault();

                    existing.MeanGravityAcceleration = meanGravityAcceleration;
                    existing.TotalEarthMass = earthMass;
                    existing.Location = request.Form["location"];
                    existing.Position = new Coordinate(latitude, longitude);

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(existing);
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed to edit submission: " + e.Message);
                throw new Exception("Failed to edit existing submission.", e);
            }

            return res;
        }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            double massAverage = 0;
            int numberOfSubmissions = 0;
            var measurements = new List<double>();

            try
            {
                List<EarthMassV2Submission> allSubmissions =
                    (from o in _sessionManager.DbContext.EarthMassV2Submissions
                        where o.ContextId == _session.ContextId
                        select o).ToList();

                measurements = allSubmissions.Select(x => x.TotalEarthMass).ToList();

                foreach (var submission in allSubmissions)
                {
                    massAverage += submission.TotalEarthMass;
                    numberOfSubmissions++;
                }
                massAverage = massAverage / numberOfSubmissions;
            }
            catch (Exception e)
            {
                _log.Error("Failed to get all previous submissions: " + e.Message);
                throw new Exception("Failed to get all previous submissions.", e);
            }

            return new ViewIdentifierAndModel("~/Views/EarthMassV2ToolView.cshtml",
                new EarthMassV2ToolViewModel()
                {
                    Submission = GetDataForCurrentStudent(),
                    EarthMassAverage = massAverage,
                    NumberOfSubmissions = numberOfSubmissions,
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles,
                    ResponseMessage = message,
                    Measurements = measurements
                });
        }

        #region Private methods

        private List<EarthMassV2Submission> GetAllData()
        {
            var res = new List<EarthMassV2Submission>();

            try
            {
                res = (from o in _sessionManager.DbContext.EarthMassV2Submissions
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

        private EarthMassV2Submission GetDataForCurrentStudent()
        {
            EarthMassV2Submission res = null;

            try
            {
                res = (from o in _sessionManager.DbContext.EarthMassV2Submissions
                       where o.ContextId == _session.ContextId &&
                           o.UserId == _session.UserId
                       select o).SingleOrDefault();
            }
            catch (Exception e)
            {
                _log.Error("Failed to fetch submission for current student: " + e.Message);
                throw new Exception("Failed to fetch submission for current student.", e);
            }

            return res;
        }

        private string SubmitScore(EarthMassV2Submission submission)
        {
            var res = "";

            OutcomesClient.PostScore(
                _session.LtiRequest.LisOutcomeServiceUrl,
                _session.LtiRequest.ConsumerKey,
                ConsumerSecret,
                _session.LtiRequest.LisResultSourcedId,
                1.0);

            res = "<span style='color: green;'>Successfully saved data.</span>";

            return res;
        }

        #endregion
    }
}