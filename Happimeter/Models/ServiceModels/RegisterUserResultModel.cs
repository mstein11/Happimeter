using System;
namespace Happimeter.Models.ServiceModels
{
    public class RegisterUserResultModel
    {
        public RegisterUserResultModel()
        {
        }

        public bool IsSuccess => ResultType == RegisterUserResultTypes.Success;
        public RegisterUserResultTypes ResultType { get; set; }
    }

    public enum RegisterUserResultTypes
    {
        Success,
        ErrorUserAlreadyTaken,
        ErrorInvalidEmail,
        ErrorPasswordInsufficient,
        ErrorNoInternet,
        ErrorUnknown
    }
}
