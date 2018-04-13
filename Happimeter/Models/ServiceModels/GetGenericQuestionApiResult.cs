using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Happimeter.Models.ServiceModels
{
    public class GetGenericQuestionApiResult
    {
        public GetGenericQuestionApiResult()
        {
            Questions = new List<GenericQuestionItemApiResult>();
        }

        public List<GenericQuestionItemApiResult> Questions { get; set; }

        public HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == HappimeterApiResultInformation.Success;
    }

    public class GenericQuestionItemApiResult
    {
        public int Id { get; set; }
        public string Question { get; set; }
        [JsonProperty("question_short")]
        public string QuestionShort { get; set; }
    }
}
