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
using Happimeter.Events;
using Plugin.Permissions;
using Xamarin.Forms;
using Plugin.Permissions.Abstractions;
using Happimeter.Core.Services;

namespace Happimeter.Services
{
    public class BluetoothService : IBluetoothService
    {
        private double _scanTimeoutSeconds = 30;
        private double _connectTimeoutSeconds = 120;
        private double _messageTimeoutSeconds = 10;

        public BluetoothService()
        {
        }

        private IObservable<AdapterStatus> WhenAdapterStatusChanged { get; set; }

        public async Task<bool> EnsureBluetoothAllowedAndroid()
        {
            if (ServiceLocator.Instance.Get<IDeviceInformationService>().IsIos())
            {
                return true;
            }
            var hasPermission = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            if (hasPermission != PermissionStatus.Granted)
            {
                var shouldShow = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location);
                if (shouldShow)
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                    {
                        var page = Application.Current.MainPage;

                        if (page != null)
                        {
                            await page.DisplayAlert("Location",
                                    "Please enable location tracking, which is required to pair with a bluetooth device",
                                    "Ok");
                            await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                        }
                    });
                    return false;
                }
                else
                {
                    CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    return false;
                }
            }
            return true;
        }

        private bool _isInited = false;
        private DateTime? _isInitingDate = null;
        private object _initLock = new object();
        public async Task Init(bool force = false)
        {
            Console.WriteLine("Starting to init BT-Service");
            var devices = await CrossBleAdapter.Current.GetConnectedDevices();
            var device = devices.FirstOrDefault(x => x.Name?.Contains("Happimeter") ?? false);

            //ensure we init only once!!!
            lock (_initLock)
            {
                if (!force && _isInited && device != null && device.IsConnected())
                {
                    Console.WriteLine("BT-Service already Inited");
                    return;
                }
                if (_isInitingDate != null && Math.Abs((_isInitingDate.Value - DateTime.UtcNow).TotalSeconds) < 15)
                {
                    Console.WriteLine("BT-Service is currently initializing in another thread - return");
                    return;
                }
                _isInitingDate = DateTime.UtcNow;
            }


            if (force || device != null && device.Status != ConnectionStatus.Connected)
            {
                ReleaseSubscriptions();
            }
            SubscribeToNotifications();
            CharacteristicsReplaySubject = new ReplaySubject<IGattCharacteristic>();

            if (WhenAdapterStatusChanged == null)
            {
                WhenAdapterStatusChanged = CrossBleAdapter.Current.WhenStatusChanged();
                var isFirst = true;
                WhenAdapterStatusChanged.Subscribe(async status =>
                {
                    if (status != AdapterStatus.PoweredOn)
                    {
                        ReleaseSubscriptions();
                    }
                    else
                    {
                        if (!isFirst)
                        {
                            await Init();
                        }
                        isFirst = false;

                    }
                });
            }

            //sometimes we get stuck in the connecting status - we need to disconnect then
            if (device != null && device.Status == ConnectionStatus.Connecting)
            {
                Console.WriteLine("We have a device in Connecting Status, so we cancel the connection!");
                device.CancelConnection();
            }
            //we are already connected, so we just hook up the events
            if (device != null && device.Status == ConnectionStatus.Connected)
            {
                Console.WriteLine("We have a device that is already connected!");
                //todo: give reference to subscription
                if (WhenStatusChangedSubscription != null)
                {
                    WhenStatusChangedSubscription.Dispose();
                }
                Debug.WriteLine("Subscribing to Status Changes!");
                WhenStatusChangedSubscription = device.WhenStatusChanged().Subscribe(status => WhenConnectionStatusChanged(status, device));
            }
            else
            {
                Console.WriteLine("We need to scan for the device!");
                //we need to find our device first! 
                if (!CrossBleAdapter.Current.IsScanning || ScanReplaySubject == null)
                {
                    if (CrossBleAdapter.Current.IsScanning)
                    {
                        Console.WriteLine("Adapted is scanning, lets stop!");
                        CrossBleAdapter.Current.StopScan();
                    }
                    Console.WriteLine("Not connected, not scanning, starting scanning");
                    //do not await here!
                    StartScan(UuidHelper.AndroidWatchServiceUuidString);
                }

                var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
                var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());
                //we skip 2 because the first two bytes are not relevant to us. the actual advertisement data start at position 3
                var scannedDevice = await ScanReplaySubject.Where(scanRes => scanRes?.AdvertisementData?.ServiceData?.FirstOrDefault()?.Skip(2)?.SequenceEqual(userIdBytes) ?? false)
                                                    .Select(result => result.Device)
                                                    .Take(1)
                                                    .Timeout(TimeSpan.FromSeconds(_scanTimeoutSeconds))
                                                    .Catch((Exception arg) =>
                                                    {
                                                        Console.WriteLine(arg.Message);
                                                        return Observable.Return<IDevice>(null);
                                                    })
                                                    .DefaultIfEmpty();
                if (scannedDevice == null)
                {
                    Console.WriteLine("Could not find the device! We stop the initiation process!");
                    _isInitingDate = null;
                    return;
                }
                Console.WriteLine("Found the correct happimeter device - now trying to connect!");
                //todo: maybe introduce timeout
                await ConnectDevice(scannedDevice);

            }
            _isInited = true;
            _isInitingDate = null;
            Console.WriteLine("finished BT-Service Init");
        }

        public async void UnpairConnection()
        {
            //release first, so that we don't reconnect in WhenConnectionStateChanged Observable.
            ReleaseSubscriptions();
            var devices = await CrossBleAdapter.Current.GetConnectedDevices();
            var device = devices.FirstOrDefault(x => x.Name?.Contains("Happimeter") ?? false);
            if (device != null)
            {
                device.CancelConnection();
            }
        }

        public ReplaySubject<IScanResult> ScanReplaySubject { get; private set; }
        public List<IScanResult> FoundDevices { get; private set; }
        public IObservable<IScanResult> StartScan(string serviceGuid)
        {
            return StartScan(new List<Guid> { Guid.Parse(serviceGuid) });
        }

        public IObservable<IScanResult> StartScan(List<Guid> serviceGuids = null)
        {
            var bluetoothAllowed = EnsureBluetoothAllowedAndroid().Result;
            if (CrossBleAdapter.Current.IsScanning || CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff || !bluetoothAllowed)
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff || !bluetoothAllowed || ScanReplaySubject == null)
                {
                    Console.WriteLine("Not scanning because adapter is powered off!");
                    //reset the replaysubject
                    ScanReplaySubject = new ReplaySubject<IScanResult>();
                    ScanReplaySubject.OnCompleted();
                }
                return ScanReplaySubject;
            }

            IObservable<IScanResult> scannerObs;
            if (serviceGuids == null)
            {
                scannerObs = CrossBleAdapter.Current.Scan();
            }
            else
            {
                scannerObs = CrossBleAdapter.Current.Scan(new ScanConfig { ServiceUuids = serviceGuids, ScanType = BleScanType.LowLatency });
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

        public ReplaySubject<IGattCharacteristic> CharacteristicsReplaySubject { get; set; } = new ReplaySubject<IGattCharacteristic>();
        private ReplaySubject<IDevice> ConnectedReplaySubject { get; set; }
        private IDisposable WhenStatusChangedSubscription { get; set; }
        private IDisposable WhenConnectedSubscription { get; set; }
        public IObservable<object> ConnectDevice(IDevice device)
        {
            var connectionError = false;
            ConnectedReplaySubject = new ReplaySubject<IDevice>();
            try
            {
                device.Connect(new ConnectionConfig
                {
                    AutoConnect = false,
                    AndroidConnectionPriority = ConnectionPriority.High
                });
                Console.WriteLine("ConnectDevice: after device.Connect()");

                var obs = device.WhenConnected();
                if (WhenConnectedSubscription != null)
                {
                    WhenConnectedSubscription.Dispose();
                }
                WhenConnectedSubscription = obs.Subscribe(success =>
                {
                    Debug.WriteLine("Inside OnConnected!");
                    if (connectionError)
                    {
                        ConnectedReplaySubject.OnError(new Exception("Connection was not successful"));
                        return;
                    }
                    if (WhenStatusChangedSubscription != null)
                    {
                        WhenStatusChangedSubscription.Dispose();
                    }
                    Debug.WriteLine("Subscribing to Status Changes!");
                    WhenStatusChangedSubscription = device.WhenStatusChanged().Subscribe(status => WhenConnectionStatusChanged(status, device));


                    ConnectedReplaySubject.OnNext(device);
                    ConnectedReplaySubject.OnCompleted();
                });
                Console.WriteLine("ConnectDevice: after subscription()");
                return ConnectedReplaySubject;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while connection: " + e.Message);
                ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
                ConnectedReplaySubject.OnError(e);
                return ConnectedReplaySubject;
            }
        }

        private IDisposable WhenCharacteristicDiscoveredSubscription { get; set; }
        public void WhenConnectionStatusChanged(ConnectionStatus status, IDevice device)
        {
            Debug.WriteLine("Status changed: " + status);
            if (status == ConnectionStatus.Disconnected)
            {
                CharacteristicsReplaySubject.OnCompleted();
                WhenCharacteristicDiscoveredSubscription?.Dispose();
                WhenCharacteristicDiscoveredSubscription = null;
                CharacteristicsReplaySubject.Dispose();
                CharacteristicsReplaySubject = new ReplaySubject<IGattCharacteristic>();
                //device.Connect();
            }
            if (status == ConnectionStatus.Connected)
            {
                WhenCharacteristicDiscoveredSubscription = device.WhenAnyCharacteristicDiscovered().Subscribe(async characteristic =>
                {
                    if (!UuidHelper.KnownCharacteristics().Contains(characteristic.Uuid))
                    {
                        System.Diagnostics.Debug.WriteLine("We don't know characteristic: " + characteristic.Uuid);
                        return;
                    }
                    System.Diagnostics.Debug.WriteLine("Found characteristic: " + characteristic.Uuid);
                    if (characteristic.CanNotifyOrIndicate())
                    {
                        await EnableNotificationsFor(characteristic);
                    }
                    CharacteristicsReplaySubject.OnNext(characteristic);
                });
                var timer = Observable.Timer(TimeSpan.FromSeconds(1));
                var tmp = timer.Merge(CharacteristicsReplaySubject.Select(x => (long)-1));
                tmp.Take(1).Subscribe(x =>
                {
                    if (x != -1)
                    {
                        Console.WriteLine("We could not find the characteristics we need!");
                        ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.CouldNotUploadSensorOldFormat);
                        Plugin.LocalNotifications.CrossLocalNotifications.Current.Show("Bluetooth Problem", "Please restart Bluetooth on your Watch!");
                    }
                });

            }
        }

        private IList<IDisposable> NotificationSubscriptions = new List<IDisposable>();
        private void SubscribeToNotifications()
        {
            foreach (var sub in NotificationSubscriptions)
            {
                sub.Dispose();
            }
            NotificationSubscriptions = new List<IDisposable>();

            NotificationSubscriptions.Add(NotificationSubject.Where(x => x.Item1 == DataExchangeInitMessage.MessageNameConstant).Subscribe(x =>
            {
                Debug.WriteLine("Got DataExchange Notification");
                ExchangeData();
            }));

            NotificationSubscriptions.Add(NotificationSubject.Where(x => x.Item1 == PreSurveyFirstMessage.MessageNameConstant).Subscribe(async res =>
            {
                try
                {
                    Debug.WriteLine("Got PreSurveyMessage");
                    ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeReceivedNotification);
                    var predictionService = ServiceLocator.Instance.Get<IPredictionService>();
                    var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();

                    var downloadPredictionsTask = predictionService.DownloadAndSavePrediction();
                    var downloadQuestionsTask = measurementService.DownloadAndSaveGenericQuestions();
                    var httpDone = Task.WhenAll(downloadQuestionsTask, downloadPredictionsTask);
                    var timeout = Task.Delay(5000);
                    if (await Task.WhenAny(httpDone, timeout) == timeout)
                    {
                        Debug.WriteLine("couldn't complete httpRequests");
                    }

                    var questions = measurementService.GetGenericQuestions();
                    var predictions = predictionService.GetLastPrediction();
                    var activation = predictions.FirstOrDefault(x => x.QuestionId == 1);
                    var pleasance = predictions.FirstOrDefault(x => x.QuestionId == 2);

                    var message = new PreSurveySecondMessage();
                    message.PredictedActivation = activation.PredictedValue;
                    message.PredictedPleasance = pleasance.PredictedValue;
                    message.PredictionFrom = pleasance.Timestamp;
                    message.Questions = questions.ToList();


                    var charac = await CharacteristicsReplaySubject
                        .Where(x => x.Uuid == UuidHelper.PreSurveyDataCharacteristicUuid)
                        .FirstOrDefaultAsync()
                        .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                        .Catch((Exception arg) =>
                        {
                            return Observable.Return<IGattCharacteristic>(null);
                        });
                    await WriteAsync(charac, message);
                }
                catch (Exception e)
                {
                    ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
                }
            }));
        }

        private Dictionary<Guid, IDisposable> NotificationSubscription = new Dictionary<Guid, IDisposable>();
        public ReplaySubject<(string, string)> NotificationSubject { get; set; } = new ReplaySubject<(string, string)>(TimeSpan.FromSeconds(2));
        public async Task<bool> EnableNotificationsFor(IGattCharacteristic characteristic)
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
            Debug.WriteLine("Enabled Notifications for:" + characteristic.Uuid);
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
                Debug.WriteLine("Got Notification!");
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
                    Debug.WriteLine($"Got Notification with name: {name}!");
                }
            });
            NotificationSubscription.Add(characteristic.Uuid, subscription);
            return true;
        }

        public void ReleaseSubscriptions()
        {
            Console.WriteLine("Relesign all subscriptions.");
            foreach (var key in NotificationSubscription.Keys.ToList())
            {
                NotificationSubscription[key].Dispose();
                NotificationSubscription.Remove(key);
            }
            WhenCharacteristicDiscoveredSubscription?.Dispose();
            WhenCharacteristicDiscoveredSubscription = null;
            WhenConnectedSubscription?.Dispose();
            WhenConnectedSubscription = null;
            WhenStatusChangedSubscription?.Dispose();
            WhenStatusChangedSubscription = null;
            CharacteristicsReplaySubject?.Dispose();
            CharacteristicsReplaySubject = new ReplaySubject<IGattCharacteristic>();
            NotificationSubject?.Dispose();
            NotificationSubject = new ReplaySubject<(string, string)>();
            _isInited = false;
        }

        public async Task SendGenericQuestions(Action<BluetoothWriteEvent> statusUpdate = null)
        {
            statusUpdate?.Invoke(BluetoothWriteEvent.Initialized);
            await Init();
            var charac = await CharacteristicsReplaySubject
                .Where(x => x.Uuid == UuidHelper.GenericQuestionCharacteristicUuid)
                .FirstOrDefaultAsync()
                .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                .Catch((Exception arg) =>
                {
                    return Observable.Return<IGattCharacteristic>(null);
                });
            statusUpdate?.Invoke(BluetoothWriteEvent.Connected);
            if (charac == null)
            {
                //we did not find the characteristic within the give timeframe
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }

            var genericQuestions = ServiceLocator.Instance.Get<IMeasurementService>().GetActiveGenericQuestions();
            var genericQuestionMessage = new GenericQuestionMessage
            {
                Questions = genericQuestions.ToList()
            };

            var result = await WriteAsync(charac, genericQuestionMessage);
            if (result)
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.Complete);
            }
            else
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnWrite);
            }
        }

        public async Task SendMeasurementMode(int modeId, Action<BluetoothWriteEvent> statusUpdate = null)
        {
            statusUpdate?.Invoke(BluetoothWriteEvent.Initialized);
            await Init();
            var charac = await CharacteristicsReplaySubject
                .Where(x => x.Uuid == UuidHelper.MeasurementModeCharacteristicUuid)
                .FirstOrDefaultAsync()
                .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                .Catch((Exception arg) =>
                {
                    return Observable.Return<IGattCharacteristic>(null);
                });
            statusUpdate?.Invoke(BluetoothWriteEvent.Connected);
            if (charac == null)
            {
                //we did not find the characteristic within the give timeframe
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }

            if (charac == null)
            {
                //we did not find the characteristic within the give timeframe
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }

            var message = new SwitchMeasurementModeMessage(modeId);

            var result = await WriteAsync(charac, message);
            if (result)
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.Complete);
            }
            else
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnWrite);
            }
        }

        private Dictionary<Guid, bool> IsBusy = new Dictionary<Guid, bool>();
        public async void ExchangeData()
        {
            DataExchangeStatusUpdate?.Invoke(this,
                     new AndroidWatchExchangeDataEventArgs
                     {
                         EventType = AndroidWatchExchangeDataStates.SearchingForDevice
                     });
            Console.WriteLine("Exchange Data started.");
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
                    DataExchangeStatusUpdate?.Invoke(this,
                     new AndroidWatchExchangeDataEventArgs
                     {
                         EventType = AndroidWatchExchangeDataStates.DeviceNotFound
                     });
                    Console.WriteLine("We couldn't find our characteristic! We have to reinit");
                    Debug.WriteLine("WATCH DOES NOT HAVE CHARACTERISTICS!");
                    //todo: if we don't find the charac, we have to reinit somehow
                    ReleaseSubscriptions();
                    return;
                }
                Console.WriteLine("Characteristic discovered: " + charac.Uuid);
                await CharacteristicDiscoveredForDataExchange(charac);
            }, "data_exchange_task");
        }
        public event EventHandler<AndroidWatchExchangeDataEventArgs> DataExchangeStatusUpdate;
        private async Task CharacteristicDiscoveredForDataExchange(IGattCharacteristic characteristic)
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
                DataExchangeStatusUpdate?.Invoke(this,
                     new AndroidWatchExchangeDataEventArgs
                     {
                         EventType = AndroidWatchExchangeDataStates.ErrorOnExchange
                     });
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
                    DataExchangeStatusUpdate?.Invoke(this,
                             new AndroidWatchExchangeDataEventArgs
                             {
                                 EventType = AndroidWatchExchangeDataStates.ReadUpdate,
                                 BytesRead = read,
                                 TotalBytes = total
                             });
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
                DataExchangeStatusUpdate?.Invoke(this,
                        new AndroidWatchExchangeDataEventArgs
                        {
                            EventType = AndroidWatchExchangeDataStates.Complete,
                        });
                var eventData = new Dictionary<string, string> {
                            {"durationSeconds", stopWatch.Elapsed.TotalSeconds.ToString()},
                            {"bytesTransfered", result.Count().ToString()}
                        };
                ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeEnd, eventData);
            }
            catch (Exception e)
            {
                DataExchangeStatusUpdate?.Invoke(this,
                        new AndroidWatchExchangeDataEventArgs
                        {
                            EventType = AndroidWatchExchangeDataStates.ErrorOnExchange,
                        });
                Console.WriteLine($"Exception on Dataexchange after starting the exchange: {e.Message}");
                ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeFailure);
                ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
                IsBusy.Remove(characteristic.Service.Device.Uuid);
            }
        }

        /// <summary>
        ///     We return false if there is an error during the write process.
        ///     We return true if the write process succeds.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="characteristic">Characteristic.</param>
        /// <param name="message">Message.</param>
        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, BaseBluetoothMessage message)
        {
            //send reset bytes, to ensure, that the other side knows we are starting a new connection
            //Otherwise the other side wouldn't know, that the next message is the header.
            var nullByteSeq = new byte[3] { 0x00, 0x00, 0x00 };
            var reseted = await characteristic.Write(nullByteSeq)
                .Timeout(TimeSpan.FromSeconds(5))
                                              .Catch<CharacteristicGattResult, Exception>((arg) =>
                                              {
                                                  if (arg is TimeoutException)
                                                  {
                                                      Console.WriteLine("Writing took longer than 5 seconds. Abort!");
                                                  }
                                                  else
                                                  {
                                                      Console.WriteLine("Got error while writing reset!");
                                                  }
                                                  return Observable.Return<CharacteristicGattResult>(null);
                                              });
            if (reseted == null)
            {
                return false;
            }

            //creating and sending header.
            var messageJson = BluetoothHelper.GetMessageJson(message);
            var header = BluetoothHelper.GetMessageHeader(message);
            var written = await characteristic.Write(header)
                .Timeout(TimeSpan.FromSeconds(5))
                                              .Catch<CharacteristicGattResult, Exception>((arg) =>
                                              {
                                                  if (arg is TimeoutException)
                                                  {
                                                      Console.WriteLine("Writing took longer than 5 seconds. Abort!");
                                                  }
                                                  else
                                                  {
                                                      Console.WriteLine("Got error on write, while writing header");
                                                  }
                                                  return Observable.Return<CharacteristicGattResult>(null);
                                              });
            if (written == null)
            {
                return false;
            };

            //negotiating higher mtu would increase the transfer speed, however it makes it very instable at the moment we stick with 20 bytes which is the default
            var mtu = 20;

            var bytesSentCounter = 0;
            while (bytesSentCounter < messageJson.Count())
            {
                var toSend = messageJson.Skip(bytesSentCounter).Take(mtu).ToArray();
                var sent = await characteristic.Write(toSend)
                   .Timeout(TimeSpan.FromSeconds(8))
                                               .Catch<CharacteristicGattResult, Exception>((arg) =>
                                               {
                                                   if (arg is TimeoutException)
                                                   {
                                                       Console.WriteLine("Writing took longer than 8 seconds. Abort!");
                                                   }
                                                   else
                                                   {
                                                       Console.WriteLine("Got error on write, while transfering data");
                                                   }
                                                   return Observable.Return<CharacteristicGattResult>(null);
                                               });
                if (sent == null)
                {
                    return false;
                };

                bytesSentCounter += toSend.Count();
            }
            return true;
        }

        /// <summary>
        ///     In case there is something wrong with the read, we return null.
        ///     If the read succeeds, string is the json representation of the received data.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="characteristic">Characteristic.</param>
        /// <param name="statusUpdateAction">Status update action.</param>
        public async Task<string> ReadAsync(IGattCharacteristic characteristic, Action<int, int> statusUpdateAction = null)
        {

            //from the first read request we assume to get an header, which contains information what and how much is sent
            var headerResult = await characteristic.Read()
                    .Timeout(TimeSpan.FromSeconds(10))
                                                   .Catch<CharacteristicGattResult, Exception>((arg) =>
                                                   {
                                                       if (arg is TimeoutException)
                                                       {
                                                           Console.WriteLine("Reading took longer than 5 seconds. Abort!");
                                                       }
                                                       else
                                                       {
                                                           Console.WriteLine("Got error while reading header!");
                                                       }
                                                       return Observable.Return<CharacteristicGattResult>(null);
                                                   });
            if (headerResult == null)
            {
                return null;
            }
            var context = new WriteReceiverContext(headerResult.Data);

            while (!context.ReadComplete)
            {
                //here we receive the actual data until we got the complete message
                var nextBytes = await characteristic.Read()
                    .Timeout(TimeSpan.FromSeconds(10))
                                                    .Catch<CharacteristicGattResult, Exception>((arg) =>
                                                    {
                                                        if (arg is TimeoutException)
                                                        {
                                                            Console.WriteLine("Reading took longer than 10 seconds. Abort!");
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Got error on reading data!");
                                                        }
                                                        return Observable.Return<CharacteristicGattResult>(null);
                                                    });
                if (nextBytes == null || nextBytes.Data.Length == 3 && nextBytes.Data.All(x => x == 0x00))
                {
                    //if next bytes are null, the read process throw an exception
                    //if we receive three zero bytes. the other side got an error
                    return null;
                }

                context.AddMessagePart(nextBytes.Data);

                if (statusUpdateAction != null)
                {
                    statusUpdateAction(context.Cursor, context.MessageSize);
                }
            }
            var returnJson = context.GetMessageAsJson();
            return returnJson;
        }
    }
}
