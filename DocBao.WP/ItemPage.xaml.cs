using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices;
using Davang.Parser.Dto;
using Microsoft.Phone.Tasks;
using Davang.Utilities.Helpers;
using DocBao.WP.Helper;
using Microsoft.Phone.Net.NetworkInformation;
using DocBao.ApplicationServices.UserBehavior;
using System.Diagnostics;
using DocBao.WP.ViewModels;
using System.Windows.Input;
using Davang.WP.Utilities.Helper;
using SOMAWP8;

namespace DocBao.WP
{
    public partial class ItemPage : DBBasePage
    {
        FeedViewModel _itemContainer;
        int _currentIndex=-1;
        private bool _webLoaded = false;
        private PreviousPage _previousPage = PreviousPage.FeedPage;
        WebBrowser _wbContent;

        public ItemPage()
        {
            InitializeComponent();

            BindingWebBrowser();

            if (LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                adControl.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                adControl.Visibility = System.Windows.Visibility.Visible;
                adControl.StartAds();
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();
            if (_webLoaded)
                return;

            var feedId = NavigationContext.QueryString.GetQueryStringToGuid("feedId");
            var categoryId = NavigationContext.QueryString.GetQueryStringToGuid("categoryId");
            var itemId = HttpUtility.UrlDecode(NavigationContext.QueryString.GetQueryString("itemId"));
            
            var previousPage = NavigationService.BackStack.First().Source;
            if (previousPage.ToString().Contains("FeedPage.xaml"))
                _previousPage = PreviousPage.FeedPage;
            else if (previousPage.ToString().Contains("StoredItemsPage.xaml"))
                _previousPage = PreviousPage.StoredItemsPage;
            else if (previousPage.ToString().Contains("CategoryPage.xaml"))
                _previousPage = PreviousPage.CategoryPage;

            BindItemListToFlick(feedId, categoryId);

            _currentIndex = _currentIndex == -1 ? _itemContainer.AllItemViewModels.IndexOf(_itemContainer.AllItemViewModels.FirstOrDefault(i => i.Id.Equals(itemId))) : _currentIndex;
            if (_currentIndex == -1 && _itemContainer.AllItemViewModels.Count > 0)
                _currentIndex = 0;

            Binding();
            BindingNavBar();
            CreateAppBar();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                DisposeAll();
                        
            base.OnNavigatingFrom(e);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            DisposeAll();
            base.OnRemovedFromJournal(e);
        }

        private void DisposeAll()
        {
            _wbContent.Navigating -= wbContent_Navigating;
            _wbContent.Navigated -= wbsContent_Navigated;
            _wbContent.LoadCompleted -= wbContent_LoadCompleted;
            _wbContent.IsScriptEnabled = false;
            _wbContent.NavigateToString("<html></html>");
            WBContainer.Child = null;
            brdTitle.ManipulationCompleted -= Border_ManipulationCompleted;
            _itemContainer.Dispose();
            adControl.StopAds();
            adControl.Dispose();
            NavBar.Dispose();
        }

        void wbContent_Navigating(object sender, NavigatingEventArgs e)
        {
            this.SetProgressIndicator(true, "đang mở...");
        }

        void wbsContent_Navigated(object sender, NavigationEventArgs e)
        {
            this.SetProgressIndicator(true, "đang tải...");
        }

        void wbContent_LoadCompleted(object sender, NavigationEventArgs e)
        {
            this.SetProgressIndicator(false);
            _webLoaded = true;
        }

        void Binding()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Messenger.ShowToast("không có mạng...");
                return;
            }

            var item = _itemContainer.AllItemViewModels[_currentIndex];
            if (item != null)
            {
                txtItemTitle.Text = item.Title;
                if (_previousPage == PreviousPage.FeedPage || _previousPage == PreviousPage.CategoryPage)
                    _feedManager.MarkItemAsRead(item.FeedId, item.Id, true);
                else
                    _feedManager.MarkStoredItemAsRead(item.Id, true);
                CreateAppBar();
                _feedManager.SetLastId<string>(item.Id);
                
                _wbContent.Navigate(new Uri(item.Link, UriKind.Absolute));
            }
        }

