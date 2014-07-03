using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Davang.Utilities.Helpers;
using DocBao.ApplicationServices;
using DocBao.WP.Helper;

namespace DocBao.WP
{
    public partial class AboutPage : DBBasePage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MyOnNavigatedTo();
            Binding();
            base.OnNavigatedTo(e);
        }

        private void btnRating_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask oRateTask = new MarketplaceReviewTask();
            oRateTask.Show();
        }

        private void btnPro_Click(object sender, RoutedEventArgs e)
        {
            LicenseHelper.PurchaseProduct(AppConfig.PAID_VERSION);
            Binding();
        }

        private void Binding()
        {
            txtPageName.Text = "giới thiệu";
            firstNextIcon.Visibility = System.Windows.Visibility.Visible;
            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


            var purchased = LicenseHelper.Purchased(AppConfig.PAID_VERSION);
            if (purchased)
            {
                btnPro.Visibility = System.Windows.Visibility.Collapsed;
                abtVersion.Text = "phiên bản trả tiền " + assemblyVersion;
            }
            else
            {
                btnPro.Visibility = System.Windows.Visibility.Visible;
                abtVersion.Text = "phiên bản " + assemblyVersion;
            }
        }

        private void txtAppName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.BackToPreviousPage();
        }
    }
}