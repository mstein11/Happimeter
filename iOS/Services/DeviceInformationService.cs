using System;
using System.Threading.Tasks;
using Happimeter.Interfaces;
using UIKit;

namespace Happimeter.iOS.Services
{
    public class DeviceInformationService : IDeviceInformationService
    {
        public DeviceInformationService()
        {
        }

        public string GetPhoneOs()
        {
            return "iOS";
        }

        public string GetDeviceName()
        {
            return UIDevice.CurrentDevice.Name;
        }


        /// <summary>
        ///     Start a Background Task for the given action. This enables us to run for up to 180 seconds in background instaed of only 10 seconds.
        ///     We may want to add an overload for this method that takes not async actions. 
        /// </summary>
        /// <returns>The code in background mode.</returns>
        /// <param name="action">Action.</param>
        public async Task RunCodeInBackgroundMode(Func<Task> action, string name = "MyBackgroundTaskName")
        {
            nint taskId = 0;
            var taskEnded = false;
            taskId = UIApplication.SharedApplication.BeginBackgroundTask(name, () =>
            {
                Console.WriteLine($"Background task '{name}' got killed");
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
            await Task.Factory.StartNew(async () =>
            {
                Console.WriteLine($"Background task '{name}' started");
                await action();
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
                Console.WriteLine($"Background task '{name}' finished");
            });

            await Task.Factory.StartNew(async () =>
            {
                while (!taskEnded)
                {
                    Console.WriteLine($"Background task '{name}' time remaining: {UIApplication.SharedApplication.BackgroundTimeRemaining}");
                    await Task.Delay(1000);
                }
            });
        }
    }
}
