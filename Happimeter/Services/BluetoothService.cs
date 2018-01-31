using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Happimeter.Interfaces;
using Plugin.BluetoothLE;

namespace Happimeter.Services
{
    public class BluetoothService : IBluetoothService
    {

        public IDevice PairedDevice { get; private set; }
        public IList<IDevice> ConnectedDevices { get; private set; }
        public IList<IScanResult> FoundDevices { get; private set; }


        public const string HeartRateService = "0000180D-0000-1000-8000-00805f9b34fb";
        public const string GenericService = "00001800-0000-1000-8000-00805f9b34fb";
        public const string MiBandService = "0000FEE0-0000-1000-8000-00805f9b34fb";

        public const string NotificationHeartRate = "00002a37-0000-1000-8000-00805f9b34fb";
        public const string ControlHeartRate = "00002a39-0000-1000-8000-00805f9b34fb";
        public const string NameCharacteristic = "00002A00-0000-1000-8000-00805f9b34fb";//not discovered
        public static Guid ButtonTouch = Guid.Parse("00000010-0000-3512-2118-0009af100700");

        //public const string MiAuthCharacteristic = "00000009-0000-1000-8000-00805f9b34fb";//maybe instead : 00000009-0000-3512-2118-0009af100700
        public const string MiAuthCharacteristic = "00000009-0000-3512-2118-0009af100700";//maybe instead : 00000009-0000-3512-2118-0009af100700
        public static readonly Guid NotificationCharacteristic = Guid.Parse("00002a46-0000-1000-8000-00805f9b34fb");

        public static readonly byte[] MiBandSecret = new byte[] {0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45};
        public Dictionary<Guid, Dictionary<Guid, IGattCharacteristic>> DevicesCharacteristics { get; set; }

        public BluetoothService()
        {
            ConnectedDevices = new List<IDevice>();
            FoundDevices = new List<IScanResult>();
            DevicesCharacteristics = new Dictionary<Guid, Dictionary<Guid, IGattCharacteristic>>();
        }

        public IObservable<IScanResult> StartScan() {
            var scannerObs = CrossBleAdapter.Current.ScanWhenAdapterReady();
            /*
            scannerObs.Subscribe(scan => {
                if (!FoundDevices.Select(x => x.Device.Uuid).Contains(scan.Device.Uuid)) {
                    FoundDevices.Add(scan);    
                }
            }); */
            return scannerObs;
        }

        public bool IsConnected(IDevice device) {
            return CrossBleAdapter.Current.GetConnectedDevices().Select(dev => dev.Uuid).Contains(device.Uuid);
        }

        public IObservable<object> PairDevice(IDevice device)
        {
            
            var isAvailable = device.IsPairingAvailable();
            var pairingStatus = device.PairingStatus;
            if (isAvailable) {
                var pairingObs = device.PairingRequest();
                pairingObs.Subscribe(x =>
                {
                    if (x)
                        PairedDevice = device;
                });                
            }
            device.WhenStatusChanged().Subscribe(changed => {
                Debug.WriteLine("Status changed " + changed);
            });
            var obs = device.Connect();
            obs.Subscribe(result => {
                if (device.Status == ConnectionStatus.Connected) {
                    ConnectedDevices.Add(device);
                }

                if (device.Name.Contains("MI Band 2")) {
                    //AuthMiBand(device);
                    DiscoverCharacteristics(device);
                    return;
                }

                var service = device.GetKnownService(Guid.Parse(HeartRateService));
                var charateristic = service.Subscribe(x =>
                {
                    Debug.WriteLine(x.Description);
                });

                var characteristic = device.GetKnownCharacteristics(Guid.Parse(HeartRateService), new[] { Guid.Parse(NotificationHeartRate) });
                characteristic.Subscribe(CharacteristicDebug);

                device.GetKnownCharacteristics(Guid.Parse(HeartRateService), new Guid[] { Guid.Parse(ControlHeartRate) }).Subscribe(x => {
                    if (x.CanWrite()) {
                        x.Write(new byte[]{ 21, 2, 1}).Subscribe(writeResult => {
                            Debug.WriteLine(writeResult);
                        });
                    }
                    x.EnableNotifications().Subscribe();
                    if (x.CanNotify()) {
                        x.RegisterAndNotify(true).Subscribe(y => {
                            Debug.WriteLine("asdasd");
                        });
                    }
                });

                device.GetKnownCharacteristics(Guid.Parse(GenericService), new Guid[] { Guid.Parse(NameCharacteristic) }).Subscribe(CharacteristicDebug);
                device.GetKnownCharacteristics(Guid.Parse(MiBandService), new Guid[] { ButtonTouch }).Subscribe(CharacteristicDebug);
            });
            
            return obs;
        }

