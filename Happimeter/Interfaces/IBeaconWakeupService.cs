namespace Happimeter.Interfaces
{
    public interface IBeaconWakeupService
    {
        void StartWakeupForBeacon(string uuid, int minor, int major);
    }
}