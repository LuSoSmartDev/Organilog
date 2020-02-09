using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Organilog.IServices
{
    public interface IEquipmentService
    {
        Task<List<Equipment>> GetEquipments(int limit = 0, int offset = 0);

        Task<List<Equipment>> SearchEquipments(string searchKey, int limit = 0);

        Task<List<Equipment>> SearchEquipmentByClient(string searchKey,int clientId, int limit = 0);

        Task<Equipment> GetEquipmentDetail(Guid id);

        Task<bool> UpdateEquipment(Equipment equipment);


    }
}
