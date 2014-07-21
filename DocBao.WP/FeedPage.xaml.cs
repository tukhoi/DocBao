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
using DocBao.ApplicationServices.RssService;
using Davang.Utilities.Log;
using Davang.WP.Utilities.Extensions;
using DocBao.ApplicationServices.UserBehavior;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Davang.WP.Utilities.Helper;

namespace DocBao.WP
{
    public partial class FeedPage : DBBasePage
    {
        RssParserService _rssParser = RssParserService.Instance;
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
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();

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
            BindingNavBar();
            if (newItemCount > 0)
                Messenger.ShowToast(newItemCount + " tin mới");
            if (newItemCount == -1)
                Messenger.ShowToast("lấy tin mới bị lỗi...");

            //var previousPage = NavigationService.BackStack.First().Source;
            //if (previousPage.ToString().Contains("FeedPage.xaml") || previousPage.ToString().Contains("ItemPage.xaml"))
            //    NavigationService.RemoveBackEntry(); 

            SetSecondPage();

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

        private void DisposeAll()
        {
            ActionOnChildControls<Grid>(llsItemList, Grid.NameProperty, "grdItem", ((c) => ContextMenuService.SetContextMenu(c, null)));
            llsItemList.ItemsSource = null;
            llsItemList.ItemTemplate = null;
            _viewModel.Dispose();
            llsItemList.ItemRealized -= llsItemList_ItemRealized;
            ContentPanel.ManipulationCompleted -= ContentPanel_ManipulationCompleted;

            adControl = null;
        }

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

                updated = await _viewModel.RefreshLatestData(feedId, _currentPubisher.Id, requireUpdate);
                _feedManager.SetLastId<Guid>(feedId.ToString());

                if (_viewModel.AllItemViewModels.Count == 0)
                {
                    this.SetProgressIndicator(false);
                    Messenger.ShowToast("chưa có tin nào...", completedAction: () => this.BackToPreviousPage());
                    return 0;
                }

                if (updated > 0)
                {
                    _viewModel.PagedItemViewModels.Clear();
                    _pageNumber = 1;
                }

                _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);

                UpdateItemReadCount();
                UpdateViewTitle();
                this.llsItemList.ItemsSource = _viewModel.PagedItemViewModels;

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
                if (goBackOnFail)
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

            var subscribedPublisher = _feedManager.GetSubscribedPublishers();
            subscribedPublisher.Target.ForEach(p =>
                {
                    navBarViewModel.FirstBrothers.Add(new Brother()
                        {
                            Id = p.Id.ToString(),
                            Name = p.Name,
                            ImageUri = p.ImageUri.ToString().StartsWith("/") ? p.ImageUri : new Uri("/" + p.ImageUri.ToString(), UriKind.Relative),
                            Stats = PublisherHelper.GetStatsString(p.Id),
                            Selected = p.Id.Equals(_viewModel.Publisher.Id),
                            NavigateUri = new Uri(string.Format("/FeedPage.xaml?publisherId={0}&feedId={1}", p.Id, p.FeedIds[0]), UriKind.Relative)
                        });
                });

            _currentPubisher.FeedIds.ForEach(f =>
                {
                    var feedResult = _feedManager.GetSubscribedFeed(f);
                    if (feedResult.HasError) return;

                    navBarViewModel.SecondBrothers.Add(new Brother() 
                        {
                            Id = f.ToString(),
                            Name = feedResult.Target.Name,
                            ImageUri = null,
                            Stats = FeedHelper.GetStatsString(f),
                            Selected = f.Equals(_viewModel.Id),
                            NavigateUri = new Uri(string.Format("/FeedPage.xaml?publisherId={0}&feedId={1}", feedResult.Target.Publisher.Id, f), UriKind.Relative)
                        });
                });