        private void DiscoverCharacteristics(IDevice device) {
            device.WhenAnyCharacteristicDiscovered().Subscribe(anyDis => { 
                Debug.WriteLine("Characteristic UUid: " + anyDis.Uuid.ToString() + " Service UUid " + anyDis.Service.Uuid + " Service Descr: " + anyDis.Service.Description);
                if (device.Name.Contains("MI Band") && anyDis.Uuid == Guid.Parse(MiAuthCharacteristic))
                {
                    //if we found auth
                    if (!DevicesCharacteristics.ContainsKey(device.Uuid))
                    {
                        //if we have no Key for the current BT device
                        DevicesCharacteristics.Add(device.Uuid, new Dictionary<Guid, IGattCharacteristic>());
                    }
                    if (!DevicesCharacteristics[device.Uuid].ContainsKey(anyDis.Uuid))
                    {
                        //if we did not save auth Characteristic
                        DevicesCharacteristics[device.Uuid].Add(anyDis.Uuid, anyDis);
                        AuthMiBand2(device);
                    }
                }

                if (anyDis.Uuid == NotificationCharacteristic) {
                    if (!DevicesCharacteristics.ContainsKey(device.Uuid))
                    {
                        //if we have no Key for the current BT device
                        DevicesCharacteristics.Add(device.Uuid, new Dictionary<Guid, IGattCharacteristic>());
                    }
                    if (!DevicesCharacteristics[device.Uuid].ContainsKey(anyDis.Uuid))
                    {
                        //if we did not save notification Characteristic
                        DevicesCharacteristics[device.Uuid].Add(anyDis.Uuid, anyDis);
                    }
                }

                if (anyDis.Uuid == ButtonTouch)
                {
                    Debug.WriteLine("FoundButton");
                    if (!DevicesCharacteristics.ContainsKey(device.Uuid))
                    {
                        //if we have no Key for the current BT device
                        DevicesCharacteristics.Add(device.Uuid, new Dictionary<Guid, IGattCharacteristic>());
                    }
                    if (!DevicesCharacteristics[device.Uuid].ContainsKey(anyDis.Uuid))
                    {
                        //if we did not save notification Characteristic
                        DevicesCharacteristics[device.Uuid].Add(anyDis.Uuid, anyDis);
                    }
                }
            });
        }


        private void SendSms(IDevice device) {
            var buttonCharacteristic = DevicesCharacteristics[device.Uuid][ButtonTouch];

            buttonCharacteristic.EnableNotifications(true);
            buttonCharacteristic.WhenNotificationReceived().Subscribe(res => {
                Debug.WriteLine("Button Pressed");
            });

            var notificationCharacteristic = DevicesCharacteristics[device.Uuid][NotificationCharacteristic];


            var testString = System.Text.Encoding.ASCII.GetBytes("Test"); //shoud be 84, 101, 115, 116
            var moodString = System.Text.Encoding.ASCII.GetBytes("Mood?");

            notificationCharacteristic.Write(new byte[] { 5, 1 }.Concat(moodString).ToArray()).Subscribe(res => {
                Debug.WriteLine("Send Sms");
            });

            /*
            notificationCharacteristic.Write(new byte[] { 4, 1, 84, 101, 115, 116 }).Subscribe(res => {
                Debug.WriteLine("Send Sms");
            });

            notificationCharacteristic.Write(new byte[] { 3, 1, 84, 101, 115, 116 }).Subscribe(res => {
                Debug.WriteLine("Send Sms");
            });

            notificationCharacteristic.Write(new byte[] { 2, 1, 84, 101, 115, 116 }).Subscribe(res => {
                Debug.WriteLine("Send Sms");
            });

            notificationCharacteristic.Write(new byte[] { 1, 1, 84, 101, 115, 116 }).Subscribe(res => {
                Debug.WriteLine("Send Sms");
            });
            */
        }


