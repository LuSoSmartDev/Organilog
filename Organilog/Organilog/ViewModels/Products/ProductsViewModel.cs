using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Products
{
    public class ProductsViewModel : BaseViewModel
    {
        
        private readonly ISyncDataService syncService;
        private readonly IProductService productService;

        private readonly int CurrentUserId = Settings.CurrentUserId;

        private CancellationTokenSource cts;

        private ObservableCollection<Product> listProduct = new ObservableCollection<Product>();
        public ObservableCollection<Product> ListProduct { get => listProduct; set => SetProperty(ref listProduct, value); }

        public ICommand GetSyncCommand { get; set; }
        public ICommand LoadMoreProductCommand { get; set; }
        public ICommand SearchProductCommand { get; set; }
        public ICommand AddProductCommand { get; set; }
        public ICommand EditProductCommand { get; set; }

        public ProductsViewModel(ISyncDataService syncService, IProductService productService)
        {
            this.syncService = syncService;
            this.productService = productService;

            GetSyncCommand = new Command(GetSync);
            LoadMoreProductCommand = new Command(LoadMoreProduct);
            SearchProductCommand = new Command<string>(SearchProduct);
            AddProductCommand = new AwaitCommand(AddProduct);
            EditProductCommand = new AwaitCommand<Product>(EditProduct);
        }

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            LoadMoreProduct(null);
        }

        public override void OnPageCreated()
        {
            base.OnPageCreated();

            MessagingCenter.Subscribe<NewProductViewModel>(this, MessageKey.PRODUCT_CHANGED, OnProductChanged);
            MessagingCenter.Subscribe<ProductDetailViewModel>(this, MessageKey.PRODUCT_CHANGED, OnProductChanged);
        }

        public override void OnPopped()
        {
            base.OnPopped();

            MessagingCenter.Unsubscribe<NewProductViewModel>(this, MessageKey.PRODUCT_CHANGED);
            MessagingCenter.Unsubscribe<ProductDetailViewModel>(this, MessageKey.PRODUCT_CHANGED);
        }

        private void OnProductChanged(object sender)
        {
            ListProduct.Clear();
            LoadMoreProduct(null);
        }

        private void GetSync(object sender)
        {
            syncService.SyncFromServer(method: 2, onSuccess: () => OnProductChanged(null), showOverlay: true);
        }

        private void LoadMoreProduct(object sender)
        {
            Task.Run(async () =>
            {
                return await productService.GetProducts(CurrentUserId, offset: ListProduct.Count, limit: 20);
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    ListProduct.AddRange(task.Result);
                }
            }));
        }

        private void SearchProduct(string newValue)
        {
            if (cts != null)
                cts.Cancel();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            Task.Run(async () =>
            {
                await Task.Delay(250, token);

                if (string.IsNullOrWhiteSpace(newValue))
                {
                    return await productService.GetProducts(CurrentUserId, limit: 20);
                }
                else
                {
                    return await productService.SearchProduct(CurrentUserId, newValue, 100);
                }
            }, token).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    ListProduct = task.Result.ToObservableCollection();
                }
                else if (task.IsFaulted && !token.IsCancellationRequested)
                {
                    //CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception?.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                }
            }));
        }

        private async void AddProduct(object sender, TaskCompletionSource<bool> tcs)
        {
            if (IsBusy)
                return;

            await CoreMethods.PushViewModel<NewProductViewModel>(modal: true);

            tcs.SetResult(true);
        }

        private async void EditProduct(Product value, TaskCompletionSource<bool> tcs)
        {
            if (IsBusy)
            {
                tcs.SetResult(true);
                return;
            }

            var parameters = new NavigationParameters()
            {
                { ContentKey.SELECTED_PRODUCT,  value}
            };

            await CoreMethods.PushViewModel<ProductDetailViewModel>(parameters);

            tcs.SetResult(true);
        }
    }
}
