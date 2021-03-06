﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DocBao.WP.Helper;
using Davang.Utilities.Helpers;
using DocBao.ApplicationServices;
using DocBao.WP.ViewModels;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices.Bank;
using System.Threading.Tasks;
using Microsoft.Phone.Net.NetworkInformation;
using Davang.Parser.Dto;
using Microsoft.Phone.Tasks;
using Davang.Utilities.Log;
using Davang.WP.Utilities.Extensions;
using Davang.WP.Utilities.Helper;
using System.Windows.Input;
using DocBao.ApplicationServices.UserBehavior;
using Davang.WP.Utilities;

namespace DocBao.WP
{
    public partial class CategoryPage : DBBasePage
    {
        int _pageNumber = 0;
        string _lastItemId;
        CategoryModel _viewModel = new CategoryModel();
        int _currentIndex = -1;
        Guid _catIdFromQS = default(Guid);

        public CategoryPage()
        {
            InitializeComponent();

            if (LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                adControl.Visibility = System.Windows.Visibility.Collapsed;
            else
                adControl.Visibility = System.Windows.Visibility.Visible;
        }

        #region Load

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();

            if (default(Guid).Equals(_catIdFromQS))
                _catIdFromQS = NavigationContext.QueryString.GetQueryStringToGuid("categoryId");

            await Binding();

            var previousPage = NavigationService.BackStack.First().Source;
            if (previousPage.ToString().Contains("ItemPage.xaml"))
                SetAsSecondPage();
            base.OnNavigatedTo(e);
        }

        async Task Binding()
        {
            var lastItemId = _feedManager.GetLastId<string>();
            if (!string.IsNullOrEmpty(lastItemId))
                _lastItemId = lastItemId;

            var categories = _feedManager.GetCategories();
            //if _currentIndex hasn't been initialized (-1) then initialize it
            //otherwise leave it as-is
            //_currentIndex = _currentIndex == -1 || findNewCurrentIndex
            //    ? categories.IndexOf(categories.FirstOrDefault(c => c.Id.Equals(_catIdFromQS))) 
            //    : _currentIndex;

            _currentIndex = categories.IndexOf(categories.FirstOrDefault(c => c.Id.Equals(_catIdFromQS)));
            if (_currentIndex == -1 && categories.Count > 0)
                _currentIndex = 0;

            var newItemCount = await BindingContent();
            BindingNavBar();
            if (newItemCount > 0)
                Messenger.ShowToast(newItemCount + " tin mới");
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
            ActionOnChildControls<Grid>(llsItemList, Grid.NameProperty, "grdItem", ((c) => ContextMenuService.SetContextMenu(c, null)));
            llsItemList.ItemsSource = null;
            llsItemList.ItemTemplate = null;
            llsItemList.ItemRealized -= llsItemList_ItemRealized;
            ContentPanel.ManipulationCompleted -= ContentPanel_ManipulationCompleted;
            adControl = null;
            NavBar.SelectedEvent -= NavBar_SelectedEvent;
            NavBar.Dispose();
        }

        private async Task<int> BindingContent(bool refresh = false, bool gobackOnFail = true)
        {
            try
            {
                int updated = 0;
                var currentCategory = _feedManager.GetCategories()[_currentIndex];

                var publisherCount = currentCategory.Feeds.Select(f => f.Publisher).Distinct(new PublisherComparer()).Count();

                this.SetProgressIndicator(true, "đang cập nhật từ " + publisherCount + " báo...");
                if (_pageNumber == 0)
                    _pageNumber = 1;

                if (refresh && !NetworkInterface.GetIsNetworkAvailable())
                {
                    Messenger.ShowToast("không có mạng...");
                    refresh = false;
                }

                updated = await _viewModel.RefreshLatestData(currentCategory.Id, refresh);
                _feedManager.SetLastId<Guid>(currentCategory.Id.ToString());

                if (_viewModel.Items.Count == 0)
                {
                    this.SetProgressIndicator(false);
                    Messenger.ShowToast("chưa có tin nào...");
                    this.BackToPreviousPage();
                    return 0;
                }

                if (updated > 0)
                {
                    _viewModel.ItemViewModels.Clear();
                    _pageNumber = 1;
                }

                _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);
                
                UpdateItemReadCount();
                UpdateViewTitle();
                this.llsItemList.ItemsSource = _viewModel.ItemViewModels;
                CreateAppBar();

                if (updated > 0)
                    llsItemList.ScrollToTop();
                else
                    llsItemList.ScrollTo<string>(_lastItemId);

                this.SetProgressIndicator(false);

                return updated;
            }
            catch (Exception ex)
            {
                this.SetProgressIndicator(false);
                var message = string.Format("mục này đang bị lỗi...");
                if (gobackOnFail)
                    Messenger.ShowToast(message, completedAction: (() => this.BackToPreviousPage()));
                else
                    Messenger.ShowToast(message);

                GA.LogException(ex);
                return 0;
            }
        }

