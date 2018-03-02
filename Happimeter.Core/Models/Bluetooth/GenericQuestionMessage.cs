using System;
using System.Collections.Generic;
using Happimeter.Core.Database;

namespace Happimeter.Core.Models.Bluetooth
{
    public class GenericQuestionMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "GQMess";

        public GenericQuestionMessage() : base(MessageNameConstant)
        {
            Questions = new List<GenericQuestion>();
        }

        public List<GenericQuestion> Questions { get; set; }
    }
}
