using Acr.UserDialogs;
using Organilog.Common;
using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models;
using Organilog.ViewModels.Interventions;
using Plugin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Products
{
    class ProductDetailViewModel: BaseViewModel
    {
        private Product product;
        public Product Product { get => product; set => SetProperty(ref product, value); }

        private readonly IProductService productService;

        private readonly int CurrentUserId = Settings.CurrentUserId;

        
        public ICommand DeleteProductCommand { get; set; }
        public ICommand EditProductCommand { get; set; }
        public ICommand AddInterventionCommand { get; set; }
        

        public ProductDetailViewModel(IProductService productService)
        {
            this.productService = productService;
            DeleteProductCommand  = new AwaitCommand(DeleteProduct);
            EditProductCommand = new AwaitCommand(EditProduct);
            AddInterventionCommand = new AwaitCommand(AddIntervention);
        }

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Product = parameters?.GetValue<Product>(ContentKey.SELECTED_PRODUCT)?.DeepCopy();

            Title =  " #" + Product.Code;

            
        }
        private async void AddIntervention(object sender, TaskCompletionSource<bool> tcs)
        {
            if (IsBusy)
                return;

            var parameters = new NavigationParameters()
            {
                { ContentKey.SELECTED_PRODUCT, Product}
            };

            await CoreMethods.PushViewModel<NewInterventionViewModel>(parameters);

            tcs.SetResult(true);
        }
       

        private async void EditProduct(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_PRODUCT,  Product},
                    { ContentKey.PRODUCT_MODE, EditMode.Modify}
                };

                await CoreMethods.PushViewModel<NewProductViewModel>(parameters);
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

        private async void DeleteProduct(object sender, TaskCompletionSource<bool> tcs)
        {
            if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_quote"), TranslateExtension.GetValue("alert_message_delete_quote_confirm"), TranslateExtension.GetValue("alert_message_yes"), TranslateExtension.GetValue("alert_message_no")))
            {
                Product.IsActif = 0;
                Product.IsToSync = true;

                App.LocalDb.Update(Product);

                MessagingCenter.Send(this, MessageKey.PRODUCT_CHANGED);

                await CoreMethods.PopViewModel();
            };

            tcs.SetResult(true);
        }
    }
}
