using ChalmersxTools.Models;
using ChalmersxTools.Models.View;
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
            return new ViewIdentifierAndModel("~/Views/EarthSpheresImageToolView.cshtml",
                new EarthSpheresImageToolViewModel()
                {
                    LtiSessionId = _session.Id.ToString()
                });
        }

        public override CsvFileData HandleDataRequest()
        {
            string data = "", courseOrg = "", courseId = "", courseRun = "";

            courseOrg = _session.CourseOrg;
            courseId = _session.CourseId;
            courseRun = _session.CourseRun;

            return new CsvFileData(courseOrg + "-" + courseId + "-" + courseRun + "-earth-spheres-images.csv",
                new System.Text.UTF8Encoding().GetBytes(data));
        }
    }
}