#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Davang.Parser.Dto;
using DocBao.ApplicationServices;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Linq;
using DocBao.ApplicationServices.Persistence;
using DocBao.ApplicationServices.RssService;
using System.Text;
using GoogleAnalytics.Core;
using Davang.Utilities.Log;
using Davang.Utilities;
using System.IO.IsolatedStorage;
using Davang.Utilities.Helpers;
using DocBao.ApplicationServices.Background;
using System.Windows.Threading;

namespace DocBao.BackgroundUpdater
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            Action intialization = new Action(() 
                => GA.Initialize(AppConfig.ClientId.ToString(), AppConfig.GA_ID, AppConfig.GA_APP_NAME, AppConfig.GA_APP_VERSION));

            Task.Factory.StartNew(() =>
                Deployment.Current.Dispatcher.BeginInvoke(intialization));

            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            GA.LogException(e.ExceptionObject, true);

            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// Not over 25 seconds and comsumes 6MB RAM
        /// </remarks>
        /// 
        protected override void OnInvoke(ScheduledTask task)
        {
            //TODO: Add code to perform your task in background
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable()
                        || !AppConfig.AllowBackgroundUpdate
                        //|| AppConfig.AppRunning
                        || (AppConfig.JustUpdateOverWifi && (!AppConfig.JustUpdateOverWifi || NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)))
                {
                    var reason = string.Format("Exit - User allows: {0} - AppRuning: {1} - WifiOnly: {2} - CurrentNework: {3}",
                        AppConfig.AllowBackgroundUpdate,
                        //AppConfig.AppRunning,
                        AppConfig.JustUpdateOverWifi,
                        NetworkInterface.NetworkInterfaceType.ToString());
                    GA.LogBackgroundAgent(reason, 0);
                    return;
                }
            
                var feedsDownloaded = BackgroundDownload.DownloadFeeds();
                if (feedsDownloaded != null && feedsDownloaded.Count > 0)
                {
                    BackgroundDownload.CleanOldFiles();
                    BackgroundDownload.PostDownload(feedsDownloaded);
                    GA.LogBackgroundAgent("Downloaded completed", feedsDownloaded.Sum(f => f.Items.Count));
                }
            }
            catch (OutOfMemoryException)
            {
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
            }
            finally
            {
                NotifyComplete();
            }
        }
    }
}