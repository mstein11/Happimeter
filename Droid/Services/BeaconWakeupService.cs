using System;
using Happimeter.Interfaces;

namespace Happimeter.Droid.Services
{
    public class BeaconWakeupService : IBeaconWakeupService
    {
        public BeaconWakeupService()
        {
        }

        public void StartWakeupForBeacon(string uuid, int minor, int major)
        {
            throw new NotImplementedException();
        }

        public void StartWakeupForBeacon()
        {
            throw new NotImplementedException();
        }
    }
}
