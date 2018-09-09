using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Java.Util;
using Android.Bluetooth.LE;
using Android.Runtime;
using Android.OS;
using Happimeter.Core.Helper;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.AppCenter.Crashes;
using Happimeter.Watch.Droid.Services;

namespace Happimeter.Watch.Droid.Workers
{
    public class BluetoothScannerWorker : AbstractWorker
    {
        public BluetoothScannerWorker()
        {
        }

        /// <summary>
        ///     UserId, rssi value
        /// </summary>
        public static ConcurrentBag<(int, int)> ProximityMeasures = new ConcurrentBag<(int, int)>();

        private static BluetoothScannerWorker Instance { get; set; }
        private BeaconListenerService BeaconListenerService { get; set; }
        private BluetoothLeScanner BluetoothLeScanner { get; set; }
        private CallBack BluetoothScanCallback { get; set; }

        public static BluetoothScannerWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BluetoothScannerWorker();
            }

            return Instance;
        }

        public void Start()
        {
            try
            {
                BeaconListenerService = new BeaconListenerService();
                BeaconListenerService.StartListeningForBeacons();
                IsRunning = true;
                var bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
                var BluetoothAdapter = bluetoothManager.Adapter;
                BluetoothLeScanner = bluetoothManager.Adapter.BluetoothLeScanner;
                if (BluetoothLeScanner == null)
                {
                    throw new Exception("BluetoothLeScanner is null");
                }
                BluetoothScanCallback = new CallBack();
                var scanFilterBuilder = new ScanFilter.Builder();
                scanFilterBuilder.SetServiceUuid(ParcelUuid.FromString(UuidHelper.AndroidWatchServiceUuidString));
                var scanFilter = scanFilterBuilder.Build();
                var settingsBuilder = new ScanSettings.Builder();
                settingsBuilder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);
                //settingsBuilder.SetMatchMode(BluetoothScanMatchMode.Aggressive);
                var settings = settingsBuilder.Build();
                BluetoothLeScanner.StartScan(new List<ScanFilter> { scanFilter }, settings, BluetoothScanCallback);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.WriteLine("There was an error during BT scanning: " + e.Message);
                IsRunning = false;
            }
        }

        public void Stop()
        {
            System.Diagnostics.Debug.WriteLine("BeaconScannerWorker - STOP");
            IsRunning = false;
            BeaconListenerService?.StopListeningForBeacons();
            BluetoothLeScanner?.StopScan(BluetoothScanCallback);
        }
    }

    public class CallBack : ScanCallback
    {
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            System.Diagnostics.Debug.WriteLine(result.ScanRecord.DeviceName);
            if (!result.ScanRecord?.ServiceUuids?.Select(x => x.Uuid.ToString().ToLower()).Any(x => x == UuidHelper.AndroidWatchServiceUuidString.ToLower()) ?? true)
            {
                base.OnScanResult(callbackType, result);
                return;
            }
            var userId = Encoding.UTF8.GetString(result.ScanRecord.ServiceData.FirstOrDefault().Value);
            int userIdInt;
            if (int.TryParse(userId, out userIdInt))
            {
                BluetoothScannerWorker.ProximityMeasures.Add((userIdInt, result.Rssi));
            }
            base.OnScanResult(callbackType, result);
        }
    }
}

