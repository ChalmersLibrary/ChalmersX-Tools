using ChalmersxTools.Models;
using ChalmersxTools.Models.View;
using LtiLibrary.Core.Outcomes.v1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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
            var res = "";

            if (request.Form["action"] == "create")
            {
                res = CreateSubmission(request);
            }

            if (request.Form["action"] == "edit")
            {
                res = EditSubmission(request);
            }

            return new ViewIdentifierAndModel("~/Views/EarthSpheresImageToolView.cshtml",
                new EarthSpheresImageToolViewModel()
                {
                    Submission = GetSubmissionForCurrentStudent(),
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles, 
                    ResponseMessage = res
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

        private string CreateSubmission(HttpRequestBase request)
        {
            var res = "";

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

                res = SubmitScore(newSubmission);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create submission.", e);
            }

            return res;
        }

        private string EditSubmission(HttpRequestBase request)
        {
            var res = "";

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

                res = SubmitScore(existing);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to edit existing student presentation.", e);
            }

            return res;
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

        private bool CanAccessImageUrl(string url)
        {
            var res = !String.IsNullOrWhiteSpace(url);

            if (res)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                    request.Method = "HEAD";
                    res = request.GetResponse().ContentType.Contains("image");
                }
                catch
                {
                    res = false;
                }
            }

            return res;
        }

        private string SubmitScore(EarthSpheresImagesSubmission submission)
        {
            var res = "";

            var canAccessImage1 = CanAccessImageUrl(submission.Sphere1Url);
            var canAccessImage2 = CanAccessImageUrl(submission.Sphere2Url);

            if (canAccessImage1 && !String.IsNullOrWhiteSpace(submission.Sphere1Name) &&
                canAccessImage2 && !String.IsNullOrWhiteSpace(submission.Sphere2Name))
            {
                OutcomesClient.PostScore(
                    _session.LtiRequest.LisOutcomeServiceUrl,
                    _session.LtiRequest.ConsumerKey,
                    ConsumerSecret,
                    _session.LtiRequest.LisResultSourcedId,
                    1.0);
            }
            else if ((canAccessImage1 && !String.IsNullOrWhiteSpace(submission.Sphere1Name)) ||
                (canAccessImage2 && !String.IsNullOrWhiteSpace(submission.Sphere2Name)))
            {
                OutcomesClient.PostScore(
                    _session.LtiRequest.LisOutcomeServiceUrl,
                    _session.LtiRequest.ConsumerKey,
                    ConsumerSecret,
                    _session.LtiRequest.LisResultSourcedId,
                    0.5);
            }

            if (!canAccessImage1 && !canAccessImage2)
            {
                res = "<span style='color: red;'>Couldn't access any of the submitted images.</span>";
            }
            else if (!canAccessImage1 && !String.IsNullOrWhiteSpace(submission.Sphere1Url))
            {
                res = "<span style='color: red;'>Couldn't access image 1.</span>";
            }
            else if (!canAccessImage2 && !String.IsNullOrWhiteSpace(submission.Sphere2Url))
            {
                res = "<span style='color: red;'>Couldn't access image 2.</span>";
            }
            else
            {
                res = "<span style='color: green;'>Successfully saved image URLs.</span>";
            }

            return res;
        }

        #endregion
    }
}