using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
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
    public class EarthSpheresImageTool : SimpleDataStorageToolBase
    {
        public static readonly string CONSUMER_KEY = "ChalmersxEarthSpheresImageTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        protected override string ConsumerSecret { get { return ConfigurationManager.AppSettings["ltiConsumerSecret"]; } }

        public override CsvFileData HandleDataRequest()
        {
            string data = "";
            data += "sphere1Name,sphere1Url,sphere1Location,sphere1Latitude,sphere1Longitude,sphere2Name,sphere2Url,sphere2Location,sphere2Latitude,sphere2Longitude\n";
            foreach (var submission in GetAllSubmissionsForCourseRun())
            {
                data += csv(submission.Sphere1Name) + "," + 
                    csv(submission.Sphere1Url) + "," + 
                    csv(submission.Sphere1Location) + "," +
                    csv(submission.Sphere1Coordinate.Latitude.ToString()) + "," +
                    csv(submission.Sphere1Coordinate.Longitude.ToString()) + "," +
                    csv(submission.Sphere2Name) + "," + 
                    csv(submission.Sphere2Url) + "," +
                    csv(submission.Sphere2Location) + "," +
                    csv(submission.Sphere2Coordinate.Latitude.ToString()) + "," +
                    csv(submission.Sphere2Coordinate.Longitude.ToString()) + "\n";
            }
            return new CsvFileData(_session.ContextId + "-earth-spheres-images.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }

        public override ViewIdentifierAndModel HandleVisualizationRequest()
        {
            var model = new EarthSpheresImageGalleryViewModel();

            foreach (var submission in GetAllSubmissionsForCourseRun())
            {
                var img1 = new GeoImage()
                {
                    ImageUrl = submission.Sphere1Url,
                    Coordinate = new Coordinate(submission.Sphere1Coordinate.Latitude, submission.Sphere1Coordinate.Longitude)
                };

                var img2 = new GeoImage()
                {
                    ImageUrl = submission.Sphere2Url,
                    Coordinate = new Coordinate(submission.Sphere2Coordinate.Latitude, submission.Sphere2Coordinate.Longitude)
                };

                AddSphereImageToModel(model, img1, submission.Sphere1Name);
                AddSphereImageToModel(model, img2, submission.Sphere2Name);
            }

            return new ViewIdentifierAndModel("~/Views/EarthSpheresImageGalleryView.cshtml", model);
        }

        protected override string Create(HttpRequestBase request)
        {
            var res = "";

            try
            {
                var url1 = request.Form["sphere1Url"].ToString();
                var url2 = request.Form["sphere2Url"].ToString();

                if (!url1.StartsWith("https"))
                {
                    throw new Exception("URL to image 1 have to start with https.");
                }

                if (!url2.StartsWith("https"))
                {
                    throw new Exception("URL to image 2 have to start with https.");
                }

                var url1IsEmpty = String.IsNullOrWhiteSpace(url1);
                var url2IsEmpty = String.IsNullOrWhiteSpace(url2);
                var url1IsValidImageUrl = CanAccessImageUrl(url1);
                var url2IsValidImageUrl = CanAccessImageUrl(url2);
                double sphere1Latitude, sphere1Longitude, sphere2Latitude, sphere2Longitude;
                var sphere1LatitudeParsed = Double.TryParse(request.Form["sphere1Latitude"], out sphere1Latitude);
                var sphere1LongitudeParsed = Double.TryParse(request.Form["sphere1Longitude"], out sphere1Longitude);
                var sphere2LatitudeParsed = Double.TryParse(request.Form["sphere2Latitude"], out sphere2Latitude);
                var sphere2LongitudeParsed = Double.TryParse(request.Form["sphere2Longitude"], out sphere2Longitude);

                if ((!url1IsEmpty && (!sphere1LatitudeParsed || !sphere1LongitudeParsed)) ||
                    (!url2IsEmpty && (!sphere2LatitudeParsed || !sphere2LongitudeParsed)))
                {
                    res = "<span style='color: red;'>Failed to parse coordinates.</span>";
                }
                else
                {
                    var newSubmission = _sessionManager.DbContext.EarthSpheresImagesSubmissions.Add(new EarthSpheresImagesSubmission()
                    {
                        UserId = _session.UserId,
                        ContextId = _session.ContextId,
                        Sphere1Name = (url1IsValidImageUrl ? request.Form["sphere1Name"].ToString() : ""),
                        Sphere1Url = (url1IsValidImageUrl ? url1 : ""),
                        Sphere1Location = (url1IsValidImageUrl ? request.Form["sphere1Location"].ToString() : ""),
                        Sphere1Coordinate = (url1IsValidImageUrl ? new Coordinate(sphere1Latitude, sphere1Longitude) : new Coordinate()),
                        Sphere2Name = (url2IsValidImageUrl ? request.Form["sphere2Name"].ToString() : ""),
                        Sphere2Url = (url2IsValidImageUrl ? url2 : ""),
                        Sphere2Location = (url2IsValidImageUrl ? request.Form["sphere2Location"].ToString() : ""),
                        Sphere2Coordinate = (url2IsValidImageUrl ? new Coordinate(sphere2Latitude, sphere2Longitude) : new Coordinate())
                    });

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(newSubmission, url1IsEmpty, url1IsValidImageUrl, url2IsEmpty, url2IsValidImageUrl);
                }
            }
            catch (Exception e)
            {
                res = "<span style='color: red;'>Failed to create submission: " + e.Message + "</span>";
            }

            return res;
        }

        protected override string Edit(HttpRequestBase request)
        {
            var res = "";

            try
            {
                var url1 = request.Form["sphere1Url"].ToString();
                var url2 = request.Form["sphere2Url"].ToString();

                if (!url1.StartsWith("https"))
                {
                    throw new Exception("URL to image 1 have to start with https.");
                }

                if (!url2.StartsWith("https"))
                {
                    throw new Exception("URL to image 2 have to start with https.");
                }

                var url1IsEmpty = String.IsNullOrWhiteSpace(url1);
                var url2IsEmpty = String.IsNullOrWhiteSpace(url2);
                var url1IsValidImageUrl = CanAccessImageUrl(url1);
                var url2IsValidImageUrl = CanAccessImageUrl(url2);
                double sphere1Latitude, sphere1Longitude, sphere2Latitude, sphere2Longitude;
                var sphere1LatitudeParsed = Double.TryParse(request.Form["sphere1Latitude"], out sphere1Latitude);
                var sphere1LongitudeParsed = Double.TryParse(request.Form["sphere1Longitude"], out sphere1Longitude);
                var sphere2LatitudeParsed = Double.TryParse(request.Form["sphere2Latitude"], out sphere2Latitude);
                var sphere2LongitudeParsed = Double.TryParse(request.Form["sphere2Longitude"], out sphere2Longitude);

                if ((!url1IsEmpty && (!sphere1LatitudeParsed || !sphere1LongitudeParsed)) ||
                    (!url2IsEmpty && (!sphere2LatitudeParsed || !sphere2LongitudeParsed)))
                {
                    res = "<span style='color: red;'>Failed to parse coordinates.</span>";
                }
                else
                {
                    EarthSpheresImagesSubmission existing =
                        (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
                         where o.UserId == _session.UserId &&
                         o.ContextId == _session.ContextId
                         select o).SingleOrDefault();

                    existing.Sphere1Name = (url1IsValidImageUrl ? request.Form["sphere1Name"].ToString() : "");
                    existing.Sphere1Url = (url1IsValidImageUrl ? url1 : "");
                    existing.Sphere1Location = (url1IsValidImageUrl ? request.Form["sphere1Location"].ToString() : "");
                    existing.Sphere1Coordinate = (url1IsValidImageUrl ? new Coordinate(sphere1Latitude, sphere1Longitude) : new Coordinate());
                    existing.Sphere2Name = (url2IsValidImageUrl ? request.Form["sphere2Name"].ToString() : "");
                    existing.Sphere2Url = (url2IsValidImageUrl ? url2 : "");
                    existing.Sphere2Location = (url2IsValidImageUrl ? request.Form["sphere2Location"].ToString() : "");
                    existing.Sphere2Coordinate = (url2IsValidImageUrl ? new Coordinate(sphere2Latitude, sphere2Longitude) : new Coordinate());

                    _sessionManager.DbContext.SaveChanges();

                    res = SubmitScore(existing, url1IsEmpty, url1IsValidImageUrl, url2IsEmpty, url2IsValidImageUrl);
                }
            }
            catch (Exception e)
            {
                res = "<span style='color: red;'>Failed to edit existing submission: " + e.Message + "</span>";
            }

            return res;
        }

        protected override ViewIdentifierAndModel GetViewIdentifierAndModel(string message)
        {
            int numberOfSubmissions = 0;

            try
            {
                numberOfSubmissions = (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
                     where o.ContextId == _session.ContextId
                     select o).ToList().Count;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get all previous submissions.", e);
            }

            return new ViewIdentifierAndModel("~/Views/EarthSpheresImageToolView.cshtml",
                new EarthSpheresImageToolViewModel()
                {
                    Submission = GetSubmissionForCurrentStudent(),
                    LtiSessionId = _session.Id.ToString(),
                    Roles = _session.LtiRequest.Roles,
                    ResponseMessage = message,
                    NumberOfSubmissions = numberOfSubmissions
                });
        }

        #region Private methods

        private EarthSpheresImagesSubmission GetSubmissionForCurrentStudent()
        {
            EarthSpheresImagesSubmission res = null;

            try
            {
                res = (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
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

        private List<EarthSpheresImagesSubmission> GetAllSubmissionsForCourseRun()
        {
            var res = new List<EarthSpheresImagesSubmission>();

            try
            {
                res = (from o in _sessionManager.DbContext.EarthSpheresImagesSubmissions
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

        private string SubmitScore(EarthSpheresImagesSubmission submission, bool url1IsEmpty, bool canAccessImage1, bool url2IsEmpty, bool canAccessImage2)
        {
            var res = "";

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

            if (!canAccessImage1 && !url1IsEmpty && !canAccessImage2 && !url2IsEmpty)
            {
                res = "<span style='color: red;'>Couldn't access any of the submitted images.</span>";
            }
            else if (!canAccessImage1 && !url1IsEmpty)
            {
                res = "<span style='color: red;'>Couldn't access image 1.</span>";
            }
            else if (!canAccessImage2 && !url2IsEmpty)
            {
                res = "<span style='color: red;'>Couldn't access image 2.</span>";
            }
            else
            {
                res = "<span style='color: green;'>Successfully saved image URLs.</span>";
            }

            return res;
        }

        private void AddSphereImageToModel(EarthSpheresImageGalleryViewModel model, GeoImage img, string sphereName)
        {
            if (sphereName == "geosphere")
            {
                model.GeosphereImages.Add(img);
            }
            else if (sphereName == "atmosphere")
            {
                model.AtmosphereImages.Add(img);
            }
            else if (sphereName == "biosphere")
            {
                model.BiosphereImages.Add(img);
            }
            else if (sphereName == "hydrosphere")
            {
                model.HydrosphereImages.Add(img);
            }
            else if (sphereName == "cryosphere")
            {
                model.CryosphereImages.Add(img);
            }
        }

        #endregion
    }
}