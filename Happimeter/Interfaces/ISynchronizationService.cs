using System.Threading.Tasks;

namespace Happimeter.Services
{
    public interface ISynchronizationService
    {
        Task Sync();
    }
}