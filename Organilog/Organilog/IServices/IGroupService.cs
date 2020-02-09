using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Organilog.Models;

namespace Organilog.IServices
{
    public interface IGroupService
    {
        Task<List<Category>> GetGroups(int userId, int offset = 0, int limit = 0);

        Task<List<Category>> SearchGroup(int userId, string searchKey, int limit = 0);
        Task<List<Category>> SearchGroupByClient(Client client, int userId, string searchKey, int limit = 0);
    }
}
