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
using System.Threading.Tasks;
using DocBao.WP.Helper;
using DocBao.WP.ViewModels;
using Davang.Utilities.Extensions;
using Davang.Utilities.Helpers;
using DocBao.ApplicationServices.UserBehavior;
using System.Diagnostics;

namespace DocBao.WP
{
    public partial class HubTilePage : DBMainPage
    {
        public HubTilePage()
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            tileList.ItemsSource = null;

            base.OnNavigatedFrom(e);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            tileList.ItemsSource = null;
            tileList.ItemTemplate = null;
            base.OnRemovedFromJournal(e);
        }

        private void HubTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var tileItem = sender as HubTile;
            if (tileItem == null) return;

            var publishersResult = _feedManager.GetSubscribedPublishers();
            if (publishersResult.HasError)
            {
                Messenger.ShowToast(publishersResult.ErrorMessage());
                return;
            }

            var publisher = publishersResult.Target.FirstOrDefault(p => p.Name.Equals(tileItem.Title, StringComparison.InvariantCultureIgnoreCase));
            if (publisher == null) return;
            
            //var uri = publisher.FeedIds.Count() > 1
            //    ? string.Format("/PublisherPage.xaml?publisherId={0}", publisher.Id.ToString())
            //    : string.Format("/FeedPage.xaml?feedId={0}&publisherId={1}", publisher.FeedIds[0], publisher.Id);

            var uri = string.Format("/FeedPage.xaml?feedId={0}&publisherId={1}", publisher.FeedIds[0], publisher.Id);
            UserBehaviorManager.Instance.Log(UserAction.PubEnter, publisher.Id.ToString());
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void Binding()
        {
            var result = _feedManager.GetSubscribedPublishers();
            if (result.HasError)
            {
                Messenger.ShowToast(result.ErrorMessage());
                return;
            }

            var publishers = result.Target;
            var hubTiles = new List<HubTileItem>();
            publishers.ForEach(p => hubTiles.Add(new HubTileItem().ConvertFromPublisherModel(p)));

            this.tileList.ItemsSource = hubTiles;
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
            AppConfig.UseCustomView = true;

            var uri = new Uri("/CustomViewPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}