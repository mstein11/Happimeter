using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class AskMoodMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "AskMMess2";

        public AskMoodMessage() : base(MessageNameConstant)
        {
            MessageValue = "asd";
        }
    }
}
