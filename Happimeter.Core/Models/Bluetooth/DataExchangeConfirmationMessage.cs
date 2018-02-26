using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class DataExchangeConfirmationMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "DEConfirm";

        public DataExchangeConfirmationMessage() : base(MessageNameConstant)
        {
        }
    }
}
