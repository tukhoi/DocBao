using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Davang.Utilities.Extensions;
using System.Windows.Controls;

namespace DocBao.WP.Helper
{
    public static class BasePageExtensions
    {
        //public static void SetBackground(this BasePage page)
        //{
        //    SetBackground(page, "LayoutRoot");
        //}

        //public static void SetBackground(this BasePage page, string controlName = "")
        //{
        //    var grid = page.FindName(controlName) as Grid;

        //    if (grid != null)
        //    {
        //        var background = new ImageBrush();
        //        background.ImageSource = new BitmapImage(new Uri("/Images/background.png", UriKind.Relative));
        //        grid.Background = background;
        //    }
        //}

        //public static void LogPage(this BasePage page)
        //{
        //    GoogleAnalytics.EasyTracker.GetTracker().SendView(page.ToString());
        //}

        //public static void LogAdsClicked(this BasePage page)
        //{
        //    GoogleAnalytics.EasyTracker.GetTracker().SendEvent(page.ToString(), "Ads clicked", null, 0);
        //}
    }
}