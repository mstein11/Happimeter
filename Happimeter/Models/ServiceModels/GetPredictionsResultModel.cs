using System;
using Newtonsoft.Json;
using Happimeter.Models.ApiResultModels;

namespace Happimeter.Models.ServiceModels
{
    public class GetPredictionsResultModel : AbstractResultModel
    {
        public GetPredictionsResultModel()
        {
        }

        [JsonProperty("happiness")]
        public int Pleasance { get; set; }
        [JsonProperty("activation")]
        public int Activation { get; set; }
    }
}
