using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using TinyMVVM;
using System.Collections.Generic;

namespace Organilog.ViewModels.Shared
{
    public class GroupLookUpViewModel : TinyViewModel
    {
        private readonly IGroupService GroupService;

        private readonly int CurrentUserId = Settings.CurrentUserId;

        private CancellationTokenSource cts;

        private ObservableCollection<Category> listGroup;
        public ObservableCollection<Category> ListGroup { get => listGroup; set => SetProperty(ref listGroup, value); }

        private string searchKey;
        public string SearchKey { get => searchKey; set => SetProperty(ref searchKey, value, onChanged: OnsearchKeyChanged); }

        public ICommand CancelSearchCommand { get; set; }

        public ICommand GroupSelectedCommand { get; set; }

        private Client client;
        public Client Client { get => client; set => SetProperty(ref client, value); }


        public GroupLookUpViewModel(IGroupService GroupService)
        {
            this.GroupService = GroupService;

            GroupSelectedCommand = new AwaitCommand<Category>(GroupSelected);
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

            //MessagingCenter.Subscribe<NewCustomerViewModel>(this, MessageKey.CUSTOMER_CHANGED, OnCustomerChanged);
        }

        public override void OnPopped()
        {
            base.OnPopped();

            //MessagingCenter.Unsubscribe<NewCustomerViewModel>(this, MessageKey.CUSTOMER_CHANGED);
        }

        private void OnCustomerChanged(object sender)
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
                Task.Delay(250, token).Wait();
                if (Client != null)
                    return await GroupService.SearchGroupByClient(Client, CurrentUserId, SearchKey);
                else
                    return new List<Category>();

            }, token).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    ListGroup = task.Result.ToObservableCollection();
                }
                else if (task.IsFaulted && !token.IsCancellationRequested)
                {
                    //CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception?.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                }
            }));
        }

        private async void GroupSelected(Category value, TaskCompletionSource<bool> tcs)
        {
            MessagingCenter.Send(this, MessageKey.GROUP_SELECTED, value);

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