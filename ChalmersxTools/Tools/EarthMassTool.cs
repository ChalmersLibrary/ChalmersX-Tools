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
            return new CsvFileData(_session.ContextId + "-earth-mass.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";

            try
            {
                double meanGravityAcceleration = 0, earthMass = 0;

                if (!Double.TryParse(request.Form["meanGravityAcceleration"].ToString(), out meanGravityAcceleration))
                {
                    res = "<span style='color: red;'>Failed to parse mean gravity acceleration.</span>";
                }
                else if (!Double.TryParse(request.Form["earthMass"].ToString(), out earthMass))
                {
                    res = "<span style='color: red;'>Failed to parse earth mass.</span>";
                }
                else
                {
                    var newSubmission = _sessionManager.DbContext.EarthMassSubmissions.Add(new EarthMassSubmission()
                    {
                        UserId = _session.UserId,
                        ContextId = _session.ContextId,
                        MeanGravityAcceleration = meanGravityAcceleration,
                        TotalEarthMass = earthMass
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
                double meanGravityAcceleration = 0, earthMass = 0;

                if (!Double.TryParse(request.Form["meanGravityAcceleration"].ToString(), out meanGravityAcceleration))
                {
                    res = "<span style='color: red;'>Failed to parse mean gravity acceleration.</span>";
                }
                else if (!Double.TryParse(request.Form["earthMass"].ToString(), out earthMass))
                {
                    res = "<span style='color: red;'>Failed to parse earth mass.</span>";
                }
                else
                {
                    EarthMassSubmission existing =
                        (from o in _sessionManager.DbContext.EarthMassSubmissions
                         where o.UserId == _session.UserId &&
                         o.ContextId == _session.ContextId
                         select o).SingleOrDefault();

                    existing.MeanGravityAcceleration = meanGravityAcceleration;
                    existing.TotalEarthMass = earthMass;

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

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            double massAverage = 0;
            int numberOfSubmissions = 0;

            try
            {
                List<EarthMassSubmission> allSubmissions =
                    (from o in _sessionManager.DbContext.EarthMassSubmissions
                        where o.ContextId == _session.ContextId
                        select o).ToList();

                foreach (var submission in allSubmissions)
                {
                    massAverage += submission.TotalEarthMass;
                    numberOfSubmissions++;
                }
                massAverage = massAverage / numberOfSubmissions;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get all previous submissions.", e);
            }

            return new ViewIdentifierAndModel("~/Views/EarthMassToolView.cshtml",
                new EarthMassToolViewModel()
                {
                    Submission = GetDataForCurrentStudent(),
                    EarthMassAverage = massAverage,
                    NumberOfSubmissions = numberOfSubmissions,
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
                           o.ContextId == _session.ContextId
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
                       where o.ContextId == _session.ContextId &&
                           o.UserId == _session.UserId
                       select o).SingleOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch earth mass submission for current student.", e);
            }

            return res;
        }

        private string SubmitScore(EarthMassSubmission submission)
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