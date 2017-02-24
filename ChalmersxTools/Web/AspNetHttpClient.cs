using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace ChalmersxTools.Web
{
    public class SystemNetHttpClient : IWebApiClient
    {
        HttpClient _client;

        public SystemNetHttpClient()
        {
            _client = new HttpClient();
        }

        public dynamic GetJson(string url)
        {
            dynamic res = null;

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var asyncOperation = _client.GetAsync(url);
            asyncOperation.Wait();
            if (asyncOperation.Result.IsSuccessStatusCode)
            {
                var asyncOperation2 = asyncOperation.Result.Content.ReadAsAsync<dynamic>();
                asyncOperation2.Wait();
                res = asyncOperation2.Result;
            }

            return res;
        }
    }
}