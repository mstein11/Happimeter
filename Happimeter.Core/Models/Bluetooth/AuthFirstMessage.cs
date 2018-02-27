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

    }
}
