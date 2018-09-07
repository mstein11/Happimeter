using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Happimeter.Models.ApiResultModels;
namespace Happimeter.Models.ServiceModels
{
    public class GetTeamsResultModel : AbstractResultModel
    {
        public GetTeamsResultModel()
        {
        }
        [JsonPropertyAttribute("teams")]
        public IList<TeamResultModel> Teams { get; set; } = new List<TeamResultModel>();
    }

    public class TeamResultModel
    {
        [JsonPropertyAttribute("id")]
        public int Id { get; set; }
        [JsonPropertyAttribute("name")]
        public string Name { get; set; }
        [JsonPropertyAttribute("is_admin")]
        public bool IsAdmin { get; set; }
    }
}
