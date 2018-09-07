using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Happimeter.Models.ApiResultModels;

namespace Happimeter.Models.ServiceModels
{
    public class GetSignalsModel : AbstractResultModel
    {
        public GetSignalsModel()
        {
            Data = new List<SynchronizationItemModel>();
            DataTurnTaking = new List<TurnTakingItemModel>();
        }

        public List<SynchronizationItemModel> Data { get; set; }
        [JsonProperty("data_turn_taking")]
        public List<TurnTakingItemModel> DataTurnTaking { get; set; }
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
