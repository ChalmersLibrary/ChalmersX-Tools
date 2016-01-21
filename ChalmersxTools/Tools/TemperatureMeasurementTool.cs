using ChalmersxTools.Models;
using ChalmersxTools.Models.View;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LtiLibrary.Core.Outcomes.v1;
using ChalmersxTools.Models.Database;

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
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles,
                    ResponseMessage = message
                });
        }

        public override CsvFileData HandleDataRequest()
        {
            string data = "", courseOrg = "", courseId = "", courseRun = "";

            courseOrg = _session.CourseOrg;
            courseId = _session.CourseId;
            courseRun = _session.CourseRun;
            var submissions = GetAllSubmissionsForCourseRun();
            data += "latitude,longitude,measurement1,measurement2\n";
            foreach (var submission in submissions)
            {
                data += "\"" + submission.Position.Latitude + "\",\"" +
                    submission.Position.Longitude + "\",\"" +
                    submission.Measurement1 + "\",\"" +
                    submission.Measurement2 + "\"\n";
            }

            return new CsvFileData(courseOrg + "-" + courseId + "-" + courseRun + "-earth-spheres-images.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";

            try
            {
                var newSubmission = _sessionManager.DbContext.TemperatureMeasurementSubmissions.Add(new TemperatureMeasurementSubmission()
                {
                    UserId = _session.UserId,
                    CourseOrg = _session.CourseOrg,
                    CourseId = _session.CourseId,
                    CourseRun = _session.CourseRun,
                    Position = new Coordinate(Double.Parse(request.Form["lat"].ToString()), Double.Parse(request.Form["long"].ToString())),
                    Measurement1 = Double.Parse(request.Form["measurement1"].ToString()),
                    Measurement2 = Double.Parse(request.Form["measurement2"].ToString())
                });

                _sessionManager.DbContext.SaveChanges();

                res = SubmitScore(newSubmission);

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
                TemperatureMeasurementSubmission existing =
                    (from o in _sessionManager.DbContext.TemperatureMeasurementSubmissions
                     where o.UserId == _session.UserId &&
                     o.CourseOrg == _session.CourseOrg &&
                     o.CourseId == _session.CourseId &&
                     o.CourseRun == _session.CourseRun
                     select o).SingleOrDefault();

                existing.Position = new Coordinate(Double.Parse(request.Form["lat"].ToString()), Double.Parse(request.Form["long"].ToString()));
                existing.Measurement1 = Double.Parse(request.Form["measurement1"].ToString());
                existing.Measurement2 = Double.Parse(request.Form["measurement2"].ToString());

                _sessionManager.DbContext.SaveChanges();

                res = SubmitScore(existing);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }

            return res;
        }
        #region Private methods



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
                    res = "Your measurements are saved";
                } else if (submission.Measurement1 == null || submission.Measurement2 == null)
                {
                    OutcomesClient.PostScore(
                        _session.LtiRequest.LisOutcomeServiceUrl,
                        _session.LtiRequest.ConsumerKey,
                        ConsumerSecret,
                        _session.LtiRequest.LisResultSourcedId,
                        0.5);
                    res = "Your measurement is saved";
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
                       where o.CourseOrg == _session.CourseOrg &&
                           o.CourseId == _session.CourseId &&
                           o.CourseRun == _session.CourseRun &&
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
                           o.CourseOrg == _session.CourseOrg &&
                           o.CourseId == _session.CourseId &&
                           o.CourseRun == _session.CourseRun
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