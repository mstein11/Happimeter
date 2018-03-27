using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Happimeter.Interfaces;
using System.Diagnostics;

namespace Happimeter.Droid.Services
{
    public class DeviceInformationService : IDeviceInformationService
    {
        public DeviceInformationService()
        {
        }

        public string GetPhoneOs()
        {
            return "Android";
        }

        public string GetDeviceName()
        {
            return Android.OS.Build.Model;
        }

        public async Task RunCodeInBackgroundMode(Func<Task> action, string name = "MyBackgroundTaskName")
        {
            var powerManager = (PowerManager)Application.Context.GetSystemService(Context.PowerService);
            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial,
                                                    name);
            wakeLock.Acquire();
            var taskEnded = false;

            await Task.Factory.StartNew(async () =>
            {
                Console.WriteLine($"Background task '{name}' started");
                await action();
                Console.WriteLine($"Background task '{name}' finished");
                wakeLock.Release();
                taskEnded = true;
            });

            await Task.Factory.StartNew(async () =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (!taskEnded)
                {
                    Console.WriteLine($"Background '{name}' task with wakelock still running ({stopwatch.Elapsed.TotalSeconds} seconds)");
                    await Task.Delay(1000);
                }
                stopwatch.Stop();
            });
        }
    }
}
