using System.Net.Http;
using System.Threading.Tasks;

namespace Happimeter.Interfaces
{
    public interface IRestService
    {
        void AddAuthorizationTokenToInstance(string token);
        Task<HttpResponseMessage> Get(string url);
        Task<HttpResponseMessage> Post(string url, object data);
        Task<HttpResponseMessage> Delete(string url);
        Task<HttpResponseMessage> FileUpload(string url, string path);
    }
}
