using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Happimeter.Services
{
    public class RestService
    {
        private HttpClient _httpClient;
        public RestService()
        {
            _httpClient = new HttpClient();
            //smaller than default value
            _httpClient.MaxResponseContentBufferSize = 256000;
        }

        public RestService(string authToken) : this()
        {
            AddAuthorizationTokenToInstance(authToken);
        }

        public void AddAuthorizationTokenToInstance(string token) {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<HttpResponseMessage> Get(string url) {
            return await _httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> Post(string url, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(url, content);
        }

    }
}
