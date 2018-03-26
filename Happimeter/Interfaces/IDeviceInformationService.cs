using System;
using System.Threading.Tasks;

namespace Happimeter.Interfaces
{
    public interface IDeviceInformationService
    {
        string GetPhoneOs();
        string GetDeviceName();
        Task RunCodeInBackgroundMode(Action action);
    }
}