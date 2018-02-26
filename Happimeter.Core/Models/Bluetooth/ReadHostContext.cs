using System;
using System.Collections.Generic;
using System.Linq;
using Happimeter.Core.Helper;

namespace Happimeter.Core.Models.Bluetooth
{
    public class ReadHostContext
    {
        public byte[] Header { get; set; }
        public BaseBluetoothMessage Message { get; set; }
        public List<byte> BytesToSend { get; set; }
        public int Cursor { get; set; }


        public ReadHostContext(BaseBluetoothMessage message)
        {
            Message = message;
            Header = BluetoothHelper.GetMessageHeader(message);
            BytesToSend = BluetoothHelper.GetMessageJson(message).ToList();
            Cursor = 0;
        }

        public byte[] GetNextBytes(int amount) {
            var bytes = BytesToSend.Skip(Cursor).Take(amount).ToArray();
            //bytes.Count can be less than amount
            Cursor += bytes.Count();

            if (Cursor == BytesToSend.Count) {
                Complete = true;
            }

            return bytes;
        }

        public bool Complete { get; set; }
    }
}
