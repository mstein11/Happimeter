using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class AuthNotificationMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "AUNoti";
        public AuthNotificationMessage(bool accepted) : base(MessageNameConstant)
        {
            MessageValue = "Hallo";
            Accepted = accepted;
        }
        public bool Accepted { get; set; }
    }
}

