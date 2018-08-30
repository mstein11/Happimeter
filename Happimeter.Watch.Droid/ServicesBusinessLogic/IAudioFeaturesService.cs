using System;
namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public interface IAudioFeaturesService
    {
        bool IsActive
        {
            get;
        }

        void Toggle();
        void OnApplicationStartup();
    }
}
