using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Organilog.IServices
{
    public interface ISyncEquipmentService
    {
        Task<bool> SyncFromServer(int method, Action onSuccess, Action<string> onError = null, bool showOverlay = false);

        Task<bool> SyncFromServer(int method);

        Task<bool> SyncToServer(int method);

    }
}
