using System.Threading.Tasks;

namespace Happimeter.Services
{
    public interface INotificationService
    {
        void SetupNotificationHooks();
        void SubscibeToChannel(string channel);
        Task UploadDeviceToken(string token);
    }
}