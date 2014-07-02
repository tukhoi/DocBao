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

namespace DocBao.WP
{
    public partial class ItemPage : BasePage
    {
        Feed _currentFeed;
        int _currentIndex=-1;
        private bool _webLoaded = false;
        //private bool _comeFromFeedPage = false;
        private PreviousPage _previousPage = PreviousPage.FeedPage;

        public ItemPage()
        {
            InitializeComponent();

            adControl.StartAds();

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
            if (previousPage.ToString().Contains("StoredItemsPage.xaml"))
                _previousPage = PreviousPage.StoredItemsPage;
            if (previousPage.ToString().Contains("CategoryPage.xaml"))
                _previousPage = PreviousPage.CategoryPage;

            BindItemListToFlick(feedId, categoryId);

            //if (_comeFromFeedPage)
            //{
            //    var feedResult = _feedManager.GetSubscribedFeed(feedId);
            //    if (feedResult.HasError)
            //    {
            //        Messenger.ShowToast("không tìm thấy báo");
            //        return;
            //    }

            //    _currentFeed = feedResult.Target;
            //}
            //else
            //{
            //    var storedItemsResult = _feedManager.GetStoredItems();
            //    if (!storedItemsResult.HasError)
            //    {
            //        _currentFeed = new Feed();
            //        _currentFeed.Items = storedItemsResult.Target;
            //    }
            //}

            _currentIndex = _currentIndex == -1 ? _currentFeed.Items.IndexOf(_currentFeed.Items.FirstOrDefault(i => i.Id.Equals(itemId))) : _currentIndex;
            if (_currentIndex == -1 && _currentFeed.Items.Count > 0)
                _currentIndex = 0;

            Binding();
            CreateAppBar();

            base.OnNavigatedTo(e);
        }

        void wbsContent_Navigating(object sender, NavigatingEventArgs e)
        {
            this.SetProgressIndicator(true, "đang tải...");
        }

        void wbsContent_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
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

            var item = _currentFeed.Items[_currentIndex];
            if (item != null)
            {
                BindingNavigationBar(item);
                if (_previousPage == PreviousPage.FeedPage || _previousPage == PreviousPage.CategoryPage)
                    _feedManager.MarkItemAsRead(item.FeedId, item.Id, true);
                else
                    _feedManager.MarkStoredItemAsRead(item.Id, true);
                CreateAppBar();
                _feedManager.SetLastId<string>(item.Id);
                wbsContent.Navigate(new Uri(item.Link, UriKind.Absolute));
            }
        }

        private void ieButton_Click(object sender, EventArgs e)
        {
            var item = _currentFeed.Items[_currentIndex];
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
            var item = _currentFeed.Items[_currentIndex];

            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Gởi từ app duyệt báo: " + item.Title;
            emailComposeTask.Body = item.Link;
            
            emailComposeTask.Show();

        }

        private void copyLinkButton_Click(object sender, EventArgs e)
        {
            var item = _currentFeed.Items[_currentIndex];

            if (!string.IsNullOrEmpty(item.Link))
            {
                Clipboard.SetText(item.Link);
                Messenger.ShowToast("đã chép link");
            }
            else
                Messenger.ShowToast("không tìm thấy link");
        }

        private void facebookButton_Click(object sender, EventArgs e)
        {
            var item = _currentFeed.Items[_currentIndex];

            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = "gởi từ app duyệt báo: " + item.Title;
            shareLinkTask.LinkUri = new Uri(item.Link, UriKind.Absolute);
            shareLinkTask.Message = item.Link;
            shareLinkTask.Show();
        }

        private async void storeButton_Click(object sender, EventArgs e)
        {
            var item = _currentFeed.Items[_currentIndex];
            if (item == null) return;

            var stored = _feedManager.IsStored(item.Id);
            var message = stored ? "đang xóa..." : "đang lưu...";
            var doneMessage = stored ? "xóa xong..." : "lưu xong...";

            this.SetProgressIndicator(true, message);
            var result = stored ? await _feedManager.DeleteStoredItemAsync(item.Id) : await _feedManager.StoreItemAsync(item);
            if (result.HasError)
                Messenger.ShowToast(result.ErrorMessage());
            else
            {
                Messenger.ShowToast(doneMessage);
                CreateAppBar();
            }

            this.SetProgressIndicator(false);
        }

