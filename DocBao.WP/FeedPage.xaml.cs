using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DocBao.ApplicationServices;
using Davang.Parser.Dto;
using Davang.Utilities.Extensions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DocBao.WP.Helper;
using Microsoft.Phone.Tasks;
using DocBao.WP.ViewModels;
using System.Windows.Data;
using Davang.Utilities.Helpers;
using Microsoft.Phone.Net.NetworkInformation;

namespace DocBao.WP
{
    public partial class FeedPage : PhoneApplicationPage
    {
        FeedManager _feedManager = FeedManager.GetInstance();
        RssParserService _rssParser = RssParserService.GetInstance();
        FeedViewModel _viewModel = new FeedViewModel();
        int _pageNumber = 0;
        string _lastItemId;

        Publisher _currentPubisher;
        int _currentIndex=-1;

        public FeedPage()
        {
            InitializeComponent();

            if (LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                adControl.Visibility = System.Windows.Visibility.Collapsed;
            else
                adControl.Visibility = System.Windows.Visibility.Visible;

            //adControl.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var feedId = NavigationContext.QueryString.GetQueryStringToGuid("feedId");
            var publisherId = NavigationContext.QueryString.GetQueryStringToGuid("publisherId");
            var publisherResult = _feedManager.GetSubscribedPublisher(publisherId);
            if (publisherResult.HasError)
            {
                Messenger.ShowToast("không tìm thấy báo...");
                return;
            }

            var lastItemId = _feedManager.GetLastId<string>();
            if (!string.IsNullOrEmpty(lastItemId))
                _lastItemId = lastItemId;

            _currentPubisher = publisherResult.Target;
            //if _currentIndex hasn't been initialized (-1) then initialize it
            //otherwise leave it as-is
            _currentIndex = _currentIndex == -1 ? _currentPubisher.FeedIds.IndexOf(_currentPubisher.FeedIds.FirstOrDefault(id => id.Equals(feedId))) : _currentIndex;
            if (_currentIndex == -1 && _currentPubisher.FeedIds.Count > 0)
                _currentIndex = 0;

            var newItemCount = await Binding();
            if (newItemCount > 0)
                Messenger.ShowToast(newItemCount + " tin mới");

            base.OnNavigatedTo(e);
        }

        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{
        //    _pageNumber--;
        //    base.OnNavigatedFrom(e);
        //}

        private async Task<int> Binding(bool requireUpdate = false, bool goBackOnFail = true)
        {
            try
            {
                int updated = 0;

                this.SetProgressIndicator(true, "đang cập nhật...");
                if (_pageNumber == 0)
                    _pageNumber = 1;

                var feedId = _currentPubisher.FeedIds[_currentIndex];

                if (requireUpdate && !NetworkInterface.GetIsNetworkAvailable())
                {
                    Messenger.ShowToast("không có mạng...");
                    requireUpdate = false;
                }

                updated = await _viewModel.Initialize(feedId, _currentPubisher.Id, requireUpdate);
                _feedManager.SetLastId<Guid>(feedId.ToString());

                if (_viewModel.Items.Count == 0)
                {
                    this.SetProgressIndicator(false);
                    Messenger.ShowToast("chưa có tin nào...");
                    this.BackToPreviousPage();
                    return 0;
                }

                _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);
                txtPublisherName.Text = _viewModel.Publisher.Name;
                txtFeedName.Text = _viewModel.Name;
                firstNextIcon.Visibility = System.Windows.Visibility.Visible;
                secondNextIcon.Visibility = System.Windows.Visibility.Visible;
                UpdateItemReadCount();
                UpdateViewTitle();
                this.llsItemList.DataContext = _viewModel;

                CreateAppBar();

                if (_lastItemId != null)
                    ScrollTo(_lastItemId);

                this.SetProgressIndicator(false);

                return updated;
            }
            catch (Exception ex)
            {
                this.SetProgressIndicator(false);
                var message = string.Format("mục này đang bị lỗi...");
                if (goBackOnFail)
                    Messenger.ShowToast(message, completedAction: (() => this.BackToPreviousPage()));
                else
                    Messenger.ShowToast(message);
                return 0;
            }
        }

        private void UpdateItemReadCount()
        {
            txtReadCount.Text = _viewModel.ReadStats;
        }

        private void UpdateViewTitle()
        {
            if (AppConfig.ShowTitleOnly)
                _viewModel.ItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Collapsed);
            else
                _viewModel.ItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Visible);
        }

