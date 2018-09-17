using System;
using Plugin.FirebasePushNotification;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Happimeter.Core.Database;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Happimeter.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDeviceInformationService _deviceInformationService;
        private readonly ILoggingService _loggingService;
        private readonly ISharedDatabaseContext _sharedDatabaseContext;
        private readonly IBluetoothService _bluetoothService;
        private readonly IHappimeterApiService _apiService;
        private readonly IConfigService _configService;
        public NotificationService()
        {
            _deviceInformationService = ServiceLocator.Instance.Get<IDeviceInformationService>();
            _loggingService = ServiceLocator.Instance.Get<ILoggingService>();
            _sharedDatabaseContext = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            _bluetoothService = ServiceLocator.Instance.Get<IBluetoothService>();
            _apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
            _configService = ServiceLocator.Instance.Get<IConfigService>();
        }

        public const string NotificationChannelAllDevices = "allDevices";

        public void SubscibeToChannel(string channel)
        {
            CrossFirebasePushNotification.Current.Subscribe(channel);
        }

        public void SetupNotificationHooks()
        {
            CrossFirebasePushNotification.Current.OnTokenRefresh += async (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN : {p.Token}");
                var newToken = p.Token;
                var oldToken = _configService.GetConfigValueByKey(p.Token);
                if (newToken == oldToken)
                {
                    return;
                }
                _configService.AddOrUpdateConfigEntry(ConfigService.NotificationDeviceToken, p.Token);
                await UploadDeviceToken(p.Token);
            };

            CrossFirebasePushNotification.Current.OnNotificationReceived += async (s, p) =>
            {
                if (!p.Data.ContainsKey("AskMood")
                    || (string)p.Data["AskMood"] != "1")
                {
                    return;
                }
                await _deviceInformationService.RunCodeInBackgroundMode(async () =>
                {
                    _loggingService.LogEvent(LoggingService.ReceivedAskMoodNotification);
                    var pairing = _sharedDatabaseContext
                        .Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
                    if (pairing != null)
                    {
                        await _bluetoothService.SendAskForMood();
                    }
                }, "AskMoodNotificationTask");
            };
            CrossFirebasePushNotification.Current.OnNotificationError += (source, e) =>
            {
                _loggingService.LogEvent(LoggingService.OnNotificationError);
            };
        }

        public async Task UploadDeviceToken(string token)
        {
            await _apiService.UpdateNotificationDeviceToken(token);
        }
    }
}
