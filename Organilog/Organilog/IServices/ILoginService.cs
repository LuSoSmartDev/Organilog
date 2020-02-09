using Organilog.Models.Response;
using System.Threading.Tasks;

namespace Organilog.IServices
{
    public interface ILoginService
    {
        Task<LoginResponse> Login(string account, string userName, string password);
    }
}