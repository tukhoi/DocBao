using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DocBao.WP.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices;
using DocBao.WP.Helper;
using System.Threading.Tasks;
using Davang.Utilities.Log;
using Davang.WP.Utilities.Extensions;
using Davang.WP.Utilities;

namespace DocBao.WP
{
    public partial class FeedPickupPage : DBBasePage
    {
        FeedPickupViewModel _viewModel;
        Guid _lastFeedId;

        public FeedPickupPage()
        {
            InitializeComponent();
            _feedManager = FeedManager.Instance;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();
            Binding();

            var previousPage = NavigationService.BackStack.First().Source;
            if (previousPage.ToString().Contains("ItemPage.xaml"))
                SetAsThirdPage();                

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            llmsFeed.ItemsSource = null;
            base.OnNavigatedFrom(e);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            llmsFeed.ItemsSource = null;
            llmsFeed.ItemTemplate = null;
            base.OnRemovedFromJournal(e);
        }

        private void Binding()
        {
            var publisherId = NavigationContext.QueryString.GetQueryStringToGuid("publisherId");
            if (default(Guid).Equals(publisherId)) return;

            _viewModel = new FeedPickupViewModel(publisherId);            
            
            txtPublisherName.Text = "chọn mục trong  " + _viewModel.Name;
            firstNextIcon.Visibility = System.Windows.Visibility.Visible;
            txtStats.Text = PublisherHelper.GetStatsString(publisherId);
            this.llmsFeed.ItemsSource = _viewModel.FeedBankViewModels;

            llmsFeed.ScrollTo<Guid>(_lastFeedId);
        }

        private async void OnItemContentTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FeedBankViewModel feed = ((FrameworkElement)sender).DataContext as FeedBankViewModel;
            _lastFeedId = feed.Id;
            if (feed != null)
            {
                var message = string.Format("đang {0} {1}...", feed.Subscribed ? "gỡ" : "cài", feed.Name);
                this.SetProgressIndicator(true, message);
                this.llmsFeed.IsEnabled = false;

                AppResult<bool> result = feed.Subscribed ? await _feedManager.UnsubscribeFeed(feed.Id) : await _feedManager.SubscribeFeed(feed.Id);
                if (result.HasError)
                    Messenger.ShowToast(result.ErrorMessage());
                else
                {
                    Binding();
                    var doneMessage = string.Format("{0} {1} xong...", feed.Subscribed ? "cài" : "gỡ", feed.Name);
                    Messenger.ShowToast(doneMessage);
                }

                this.llmsFeed.IsEnabled = true;
                this.SetProgressIndicator(false);
            }
        }

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }
    }
}