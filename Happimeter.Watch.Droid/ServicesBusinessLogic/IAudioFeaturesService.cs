using System;
using System.Collections.Concurrent;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public interface IAudioFeaturesService
    {
        ConcurrentBag<double> VadMeasures { get; set; };
        bool IsActive
        {
            get;
        }

        void Toggle();
        void OnApplicationStartup();
    }
}
