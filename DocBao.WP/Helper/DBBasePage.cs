using Davang.Utilities.Log;
using Davang.WP.Utilities;
using DocBao.ApplicationServices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
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
        Popup popUpNewVersion = new Popup();

        public DBBasePage()
        {
            _feedManager = FeedManager.GetInstance();
        }

        protected async Task MyOnNavigatedTo()
        {
            try
            {
                if (IsMainPage()
                    && (AppConfig.AppUpdate == UpdateVersion.NotSet || AppConfig.AppUpdate == UpdateVersion.V1_4)
                    && !popUpNewVersion.IsOpen)
                {
                    this.IsEnabled = false;
                    ApplicationBar.IsVisible = false;
                    PopUpNewVersion();
                    popUpNewVersion.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
                ClearPopUpError();
            }

            var updatedFeeds = await _feedManager.LoadAsync();

            if (updatedFeeds != null && updatedFeeds.Count > 0)
            {
                var message = FeedHelper.BuildUpdateStatus(updatedFeeds);
                var count = updatedFeeds.Count > AppConfig.MAX_NEW_FEED_UPDATED_SHOW
                    ? AppConfig.MAX_NEW_FEED_UPDATED_SHOW
                    : updatedFeeds.Count;
                
                Messenger.ShowToast(message, miliSecondsUntilHidden:count * 2000);
            }
        }
        
        private void PopUpNewVersion()
        {
            try
            {
                Border border = new Border();
                border.BorderBrush = new SolidColorBrush(Colors.Green);
                border.BorderThickness = new Thickness(2);
                border.Margin = new Thickness(10, 10, 10, 10);

                StackPanel skt_pnl_outter = new StackPanel();
                var background = new ImageBrush();
                skt_pnl_outter.Background = new SolidColorBrush(Colors.LightGray);
                skt_pnl_outter.Orientation = System.Windows.Controls.Orientation.Vertical;

                Image img_disclaimer = new Image();
                img_disclaimer.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                img_disclaimer.Stretch = Stretch.Fill;
                img_disclaimer.Margin = new Thickness(0, 15, 0, 5);
                Uri uriR = new Uri("/Resources/new-icon.png", UriKind.Relative);
                BitmapImage imgSourceR = new BitmapImage(uriR);
                img_disclaimer.Source = imgSourceR;
                skt_pnl_outter.Children.Add(img_disclaimer);

                TextBlock txt_blk1 = new TextBlock();
                txt_blk1.Text = "Phiên bản 1.5";
                txt_blk1.TextAlignment = TextAlignment.Center;
                txt_blk1.FontSize = 40;
                txt_blk1.Margin = new Thickness(10, 0, 10, 0);
                txt_blk1.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk2 = new TextBlock();
                txt_blk2.Text = "4 báo mới!";
                txt_blk2.TextAlignment = TextAlignment.Left;
                txt_blk2.TextWrapping = TextWrapping.Wrap;
                txt_blk2.FontSize = 21;
                txt_blk2.Margin = new Thickness(10, 0, 10, 0);
                txt_blk2.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk2bis = new TextBlock();
                txt_blk2bis.Text = "Yan TV, Zing News, LinkHay, WebTreTho";
                txt_blk2bis.TextAlignment = TextAlignment.Left;
                txt_blk2bis.FontSize = 14;
                txt_blk2bis.Margin = new Thickness(10, 0, 10, 0);
                txt_blk2bis.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk3 = new TextBlock();
                txt_blk3.Text = "(*) chọn để xem trong phần chọn báo";
                txt_blk3.TextAlignment = TextAlignment.Left;
                txt_blk3.FontSize = 14;
                txt_blk3.Margin = new Thickness(10, 0, 10, 0);
                txt_blk3.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk4 = new TextBlock();
                txt_blk4.Text = "và pin được tối ưu hơn bao giờ hết!";
                txt_blk4.TextAlignment = TextAlignment.Left;
                txt_blk4.FontSize = 21;
                txt_blk4.Margin = new Thickness(10, 0, 10, 0);
                txt_blk4.Foreground = new SolidColorBrush(Colors.White);

                skt_pnl_outter.Children.Add(txt_blk1);
                skt_pnl_outter.Children.Add(txt_blk2);
                skt_pnl_outter.Children.Add(txt_blk2bis);
                skt_pnl_outter.Children.Add(txt_blk3);
                skt_pnl_outter.Children.Add(txt_blk4);

                StackPanel skt_pnl_inner = new StackPanel();
                skt_pnl_inner.Orientation = System.Windows.Controls.Orientation.Horizontal;

                Button btn_cancel = new Button();
                btn_cancel.Content = "đóng";
                btn_cancel.Width = 215;
                btn_cancel.Click += new RoutedEventHandler(btnClosePopUp_Click);

                skt_pnl_inner.Children.Add(btn_cancel);
                skt_pnl_outter.Children.Add(skt_pnl_inner);

                border.Child = skt_pnl_outter;
                popUpNewVersion.Child = border;

                popUpNewVersion.Width = 400;
                popUpNewVersion.VerticalOffset = 24;
                popUpNewVersion.HorizontalOffset = 12;

                popUpNewVersion.IsOpen = true;
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
                ClearPopUpError();
            }
        }

        private void btnClosePopUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (popUpNewVersion.IsOpen)
                {
                    popUpNewVersion.IsOpen = false;
                    this.IsEnabled = true;
                    ApplicationBar.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
                ClearPopUpError();
            }
        }

        private void ClearPopUpError()
        {
            if (!this.IsEnabled) this.IsEnabled = true;
            if (!ApplicationBar.IsVisible) ApplicationBar.IsVisible = true;
            if (popUpNewVersion.IsOpen) popUpNewVersion.IsOpen = false;
        }
    }
}
