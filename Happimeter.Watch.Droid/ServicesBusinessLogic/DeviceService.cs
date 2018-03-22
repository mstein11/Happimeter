using System;
using Android.App;
using Android.Content;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Core.Services;
using Happimeter.Watch.Droid.Activities;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public class DeviceService : IDeviceService
    {
        public DeviceService()
        {
        }

        public string GetDeviceName() {
            var configService = ServiceLocator.Instance.Get<IConfigService>();
            var name = configService.GetConfigValueByKey(ConfigService.WatchNameKey);
            if (name == null) {
                name = "Happimeter " + BluetoothHelper.GetBluetoothName();
                configService.AddOrUpdateConfigEntry(ConfigService.WatchNameKey, name);
            }
            return name;
        }

        public void AddPairing(AuthSecondMessage message) {
            var pairedDevice = new BluetoothPairing
            {
                PhoneOs = message.PhoneOs,
                Password = message.Password,
                LastDataSync = null,
                IsPairingActive = true,
                PairedAt = DateTime.UtcNow,
                //PairedDeviceName = address,
                PairedWithUserName = message.HappimeterUsername,
                PairedWithUserId = message.HappimeterUserId
            };

            ServiceLocator.Instance.Get<IDatabaseContext>().Add(pairedDevice);
            //start beacon
            BeaconWorker.GetInstance().Start();
            BluetoothWorker.GetInstance().RemoveAuthService();
        }

        public void RemovePairing() {
            ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAll<BluetoothPairing>();
            BeaconWorker.GetInstance().Stop();
            BluetoothWorker.GetInstance().AddAuthService();
        }

        public bool IsPaired() {
            return ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive) != null;
        }

        public void NavigateToPairingRequestPage(string deviceName) {
            var intent = new Intent(Application.Context.ApplicationContext, typeof(PairingRequestActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            intent.PutExtra("DeviceName", deviceName);
            Application.Context.StartActivity(intent);
        }
    }
}