        /// <summary>
        /// https://stackoverflow.com/questions/41417747/connection-to-mi-band-2?answertab=votes#tab-top
        /// </summary>
        /// <param name="device">Device.</param>
        private void AuthMiBand2(IDevice device) {
            var authCharacteristic = DevicesCharacteristics[device.Uuid][Guid.Parse(MiAuthCharacteristic)];

            //allow Notifications with the band
            authCharacteristic.EnableNotifications(true).Subscribe();

            //tell Miband about our with to authenticate
            var byteArr = new byte[] { 0x01, 0x8 }.Concat(MiBandSecret).ToArray();
            authCharacteristic.Write(byteArr).Subscribe(writeREsponse => {
                Debug.WriteLine("AUTH: wrote first Time");
            });

            //listen to the notifications
            authCharacteristic.WhenNotificationReceived().Subscribe(notification => {
                if (notification.Data == null)  {
                    //notification has no data, continue
                    return;
                }

                if (notification.Data.SequenceEqual(new byte[] { 16, 1, 1 })) {
                    //MIband has accept our first write attempt

                    //don't really know the meaning of the bytes we send
                    notification.Characteristic.Write(new byte[] { 0x02, 0x8 }).Subscribe(notiRes => {
                        //we wrote for the second time and the response should arrive in the method above
                        Debug.WriteLine("Wrote second time");
                    });
                }

                if (notification.Data.Take(3).SequenceEqual(new byte[] { 16, 2, 1 }))
                {
                    //MIband send second notification in the authentication process
                    var messageToEncrypt = notification.Data.Skip(3);


                    using (RijndaelManaged rijAlg = new RijndaelManaged())
                    {
                        rijAlg.Key = MiBandSecret;
                        rijAlg.Mode = CipherMode.ECB;
                        rijAlg.Padding = PaddingMode.None;

                        var encrypt = rijAlg.CreateEncryptor();
                        var encrypted = encrypt.TransformFinalBlock(messageToEncrypt.ToArray(), 0, 16);
                        var toSend = new byte[] { 0x03, 0x8 }.Concat(encrypted).ToArray();

                        notification.Characteristic.Write(toSend).Subscribe(x => {
                            Debug.WriteLine("Wrote last message");
                            SendSms(device);
                        });
                    }
                }
            });
        }

