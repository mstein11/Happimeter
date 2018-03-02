using System;
using System.Threading.Tasks;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Happimeter.Models.ServiceModels;

namespace Happimeter.iOS.Services
{
    public class LoginService
    {
        private IHappimeterApiService _happimeterApiService;

        public LoginService()
        {
            _happimeterApiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
        }

        public async Task<AuthResultModel> Login(string userName, string password) {
            var result = await _happimeterApiService.Auth(userName, password);
            return result;
        }
    }
}