            //NavBar.SelectedFirstBrotherId = _currentPubisher.Id.ToString();
            //NavBar.SelectedSecondBrotherId = _viewModel.Id.ToString();
            NavBar.Binding(navBarViewModel);
            NavBar.Navigation = ((uri) => NavigationService.Navigate(uri));
            NavBar.NavigateHome = (() => this.BackToMainPage());
        }

        private void UpdateItemReadCount()
        {
            txtReadCount.Text = _viewModel.ReadStats;
            //txtLastUpdated.Text = FeedHelper.GetUpdateStats(_viewModel.Id);
        }

        private void UpdateViewTitle()
        {
            if (AppConfig.ShowTitleOnly)
                _viewModel.PagedItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Collapsed);
            else
                _viewModel.PagedItemViewModels.ForEach(i => i.SummaryVisibility = System.Windows.Visibility.Visible);
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
                    _feedManager.MarkItemAsRead(_viewModel.Id, item.Id, true);
                    _lastItemId = item.Id;
                    var uri = string.Format("/ItemPage.xaml?feedId={0}&itemId={1}", item.FeedId, HttpUtility.UrlEncode(item.Id));
                    UserBehaviorManager.Instance.Log(UserAction.ItemClick, item.FeedId.ToString());
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
            }

            this.SetProgressIndicator(false);
        }

        #endregion

        #region AppBar

        private async void refreshButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang cập nhật...");
            this._lastItemId = string.Empty;
            var updated = await Binding(true, false);
            if (updated > 0)
                Messenger.ShowToast(updated + " tin mới");
            if (updated == -1)
                Messenger.ShowToast("lấy tin mới bị lỗi...");

            this.SetProgressIndicator(false);
        }

        private async void readAllButton_Click(object sender, EventArgs e)
        {
            this.SetProgressIndicator(true, "đang đánh dấu...");
            await Task.Run(() => _viewModel.AllItemViewModels.ForEach(i => _feedManager.MarkItemAsRead(_viewModel.Id, i.Id, true)));
            _pageNumber = 1;
            _viewModel.PagedItemViewModels.Clear();
            _lastItemId = null;
            await Binding();
            this.SetProgressIndicator(false);
        }

        private void feedPickUpButton_Click(object sender, EventArgs e)
        {
            var uri = new Uri("/FeedPickupPage.xaml?publisherId=" + _viewModel.Publisher.Id, UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private async void titleOnlyMenuItem_Click(object sender, EventArgs e)
        {
            AppConfig.ShowTitleOnly = !AppConfig.ShowTitleOnly;
            _pageNumber = 1;
            _viewModel.PagedItemViewModels.Clear();
            _lastItemId = null;
            await Binding();
        }

        private async void unreadItemOnly_Click(object sender, EventArgs e)
        {
            AppConfig.ShowUnreadItemOnly = !AppConfig.ShowUnreadItemOnly;
            _pageNumber = 1;
            _viewModel.PagedItemViewModels.Clear();
            _lastItemId = null;
            await Binding();

        }

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

            var feedPickUpButton = new ApplicationBarMenuItem();
            feedPickUpButton.Text = "cài/gỡ chuyên mục";
            feedPickUpButton.Click += new EventHandler(feedPickUpButton_Click);

            var titleOnlyMenuItem = new ApplicationBarMenuItem();
            titleOnlyMenuItem.Text = AppConfig.ShowTitleOnly ? "hiện tóm tắt" : "chỉ hiện tiêu đề";
            titleOnlyMenuItem.Click += new EventHandler(titleOnlyMenuItem_Click);

            var unreadItemOnly = new ApplicationBarMenuItem();
            unreadItemOnly.Text = AppConfig.ShowUnreadItemOnly ? "hiện tất cả tin" : "chỉ hiện tin chưa đọc";
            unreadItemOnly.Click += new EventHandler(unreadItemOnly_Click);

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(readAllButton);
            ApplicationBar.MenuItems.Add(feedPickUpButton);
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
                if ((e.Container.Content as ItemViewModel).Equals(llsItemList.ItemsSource[llsItemList.ItemsSource.Count - AppConfig.ITEM_COUNT_BEFORE_NEXT_LOADING]))
                {
                    this.SetProgressIndicator(true, "tải thêm tin...");

                    _pageNumber++;
                    var maxPageNumber = _viewModel.AllItemViewModels.GetMaxPageNumber(AppConfig.ITEM_COUNT_PER_FEED);
                    if (_pageNumber <= maxPageNumber)
                        _viewModel.LoadPage(_pageNumber, AppConfig.ShowUnreadItemOnly);
                    else
                        _pageNumber = maxPageNumber;

                    UpdateViewTitle();
                    UpdateItemReadCount();

                    this.SetProgressIndicator(false);
                }
        }

        #region Flick

        private async void OnFlick(object sender, ManipulationCompletedEventArgs e)
        {
            if (_currentPubisher.FeedIds.Count == 1) return;

            Point transformedVelocity = GestureHelper.GetTransformNoTranslation(transform).Transform(e.FinalVelocities.LinearVelocity);
            double horizontalVelocity = transformedVelocity.X;
            double verticalVelocity = transformedVelocity.Y;

            var direction = GestureHelper.GetDirection(horizontalVelocity, verticalVelocity);
            if (direction == System.Windows.Controls.Orientation.Horizontal)
            {
                _pageNumber = 0;
                _viewModel.PagedItemViewModels.Clear();

                if (horizontalVelocity < 0)
                    await LoadNextFeed();
                else
                    await LoadPreviousFeed();

                _lastItemId = string.Empty;
                _feedManager.SetLastId<string>(string.Empty);
            }


            //if (_currentPubisher.FeedIds.Count == 1) return;
            //if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            //{
            //    _pageNumber = 0;
            //    _viewModel.PagedItemViewModels.Clear();

            //    if (e.HorizontalVelocity < 0)
            //        await LoadNextFeed();
            //    else
            //        await LoadPreviousFeed();

            //    _lastItemId = string.Empty;
            //    _feedManager.SetLastId<string>(string.Empty);
            //}
        }

        //private GeneralTransform GetTransformNoTranslation(CompositeTransform transform)
        //{
        //    CompositeTransform newTransform = new CompositeTransform();
        //    newTransform.Rotation = transform.Rotation;
        //    newTransform.ScaleX = transform.ScaleX;
        //    newTransform.ScaleY = transform.ScaleY;
        //    newTransform.CenterX = transform.CenterX;
        //    newTransform.CenterY = transform.CenterY;
        //    newTransform.TranslateX = 0;
        //    newTransform.TranslateY = 0;

        //    return newTransform;
        //}

        //private Orientation GetDirection(double x, double y)
        //{
        //    return Math.Abs(x) >= Math.Abs(y) ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
        //}

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

        //private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    //if (_currentPubisher.FeedIds.Count == 1)
        //    //    this.BackToPreviousPage();
        //    //else
        //    //    this.BackToPreviousPage(1);

        //    this.BackToMainPage();
        //}

        private void txtPublisherName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //if (_currentPubisher.FeedIds.Count == 1) return;
            this.BackToPreviousPage();
        }

        private void ContentPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (e.IsInertial)
                this.OnFlick(sender, e);
        }
    }
}