﻿using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Davang.Utilities.Extensions;

namespace DocBao.WP.Helper
{
    public static class PhoneApplicationPageExtensions
    {
        public static void SetProgressIndicator(this PhoneApplicationPage page, bool isVisible, string message = "")
        {
            if (SystemTray.ProgressIndicator == null)
                SystemTray.ProgressIndicator = new ProgressIndicator();

            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
            SystemTray.ProgressIndicator.Text = message;
        }

        public static void BackToMainPage(this PhoneApplicationPage page)
        {
            var mainPageUri = "/HubTilePage.xaml";
            page.NavigationService.Navigate(new Uri(mainPageUri, UriKind.Relative));
        }

        public static void BackToPreviousPage(this PhoneApplicationPage page, short skip = 0)
        {
<<<<<<< HEAD
            try
            {
                for (short i = 0; i < skip; i++)
                {
                    if (page.NavigationService.BackStack.Count() > 0)
                        page.NavigationService.RemoveBackEntry();
                }

                if (page.NavigationService.CanGoBack)
                    page.NavigationService.GoBack();
                else
                {
                    page.NavigationService.Navigate(new Uri("/HubTilePage.xaml", UriKind.Relative));
                }
            }
            catch (Exception)
            {
                int i = 1;
            }
=======
            for (short i=0; i<skip; i++)
                page.NavigationService.RemoveBackEntry();

            page.NavigationService.GoBack();
>>>>>>> parent of db4037a... Stored item feature
        }

        public static void SetBackground(this PhoneApplicationPage page, Uri backgroundImageUri)
        {
            var background = new ImageBrush();
            background.ImageSource = new BitmapImage(backgroundImageUri);
            page.Background = background;
        }
    }
}
