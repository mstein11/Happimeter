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

        public static BluetoothScannerWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BluetoothScannerWorker();
            }

            return Instance;
        }

        public async void StartFor(int seconds)
        {
            IsRunning = true;
            var bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
            var BluetoothAdapter = bluetoothManager.Adapter;
            var bluetoothLeScanner = bluetoothManager.Adapter.BluetoothLeScanner;
            var callBack = new CallBack();
            var scanFilterBuilder = new ScanFilter.Builder();
            scanFilterBuilder.SetServiceUuid(ParcelUuid.FromString(UuidHelper.AndroidWatchServiceUuidString));
            var scanFilter = scanFilterBuilder.Build();
            var settingsBuilder = new ScanSettings.Builder();
            settingsBuilder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower);
            var settings = settingsBuilder.Build();
            bluetoothLeScanner.StartScan(new List<ScanFilter> { scanFilter }, settings, callBack);
            await Task.Delay(seconds * 1000);
            bluetoothLeScanner.StopScan(callBack);
        }

        public async void Start()
        {
            IsRunning = true;
            var bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
            var BluetoothAdapter = bluetoothManager.Adapter;
            var bluetoothLeScanner = bluetoothManager.Adapter.BluetoothLeScanner;
            while (IsRunning)
            {

                var callBack = new CallBack();
                var scanFilterBuilder = new ScanFilter.Builder();
                scanFilterBuilder.SetServiceUuid(ParcelUuid.FromString(UuidHelper.AndroidWatchServiceUuidString));
                var scanFilter = scanFilterBuilder.Build();
                var settingsBuilder = new ScanSettings.Builder();
                settingsBuilder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower);
                var settings = settingsBuilder.Build();
                bluetoothLeScanner.StartScan(new List<ScanFilter> { scanFilter }, settings, callBack);
                await Task.Delay(30 * 1000);
                bluetoothLeScanner.StopScan(callBack);
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }

    public class CallBack : ScanCallback
    {
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
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

