using System;
namespace Happimeter.Models.ServiceModels
{
    public class GetMeResultModel
    {
        public string Mail { get; set; }
        public string Token { get; set; }
        public string Id { get; set; }
        public string Exp { get; set; }
        public HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == HappimeterApiResultInformation.Success;
    }
}