using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using ChalmersxTools.Models.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Tools
{
    public class EarthMassTool : SimpleDataStorageToolBase
    {
        public static readonly string CONSUMER_KEY = "ChalmersxEarthMassTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        protected override string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";
            data += "meanGravityAcceleration,massOfEarth\n";
            foreach (var submission in GetAllData())
            {
                data += "\"" + submission.MeanGravityAcceleration.ToString() + "\",\"" +
                    submission.TotalEarthMass.ToString() + "\"\n";
            }
            return new CsvFileData(_session.CourseOrg + "-" + _session.CourseId + "-" + _session.CourseRun + "-earth-mass.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";

            try
            {
                throw new Exception("Not implemented!");
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
                throw new Exception("Not implemented!");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }

            return res;
        }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            return new ViewIdentifierAndModel("~/Views/EarthMassToolView.cshtml",
                new EarthMassToolViewModel()
                {
                    Submission = GetDataForCurrentStudent(),
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles,
                    ResponseMessage = message
                });
        }

        #region Private methods

        private List<EarthMassSubmission> GetAllData()
        {
            var res = new List<EarthMassSubmission>();

            try
            {
                res = (from o in _sessionManager.DbContext.EarthMassSubmissions
                       where
                           o.CourseOrg == _session.CourseOrg &&
                           o.CourseId == _session.CourseId &&
                           o.CourseRun == _session.CourseRun
                       select o).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get all earth mass submissions for course run.", e);
            }

            return res;
        }

        private EarthMassSubmission GetDataForCurrentStudent()
        {
            EarthMassSubmission res = null;

            try
            {
                res = (from o in _sessionManager.DbContext.EarthMassSubmissions
                       where o.CourseOrg == _session.CourseOrg &&
                           o.CourseId == _session.CourseId &&
                           o.CourseRun == _session.CourseRun &&
                           o.UserId == _session.UserId
                       select o).SingleOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch earth mass submission for current student.", e);
            }

            return res;
        }

        #endregion
    }
}