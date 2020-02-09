using Acr.UserDialogs;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Models.Response;
using Organilog.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Interventions
{
    public class NewInterventionViewModel : BaseViewModel
    {
        private readonly IInterventionService interventionService;

        private readonly LoginResponse CurrentUser = Settings.CurrentUser;

        private Client selectedClient;
        private Address selectedAddress;
        private Equipment selectedEquipment;
        private Product selectedProduct;

        private List<Intervention> NewChildren = new List<Intervention>();
        private List<Intervention> DeletedChildren = new List<Intervention>();

        private List<LinkInterventionProduct> NewProducts = new List<LinkInterventionProduct>();
        private List<LinkInterventionProduct> DeletedProducts = new List<LinkInterventionProduct>();

        private List<LinkInterventionEquipment> NewEquipments = new List<LinkInterventionEquipment>();
        private List<LinkInterventionEquipment> DeletedEquipments = new List<LinkInterventionEquipment>();

        private List<MediaLink> NewMedias = new List<MediaLink>();
        private List<MediaLink> DeletedMedias = new List<MediaLink>();

        private int mode; // 0: Add 1: Edit
        public int Mode { get => mode; set => SetProperty(ref mode, value); }

        private bool canEditClientAddress;
        public bool CanEditClientAddress { get => canEditClientAddress; set => SetProperty(ref canEditClientAddress, value); }

        private Intervention intervention;
        public Intervention Intervention { get => intervention; set => SetProperty(ref intervention, value); }

        private ObservableCollection<User> speakers = new ObservableCollection<User>();
        public ObservableCollection<User> Speakers { get => speakers; set => SetProperty(ref speakers, value); }

        private List<Filiale> listFiliale = new List<Filiale>();
        public List<Filiale> ListFiliale { get => listFiliale; set => SetProperty(ref listFiliale, value); }

        private int selectedFiliale;
        public int SelectedFiliale { get => selectedFiliale; set => SetProperty(ref selectedFiliale, value); }

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

        private string nonWorking;
        public string NonWorking { get => nonWorking; set => SetProperty(ref nonWorking, value); }

        public ICommand OnClientFocusedCommand { get; private set; }
        public ICommand OnAddresseFocusedCommand { get; private set; }
        public ICommand OnContractFocusedCommand { get; private set; }
        public ICommand OnGroupFocusedCommand { get; private set; }
        
        public ICommand AddSpeakerCommand { get; private set; }
        public ICommand DeleteSpeakerCommand { get; private set; }
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
        public ICommand SaveInterventionCommand { get; private set; }
        public ICommand AddEquipmentCommand { get; private set; }
        public ICommand DeleteEquipmentCommand { get; private set; }


        public NewInterventionViewModel(IInterventionService interventionService)
        {
            this.interventionService = interventionService;

            OnClientFocusedCommand = new AwaitCommand(OnClientFocused);
            OnAddresseFocusedCommand = new AwaitCommand(OnAddresseFocused);
            OnContractFocusedCommand = new AwaitCommand(OnContractFocused);
            OnGroupFocusedCommand = new AwaitCommand(OnGroupFocused);
            AddSpeakerCommand = new AwaitCommand(AddSpeaker);
            DeleteSpeakerCommand = new AwaitCommand<Intervention>(DeleteSpeaker);
            DateStartUnFocusedCommand = new AwaitCommand(DateStartUnFocused);
            DateEndUnFocusedCommand = new AwaitCommand(DateEndUnFocused);
            ReCalculateWorkingTimeCommand = new AwaitCommand(ReCalculateWorking);
            FilialeSelectedCommand = new AwaitCommand<Filiale>(FilialeSelected);
            SelectTaskCommand = new AwaitCommand<Tasks>(SelectTask);
            AddEquipmentCommand = new AwaitCommand(AddEquipment);
            AddProductCommand = new AwaitCommand(AddProduct);
            DeleteProductCommand = new AwaitCommand<LinkInterventionProduct>(DeleteProduct);
            DeleteEquipmentCommand = new AwaitCommand<LinkInterventionEquipment>(DeleteEquipment);
            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);
            SaveInterventionCommand = new AwaitCommand(SaveIntervention);
           
        }

        

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Mode = parameters?.GetValue<int>(ContentKey.INTERVENTION_MODE) ?? 0;

            CanEditClientAddress = Mode == 0 ? true : AppSettings.MobileCanEditClientAddress;

            selectedClient = parameters?.GetValue<Client>(ContentKey.SELECTED_CUSTOMER)?.DeepCopy();
            selectedAddress = parameters?.GetValue<Address>(ContentKey.SELECTED_ADDRESS)?.DeepCopy();

            selectedEquipment = parameters?.GetValue<Equipment>(ContentKey.SELECTED_EQUIPMENT)?.DeepCopy();
            selectedProduct = parameters?.GetValue<Product>(ContentKey.SELECTED_PRODUCT)?.DeepCopy();

            Intervention = parameters?.GetValue<Intervention>(ContentKey.SELECTED_INTERVENTION)?.DeepCopy();

            if (Intervention == null)
            {
                AddIntervention();
            }

            Title = Mode != 0 ? (string.IsNullOrWhiteSpace(Intervention?.Nom) ? TranslateExtension.GetValue("page_title_edit_intervention") : Intervention?.Nom) : TranslateExtension.GetValue("page_title_new_intervention");

            ListFiliale.Add(new Filiale()
            {
                Id = Guid.NewGuid(),
                Code = -1,
                UserId = CurrentUser.Id,
                Nom = TranslateExtension.GetValue("none"),
                IsActif = 1
            });
            ListFiliale.AddRange(App.LocalDb.Table<Filiale>().ToList().FindAll(f => f.UserId == CurrentUser.Id && f.IsActif == 1).OrderBy(f => f.Code).ToList());
            NonWorking = "00:00";

        }

        private void AddIntervention()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            UserDialogs.Instance.Loading(TranslateExtension.GetValue("message_loading")).Show();

            Task.Run(async () =>
            {
                var intervention = new Intervention()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    FkUserAppId = CurrentUser.Uuid,
                    FkUserServerlId = CurrentUser.Id,
                    LinkInterventionTasks = new List<LinkInterventionTask>(),
                    UniteLinks = new List<UniteLink>(),
                    LinkInterventionProducts = new ObservableCollection<LinkInterventionProduct>(),
                    LinkInterventionEquipments = new ObservableCollection<LinkInterventionEquipment>(), 
                    MediaLinks = new ObservableCollection<MediaLink>(),
                    IsActif = 1
                };

                intervention = await interventionService.GetRelations(intervention);
                


                return intervention;
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(async () =>
            {
                UserDialogs.Instance.Loading().Hide();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    Intervention = task.Result;
                    if (selectedClient != null)
                        OnClientSelected(null, selectedClient);

                    if (selectedEquipment != null)
                        OnEquipmentSelected(null, selectedEquipment);

                    if (selectedProduct != null)
                        OnProductSelected(null, selectedProduct);

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

            InitIntervention();

            MessagingCenter.Subscribe<ClientLookUpViewModel, Client>(this, MessageKey.CLIENT_SELECTED, OnClientSelected);
            MessagingCenter.Subscribe<AddressLookUpViewModel, Address>(this, MessageKey.ADDRESS_SELECTED, OnAddressSelected);
            MessagingCenter.Subscribe<ContractLookUpViewModel, Contract>(this, MessageKey.CONTRACT_SELECTED, OnContractSelected);
            MessagingCenter.Subscribe<GroupLookUpViewModel, Category>(this, MessageKey.GROUP_SELECTED, OnGroupSelected);
            MessagingCenter.Subscribe<UserLookUpViewModel, User>(this, MessageKey.USER_SELECTED, OnUserSelected);
            MessagingCenter.Subscribe<ProductLookUpViewModel, Product>(this, MessageKey.PRODUCT_SELECTED, OnProductSelected);
            MessagingCenter.Subscribe<EquipmentLookUpViewModel, Equipment>(this, MessageKey.EQUIPMENT_SELECTED, OnEquipmentSelected);
            MessagingCenter.Subscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);
        }

        public override void OnPopped()
        {
            base.OnPopped();

            MessagingCenter.Unsubscribe<ClientLookUpViewModel, Client>(this, MessageKey.CLIENT_SELECTED);
            MessagingCenter.Unsubscribe<AddressLookUpViewModel, Address>(this, MessageKey.ADDRESS_SELECTED);
            MessagingCenter.Unsubscribe<UserLookUpViewModel, User>(this, MessageKey.USER_SELECTED);
            MessagingCenter.Unsubscribe<ProductLookUpViewModel, Product>(this, MessageKey.PRODUCT_SELECTED);
            MessagingCenter.Unsubscribe<ProductLookUpViewModel, Product>(this, MessageKey.EQUIPMENT_SELECTED);
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED);
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.GROUP_SELECTED);
            MessagingCenter.Unsubscribe<MediaDetailViewModel, MediaLink>(this, MessageKey.CONTRACT_SELECTED);
        }

        private void InitIntervention()
        {
            if (Mode != 0)
            {
                SelectedFiliale = ListFiliale.FindIndex(fi => (fi.ServerId > 0 && fi.ServerId == Intervention.FkFilialeServerId) || (!fi.Id.Equals(Guid.Empty) && fi.Id.Equals(Intervention.FkFilialeAppId)));

                if (SelectedFiliale < 0 && ListFiliale.Count > 0)
                {
                    SelectedFiliale = 0;

                    Intervention.FkFilialeAppId = ListFiliale[SelectedFiliale].Id;
                    Intervention.FkFilialeServerId = ListFiliale[SelectedFiliale].ServerId;
                    GetLitProduct();
                    GetListUnite();
                    GetListTask();
                }
            }
            else
            {
                if (Intervention == null)
                    Intervention = new Intervention();

                if (selectedClient != null)
                {
                    OnClientSelected(null, selectedClient);
                }

                if (ListFiliale.Count > 0)
                {
                    SelectedFiliale = 0;

                    Intervention.FkFilialeAppId = ListFiliale[SelectedFiliale].Id;
                    Intervention.FkFilialeServerId = ListFiliale[SelectedFiliale].ServerId;

                    GetListUnite();
                    GetListTask();
                }
            }

            InitDateTime();

            Intervention.PropertyChanged += Intervention_PropertyChanged;
        }

        private void GetLitProduct()
        {
            //send list product 
            /*
            if (Intervention.LinkInterventionProducts != null)
            {
                foreach(var productLine in Intervention.LinkInterventionProducts)
                {
                    productLine.Product = App.LocalDb.Table<Product>().FirstOrDefault(p=>p.ServerId==productLine.FkProductServerId);

                }
            }*/
        }

        private void InitDateTime()
        {
            try
            {
                if (Intervention.DoneDateStart.HasValue)
                    DateStart = Intervention.DoneDateStart.Value;
                if (Intervention.DoneDateEnd.HasValue)
                    DateEnd = Intervention.DoneDateEnd.Value;

                if (TimeSpan.TryParse(Intervention.DoneHourStart, out TimeSpan start) is bool t1)
                    HourStart = start;
                if (TimeSpan.TryParse(Intervention.DoneHourEnd, out TimeSpan end) is bool t2)
                    HourEnd = end;

                WorkingHours = Convert.ToInt32(Intervention.DoneHour.Split(':')[0]);
                WorkingMins = Convert.ToInt32(Intervention.DoneHour.Split(':')[1]);
            }
            catch {
 }
        }

        private void Intervention_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Intervention.IsDone))
            {
                OnDoneChanged();
            }else if(e.PropertyName == nameof(Intervention.DoneHourEnd)|| e.PropertyName == nameof(Intervention.DoneHourStart))
            {
                CalculateWorkingTime();
            }else if(e.PropertyName == nameof(Intervention.Client))
            {
                bool test = false;
            }
        }

        bool isNeedWait = false;
        private void OnDoneChanged()
        {
            if (Intervention.IsDone == 1)
            {
                Intervention.DoneDateStart = Intervention.DoneDateStart ?? DateTime.Now;
                Intervention.DoneDateEnd = DateTime.Now;

                if (string.IsNullOrWhiteSpace(Intervention.DoneHourStart))
                {
                    if (Settings.LastDoneDate.Date == DateTime.Today && !string.IsNullOrWhiteSpace(Settings.LastDoneTime))
                    {
                        Intervention.DoneHourStart = Settings.LastDoneTime;
                    }
                    else if (AppSettings.MobileHourStartEnable && TimeSpan.TryParse(AppSettings.MobileHourStartDefault, out TimeSpan defaultTime) && defaultTime <= DateTime.Now.TimeOfDay)
                    {
                        Intervention.DoneHourStart = AppSettings.MobileHourStartDefault;

                    }
                    else
                    {
                        Intervention.DoneHourStart = DateTime.Now.ToString("HH:mm");
                    }
                }

                if (TimeSpan.TryParse(Intervention.DoneHourStart, out TimeSpan start))
                    HourStart = start;

                Intervention.DoneHourEnd = DateTime.Now.ToString("HH:mm");

                if (TimeSpan.TryParse(Intervention.DoneHourEnd, out TimeSpan end))
                    HourEnd = end;

                CalculateWorkingTime();
            }
            else
            {
                Intervention.DoneDateEnd = null;
                Intervention.DoneHourEnd = "";
                Intervention.DoneHour = "00:00";
            }

            InitDateTime();
        }

        private void CalculateWorkingTime()
        {
            if (
                 TimeSpan.TryParse(string.IsNullOrWhiteSpace(Intervention.DoneHourStart) ? "00:00" : Intervention.DoneHourStart, out TimeSpan t1)
                && TimeSpan.TryParse(string.IsNullOrWhiteSpace(Intervention.DoneHourEnd) ? "00:00" : Intervention.DoneHourEnd, out TimeSpan t2))
            {
                //calculate for setting time break 
                TimeSpan brTime, brTimeStop;
               
                brTime = TimeSpan.Parse(AppSettings.MobileHourStartPauseDefault);
                brTimeStop = TimeSpan.Parse(AppSettings.MobileHourEndPauseDefault);

                TimeSpan wt;

                if (Intervention.DoneDateStart.HasValue && Intervention.DoneDateEnd.HasValue) { 
                
                    wt = Intervention.DoneDateEnd.Value.Date.Add(t2).Subtract(Intervention.DoneDateStart.Value.Date.Add(t1));
                    if (t2.Subtract(brTimeStop).Ticks > 0) { 
                        if(t1.Subtract(brTimeStop).Ticks<0)
                            wt = wt.Subtract(brTimeStop.Subtract(brTime));

                    }else if (t2 >= brTime && t2 <= brTimeStop)
                        wt = wt.Subtract(t2.Subtract(brTime));
                }
                else { 
                    wt = t2.Subtract(t1);
                    if (t2.Subtract(brTimeStop).Ticks > 0){
                        if (t1.Subtract(brTimeStop).Ticks < 0)
                            wt = wt.Subtract(brTimeStop.Subtract(brTime));

                    }else if (t2 >= brTime && t2 <= brTimeStop)
                        wt = wt.Subtract(t2.Subtract(brTime));

                }
                if (wt.Ticks > 0)
                    Intervention.DoneHour = string.Format("{0}:{1}", ((int)wt.TotalHours).ToString("00"), wt.Minutes.ToString("00"));
                else
                    Intervention.DoneHour = "00:00";
            }
            else
            {
                Intervention.DoneHour = "00:00";
            }

            try
            {
                WorkingHours = Convert.ToInt32(Intervention.DoneHour.Split(':')[0]);
                WorkingMins = Convert.ToInt32(Intervention.DoneHour.Split(':')[1]);
            }
            catch { }
        }

        private void DateStartUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Intervention.DoneDateStart = DateStart;

            tcs.TrySetResult(true);
        }

        private void DateEndUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Intervention.DoneDateEnd = DateEnd;

            tcs.TrySetResult(true);
        }

        private void OnHourStartChanged()
        {
            if (IsDisposing)
                return;

            Intervention.DoneHourStart = HourStart.ToString(@"hh\:mm");
        }

        private void OnHourEndChanged()
        {
            if (IsDisposing)
                return;

            Intervention.DoneHourEnd = HourEnd.ToString(@"hh\:mm");
        }

        private void OnWorkingHoursChanged()
        {
            if (IsDisposing)
                return;

            try
            {
                if (TimeSpan.TryParse(Intervention.DoneHour, out TimeSpan time))
                {
                    Intervention.DoneHour = (new TimeSpan(WorkingHours, time.Minutes, 0)).ToString(@"hh\:mm");
                }
            }
            catch { }
            reCalculateWorkingHour();
        }

        private void reCalculateWorkingHour()
        {
            
            if (!string.IsNullOrEmpty(Intervention.DoneHour) && Intervention.DoneHourStart != null && Intervention.DoneHourEnd != null)
            {

                if (
                TimeSpan.TryParse(string.IsNullOrWhiteSpace(Intervention.DoneHourStart) ? "00:00" : Intervention.DoneHourStart, out TimeSpan t1)
               && TimeSpan.TryParse(string.IsNullOrWhiteSpace(Intervention.DoneHourEnd) ? "00:00" : Intervention.DoneHourEnd, out TimeSpan t2)
               && TimeSpan.TryParse(string.IsNullOrWhiteSpace(Intervention.DoneHour) ? "00:00" : Intervention.DoneHour, out TimeSpan t3))
                {
                    if(t2>t1)
                         NonWorking = t2.Subtract(t1).Subtract(t3).ToString(@"hh\:mm");
                }
                    //var splitstringStart = Intervention.DoneHourStart.Split(':');
                    // var splitstringEnd = Intervention.DoneHourEnd.Split(':');
                    // var splitstringworking = Intervention.DoneHour.Split(':');

                    /* if (splitstringStart != null && splitstringStart.Length > 0 && splitstringEnd != null && splitstringEnd.Length > 0)
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
                         if (realHour < 0) realHour = 0;
                         if (realMin < 0) realMin = 0;

                         NonWorking = string.Format("{0}:{1}", realHour <= 0 ? "00" : realHour +"" , realMin > 9 ? "" + realMin : "0" + realMin);

                     }*/

            }
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

                if (TimeSpan.TryParse(Intervention.DoneHour, out TimeSpan time))
                {
                    Intervention.DoneHour = (new TimeSpan(time.Hours, WorkingMins, 0)).ToString(@"hh\:mm");
                }
            }
            catch { }
            reCalculateWorkingHour();
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
            if (Intervention == null)
                Intervention = new Intervention();

            Intervention.FkClientAppId = value.Id;
            Intervention.FkClientServerId = value.ServerId;
            Intervention.Client = value;

            if (selectedAddress != null)
            {
                OnAddressSelected(null, selectedAddress);
            }
            else { 
                if (App.LocalDb.Table<Address>().ToList().FindAll(a => ((a.FkClientServerId > 0 && a.FkClientServerId == Intervention.Client.ServerId) || (!a.FkClientAppliId.Equals(Guid.Empty) && a.Id.Equals(Intervention.Client.Id)))) is List<Address> addresses && addresses.Count > 0)
                {
                    OnAddressSelected(null, addresses.First());
                }
                else
                {
                    Intervention.Address = null;
                }
            }
            Intervention.OnPropertyChanged(nameof(Intervention.Client));
            Intervention.OnPropertyChanged(nameof(Intervention.Address));
        }

        private async void OnAddresseFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Intervention.Client }
            };

            await CoreMethods.PushViewModel<AddressLookUpViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }

        private void OnAddressSelected(AddressLookUpViewModel sender, Address value)
        {
            Intervention.FkAddressAppId = value.Id;
            Intervention.FkAddressServerId = value.ServerId;
            Intervention.Address = value;
            Intervention.OnPropertyChanged(nameof(Intervention.Address));
        }

        private async void OnGroupFocused(object arg1, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Intervention.Client }
            };
            await CoreMethods.PushViewModel<GroupLookUpViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }

        private async void OnContractFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Intervention.Client }
            };

            await CoreMethods.PushViewModel<ContractLookUpViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }

        private void OnContractSelected(object sender, Contract e)
        {
            Intervention.FkContratAppId = e.Id;
            Intervention.FkContratServerId = e.ServerId;
            Intervention.Contract = e;
        }
        private void OnGroupSelected(object sender, Category e)
        {
            Intervention.FkGroupAppId = e.Id;
            Intervention.FkGroupServerId = e.ServerId;
            Intervention.Category = e;
        }

        private async void AddSpeaker(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<UserLookUpViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

        private void OnUserSelected(object sender, User user)
        {
            if (Intervention.ChildInterventions.ToList().Exists(ch => (ch.FkUserServerlId > 0 && ch.FkUserServerlId == user.ServerId) || (!ch.FkUserAppId.Equals(Guid.Empty) && !user.Id.Equals(Guid.Empty) && ch.FkUserAppId.Equals(user.Id))))
            {
                CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_user_already_exist"), TranslateExtension.GetValue("ok"));
                return;
            }

            var child = Intervention.DeepCopy();

            child.Id = Guid.NewGuid();
            child.ServerId = 0;
            child.Code = 0;
            child.FkUserAppId = user.Id;
            child.FkUserServerlId = user.ServerId;
            child.FkParentAppId = Intervention.Id;
            child.FkParentServerlId = Intervention.ServerId;

            child.User = user;
            child.IsToSync = true;

            Intervention.ChildInterventions.Add(child);
            NewChildren.Add(child);
        }

        private void DeleteSpeaker(Intervention child, TaskCompletionSource<bool> tcs)
        {
            if (NewChildren.Contains(child))
            {
                Intervention.ChildInterventions.Remove(child);
                NewChildren.Remove(child);
            }
            else
            {
                if (child.ServerId > 0)
                {
                    child.SetProperty(nameof(child.IsActif), 0);
                    child.IsToSync = true;
                }
                else
                {
                    Intervention.ChildInterventions.Remove(child);
                    DeletedChildren.Add(child);
                }
            }

            tcs.TrySetResult(true);
        }
        private int i = 0;
        private void FilialeSelected(Filiale value, TaskCompletionSource<bool> tcs)
        {
            if (Intervention == null)
            {
                tcs.TrySetResult(true);
                return;
            }

            if (i == 0 && mode != 0)
            {

            }
            else
            {
                Intervention.FkFilialeAppId = value.Id;
                Intervention.FkFilialeServerId = value.ServerId;
            }
            i++;

            GetListUnite();
            GetListTask();

            tcs.TrySetResult(true);
        }

        private void GetListUnite()
        {
            if (Intervention == null)
                Intervention = new Intervention();

            List<Unite> result = new List<Unite>();

            if (ListFiliale.ElementAtOrDefault(SelectedFiliale) is Filiale filiale)
            {
                if (filiale.Nom != TranslateExtension.GetValue("none"))
                {
                    if(AppSettings.MobileVisibleWithFilialeZero)
                        result = App.LocalDb.Table<Unite>().ToList().FindAll(un => un.UserId == CurrentUser.Id && (un.FilialeServerKey == 0 || un.FilialeServerKey == filiale.ServerId) && un.IsActif == 1).OrderBy(un => un.Position).ToList();
                    else
                        result = App.LocalDb.Table<Unite>().ToList().FindAll(un => un.UserId == CurrentUser.Id && (un.FilialeServerKey == filiale.ServerId) && un.IsActif == 1).OrderBy(un => un.Position).ToList();
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

            if (Intervention.UniteLinks == null)
                Intervention.UniteLinks = new List<UniteLink>();
            foreach (var un in result)
            {

                if (Intervention.UniteLinks.Find(ul => (ul.FkUniteServerId > 0 && ul.FkUniteServerId == un.ServerId) || (!ul.FkUniteAppliId.Equals(Guid.Empty) && !un.Id.Equals(Guid.Empty) && ul.FkUniteAppliId.Equals(un.Id))) is UniteLink uniteLink && !string.IsNullOrWhiteSpace(uniteLink.UniteValue))
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
                        un.Name = un.Nom + " (" + children.Count + " sous-champ)";
                    else
                        un.Name = un.Nom + " (" + children.Count + " sous-champs)";
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

        private void GetListTask()
        {
            List<Tasks> result = new List<Tasks>();
            if (Intervention.LinkInterventionTasks == null)
                Intervention.LinkInterventionTasks = new List<LinkInterventionTask>();
            if (ListFiliale.ElementAtOrDefault(SelectedFiliale) is Filiale filiale)
            {
                if (filiale.Nom != TranslateExtension.GetValue("none"))
                {
                    if(AppSettings.MobileVisibleWithFilialeZero)
                        result = App.LocalDb.Table<Tasks>().ToList().FindAll(ta => ta.UserId == CurrentUser.Id && (ta.FilialeServerKey == 0 || ta.FilialeServerKey == filiale.ServerId) && ta.IsActif == 1);
                    else  //show only filiale with server key.
                        result = App.LocalDb.Table<Tasks>().ToList().FindAll(ta => ta.UserId == CurrentUser.Id && ta.FilialeServerKey == filiale.ServerId && ta.IsActif == 1);

                }
                else
                {
                    result = App.LocalDb.Table<Tasks>().ToList().FindAll(ta => ta.UserId == CurrentUser.Id && ta.IsActif == 1 && ta.FilialeServerKey == 0);
                }
            }

            foreach (var ta in result)
            {
                if (Intervention.LinkInterventionTasks.Find(lit =>lit.IsDone == 1 && (lit.FkTaskServerId > 0 && lit.FkTaskServerId == ta.ServerId) || (!lit.FkTaskAppliId.Equals(Guid.Empty) && !ta.Id.Equals(Guid.Empty) && lit.FkTaskAppliId.Equals(ta.Id))) is LinkInterventionTask linkInterventionTask)
                {
                        ta.Selected = true;
                        ta.Minute = linkInterventionTask.DoneMinute;

                }
            }

            ListTask = result;
        }

        private void SelectTask(Tasks selectedTask, TaskCompletionSource<bool> tcs)
        {
            if (selectedTask.Selected)
            {
                if (selectedTask.Minute == 0)
                {
                    //if (TimeSpan.TryParse(Intervention.DoneHour, out TimeSpan workingTime))
                    //{
                    //    selectedTask.Minute = Convert.ToInt32(workingTime.TotalMinutes);

                    //}
                    //else
                    //{
                        selectedTask.Minute = 0;
                    //}
                }
            }
            else
            {
                selectedTask.Minute = 0;
            }

            tcs.TrySetResult(true);
        }
        private async void AddEquipment(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Intervention.Client }
            };

            await CoreMethods.PushViewModel<EquipmentLookUpViewModel>(@params,modal: true);

            tcs.TrySetResult(true);
        }
        private async void AddProduct(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<ProductLookUpViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

        private void OnProductSelected(ProductLookUpViewModel sender, Product value)
        {
            var linkInterventionProduct = new LinkInterventionProduct()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkInterventionAppliId = Intervention.Id,
                FkInterventionServerId = Intervention.ServerId,
                FkProductId = value.Id,
                FkProductServerId = value.ServerId,
                ProductName = value.Nom,
                Currency = value.Currency,
                PriceTax = Public.GetDecimal(value.PriceTax),
                PriceNotTax = Public.GetDecimal(value.Price),
                Quantity = value.Quantity,
                Position = Intervention.LinkInterventionProducts.Count,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                IsActif = 1,
                Product = value,
                IsToSync = true
            };

            Intervention.LinkInterventionProducts.Add(linkInterventionProduct);
            NewProducts.Add(linkInterventionProduct);
        }

        private void OnEquipmentSelected(EquipmentLookUpViewModel sender, Equipment value)
        {
            var linkInterventionEquipment = new LinkInterventionEquipment()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkInterventionAppliId = Intervention.Id,
                FkInterventionServerId = Intervention.ServerId,
                FkEquipmentAppliId = value.Id,
                FkEquipmentServerId = value.ServerId,
                EquipmentName = value.Title,

                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                IsActif = 1,
                Equipment = value,
                IsToSync = true
            };

            Intervention.LinkInterventionEquipments.Add(linkInterventionEquipment);
            NewEquipments.Add(linkInterventionEquipment);
            Intervention.OnPropertyChanged(nameof(Intervention.LinkInterventionEquipments));
        }


        private void DeleteProduct(LinkInterventionProduct value, TaskCompletionSource<bool> tcs)
        {
            if (NewProducts.Contains(value))
            {
                Intervention.LinkInterventionProducts.Remove(value);
                NewProducts.Remove(value);
            }
            else
            {
                if (value.ServerId > 0)
                {
                    value.SetProperty(nameof(value.IsActif), 0);
                    value.IsToSync = true;
                }
                else
                {
                    Intervention.LinkInterventionProducts.Remove(value);
                    DeletedProducts.Add(value);
                }
            }

            tcs.TrySetResult(true);
        }

        private void DeleteEquipment(LinkInterventionEquipment value, TaskCompletionSource<bool> tcs)
        {
            if (NewEquipments.Contains(value))
            {
                Intervention.LinkInterventionEquipments.Remove(value);
                NewEquipments.Remove(value);
            }
            else
            {
                if (value.ServerId > 0)
                {
                    value.SetProperty(nameof(value.IsActif), 0);
                    value.IsToSync = true;
                }
                else
                {
                    Intervention.LinkInterventionEquipments.Remove(value);
                    DeletedEquipments.Add(value);
                }
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
            NewMedias.Add(mediaLink);

            Intervention.OnPropertyChanged(nameof(Intervention.MediaLinks));
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

                    Intervention.MediaLinks.Remove(mediaLink);
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
            if (NewMedias.Contains(mediaLink))
            {
                Intervention.MediaLinks.Remove(mediaLink);
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
                    Intervention.MediaLinks.Remove(mediaLink);
                    DeletedMedias.Add(mediaLink);
                }
            }

            Intervention.OnPropertyChanged(nameof(Intervention.MediaLinks));

            tcs.TrySetResult(true);
        }

        private async void SaveIntervention(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                if (Mode == 0)
                {
                    if (Intervention.FkClientAppId.Equals(Guid.Empty) && Intervention.FkClientServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_intervention_no_client"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }

                    if (Intervention.FkAddressAppId.Equals(Guid.Empty) && Intervention.FkAddressServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_intervention"), TranslateExtension.GetValue("alert_message_intervention_no_address"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }
                }

                //UserDialogs.Instance.Loading(TranslateExtension.GetValue("Saving")).Show();

                if (Intervention.IsDone == 1)
                {
                    if (Intervention.DoneDateEnd.HasValue && !string.IsNullOrWhiteSpace(Intervention.DoneHourEnd))
                    {
                        Settings.LastDoneDate = Intervention.DoneDateEnd.Value;
                        Settings.LastDoneTime = Intervention.DoneHourEnd;
                    }

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
                }

                App.LocalDb.BeginTransaction();

                Intervention.IsToSync = true;

                if (Mode != 0)
                {
                    Intervention.EditDate = DateTime.Now;

                    if (!await interventionService.UpdateIntervention(Intervention))
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }
                else
                {
                    //if (AppSettings.MobilePreremplirPlanningDate)
                    //{
                        Intervention.PlanningDateStart = DateTime.Now;
                        Intervention.PlanningDateEnd = DateTime.Now;
                    //}

                    Intervention.AddDate = DateTime.Now;

                    if (!await interventionService.CreateIntervention(Intervention))
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }

                foreach (var un in ListUnite)
                {
                    if (Intervention.UniteLinks != null && Intervention.UniteLinks.Find(ul => (ul.FkUniteServerId > 0 && ul.FkUniteServerId == un.ServerId) || (!ul.FkUniteAppliId.Equals(Guid.Empty) && !un.Id.Equals(Guid.Empty) && ul.FkUniteAppliId.Equals(un.Id))) is UniteLink uniteLink)
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
                            Intervention.UniteLinks.Add(new UniteLink()
                            {
                                Id = Guid.NewGuid(),
                                UserId = CurrentUser.Id,
                                FkColumnAppliId = Intervention.Id,
                                FkColumnServerId = Intervention.ServerId,
                                LinkTableName = "intervention",
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

                foreach (var ta in ListTask)
                {
                    if (Intervention.LinkInterventionTasks.Find(lit => (lit.FkTaskServerId > 0 && lit.FkTaskServerId == ta.ServerId)
                     || (!lit.FkTaskAppliId.Equals(Guid.Empty) && !ta.Id.Equals(Guid.Empty) && lit.FkTaskAppliId.Equals(ta.Id))) is LinkInterventionTask linkInterventionTask)
                    {
                        if (ta.Selected)
                        {
                            linkInterventionTask.DoneMinute = ta.Minute;
                            linkInterventionTask.IsActif = 1;
                            linkInterventionTask.IsDone = 1;
                        }
                        else
                        {
                            linkInterventionTask.DoneMinute = 0;
                            linkInterventionTask.IsDone = 0;
                        }
                        linkInterventionTask.EditDate = DateTime.Now;
                        linkInterventionTask.IsToSync = true;
                    }
                    else if (ta.Selected)
                    {
                        Intervention.LinkInterventionTasks.Add(new LinkInterventionTask()
                        {
                            Id = Guid.NewGuid(),
                            UserId = CurrentUser.Id,
                            FkInterventionAppliId = Intervention.Id,
                            FkInterventionServerId = Intervention.ServerId,
                            FkTaskAppliId = ta.Id,
                            FkTaskServerId = ta.ServerId,
                            DoneMinute = ta.Minute,
                            IsActif = 1,
                            AddDate = DateTime.Now,
                            EditDate = DateTime.Now,
                            IsToSync = true,
                            IsDone = 1,
                           
                        });
                    }
                }

                //foreach (var lit in Intervention.LinkInterventionTasks)
                //{
                //    if (lit.DoneMinute == 0)
                //    {
                //        try { 
                //                if (TimeSpan.TryParse(Intervention.DoneHour, out TimeSpan workingTime))
                //                {
                //                    lit.DoneMinute = Convert.ToInt32(workingTime.TotalMinutes);
                //                }
                //                else
                //                {
                //                    lit.DoneMinute = lit.PlanningMinute;
                //                }
                //        }catch(Exception ex)
                //        {

                //        }
                //    }
                //}

                foreach (var child in Intervention.ChildInterventions)
                {
                    if (child.IsActif == 1)
                        child.UpdatePropertiesFrom(Intervention, nameof(Intervention.Id), nameof(Intervention.ServerId), nameof(Intervention.Code), nameof(Intervention.FkUserAppId), nameof(Intervention.FkUserServerlId), nameof(Intervention.FkParentAppId), nameof(Intervention.FkParentServerlId));

                    App.LocalDb.InsertOrReplace(child);
                }

                foreach (var child in DeletedChildren)
                {
                    App.LocalDb.Delete(child);
                }

                foreach (var ul in Intervention.UniteLinks)
                {
                    App.LocalDb.InsertOrReplace(ul);
                    }

                foreach (var lit in Intervention.LinkInterventionTasks)
                {
                    App.LocalDb.InsertOrReplace(lit);
                }

                foreach (var lip in Intervention.LinkInterventionProducts)
                {
                    App.LocalDb.InsertOrReplace(lip);
                }

                foreach (var ip in DeletedProducts)
                {
                    App.LocalDb.Delete(ip);
                }

                foreach (var lie in Intervention.LinkInterventionEquipments)
                {
                    App.LocalDb.InsertOrReplace(lie);
                }

                foreach (var ep in DeletedEquipments)
                {
                    App.LocalDb.Delete(ep);
                }

                foreach (var medl in Intervention.MediaLinks)
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

                MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED, Intervention);
                MessagingCenter.Send(this, MessageKey.INTERVENTION_CHANGED);

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