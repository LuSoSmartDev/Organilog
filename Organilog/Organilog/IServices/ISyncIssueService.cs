using System;
using System.Threading.Tasks;

namespace Organilog.IServices
{
    public interface ISyncIssueService
    {
        Task<bool> SyncFromServer(int method, Action onSuccess, Action<string> onError = null, bool showOverlay = false);

        Task<bool> SyncFromServer(int method);

        Task<bool> SyncToServerAsync(int method);
    }
}
