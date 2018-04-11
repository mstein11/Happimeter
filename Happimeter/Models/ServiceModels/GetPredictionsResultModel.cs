using System;
using Newtonsoft.Json;

namespace Happimeter.Models.ServiceModels
{
    public class GetPredictionsResultModel
    {
        public GetPredictionsResultModel()
        {
        }

        [JsonProperty("happiness")]
        public int Pleasance { get; set; }
        [JsonProperty("activation")]
        public int Activation { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }


        public HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == HappimeterApiResultInformation.Success && Status == 200;
    }
}
