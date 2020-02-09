using System;
using System.Collections.Generic;
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
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Issues
{
    public class NewIssueViewModel: BaseViewModel
    {

        private readonly IIssueService issueService;

        private readonly LoginResponse CurrentUser = Settings.CurrentUser;

        private Client selectedClient;

        private List<Issue> NewChildren = new List<Issue>();
        private List<Issue> DeletedChildren = new List<Issue>();
     
        private List<MediaLink> NewMedias = new List<MediaLink>();
        private List<MediaLink> DeletedMedias = new List<MediaLink>();

        private int mode; // 0: Add 1: Edit
        public int Mode { get => mode; set => SetProperty(ref mode, value); }

        private bool canEditClientAddress;
        public bool CanEditClientAddress { get => canEditClientAddress; set => SetProperty(ref canEditClientAddress, value); }

        private Issue issue;
        public Issue Issue { get => issue; set => SetProperty(ref issue, value); }

        private List<Filiale> listFiliale = new List<Filiale>();
        public List<Filiale> ListFiliale { get => listFiliale; set => SetProperty(ref listFiliale, value); }

        private int selectedFiliale;
        public int SelectedFiliale { get => selectedFiliale; set => SetProperty(ref selectedFiliale, value); }

        private List<Status> listStatus = new List<Status>();
        public List<Status> ListStatus { get => listStatus; set => SetProperty(ref listStatus, value); }

        //private int selectedStatus;
        //public int SelectedStatus { get => selectedStatus; set => SetProperty(ref selectedStatus, value); }


        private List<Unite> listUnite = new List<Unite>();
        public List<Unite> ListUnite { get => listUnite; set => SetProperty(ref listUnite, value); }

        private List<Tasks> listTask = new List<Tasks>();
        public List<Tasks> ListTask { get => listTask; set => SetProperty(ref listTask, value); }

        private DateTime dateStart = DateTime.Now;
        public DateTime DateStart { get => dateStart; set => SetProperty(ref dateStart, value); }

        private DateTime dateEnd = DateTime.Now;
        public DateTime DateEnd { get => dateEnd; set => SetProperty(ref dateEnd, value); }

        private TimeSpan hourStart = TimeSpan.Zero;
        public TimeSpan HourStart { get => hourStart; set => SetProperty(ref hourStart, value, onChanged: OnHourStartChanged); }

        private TimeSpan hourEnd = TimeSpan.Zero;
        public TimeSpan HourEnd { get => hourEnd; set => SetProperty(ref hourEnd, value, onChanged: OnHourEndChanged); }

        private int workingHours;
        public int WorkingHours { get => workingHours; set => SetProperty(ref workingHours, value, onChanged: OnWorkingHoursChanged); }

        private int workingMins;
        public int WorkingMins { get => workingMins; set => SetProperty(ref workingMins, value, onChanged: OnWorkingMinsChanged); }

        public ICommand OnClientFocusedCommand { get; private set; }
        public ICommand OnAddresseFocusedCommand { get; private set; }
        public ICommand OnStatusSelectedCommand { get; private set; }
        public ICommand AddSpeakerCommand { get; private set; }
        public ICommand DateStartUnFocusedCommand { get; private set; }
        public ICommand DateEndUnFocusedCommand { get; private set; }
        public ICommand ReCalculateWorkingTimeCommand { get; private set; }
        public ICommand FilialeSelectedCommand { get; private set; }
        public ICommand SelectTaskCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand PickPhotoCommand { get; private set; }
        public ICommand SignatureCommand { get; private set; }
        public ICommand ViewMediaCommand { get; private set; }
        public ICommand DeleteMediaCommand { get; private set; }
        public ICommand SaveIssueCommand { get; private set; }

        public class Status
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public Status(int id , string name)
            {
                Id = id;
                Name = name;
            }

        }

        public NewIssueViewModel(IIssueService issueService)
        {
            this.issueService = issueService;

            OnClientFocusedCommand = new AwaitCommand(OnClientFocused);
            OnAddresseFocusedCommand = new AwaitCommand(OnAddresseFocused);

            OnStatusSelectedCommand = new AwaitCommand(OnStatusSelected);
            DateStartUnFocusedCommand = new AwaitCommand(DateStartUnFocused);
            DateEndUnFocusedCommand = new AwaitCommand(DateEndUnFocused);
            ReCalculateWorkingTimeCommand = new AwaitCommand(ReCalculateWorking);
            FilialeSelectedCommand = new AwaitCommand<Filiale>(FilialeSelected);
        
            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);
            SaveIssueCommand = new AwaitCommand(SaveIssue);
        }

        private void OnStatusSelected(object obj, TaskCompletionSource<bool> tcs)
        {
            //TODO 
        }

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            initStatus();
            Mode = parameters?.GetValue<int>(ContentKey.ISSUE_MODE) ?? 0;

            CanEditClientAddress = Mode == 0 ? true : AppSettings.MobileCanEditClientAddress;

            selectedClient = parameters?.GetValue<Client>(ContentKey.SELECTED_CUSTOMER)?.DeepCopy();

            var issueOrganil = parameters?.GetValue<Issue>(ContentKey.SELECTED_ISSUE);
            Issue = parameters?.GetValue<Issue>(ContentKey.SELECTED_ISSUE)?.DeepCopy();

            if (Mode == 0)
            {
                AddIssue();
            }else
            {
                Issue.Client = issueOrganil.Client;
                Issue.Address = issueOrganil.Address;
                Issue.MediaLinks = issueOrganil.MediaLinks;
                Issue.Messages = issueOrganil.Messages;
                Issue.SelectedIndex = Issue.Status > 0 ? Issue.Status-1 : 0; //status 
                Issue.Priority = Issue.Priority !=0 ? Issue.Priority : 1 ;

            }

            Title = Mode != 0 ? (string.IsNullOrWhiteSpace(Issue?.Nom) ? TranslateExtension.GetValue("page_title_edit_issue") : Issue?.Nom) : TranslateExtension.GetValue("page_title_new_issue");

            ListFiliale.Add(new Filiale()
            {
                Id = Guid.NewGuid(),
                Code = -1,
                UserId = CurrentUser.Id,
                Nom = TranslateExtension.GetValue("none"),
                IsActif = 1
            });
            ListFiliale.AddRange(App.LocalDb.Table<Filiale>().ToList().FindAll(f => f.UserId == CurrentUser.Id && f.IsActif == 1).OrderBy(f => f.Code).ToList());
        }

        private void initStatus()
        {
            listStatus = new List<Status>();
            for(int i = 1; i < 7; i++)
            {
                switch (i) {
                    case 1:
                        listStatus.Add(new Status(i, "Nouveau"));
                        break;
                    case 2:
                        listStatus.Add(new Status(i, "En cours"));
                        break;

                    case 3:
                        listStatus.Add(new Status(i, "Attente de réponse"));
                        break;                       
                    case 4:
                        listStatus.Add(new Status(i, "Résolu"));
                        
                        break;
                    case 5:
                        listStatus.Add(new Status(i, "Fermé"));
                        break;
                    case 6:
                        listStatus.Add(new Status(i, "Rejeté"));
                        break;
                }
            }
        }

        private void AddIssue()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            UserDialogs.Instance.Loading(TranslateExtension.GetValue("message_loading")).Show();

            Task.Run(async () =>
            {
                var issue = new Issue()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    FkUserAppId = CurrentUser.Uuid,
                    FkUserServerlId = CurrentUser.Id,
                    UniteLinks = new List<UniteLink>(),
                    MediaLinks = new ObservableCollection<MediaLink>(),
                    Address = new Address(),
                    Client = new Client(),
                    Status = 1,
                    IsActif = 1,
                    Priority =1,
                    
                };
                //TODO 
                issue = await issueService.GetRelations(issue);

                return issue;
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(async () =>
            {
                UserDialogs.Instance.Loading().Hide();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    Issue = task.Result;
                    if(Issue.Address == null)
                    {
                        Issue.Address = new Address();
                    }
                }
                else if (task.IsFaulted)
                {
                    await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                }

                tcs.TrySetResult(true);
            }));
        }


        public override void OnPushed(NavigationParameters parameters)
        {
            base.OnPushed(parameters);

            InitIssue();

            MessagingCenter.Subscribe<ClientLookUpViewModel, Client>(this, MessageKey.CLIENT_SELECTED, OnClientSelected);
            MessagingCenter.Subscribe<AddressLookUpViewModel, Address>(this, MessageKey.ADDRESS_SELECTED, OnAddressSelected);
            MessagingCenter.Subscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);
        }

        public override void OnPopped()
        {
            base.OnPopped();

            MessagingCenter.Unsubscribe<ClientLookUpViewModel, Client>(this, MessageKey.CLIENT_SELECTED);
            MessagingCenter.Unsubscribe<AddressLookUpViewModel, Address>(this, MessageKey.ADDRESS_SELECTED);
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED);
        }

        private void InitIssue()
        {
            if (Mode != 0)
            {
                SelectedFiliale = ListFiliale.FindIndex(fi => (fi.ServerId > 0 && fi.ServerId == Issue.FkFilialeServerId) );

                if (SelectedFiliale < 0 && ListFiliale.Count > 0)
                {
                    SelectedFiliale = 0;

                    Issue.FilialeId = ListFiliale[SelectedFiliale].Id;
                    Issue.FkFilialeServerId = ListFiliale[SelectedFiliale].ServerId;

                    GetListUnite();
                   
                }
            }
            else
            {
                if (Issue == null)
                    Issue = new Issue();

                if (selectedClient != null)
                {
                    OnClientSelected(null, selectedClient);
                }

                if (ListFiliale.Count > 0)
                {
                    SelectedFiliale = 0;

                    Issue.FilialeId = ListFiliale[SelectedFiliale].Id;
                    Issue.FkFilialeServerId = ListFiliale[SelectedFiliale].ServerId;

                    GetListUnite();
                  
                }
            }

            InitDateTime();

            Issue.PropertyChanged += Issue_PropertyChanged;
        }

        private void InitDateTime()
        {
            try
            {
                if (Issue.DateStart.HasValue)
                    DateStart = Issue.DateStart.Value;
                if (Issue.DateEnd.HasValue)
                    DateEnd = Issue.DateEnd.Value;

                if (TimeSpan.TryParse(Issue.HourStart, out TimeSpan start) is bool t1)
                    HourStart = start;
                if (TimeSpan.TryParse(Issue.HourEnd, out TimeSpan end) is bool t2)
                    HourEnd = end;

                WorkingHours = Convert.ToInt32(Issue.HourStart.Split(':')[0]);
                WorkingMins = Convert.ToInt32(Issue.HourStart.Split(':')[1]);
            }
            catch {
             }
        }

        private void Issue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Issue.Status))
            {
                //TODO 
            }
        }

        private void OnDoneChanged()
        {
            if (Issue.Status == 1)
            {
                Issue.DateStart = Issue.DateStart ?? DateTime.Now;
                Issue.DateEnd = DateTime.Now;

                if (string.IsNullOrWhiteSpace(Issue.HourStart))
                {
                    if (Settings.LastDoneDate.Date == DateTime.Today && !string.IsNullOrWhiteSpace(Settings.LastDoneTime))
                    {
                        Issue.HourStart = Settings.LastDoneTime;
                    }
                    else if (AppSettings.MobileHourStartEnable && TimeSpan.TryParse(AppSettings.MobileHourStartDefault, out TimeSpan defaultTime) && defaultTime <= DateTime.Now.TimeOfDay)
                    {
                        Issue.HourStart = AppSettings.MobileHourStartDefault;
                    }
                    else
                    {
                        Issue.HourStart = DateTime.Now.ToString("HH:mm");
                    }
                }

                Issue.HourEnd = DateTime.Now.ToString("HH:mm");

                CalculateWorkingTime();
            }
            else
            {
                Issue.DateEnd = null;
                Issue.HourEnd = "";
               
            }

            InitDateTime();
        }

        private void CalculateWorkingTime()
        {
            if (Issue.DateStart.HasValue && Issue.DateEnd.HasValue
                && TimeSpan.TryParse(string.IsNullOrWhiteSpace(Issue.HourStart) ? "00:00" : Issue.HourStart, out TimeSpan t1)
                && TimeSpan.TryParse(string.IsNullOrWhiteSpace(Issue.HourEnd) ? "00:00" : Issue.HourEnd, out TimeSpan t2))
            {
                var wt = Issue.DateEnd.Value.Date.Add(t2).Subtract(Issue.DateStart.Value.Date.Add(t1));
                if (wt.Ticks > 0)
                    Issue.HourStart = string.Format("{0}:{1}", ((int)wt.TotalHours).ToString("00"), wt.Minutes.ToString("00"));
                else
                    Issue.HourStart = "00:00";
            }
            else
            {
                Issue.HourStart = "00:00";
            }

            try
            {
                WorkingHours = Convert.ToInt32(Issue.HourStart.Split(':')[0]);
                WorkingMins = Convert.ToInt32(Issue.HourStart.Split(':')[1]);
            }
            catch { }
        }

        private void DateStartUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Issue.DateStart = DateStart;

            tcs.TrySetResult(true);
        }

        private void DateEndUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Issue.DateEnd = DateEnd;

            tcs.TrySetResult(true);
        }

        private void OnHourStartChanged()
        {
            if (IsDisposing)
                return;

            Issue.HourStart = HourStart.ToString(@"hh\:mm");
        }

        private void OnHourEndChanged()
        {
            if (IsDisposing)
                return;

            Issue.HourEnd = HourEnd.ToString(@"hh\:mm");
        }

        private void OnWorkingHoursChanged()
        {
            if (IsDisposing)
                return;

            try
            {
                if (TimeSpan.TryParse(Issue.HourStart, out TimeSpan time))
                {
                    Issue.HourStart = (new TimeSpan(WorkingHours, time.Minutes, 0)).ToString(@"hh\:mm");
                }
            }
            catch { }
        }

        private int oldMins;

        private void OnWorkingMinsChanged()
        {
            if (IsDisposing)
                return;

            try
            {
                if (WorkingMins > 60)
                {
                    WorkingMins = oldMins;
                }
                else
                {
                    oldMins = WorkingMins;
                }

                if (TimeSpan.TryParse(Issue.HourStart, out TimeSpan time))
                {
                    Issue.HourStart = (new TimeSpan(time.Hours, WorkingMins, 0)).ToString(@"hh\:mm");
                }
            }
            catch { }
        }

        private void ReCalculateWorking(object sender, TaskCompletionSource<bool> tcs)
        {
            CalculateWorkingTime();

            tcs.TrySetResult(true);
        }

        private async void OnClientFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<ClientLookUpViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

        private void OnClientSelected(ClientLookUpViewModel sender, Client value)
        {
            if (Issue == null)
                Issue = new Issue();

            Issue.FkClientAppId = value.Id;
            Issue.FkClientServerId = value.ServerId;
            Issue.Client = value;

            if (App.LocalDb.Table<Address>().ToList().FindAll(a => ((a.FkClientServerId > 0 && a.FkClientServerId == Issue.Client.ServerId) || (!a.FkClientAppliId.Equals(Guid.Empty) && a.Id.Equals(Issue.Client.Id)))) is List<Address> addresses && addresses.Count > 0)
            {
                OnAddressSelected(null, addresses.First());
            }
            else
            {
                Issue.Address = null;
            }
        }

        private async void OnAddresseFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Issue.Client }
            };

            await CoreMethods.PushViewModel<AddressLookUpViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }

        private void OnAddressSelected(AddressLookUpViewModel sender, Address value)
        {
            Issue.FkAddressAppliId = value.Id;
            Issue.FkAddressServerId = value.ServerId;
            Issue.Address = value;
        }
        private async void AddSpeaker(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<UserLookUpViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

       
        private int i = 0;
        private void FilialeSelected(Filiale value, TaskCompletionSource<bool> tcs)
        {
            if (Issue == null)
            {
                tcs.TrySetResult(true);
                return;
            }

            if (i == 0 && mode != 0)
            {

            }
            else
            {
                Issue.FilialeId = value.Id;
                Issue.FkFilialeServerId = value.ServerId;
            }
            i++;
            GetListUnite();
  

            tcs.TrySetResult(true);
        }

        private void GetListUnite()
        {
            if (Issue == null)
                Issue = new Issue();

            List<Unite> result = new List<Unite>();

            if (ListFiliale.ElementAtOrDefault(SelectedFiliale) is Filiale filiale)
            {
                if (filiale.Nom != TranslateExtension.GetValue("none"))
                {
                    result = App.LocalDb.Table<Unite>().ToList().FindAll(un => un.UserId == CurrentUser.Id && (un.FilialeServerKey == 0 || un.FilialeServerKey == filiale.ServerId) && un.IsActif == 1).OrderBy(un => un.Position).ToList();
                    foreach (var un in result)
                    {
                        un.UniteItems = App.LocalDb.Table<UniteItem>().ToList().FindAll(uni => uni.UserId == CurrentUser.Id && ((uni.UniteServerId > 0 && uni.UniteServerId == un.ServerId) || (!uni.UniteAppId.Equals(Guid.Empty) && !un.Id.Equals(Guid.Empty) && uni.UniteAppId.Equals(un.Id))) && uni.IsActif == 1);
                    }
                }
                else
                {
                    result = App.LocalDb.Table<Unite>().ToList().FindAll(un => un.UserId == CurrentUser.Id && un.IsActif == 1 && un.FilialeServerKey == 0).OrderBy(un => un.Position).ToList();
                    foreach (var un in result)
                    {
                        un.UniteItems = App.LocalDb.Table<UniteItem>().ToList().FindAll(uni => uni.UserId == CurrentUser.Id && ((uni.UniteServerId > 0 && uni.UniteServerId == un.ServerId) || (!uni.UniteAppId.Equals(Guid.Empty) && !un.Id.Equals(Guid.Empty) && uni.UniteAppId.Equals(un.Id))) && uni.IsActif == 1);
                    }
                }
            }

            int pos = -1;
            foreach (var un in result)
            {
                pos++;
                un.postion_nosort = pos;
                un.Name = un.Nom;
            }

            if (Issue.UniteLinks == null)
                Issue.UniteLinks = new List<UniteLink>();
            foreach (var un in result)
            {

                if (Issue.UniteLinks.Find(ul => (ul.FkUniteServerId > 0 && ul.FkUniteServerId == un.ServerId) || (!ul.FkUniteAppliId.Equals(Guid.Empty) && !un.Id.Equals(Guid.Empty) && ul.FkUniteAppliId.Equals(un.Id))) is UniteLink uniteLink && !string.IsNullOrWhiteSpace(uniteLink.UniteValue))
                {
                    var tempValue = "";
                    if ((un.FieldType == 4 || un.FieldType == 10) && int.TryParse(uniteLink.UniteValue, out int n))
                    {
                        var uniteItem = App.LocalDb.Table<UniteItem>().FirstOrDefault(uni => uni.ServerId == n && uni.IsActif == 1);
                        tempValue = uniteItem.Value;
                    }
                    if (un.FieldType == 3 && int.TryParse(uniteLink.UniteValue, out int m))
                    {
                        var fieldType = m;
                    }

                    if (!"".Equals(tempValue))
                        un.Value = tempValue;
                    else
                        un.Value = uniteLink.UniteValue;

                }

                var children = result.FindAll(u => u.ParentId > 0 && u.ServerId != un.ServerId && un.ServerId == u.ParentId);

                if (children.Count > 0)
                {

                    if (children.Count == 1)
                        un.Name = un.Nom + " (" + children.Count + " child)";
                    else
                        un.Name = un.Nom + " (" + children.Count + " childrens)";
                    var idx = -1;
                    foreach (var child in children)
                    {
                        idx++;
                        child.Name = child.Nom + " (#" + un.Nom + ")";
                        child.IsVisible = !string.IsNullOrWhiteSpace(un.Value);

                        var indexParent = result.FindIndex(uni => uni.ServerId == child.ParentId);
                        var currentIndexChild = result.FindIndex(uni => uni.ServerId == child.ServerId);

                        //for(int i = indexParent+1; i < result.Count; i++)
                        //{
                        //    var update = result[i];
                        //    update.postion_nosort++;
                        //}

                        //update may thang sau len 1 

                        child.postion_nosort = result[indexParent].postion_nosort; //the same thing 

                    }
                }

                un.PropertyChanged += Un_PropertyChanged;
            }



            ListUnite = result.OrderBy(e => e.postion_nosort).ToList();

            //var test = ListUnite.Count;
        }

        private void Un_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                if (sender is Unite unite)
                {
                    var children = ListUnite.FindAll(un => un.ParentId > 0 && un.ServerId != unite.ServerId && un.ParentId == unite.ServerId);
                    foreach (var child in children)
                    {
                        if (unite.FieldType == 3)
                        {
                            child.IsVisible = unite?.Value.Equals("1") ?? false;
                        }
                        else
                        {
                            child.IsVisible = !string.IsNullOrWhiteSpace(child.Value);
                        }
                    }
                }
            }
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
            NewMedias.Add(mediaLink);

            Issue.OnPropertyChanged(nameof(Issue.MediaLinks));
        }

        private async void ViewMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_MEDIA, mediaLink}
                };

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
                    //mediaLink.IsDelete = true;

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
            if (NewMedias.Contains(mediaLink))
            {
                Issue.MediaLinks.Remove(mediaLink);
                NewMedias.Remove(mediaLink);
            }
            else
            {
                if (mediaLink.ServerId > 0)
                {
                    if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                    {
                        mediaLink.Media.IsActif = 0;
                        mediaLink.Media.IsToSync = true;
                    }

                    mediaLink.IsActif = 0;
                    mediaLink.IsToSync = true;
                }
                else
                {
                    Issue.MediaLinks.Remove(mediaLink);
                    DeletedMedias.Add(mediaLink);
                }
            }

            Issue.OnPropertyChanged(nameof(Issue.MediaLinks));

            tcs.TrySetResult(true);
        }

        private async void SaveIssue(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                Issue.Status = Issue.SelectedIndex + 1;
                if (Mode == 0)
                {
                    if (Issue.FkClientAppId.Equals(Guid.Empty) && Issue.FkClientServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_intervention_no_client"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }

                    if (Issue.FkAddressAppliId.Equals(Guid.Empty) && Issue.FkAddressServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_intervention_no_address"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }
                }

                UserDialogs.Instance.Loading(TranslateExtension.GetValue("Saving")).Show();

                if (Issue.Status == 1)
                {
                    if (Issue.DateEnd.HasValue)
                    {
                        Settings.LastDoneDate = Issue.DateEnd.Value;
                        if (!string.IsNullOrWhiteSpace(Issue.HourEnd))
                            Settings.LastDoneTime = Issue.HourEnd;
                    }

                    if (await LocationHelper.CheckLocationPermission(false) && LocationHelper.IsGeolocationAvailable(false) && LocationHelper.IsGeolocationEnabled(false))
                    {
                        var position = await LocationHelper.GetCurrentPosition(showOverlay: false);

                       
                    }
                }

                App.LocalDb.BeginTransaction();

                Issue.IsToSync = true;
               

                if (Mode != 0)
                {

                    if (Issue.UserId == 0)
                    {
                        Issue.UserId = CurrentUser.Id;
                    }
                    Issue.EditDate = DateTime.Now;

                    if (!await issueService.UpdateIssue(Issue))
                    {
                        tcs.TrySetResult(true);
                        return;
                    }

                    var newIssue = issueService.GetIssueDetail(Issue.Id);
                    
                }
                else
                {
                    if (AppSettings.MobilePreremplirPlanningDate)
                    {
                        Issue.DateStart = Issue.DateStart ?? DateTime.Now;
                        Issue.DateEnd = DateTime.Now;
                    }

                    Issue.AddDate = DateTime.Now;

                    if (!await issueService.CreateIssue(Issue))
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }

                foreach (var un in ListUnite)
                {
                    if (Issue.UniteLinks != null && Issue.UniteLinks.Find(ul => (ul.FkUniteServerId > 0 && ul.FkUniteServerId == un.ServerId) || (!ul.FkUniteAppliId.Equals(Guid.Empty) && !un.Id.Equals(Guid.Empty) && ul.FkUniteAppliId.Equals(un.Id))) is UniteLink uniteLink)
                    {
                        if (un.FieldType == 10)
                        {
                            un.Value = "";
                            foreach (var ui in un.UniteItems)
                            {
                                if (ui.Selected)
                                {
                                    un.Value += string.IsNullOrWhiteSpace(un.Value) ? ui.ServerId + "" : ("; " + ui.ServerId);
                                    un.ValueUI += string.IsNullOrWhiteSpace(un.ValueUI) ? ui.Value + "" : ("; " + ui.Value);
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(un.Value))
                        {
                            uniteLink.UniteValueUI = un.ValueUI;
                            uniteLink.UniteValue = un.Value;
                            uniteLink.IsActif = 1;
                        }
                        else
                        {
                            uniteLink.UniteValueUI = un.ValueUI;
                            uniteLink.UniteValue = un.Value;
                            uniteLink.IsActif = 0;
                        }
                        uniteLink.EditDate = DateTime.Now;
                        uniteLink.IsToSync = true;
                    }
                    else
                    {
                        if (un.FieldType == 10)
                        {
                            un.Value = "";
                            foreach (var ui in un.UniteItems)
                            {
                                if (ui.Selected)
                                {
                                    un.Value += string.IsNullOrWhiteSpace(un.Value) ? ui.ServerId + "" : ("; " + ui.ServerId);
                                    un.ValueUI += string.IsNullOrWhiteSpace(un.ValueUI) ? ui.Value + "" : ("; " + ui.Value);
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(un.Value))
                        {
                            Issue.UniteLinks.Add(new UniteLink()
                            {
                                Id = Guid.NewGuid(),
                                UserId = CurrentUser.Id,
                                FkColumnAppliId = Issue.Id,
                                FkColumnServerId = Issue.ServerId,
                                LinkTableName = "issue",
                                FkUniteAppliId = un.Id,
                                FkUniteServerId = un.ServerId,
                                UniteValue = un.Value,
                                UniteValueUI = un.ValueUI,
                                IsActif = 1,
                                AddDate = DateTime.Now,
                                EditDate = DateTime.Now,
                                IsToSync = true
                            });
                        }
                    }
                }




                foreach (var ul in Issue.UniteLinks)
                {
                    App.LocalDb.InsertOrReplace(ul);
                }


                foreach (var medl in Issue.MediaLinks)
                {
                    if (medl.Media != null)
                    {
                        App.LocalDb.InsertOrReplace(medl.Media);
                    }
                    App.LocalDb.InsertOrReplace(medl);
                }

                foreach (var medl in DeletedMedias)
                {
                    if (medl.Media != null)
                    {
                        App.LocalDb.Delete(medl.Media);
                    }
                    App.LocalDb.Delete(medl);
                }

                App.LocalDb.Commit();

                MessagingCenter.Send(this, MessageKey.ISSUE_CHANGED, Issue);
                MessagingCenter.Send(this, MessageKey.ISSUE_CHANGED);

                await CoreMethods.PopViewModel();
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));

                App.LocalDb.Rollback();
            }
            finally
            {
                tcs.TrySetResult(true);
                UserDialogs.Instance.Loading().Hide();
            }
        }
    }
}
