using System.Threading.Tasks;
using Happimeter.Interfaces;

namespace Happimeter.Services
{
    public class HappimeterApiService : IHappimeterApiService
    {
        private IRestService _restService;
        //private const string ApiUrl = "http://api.happimeter.org";
        private const string ApiUrl = "http://localhost:4711";
        private const string ApiPathAuth = "/v1/auth";

        private static string GetUrlForPath(string path) {
            return ApiUrl + path;
        }

        public HappimeterApiService()
        {
            _restService = ServiceLocator.Instance.Get<IRestService>();
        }

        public async Task<object> Auth(string email, string password)
        {
            var url = GetUrlForPath(ApiPathAuth);
            var data = new { mail = email, password };
            var result = await _restService.Post(url, data);
            return result;
        }
    }
}
