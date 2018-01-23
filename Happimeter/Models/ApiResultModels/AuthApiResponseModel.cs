using System;
namespace Happimeter.Models.ApiResultModels
{
    public class AuthApiResponseModel
    {

        public int Status { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
