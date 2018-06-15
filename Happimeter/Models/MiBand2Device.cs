using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using Plugin.BluetoothLE;

namespace Happimeter.Models
{
	//https://stackoverflow.com/questions/41417747/connection-to-mi-band-2?answertab=votes#tab-top
	public class MiBand2Device : BluetoothDevice
	{

		public static readonly Guid MiAuthCharacteristic = Guid.Parse("00000009-0000-3512-2118-0009af100700");//maybe instead : 00000009-0000-3512-2118-0009af100700
		public static readonly Guid NotificationCharacteristic = Guid.Parse("00002a46-0000-1000-8000-00805f9b34fb");
		public static readonly Guid ButtonTouch = Guid.Parse("00000010-0000-3512-2118-0009af100700");

		public static readonly byte[] SmsNotificationByte = { 5, 1 };

		public static readonly byte[] MiBandSecret = { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45 };

		public Dictionary<Guid, IGattCharacteristic> Characteristics { get; set; }

		//private readonly ReplaySubject<bool> InitializedReplaySubject;

		public MiBand2Device(IDevice device) : base(device)
		{
			//InitializedReplaySubject = new ReplaySubject<bool>();
			Characteristics = new Dictionary<Guid, IGattCharacteristic>();
		}

		public override IObservable<object> Connect()
		{
			/*
            var connection = Device.Connect();

            connection.Subscribe(emptyObj => {

                Device.WhenAnyCharacteristicDiscovered().Subscribe(anyDis => {
                    Debug.WriteLine("Characteristic UUid: " + anyDis.Uuid.ToString() + " Service UUid " + anyDis.Service.Uuid + " Service Descr: " + anyDis.Service.Description);
                    if (anyDis.Uuid == MiAuthCharacteristic)
                    {
                        //if we found auth
                        if (!Characteristics.ContainsKey(anyDis.Uuid))
                        {
                            //if we did not save auth Characteristic
                            Characteristics.Add(anyDis.Uuid, anyDis);
                            AuthMiBand2();
                        }
                    }

                    if(!Characteristics.ContainsKey(anyDis.Uuid)) {
                        Characteristics.Add(anyDis.Uuid, anyDis);
                    }
                });
            });
            */
			return null;
		}

		public override IObservable<bool> WhenDeviceReady()
		{
			return InitializedReplaySubject.AsObservable();
		}

		public override void SendNotification()
		{
			var notificationCharacteristic = Characteristics[NotificationCharacteristic];

			var testString = System.Text.Encoding.ASCII.GetBytes("Test"); //shoud be 84, 101, 115, 116
			var moodString = System.Text.Encoding.ASCII.GetBytes("Mood?");

			notificationCharacteristic.Write(new byte[] { 5, 1 }.Concat(moodString).ToArray()).Subscribe(res =>
			{
				Debug.WriteLine("Send Sms");
			});
		}

		public void QueryUserMood()
		{
			var moodQuestion = System.Text.Encoding.ASCII.GetBytes("Mood?");
			var activationQuestion = System.Text.Encoding.ASCII.GetBytes("Activation?");

			Device.WhenAnyCharacteristicDiscovered()
				  .Where(charac => { return charac.Uuid == NotificationCharacteristic; })
				  .Subscribe(charac =>
				  {

					  charac.Write(SmsNotificationByte.Concat(moodQuestion).ToArray()).Subscribe();
					  Device.WhenAnyCharacteristicDiscovered()
							.Where(buttonCharac => { return charac.Uuid == ButtonTouch; })
							.Subscribe(buttonCharac =>
							{
								buttonCharac.EnableNotifications();
								buttonCharac.WhenNotificationReceived().Subscribe(noti =>
								{

								});
							});

				  });
		}

		private void AuthMiBand2()
		{
			var device = Device;

			var authCharacteristic = Characteristics[MiAuthCharacteristic];

			//allow Notifications with the band
			authCharacteristic.EnableNotifications(true).Subscribe();

			//tell Miband about our with to authenticate
			var byteArr = new byte[] { 0x01, 0x8 }.Concat(MiBandSecret).ToArray();
			authCharacteristic.Write(byteArr).Subscribe(writeREsponse =>
			{
				Debug.WriteLine("AUTH: wrote first Time");
			});

			//listen to the notifications
			authCharacteristic.WhenNotificationReceived().Subscribe(notification =>
			{
				if (notification.Data == null)
				{
					//notification has no data, continue
					return;
				}

				if (notification.Data.SequenceEqual(new byte[] { 16, 1, 1 }))
				{
					//MIband has accept our first write attempt

					//don't really know the meaning of the bytes we send
					notification.Characteristic.Write(new byte[] { 0x02, 0x8 }).Subscribe(notiRes =>
					{
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

						notification.Characteristic.Write(toSend).Subscribe(x =>
						{
							Debug.WriteLine("Wrote last message");
							//SendSms(device);
							InitializedReplaySubject.OnNext(true);
							//DONE!
						});
					}
				}
			});
		}
	}
}
