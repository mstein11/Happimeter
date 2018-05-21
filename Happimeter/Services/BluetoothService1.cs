using System;
using System.Collections.Generic;
using Plugin.BluetoothLE;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;
using Happimeter.Core.Helper;
using System.Diagnostics;
using System.Threading.Tasks;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Interfaces;
using Happimeter.Core.Database;

namespace Happimeter.Services
{
	public class BluetoothService1 : BluetoothService, IBluetoothService1
	{
		private double _scanTimeoutSeconds = 30;
		private double _connectTimeoutSeconds = 120;
		private double _messageTimeoutSeconds = 10;

		public BluetoothService1()
		{
		}

		private bool _isInited = false;
		public async Task Init()
		{
			if (_isInited)
			{
				return;
			}
			var devices = CrossBleAdapter.Current.GetConnectedDevices();
			var device = devices.FirstOrDefault(x => x.Name.Contains("Happimeter"));
			if (device != null && device.Status == ConnectionStatus.Connected)
			{
				device.WhenStatusChanged().Subscribe(status => WhenConnectionStatusChanged(status, device));
			}
			else
			{
				if (!CrossBleAdapter.Current.IsScanning)
				{
					Console.WriteLine("Not connected, not scanning, starting scanning");
					var replaySubj = StartScan(UuidHelper.AndroidWatchServiceUuidString);
				}
				var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
				var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());
				//we skip 2 because the first two bytes are not relevant to us. the actual advertisement data start at position 3
				var scannedDevice = await ScanReplaySubject.Where(scanRes => scanRes?.AdvertisementData?.ServiceData?.FirstOrDefault()?.Skip(2)?.SequenceEqual(userIdBytes) ?? false)
													.Select(result => result.Device)
													.Take(1)
													.Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
													.Catch((Exception arg) =>
													{
														Console.WriteLine(arg.Message);
														return Observable.Return<IDevice>(null);
													})
													.DefaultIfEmpty();
				if (scannedDevice == null)
				{
					return;
				}
				await ConnectDevice(scannedDevice);

			}
			_isInited = true;
		}

		public ReplaySubject<IScanResult> ScanReplaySubject { get; private set; }
		public List<IScanResult> FoundDevices { get; private set; }
		public new IObservable<IScanResult> StartScan(string serviceGuid = null)
		{

			if (CrossBleAdapter.Current.IsScanning || CrossBleAdapter.Current.Status != AdapterStatus.PoweredOn)
			{
				if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
				{
					//reset the replaysubject
					ScanReplaySubject = new ReplaySubject<IScanResult>();
					ScanReplaySubject.OnCompleted();
				}
				return ScanReplaySubject;
			}

			IObservable<IScanResult> scannerObs;
			if (serviceGuid == null)
			{
				scannerObs = CrossBleAdapter.Current.Scan();
			}
			else
			{
				scannerObs = CrossBleAdapter.Current.Scan(new ScanConfig { ServiceUuids = new List<Guid> { Guid.Parse(serviceGuid) }, ScanType = BleScanType.LowLatency });
			}
			ScanReplaySubject = new ReplaySubject<IScanResult>();
			FoundDevices = new List<IScanResult>();

			var timerObs = Observable.Timer(TimeSpan.FromSeconds(_scanTimeoutSeconds));
			scannerObs.TakeUntil(timerObs).Subscribe(scan =>
			{
				if (!FoundDevices.Select(x => x.Device.Uuid).Contains(scan.Device.Uuid))
				{
					Console.WriteLine($"Found device. Name: {scan.Device.Name}, Uuid: {scan.Device.Uuid}, data: {System.Text.Encoding.UTF8.GetString(scan.AdvertisementData.ServiceData?.FirstOrDefault() ?? new byte[0])}");
					FoundDevices.Add(scan);
					ScanReplaySubject.OnNext(scan);
				}
			});
			timerObs.Subscribe((longi) =>
			{
				ScanReplaySubject.OnCompleted();
			});
			return ScanReplaySubject;
		}

