using System;
using System.Collections.Generic;

namespace Happimeter.Models.ServiceModels
{
    public class GetGenericQuestionApiResult
    {
        public GetGenericQuestionApiResult()
        {
            Questions = new List<string>();
        }

        public List<string> Questions { get; set; }

        public HappimeterApiResultInformation ResultType { get; set; }
        public bool IsSuccess => ResultType == HappimeterApiResultInformation.Success;
    }
}
