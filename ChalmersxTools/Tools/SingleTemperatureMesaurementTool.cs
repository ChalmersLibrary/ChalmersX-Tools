using ChalmersxTools.Config;
using ChalmersxTools.Lti;
using ChalmersxTools.Models;
using ChalmersxTools.Models.Database;
using ChalmersxTools.Models.View;
using ChalmersxTools.Web;
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
        public static readonly string CONSUMER_KEY = "ChalmersxSingleTemperatureMeasurementTool";

        public override string ConsumerKey { get { return CONSUMER_KEY; } }
        override protected string ConsumerSecret { get { return _config.LtiConsumerSecret; } }

        private string _openWeatherMapApiKey { get { return _config.OpenWeatherMapApiKey; } }
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IConfig _config;
        private IWebApiClient _webApiClient;
        private ILtiOutcomesClient _ltiOutcomesClient;

        public SingleTemperatureMesaurementTool(IConfig config, IWebApiClient webApiClient, ILtiOutcomesClient ltiOutcomesClient)
        {
            _config = config;
            _webApiClient = webApiClient;
            _ltiOutcomesClient = ltiOutcomesClient;
        }

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
                double lat = 0, lng = 0;
                double? measurement = null;
                double mesout;
                DateTime time = DateTime.Now;
                if (Double.TryParse(request.Form["measurement"], out mesout))
                {
                    measurement = mesout;
                }

                var goodToGo = false;
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
                    goodToGo = true;
                }

                TempTimeAndPos stationTempTimeAndPos = null;
                if (goodToGo)
                {
                    try
                    {
                        stationTempTimeAndPos = GetTempNow(lat, lng);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Failed when fetching temperature from Open Weather Map.", e);
                        res = "<span style='color: red;'>Failed to fetch temperature.</span>";
                        goodToGo = false;
                    }
                }

                if (goodToGo)
                {
                    var measurementPosition = new Coordinate(lat, lng);
                    var newSubmission = _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions.Add(new SingleTemperatureMeasurementSubmission()
                    {
                        UserId = _session.UserId,
                        ContextId = _session.ContextId,
                        Position = measurementPosition,
                        Measurement = measurement,
                        Time = time,
                        StationPosition = stationTempTimeAndPos.Position,
                        StationMeasurement = stationTempTimeAndPos.Temp,
                        StationTime = stationTempTimeAndPos.Time,
                        DistanceInMeters = CalculateDistanceInMeters(measurementPosition, stationTempTimeAndPos.Position)
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
                double lat = 0, lng = 0;
                double? measurement = null;
                double mesout;
                DateTime time = DateTime.Now;
                if (Double.TryParse(request.Form["measurement"], out mesout))
                {
                    measurement = mesout;
                }

                var goodToGo = false;
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
                    goodToGo = true;
                }

                TempTimeAndPos stationTempTimeAndPos = null;
                if (goodToGo)
                {
                    try
                    {
                        stationTempTimeAndPos = GetTempNow(lat, lng);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Failed when fetching temperature from Open Weather Map.", e);
                        res = "<span style='color: red;'>Failed to fetch temperature.</span>";
                        goodToGo = false;
                    }
                }

                if (goodToGo)
                {
                    SingleTemperatureMeasurementSubmission existing =
                    (from o in _sessionManager.DbContext.SingleTemperatureMeasurementSubmissions
                     where o.UserId == _session.UserId &&
                     o.ContextId == _session.ContextId
                     select o).SingleOrDefault();

                    existing.Position = new Coordinate(lat, lng);
                    existing.Measurement = measurement;
                    existing.Time = time;
                    existing.StationPosition = stationTempTimeAndPos.Position;
                    existing.StationMeasurement = stationTempTimeAndPos.Temp;
                    existing.StationTime = stationTempTimeAndPos.Time;
                    existing.DistanceInMeters = CalculateDistanceInMeters(existing.Position, stationTempTimeAndPos.Position);

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
                    _ltiOutcomesClient.PostScore(
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

        private TempTimeAndPos GetTempNow(double lat, double lng)
        {
            TempTimeAndPos res = null;

            var weatherResponse = _webApiClient.GetJson("http://api.openweathermap.org/data/2.5/weather?lat=" +
                lat + "&lon=" + lng + "&apikey=" +
                _openWeatherMapApiKey + "&units=metric");

            long timestamp = 0;
            if (weatherResponse != null && weatherResponse.cod == "200")
            {
                res = new TempTimeAndPos
                {
                    Position = new Coordinate
                    {
                        Latitude = weatherResponse.coord.lat,
                        Longitude = weatherResponse.coord.lon
                    },
                    Temp = weatherResponse.main.temp
                };
                timestamp = weatherResponse.dt;
            }
            else
            {
                throw new Exception("Failed to fetch temperature from Open Weather Map.");
            }

            var timezoneResponse = _webApiClient.GetJson("https://maps.googleapis.com/maps/api/timezone/json?location=" +
                lat.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "," +
                lng.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) +
                "&timestamp=" + timestamp);

            if (timezoneResponse != null && timezoneResponse.status == "OK")
            {
                var daylightSavingsTimeOffset = timezoneResponse.dstOffset.ToObject<int>();
                var rawOffset = timezoneResponse.rawOffset.ToObject<int>();
                timestamp = timestamp + daylightSavingsTimeOffset + rawOffset;
                res.Time = new DateTime(new DateTime(1970, 1, 1).Ticks + timestamp * 10 * 1000 * 1000);
            }
            else
            {
                throw new Exception("Failed to convert time to UTC.");
            }

            return res;
        }

        private DateTime ConvertToUtc(double lat, double lng, DateTime time)
        {
            var res = time;

            var response = _webApiClient.GetJson("https://maps.googleapis.com/maps/api/timezone/json?location=" +
                lat.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "," + 
                lng.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + 
                "&timestamp=" + DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks/10/1000/1000);

            if (response.status == "OK")
            {
                var daylightSavingsTimeOffset = response.dstOffset.ToObject<double>();
                var rawOffset = response.rawOffset.ToObject<double>();
                res = res.AddSeconds(-1 * daylightSavingsTimeOffset);
                res = res.AddSeconds(-1 * rawOffset);
            }
            else
            {
                throw new Exception("Failed to convert to GMT.");
            }

            return res;
        }

        private int CalculateDistanceInMeters(Coordinate pos1, Coordinate pos2)
        {
            var radiusOfEarth = 6371e3;
            var lat1Rad = pos1.Latitude / 180 * Math.PI;
            var lat2Rad = pos2.Latitude / 180 * Math.PI;
            var lon1Rad = pos1.Longitude / 180 * Math.PI;
            var lon2Rad = pos2.Longitude / 180 * Math.PI;
            var deltaLatRad = lat2Rad - lat1Rad;
            var deltaLonRad = lon2Rad - lon1Rad;
            var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * 
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = radiusOfEarth * c;
            return Convert.ToInt32(Math.Round(distance));
        }

        #endregion
    }

    class TempTimeAndPos
    {
        public Coordinate Position { get; set; }
        public DateTime Time { get; set; }
        public Double Temp { get; set; }
    }
}