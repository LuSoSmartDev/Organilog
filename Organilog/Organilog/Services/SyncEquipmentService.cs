using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Models.SetSync;
using Organilog.Views.Popups;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.Services
{
    public class SyncEquipmentService : BaseService, ISyncEquipmentService
    {
        private readonly string[] ExcludedUpdateProperties =
       {
            "Id",
            "UserId",
            "AccountId",
            "IsToSync"
        };

        private DateTime timeStart;
        private int totalRowInserted, totalRowUpdated;
        public Task<bool> SyncFromServer(int method, Action onSuccess, Action<string> onError = null, bool showOverlay = false)
        {
            //process 
            if (!ConnectivityHelper.IsNetworkAvailable(method == 2))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    onError?.Invoke(TranslateExtension.GetValue("alert_no_internt_message"));
                });
                return Task.FromResult(false);
            }

            if (showOverlay)
                DependencyService.Get<IPopupService>().ShowContent(new LoadingScreen1() { Message = TranslateExtension.GetValue("content_message_synchronizing") });

            Task.Run(async () =>
            {
                return await SyncFromServer(method);
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
            {
                if (showOverlay)
                    DependencyService.Get<IPopupService>().HideContent();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    if (task.Result)
                    {
                        if (showOverlay)
                            UserDialogs.Instance.Toast(new ToastConfig(TranslateExtension.GetValue("alert_message_sync_completed")) { });
                        onSuccess?.Invoke();
                    }
                    else
                    {
                        if (showOverlay)
                            UserDialogs.Instance.Toast(new ToastConfig(TranslateExtension.GetValue("alert_message_sync_failed")) { });
                        onError?.Invoke(TranslateExtension.GetValue("alert_message_sync_failed"));
                    }
                }
                else if (task.IsFaulted && task.Exception?.GetBaseException().Message is string message)
                {
                    if (showOverlay)
                        UserDialogs.Instance.Toast(new ToastConfig(message) { });
                    onError?.Invoke(message);
                }
            }));

            return Task.FromResult(true);
        }

        public async Task<bool> SyncFromServer(int method)
        {
            timeStart = DateTime.Now;
            totalRowInserted = 0;
            totalRowUpdated = 0;

            Debug.WriteLine("Sync Equipment From Server Started!");

            if (!Settings.LastSyncEquipment.Equals("0"))
            {
                await SyncToServer(method);
            }

            int page = 0;
            bool NeedToSync = true;
            Debug.WriteLine("Sync Equipment To Server Started!");
            while (NeedToSync)
            {
                string url = ApiURI.URL_BASE(CurrentAccount) + ApiURI.URL_GET_EQUIPMENT(CurrentUserName, Settings.CurrentPassword, page, Settings.LastSyncEquipment);
                Debug.WriteLine("url: " + url);
                var response = await restClient.GetStringAsync(url);
                Debug.WriteLine("Response Invoice: " + response);
                if (!string.IsNullOrWhiteSpace(response) && !response.Equals("[]"))
                {
                    ProcessData(response);
                    page++;
                }
                else
                {
                    SetLastSyncEquipmnet();
                    NeedToSync = false;
                }
            }

            Debug.WriteLine("Sync Invoice Time: " + DateTime.Now.Subtract(timeStart).TotalSeconds);
            Debug.WriteLine("Total Invoice Inserted: " + totalRowInserted);
            Debug.WriteLine("Total Invoice Updated: " + totalRowUpdated);
            Debug.WriteLine("Sync Invoice From Server Ended!");

            return await Task.FromResult(true);
        }

        private void ProcessData(string response)
        {
            var result = SyncData.FromJson(response);

            if (result != null)
            {
                if (DoSync(result))
                {
                    SetLastSyncEquipmnet();
                }

                return;
            }
        }

        private void SetLastSyncEquipmnet()
        {
            Settings.LastSyncEquipment = ((long)DateTime.Now.AddHours(0).ToUnixTimestamp()).ToString();

            if (App.LocalDb.Table<Setting>().ToList().Find(se => se.Name.Equals("APP_LAST_SYNCHRO_EQUIPMENT")) is Setting  lastSyncEquipment)
            {
                lastSyncEquipment.Value = Settings.LastSyncEquipment;
                App.LocalDb.Update(lastSyncEquipment);
            }
            /*
            Settings.LastSync = ((long)DateTime.Now.ToUnixTimestamp()).ToString();
            
            if (App.LocalDb.Table<Setting>().ToList().Find(se => se.Name.Equals("APP_LAST_SYNCHRO")) is Setting lastSync)
            {
                lastSync.Value = Settings.LastSync.ToString();
                App.LocalDb.Update(lastSync);
            }*/
        }

        private bool DoSync(Dictionary<string, List<SyncData>> syncDatas)
        {
            try
            {
                App.LocalDb.BeginTransaction();

                foreach (var data in syncDatas)
                {
                    foreach (var item in data.Value)
                    {
                        if (item != null && item.V != null)
                        {
                            switch (item.I)
                            {
                                case "equ":
                                    InsertOrUpdateEquipment(item.V);
                                    break;

                                case "lie":
                                    InsertOrUpdateInterventionEquipment(item.V);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }

                App.LocalDb.Commit();

                return true;
            }
            catch (Exception ex)
            {
                App.LocalDb.Rollback();

                totalRowInserted = 0;

                Debug.WriteLine("Sync Error: ");
                Debug.WriteLine(ex.GetBaseException().Message);
                Debug.WriteLine(ex);

                return false;
            }
        }

        private void InsertOrUpdateEquipment(Dictionary<string, object> data)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<Models.Response.EquipmentResponse>(JsonConvert.SerializeObject(data), App.DefaultDeserializeSettings);
                if (result == null || result.ServerId == 0)
                    return;
                if (App.LocalDb.Table<Equipment>().FirstOrDefault(iv => iv.ServerId == result.ServerId) is Equipment equipment)
                {
                    //PropertyExtension.UpdateProperties(result, invoice, ExcludedUpdateProperties);
                    equipment.UpdateFromResponse(result);

                    equipment.UserId = CurrentUser.Id;

                    App.LocalDb.Update(equipment);
                    totalRowUpdated++;
                }
                else
                {
                    App.LocalDb.InsertOrReplace(new Equipment(result)
                    {
                        UserId = CurrentUser.Id,
                        AccountId = CurrentUser.FkAccountId,
                    });
                    totalRowInserted++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Insert equipment failed: " + ex);
            }
        }

        private void InsertOrUpdateInterventionEquipment(Dictionary<string, object> data)
        {
            //TODO work for link to intervention
            /*try
            {
                var result = JsonConvert.DeserializeObject<>(JsonConvert.SerializeObject(data), App.DefaultDeserializeSettings);
                if (result == null || result.ServerId == 0)
                    return;

                if (App.LocalDb.Table<InvoiceProduct>().FirstOrDefault(ip => ip.ServerId == result.ServerId) is InvoiceProduct invoiceProduct)
                {
                    //PropertyExtension.UpdateProperties(result, invoiceProduct, ExcludedUpdateProperties);
                    invoiceProduct.UpdateFromResponse(result);

                    invoiceProduct.UserId = CurrentUser.Id;

                    App.LocalDb.Update(invoiceProduct);
                    totalRowUpdated++;
                }
                else
                {
                    App.LocalDb.InsertOrReplace(new InvoiceProduct(result)
                    {
                        UserId = CurrentUser.Id,
                        AccountId = CurrentUser.FkAccountId,
                    });
                    totalRowInserted++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Insert InvoiceProduct failed: " + ex);
            }*/
        }

        public async Task<bool> SyncToServer(int method)
        {
            try { 
            var equipments = App.LocalDb.Table<Equipment>().Where(inv => inv.UserId == CurrentUser.Id && inv.IsToSync).ToArray();
            var interventions_euipments = App.LocalDb.Table<LinkInterventionEquipment>().Where(lie => lie.UserId == CurrentUser.Id && lie.IsToSync).ToArray();

            JObject @params = new JObject();

            if (equipments.LongLength > 0)
                @params.Add(new JProperty("equipments", JArray.FromObject(equipments).RemoveEmptyChildren(equipments.FirstOrDefault()?.PropertyIgnore)));

            if (interventions_euipments.LongLength > 0)
                @params.Add(new JProperty("equipements_links", JArray.FromObject(interventions_euipments).RemoveEmptyChildren(interventions_euipments.FirstOrDefault()?.PropertyIgnore)));

            if (@params.Count > 0)
            {
                @params.Add(new JProperty("api_version", ApiURI.API_MOBILE_TO_SERVER_VERSION));
                @params.Add(new JProperty("appVersion", ApiURI.APP_VERSION));
            }
            else
            {
                return await Task.FromResult(true);
            }

            var response = await restClient.PostAsync(ApiURI.URL_BASE(CurrentAccount) + ApiURI.URL_SET_EQUIPMENT(CurrentUserName, Settings.CurrentPassword), @params);
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("RESPONSE: " + responseContent);
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<SetSyncEquipment>(responseContent, App.DefaultDeserializeSettings);
                if (result != null && result.Success.Equals("OK"))
                {
                    foreach (var inv in equipments)
                    {
                        if (result.Equipements.FirstOrDefault(c => ((c.ServerId > 0 && c.ServerId == inv.ServerId) || Guid.TryParse(c.AppId, out Guid id) && inv.Id.Equals(id))) is EquipmentResponse invr && invr.ServerId > 0)
                        {
                            var equipment = inv.DeepCopy();
                            App.LocalDb.Delete(inv);
                            equipment.ServerId = invr.ServerId;
                            equipment.CodeId = invr.CodeId;
                            //invoice.SynchronizationDate = DateTime.Now;

                            equipment.IsToSync = false;
                            App.LocalDb.Insert(equipment);
                        }
                        else
                        {
                            inv.IsToSync = false;
                            //inv.SynchronizationDate = DateTime.Now;
                            App.LocalDb.Update(inv);
                        }
                    }
                    /*
                    foreach (var invl in interventions_euipments)
                    {
                        if (result.InvoicesLines.FirstOrDefault(a => ((a.ServerId > 0 && a.ServerId == invl.ServerId) || Guid.TryParse(a.AppId, out Guid id) && invl.Id.Equals(id))) is Models.SetSync.InvoiceLineResponse invlr && invlr.ServerId > 0)
                        {
                            var invoiceProduct = invl.DeepCopy();
                            App.LocalDb.Delete(invl);
                            invoiceProduct.ServerId = invlr.ServerId;
                            //invoiceProduct.SynchronizationDate = DateTime.Now;
                            invoiceProduct.IsToSync = false;
                            App.LocalDb.Insert(invoiceProduct);
                        }
                        else
                        {
                            invl.IsToSync = false;
                            //invl.SynchronizationDate = DateTime.Now;
                            App.LocalDb.Update(invl);
                        }
                    }*/

                    Debug.WriteLine("SET_SYNC_Equipment Successed!");
                }
            }
            else
            {
                Debug.WriteLine("SET_SYNC_INVOICE Failed: ");
            }
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return await Task.FromResult(true);
        }

    }
}
