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
using Davang.Utilities.Log;
using Davang.WP.Utilities;
using DocBao.ApplicationServices;
using Microsoft.Phone.Shell;

namespace DocBao.WP.Helper
{
    public abstract class DBMainPage : DBBasePage
    {
        Popup popUpNewVersion;

        protected override async Task MyOnNavigatedTo()
        {
            await base.MyOnNavigatedTo();

            if ((AppConfig.AppUpdate == UpdateVersion.NotSet
                || AppConfig.AppUpdate == UpdateVersion.V1_4
                || AppConfig.AppUpdate == UpdateVersion.V1_5)
                && !popUpNewVersion.IsOpen)
            {
                this.IsEnabled = false;
                ApplicationBar.IsVisible = false;
                ShowPopUp();
                popUpNewVersion.IsOpen = true;
            }

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
                int count = updatedFeeds.OrderByDescending(f => f.Value).Take(AppConfig.MAX_NEW_FEED_UPDATED_SHOW).Count();
                Messenger.ShowToast(message, miliSecondsUntilHidden: count * 1500);

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

        private void ShowPopUp()
        {
            try
            {
                popUpNewVersion = new Popup();

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
                txt_blk1.Text = "Phiên bản 1.6";
                txt_blk1.TextAlignment = TextAlignment.Center;
                txt_blk1.FontSize = 40;
                txt_blk1.Margin = new Thickness(10, 0, 10, 0);
                txt_blk1.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk2 = new TextBlock();
                txt_blk2.Text = "5 báo mới!";
                txt_blk2.TextAlignment = TextAlignment.Left;
                txt_blk2.TextWrapping = TextWrapping.Wrap;
                txt_blk2.FontSize = 21;
                txt_blk2.Margin = new Thickness(10, 0, 10, 0);
                txt_blk2.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk2bis = new TextBlock();
                txt_blk2bis.Text = "VOV, Công an Nhân dân, Quân đội Nhân dân, Tiền phong, VTC";
                txt_blk2bis.TextAlignment = TextAlignment.Left;
                txt_blk2bis.FontSize = 14;
                txt_blk2bis.Margin = new Thickness(10, 0, 10, 0);
                txt_blk2bis.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk3 = new TextBlock();
                txt_blk3.Text = "(*) chọn để xem trong phần chọn báo";
                txt_blk3.TextAlignment = TextAlignment.Left;
                txt_blk3.FontSize = 16;
                txt_blk3.Margin = new Thickness(10, 0, 10, 0);
                txt_blk3.Foreground = new SolidColorBrush(Colors.White);

                TextBlock txt_blk4 = new TextBlock();
                txt_blk4.Text = "và nhiều lỗi được sửa!";
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

        private void ClearPopUpError()
        {
            if (!this.IsEnabled) this.IsEnabled = true;
            if (!ApplicationBar.IsVisible) ApplicationBar.IsVisible = true;
            if (popUpNewVersion.IsOpen) popUpNewVersion.IsOpen = false;
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
            finally
            {
                popUpNewVersion = null;
            }
        }
    }
}
