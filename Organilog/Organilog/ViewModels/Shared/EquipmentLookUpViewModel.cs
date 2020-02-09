using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models;
using Organilog.ViewModels.Equipments;
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

namespace Organilog.ViewModels.Shared
{
    class EquipmentLookUpViewModel: BaseViewModel
    {
       
            private readonly IEquipmentService EquipmentService;

            private readonly int CurrentUserId = Settings.CurrentUserId;

            private CancellationTokenSource cts;

            private List<Equipment> listEquipment;
            public List<Equipment> ListEquipment { get => listEquipment; set => SetProperty(ref listEquipment, value); }

            private string searchKey;
            public string SearchKey { get => searchKey; set => SetProperty(ref searchKey, value, onChanged: OnsearchKeyChanged); }

            public ICommand AddEquipmentCommand { get; private set; }
            public ICommand EquipmentSelectedCommand { get; private set; }
            public ICommand CancelSearchCommand { get; private set; }

            private Client client;
            public Client Client { get => client; set => SetProperty(ref client, value); }

            public EquipmentLookUpViewModel(IEquipmentService EquipmentService)
            {
                this.EquipmentService = EquipmentService;

                AddEquipmentCommand = new AwaitCommand(AddEquipment);
                EquipmentSelectedCommand = new AwaitCommand<Equipment>(EquipmentSelected);
                CancelSearchCommand = new AwaitCommand(CancelSearch);
            }

            public override void InitAsync(NavigationParameters parameters)
            {
                Client = parameters?.GetValue<Client>(ContentKey.SELECTED_CUSTOMER)?.DeepCopy();
                base.InitAsync(parameters);

                OnsearchKeyChanged();
            }

            public override void OnPageCreated()
            {
                base.OnPageCreated();

                MessagingCenter.Subscribe<NewEquipmentViewModel>(this, MessageKey.EQUIPMENT_CHANGED, OnEquipmentChanged);
            }

            public override void OnPopped()
            {
                base.OnPopped();

                MessagingCenter.Unsubscribe<NewEquipmentViewModel>(this, MessageKey.EQUIPMENT_CHANGED);
            }

            private void OnEquipmentChanged(object sender)
            {
                SearchKey = "";
                OnsearchKeyChanged();
            }

            private void OnsearchKeyChanged()
            {
                if (IsDisposing)
                    return;

                if (cts != null)
                    cts.Cancel();

                cts = new CancellationTokenSource();
                var token = cts.Token;

                Task.Run(async () =>
                {
                    await Task.Delay(250, token);

                    if(Client==null)
                        return new List<Equipment>();
                    if (string.IsNullOrWhiteSpace(SearchKey))
                    {
                        return await EquipmentService.SearchEquipmentByClient("",clientId:Client.ServerId,limit: 100);
                    }
                    else
                    {
                        return await EquipmentService.SearchEquipmentByClient(SearchKey,clientId:client.ServerId, limit:100);
                    }
                }, token).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        ListEquipment = task.Result;
                    }
                    else if (task.IsFaulted && !token.IsCancellationRequested)
                    {
                        CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception?.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                    }
                }));
            }

            private async void AddEquipment(object sender, TaskCompletionSource<bool> tcs)
            {
                await CoreMethods.PushViewModel<NewEquipmentViewModel>(modal: true);

                tcs.TrySetResult(true);
            }

            private async void EquipmentSelected(Equipment value, TaskCompletionSource<bool> tcs)
            {
                MessagingCenter.Send(this, MessageKey.EQUIPMENT_SELECTED, value);
                await CoreMethods.PopViewModel(modal: true);

                tcs.TrySetResult(true);
            }

            private async void CancelSearch(object sender, TaskCompletionSource<bool> tcs)
            {
                await CoreMethods.PopViewModel(modal: true);

                tcs.TrySetResult(true);
            }
        

    }
}
