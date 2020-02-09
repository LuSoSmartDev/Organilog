using Acr.UserDialogs;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Models.Response;
using Organilog.ViewModels.Shared;
using Plugin.ExternalMaps;
using Plugin.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration;

namespace Organilog.ViewModels.Interventions
{
    public class InterventionDetailViewModel : BaseViewModel

    {
        private readonly IInterventionService interventionService;

        private readonly LoginResponse CurrentUser = Settings.CurrentUser;

        private Intervention intervention;
        public Intervention Intervention { get => intervention; set => SetProperty(ref intervention, value); }

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

        public string Done_Time_Non_Working { get; private set; }
        public string Plaining_Time_Non_working { get; set; }
        public ICommand DeleteInterventionCommand { get; private set; }
        public ICommand EditInterventionCommand { get; private set; }
        public ICommand AssignToMeCommand { get; private set; }
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

        public ICommand OpenPhoneCall { get; private set; }
        public ICommand SendMailCommand { get; private set; }

        public ICommand Onclick_Label { get; private set; }

        public ICommand TapCommandURL { get; private set; }

        public enum MAPFilter
        {
            GooleMap = 0, //default 
            Waze,
            AppleMap,
            Bing,
            Other
        }

        public InterventionDetailViewModel(IInterventionService interventionService)
        {
            this.interventionService = interventionService;

            DeleteInterventionCommand = new AwaitCommand(DeleteIntervention);
            EditInterventionCommand = new AwaitCommand(EditIntervention);
            AssignToMeCommand = new AwaitCommand(AssignToMe);
            OpenInterventionDetailCommand = new AwaitCommand(OpenInterventionDetail);
            SendMailToCustomerCommand = new AwaitCommand(SendMailToCustomer);
            ViewDetailCommand = new AwaitCommand(ViewDetail);
            ViewMediasCommand = new AwaitCommand(ViewMedias);
            OpenMapCommand = new AwaitCommand(OpenMap);
            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);
            ViewHistoryCommand = new AwaitCommand(ViewHistory);
            OpenPhoneCall = new AwaitCommand<string>(OnPhoneCall);
            SendMailCommand = new AwaitCommand<string>(SendMail);
            TapCommandURL = new AwaitCommand<string>(OpenURL);
        }

        private void OpenURL(string url, TaskCompletionSource<bool> arg2)
        {
          Device.OpenUri(new Uri(url));

            arg2.SetResult(true);
        }

        private void OnPhoneCall(object arg1, TaskCompletionSource<bool> arg2)
        {
            var phoneDialer = CrossMessaging.Current.PhoneDialer;
            if (phoneDialer.CanMakePhoneCall)
                phoneDialer.MakePhoneCall(arg1.ToString());

            arg2.SetResult(true);
        }

        private void SendMail(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var emailMessenger = CrossMessaging.Current.EmailMessenger;
                if (emailMessenger.CanSendEmail)
                {
                    // Send simple e-mail to single receiver without attachments, bcc, cc etc.
                    emailMessenger.SendEmail(
                        sender.ToString(),
                        "Organilog Test Send Mail To Client",
                        "");

                    //UserDialogs.Instance.Toast(new ToastConfig(TranslateExtension.GetValue("alert_message_send_mail_successed")));
                }
                else
                {
                    UserDialogs.Instance.Toast(new ToastConfig(TranslateExtension.GetValue("alert_message_cant_send_email")));
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Toast(new ToastConfig(ex.Message));
            }

            tcs.SetResult(true);
        }

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Intervention = parameters.GetValue<Intervention>(ContentKey.SELECTED_INTERVENTION)?.DeepCopy();

