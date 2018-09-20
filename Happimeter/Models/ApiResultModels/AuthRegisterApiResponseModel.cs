using System;
namespace Happimeter.Models.ApiResultModels
{
    public class AuthRegisterApiResponseModel
    {

        public int Status { get; set; }
        public int Id { get; set; }
        public string Token { get; set; }
        public int Expires { get; set; }
    }
}
