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
using DocBao.WP.ViewModels;
using DocBao.WP.Helper;
using DocBao.ApplicationServices;
using DocBao.ApplicationServices.Bank;
using Davang.Utilities.Helpers;

namespace DocBao.WP
{
    public partial class CustomViewPage : DBBasePage
    {
        public CustomViewPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();

            this.SetProgressIndicator(true, "đang mở...");
            Binding();
            HubTileService.UnfreezeGroup("Publishers");
            this.SetProgressIndicator(false);

            this.SetMainPage();

            base.OnNavigatedTo(e);
        }

        public void Binding()
        {
            var customCategories = new List<CustomHubTileItem>();
            
            _feedManager.GetCategories().ForEach(c => customCategories.Add(new CustomHubTileItem().ConvertFromCustomCategory(c)));
            this.tileList.ItemsSource = customCategories;
        }

        private void HubTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var tileItem = sender as HubTile;
            if (tileItem == null) return;

            var category = CategoryBank.Categories.FirstOrDefault(c => c.Name.Equals(tileItem.Title, StringComparison.InvariantCultureIgnoreCase));
            if (category == null) return;

            var uri = string.Format("/CategoryPage.xaml?categoryId={0}", category.Id.ToString());
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var uri = new Uri("/PublisherPickupPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            var uri = new Uri("/AboutPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            var uri = new Uri("/ConfigPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void savedItemButton_Click(object sender, EventArgs e)
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
            {
                Messenger.ShowToast("vui lòng mua bản trả tiền để có thể lưu lại tin...");
                return;
            }

            var storedItemsResult = _feedManager.GetStoredItems();
            if (storedItemsResult.HasError || storedItemsResult.Target.Count == 0)
            {
                Messenger.ShowToast("chưa có tin nào được lưu...");
                return;
            }

            var uri = new Uri("/StoredItemsPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void mnuSwitchView_Click(object sender, EventArgs e)
        {
            AppConfig.UseCustomView = false;

            var uri = new Uri("/HubTilePage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}