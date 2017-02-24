using ChalmersxTools.Models.Database;
using ChalmersxTools.Tools;
using ChalmersxTools.Web;
using LtiLibrary.Core.Common;
using LtiLibrary.Core.Lti1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChalmersxTools.Tests.Tools
{
    class SingleTemperatureMeasurementToolTest
    {
        [TestClass]
        public class ToolsTest
        {
            /// <summary>
            /// Silly test for easier debugging.
            /// </summary>
            [TestMethod, TestCategory("Integration")]
            public void HandleDataRequestCreate_ValidInput_StuffWentWell()
            {
                SingleTemperatureMeasurementSubmission res;
                var tool = new SingleTemperatureMesaurementTool(
                        new Config.Fakes.StubIConfig
                        {
                            LtiConsumerSecretGet = () => { return ""; },
                            OpenWeatherMapApiKeyGet = () => { return ConfigurationManager.AppSettings["openWeatherMapApiKey"]; }
                        },
                        new SystemNetHttpClient(), // We want to do a real request to Googles API.
                        new ChalmersxTools.Lti.Fakes.StubILtiOutcomesClient()
                        {
                            PostScoreStringStringStringStringNullableOfDouble = (a, b, c, d, e) => { return new BasicResult(true, ""); }
                        }
                    ).SetSessionManager(new Sessions.Fakes.StubISessionManager()
                    {
                        DbContextGet = () => new Database.Fakes.StubLearningToolServerDbContext()
                        {
                            SingleTemperatureMeasurementSubmissions = new System.Data.Entity.Fakes.StubDbSet<SingleTemperatureMeasurementSubmission>()
                            {
                                AddT0 = (submission) =>
                                {
                                    res = submission;
                                    return submission;
                                }
                            },
                            SaveChanges01 = () => { return 1; }
                        }
                    })
                    .SetSession(new ChalmersxTools.Models.Database.Fakes.StubLtiSession()
                    {
                        UserId = "123",
                        ContextId = "234",
                        LtiRequest = new LtiRequest()
                        {
                            LisOutcomeServiceUrl = "besturl",
                            ConsumerKey = "987",
                            LisResultSourcedId = "876"
                        }
                    });

                var formDictionary = ConvertDictionaryToNameValueCollection(new Dictionary<string, object>()
                {
                    { "action", "create" },
                    { "measurement", "5,6" },
                    { "lat", "21,300833" },
                    { "long", "-157,835468" },
                    { "time", "14:00" }
                });

                var httpRequest = new System.Web.Fakes.StubHttpRequestBase()
                {
                    FormGet = () => { return formDictionary; }
                };

                try
                {
                    tool.HandleRequest(httpRequest);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Failed to get all previous submissions.", e.Message);
                }
            }

            #region Private methods

            private NameValueCollection ConvertDictionaryToNameValueCollection(IDictionary<string, object> dictionary)
            {
                var res = new NameValueCollection();
                foreach (var kvp in dictionary)
                {
                    res[kvp.Key] = Convert.ToString(kvp.Value);
                }
                return res;
            }

            #endregion
        }
    }
}
