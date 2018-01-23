using System.Threading.Tasks;
using Happimeter.Models.ServiceModels;

namespace Happimeter.Interfaces
{
    public interface IHappimeterApiService
    {
        Task<AuthResultModel> Auth(string email, string password);
        Task<GetMeResultModel> GetMe();
    }
}