using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Happimeter.Models.ServiceModels
{
    public class GetSignalsModel
    {
        public GetSignalsModel()
        {
        }

        public List<SynchronizationItemModel> Data { get; set; }
        [JsonProperty("data_turn_taking")]
        public List<TurnTakingItemModel> DataTurnTaking { get; set; }

        public HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == HappimeterApiResultInformation.Success;
    }

    public class TurnTakingItemModel
    {

        public DateTime Timestamp { get; set; }
        [JsonProperty("loudest_user_id")]
        public int LoudestUserId { get; set; }
        public string UserName { get; set; }
        public double Volume { get; set; }
    }

    public class SynchronizationItemModel
    {
    }
}