        private void BindingNavBar()
        {
            var navBarViewModel = new NavBarViewModel();

            var categories = _feedManager.GetCategories();
            categories.ForEach(c =>
            {
                navBarViewModel.FirstBrothers.Add(new Brother()
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    ImageUri = c.ImageUri.ToString().StartsWith("/") ? c.ImageUri : new Uri("/" + c.ImageUri.ToString(), UriKind.Relative),
                    Stats = CategoryHelper.GetStatsString(c.Id),
                    Selected = c.Id.Equals(_viewModel.Id),
                    PostAction = ViewModels.PostAction.Binding,
                    BindingData = new BindingData() 
                        {
                            CategoryId = c.Id,
                            PublisherId = default(Guid),
                            FeedId = default(Guid)
                        }
                });
            });

            NavBar.BindingNavBar(navBarViewModel);
            NavBar.SelectedEvent -= NavBar_SelectedEvent;
            NavBar.SelectedEvent += NavBar_SelectedEvent;
            NavBar.NavigateHome = (() => this.BackToMainPage());
        }

        Task NavBar_SelectedEvent(BindingData bindingData)
        {
            return Task.Run(() =>
                {
                    UserBehaviorManager.Instance.Log(UserAction.CatEnter, bindingData.CategoryId.ToString());
                    _viewModel.ItemViewModels.Clear();
                    _catIdFromQS = bindingData.CategoryId;
                    _lastItemId = null;
                    _feedManager.SetLastId<string>(null);
                    _pageNumber = 0;
                    //await Binding(true);
                });
        }

        #endregion

        #region Event Handler

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
                    _feedManager.MarkItemAsRead(item.FeedId, item.Id, true);
                    _lastItemId = item.Id;
                    var uri = string.Format("/ItemPage.xaml?feedId={0}&itemId={1}&categoryId={2}", item.FeedId, HttpUtility.UrlEncode(item.Id), _feedManager.GetCategories()[_currentIndex].Id);
                    UserBehaviorManager.Instance.Log(UserAction.ItemEnter, item.Id.ToString());
                    NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                }
            }
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
                    var maxPageNumber = _viewModel.Items.GetMaxPageNumber(AppConfig.ITEM_COUNT_PER_FEED);
                    if (_pageNumber <= maxPageNumber)
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

        #endregion

        #region Flick

        private void ContentPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (e.IsInertial)
                this.OnFlick(sender, e);
        }

        private async void OnFlick(object sender, ManipulationCompletedEventArgs e)
        {
            Point transformedVelocity = GestureHelper.GetTransformNoTranslation(transform).Transform(e.FinalVelocities.LinearVelocity);
            double horizontalVelocity = transformedVelocity.X;
            double verticalVelocity = transformedVelocity.Y;

            var direction = GestureHelper.GetDirection(horizontalVelocity, verticalVelocity);

            if (direction == System.Windows.Controls.Orientation.Horizontal)
            {
                _pageNumber = 0;
                _viewModel.ItemViewModels.Clear();

                if (horizontalVelocity < 0)
                    LoadNextCategory();
                else
                    LoadPreviousCategory();

                _catIdFromQS = _feedManager.GetCategories()[_currentIndex].Id;
                UserBehaviorManager.Instance.Log(UserAction.CatEnter, _catIdFromQS.ToString());
                await BindingContent();
                BindingNavBar();

                _lastItemId = string.Empty;
                _feedManager.SetLastId<string>(string.Empty);
            }
        }

        private void LoadNextCategory()
        {
            if (_currentIndex == _feedManager.GetCategories().Count - 1)
                _currentIndex = 0;
            else
                _currentIndex++;
        }

        private void LoadPreviousCategory()
        {
            if (_currentIndex == 0 && _feedManager.GetCategories().Count > 0)
                _currentIndex = _feedManager.GetCategories().Count - 1;
            else
                _currentIndex--;
        }

        #endregion

        #region Context Menu

        private void ctxOpenIE_Click(object sender, RoutedEventArgs e)
        {
            var item = GetItemFromContextMenu(sender);
            if (item == null) return;

            _feedManager.MarkItemAsRead(item.FeedId, item.Id, true);
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

            var result = item.Read ? _feedManager.MarkItemAsRead(item.FeedId, item.Id, false) : _feedManager.MarkItemAsRead(item.FeedId, item.Id, true);
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

        #endregion

        #region AppBar

        private void CreateAppBar()
        {
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

        private async void refreshButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang cập nhật...");
            var updated = await BindingContent(true, false);
            if (updated > 0)
                Messenger.ShowToast(updated + " tin mới");
            this.SetProgressIndicator(false);
        }

        private async void readAllButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang đánh dấu...");
            await Task.Run(() => _viewModel.Items.ForEach(i => _feedManager.MarkItemAsRead(i.FeedId, i.Id, true)));
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

        #endregion

        #region Private

        private ItemViewModel GetItemFromContextMenu(object sender)
        {
            var dataContext = (sender as MenuItem).DataContext;
            return dataContext as ItemViewModel;
        }

        private void UpdateItemReadCount()
        {
            txtReadCount.Text = _viewModel.ReadStats;
            //txtLastUpdated.Text = CategoryHelper.GetUpdateStats(_viewModel.Id);
        }

        private void UpdateViewTitle()
        {
            if (AppConfig.ShowTitleOnly)
                _viewModel.ItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Collapsed);
            else
                _viewModel.ItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Visible);
        }

        #endregion
    }
}