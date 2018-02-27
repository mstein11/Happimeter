using System;
using System.Linq;
using Happimeter.Core.Helper;

namespace Happimeter.Core.Models.Bluetooth
{
    public class WriteReceiverContext
    {
        public WriteReceiverContext(byte[] header)
        {
            if (header.Count() != 20) {
                throw new ArgumentException("Header must have size of 20 bytes");
            }
            Header = header;
            (var messageName, var messageSize) = BluetoothHelper.GetMessageHeaderContent(header);
            MessageName = messageName;
            MessageSize = messageSize;

            ReceivedMessageContent = new byte[messageSize];
        }

        public byte[] Header { get; set; }
        public string MessageName { get; set; }
        public int MessageSize { get; set; }
        public int Cursor = 0;
        public bool ReadComplete { get; set; }

        public byte[] ReceivedMessageContent { get; set; }

        public bool CanAddMessagePart(byte[] messagePart) {
            if (Cursor + messagePart.Count() > MessageSize) {
                return false;
            }
            return true;
        }

        public void AddMessagePart(byte[] messagePart) {
            foreach (var contentByte in messagePart) {
                ReceivedMessageContent[Cursor] = contentByte;
                Cursor++;
            }

            if (Cursor == MessageSize) {
                ReadComplete = true;
            }
        }

        public string GetMessageAsJson() {
            try {
                return BluetoothHelper.GetMessageJsonFromBytes(ReceivedMessageContent);    
            } catch(Exception e) {
                //if we have maleformated gzip we need to catch that!
                return null;
            }

        }
    }
}
