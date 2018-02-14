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

        public string GetAsJson() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public byte[] GetAsBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(GetAsJson());
        }
    }
}
