using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Models.Response;
using Organilog.Models.SetSync;
using Organilog.Views.Popups;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.Services
{
    public class SyncIssueService : BaseService, ISyncIssueService
    {
        private DateTime timeStart;
        private int totalRowInserted, totalRowUpdated;


        public Task<bool> SyncFromServer(int method, Action onSuccess, Action<string> onError = null, bool showOverlay = false)
        {
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
            int page = 0;
            Debug.WriteLine("Sync Issue From Server Started!");

            if (!Settings.LastSyncIssue.Equals("0"))
            {
                await SyncToServerAsync(method);
                var url = ApiURI.URL_BASE(CurrentAccount) + ApiURI.URL_GET_ISSUE(CurrentUserName, Settings.CurrentPassword, page, Settings.LastSyncIssue);
                var response = await restClient.GetStringAsync(url);

                if (!string.IsNullOrWhiteSpace(response) && !response.Equals("[]"))
                {
                    ProcessData(response);
                    page++; //page increase 
                }
                else
                {
                    SetLastSyncIssue();
                    //NeedToSync = false;
                }
                Debug.WriteLine("Sync Issue Time: " + DateTime.Now.Subtract(timeStart).TotalSeconds);
                Debug.WriteLine("Total Issue Inserted: " + totalRowInserted);
                Debug.WriteLine("Total Issue Updated: " + totalRowUpdated);
                Debug.WriteLine("Sync Issue From Server Ended!");

                return await Task.FromResult(true);
            }
            
            bool NeedToSync = true;
            while (NeedToSync) {
                                
                var response = await restClient.GetStringAsync(ApiURI.URL_BASE(CurrentAccount) + ApiURI.URL_GET_ISSUE(CurrentUserName, Settings.CurrentPassword,page, Settings.LastSyncIssue));
               
                if (!string.IsNullOrWhiteSpace(response) && !response.Equals("[]"))
                {
                    ProcessData(response);
                    page++; //page increase 
                }
                else 
                {
                    SetLastSyncIssue();
                    NeedToSync = false;
                }
            }

            Debug.WriteLine("Sync Issue Time: " + DateTime.Now.Subtract(timeStart).TotalSeconds);
            Debug.WriteLine("Total Issue Inserted: " + totalRowInserted);
            Debug.WriteLine("Total Issue Updated: " + totalRowUpdated);
            Debug.WriteLine("Sync Issue From Server Ended!");

            return await Task.FromResult(true);
        }

        private void ProcessData(string response)
        {
            var result = SyncData.FromJson(response);

            if (result != null)
            {
                DoSync(result);
            }
        }

        private void SetLastSyncIssue()
        {
            //update before 1 hour
            Settings.LastSyncIssue = DateTime.Now.AddHours(0).ToUnixTimestamp().ToString();// remove add -1 hour
            if (Settings.LastSyncIssue.Contains(","))
            {
                Settings.LastSyncIssue = Settings.LastSyncIssue.Split(',')[0];
            }
            if (App.LocalDb.Table<Setting>().ToList().Find(se => se.Name.Equals("APP_LAST_SYNCHRO_ISSUE")) is Setting lastSyncIssue)
            {
                lastSyncIssue.Value = Settings.LastSyncIssue;
                App.LocalDb.Update(lastSyncIssue);
            }

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
                                case "iss":
                                    InsertOrUpdateIssue(item.V);
                                    break;

                                case "issr":
                                    InsertOrUpdateIssueLink(item.V);
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

        private void InsertOrUpdateIssueLink(Dictionary<string, object> v)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<IssueReplyResponse>(JsonConvert.SerializeObject(v), App.DefaultDeserializeSettings);
                if (result == null || result.ServerId == 0)
                    return;
                if (App.LocalDb.Table<IssueLink>().FirstOrDefault(adr => adr.ServerId == result.ServerId) is IssueLink issueLink)
                {
                   
                    issueLink.UpdateFromResponse(result);

                    issueLink.UserId = CurrentUser.Id;

                    App.LocalDb.Update(issueLink);
                    totalRowUpdated++;
                }
                else
                {
                    App.LocalDb.InsertOrReplace(new IssueLink(result)
                    {
                        UserId = CurrentUser.Id
                    });
                    totalRowInserted++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Deserialize IssueLink failed: " + ex);
            }
        }

        private void InsertOrUpdateIssue(Dictionary<string, object> data)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<Models.Response.IssueResponse>(JsonConvert.SerializeObject(data), App.DefaultDeserializeSettings);
                if (result == null || result.ServerId == 0)
                    return;

                if (App.LocalDb.Table<Issue>().FirstOrDefault(iv => iv.ServerId == result.ServerId) is Issue issue)
                {
                    //PropertyExtension.UpdateProperties(result, invoice, ExcludedUpdateProperties);
                   //TODO 
                    issue.UpdateFromResponse(result);

                    issue.UserId = CurrentUser.Id;

                    App.LocalDb.Update(issue);
                    totalRowUpdated++;
                }
                else
                {
                    App.LocalDb.InsertOrReplace(new Issue(result)
                    {
                        UserId = CurrentUser.Id,
                        AccountId = CurrentUser.FkAccountId,
                    });
                    totalRowInserted++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Insert Issue failed: " + ex);
            }
        }

        public async Task<bool> SyncToServerAsync(int method)
        {
            var issues = App.LocalDb.Table<Issue>().Where(iss => iss.UserId == CurrentUser.Id && iss.IsToSync).ToArray();
            var issue_replys = App.LocalDb.Table<IssueLink>().Where(issr => issr.UserId == CurrentUser.Id && issr.IsToSync).ToArray();

            JObject @params = new JObject();

            if (issues.LongLength > 0)
                @params.Add(new JProperty("issues", JArray.FromObject(issues).RemoveEmptyChildren(issues.FirstOrDefault()?.PropertyIgnore)));

            if (issue_replys.LongLength > 0)
                @params.Add(new JProperty("issues_lines", JArray.FromObject(issue_replys).RemoveEmptyChildren(issue_replys.FirstOrDefault()?.PropertyIgnore)));

            if (@params.Count > 0)
            {
                @params.Add(new JProperty("api_version","2"));
                @params.Add(new JProperty("appVersion", ApiURI.APP_VERSION));
            }
            else
            {
                return await Task.FromResult(true);
            }

            var response = await restClient.PostAsync(ApiURI.URL_BASE(CurrentAccount) + ApiURI.URL_SET_ISSUE(CurrentUserName, Settings.CurrentPassword), @params);
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("RESPONSE: " + responseContent);

            if (response.IsSuccessStatusCode)
            {

                var result = JsonConvert.DeserializeObject<SetIssueResponse>(responseContent, App.DefaultDeserializeSettings);
                if (result != null && result.Success.Equals("OK"))
                {
                    foreach (var issue in issues)
                    {
                        if (result.Issues.FirstOrDefault(c => ((c.ServerId > 0 && c.ServerId == issue.ServerId) || Guid.TryParse(c.AppId, out Guid id) && issue.Id.Equals(id))) is Models.SetSync.IssueResponse issresponse && issresponse.ServerId > 0)
                        {
                            var iss = issue.DeepCopy();
                            App.LocalDb.Delete(issue);
                            iss.ServerId = issresponse.ServerId;
                            iss.UserId  = CurrentUser.Id;
                            //invoice.SynchronizationDate = DateTime.Now;
                            iss.IsToSync = false;
                            iss.CodeId = issresponse.CodeId;
                            App.LocalDb.Insert(iss);
                        }
                        else
                        {
                            issue.IsToSync = false;
                            //inv.SynchronizationDate = DateTime.Now;
                            App.LocalDb.Update(issue);
                        }
                    }

                    foreach (var invl in issue_replys)
                    {
                        if (result.IssuesLines.FirstOrDefault(a => ((a.ServerId > 0 && a.ServerId == invl.ServerId) || Guid.TryParse(a.AppId, out Guid id) && invl.Id.Equals(id))) is Models.SetSync.IssueLineResponse invlr && invlr.ServerId > 0)
                        {
                            var issueLink = invl.DeepCopy();
                            App.LocalDb.Delete(invl);
                            issueLink.ServerId = invlr.ServerId;
                            //invoiceProduct.SynchronizationDate = DateTime.Now;
                            issueLink.IsToSync = false;
                            App.LocalDb.Insert(issueLink);
                        }
                        else
                        {
                            invl.IsToSync = false;
                            //invl.SynchronizationDate = DateTime.Now;
                            App.LocalDb.Update(invl);
                        }
                    }

                    Debug.WriteLine("SET_SYNC_INVOICE Successed!");
                }
                
            }
            else
            {
                Debug.WriteLine("SET_SYNC_INVOICE Failed: ");
            }

            return await Task.FromResult(true);
        }
    }
}
