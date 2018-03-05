﻿using System;
using System.Diagnostics;
using System.Linq;
using Happimeter.Core.ExtensionMethods;
using Happimeter.Core.Models.Bluetooth;

namespace Happimeter.Core.Helper
{
    public static class BluetoothHelper
    {

        public static byte[] GetMessageHeader(BaseBluetoothMessage message, byte[] jsonBytes = null) {
            var header = new byte[20];
            var messageNameBytes = System.Text.Encoding.UTF8.GetBytes(message.MessageName);
            if (messageNameBytes.Count() > 10)
            {
                throw new ArgumentException("Message Name must not exceed 10 bytes");
            }
            for (var i = 0; i < 10; i++)
            {
                if (messageNameBytes.Count() > i)
                {
                    header[i] = messageNameBytes[i];
                }
                else
                {
                    header[i] = 0x00;
                }
            }
            if (jsonBytes == null) {
                jsonBytes = GetMessageJson(message);     
            }

            var sizeOfMessage = jsonBytes.Count();
            var sizeOfMessageBytes = System.Text.Encoding.UTF8.GetBytes(sizeOfMessage.ToString("D"));
            if (sizeOfMessageBytes.Count() > 10)
            {
                throw new ArgumentException("Message Size must not exceed 10 bytes");
            }
            for (var i = 10; i < 20; i++)
            {
                if (sizeOfMessageBytes.Count() > i - 10)
                {
                    header[i] = sizeOfMessageBytes[i - 10];
                }
                else
                {
                    header[i] = 0x00;
                }
            }

            return header;
        }

        public static (byte[], byte[]) GetHeaderAndContent(BaseBluetoothMessage message) {
            var json = GetMessageJson(message);
            var header = GetMessageHeader(message, json);

            return (header, json);
        }

        public static (string messageName, int messageSize) GetMessageHeaderContent(byte[] header) {
            var messageNamebytes = header.Take(10).ToList();
            var removed = messageNamebytes.RemoveAll(x => x == (byte) 0x00);
            var messageName = System.Text.Encoding.UTF8.GetString(messageNamebytes.ToArray());

            var messageSizeBytes = header.Skip(10).Take(10).ToList();
            messageSizeBytes.RemoveAll(x => x == 0x00);
            var messageSize = int.Parse(System.Text.Encoding.UTF8.GetString(messageSizeBytes.ToArray()));

            return (messageName, messageSize);
        }

        public static byte[] GetMessageJson(BaseBluetoothMessage message) {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var json = message.GetAsJson();
            stopWatch.Stop();
            Debug.WriteLine($"Took {stopWatch.Elapsed.Seconds} Seconds to serialize obj");
            stopWatch.Reset();
            stopWatch.Start();
            var compressed = json.Zip();
            stopWatch.Stop();
            Console.WriteLine($"Compressed json of size {json.Count()} into {compressed.Count()} via gzip. Took {stopWatch.Elapsed.Seconds} seconds");
            return compressed;
        }

        public static string GetMessageJsonFromBytes(byte[] bytes) {
            var json = bytes.Unzip();
            Console.WriteLine($"Decompressed bytes of size {bytes.Count()} into json with size of {json.Count()} via gzip");
            //var json = System.Text.Encoding.UTF8.GetString(bytes);
            return json;
        }

        public static string GetBluetoothName()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
                              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