        private void ScrollTo(string lastItemId)
        {
            try
            {
                int i = 0;
                while (i < llsItemList.ItemsSource.Count && !lastItemId.Equals((llsItemList.ItemsSource[i] as ItemViewModel).Id))
                    i++;
                if (i < llsItemList.ItemsSource.Count)
                    llsItemList.ScrollTo(llsItemList.ItemsSource[i]);
            }
            catch (Exception) { }
        }

        private void llsItemList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Messenger.ShowToast("không có mạng...");
                return;
            }

            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                var item = fe.DataContext as Item;
                if (item != null)
                {
                    _feedManager.MarkItemAsRead(_viewModel.Id, item.Id, true);
                    //_feedManager.SetReading<Item, string>(item);
                    _lastItemId = item.Id;
                    var uri = string.Format("/ItemPage.xaml?feedId={0}&itemId={1}", item.FeedId, HttpUtility.UrlEncode(item.Id));
                    NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                }
            }
        }

        #region Context Menu

        private void ctxOpenIE_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            _feedManager.MarkItemAsRead(_viewModel.Id, item.Id, true);
            _lastItemId = item.Id;
            WebBrowserTask webTask = new WebBrowserTask();
            webTask.Uri = new Uri(item.Link, UriKind.Absolute);
            webTask.Show();
        }

        private async void ctxMarkRead_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            var message = item.Read ? "đang đánh dấu chưa đọc..." : "đang đánh dấu đã đọc...";
            this.SetProgressIndicator(true, message);

            var result = item.Read ? _feedManager.MarkItemAsRead(_viewModel.Id, item.Id, false) : _feedManager.MarkItemAsRead(_viewModel.Id, item.Id, true);
            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                _lastItemId = item.Id;
                await Binding();
            }

            this.SetProgressIndicator(false);
        }

        private void ctxCopyLink_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            if (!string.IsNullOrEmpty(item.Link))
            {
                Clipboard.SetText(item.Link);
                Messenger.ShowToast("đã chép link");
            }
            else
                Messenger.ShowToast("không tìm thấy link");
        }

<<<<<<< HEAD
        private async void ctxStoreItem_Click(object sender, RoutedEventArgs e)
        {
            var itemViewModel = GetItemFromContextMenu(sender);
            if (itemViewModel == null) return;

            this.SetProgressIndicator(true, "đang lưu...");
            var result = await _feedManager.StoreItemAsync(itemViewModel.ToItem());
            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                Messenger.ShowToast("lưu xong...");
                //await Binding();
            }

            this.SetProgressIndicator(false);
        }