		private ReplaySubject<IGattCharacteristic> CharacteristicsReplaySubject { get; set; } = new ReplaySubject<IGattCharacteristic>();
		private ReplaySubject<IDevice> ConnectedReplaySubject { get; set; }
		public new IObservable<object> ConnectDevice(IDevice device)
		{
			var connectionError = false;
			ConnectedReplaySubject = new ReplaySubject<IDevice>();
			device.Connect(new ConnectionConfig
			{
				AutoConnect = true,
				AndroidConnectionPriority = ConnectionPriority.High
			});


			var obs = device.WhenConnected();
			obs.Subscribe(success =>
			{
				device.WhenStatusChanged().Subscribe(status => WhenConnectionStatusChanged(status, device));

			});

			/*var obs = device.Connect(new GattConnectionConfig
			{
				AutoConnect = false,
				Priority = ConnectionPriority.High,
				IsPersistent = false
			})
				.Timeout(TimeSpan.FromSeconds(_connectTimeoutSeconds))
				.Catch((Exception arg) =>
				{
					connectionError = true;
					Console.WriteLine(arg.Message);
					return Observable.Return<object>(null);
				});
*/
			obs.Subscribe(success =>
			{
				if (connectionError)
				{
					ConnectedReplaySubject.OnError(new Exception("Connection was not successful"));
					return;
				}
				device.WhenStatusChanged().Subscribe(status => WhenConnectionStatusChanged(status, device));
				ConnectedReplaySubject.OnNext(device);
				ConnectedReplaySubject.OnCompleted();
			});
			return ConnectedReplaySubject;
		}

		public new void WhenConnectionStatusChanged(ConnectionStatus status, IDevice device)
		{
			Debug.WriteLine("Status changed: " + status);
			if (status == ConnectionStatus.Disconnected)
			{
				CharacteristicsReplaySubject.OnCompleted();
				CharacteristicsReplaySubject = new ReplaySubject<IGattCharacteristic>();
				device.Connect();
			}
			if (status == ConnectionStatus.Connected)
			{
				device.WhenAnyCharacteristicDiscovered().Subscribe(async characteristic =>
				{
					if (!UuidHelper.KnownCharacteristics().Contains(characteristic.Uuid))
					{
						System.Diagnostics.Debug.WriteLine("We don't know characteristic: " + characteristic.Uuid);
						return;
					}
					if (true || !await CharacteristicsReplaySubject.DefaultIfEmpty().Contains(characteristic))
					{
						if (characteristic.CanNotifyOrIndicate())
						{
							await EnableNotificationsFor(characteristic);
						}
						CharacteristicsReplaySubject.OnNext(characteristic);
					}
				});
			}
		}

