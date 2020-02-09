

using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Models.Response;
using Organilog.ViewModels.Interventions;
using Organilog.ViewModels.Shared;
using Plugin.ExternalMaps;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Equipments
{
    public class EquipmentDetailViewModel : BaseViewModel
    {
        private readonly IEquipmentService EquipmentService;

        private readonly LoginResponse CurrentUser = Settings.CurrentUser;

        private Equipment equipment;
        public Equipment Equipment { get => equipment; set => SetProperty(ref equipment, value); }

        private bool isDone;
        public bool IsDone { get => isDone; set => SetProperty(ref isDone, value); }

        private bool isAssigned;
        public bool IsAssigned { get => isAssigned; set => SetProperty(ref isAssigned, value); }

        private bool detailVisible = true;
        public bool DetailVisible { get => detailVisible; set => SetProperty(ref detailVisible, value); }

     
     
        private bool mediaVisible;
        public bool MediaVisible { get => mediaVisible; set => SetProperty(ref mediaVisible, value); }

        
        public ICommand DeleteEquipmentCommand { get; private set; }
        public ICommand EditEquipmentCommand { get; private set; }
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
        public ICommand AddInterventionCommand { get; set; }

        public EquipmentDetailViewModel(IEquipmentService interventionService)
        {
            this.EquipmentService = interventionService;

            DeleteEquipmentCommand = new AwaitCommand(DeleteEquipment);
            EditEquipmentCommand = new AwaitCommand(EditEquipment);

            OpenInterventionDetailCommand = new AwaitCommand(OpenEquipmentDetail);

            ViewDetailCommand = new AwaitCommand(ViewDetail);
            ViewMediasCommand = new AwaitCommand(ViewMedias);
            OpenMapCommand = new AwaitCommand(OpenMap);
            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);
            ViewHistoryCommand = new AwaitCommand(ViewHistory);
            AddInterventionCommand = new AwaitCommand(AddIntervention);

        }

        private async void AddIntervention(object sender, TaskCompletionSource<bool> tcs)
        {
            if (IsBusy)
                return;

            var parameters = new NavigationParameters()
            {
                { ContentKey.SELECTED_EQUIPMENT, Equipment}
            };

            await CoreMethods.PushViewModel<NewInterventionViewModel>(parameters);

            tcs.SetResult(true);
        }

        public override async void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Equipment = parameters.GetValue<Equipment>(ContentKey.SELECTED_EQUIPMENT)?.DeepCopy();

            InitEquipment();
            Equipment = await EquipmentService.GetEquipmentDetail(Equipment.Id);

            if (Equipment.MediaLinks == null)
            {
                Equipment.MediaLinks = new ObservableCollection<MediaLink>();
            }

            PropertyChanged += ViewModel_PropertyChanged;
        }



        public override void OnPushed(NavigationParameters parameters)
        {
            base.OnPushed(parameters);

            MessagingCenter.Subscribe<NewEquipmentViewModel>(this, MessageKey.EQUIPMENT_CHANGED, OnEquipmentChanged);
         
        }

        
        public override void OnPopped()
        {
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED);
            MessagingCenter.Unsubscribe<NewEquipmentViewModel>(this, MessageKey.EQUIPMENT_CHANGED);
        }

        private async void OnEquipmentChanged(NewEquipmentViewModel sender)
        {
            Equipment = await EquipmentService.GetEquipmentDetail(Equipment.Id);

            InitEquipment();

        }


        private void InitEquipment()
        {
            if (Equipment != null)
            {
                if (!String.IsNullOrEmpty(Equipment.CodeId))
                    Title = "Nº" + Equipment.CodeId;
                else
                    Title = " ";
                //if (Equipment.Messages.Count > 0)
                //{
                //    foreach(var item in Equipment.Messages)
                //    {
                //        var authorName = "Unknown";
                //        if (item.AuthorType == 0)
                //        {
                //            var user = App.LocalDb.Table<User>().ToList().FirstOrDefault(u => item.AddedByFkUserId > 0 && item.AddedByFkUserId == u.ServerId && u.IsActif == 1);
                //            if (user != null)
                //                authorName = user.FullName;
                //        }else if (item.AuthorType == 1)
                //        {
                //            authorName = Equipment.Client.FullName;
                //        }


                //        var dateformatstring = String.Format("{0:dd/MM/yyyy à HH:MM}", item.EditDate != null ? item.EditDate : item.AddDate);
                //        item.displayOwner = string.Format("{0}, le {1}", authorName, dateformatstring);
                //    }
                //}
            }
            Equipment.PropertyChanged += Equipment_PropertyChanged;
        }

        void MediaLinks_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }


        private async void Equipment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO 
            Equipment.PropertyChanged -= Equipment_PropertyChanged;

            //if (e.PropertyName == nameof(Intervention.SendMail))
            //{
            //    Equipment.IsToSync = true;

            //    await interventionService.UpdateIntervention(Equipment);

            //    MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);
            //}

            Equipment.PropertyChanged += Equipment_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged -= ViewModel_PropertyChanged;

            if (e.PropertyName == nameof(Messages))
            {

            }

            PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void DeleteEquipment(object sender, TaskCompletionSource<bool> tcs)
        {
            if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_equipment"), TranslateExtension.GetValue("alert_message_delete_intervention_confirm"), TranslateExtension.GetValue("alert_message_yes"), TranslateExtension.GetValue("alert_message_no")))
            {
                Equipment.IsActif = 0;
                Equipment.IsToSync = true;

                await EquipmentService.UpdateEquipment(Equipment);

                MessagingCenter.Send(this, MessageKey.EQUIPMENT_CHANGED);

                await CoreMethods.PopViewModel();
            };

            tcs.TrySetResult(true);
        }

        private async void EditEquipment(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.EQUIPMENT_MODE, 1 },
                    { ContentKey.SELECTED_EQUIPMENT,  Equipment}
                };

                await CoreMethods.PushViewModel<NewEquipmentViewModel>(parameters);
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



        private void OpenEquipmentDetail(object sender, TaskCompletionSource<bool> tcs)
        {
            //TODO 
            //if (Equipment.ServerId > 0 && !string.IsNullOrWhiteSpace(Equipment.Nonce))
            //{
            //    Device.OpenUri(new Uri(ApiURI.URL_PDF(Settings.CurrentAccount, Intervention.ServerId, Intervention.Nonce)));
            //}

            tcs.TrySetResult(true);
        }

        private void ViewDetail(object sender, TaskCompletionSource<bool> tcs)
        {
          
            DetailVisible = true;

            MediaVisible = false;

            tcs.TrySetResult(true);
        }

        private void ViewMedias(object sender, TaskCompletionSource<bool> tcs)
        {
           
            MediaVisible = true;
            DetailVisible = false;

            tcs.TrySetResult(true);
        }



        private async void OpenMap(object sender, TaskCompletionSource<bool> tcs)
        {

            if (Device.RuntimePlatform == Device.Android)
            {
                DependencyService.Get<IOpenMapIntentService>().Open(Equipment.Address.Latitude, Equipment.Address.Longitude);
                return;
            }

            if (CrossExternalMaps.IsSupported)
            {
                await CrossExternalMaps.Current.NavigateTo("", Equipment.Address.Latitude, Equipment.Address.Longitude);
            }
            else
            {

                Device.OpenUri(new Uri(string.Format("http://maps.apple.com/?daddr=" + Equipment.Address.Latitude + "," + Equipment.Address.Longitude + "")));

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
                Type = "Equipment"
            };

            var mediaLink = new MediaLink()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkColumnAppliId = Equipment.Id,
                FkColumnServerId = Equipment.ServerId,
                FkMediaAppliId = media.Id,
                FkMediaServerId = media.ServerId,
                LinkTable = "Equipment",
                IsActif = 1,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                Media = media,
                IsToSync = true
            };

            Equipment.MediaLinks.Add(mediaLink);

            App.LocalDb.InsertOrReplace(media);
            App.LocalDb.InsertOrReplace(mediaLink);

            Equipment.IsToSync = true;

            Equipment.OnPropertyChanged("MediaLinks");
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

                    Equipment.MediaLinks.Remove(mediaLink);
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
                    Equipment.MediaLinks.Remove(mediaLink);
                    Equipment.OnPropertyChanged("MediaLinks");

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

                if (Equipment.MediaLinks.FirstOrDefault(m => m.Id == mediaLink.Id) is MediaLink mediaLnk)
                {
                    Equipment.MediaLinks[Equipment.MediaLinks.IndexOf(mediaLnk)] = mediaLink;
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
                    App.LocalDb.Delete(mediaLink);

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

            Equipment.MediaLinks.Remove(mediaLink);

            Equipment.OnPropertyChanged("MediaLinks");

            tcs.TrySetResult(true);
        }

        private async void ViewHistory(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_INTERVENTION, Equipment}
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
