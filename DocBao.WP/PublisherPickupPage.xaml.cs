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

namespace DocBao.WP
{
    public partial class PublisherPickupPage : PhoneApplicationPage
    {
        PublisherPickupViewModel _viewModel;
        FeedManager _feedManager;
        PublisherBankViewModel _lastItem;

        public PublisherPickupPage()
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
            _viewModel = new PublisherPickupViewModel();

            txtPickupName.Text = "chọn báo";
            firstNextIcon.Visibility = System.Windows.Visibility.Visible;
            txtStats.Text = PublisherHelper.GetAllStatsString();
            this.llmsPublisher.ItemsSource = _viewModel.PublisherBankViewModels;

            if (_lastItem != null)
                ScrollTo(_lastItem);
        }

        private void ScrollTo(PublisherBankViewModel item)
        {
            try
            {
                int i = 0;
                while (i < llmsPublisher.ItemsSource.Count && !item.Id.Equals((llmsPublisher.ItemsSource[i] as PublisherBankViewModel).Id))
                    i++;
                if (i < llmsPublisher.ItemsSource.Count)
                    llmsPublisher.ScrollTo(llmsPublisher.ItemsSource[i]);
            }
            catch (Exception) { }
        }

        private void OnEmailListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void OnEmailListIsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private async void OnItemContentTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PublisherBankViewModel publisher = ((FrameworkElement)sender).DataContext as PublisherBankViewModel;
            _lastItem = publisher;
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
    }
}