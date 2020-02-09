
using Organilog.Common;
using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Equipments
{
   public class EquipmentsViewModel : BaseViewModel
    {
        private readonly IEquipmentService equipmentService;

        private bool isIssueChanged = true;

        private List<Equipment> listEquipment = new List<Equipment>();
        public List<Equipment> ListEquipment { get => listEquipment; set => SetProperty(ref listEquipment, value); }

        private List<Equipment> listAllEquipment = new List<Equipment>();
        public List<Equipment> ListAllEquipment { get => listAllEquipment; set => SetProperty(ref listAllEquipment, value); }
        

        public ICommand GetSyncCommand { get; set; }
        public ICommand GetEquipmentsCommand { get; set; }
        public ICommand AddEquipmentCommand { get; set; }
        public ICommand ViewEquipmentCommand { get; set; }

        public ICommand ViewOpenIssueCommand { get; set; }
        public ICommand ViewClosedCommand { get; set; }

        private Color detailTabColor = Color.FromHex("#47CEC0");
        public Color DetailTabColor { get => detailTabColor; set => SetProperty(ref detailTabColor, value); }


        private Color mediaTabColor;
        public Color MediaTabColor { get => mediaTabColor; set => SetProperty(ref mediaTabColor, value); }

        private int currentTab { get; set; }
        public EquipmentsViewModel(IEquipmentService equipmentService)
        {
            this.equipmentService = equipmentService;

            GetSyncCommand = new Command(GetSync);
            GetEquipmentsCommand = new Command(GetListEquipment);
            AddEquipmentCommand = new AwaitCommand(AddEquipment);
            ViewEquipmentCommand = new AwaitCommand<Equipment>(ViewEquipment);

            ViewOpenIssueCommand = new Command(ViewOpenIssue);
            ViewClosedCommand = new Command(ViewClosedIssue);
            DetailTabColor = Xamarin.Forms.Color.WhiteSmoke;
            MediaTabColor = Xamarin.Forms.Color.FromHex("#47CEC0");

            currentTab = 0;

        }

        private void ViewClosedIssue(object obj)
        {
            DetailTabColor = Color.FromHex("#47CEC0");
            MediaTabColor = Color.WhiteSmoke;
            currentTab = 1;


        }

        private void ViewOpenIssue(object obj)
        {
            DetailTabColor = Color.WhiteSmoke;
            MediaTabColor = Color.FromHex("#47CEC0");
            currentTab = 0;
          
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            if (isIssueChanged)
            {
                GetListEquipment();
                isIssueChanged = false;
            }
        }

        public override void OnPageCreated()
        {
            base.OnPageCreated();
            //TODO 
            RegisterMessagingCenter<NewEquipmentViewModel>(this, MessageKey.EQUIPMENT_CHANGED, OnEquipmentChanged);
            RegisterMessagingCenter<EquipmentDetailViewModel>(this, MessageKey.EQUIPMENT_CHANGED, OnEquipmentChanged);
        }

        private async void OnEquipmentChanged(object sender)
        {
            isIssueChanged = true;

        }

        private void GetSync(object sender)
        {
            syncService.SyncFromServer(1, onSuccess: GetListEquipment, showOverlay: true);
        }

        private void GetListEquipment()
        {
            Task.Run(async () =>
            {
                return await equipmentService.GetEquipments();
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = false;

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    ListEquipment = task.Result;
                    ListEquipment = ListEquipment.FindAll(equ => equ.Client != null);

                }
                else if (task.IsFaulted)
                {
                    CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                }
            }));
        }

        private async void AddEquipment(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.EQUIPMENT_MODE, EditMode.New}
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

        private async void ViewEquipment(Equipment equipment, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_EQUIPMENT, equipment}
                };

                await CoreMethods.PushViewModel<EquipmentDetailViewModel>(parameters);
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
