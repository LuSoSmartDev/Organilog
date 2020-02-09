using Organilog.Common;
using Organilog.Constants;
using Organilog.IServices;
using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using TinyMVVM;

namespace Organilog.ViewModels.Issues
{
    public class IssuesViewModel : BaseViewModel
    {
        private readonly IIssueService issueService;

        private bool isIssueChanged = true;

        private List<Issue> listIssue = new List<Issue>();
        public List<Issue> ListIssue { get => listIssue; set => SetProperty(ref listIssue, value); }

        private List<Issue> listAllIssue = new List<Issue>();
        public List<Issue> ListAllIssue { get => listAllIssue; set => SetProperty(ref listAllIssue, value); }

        public ICommand GetSyncCommand { get; set; }
        public ICommand GetIssuesCommand { get; set; }
        public ICommand AddIssueCommand { get; set; }
        public ICommand ViewIssueCommand { get; set; }

        public ICommand ViewOpenIssueCommand { get; set; }
        public ICommand ViewClosedCommand { get; set; }

        private Color detailTabColor = Color.FromHex("#47CEC0");
        public Color DetailTabColor { get => detailTabColor; set => SetProperty(ref detailTabColor, value); }

       
        private Color mediaTabColor;
        public Color MediaTabColor { get => mediaTabColor; set => SetProperty(ref mediaTabColor, value); }

        private int currentTab { get; set; }
        public IssuesViewModel(IIssueService issueService)
        {
            this.issueService = issueService;

            GetSyncCommand = new Command(GetSync);
            GetIssuesCommand = new Command(GetListIssue);
            AddIssueCommand = new AwaitCommand(AddIssue);
            ViewIssueCommand = new AwaitCommand<Issue>(ViewIssue);

            ViewOpenIssueCommand = new Command(ViewOpenIssue);
            ViewClosedCommand = new Command(ViewClosedIssue);
            DetailTabColor = Color.WhiteSmoke;  
            MediaTabColor = Color.FromHex("#47CEC0");

            currentTab = 0;

        }

        private void ViewClosedIssue(object obj)
        {
            DetailTabColor = Color.FromHex("#47CEC0");
            MediaTabColor = Color.WhiteSmoke;
            currentTab = 1;
            
            ListIssue = ListAllIssue.FindAll(e => e.Status == 5 || e.Status == 3 || e.Status == 6);

          
        }

        private void ViewOpenIssue(object obj)
        {
            DetailTabColor = Color.WhiteSmoke;
            MediaTabColor = Color.FromHex("#47CEC0");
            currentTab = 0;
            ListIssue = ListAllIssue.FindAll(e => e.Status == 1 || e.Status == 2 || e.Status == 4);
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            if (isIssueChanged)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    GetListIssue();
                    isIssueChanged = false;
                });
                
            }
        }

        public override void OnPageCreated()
        {
            base.OnPageCreated();
            //TODO 
            RegisterMessagingCenter<NewIssueViewModel>(this, MessageKey.ISSUE_CHANGED, OnIssueChanged);
            ///RegisterMessagingCenter<IssueDetailViewModel>(this, MessageKey.ISSUE_CHANGED, OnIssueChanged);
        }
        public override void OnPopped()
        {
            base.OnPopped();
            //TODO 
            MessagingCenter.Unsubscribe<NewIssueViewModel>(this, MessageKey.ISSUE_CHANGED);
            ///MessagingCenter.Unsubscribe<IssueDetailViewModel>(this, MessageKey.ISSUE_CHANGED);
            //RegisterMessagingCenter<NewIssueViewModel>(this, MessageKey.ISSUE_CHANGED, OnIssueChanged);
            //RegisterMessagingCenter<IssueDetailViewModel>(this, MessageKey.ISSUE_CHANGED, OnIssueChanged);
        }


        private async void OnIssueChanged(object sender)
        {
            isIssueChanged = true;
          
        }

        private void GetSync(object sender)
        {
            syncService.SyncFromServer(1, onSuccess: GetListIssue, showOverlay: true);
        }

        private void GetListIssue()
        {
            Task.Run(async () =>
            {
                return await issueService.GetIssues();
            }).ContinueWith(task => Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = false;

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    ListAllIssue = task.Result;
                    if (currentTab == 0)
                    {
                        ListIssue = ListAllIssue.FindAll(e => e.Status == 1 || e.Status == 2|| e.Status == 4);
                    }
                    else
                    {
                        ListIssue = ListAllIssue.FindAll(e => e.Status == 5 || e.Status == 3 || e.Status == 6);

                    }

                }
                else if (task.IsFaulted)
                {
                    CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), task.Exception.GetBaseException().Message, TranslateExtension.GetValue("ok"));
                }
            }));
        }

        private async void AddIssue(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.ISSUE_MODE, EditMode.New}
                };

                await CoreMethods.PushViewModel<NewIssueViewModel>(parameters);
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

        private async void ViewIssue(Issue issue, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_ISSUE, issue}
                };

                await CoreMethods.PushViewModel<IssueDetailViewModel>(parameters);
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