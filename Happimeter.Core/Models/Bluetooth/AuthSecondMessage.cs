using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class AuthSecondMessage : BaseBluetoothMessage
    {
        public AuthSecondMessage() : base("AuthSecondMessage")
        {
        }

        public int HappimeterUserId { get; set; }
        public string Password { get; set; }
        public string PhoneOs { get; set; }
        public string HappimeterUsername { get; set; }
    }
}
