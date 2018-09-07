using System;
using Newtonsoft.Json;
namespace Happimeter.Models.ApiResultModels
{
    public abstract class AbstractResultModel
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        public ServiceModels.HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == ServiceModels.HappimeterApiResultInformation.Success && Status == 200;
    }
}
