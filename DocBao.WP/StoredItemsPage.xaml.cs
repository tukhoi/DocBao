using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DocBao.WP.Helper;
using DocBao.WP.ViewModels;
using DocBao.ApplicationServices;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using Microsoft.Phone.Net.NetworkInformation;
using Davang.Parser.Dto;
using Microsoft.Phone.Tasks;
using Davang.Utilities.Log;
using Davang.WP.Utilities.Extensions;

namespace DocBao.WP
{
    public partial class StoredItemsPage : DBBasePage
    {
        StoredItemsViewModel _viewModel;
        string _lastItemId;
        int _pageNumber = 0;

        public StoredItemsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();

            _viewModel = new StoredItemsViewModel();
            var lastItemId = _feedManager.GetLastId<string>();
            if (!string.IsNullOrEmpty(lastItemId))
                _lastItemId = lastItemId;

             Binding();
             base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                DisposeAll();
            else
                llsItemList.ItemsSource = null;
            base.OnNavigatedFrom(e);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            DisposeAll();
            base.OnRemovedFromJournal(e);
        }

        void DisposeAll()
        {
            llsItemList.ItemsSource = null;
            llsItemList.ItemTemplate = null;
            llsItemList.ItemRealized -= llsItemList_ItemRealized;
            ActionOnChildControls<Grid>(llsItemList, Grid.NameProperty, "grdItem", ((c) => ContextMenuService.SetContextMenu(c, null)));
        }

        private void Binding()
        {
            try
            {
                this.SetProgressIndicator(true, "đang cập nhật...");
                if (_pageNumber == 0)
                    _pageNumber = 1;

                _viewModel.Initialize();

                if (_viewModel.AllItemViewModels.Count == 0)
                {
                    Messenger.ShowToast("chưa có tin nào được lưu", completedAction: () => this.BackToPreviousPage());
                    return;
                }

                _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);
                firstNextIcon.Visibility = System.Windows.Visibility.Visible;
                UpdateViewTitle();
                UpdateItemReadCount();
                this.llsItemList.ItemsSource = _viewModel.PagedItemViewModels;

                CreateAppBar();
                llsItemList.ScrollTo<string>(_lastItemId);

                this.SetProgressIndicator(false);
            }
            catch (Exception ex)
            {
                this.SetProgressIndicator(false);
                Messenger.ShowToast("lỗi lấy dữ liệu...");
                GA.LogException(ex);
                return;
            }
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
                var item = fe.DataContext as ItemViewModel;
                if (item != null)
                {
                     _feedManager.MarkStoredItemAsRead(item.Id, true);
                    _lastItemId = item.Id;
                    var uri = string.Format("/ItemPage.xaml?feedId={0}&itemId={1}", item.FeedId, HttpUtility.UrlEncode(item.Id));
                    NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                }
            }
        }

        private void UpdateViewTitle()
        {
            if (AppConfig.ShowTitleOnly)
                _viewModel.PagedItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Collapsed);
            else
                _viewModel.PagedItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Visible);
        }

        #region AppBar

        private async void readAllButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang đánh dấu...");
            await Task.Run(() => _viewModel.AllItemViewModels.ForEach(i => _feedManager.MarkStoredItemAsRead(i.Id, true)));
            _pageNumber = 1;
            _viewModel.PagedItemViewModels.Clear();
            _lastItemId = null;
            Binding();
            this.SetProgressIndicator(false);
        }

        private void titleOnlyMenuItem_Click(object sender, EventArgs e)
        {
            AppConfig.ShowTitleOnly = !AppConfig.ShowTitleOnly;
            _pageNumber = 1;
            _viewModel.PagedItemViewModels.Clear();
            _lastItemId = null;
            Binding();
        }

        private void unreadItemOnly_Click(object sender, EventArgs e)
        {
            AppConfig.ShowUnreadItemOnly = !AppConfig.ShowUnreadItemOnly;
            _pageNumber = 1;
            _viewModel.PagedItemViewModels.Clear();
            _lastItemId = null;
            Binding();

        }

        private async void ctxStoreItem_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            this.SetProgressIndicator(true, "đang xóa...");
            var result = await _feedManager.DeleteStoredItemAsync(item.Id);
            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                Messenger.ShowToast("xóa xong...");
                Binding();
            }

            this.SetProgressIndicator(false);
        }

        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

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

            ApplicationBar.Buttons.Add(readAllButton);
            ApplicationBar.MenuItems.Add(titleOnlyMenuItem);
            ApplicationBar.MenuItems.Add(unreadItemOnly);
        }

        #endregion

        #region Context Menu

        private void ctxOpenIE_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            _feedManager.MarkStoredItemAsRead(item.Id, true);
            _lastItemId = item.Id;
            WebBrowserTask webTask = new WebBrowserTask();
            webTask.Uri = new Uri(item.Link, UriKind.Absolute);
            webTask.Show();
        }

        private void ctxMarkRead_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            var message = item.Read ? "đang đánh dấu chưa đọc..." : "đang đánh dấu đã đọc...";
            this.SetProgressIndicator(true, message);

            var result = item.Read ?
                _feedManager.MarkStoredItemAsRead(item.Id, false) :
                _feedManager.MarkStoredItemAsRead(item.Id, true);

            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                _lastItemId = item.Id;
                Binding();
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

        private ItemViewModel GetItemFromContextMenu(object sender)
        {
            var dataContext = (sender as MenuItem).DataContext;
            return dataContext as ItemViewModel;
        }

        #endregion

        private void UpdateItemReadCount()
        {
            txtReadCount.Text = _viewModel.ReadStats;
        }

        private void llsItemList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (!_viewModel.IsLoading
                && llsItemList.ItemsSource != null
                && llsItemList.ItemsSource.Count >= AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING
                && e.ItemKind == LongListSelectorItemKind.Item)
                if ((e.Container.Content as ItemViewModel).Equals(llsItemList.ItemsSource[llsItemList.ItemsSource.Count - AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING]))
                {
                    this.SetProgressIndicator(true, "tải thêm tin...");

                    _pageNumber++;
                    var maxPageNumber = _viewModel.AllItemViewModels.GetMaxPageNumber(AppConfig.ITEM_COUNT_PER_FEED);
                    if (_pageNumber < maxPageNumber)
                        _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);
                    else
                        _pageNumber = maxPageNumber;

                    UpdateViewTitle();
                    UpdateItemReadCount();

                    this.SetProgressIndicator(false);
                }
        }

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }
    }
}