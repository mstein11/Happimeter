using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class AuthFirstMessage : BaseBluetoothMessage
    {
        public AuthFirstMessage() : base("AuthFirstMessage")
        {
            MessageValue = "Hallo";
        }

    }
}