		private Dictionary<Guid, IDisposable> NotificationSubscription = new Dictionary<Guid, IDisposable>();
		private ReplaySubject<(string, string)> NotificationSubject = new ReplaySubject<(string, string)>(TimeSpan.FromSeconds(2));
		public new async Task<bool> EnableNotificationsFor(IGattCharacteristic characteristic)
		{
			var res = await characteristic.EnableNotifications().Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds)).Catch((Exception arg) =>
			{
				Console.WriteLine(arg.Message);
				return Observable.Return<CharacteristicGattResult>(null);
			});
			if (res == null)
			{
				//todo:implement
				return false;
			}
			WriteReceiverContext context = null;

			//make sure we have only one WhenNotificationReceived by releasing old subscriptions
			if (NotificationSubscription.ContainsKey(characteristic.Uuid))
			{
				var oldSubscription = NotificationSubscription[characteristic.Uuid];
				oldSubscription.Dispose();
				NotificationSubscription.Remove(characteristic.Uuid);
			}

			var subscription = characteristic.WhenNotificationReceived().Subscribe(result =>
			{
				if (UuidHelper.KnownCharacteristics().All(x => result.Characteristic.Uuid != x))
				{
					return;
				}
				if (result.Data == null)
				{
					return;
				}
				if (context == null)
				{
					context = new WriteReceiverContext(result.Data);
					return;
				}
				if (!context.CanAddMessagePart(result.Data))
				{
					context = null;
					return;
				}
				context.AddMessagePart(result.Data);
				if (context.ReadComplete)
				{
					var json = context.GetMessageAsJson();
					var name = context.MessageName;
					context = null;
					NotificationSubject.OnNext((name, json));
				}
			});
			NotificationSubscription.Add(characteristic.Uuid, subscription);
			return true;
		}

		private Dictionary<Guid, bool> IsBusy = new Dictionary<Guid, bool>();
		public new async void ExchangeData()
		{
			var deviceInfoServic = ServiceLocator.Instance.Get<IDeviceInformationService>();
			await deviceInfoServic.RunCodeInBackgroundMode(async () =>
			{
				await Init();
				var charac = await CharacteristicsReplaySubject
					.Where(x => x.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
					.FirstOrDefaultAsync()
					.Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
					.Catch((Exception arg) =>
				{
					return Observable.Return<IGattCharacteristic>(null);
				});
				if (charac == null)
				{
					return;
				}
				Console.WriteLine("Characteristic discovered: " + charac.Uuid);
				CharacteristicDiscoveredForDataExchange(charac);
			}, "data_exchange_task");
		}

		private async void CharacteristicDiscoveredForDataExchange(IGattCharacteristic characteristic)
		{
			if (!IsBusy.ContainsKey(characteristic.Service.Device.Uuid))
			{
				IsBusy.Add(characteristic.Service.Device.Uuid, true);//check wheter we are locked
			}
			else
			{
				Console.WriteLine("We are still busy with an previous data exchnage, lets abort the current one.");
				return;
			}

			var success = await WriteAsync(characteristic, new DataExchangeFirstMessage());
			Console.WriteLine("wrote successfully");
			if (!success)
			{
				//writing was not successful
				IsBusy.Remove(characteristic.Service.Device.Uuid);
				return;
			}

			try
			{
				var stopWatch = new Stopwatch();
				stopWatch.Start();
				var toCompareWithWatchTime = DateTime.UtcNow;
				var result = await ReadAsync(characteristic, (read, total) =>
				{

				});

				if (result == null)
				{
					//we throw an exception and let the catch block handle it
					throw new ArgumentException("Error during read process");
				}
				stopWatch.Stop();
				Console.WriteLine($"Took {stopWatch.Elapsed.TotalSeconds} seconds to receive {result.Count()} bytes");

				//get the read data and save it to db
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataExchangeMessage>(result);
				if (data.CurrentTimeUtc != default(DateTime) && Math.Abs((toCompareWithWatchTime - data.CurrentTimeUtc).TotalHours) > TimeSpan.FromHours(1).TotalHours)
				{
					//adjust the times
					var diff = toCompareWithWatchTime - data.CurrentTimeUtc;
					data.SensorMeasurements.ForEach(x => x.Timestamp = x.Timestamp + diff);
					data.SurveyMeasurements.ForEach(x => x.Timestamp = x.Timestamp + diff);
				}
				await ServiceLocator.Instance.Get<IMeasurementService>().AddMeasurements(data);

				//update timestamp for last pairing
				var pairing = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
				pairing.LastDataSync = DateTime.UtcNow;
				ServiceLocator.Instance.Get<ISharedDatabaseContext>().Update(pairing);

				//inform watch that we stored his data. In turn it will delete the data on the watch.
				await WriteAsync(characteristic, new DataExchangeConfirmationMessage());
				//As last step we upload the data to the server 
				await ServiceLocator.Instance.Get<IHappimeterApiService>().UploadMood();
				await ServiceLocator.Instance.Get<IHappimeterApiService>().UploadSensor();

				Console.WriteLine("Succesfully finished data exchange");
				IsBusy.Remove(characteristic.Service.Device.Uuid);

				var eventData = new Dictionary<string, string> {
							{"durationSeconds", stopWatch.Elapsed.TotalSeconds.ToString()},
							{"bytesTransfered", result.Count().ToString()}
						};
				ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeEnd, eventData);
			}
			catch (Exception e)
			{

				Console.WriteLine($"Exception on Dataexchange after starting the exchange: {e.Message}");
				ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeFailure);
				IsBusy.Remove(characteristic.Service.Device.Uuid);
			}

		}
	}
}
