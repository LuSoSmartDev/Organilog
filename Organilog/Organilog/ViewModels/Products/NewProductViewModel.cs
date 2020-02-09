using Organilog.Constants;
using Organilog.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Products
{
    public class NewProductViewModel : TinyViewModel
    {
        private readonly int CurrentUserId = Settings.CurrentUserId;

        private int mode; // 0: Add 1: Edit
        public int Mode { get => mode; set => SetProperty(ref mode, value); }

        private Product product;
        public Product Product { get => product; set => SetProperty(ref product, value); }


        public ICommand CancelCommand { get; private set; }
        public ICommand SaveProductCommand { get; private set; }

        public NewProductViewModel()
        {
            CancelCommand = new AwaitCommand(Cancel);
            SaveProductCommand = new AwaitCommand(SaveProuct);
        }

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Mode = parameters?.GetValue<int>(ContentKey.NEW_CUSTOMER_MODE) ?? 0;

            if (Mode != 0)
            {
                Product = parameters?.GetValue<Product>(ContentKey.SELECTED_PRODUCT)?.DeepCopy();

               // Title = TranslateExtension.GetValue("product") + " #" + Product.Code;
            }
            else
            {
               // Title = TranslateExtension.GetValue("page_title_new_address");

                Product = new Product()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUserId
                };

              
            }
        }

        private async void Cancel(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PopViewModel(modal: IsModal);

            tcs.SetResult(true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Product.Nom) && Product.Quantity >0)
            {
                CoreMethods.DisplayAlert("", "Le champ \"Produict\" est obligatoire", TranslateExtension.GetValue("ok"));
                return false;
            }

            return true;
        }

        private async void SaveProuct(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                if (!ValidateInput())
                {
                    tcs.TrySetResult(true);
                    return;
                }

                Product.IsToSync = true;

                if (Mode != 0)
                {
                    Product.EditDate = DateTime.Now;

                    if (App.LocalDb.Update(Product) == 0)
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }
                else
                {
                   
                    Product.AddDate = DateTime.Now;
                    Product.EditDate = DateTime.Now;
                    Product.IsActif = 1;

                    if (App.LocalDb.Insert(Product) == 0)
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }

                MessagingCenter.Send(this, MessageKey.PRODUCT_CHANGED, Product);
                MessagingCenter.Send(this, MessageKey.PRODUCT_CHANGED);

                await CoreMethods.PopViewModel(modal: IsModal);
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