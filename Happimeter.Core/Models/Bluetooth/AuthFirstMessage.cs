using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class AuthFirstMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "AUFirst";
        public AuthFirstMessage() : base(MessageNameConstant)
        {
            MessageValue = "Hallo";
        }

        public AuthFirstMessage(string deviceName) : base(MessageNameConstant)
        {
            DeviceName = deviceName;
        }

        public string DeviceName { get; set; }

    }
}
