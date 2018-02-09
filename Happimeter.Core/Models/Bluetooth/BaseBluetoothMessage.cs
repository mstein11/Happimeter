using System;
namespace Happimeter.Core.Models.Bluetooth
{
    public class BaseBluetoothMessage
    {
        public BaseBluetoothMessage(string name)
        {
            MessageName = name;
        }


        public string MessageName { get; set; }
        public string MessageValue { get; set; }
    }
}