        /*
        private void AuthMiBand(IDevice device) {
            device.WhenAnyCharacteristicDiscovered().Subscribe(anyDis => {
                Debug.WriteLine("Characteristic UUid: " + anyDis.Uuid.ToString() + " Service UUid " + anyDis.Service.Uuid + " Service Descr: " + anyDis.Service.Description);

                if (anyDis.Uuid == Guid.Parse(MiAuthCharacteristic)) {
                    //if we found auth
                    if (!DevicesCharacteristics.ContainsKey(device.Uuid))
                    {
                        //if we have no Key for the current BT device
                        DevicesCharacteristics.Add(device.Uuid, new Dictionary<Guid, IGattCharacteristic>());
                    }
                    if (!DevicesCharacteristics[device.Uuid].ContainsKey(anyDis.Uuid))
                    {
                        //if we did not save auth Characteristic
                        DevicesCharacteristics[device.Uuid].Add(anyDis.Uuid, anyDis);
                        AuthMiBand2(device);
                    }    
                }

                if (anyDis.Uuid == Guid.Parse(MiAuthCharacteristic))
                {
                    
                }

                /*
                if (anyDis.Uuid == Guid.Parse(MiAuthCharacteristic)) {
                    anyDis.EnableNotifications(true);
                    var byteArr = new byte[] { 0x01, 0x8 }.Concat(MiBandSecret).ToArray();
                    anyDis.Write(byteArr).Subscribe(writeREsponse => {
                        Debug.WriteLine("wrote first Time");
                    });
                    anyDis.WhenNotificationReceived().Subscribe(noti => {
                        if (noti.Data != null) {
                            Debug.WriteLine("Got notification from miband with value: " + string.Concat(noti.Data));

                            if (noti.Data.SequenceEqual(new byte[] { 16, 1, 1 })) {
                                //first response 
                                noti.Characteristic.Write(new byte[] { 0x02, 0x8 }).Subscribe(notiRes => {
                                    Debug.WriteLine("Wrote second time");
                                });

                            }
                            if (noti.Data.Take(3).SequenceEqual(new byte[] { 16, 2, 1 })) {
                                //second response
                                var tmpValue = noti.Data.Skip(3);
                                using (RijndaelManaged rijAlg = new RijndaelManaged())
                                {
                                    rijAlg.Key = MiBandSecret;
                                    rijAlg.Mode = CipherMode.ECB;
                                    rijAlg.Padding = PaddingMode.None;

                                    var encrypt = rijAlg.CreateEncryptor();
                                    var encrypted = encrypt.TransformFinalBlock(tmpValue.ToArray(),0,16);
                                    var toSend = new byte[] { 0x03, 0x8 }.Concat(encrypted).ToArray();
                                    /*
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        using (CryptoStream cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                                        {
                                            using (StreamWriter sw = new StreamWriter(cs))
                                            {
                                                sw.Write(tmpValue);
                                            }
                                            var encrypted = ms.ToArray();
                                            var bytesToSend = new byte[] {0x03, 0x8}.Concat(encrypted).ToArray();

                                            noti.Characteristic.Write(bytesToSend).Subscribe(x => {
                                                Debug.WriteLine("Wrote last message");
                                            });
                                        }
                                    }

                                    noti.Characteristic.Write(toSend).Subscribe(x => {
                                        Debug.WriteLine("Wrote last message");
                                    });
                                }

                            }

                        }

                        Debug.WriteLine("noti Received");
                    });
                }


                
            });
            device.WhenHeartRateBpm().Subscribe(test => {
                Debug.WriteLine(test);
            });
        }
*/
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        void CharacteristicDebug(IGattCharacteristic knownChar) {
            knownChar.WhenDescriptorDiscovered().Subscribe(descr => {
                Debug.WriteLine(descr.Description);
            });
            if (knownChar.CanRead())
            {
                Debug.WriteLine("Can Read ");
                knownChar.Read().Subscribe(read =>
                {
                    Debug.WriteLine("Read data from " + read.Characteristic.Uuid);
                    Debug.WriteLine("Data read " + read.Data);
                });
            }
            if (knownChar.CanWrite())
            {
                Debug.WriteLine("Can Write ");
            }
            if (knownChar.CanNotify()) {
                Debug.WriteLine("Can Notify");
                knownChar.EnableNotifications().Subscribe(res => {
                    Debug.WriteLine(res);
                });
                knownChar.WhenNotificationReceived().Subscribe(noti => {
                    Debug.WriteLine("Received Notification from characteristic: " + noti.Characteristic.Uuid);
                    Debug.WriteLine("Data read " + noti.Data[0] + noti.Data[1]);
                });
                knownChar.RegisterAndNotify().Subscribe(noti => {
                    Debug.WriteLine("Received Notification from characteristic: " + noti.Characteristic.Uuid);
                    Debug.WriteLine("Data read " + noti.Data.ToString());
                });

            }
        }
    }
}
