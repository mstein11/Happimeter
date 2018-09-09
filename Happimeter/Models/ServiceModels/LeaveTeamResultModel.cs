using System;
using Happimeter.Models.ApiResultModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Happimeter.Models.ServiceModels
{
    public class LeaveTeamResultModel : AbstractResultModel
    {
        public LeaveTeamResultModel()
        {
        }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
