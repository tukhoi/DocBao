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
using RateMyApp.Controls;

namespace DocBao.WP.Helper
{
    public class DBMainPage : DBBasePage
    {
        Popup popUpNewVersion = new Popup();

        protected override async Task MyOnNavigatedTo()
        {
            if ((AppConfig.AppUpdate == UpdateVersion.NotSet
                || AppConfig.AppUpdate == UpdateVersion.V1_4
                || AppConfig.AppUpdate == UpdateVersion.V1_5)
                && !popUpNewVersion.IsOpen)
            {
                this.IsEnabled = false;
                ApplicationBar.IsVisible = false;
                PopUpNewVersion();
                popUpNewVersion.IsOpen = true;
            }
            
            CreateFeedbackOverlayControl();

            await base.MyOnNavigatedTo();
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
        }

        private void CreateFeedbackOverlayControl()
        {
            FeedbackOverlay fbOverlay = new FeedbackOverlay();
            FeedbackOverlay.SetApplicationName(fbOverlay, "duyệtbáo");
            FeedbackOverlay.SetEnableAnimation(fbOverlay, true);
            FeedbackOverlay.SetRatingTitle(fbOverlay, "Thấy hay?");
            FeedbackOverlay.SetRatingMessage1(fbOverlay, "bạn đã xài app duyệtbáo được một số lần! nếu thấy hay hãy cho 5 saoooooo nhé!");
            FeedbackOverlay.SetRatingMessage2(fbOverlay, "hay là thử cho ít sao hơn cũng được...");
            FeedbackOverlay.SetRatingYes(fbOverlay, "đồng ý");
            FeedbackOverlay.SetRatingNo(fbOverlay, "không, cám ơn");
            FeedbackOverlay.SetFeedbackTitle(fbOverlay, "Phản hồi");
            FeedbackOverlay.SetFeedbackMessage1(fbOverlay, "Huhu buồn quá bạn không muốn đánh giá duyệtbáo... Hay là bạn cho biết những gì cần cải thiện?");
            FeedbackOverlay.SetFeedbackYes(fbOverlay, "cũng được");
            FeedbackOverlay.SetFeedbackNo(fbOverlay, "không, cám ơn");
            FeedbackOverlay.SetFeedbackTo(fbOverlay, "davangsolutions@outlook.com");
            FeedbackOverlay.SetFeedbackSubject(fbOverlay, "phản hồi app duyệtbáo, clienId: " + AppConfig.ClientId);
            FeedbackOverlay.SetFeedbackBody(fbOverlay, "");
            FeedbackOverlay.SetCompanyName(fbOverlay, "Davang Solutions");
            FeedbackOverlay.SetFirstCount(fbOverlay, 10);
            FeedbackOverlay.SetSecondCount(fbOverlay, 15);
            FeedbackOverlay.SetCountDays(fbOverlay, false);

            fbOverlay.VisibilityChanged += (sender, e) =>
            {
                ApplicationBar.IsVisible = (fbOverlay.Visibility != Visibility.Visible);
            };

            var grid = this.FindName(BasePage.LayoutRoot) as Grid;
            if (grid != null)
                grid.Children.Add(fbOverlay);
        }
    }
}
