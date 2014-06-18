﻿using System;
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

namespace DocBao.WP
{
    public partial class FeedPickupPage : PhoneApplicationPage
    {
        FeedPickupViewModel _viewModel;
        FeedManager _feedManager;
        FeedBankViewModel _lastItem;

        public FeedPickupPage()
        {
            InitializeComponent();
            _feedManager = FeedManager.GetInstance();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Binding();
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
            if (_lastItem != null)
                ScrollTo(_lastItem);
        }

        private void ScrollTo(FeedBankViewModel item)
        {
            try
            {
                int i = 0;
                while (i < llmsFeed.ItemsSource.Count && !item.Id.Equals((llmsFeed.ItemsSource[i] as FeedBankViewModel).Id))
                    i++;
                if (i < llmsFeed.ItemsSource.Count)
                    llmsFeed.ScrollTo(llmsFeed.ItemsSource[i]);
            }
            catch (Exception) { }
        }

        private async void OnItemContentTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FeedBankViewModel feed = ((FrameworkElement)sender).DataContext as FeedBankViewModel;
            _lastItem = feed;
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