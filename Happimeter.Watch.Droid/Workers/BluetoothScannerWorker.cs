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
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting Scan in Battery Safer Mode");
                IsRunning = true;
                var bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
                var BluetoothAdapter = bluetoothManager.Adapter;
                var bluetoothLeScanner = bluetoothManager.Adapter.BluetoothLeScanner;
                if (bluetoothLeScanner == null)
                {
                    return;
                }
                var callBack = new CallBack();
                var scanFilterBuilder = new ScanFilter.Builder();
                scanFilterBuilder.SetServiceUuid(ParcelUuid.FromString(UuidHelper.AndroidWatchServiceUuidString));
                var scanFilter = scanFilterBuilder.Build();
                var settingsBuilder = new ScanSettings.Builder();
                settingsBuilder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);
                //settingsBuilder.SetMatchMode(BluetoothScanMatchMode.Aggressive);
                var settings = settingsBuilder.Build();
                bluetoothLeScanner.StartScan(new List<ScanFilter> { scanFilter }, settings, callBack);
                await Task.Delay(seconds * 1000);
                bluetoothLeScanner.StopScan(callBack);
                IsRunning = false;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.WriteLine("There was an error during BT scanning: " + e.Message);
                IsRunning = false;
            }
        }

        public async void Start()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting Scan in Life Mode");
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
                    settingsBuilder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);
                    //settingsBuilder.SetMatchMode(BluetoothScanMatchMode.Aggressive);
                    var settings = settingsBuilder.Build();
                    bluetoothLeScanner.StartScan(new List<ScanFilter> { scanFilter }, settings, callBack);
                    await Task.Delay(30 * 1000);
                    bluetoothLeScanner.StopScan(callBack);
                    await Task.Delay(1 * 1000);
                }
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
            IsRunning = false;
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

