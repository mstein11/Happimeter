using System;
using System.Collections.Generic;

namespace Happimeter.Core.Models.Bluetooth
{
    public class DataExchangeFirstMessage : BaseBluetoothMessage
    {
        //Data exchange first message
        public const string MessageNameConstant = "DEFirst";
        public DataExchangeFirstMessage() : base(MessageNameConstant)
        {
        }
    }
}