        #region NavBar

        private void BindingNavBar()
        {
            switch (_previousPage)
            { 
                case PreviousPage.FeedPage:
                    BindingNavBarForPrevFeedPage();
                    break;
                case PreviousPage.CategoryPage:
                    BindingNavBarForPrevCatPage();
                    break;
                case PreviousPage.StoredItemsPage:
                    BindingNavBarForPrevStoredItemsPage();
                    break;
            }
        }

        private void BindingNavBarForPrevFeedPage()
        {
            var navBarViewModel = new NavBarViewModel();
            var item = _itemContainer.AllItemViewModels[_currentIndex];

            var subscribedPublisher = _feedManager.GetSubscribedPublishers();
            subscribedPublisher.Target.ForEach(p =>
            {
                navBarViewModel.FirstBrothers.Add(new Brother()
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    ImageUri = p.ImageUri.ToString().StartsWith("/") ? p.ImageUri : new Uri("/" + p.ImageUri.ToString(), UriKind.Relative),
                    Stats = PublisherHelper.GetStatsString(p.Id),
                    Selected = p.Id.Equals(_itemContainer.Publisher.Id),
                    PostAction = ViewModels.PostAction.Navigation,
                    NavigateUri = new Uri(string.Format("/FeedPage.xaml?publisherId={0}&feedId={1}", p.Id, p.FeedIds[0]), UriKind.Relative)
                });
            });

            navBarViewModel.FirstBrothers.Add(new Brother()
            {
                Id = string.Empty,
                Name = string.Empty,
                ImageUri = new Uri("/Images/publishers/install.png", UriKind.Relative),
                Stats = "cài thêm hoặc gỡ bớt báo",
                Selected = false,
                PostAction = PostAction.Navigation,
                NavigateUri = new Uri("/PublisherPickupPage.xaml", UriKind.Relative)
            });

            _itemContainer.Publisher.FeedIds.ForEach(f =>
            {
                var feedResult = _feedManager.GetSubscribedFeed(f);
                if (feedResult.HasError) return;

                navBarViewModel.SecondBrothers.Add(new Brother()
                {
                    Id = f.ToString(),
                    Name = feedResult.Target.Name,
                    ImageUri = null,
                    Stats = FeedHelper.GetStatsString(f),
                    Selected = f.Equals(_itemContainer.Id),
                    PostAction = ViewModels.PostAction.Navigation,
                    NavigateUri = new Uri(string.Format("/FeedPage.xaml?publisherId={0}&feedId={1}", _itemContainer.Publisher.Id, f), UriKind.Relative)
                });
            });

            if (_itemContainer.Publisher.FeedIds.Count > 1)
                navBarViewModel.SecondBrothers.Add(new Brother()
                {
                    Id = string.Empty,
                    Name = "Cài thêm chuyên mục",
                    ImageUri = null,
                    Stats = "hoặc gỡ bớt chuyên mục không thích",
                    Selected = false,
                    PostAction = PostAction.Navigation,
                    NavigateUri = new Uri("/FeedPickupPage.xaml?publisherId=" + _itemContainer.Publisher.Id.ToString(), UriKind.Relative)
                });