            InitIntervention();
            Done_Time_Non_Working = "00:00";
            if (!string.IsNullOrEmpty(Intervention.DoneHour) && Intervention.DoneHourStart != null && Intervention.DoneHourEnd != null)
            {
                var splitstringStart = Intervention.DoneHourStart.Split(':');
                var splitstringEnd = Intervention.DoneHourEnd.Split(':');
                var splitstringworking = Intervention.DoneHour.Split(':');

                if (splitstringStart != null && splitstringStart.Length > 0 && splitstringEnd != null && splitstringEnd.Length > 0)
                {
                    var hourStart = splitstringStart[0];
                    var minStart = splitstringStart[1];
                    var hourEnd = splitstringEnd[0];
                    var minEnd = splitstringEnd[1];
                    var hourWorking = splitstringworking[0];
                    var minWorking = splitstringworking[1];
                    double totalHour = 0;
                    if (Intervention.DoneDateStart == null || Intervention.DoneDateEnd == null)
                        totalHour = 0;
                    else
                        totalHour = (Intervention.DoneDateEnd.Value - Intervention.DoneDateStart.Value).TotalHours;

                    var realhourMin = (Convert.ToInt32(hourEnd) * 60 + Convert.ToInt32(minEnd)) - (Convert.ToInt32(hourStart) * 60 + Convert.ToInt32(minStart));

                    var nonworkingmin = Convert.ToInt32(totalHour) * 60 + realhourMin - (Convert.ToInt32(hourWorking) * 60 + Convert.ToInt32(minWorking));
                    int realHour = nonworkingmin / 60;
                    int realMin = nonworkingmin % 60;

                    Done_Time_Non_Working = string.Format("{0}:{1}", realHour, realMin > 9 ? "" + realMin : "0" + realMin);

                }

            }