=======
>>>>>>> parent of db4037a... Stored item feature
        #endregion

        #region AppBar

        private async void refreshButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang cập nhật...");
            var updated = await Binding(true, false);
            if (updated > 0)
                Messenger.ShowToast(updated + " tin mới");
            this.SetProgressIndicator(false);
        }

        private async void readAllButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang đánh dấu...");
            await Task.Run(() => _viewModel.Items.ForEach(i => _feedManager.MarkItemAsRead(_viewModel.Id, i.Id, true)));
            _pageNumber = 1;
            _viewModel.ItemViewModels.Clear();
            _lastItemId = null;
            await Binding();
            this.SetProgressIndicator(false);
        }

        private async void titleOnlyMenuItem_Click(object sender, EventArgs e)
        {
            AppConfig.ShowTitleOnly = !AppConfig.ShowTitleOnly;
            _pageNumber = 1;
            _viewModel.ItemViewModels.Clear();
            _lastItemId = null;
            await Binding();
        }

        private async void unreadItemOnly_Click(object sender, EventArgs e)
        {
            AppConfig.ShowUnreadItemOnly = !AppConfig.ShowUnreadItemOnly;
            _pageNumber = 1;
            _viewModel.ItemViewModels.Clear();
            _lastItemId = null;
            await Binding();

        }

        private void CreateAppBar()
        {
            var feedId = _currentPubisher.FeedIds[_currentIndex];

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

            var refreshButton = new ApplicationBarIconButton();
            refreshButton.Text = "cập nhật";
            refreshButton.IconUri = new Uri("/Assets/AppBar/refresh.png", UriKind.Relative);
            refreshButton.Click += new EventHandler(refreshButton_Click);

            var readAllButton = new ApplicationBarIconButton();
            readAllButton.Text = "đã đọc hết";
            readAllButton.IconUri = new Uri("/Assets/AppBar/check.png", UriKind.Relative);
            readAllButton.Click += new EventHandler(readAllButton_Click);

            var titleOnlyMenuItem = new ApplicationBarMenuItem();
            titleOnlyMenuItem.Text = AppConfig.ShowTitleOnly ? "hiện tóm tắt" : "chỉ hiện tiêu đề";
            titleOnlyMenuItem.Click += new EventHandler(titleOnlyMenuItem_Click);

            var unreadItemOnly = new ApplicationBarMenuItem();
            unreadItemOnly.Text = AppConfig.ShowUnreadItemOnly ? "hiện tất cả tin" : "chỉ hiện tin chưa đọc";
            unreadItemOnly.Click += new EventHandler(unreadItemOnly_Click);

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(readAllButton);
            ApplicationBar.MenuItems.Add(titleOnlyMenuItem);
            ApplicationBar.MenuItems.Add(unreadItemOnly);
        }

        #endregion

        private ItemViewModel GetItemFromContextMenu(object sender)
        {
            var dataContext = (sender as MenuItem).DataContext;
            return dataContext as ItemViewModel;
        }

        private void llsItemList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (!_viewModel.IsLoading
                && llsItemList.ItemsSource != null
                && llsItemList.ItemsSource.Count >= AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING
                && e.ItemKind == LongListSelectorItemKind.Item)
                if ((e.Container.Content as Item).Equals(llsItemList.ItemsSource[llsItemList.ItemsSource.Count - AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING]))
                {
                    this.SetProgressIndicator(true, "tải thêm tin...");

                    _pageNumber++;
                    var maxPageNumber = _viewModel.Items.GetMaxPageNumber(AppConfig.ITEM_COUNT_PER_FEED);
                    if (_pageNumber < maxPageNumber)
                        _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);
                    else
                        _pageNumber = maxPageNumber;

                    UpdateViewTitle();
                    UpdateItemReadCount();

                    this.SetProgressIndicator(false);
                }
        }

        #region Flick

        private async void OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                _pageNumber = 0;
                _viewModel.ItemViewModels.Clear();

                if (e.HorizontalVelocity < 0)
                    await LoadNextFeed();
                else
                    await LoadPreviousFeed();

                _lastItemId = string.Empty;
                _feedManager.SetLastId<string>(string.Empty);
            }
        }

        private async Task LoadNextFeed()
        {
            if (_currentIndex == _currentPubisher.FeedIds.Count - 1)
                _currentIndex = 0;
            else
                _currentIndex++;

            await Binding();
        }

        private async Task LoadPreviousFeed()
        {
            if (_currentIndex == 0 && _currentPubisher.FeedIds.Count > 0)
                _currentIndex = _currentPubisher.FeedIds.Count - 1;
            else
                _currentIndex--;

            await Binding();
        }

        #endregion

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage(1);
        }

        private void txtPublisherName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }
    }
}