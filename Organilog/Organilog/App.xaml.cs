using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Organilog.IServices;
using Organilog.Models;
using Organilog.Services;
using Plugin.LocalNotification;
using SQLite;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TinyMVVM;
using TinyMVVM.IoC;
using Xamarin.Forms;
using Xamarin.Forms.Converters;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.Xaml;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Organilog
{
    public partial class App : Application
    {
        public static bool IsDeveloperMode { get; set; } = false; // For testing or debugging

        public static SQLiteConnection LocalDb { get; set; }

        public static JsonSerializerSettings DefaultDeserializeSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> { new IgnoreDataTypeConverter() }
        };

        public App()
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("OTkzNTJAMzEzNzJlMzEyZTMwbHYrZmduam1yNHRhQUhjSlpPSUZJTmpvZXc4b0VpZVdQNzNWcFNaamQxaz0=");

            InitializeComponent();

            Init();

            AppSettings.ReloadSetting();

            AssetsExtension.InitAssetsExtension("AppResources.Assets", typeof(App).GetTypeInfo().Assembly);
            ImageResourceExtension.InitImageResourceExtension("AppResources.Assets", typeof(App).GetTypeInfo().Assembly);
            TranslateExtension.InitTranslateExtension("AppResources.Localization.Resources", CultureInfo.CurrentCulture, typeof(App).GetTypeInfo().Assembly);

            RegisterDependency();

            if (Settings.LoggedIn)
            {
                MainPage = ViewModelResolver.ResolveViewModel<ViewModels.SyncViewModel>();
            }
            else
            {
                MainPage = new NavigationContainer(ViewModelResolver.ResolveViewModel<ViewModels.Login.LoginViewModel>())
                {
                    BarBackgroundColor = Color.FromHex("#2196F3"),
                    BarTextColor = Color.White
                };
            }
        }

        private void RegisterDependency()
        {
            TinyIOC.Container.Register<IRestClient, RestClient>().AsSingleton();

            TinyIOC.Container.Register<ILoginService, LoginService>().AsMultiInstance();
            TinyIOC.Container.Register<ISyncDataService, SyncDataService>().AsMultiInstance();

            TinyIOC.Container.Register<IInterventionService, InterventionService>().AsMultiInstance();
            TinyIOC.Container.Register<IClientService, ClientService>().AsMultiInstance();
            TinyIOC.Container.Register<IAddressService, AddressService>().AsMultiInstance();
            TinyIOC.Container.Register<IContractService, ContractService>().AsMultiInstance();
            TinyIOC.Container.Register<IGroupService, GroupService>().AsMultiInstance();
            TinyIOC.Container.Register<ISyncProductService, SyncProductService>().AsMultiInstance();

            //invoice
            TinyIOC.Container.Register<IInvoiceService, InvoiceService>().AsMultiInstance();
            TinyIOC.Container.Register<ISyncInvoiceService, SyncInvoiceService>().AsMultiInstance();
            TinyIOC.Container.Register<ISyncIssueService, SyncIssueService>().AsMultiInstance();
            TinyIOC.Container.Register<IIssueService, IssueService>().AsMultiInstance();

            //sync equipment 
            TinyIOC.Container.Register<IEquipmentService, EquipmentService>().AsMultiInstance();
            TinyIOC.Container.Register<ISyncEquipmentService, SyncEquipmentService>().AsMultiInstance();

            TinyIOC.Container.Register<IProductService, ProductService>().AsMultiInstance();

        }

        private void Init()
        {
            LocalDb = DependencyService.Get<ILocalDbService>().GetDbConnection();

            LocalDb.CreateTable<Filiale>();
            LocalDb.CreateTable<Intervention>();
            LocalDb.CreateTable<Client>();
            LocalDb.CreateTable<Address>();
            LocalDb.CreateTable<Chemin>();
            LocalDb.CreateTable<UniteLink>();
            LocalDb.CreateTable<Unite>();
            LocalDb.CreateTable<UniteItem>();
            LocalDb.CreateTable<LinkInterventionTask>();
            LocalDb.CreateTable<Tasks>();
            LocalDb.CreateTable<MediaLink>();
            LocalDb.CreateTable<Media>();
            LocalDb.CreateTable<LinkInterventionProduct>();
            LocalDb.CreateTable<Product>();
            LocalDb.CreateTable<CategoryTracking>();
            LocalDb.CreateTable<Location>();
            LocalDb.CreateTable<Tracking>();
            LocalDb.CreateTable<User>();
            LocalDb.CreateTable<Message>();
            LocalDb.CreateTable<Setting>();
            LocalDb.CreateTable<SettingItem>();

            //Invoice and Product in invoice
            LocalDb.CreateTable<Invoice>();
            LocalDb.CreateTable<InvoiceProduct>();

            //Issue and IssueLink 
            LocalDb.CreateTable<Issue>();
            LocalDb.CreateTable<IssueLink>();

            //contract
            LocalDb.CreateTable<Contract>();

            //GROUP
            LocalDb.CreateTable<Category>();
            //Equipment 
            LocalDb.CreateTable<Equipment>();
            LocalDb.CreateTable<LinkInterventionEquipment>();

            // Local Notification tap event listener
            MessagingCenter.Instance.Subscribe<LocalNotificationTappedEvent>(this, typeof(LocalNotificationTappedEvent).FullName, OnLocalNotificationTapped);
        }

        private void OnLocalNotificationTapped(LocalNotificationTappedEvent e)
        {
            //Debug.WriteLine("Local Notification Tapped: " + e.Data);
        }

        protected async override void OnStart()
        {


            //if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
            //{
            //    //Start AppCenter Push notification with iOS app secret
            //    AppCenter.Start("a5324b09-9a90-424f-9614-52f20d09a4a6", typeof(Push));
            //}
            //else if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.UWP)
            //{
            //    //Start AppCenter Push notification with UWP app secret
            //    AppCenter.Start("47e40440-6c3e-4b9a-b7af-9c47feb14fae", typeof(Push));
            //}
            //else if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            //{
            //    //Start AppCenter Push notification with Android app secret
            //    AppCenter.Start("47e40440-6c3e-4b9a-b7af-9c47feb14fae", typeof(Push));
            //}

            //Handle when your app starts
            //AppCenter.Start("android=47e40440-6c3e-4b9a-b7af-9c47feb14fae;" +
            //      //"ios=a5324b09-9a90-424f-9614-52f20d09a4a6;",
            //      typeof(Analytics), typeof(Crashes), typeof(Push));

            //bool isEnabled = await Push.IsEnabledAsync();
            //if (!isEnabled) {
            //    await Push.SetEnabledAsync(true);
            //}

            // Pushing for iOS and Android Platform using realtime 

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
            {
                AppCenter.Start("a5324b09-9a90-424f-9614-52f20d09a4a6", typeof(Analytics), typeof(Crashes), typeof(Push));
            }
            else if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                AppCenter.Start("47e40440-6c3e-4b9a-b7af-9c47feb14fae", typeof(Analytics), typeof(Crashes), typeof(Push));
            }


        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            // Crashes.GenerateTestCrash();
        }
    }
}
