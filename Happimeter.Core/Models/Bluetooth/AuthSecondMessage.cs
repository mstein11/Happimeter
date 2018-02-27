using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class AuthSecondMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "AUSecond";
        public AuthSecondMessage() : base(MessageNameConstant)
        {
        }

        public int HappimeterUserId { get; set; }
        public string Password { get; set; }
        public string PhoneOs { get; set; }
        public string HappimeterUsername { get; set; }
    }
}
