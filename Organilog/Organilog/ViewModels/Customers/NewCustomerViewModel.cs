using Organilog.Constants;
using Organilog.Models;
using Organilog.ViewModels.Addresses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Customers
{

    public class NewCustomerViewModel : BaseViewModel
    {
        private int mode; // 0: Add 1: Edit
        public int Mode { get => mode; set => SetProperty(ref mode, value); }

        public readonly int CurrentUserId = Settings.CurrentUserId;

        private Client client;
        public Client Client { get => client; set => SetProperty(ref client, value); }

      
        
        public ICommand CancelCommand { get; private set; }
        public ICommand SaveCustomerCommand { get; private set; }
        public ICommand AddAddressCommand { get; private set; }
        public ICommand DeleteAddressCommand { get; set; }
        public bool EnableSave { get; set; }
        private List<Address> listAddress = new List<Address>();
        public List<Address> ListAddress { get => listAddress; set => SetProperty(ref listAddress, value); }

        public NewCustomerViewModel()
        {
            CancelCommand = new AwaitCommand(Cancel);
            SaveCustomerCommand = new AwaitCommand(SaveCustomer);
            AddAddressCommand = new AwaitCommand(AddressCommand);
            DeleteAddressCommand = new AwaitCommand<Address>(DeleteAddress);
        }

       

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Mode = parameters?.GetValue<int>(ContentKey.NEW_CUSTOMER_MODE) ?? 0;

            
            if (Mode != 0)
            {
                Client = parameters?.GetValue<Client>(ContentKey.SELECTED_CUSTOMER)?.DeepCopy();

                Title = string.IsNullOrWhiteSpace(Client?.Title) ? TranslateExtension.GetValue("page_title_edit_customer") : Client?.Title;
            }
            else
            {
                Title = TranslateExtension.GetValue("page_title_new_customer");

                Client = new Client()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUserId,
                    Addressesclone = new ObservableCollection<Address>(),
                    //Addressesclone = new List<Address>(),
                };
            }
            EnableSave = !Client.RequiredClientName;
          

            //Client.Addresses.Add(new Address { CodePostal = "32323" });
        }

        private async void AddressCommand(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Client }
            };

            //TODO 

            await CoreMethods.PushViewModel<NewAddressViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }
        private void DeleteAddress(object sender, TaskCompletionSource<bool> tcs)
        {
            App.LocalDb.Delete(sender);
            Client.Addressesclone.Remove((Address)sender);
            tcs.TrySetResult(true);
        }

        public override void OnPageCreated()
        {
            base.OnPageCreated();

            MessagingCenter.Subscribe<NewAddressViewModel>(this, MessageKey.ADDRESS_CHANGED, OnAddressChanged);
        }

        public override void OnPopped()
        {
            base.OnPopped();

            MessagingCenter.Unsubscribe<NewAddressViewModel>(this, MessageKey.ADDRESS_CHANGED);
        }

        private void OnAddressChanged(object sender)
        {
           
            var address = ((NewAddressViewModel)sender).Address;
            Client.Addressesclone.Add(address);
            Client.OnPropertyChanged(nameof(Client.Addressesclone));
            Client.OnPropertyChanged();
        }




        private  void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnableSave = !Client.RequiredClientName;
            //Client.Addresses.Add(new Address { CodePostal = "32323" });
        }


        private async void Cancel(object sender, TaskCompletionSource<bool> tcs)
        {

            foreach(Address address in Client.Addressesclone) { 
                App.LocalDb.Delete(address);
                //Client.Addressesclone.Remove(address);
                //don't remove last item here

            }
            await CoreMethods.PopViewModel(modal: IsModal);
            //Remove if cancel


            tcs.SetResult(true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Client.Title))
            {
                CoreMethods.DisplayAlert("", "Le libellé du client est obligatoire", TranslateExtension.GetValue("ok"));
                return false;
            }

            return true;
        }

        private async void SaveCustomer(object sender, TaskCompletionSource<bool> tcs)
        {
            
            try
            {
                if (!ValidateInput())
                {
                    tcs.TrySetResult(true);
                    return;
                }

                Client.IsToSync = true;

                if (Mode != 0)
                {
                    Client.EditDate = DateTime.Now;

                    if (App.LocalDb.Update(Client) == 0)
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }
                else
                {
                    Client.AddDate = DateTime.Now;
                    Client.EditDate = DateTime.Now;
                    Client.IsActif = 1;

                    if (App.LocalDb.Insert(Client) == 0)
                    {
                        tcs.TrySetResult(true);
                        return;
                    }
                }

                MessagingCenter.Send(this, MessageKey.CUSTOMER_CHANGED, Client);
                MessagingCenter.Send(this, MessageKey.CUSTOMER_CHANGED);

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
        
            tcs.TrySetResult(true);
        }
    }
}