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
                //when time is up and task has not finished, call this method to finish the task to prevent the app from being terminated
                Console.WriteLine($"Background task '{name}' got killed");
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
            await Task.Factory.StartNew(async () =>
            {
                //here we run the actual task
                Console.WriteLine($"Background task '{name}' started");
                await action();
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
                Console.WriteLine($"Background task '{name}' finished");
            });

            await Task.Factory.StartNew(async () =>
            {
                //Just a method that logs how much time we have remaining. Usually a background task has around 180 seconds to complete. 
                while (!taskEnded)
                {
                    Console.WriteLine($"Background task '{name}' time remaining: {UIApplication.SharedApplication.BackgroundTimeRemaining}");
                    await Task.Delay(1000);
                }
            });
        }
    }
}