            if (!IsAssigned && Intervention.LastViewDate == null)
            {
                Intervention.LastViewDate = DateTime.Now;
                App.LocalDb.Update(Intervention);
                MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);
                MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED, Intervention.Id);

            }
            IsDone = Intervention.IsDone == 1;
            PropertyChanged += ViewModel_PropertyChanged;
        }

        public override void OnPushed(NavigationParameters parameters)
        {
            base.OnPushed(parameters);

            MessagingCenter.Subscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);
            MessagingCenter.Subscribe<NewInterventionViewModel>(this, MessageKey.INTERVENTION_CHANGED, OnInterventionChanged);
        }

        public override void OnPopped()
        {
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED);
            MessagingCenter.Unsubscribe<NewInterventionViewModel>(this, MessageKey.INTERVENTION_CHANGED);
        }

        private async void OnInterventionChanged(NewInterventionViewModel sender)
        {
            Intervention = await interventionService.GetIntervention(Intervention.Id);

            InitIntervention();
        }

        private void InitIntervention()
        {
            if (Intervention != null)
            {
                if (Intervention.Code != 0)
                    Title = "Nº " + Intervention.Code;
                else
                    Title = " ";
                if (Intervention.LinkInterventionTasks == null)
                {
                    Intervention.LinkInterventionTasks = new List<LinkInterventionTask>();
                }
                Intervention.PlanningTasks = new ObservableCollection<LinkInterventionTask>();
                Intervention.PlanningTasks.AddRange(Intervention.LinkInterventionTasks.FindAll(lit => lit.IsPlanningToDo == 1));
                Intervention.DoneTasks = new ObservableCollection<LinkInterventionTask>();
                Intervention.DoneTasks.AddRange(Intervention.LinkInterventionTasks.FindAll(lit => lit.IsDone == 1));

                IsAssigned = Intervention.FkUserServerlId != 0;

                Intervention.PropertyChanged += Intervention_PropertyChanged;
            }
            Title = "dkkkk";
        }

        private async void Intervention_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Intervention.PropertyChanged -= Intervention_PropertyChanged;

            if (e.PropertyName == nameof(Intervention.SendMail))
            {
                Intervention.IsToSync = true;

                await interventionService.UpdateIntervention(Intervention);

                MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);
            }

            Intervention.PropertyChanged += Intervention_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged -= ViewModel_PropertyChanged;

            if (e.PropertyName == nameof(IsDone))
            {
                try
                {
                    if (IsDone)
                    {
                        Intervention.IsDone = 1;

                        if (await LocationHelper.CheckLocationPermission(false) && LocationHelper.IsGeolocationAvailable(false) && LocationHelper.IsGeolocationEnabled(false))
                        {
                            var position = await LocationHelper.GetCurrentPosition(showOverlay: false);

                            if (position != null && (position.Latitude != 0 && position.Longitude != 0))
                            {
                                Intervention.DoneLatitude = position.Latitude;
                                Intervention.DoneLongitude = position.Longitude;
                                Intervention.DoneAltitude = position.Altitude;
                            }
                        }

                        await interventionService.CalculateDoneTime(Intervention);

                        MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);
                    }
                    else
                    {
                        if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_intervention_undone"), TranslateExtension.GetValue("yes"), TranslateExtension.GetValue("no")))
                        {
                            Intervention.IsDone = AppSettings.MobileShowToggleProgress ? 2 : 0;

                            await interventionService.CalculateDoneTime(Intervention);

                            MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);
                        }
                        else
                        {
                            Intervention.IsDone = 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                }
            }

            PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void DeleteIntervention(object sender, TaskCompletionSource<bool> tcs)
        {
            if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_delete_intervention_confirm"), TranslateExtension.GetValue("alert_message_yes"), TranslateExtension.GetValue("alert_message_no")))
            {
                Intervention.IsActif = 0;
                Intervention.IsToSync = true;

                await interventionService.UpdateIntervention(Intervention);

                MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);

                await CoreMethods.PopViewModel();
            };

            tcs.TrySetResult(true);
        }

        private async void EditIntervention(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.INTERVENTION_MODE,  1},
                    { ContentKey.SELECTED_INTERVENTION,  Intervention}
                };

                await CoreMethods.PushViewModel<NewInterventionViewModel>(parameters);
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

        private void AssignToMe(object sender, TaskCompletionSource<bool> tcs)
        {
            if (!ConnectivityHelper.IsNetworkAvailable())
            {
                tcs.TrySetResult(true);
                return;
            }

            UserDialogs.Instance.Loading(TranslateExtension.GetValue("alert_message_assigning")).Show();

            Task.Run(async () =>
            {
                return await interventionService.AssignToMe(Intervention.ServerId);
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(async () =>
            {
                UserDialogs.Instance.Loading().Hide();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    if (task.Result)
                    {
                        Intervention.FkUserAppId = CurrentUser.Uuid;
                        Intervention.FkUserServerlId = CurrentUser.Id;

                        await interventionService.UpdateIntervention(Intervention);

                        MessagingCenter.Send(this, MessageKey.INTERVENTION_ASSIGNED);

                        await CoreMethods.DisplayAlert("", TranslateExtension.GetValue("alert_message_intervention_assigned"), TranslateExtension.GetValue("ok"));

                        await CoreMethods.PopViewModel();
                    }
                    else
                    {
                        await CoreMethods.DisplayAlert("", TranslateExtension.GetValue("alert_message_intervention_already_assigned"), TranslateExtension.GetValue("ok"));
                    }
                }
                else
                {
                    if (task.IsFaulted && task.Exception?.GetBaseException().Message is string message)
                        Debug.Write("ASSING_INTERVENTION_ERROR: " + message);

                    await CoreMethods.DisplayAlert("", TranslateExtension.GetValue("alert_message_intervention_assign_error"), TranslateExtension.GetValue("ok"));
                }
            }));

            tcs.TrySetResult(true);
        }

        private void OpenInterventionDetail(object sender, TaskCompletionSource<bool> tcs)
        {
            if (Intervention.ServerId > 0 && !string.IsNullOrWhiteSpace(Intervention.Nonce))
            {
                Device.OpenUri(new Uri(ApiURI.URL_PDF(Settings.CurrentAccount, Intervention.ServerId, Intervention.Nonce)));
            }

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

        private void SendMailToCustomer(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var emailMessenger = CrossMessaging.Current.EmailMessenger;
                if (emailMessenger.CanSendEmail)
                {
                    // Send simple e-mail to single receiver without attachments, bcc, cc etc.
                    emailMessenger.SendEmail(
                        Intervention.Client.Email,
                        "Organilog Test",
                        Intervention.ToString());

                    UserDialogs.Instance.Toast(new ToastConfig(TranslateExtension.GetValue("alert_message_send_mail_successed")));
                }
                else
                {
                    UserDialogs.Instance.Toast(new ToastConfig(TranslateExtension.GetValue("alert_message_cant_send_email")));
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Toast(new ToastConfig(ex.Message));
            }

            tcs.TrySetResult(true);
        }

        private async void OpenMap(object sender, TaskCompletionSource<bool> tcs)
        {
          /*
            try
            {
                var lat = Intervention.Address.Latitude.ToString().Replace(",", ".");
                var longi = Intervention.Address.Longitude.ToString().Replace(",", ".");
                const string wazePrefix = "waze://";
              
                Android.Content.Intent intent = new Android.Content.Intent(Android.Content.Intent.ActionView, Android.Net.Uri.Parse(wazePrefix));
                string wazeURL = ("https://waze.com/ul?q=" + address + "&ll=" + lat + "," + longi + "&z=8&navigate=yes");
                wazeURL = wazeURL.Replace(" ", "%20");
                var resolveInfo = Android.App.Application.Context.PackageManager.ResolveActivi‌​ty(intent, 0);
                Android.Net.Uri wazeUri;
                if (resolveInfo != null)
                {
                    wazeUri = Android.Net.Uri.Parse(wazeURL);
                }
                else
                {
                    wazeUri = Android.Net.Uri.Parse("market://details?id=com.waze");
                }
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                intent.SetData(wazeUri);
                Android.App.Application.Context.StartActivity(intent);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                App.Shell.Alert("Erro ao abrir o Waze.\n" + ex.Message);
                return Task.FromResult(false);
            }*/
            /*
            if (Device.RuntimePlatform == Device.Android)
            {
                DependencyService.Get<IOpenMapIntentService>().Open(Intervention.Address.Latitude, Intervention.Address.Longitude);
                tcs.TrySetResult(true);
            }
            */
            //if (CrossExternalMaps.IsSupported)
            //{
            //    await CrossExternalMaps.Current.NavigateTo("", Intervention.Address.Latitude, Intervention.Address.Longitude);
            //}
            //else
            //{

            //    Device.OpenUri(new Uri(string.Format("http://maps.apple.com/?daddr=" + Intervention.Address.Latitude + "," + Intervention.Address.Longitude + "")));

            //}
            /*
            const string GOOGLEMAPS = "comgooglemaps://";
            const string WAZEMAPS = "waze://";

            var mapApps = new[] { GOOGLEMAPS, WAZEMAPS };
            var installedMapApps = new List<string>();

            foreach (var a in mapApps)
            {
                if (await Launcher.CanOpenAsync(a))
                    installedMapApps.Add(a);
            }

            var choice = await Application.Current.MainPage.DisplayActionSheet("Open Map in...", "Cancel", null, installedMapApps.ToArray());

            if (choice == GOOGLEMAPS) { 
                await Launcher.OpenAsync($"{GOOGLEMAPS}?center={Intervention.Address.Latitude},{Intervention.Address.Longitude}");
            } else if (choice == WAZEMAPS) {
                await Launcher.OpenAsync($"{WAZEMAPS}?l={Intervention.Address.Latitude},{Intervention.Address.Longitude}");
            }
    
            

            */
            var location = new  Xamarin.Essentials.Location(Intervention.Address.Latitude, Intervention.Address.Longitude);
            var options =  new MapLaunchOptions { NavigationMode = NavigationMode.Driving };
            await Map.OpenAsync(location, options);
             

            tcs.TrySetResult(true);
           
        }

        private async void openWaze()
        {
            var placemark = new Placemark
            {
                CountryName = "United States",
                AdminArea = "WA",
                Thoroughfare = "Microsoft Building 25",
                Locality = "Redmond"
            };
            var options = new MapLaunchOptions { Name = "Microsoft Building 25" };

            await Map.OpenAsync(placemark, options);
        }
        private void openMap()
        {

        }

        private async void TakePhoto(object sender, TaskCompletionSource<bool> tcs)
        {
            var photo = await PhotoHelper.TakePhotoStreamAsync();

            if (photo != null)
               await SetMedia(photo, false);

           

            tcs.TrySetResult(true);
            Device.BeginInvokeOnMainThread(() =>
            {

                Intervention.OnPropertyChanged("MediaLinks");
            });

        }

        private async void PickPhoto(object sender, TaskCompletionSource<bool> tcs)
        {
            var photo = await PhotoHelper.PickPhotoStreamAsync();

            if (photo != null)
                await SetMedia(photo, false);

            tcs.TrySetResult(true);

            Device.BeginInvokeOnMainThread(() =>
            {

                Intervention.OnPropertyChanged("MediaLinks");
            });

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

            Device.BeginInvokeOnMainThread(() =>
            {

                Intervention.OnPropertyChanged("MediaLinks");
            });
        }

        private async Task<bool> SetMedia(Stream content, bool isSignature)
        {
            try {

                string fileName;

                if (isSignature)
                    fileName = "SIG_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                else
                    fileName = "IMG_" + DateTime.Now.ToString("ddMMyyyyHHmmss");

                var imageStremBase64 = await content.ToBase64();
                if (content != null)
                {
                    content.Dispose();
                    content = null;
                    //System.GC.Collect();
                    
                }
                var media = new Media()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    AccountId = CurrentUser.Id,
                    FileName = fileName,
                    Year = DateTime.Today.Year.ToString(),
                    Month = DateTime.Today.Month.ToString(),
                    FileData = imageStremBase64,
                    IsActif = 1,
                    AddDate = DateTime.Now,
                    EditDate = DateTime.Now,
                    IsToSync = true
                };

                var mediaLink = new MediaLink()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    FkColumnAppliId = Intervention.Id,
                    FkColumnServerId = Intervention.ServerId,
                    FkMediaAppliId = media.Id,
                    FkMediaServerId = media.ServerId,
                    LinkTable = "intervention",
                    IsActif = 1,
                    AddDate = DateTime.Now,
                    EditDate = DateTime.Now,
                    Media = media,
                    IsToSync = true
                };

                Intervention.MediaLinks.Add(mediaLink);

                App.LocalDb.InsertOrReplace(media);
                App.LocalDb.InsertOrReplace(mediaLink);

                Intervention.IsToSync = true;

                //if(isSignature)
                //Intervention.OnPropertyChanged("MediaLinks");

                return await Task.FromResult(true);

            }
            catch
            {
                return await Task.FromResult(false);
            }
            

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

                    Intervention.MediaLinks.Remove(mediaLink);
                } else if (mediaLink.IsEdited)
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
                    Intervention.MediaLinks.Remove(mediaLink);
                    Intervention.OnPropertyChanged("MediaLinks");

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

                if (Intervention.MediaLinks.FirstOrDefault(m => m.Id == mediaLink.Id) is MediaLink mediaLnk)
                {
                    Intervention.MediaLinks[Intervention.MediaLinks.IndexOf(mediaLnk)] = mediaLink;
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

            Intervention.MediaLinks.Remove(mediaLink);

            Intervention.OnPropertyChanged("MediaLinks");

            tcs.TrySetResult(true);
        }

        private async void ViewHistory(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_INTERVENTION, Intervention}
                };

                await CoreMethods.PushViewModel<InterventionHistoryViewModel>(parameters);
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