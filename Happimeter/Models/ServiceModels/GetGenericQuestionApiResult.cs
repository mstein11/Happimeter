using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Happimeter.Models.ApiResultModels;

namespace Happimeter.Models.ServiceModels
{
    public class GetGenericQuestionApiResult : AbstractResultModel
    {
        public GetGenericQuestionApiResult()
        {
            Questions = new List<GenericQuestionItemApiResult>();
        }

        public List<GenericQuestionItemApiResult> Questions { get; set; }
    }

    public class GenericQuestionItemApiResult
    {
        public int Id { get; set; }
        public string Question { get; set; }
        [JsonProperty("question_short")]
        public string QuestionShort { get; set; }
    }
}
