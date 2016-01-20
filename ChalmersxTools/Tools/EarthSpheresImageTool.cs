using ChalmersxTools.Models;
using ChalmersxTools.Models.View;
using LtiLibrary.Core.Outcomes.v1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Tools
{
    public class EarthSpheresImageTool : ToolBase
    {
        public static readonly string CONSUMER_KEY = "ChalmersxEarthSpheresImageTool";

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

            return new ViewIdentifierAndModel("~/Views/EarthSpheresImageToolView.cshtml",
                new EarthSpheresImageToolViewModel()
                {
                    Submission = GetSubmissionForCurrentStudent(),
                    LtiSessionId = _session.Id.ToString()
                });
        }

        public override CsvFileData HandleDataRequest()
        {
            string data = "", courseOrg = "", courseId = "", courseRun = "";

            courseOrg = _session.CourseOrg;
            courseId = _session.CourseId;
            courseRun = _session.CourseRun;
            var submissions = GetAllSubmissionsForCourseRun();
            data += "sphere1Name,sphere1Url,sphere2Name,sphere2Url\n";
            foreach (var submission in submissions)
            {
                data += "\"" + submission.Sphere1Name.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                    submission.Sphere1Url.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                    submission.Sphere2Name.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\",\"" +
                    submission.Sphere2Url.Replace('"', '\'').Replace("\r", @"\r").Replace("\n", @"\n") + "\"\n";
            }

            return new CsvFileData(courseOrg + "-" + courseId + "-" + courseRun + "-earth-spheres-images.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        #region Private methods

        private void CreateSubmission(HttpRequestBase request)
        {
            try
            {
                var newSubmission = _sessionManager.DbContext.EarthSpheresImagesSubmissions.Add(new EarthSpheresImagesSubmission()
                {
                    UserId = _session.UserId,
                    CourseOrg = _session.CourseOrg,
                    CourseId = _session.CourseId,
                    CourseRun = _session.CourseRun,
                    Sphere1Name = request.Form["sphere1Name"].ToString(),
                    Sphere1Url = request.Form["sphere1Url"].ToString(),
                    Sphere2Name = request.Form["sphere2Name"].ToString(),
                    Sphere2Url = request.Form["sphere2Url"].ToString()
                });

                _sessionManager.DbContext.SaveChanges();

                if (!String.IsNullOrWhiteSpace(newSubmission.Sphere1Name) && !String.IsNullOrWhiteSpace(newSubmission.Sphere1Url) &&
                    !String.IsNullOrWhiteSpace(newSubmission.Sphere2Name) && !String.IsNullOrWhiteSpace(newSubmission.Sphere2Url))
                {
                    OutcomesClient.PostScore(
                        _session.LtiRequest.LisOutcomeServiceUrl,
                        _session.LtiRequest.ConsumerKey,
                        ConsumerSecret,
                        _session.LtiRequest.LisResultSourcedId,
                        1.0);
                }
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
                EarthSpheresImagesSubmission existing =
                    (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
                     where o.UserId == _session.UserId &&
                     o.CourseOrg == _session.CourseOrg &&
                     o.CourseId == _session.CourseId &&
                     o.CourseRun == _session.CourseRun
                     select o).SingleOrDefault();

                existing.Sphere1Name = request.Form["sphere1Name"].ToString();
                existing.Sphere1Url = request.Form["sphere1Url"].ToString();
                existing.Sphere2Name = request.Form["sphere2Name"].ToString();
                existing.Sphere2Url = request.Form["sphere2Url"].ToString();

                _sessionManager.DbContext.SaveChanges();

                if (!String.IsNullOrWhiteSpace(existing.Sphere1Name) && !String.IsNullOrWhiteSpace(existing.Sphere1Url) &&
                    !String.IsNullOrWhiteSpace(existing.Sphere2Name) && !String.IsNullOrWhiteSpace(existing.Sphere2Url))
                {
                    OutcomesClient.PostScore(
                        _session.LtiRequest.LisOutcomeServiceUrl,
                        _session.LtiRequest.ConsumerKey,
                        ConsumerSecret,
                        _session.LtiRequest.LisResultSourcedId,
                        1.0);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }
        }

        private EarthSpheresImagesSubmission GetSubmissionForCurrentStudent()
        {
            EarthSpheresImagesSubmission res = null;

            try
            {
                res = (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
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

        private List<EarthSpheresImagesSubmission> GetAllSubmissionsForCourseRun()
        {
            var res = new List<EarthSpheresImagesSubmission>();

            try
            {
                res = (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
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