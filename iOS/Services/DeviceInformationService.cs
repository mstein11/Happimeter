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

        public async Task RunCodeInBackgroundMode(Action action)
        {
            nint taskId = 0;
            var taskEnded = false;
            taskId = UIApplication.SharedApplication.BeginBackgroundTask("MyTaskName", () =>
            {
                Console.WriteLine("Background task got killed");
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
            await Task.Factory.StartNew(() =>
            {
                action();
            });

            await Task.Factory.StartNew(async () =>
            {
                while (!taskEnded)
                {
                    Console.WriteLine("Background time remaining: " + UIApplication.SharedApplication.BackgroundTimeRemaining);
                    await Task.Delay(1000);
                }
            });

            taskEnded = true;
            UIApplication.SharedApplication.EndBackgroundTask(taskId);
        }
    }
}
