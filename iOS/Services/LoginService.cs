using System;
using System.Threading.Tasks;
using Happimeter.Interfaces;

namespace Happimeter.iOS.Services
{
    public class LoginService
    {
        private IHappimeterApiService _happimeterApiService;

        public LoginService()
        {
            _happimeterApiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
        }

        public async Task Login(string userName, string password) {
            var result = await _happimeterApiService.Auth(userName, password);
            Console.WriteLine(result.ToString());
        }
    }
}
