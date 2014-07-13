using Davang.Utilities.Log;
using Davang.WP.Utilities;
using DocBao.ApplicationServices;
using DocBao.ApplicationServices.Background;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DocBao.WP.Helper
{
    public class DBBasePage : BasePage
    {
        protected FeedManager _feedManager;
        

        public DBBasePage()
        {
            _feedManager = FeedManager.GetInstance();
        }

        protected async virtual Task MyOnNavigatedTo()
        {
            //await EnsureMemory();
            this.SetProgressIndicator(message: "đang mở...");
            await _feedManager.LoadAsync();

            //this is set to null once load feeds downloaded process finished
            if (AppConfig.FeedDownloads == null) return;
            else await LoadDownloadedFeeds();
        }

        private async Task LoadDownloadedFeeds()
        {
            this.SetProgressIndicator(message: "đang cập nhật tin đã tải ngầm...");
            var updatedFeeds = await _feedManager.LoadDownloadedFeeds();
            this.SetProgressIndicator(false);

            if (updatedFeeds != null && updatedFeeds.Count > 0)
            {
                var message = FeedHelper.BuildUpdateStatus(updatedFeeds);
                var count = updatedFeeds.Count > AppConfig.MAX_NEW_FEED_UPDATED_SHOW
                    ? AppConfig.MAX_NEW_FEED_UPDATED_SHOW
                    : updatedFeeds.Count;

                Messenger.ShowToast(message, miliSecondsUntilHidden: count * 2000);

                StandardTileData tile = new StandardTileData()
                {
                    Count = 0,
                    BackBackgroundImage = new Uri("IDontExist", UriKind.Relative),
                    BackContent = string.Empty,
                    BackTitle = string.Empty
                };
                ShellTile appTile = ShellTile.ActiveTiles.First();
                if (appTile != null)
                    appTile.Update(tile);
            }
        }

        private async Task EnsureMemory()
        { 
            var currentUsage = DeviceStatus.ApplicationCurrentMemoryUsage;
            var limit = DeviceStatus.ApplicationMemoryUsageLimit;
            var availableInMB = (limit - currentUsage) / 1000000;

            if (availableInMB > 30) return;
            this.SetProgressIndicator(message: "xóa bớt bộ nhớ...");
            await Task.Run(() => GC.Collect());
            await Task.Run(() => GC.WaitForPendingFinalizers());
            this.SetProgressIndicator(false);
        }
    }
}
