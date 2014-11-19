using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DocBao.WP.ViewModels;
using DocBao.ApplicationServices;
using DocBao.WP.Helper;
using Davang.Utilities.Extensions;
using Davang.Utilities.Log;
using Davang.WP.Utilities.Extensions;
using System.Threading.Tasks;
using Davang.WP.Utilities;

namespace DocBao.WP
{
    public partial class PublisherPickupPage : DBBasePage
    {
        PublisherPickupViewModel _viewModel;
        Guid _lastPublisherId;

        public PublisherPickupPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();
            CreateAppBar();
            Binding();

            var previousPage = NavigationService.BackStack.First().Source;
            if (previousPage.ToString().Contains("ItemPage.xaml")
                || previousPage.ToString().Contains("FeedPage.xaml"))
                SetAsSecondPage();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            llmsPublisher.ItemsSource = null;
            base.OnNavigatedFrom(e);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            llmsPublisher.ItemsSource = null;
            llmsPublisher.ItemTemplate = null;
            base.OnRemovedFromJournal(e);
        }

        private void Binding()
        {
            _viewModel = new PublisherPickupViewModel(AppConfig.ShowAllPublisher);

            txtPickupName.Text = "chọn báo";
            txtGuid.Text = AppConfig.ShowAllPublisher ? "Chạm vào từng báo để cài hay gỡ..." : "Chỉ đang hiện những báo chưa cài. Chạm vào từng báo để cài hay gỡ...";
            firstNextIcon.Visibility = System.Windows.Visibility.Visible;
            txtStats.Text = PublisherHelper.GetAllStatsString();
            this.llmsPublisher.ItemsSource = _viewModel.PublisherBankViewModels;

            llmsPublisher.ScrollTo<Guid>(_lastPublisherId);
        }

        private async void OnItemContentTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PublisherBankViewModel publisher = ((FrameworkElement)sender).DataContext as PublisherBankViewModel;
            _lastPublisherId = publisher.Id;
            if (publisher != null)
            {
                var message = string.Format("đang {0} {1}...", publisher.Subscribed ? "gỡ" : "cài", publisher.Name);
                this.SetProgressIndicator(true, message);
                this.llmsPublisher.IsEnabled = false;

                AppResult<bool> result = publisher.Subscribed ? await _feedManager.UnsubscribePublisher(publisher.Id) : await _feedManager.SubscribePublisher(publisher.Id);
                if (result.HasError)
                    Messenger.ShowToast(result.ErrorMessage());
                else
                {
                    Binding();
                    var doneMessage = string.Format("{0} {1} xong...", publisher.Subscribed ? "cài" : "gỡ", publisher.Name);
                    Messenger.ShowToast(doneMessage);
                }

                this.llmsPublisher.IsEnabled = true;
                this.SetProgressIndicator(false);
            }
        }

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }

        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

            var showAllMenuItem = new ApplicationBarMenuItem();
            showAllMenuItem.Text = AppConfig.ShowAllPublisher ? "chỉ hiện báo chưa cài" : "hiện hết báo";
            showAllMenuItem.Click += new EventHandler(showAllButton_Click);

            ApplicationBar.MenuItems.Add(showAllMenuItem);
        }

        private void showAllButton_Click(object sender, EventArgs e)
        {
            AppConfig.ShowAllPublisher = !AppConfig.ShowAllPublisher;
            Binding();
            CreateAppBar();
        }
    }
}