using System;
namespace Happimeter.Models.ServiceModels
{
    public class AuthResultModel
    {
        public bool IsSuccess => ResultType == AuthResultTypes.Success;
        public AuthResultTypes ResultType { get; set; }
    }

    public enum AuthResultTypes 
    {
        Success,
        ErrorWrongCredentials,
        ErrorNoInternet,
        ErrorUnknown
        
    }
}
