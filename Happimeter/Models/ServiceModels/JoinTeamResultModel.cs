using System;
using Happimeter.Models.ApiResultModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Happimeter.Models.ServiceModels
{
    public class JoinTeamResultModel : AbstractResultModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
