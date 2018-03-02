using System;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Core.Services;
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
        }

        public void RemovePairing() {
            ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAll<BluetoothPairing>();
            BeaconWorker.GetInstance().Stop();
        }
    }
}
