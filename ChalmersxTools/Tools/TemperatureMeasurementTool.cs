using ChalmersxTools.Models;
using ChalmersxTools.Models.View;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Tools
{
    public class TemperatureMeasurementTool : ToolBase
    {
        public static readonly string CONSUMER_KEY = "ChalmersxTemperatureMeasurementTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        override protected string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        public override ViewIdentifierAndModel HandleRequest(HttpRequestBase request)
        {
            if (request.Form["action"] == "create")
            {
                CreateSubmission(request);
            }

            if (request.Form["action"] == "edit")
            {
                EditSubmission(request);
            }

            return new ViewIdentifierAndModel("~/Views/TemperatureMeasurementToolView.cshtml",
                new TemperatureMeasurementToolViewModel()
                {
                    LtiSessionId = _session.Id.ToString()
                });
        }

        public override CsvFileData HandleDataRequest()
        {
            return null;
        }
        #region Private methods

        private void CreateSubmission(HttpRequestBase request)
        {
            try { 
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
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create submission.", e);
            }
        }

        private void EditSubmission(HttpRequestBase request)
        {
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

            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }
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

        #endregion
    }
}