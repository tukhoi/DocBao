using AppPromo;
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
    public abstract class DBBasePage : BasePage
    {
        protected FeedManager _feedManager;

        public DBBasePage()
        {
            _feedManager = FeedManager.Instance;            
        }

        protected async virtual Task MyOnNavigatedTo()
        {
            //await EnsureMemory();
            this.SetProgressIndicator(message: "đang mở...");
            await _feedManager.LoadAsync();

            this.SetProgressIndicator(false);
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
