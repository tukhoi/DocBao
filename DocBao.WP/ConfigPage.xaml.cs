using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DocBao.ApplicationServices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Davang.Utilities.Extensions;
using System.Windows.Media;
using DocBao.WP.Helper;
using Davang.Utilities.Log;

namespace DocBao.WP
{
    public partial class ConfigPage : BasePage
    {
        public ConfigPage()
        {
            InitializeComponent();
            BindList();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();

            txtPageName.Text = "tùy chọn";
            firstNextIcon.Visibility = System.Windows.Visibility.Visible;

            this.SetProgressIndicator(true, "đang mở tùy chọn...");

            await Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(() => LoadConfig()));

            lpkFeedCountPerBackgroundUpdate.SelectionChanged += lpkFeedCountPerBackgroundUpdate_SelectionChanged;
            lpkMaxItemStored.SelectionChanged += lpkMaxItemStored_SelectionChanged;

            this.SetProgressIndicator(false);

            base.OnNavigatedTo(e);
        }

        private void BindList()
        {
            lpkMaxItemStored.Items.Clear();
            lpkFeedCountPerBackgroundUpdate.Items.Clear();

            AppConfig.MaxItemStoredList.ForEach(i => lpkMaxItemStored.Items.Add(i.Key));
            AppConfig.FeedCountPerBackgroundUpdateList.ForEach(i => lpkFeedCountPerBackgroundUpdate.Items.Add(i.Key));
        }

        private void LoadConfig()
        {
            try
            {
                chkShowTitleOnly.IsChecked = AppConfig.ShowTitleOnly;
                chkShowItemTitle.IsChecked = AppConfig.ShowItemTitle;
                chkShowUnreadItemOnly.IsChecked = AppConfig.ShowUnreadItemOnly;
                chkAllowBackgroundUpdate.IsChecked = AppConfig.AllowBackgroundUpdate;
                lpkMaxItemStored.SelectedItem = AppConfig.MaxItemStoredList.FirstOrDefault(kv => kv.Value.Equals(AppConfig.MaxItemStored)).Key;
                lpkFeedCountPerBackgroundUpdate.SelectedItem = AppConfig.FeedCountPerBackgroundUpdateList.FirstOrDefault(kv => kv.Value == AppConfig.FeedCountPerBackgroundUpdate).Key;
                chkShowBackgroundUpdateResult.IsChecked = AppConfig.ShowBackgroundUpdateResult;
                chkJustUpdateOverWifi.IsChecked = AppConfig.JustUpdateOverWifi;

                SetBackgroundUpdateStuff(AppConfig.AllowBackgroundUpdate);
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
            }
        }

        private void chkShowTitleOnly_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.ShowTitleOnly = chkShowTitleOnly.IsChecked.HasValue ?
                chkShowTitleOnly.IsChecked.Value : true;
        }

        private void chkShowItemTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.ShowItemTitle = chkShowItemTitle.IsChecked.HasValue ?
                chkShowItemTitle.IsChecked.Value : true;
        }

        private void chkShowUnreadItemOnly_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.ShowUnreadItemOnly = chkShowUnreadItemOnly.IsChecked.HasValue ?
                chkShowUnreadItemOnly.IsChecked.Value : true;
        }

        private void chkAllowBackgroundUpdate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.AllowBackgroundUpdate = chkAllowBackgroundUpdate.IsChecked.HasValue ?
                chkAllowBackgroundUpdate.IsChecked.Value : true;

            SetBackgroundUpdateStuff(AppConfig.AllowBackgroundUpdate);
        }

        private void SetBackgroundUpdateStuff(bool enabled)
        {
            lpkFeedCountPerBackgroundUpdate.IsEnabled = enabled;
            chkShowBackgroundUpdateResult.IsEnabled = enabled;
            chkJustUpdateOverWifi.IsEnabled = enabled;
        }

        private void chkShowBackgroundUpdateResult_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.ShowBackgroundUpdateResult = chkShowBackgroundUpdateResult.IsChecked.HasValue ?
                chkShowBackgroundUpdateResult.IsChecked.Value : true;
        }

        private void chkJustUpdateOverWifi_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.JustUpdateOverWifi = chkJustUpdateOverWifi.IsChecked.HasValue ?
                chkJustUpdateOverWifi.IsChecked.Value : true;
        }

        private void lpkFeedCountPerBackgroundUpdate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null || e.AddedItems == null || e.AddedItems.Count == 0)
                return;
            
            var key = e.AddedItems[0].ToString();
            var value = AppConfig.FeedCountPerBackgroundUpdateList[key];
            AppConfig.FeedCountPerBackgroundUpdate = value;
        }

        private void lpkMaxItemStored_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null || e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            var key = e.AddedItems[0].ToString();
            var value = AppConfig.MaxItemStoredList[key];
            AppConfig.MaxItemStored = value;
        }

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }
    }
}