using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Happimeter.Models.ApiResultModels;

namespace Happimeter.Models.ServiceModels
{
    public class GetProximityResultModel : AbstractResultModel
    {
        public GetProximityResultModel()
        {
            Data = new List<GetProximityResultItemModel>();
        }

        public List<GetProximityResultItemModel> Data { get; set; }

        public HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == HappimeterApiResultInformation.Success;
    }

    public class GetProximityResultItemModel
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("close_to_user_id")]
        public int CloseToUserId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mail")]
        public string Mail { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("average")]
        public double Average { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
