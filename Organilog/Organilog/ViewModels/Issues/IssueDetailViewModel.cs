using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Models.Response;
using Organilog.ViewModels.Shared;
using Plugin.ExternalMaps;
using Plugin.Messaging;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Issues
{
    public class IssueDetailViewModel : BaseViewModel
    {

        private readonly IIssueService issueService;

        private readonly LoginResponse CurrentUser = Settings.CurrentUser;

        private Issue issue;
        public Issue Issue { get => issue; set => SetProperty(ref issue, value); }


        private bool isDone;
        public bool IsDone { get => isDone; set => SetProperty(ref isDone, value); }

        private bool isAssigned;
        public bool IsAssigned { get => isAssigned; set => SetProperty(ref isAssigned, value); }

        private bool detailVisible = true;
        public bool DetailVisible { get => detailVisible; set => SetProperty(ref detailVisible, value); }

        private Color detailTabColor = Color.FromHex("#47CEC0");
        public Color DetailTabColor { get => detailTabColor; set => SetProperty(ref detailTabColor, value); }

        private bool mediaVisible;
        public bool MediaVisible { get => mediaVisible; set => SetProperty(ref mediaVisible, value); }

        private Color mediaTabColor;
        public Color MediaTabColor { get => mediaTabColor; set => SetProperty(ref mediaTabColor, value); }

        public ICommand DeleteIssueCommand { get; private set; }
        public ICommand EditIssueCommand { get; private set; }
        public ICommand AddReplyCommand { get; private set; }

        public ICommand OpenInterventionDetailCommand { get; private set; }
        public ICommand SendMailToCustomerCommand { get; private set; }
        public ICommand ViewDetailCommand { get; private set; }
        public ICommand ViewMediasCommand { get; private set; }
        public ICommand OpenMapCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand PickPhotoCommand { get; private set; }
        public ICommand SignatureCommand { get; private set; }
        public ICommand ViewMediaCommand { get; private set; }
        public ICommand DeleteMediaCommand { get; private set; }
        public ICommand ViewHistoryCommand { get; private set; }

        public IssueDetailViewModel(IIssueService interventionService)
        {
            this.issueService = interventionService;

            DeleteIssueCommand = new AwaitCommand(DeleteIssue);
            EditIssueCommand = new AwaitCommand(EditIssue);
           
            OpenInterventionDetailCommand = new AwaitCommand(OpenIssueDetail);
          
            ViewDetailCommand = new AwaitCommand(ViewDetail);
            ViewMediasCommand = new AwaitCommand(ViewMedias);
            OpenMapCommand = new AwaitCommand(OpenMap);
            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);
            ViewHistoryCommand = new AwaitCommand(ViewHistory);
            AddReplyCommand = new AwaitCommand(AddReply);
        }

        private async void AddReply(object obj, TaskCompletionSource<bool> tcs)
        {

             await CoreMethods.PushViewModel<AddReplyViewModel>(modal: true);

             tcs.TrySetResult(true);
            
        }

        public override async void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Issue = parameters.GetValue<Issue>(ContentKey.SELECTED_ISSUE)?.DeepCopy();

            InitIssue();

            PropertyChanged += ViewModel_PropertyChanged;

            //Issue = await issueService.GetIssueDetail(Issue.Id);


        }



        public override void OnPushed(NavigationParameters parameters)
        {
            base.OnPushed(parameters);

            MessagingCenter.Subscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);
            MessagingCenter.Subscribe<NewIssueViewModel>(this, MessageKey.ISSUE_CHANGED, OnIssueChanged);
            MessagingCenter.Subscribe<AddReplyViewModel, string>(this, MessageKey.REPLY_CHANGED, OnMessageAdded);
        }
        private static string currentText = "";
        //private static DateTime lastReceive = DateTime.Now;
        private void OnMessageAdded(AddReplyViewModel obj, string message)
        {
            /* 
             var currentTime = DateTime.Now;
             if (currentText.Equals(message) && currentTime.Subtract(lastReceive).TotalSeconds < 3)
             {
                 Issue.OnPropertyChanged("Messages");

                 return;
             }*/
           
            if (!string.IsNullOrEmpty(message))
            {
                setMessage(message);
            }

            

           // PropertyChanged += ViewModel_PropertyChanged;

        }
        private async Task<bool> setMessage(string message)
        {
            try
            {

                var replayData = new IssueLink()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    AddDate = DateTime.Now,
                    AuthorType = 0,
                    AddedByFkUserId = CurrentUser.Id,
                    EditDate = DateTime.Now,
                    IssueId = Issue.ServerId,
                    IssueApplId = Issue.Id,
                    IsActif = 1,
                    IsToSync = true,
                    Message = message,
                };
                var lastReceive = DateTime.Now;
                //lastReceive = lastReceive.AddHours(2);


                var authorName = "Unknown";
                if (replayData.AuthorType == 0)
                {
                    var user = App.LocalDb.Table<User>().ToList().FirstOrDefault(u => replayData.AddedByFkUserId > 0 && replayData.AddedByFkUserId == u.ServerId && u.IsActif == 1);
                    if (user != null)
                        authorName = user.FullName;
                }
                else if (replayData.AuthorType == 1)
                {
                    authorName = Issue.Client.FullName;
                }

                //var dateformatstring = String.Format("{0:dd/MM/yyyy à HH:MM}", replayData.EditDate != null ? replayData.EditDate : replayData.AddDate);
                //replayData.displayOwner = string.Format("{0}, le {1}", authorName, dateformatstring);

                // App.LocalDb.InsertOrReplace(replayData);
                //MessagingCenter.Send(this, MessageKey.ISSUE_CHANGED);
                //InitIssue();

                var allMessager = App.LocalDb.Table<IssueLink>().ToList().FindAll(il => (il.Message != null && il.Message.Equals(message))
                                                                                           && ((!il.IssueApplId.Equals(Guid.Empty) && il.IssueApplId.Equals(Issue.Id))
                                                                                           || (il.IssueId == Issue.ServerId && Issue.ServerId != 0))).OrderByDescending(il => il.AddDate);
                var LastMessager = allMessager != null && allMessager.Count() > 0 ? allMessager.First() : null;


                if (LastMessager == null)
                {
                    //Issue.Messages.Add(replayData);
                    App.LocalDb.InsertOrReplace(replayData);
                    MessagingCenter.Send(this, MessageKey.ISSUE_CHANGED);
                    // InitIssue();
                }
                else
                {
                    if (lastReceive.Subtract(LastMessager.AddDate.Value).TotalSeconds > 4)
                    {
                        //Issue.Messages.Add(replayData);
                        App.LocalDb.InsertOrReplace(replayData);

                        MessagingCenter.Send(this, MessageKey.ISSUE_CHANGED);
                        // Issue.Messages.First().OnPropertyChanged("Display");
                    }
                    //InitIssue();
                }


                InitIssue();

                return await Task.FromResult(false);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public override void OnPopped()
        {
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED);
            MessagingCenter.Unsubscribe<NewIssueViewModel>(this, MessageKey.ISSUE_CHANGED);
            MessagingCenter.Unsubscribe<AddReplyViewModel>(this, MessageKey.REPLY_CHANGED);
           
        }


        private async void OnIssueChanged(NewIssueViewModel sender)
        {
            Issue = await issueService.GetIssueDetail(Issue.Id);

            InitIssue(); 

        }


        private void InitIssue()
        {
            if (Issue != null)
            {
                
                UserDialogs.Instance.Loading(TranslateExtension.GetValue("message_loading")).Show();

                Task.Run(async () =>
                {
                    return await issueService.GetIssueDetail(Issue.Id);
                }).ContinueWith(task => Device.BeginInvokeOnMainThread(async () =>
                {
                    UserDialogs.Instance.Loading().Hide();
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        try
                        {

                            Issue = task.Result;

                            if (Issue.CodeId != 0)
                                Title = "Nº" + Issue.CodeId;
                            else
                                Title = " ";

                            if (Issue.Messages == null)
                            {
                                Issue.Messages = new ObservableCollection<IssueLink>();
                            }
                            if (Issue.MediaLinks == null)
                            {
                                Issue.MediaLinks = new ObservableCollection<MediaLink>();
                            }
                            //InitIssue();
                        }
                        catch (Exception ex)
                        {
                            await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
                        }
                    }
                    else if (task.IsFaulted)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                    }


                }));
            }
            Issue.PropertyChanged += Issue_PropertyChanged;
        }

        void MediaLinks_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }


        private async void Issue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO 
            Issue.PropertyChanged -= Issue_PropertyChanged;

            //if (e.PropertyName == nameof(Intervention.SendMail))
            //{
            //    Issue.IsToSync = true;

            //    await interventionService.UpdateIntervention(Issue);

            //    MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);
            //}

            Issue.PropertyChanged += Issue_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged -= ViewModel_PropertyChanged;

            if (e.PropertyName == nameof(Messages))
            {
               
            }

            PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void DeleteIssue(object sender, TaskCompletionSource<bool> tcs)
        {
            if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_delete_intervention_confirm"), TranslateExtension.GetValue("alert_message_yes"), TranslateExtension.GetValue("alert_message_no")))
            {
                Issue.IsActif = 0;
                Issue.IsToSync = true;

                await issueService.UpdateIssue(Issue);

                MessagingCenter.Send(this, MessageKey.ISSUE_CHANGED);

                await CoreMethods.PopViewModel();
            }

            tcs.TrySetResult(true);
        }

        private async void EditIssue(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.ISSUE_MODE,  1},
                    { ContentKey.SELECTED_ISSUE,  Issue}
                };

                await CoreMethods.PushViewModel<NewIssueViewModel>(parameters);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }

       

        private void OpenIssueDetail(object sender, TaskCompletionSource<bool> tcs)
        {
            //TODO 
            //if (Issue.ServerId > 0 && !string.IsNullOrWhiteSpace(Issue.Nonce))
            //{
            //    Device.OpenUri(new Uri(ApiURI.URL_PDF(Settings.CurrentAccount, Intervention.ServerId, Intervention.Nonce)));
            //}

            tcs.TrySetResult(true);
        }

        private void ViewDetail(object sender, TaskCompletionSource<bool> tcs)
        {
            DetailTabColor = Color.FromHex("#47CEC0");
            DetailVisible = true;

            MediaTabColor = Color.Transparent;
            MediaVisible = false;

            tcs.TrySetResult(true);
        }

        private void ViewMedias(object sender, TaskCompletionSource<bool> tcs)
        {
            MediaTabColor = Color.FromHex("#47CEC0");
            MediaVisible = true;

            DetailTabColor = Color.Transparent;
            DetailVisible = false;

            tcs.TrySetResult(true);
        }

       

        private async void OpenMap(object sender, TaskCompletionSource<bool> tcs)
        {

            if (Device.RuntimePlatform == Device.Android)
            {
                DependencyService.Get<IOpenMapIntentService>().Open(Issue.Address.Latitude, Issue.Address.Longitude);
                return;
            }

            if (CrossExternalMaps.IsSupported)
            {
                await CrossExternalMaps.Current.NavigateTo("", Issue.Address.Latitude, Issue.Address.Longitude);
            }
            else
            {
               
               Device.OpenUri(new Uri(string.Format("http://maps.apple.com/?daddr=" + Issue.Address.Latitude + "," + Issue.Address.Longitude + "")));
              
            }

            tcs.TrySetResult(true);
        }

        private async void TakePhoto(object sender, TaskCompletionSource<bool> tcs)
        {
            var photo = await PhotoHelper.TakePhotoStreamAsync();

            if (photo != null)
                SetMedia(photo, false);

            tcs.TrySetResult(true);
        }

        private async void PickPhoto(object sender, TaskCompletionSource<bool> tcs)
        {
            var photo = await PhotoHelper.PickPhotoStreamAsync();

            if (photo != null)
                SetMedia(photo, false);

            tcs.TrySetResult(true);
        }

        private async void Signature(object sender, TaskCompletionSource<bool> tcs)
        {
            var signaturePad = new Views.Popups.SignaturePad();
            signaturePad.ContentSaved += SignaturePad_ContentSaved;

            await CoreMethods.PushPage(signaturePad, modal: true);

            tcs.TrySetResult(true);
        }

        private void SignaturePad_ContentSaved(object sender, Stream content)
        {
            if (content != null)
                SetMedia(content, true);
        }

        private async void SetMedia(Stream content, bool isSignature)
        {
            string fileName;

            if (isSignature)
                fileName = "SIG_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
            else
                fileName = "IMG_" + DateTime.Now.ToString("ddMMyyyyHHmmss");

            var media = new Media()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                AccountId = CurrentUser.Id,
                FileName = fileName,
                Year = DateTime.Today.Year.ToString(),
                Month = DateTime.Today.Month.ToString(),
                FileData = await content.ToBase64(),
                IsActif = 1,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                IsToSync = true,
                Type = "issue"
            };

            var mediaLink = new MediaLink()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkColumnAppliId = Issue.Id,
                FkColumnServerId = Issue.ServerId,
                FkMediaAppliId = media.Id,
                FkMediaServerId = media.ServerId,
                LinkTable = "issue",
                IsActif = 1,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                Media = media,
                IsToSync = true
            };

            Issue.MediaLinks.Add(mediaLink);

            App.LocalDb.InsertOrReplace(media);
            App.LocalDb.InsertOrReplace(mediaLink);

            Issue.IsToSync = true;

            Issue.OnPropertyChanged("MediaLinks");
        }

        private async void ViewMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_MEDIA, mediaLink}
                };
                if (mediaLink.Media.isPDF && Device.RuntimePlatform == Device.Android)
                    Device.OpenUri(new Uri(mediaLink.Media.ImageUri));
                else
                    await CoreMethods.PushViewModel<MediaDetailViewModel>(parameters, modal: true);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }

        private void OnMediaSaved(object sender, MediaLink mediaLink)
        {
            try
            {
                if (mediaLink.IsDelete)
                {
                    if (mediaLink.ServerId > 0)
                    {
                        if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                        {
                            mediaLink.Media.SetProperty(nameof(mediaLink.Media.IsActif), 0);
                            mediaLink.Media.IsToSync = true;

                            App.LocalDb.Update(mediaLink.Media);
                        }
                        else
                        {
                            App.LocalDb.Delete(mediaLink.Media);
                        }

                        mediaLink.SetProperty(nameof(mediaLink.IsActif), 0);
                        mediaLink.IsToSync = true;

                        App.LocalDb.Update(mediaLink);
                    }
                    else
                    {
                        App.LocalDb.Delete(mediaLink.Media);
                        App.LocalDb.Delete(mediaLink);
                    }

                    Issue.MediaLinks.Remove(mediaLink);
                }
                else if (mediaLink.IsEdited)
                {
                    var content = new MemoryStream(Convert.FromBase64String(mediaLink.Media.FileData));
                    SetMedia(content, false);
                    mediaLink.IsActif = 0;
                    mediaLink.IsToSync = true;
                    mediaLink.IsDelete = true;

                    if (mediaLink.ServerId > 0)
                    {

                        if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                        {
                            mediaLink.Media.SetProperty(nameof(mediaLink.Media.IsActif), 0);
                            mediaLink.Media.IsToSync = true;

                            App.LocalDb.Update(mediaLink.Media);
                        }
                        else
                        {
                            App.LocalDb.Delete(mediaLink.Media);
                        }

                        mediaLink.SetProperty(nameof(mediaLink.IsActif), 0);
                        mediaLink.IsToSync = true;

                        App.LocalDb.Update(mediaLink);

                    }
                    else
                    {
                        App.LocalDb.Delete(mediaLink.Media);
                        App.LocalDb.Delete(mediaLink);
                        mediaLink.Media.IsToSync = true;
                    }
                    mediaLink.IsToSync = true;
                    Issue.MediaLinks.Remove(mediaLink);
                    Issue.OnPropertyChanged("MediaLinks");

                }
                else
                {
                    mediaLink.Media.EditDate = DateTime.Now;
                    mediaLink.EditDate = DateTime.Now;

                    mediaLink.Media.IsToSync = true;
                    mediaLink.IsToSync = true;

                    App.LocalDb.Update(mediaLink.Media);
                    App.LocalDb.Update(mediaLink);
                }

                if (Issue.MediaLinks.FirstOrDefault(m => m.Id == mediaLink.Id) is MediaLink mediaLnk)
                {
                    Issue.MediaLinks[Issue.MediaLinks.IndexOf(mediaLnk)] = mediaLink;
                }
            }
            catch (Exception ex)
            {
                CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
        }

        private void DeleteMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            if (mediaLink.ServerId > 0)
            {
                if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                {
                    mediaLink.Media.IsActif = 0;
                    mediaLink.Media.IsToSync = true;

                    App.LocalDb.Update(mediaLink.Media);
                }
                else
                {
                    App.LocalDb.Delete(mediaLink.Media);
                }

                mediaLink.IsActif = 0;
                mediaLink.IsToSync = true;

                App.LocalDb.Update(mediaLink);
            }
            else
            {
                if (mediaLink.Media != null)
                    App.LocalDb.Delete(mediaLink.Media);
                App.LocalDb.Delete(mediaLink);
            }

            Issue.MediaLinks.Remove(mediaLink);

            Issue.OnPropertyChanged("MediaLinks");

            tcs.TrySetResult(true);
        }

        private async void ViewHistory(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_INTERVENTION, Issue}
                };

                //await CoreMethods.PushViewModel<InterventionHistoryViewModel>(parameters);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }
    }
}

