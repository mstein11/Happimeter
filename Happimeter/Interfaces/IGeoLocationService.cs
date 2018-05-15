using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace Happimeter.Services
{
    public interface IGeoLocationService
    {
        Task<Position> GetLocation();
        bool IsLocationAvailable();
    }
}