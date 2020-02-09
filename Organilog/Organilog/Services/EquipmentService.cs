using Organilog.IServices;
using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Extensions;

namespace Organilog.Services
{
    public class EquipmentService : BaseService, IEquipmentService
    {
        public Task<Equipment> GetEquipmentDetail(Guid id)
        {
            if (App.LocalDb.Table<Equipment>().ToList().Find(i => !i.Id.Equals(Guid.Empty) && !id.Equals(Guid.Empty) && i.Id.Equals(id)) is Equipment equipment)
            {
                GetRelations(equipment);
             
                return Task.FromResult(equipment);
            }
            return null;

        }

        private void GetRelations(Equipment equipment)
        {
            equipment.Client = App.LocalDb.Table<Client>().FirstOrDefault(c => c.UserId == CurrentUser.Id && ((c.ServerId > 0 && c.ServerId == equipment.ClientServerId) || (!c.Id.Equals(Guid.Empty) && !equipment.ClientAppliId.Equals(Guid.Empty) && c.Id.Equals(equipment.ClientAppliId))) && c.IsActif == 1);
            equipment.Address = App.LocalDb.Table<Address>().FirstOrDefault(a => a.UserId == CurrentUser.Id && ((a.ServerId > 0 && a.ServerId == equipment.AdresseServerId) || (!a.Id.Equals(Guid.Empty) && !equipment.AdresseAppliId.Equals(Guid.Empty) && a.Id.Equals(equipment.AdresseAppliId))) && a.IsActif == 1);
            //equipment.Intervention = App.LocalDb.Table<Intervention>().FirstOrDefault(i => i.UserId == CurrentUser.Id && ((i.ServerId > 0 && i.ServerId == invoice.FkInterventionServerId) || (!i.Id.Equals(Guid.Empty) && !invoice.FkInterventionAppId.Equals(Guid.Empty) && i.Id.Equals(invoice.FkInterventionAppId))) && i.IsActif == 1);
            equipment.MediaLinks = App.LocalDb.Table<MediaLink>().ToList().FindAll(medl => medl.UserId == CurrentUser.Id && ((medl.FkColumnServerId > 0 && medl.FkColumnServerId == equipment.ServerId) || (!medl.FkColumnAppliId.Equals(Guid.Empty) && medl.FkColumnAppliId.Equals(equipment.Id))) && !medl.IsDelete && medl.IsActif == 1).ToObservableCollection();
        }

        public Task<List<Equipment>> GetEquipments(int limit = 0, int offset = 0)
        {
            IEnumerable<Equipment> result = App.LocalDb.Table<Equipment>().ToList().FindAll(inv => inv.UserId == CurrentUser.Id && (inv.ClientServerId > 0 || !inv.ClientAppliId.Equals(Guid.Empty)) && (inv.AdresseServerId>0 || !inv.AdresseAppliId.Equals(Guid.Empty)) && inv.IsActif == 1)
              .OrderByDescending(inv => inv.ServerId).ThenByDescending(inv => inv.DateGuaranteeStart).ThenByDescending(inv => inv.DateBuy);

            foreach (var item in result)
            {
                GetRelations(item);
               
            }

            return Task.FromResult(result.ToList());

        }

        public Task<List<Equipment>> SearchEquipments(string searchKey, int limit = 0)
        {
            IEnumerable<Equipment> result = App.LocalDb.Table<Equipment>().ToList().FindAll(equ => equ.UserId == CurrentUser.Id 
                && ((!string.IsNullOrWhiteSpace(equ.Nonce) && equ.Nonce.UnSignContains(searchKey)) || (!string.IsNullOrWhiteSpace(equ.CodeId) && equ.CodeId.UnSignContains(searchKey))) && equ.IsActif == 1)
                .OrderByDescending(inv => inv.DateBuy).ThenByDescending(inv => inv.DateInstall).ThenByDescending(inv => inv.EditDate);

            if (limit > 0)
                result = result.Take(limit);

            foreach (var item in result)
            {
                GetRelations(item);
              
            }

            return Task.FromResult(result.ToList());
        }

        public Task<bool> UpdateEquipment(Equipment equipment)
        {
            try
            {
                return Task.FromResult(App.LocalDb.Update(equipment) > 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update intervention failed: " + ex);
                return Task.FromResult(false);
            }
        }

        public Task<List<Equipment>> SearchEquipmentByClient(string searchKey, int clientId, int limit = 0)
        {
            IEnumerable<Equipment> result;
            if (String.IsNullOrEmpty(searchKey))
                result = App.LocalDb.Table<Equipment>().ToList().FindAll(equ => equ.UserId == CurrentUser.Id && (equ.ClientServerId > 0  && equ.ClientServerId ==clientId) && (equ.AdresseServerId > 0 || !equ.AdresseAppliId.Equals(Guid.Empty)) && equ.IsActif == 1)
                        .OrderByDescending(inv => inv.ServerId).ThenByDescending(inv => inv.DateGuaranteeStart).ThenByDescending(inv => inv.DateBuy);
            else
                result = App.LocalDb.Table<Equipment>().ToList().FindAll(equ => equ.UserId == CurrentUser.Id && equ.ClientServerId == clientId 
                    && ((!string.IsNullOrWhiteSpace(equ.Nonce) && equ.Nonce.UnSignContains(searchKey)) || (!string.IsNullOrWhiteSpace(equ.CodeId) && equ.CodeId.UnSignContains(searchKey))) && equ.IsActif == 1)
                    .OrderByDescending(inv => inv.DateBuy).ThenByDescending(inv => inv.DateInstall).ThenByDescending(inv => inv.EditDate);

            if (limit > 0)
                result = result.Take(limit);

            foreach (var item in result)
            {
                GetRelations(item);

            }

            return Task.FromResult(result.ToList());
        }
    }
}
