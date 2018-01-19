using System.Threading.Tasks;

namespace Happimeter.Interfaces
{
    public interface IHappimeterApiService
    {
        Task<object> Auth(string email, string password);
    }
}