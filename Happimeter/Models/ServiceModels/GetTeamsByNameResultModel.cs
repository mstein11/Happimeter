using System;
using Happimeter.Models.ApiResultModels;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Happimeter.Models.ServiceModels
{
    public class GetTeamsByNameResultModel : AbstractResultModel
    {
        public GetTeamsByNameResultModel()
        {
        }

        [JsonProperty("teams")]
        public IList<GetTeamsByNameItemResultModel> Teams = new List<GetTeamsByNameItemResultModel>();
    }

    public class GetTeamsByNameItemResultModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
