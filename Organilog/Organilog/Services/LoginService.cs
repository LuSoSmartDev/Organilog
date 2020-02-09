using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models.Response;
using System.Threading.Tasks;

namespace Organilog.Services
{
    public class LoginService : BaseService, ILoginService
    {
        public async Task<LoginResponse> Login(string account, string userName, string password)
        {
            var result = await restClient.GetStringAsync<LoginResponse>(ApiURI.URL_BASE(account) + ApiURI.URL_GET_LOGIN(userName, password));

            return await Task.FromResult(result);
        }
    }
}