        private void showTitleMenuItem_Click(object sender, EventArgs e)
        {
            AppConfig.ShowItemTitle = !AppConfig.ShowItemTitle;
            txtItemTitle.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
            itemNextIcon.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
            CreateAppBar();
        }

        private void unreadButton_Click(object sender, EventArgs e)
        {
            var item = _currentFeed.Items[_currentIndex];

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

        private void OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.HorizontalVelocity < 0)
                    LoadNextItem();
                else
                    LoadPreviousItem();
            }
        }

        private void LoadNextItem()
        {
            if (_currentIndex == _currentFeed.Items.Count - 1)
                _currentIndex = 0;
            else 
                _currentIndex++;

            Binding();
        }

        private void LoadPreviousItem()
        {
            if (_currentIndex == 0 && _currentFeed.Items.Count > 0)
                _currentIndex = _currentFeed.Items.Count - 1;
            else
                _currentIndex--;

            Binding();
        }

        private void CreateAppBar()
        {
            var item = _currentFeed.Items[_currentIndex];

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

        //private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    this.BackToPreviousPage(2);
        //}

        //private void txtPublisherName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    this.BackToPreviousPage(1);
        //}

        //private void txtFeedName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    this.BackToPreviousPage();
        //}

        private void BindingNavigationBar(Item item)
        {
            if (item == null) return;
            txtItemTitle.Text = item.Title;

            if (_previousPage == PreviousPage.FeedPage)
                txtAppName.Tap += ((sender, e) => this.BackToPreviousPage(2));
            else
                txtAppName.Tap += ((sender, e) => this.BackToPreviousPage(1));

            switch (_previousPage)
            { 
                case PreviousPage.FeedPage:
                    txtPublisherName.Text = _currentFeed.Publisher.Name;
                    break;
                case PreviousPage.StoredItemsPage:
                    txtPublisherName.Text = _currentFeed.Name;
                    break;
                case PreviousPage.CategoryPage:
                    txtPublisherName.Text = _currentFeed.Name;
                    break;
            }

            if (_previousPage == PreviousPage.FeedPage && _currentFeed.Publisher.FeedIds.Count() > 1)
                txtPublisherName.Tap += ((sender, e) => this.BackToPreviousPage(1));
            else
                txtPublisherName.Tap += ((sender, e) => this.BackToPreviousPage(0));

            txtFeedName.Visibility = _previousPage == PreviousPage.FeedPage && _currentFeed.Publisher.FeedIds.Count() > 1 ? Visibility.Visible : Visibility.Collapsed;
            txtFeedName.Text = _previousPage == PreviousPage.FeedPage ? _currentFeed.Name : string.Empty;

            if (_previousPage == PreviousPage.FeedPage)
                txtFeedName.Tap += ((sender, e) => this.BackToPreviousPage());

            txtItemTitle.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
            itemNextIcon.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;

            firstNextIcon.Visibility = System.Windows.Visibility.Visible;
            secondNextIcon.Visibility = txtFeedName.Visibility;

            txtItemTitle.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
            itemNextIcon.Visibility = AppConfig.ShowItemTitle ? Visibility.Visible : Visibility.Collapsed;
        }

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

                    _currentFeed = feedResult.Target;
                    break;
                case PreviousPage.StoredItemsPage:
                    var storedItemsResult = _feedManager.GetStoredItems();
                    if (!storedItemsResult.HasError)
                    {
                        _currentFeed = new Feed();
                        _currentFeed.Name = "tin đã lưu";
                        _currentFeed.Items = storedItemsResult.Target;
                    }
                    break;
                case PreviousPage.CategoryPage:
                    var result = _feedManager.GetItemsByCategory(categoryId, AppConfig.MaxItemStored, false).Result;
                    if (result.Value != null || result.Value.Count > 0)
                    {
                        _currentFeed = new Feed();
                        _currentFeed.Id = feedId;
                        _currentFeed.Name = _feedManager.GetCategory(categoryId).Name;
                        _currentFeed.Items = result.Value;
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