            NavBar.BindingNavBar(navBarViewModel);
            NavBar.Navigation = ((uri, id) =>
                {
                    DisposeAll();
                    if (!id.Equals(_itemContainer.Publisher.Id))
                        UserBehaviorManager.Instance.Log(UserAction.PubEnter, _itemContainer.Publisher.Id.ToString());
                    if (!id.Equals(_itemContainer.Id))
                        UserBehaviorManager.Instance.Log(UserAction.FeedEnter, _itemContainer.Id.ToString());

                    NavigationService.Navigate(uri);
                });
            NavBar.NavigateHome = (() => this.BackToMainPage());
        }

        private void BindingNavBarForPrevCatPage()
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
                    Selected = c.Id.Equals(_itemContainer.Id),
                    PostAction = ViewModels.PostAction.Navigation,
                    NavigateUri = new Uri(string.Format("/CategoryPage.xaml?categoryId={0}", c.Id), UriKind.Relative)
                });
            });

            NavBar.BindingNavBar(navBarViewModel);
            NavBar.Navigation = ((uri, id) => 
                { 
                    DisposeAll();
                    if (!id.Equals(_itemContainer.Id))
                        UserBehaviorManager.Instance.Log(UserAction.CatEnter, id);
                    NavigationService.Navigate(uri); 
                });
            NavBar.NavigateHome = (() => this.BackToMainPage());
        }

        private void BindingNavBarForPrevStoredItemsPage()
        {
            var navBarViewModel = new NavBarViewModel();

            navBarViewModel.SecondBrothers.Add(new Brother()
            {
                Id = null,
                Name = "tin đã lưu",
                ImageUri = null,
                Stats = "lưu các tin hay trên điện thoại",
                Selected = true,
                NavigateUri = null
            });

            navBarViewModel.SecondBrothers.Add(new Brother()
            {
                Id = null,
                Name = "xem theo báo",
                ImageUri = null,
                Stats = "xem theo các đầu báo đã cài",
                Selected = true,
                PostAction = ViewModels.PostAction.Navigation,
                NavigateUri = new Uri("/HubTilePage.xaml", UriKind.Relative)
            });

            navBarViewModel.SecondBrothers.Add(new Brother()
            {
                Id = null,
                Name = "xem theo mục",
                ImageUri = null,
                Stats = "xem nhóm các báo theo chuyên mục",
                Selected = true,
                PostAction = ViewModels.PostAction.Navigation,
                NavigateUri = new Uri("/CustomViewPage.xaml", UriKind.Relative)
            });

            NavBar.BindingNavBar(navBarViewModel);
            NavBar.Navigation = ((uri, id) => 
                { 
                    DisposeAll(); 
                    NavigationService.Navigate(uri); 
                });
            NavBar.NavigateHome = (() => this.BackToMainPage());
        }

        #endregion

        private void BindingWebBrowser()
        {
            _wbContent = WebBrowserManager.WebBrowser;
            _wbContent.Navigating += wbContent_Navigating;
            _wbContent.Navigated += wbsContent_Navigated;
            _wbContent.LoadCompleted += wbContent_LoadCompleted;
            _wbContent.Width = WBContainer.Width;
            _wbContent.Height = WBContainer.Height;
            _wbContent.IsScriptEnabled = MemoryHelper.IsLowMemDevice ? false : true;

            WBContainer.Child = _wbContent;
        }

        #region AppBar

        private void CreateAppBar()
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            var ieButton = new ApplicationBarIconButton();
            ieButton.Text = "mở bằng IE";
            ieButton.IconUri = new Uri("/Assets/AppBar/ie-icon.png", UriKind.Relative);
            ieButton.Click += new EventHandler(ieButton_Click);

            var updateButton = new ApplicationBarIconButton();
            updateButton.Text = "cập nhật";
            updateButton.IconUri = new Uri("/Assets/AppBar/refresh.png", UriKind.Relative);
            updateButton.Click += new EventHandler(refreshButton_Click);

            var markReadButton = new ApplicationBarIconButton();
            markReadButton.Text = item.Read ? "chưa đọc" : "đã đọc";
            var iconUrl = item.Read ? "/Assets/AppBar/cancel.png" : "/Assets/AppBar/check.png";
            markReadButton.IconUri = new Uri(iconUrl, UriKind.Relative);
            markReadButton.Click += new EventHandler(unreadButton_Click);

            var storeButton = new ApplicationBarIconButton();
            storeButton.Text = _feedManager.IsStored(item.Id) ? "xóa tin" : "lưu tin";
            var storeIconUrl = _feedManager.IsStored(item.Id) ? "/Assets/AppBar/minus.png" : "/Assets/AppBar/new.png";
            storeButton.IconUri = new Uri(storeIconUrl, UriKind.Relative);
            storeButton.Click += new EventHandler(storeButton_Click);

            var emailMenuItem = new ApplicationBarMenuItem();
            emailMenuItem.Text = "gởi qua email";
            emailMenuItem.Click += new EventHandler(emailButton_Click);

            var copyLinkMenuItem = new ApplicationBarMenuItem();
            copyLinkMenuItem.Text = "chép link";
            copyLinkMenuItem.Click += new EventHandler(copyLinkButton_Click);

            var facebookMenuItem = new ApplicationBarMenuItem();
            facebookMenuItem.Text = "đăng lên fb";
            facebookMenuItem.Click += new EventHandler(facebookButton_Click);

            var showTitleMenuItem = new ApplicationBarMenuItem();
            showTitleMenuItem.Text = AppConfig.ShowItemTitle ? "tắt tiêu đề" : "hiện tiêu đề";
            showTitleMenuItem.Click += new EventHandler(showTitleMenuItem_Click);

            ApplicationBar.Buttons.Add(updateButton);
            ApplicationBar.Buttons.Add(ieButton);
            ApplicationBar.Buttons.Add(markReadButton);
            ApplicationBar.Buttons.Add(storeButton);
            ApplicationBar.MenuItems.Add(emailMenuItem);
            ApplicationBar.MenuItems.Add(copyLinkMenuItem);
            ApplicationBar.MenuItems.Add(facebookMenuItem);
            ApplicationBar.MenuItems.Add(showTitleMenuItem);
        }

        private void ieButton_Click(object sender, EventArgs e)
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];
            if (item == null) return;

            WebBrowserTask webTask = new WebBrowserTask();
            webTask.Uri = new Uri(item.Link, UriKind.Absolute);
            webTask.Show();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            Binding();
        }

        private void emailButton_Click(object sender, EventArgs e)
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = "Gởi từ app duyệt báo: " + item.Title;
            emailComposeTask.Body = item.Link;
            emailComposeTask.Show();
            UserBehaviorManager.Instance.Log(UserAction.ItemEmail, item.FeedId.ToString());
        }

        private void copyLinkButton_Click(object sender, EventArgs e)
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];

            if (!string.IsNullOrEmpty(item.Link))
            {
                Clipboard.SetText(item.Link);
                Messenger.ShowToast("đã chép link");
            }
            else
                Messenger.ShowToast("không tìm thấy link");
            UserBehaviorManager.Instance.Log(UserAction.ItemLink, item.FeedId.ToString());
        }

        private void facebookButton_Click(object sender, EventArgs e)
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];

            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = "gởi từ app duyệt báo: " + item.Title;
            shareLinkTask.LinkUri = new Uri(item.Link, UriKind.Absolute);
            shareLinkTask.Message = item.Link;
            shareLinkTask.Show();
            UserBehaviorManager.Instance.Log(UserAction.ItemShare, item.FeedId.ToString());
        }

        private async void storeButton_Click(object sender, EventArgs e)
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];
            if (item == null) return;

            var stored = _feedManager.IsStored(item.Id);
            var message = stored ? "đang xóa..." : "đang lưu...";
            var doneMessage = stored ? "xóa xong..." : "lưu xong...";

            this.SetProgressIndicator(true, message);
            var result = stored ? await _feedManager.DeleteStoredItemAsync(item.Id) : await _feedManager.StoreItemAsync(item.ToItem());
            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                Messenger.ShowToast(doneMessage);
                CreateAppBar();
            }

            this.SetProgressIndicator(false);
            UserBehaviorManager.Instance.Log(UserAction.ItemStore, item.FeedId.ToString());
        }

        private void showTitleMenuItem_Click(object sender, EventArgs e)
        {
            AppConfig.ShowItemTitle = !AppConfig.ShowItemTitle;
            txtItemTitle.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
            brdTitle.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;            
            itemNextIcon.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
            CreateAppBar();
        }

        private void unreadButton_Click(object sender, EventArgs e)
        {
            var item = _itemContainer.AllItemViewModels[_currentIndex];
            var message = item.Read ? "đang đánh dấu chưa đọc..." : "đang đánh dấu đã đọc...";
            this.SetProgressIndicator(true, message);

            AppResult<bool> result;
            if (_previousPage == PreviousPage.FeedPage || _previousPage == PreviousPage.CategoryPage)
                result = item.Read ? _feedManager.MarkItemAsRead(item.FeedId, item.Id, false) : _feedManager.MarkItemAsRead(item.FeedId, item.Id, true);
            else
                result = item.Read ? _feedManager.MarkStoredItemAsRead(item.Id, false) : _feedManager.MarkStoredItemAsRead(item.Id, true);

            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                var doneMessage = item.Read ? "đánh dấu chưa đọc xong..." : "đánh dấu đã đọc xong...";
                Messenger.ShowToast("đánh dấu xong");
                CreateAppBar();
            }

            this.SetProgressIndicator(false);
        }

        #endregion

        #region Flick

        private void Border_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            this.OnFlick(sender, e);
        }

        private void OnFlick(object sender, ManipulationCompletedEventArgs e)
        {
            if (_itemContainer.AllItemViewModels.Count == 1) return;

            Point transformedVelocity = GestureHelper.GetTransformNoTranslation(transform).Transform(e.FinalVelocities.LinearVelocity);
            double horizontalVelocity = transformedVelocity.X;
            double verticalVelocity = transformedVelocity.Y;

            var direction = GestureHelper.GetDirection(horizontalVelocity, verticalVelocity);
            if (direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (horizontalVelocity < 0)
                    LoadNextItem();
                else
                    LoadPreviousItem();

                var item = _itemContainer.AllItemViewModels[_currentIndex];
                UserBehaviorManager.Instance.Log(UserAction.ItemEnter, item.FeedId.ToString());
            }

            //if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            //{
            //    if (e.HorizontalVelocity < 0)
            //        LoadNextItem();
            //    else
            //        LoadPreviousItem();

            //    var item = _itemContainer.AllItemViewModels[_currentIndex];
            //    UserBehaviorManager.Instance.Log(UserAction.ItemClick, item.FeedId.ToString());
            //}
        }

        private void LoadNextItem()
        {
            if (_currentIndex == _itemContainer.AllItemViewModels.Count - 1)
                _currentIndex = 0;
            else 
                _currentIndex++;

            Binding();
        }

        private void LoadPreviousItem()
        {
            if (_currentIndex == 0 && _itemContainer.AllItemViewModels.Count > 0)
                _currentIndex = _itemContainer.AllItemViewModels.Count - 1;
            else
                _currentIndex--;

            Binding();
        }

        #endregion

        private void BindItemListToFlick(Guid feedId, Guid categoryId)
        {
            switch (_previousPage)
            { 
                case PreviousPage.FeedPage:
                    var feedResult = _feedManager.GetSubscribedFeed(feedId);
                    if (feedResult.HasError)
                    {
                        Messenger.ShowToast("không tìm thấy báo");
                        return;
                    }

                    _itemContainer = new FeedViewModel();
                    _itemContainer.UpdateFromDomainModel(feedResult.Target);
                    break;
                case PreviousPage.StoredItemsPage:
                    var storedItemsResult = _feedManager.GetStoredItems();
                    if (!storedItemsResult.HasError)
                    {
                        _itemContainer = new FeedViewModel();
                        _itemContainer.Name = "tin đã lưu";
                        storedItemsResult.Target.ForEach(i => _itemContainer.AllItemViewModels.Add(new ItemViewModel(i)));
                    }
                    break;
                case PreviousPage.CategoryPage:
                    var result = _feedManager.GetItemsByCategory(categoryId, AppConfig.MaxItemStored, false).Result;
                    if (result.Value != null || result.Value.Count > 0)
                    {
                        _itemContainer = new FeedViewModel();
                        _itemContainer.Id = categoryId;
                        _itemContainer.Name = _feedManager.GetCategory(categoryId).Name;
                        result.Value.ForEach(i => _itemContainer.AllItemViewModels.Add(new ItemViewModel(i)));
                    }
                    break;
            }
        }
    }

    enum PreviousPage
    { 
        FeedPage,
        StoredItemsPage,
        CategoryPage
    }
}