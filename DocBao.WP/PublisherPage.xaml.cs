using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Davang.Parser.Dto;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices;
using System.Collections.ObjectModel;
using DocBao.WP.ViewModels;
using System.Windows.Data;
using DocBao.WP.Helper;

namespace DocBao.WP
{
    public partial class PublisherPage : PhoneApplicationPage
    {
        FeedManager _feedManager;
        PublisherViewModel _viewModel;
        int _pageNumber = 0;
        Guid _lastFeedId;

        public PublisherPage()
        {
            InitializeComponent();
            _feedManager = FeedManager.GetInstance();
            _viewModel = new PublisherViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var lastFeedId = _feedManager.GetLastId<Guid>();
            if (!string.IsNullOrEmpty(lastFeedId))
                _lastFeedId = new Guid(lastFeedId);

            Binding();
            base.OnNavigatedTo(e);
        }

        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{
        //    _pageNumber--;
        //    base.OnNavigatedFrom(e);
        //}

        private void Binding()
        {
            var publisherId = NavigationContext.QueryString.GetQueryStringToGuid("publisherId");
            if (default(Guid).Equals(publisherId)) return;

            if (_pageNumber == 0)
                _pageNumber = 1;

            _viewModel.Initialize(publisherId);
            _viewModel.LoadPage( _pageNumber);

            txtPublisherName.Text = _viewModel.Name;
            firstNextIcon.Visibility = System.Windows.Visibility.Visible;
            txtStats.Text = PublisherHelper.GetStatsString(_viewModel.Id);
            this.llsFeedList.ItemsSource = _viewModel.FeedViewModels;

            //var readingFeed = _feedManager.GetReading<FeedViewModel, Guid>(_viewModel.FeedViewModels);
            //if (readingFeed != null)
            //    ScrollTo(readingFeed);

            if (_lastFeedId != null)
                ScrollTo(_lastFeedId);
        }

        private void ScrollTo(Guid lastFeedId)
        { 
            try
            {
                int i = 0;
                while (i < llsFeedList.ItemsSource.Count && !lastFeedId.Equals((llsFeedList.ItemsSource[i] as FeedViewModel).Id))
                    i++;
                if (i < llsFeedList.ItemsSource.Count)
                    llsFeedList.ScrollTo(llsFeedList.ItemsSource[i]);
            }
            catch (Exception)
            { }
        }

        //private void ScrollTo(Feed readingFeed)
        //{
        //    try
        //    {
        //        int i = 0;
        //        while (i < llsFeedList.ItemsSource.Count && !readingFeed.Id.Equals((llsFeedList.ItemsSource[i] as FeedViewModel).Id))
        //            i++;
        //        if (i < llsFeedList.ItemsSource.Count)
        //            llsFeedList.ScrollTo(llsFeedList.ItemsSource[i]);
        //    }
        //    catch (Exception)
        //    { }
        //}

        private void llsFeedList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                var feed = fe.DataContext as Feed;
                if (feed != null)
                {
                    //_feedManager.SetReading<Feed, Guid>(feed);
                    _lastFeedId = feed.Id;
                    var uri = string.Format("/FeedPage.xaml?feedId={0}&publisherId={1}", feed.Id, feed.Publisher.Id);
                    NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                }
            }
        }

        //private void Page_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var progressIndicator = SystemTray.ProgressIndicator;
        //    if (progressIndicator != null) return;

        //    progressIndicator = new ProgressIndicator();
        //    SystemTray.SetProgressIndicator(this, progressIndicator);
        //    Binding binding = new Binding("IsLoading") { Source = _viewModel };
        //    BindingOperations.SetBinding(progressIndicator, ProgressIndicator.IsVisibleProperty, binding);
        //    binding = new Binding("IsLoading") { Source = _viewModel };
        //    BindingOperations.SetBinding(progressIndicator, ProgressIndicator.IsIndeterminateProperty, binding);
        //    progressIndicator.Text = "tải thêm kênh...";
        //}

        private void llsFeedList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (!_viewModel.IsLoading 
                && llsFeedList.ItemsSource != null 
                && llsFeedList.ItemsSource.Count >= AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING
                && e.ItemKind == LongListSelectorItemKind.Item)
                if ((e.Container.Content as Feed).Equals(llsFeedList.ItemsSource[llsFeedList.ItemsSource.Count - AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING]))
                {
                    this.SetProgressIndicator(true, "tải thêm kênh...");

                    _pageNumber++;
                    var maxPageNumber = _viewModel.FeedIds.GetMaxPageNumber(AppConfig.FEED_COUNT_PER_PUBLISHER);
                    if (_pageNumber <= maxPageNumber)
                        _viewModel.LoadPage(_pageNumber);
                    else
                        _pageNumber = maxPageNumber;

                    this.SetProgressIndicator(false);
                }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            _pageNumber = 1;
            _viewModel.FeedViewModels.Clear();
            var uri = new Uri("/FeedPickupPage.xaml?publisherId=" + _viewModel.Id, UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }
    }
}