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

namespace DocBao.BackgroundUpdater
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
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
                    || AppConfig.AppRunning
                    || (AppConfig.JustUpdateOverWifi && (!AppConfig.JustUpdateOverWifi || NetworkInterface.NetworkInterfaceType!=NetworkInterfaceType.Wireless80211)))
                    return;

                IDictionary<string, int> updated = new Dictionary<string, int>();
                var dbContext = new PersistentManager();
                var rssService = RssParserService.GetInstance();
                var subscribedFeeds = dbContext.ReadSerializedCopy<IDictionary<Guid, Feed>>(AppConfig.SUBSCRIBED_FEED_FILE_NAME);
                var feedsToBeUpdated = subscribedFeeds.Values.OrderBy(f => f.LastUpdatedTime).Take(AppConfig.FeedCountPerBackgroundUpdate).ToList();

                if (feedsToBeUpdated != null && feedsToBeUpdated.Count > 0)
                {
                    feedsToBeUpdated.ForEach(f =>
                    {
                        var updatedCount = rssService.UpdateItemsAsync(f).Result;
                        f.LastUpdatedTime = DateTime.Now;
                        if (updatedCount > 0)
                            updated.Add(f.Name, updatedCount);
                    });
                }

                //FeedManager.SyncFeeds(subscribedFeeds);

                if (dbContext.UpdateSerializedCopy(subscribedFeeds, AppConfig.SUBSCRIBED_FEED_FILE_NAME, true)
                    && updated.Count > 0)
                {
                    if (AppConfig.ShowBackgroundUpdateResult)
                    {
                        ShellToast toast = new ShellToast();
                        toast.Title = "duyệt báo";
                        toast.Content = string.Format("{0} mục và {1} tin được cập nhật", updated.Count, updated.Values.Sum().ToString());
                        toast.Show();
                    }

                    FlipTileData flipTileData = new FlipTileData()
                    {
                        Count = updated.Values.Sum(),
                        BackContent = string.Format("{0} tin mới", updated.Values.Sum()),
                        BackTitle = string.Format("{0} mục cập nhật", updated.Count),
                        BackBackgroundImage = new Uri("Resources/tile-med-back.png", UriKind.Relative)
                    };

                    ShellTile appTile = ShellTile.ActiveTiles.First();
                    if (appTile != null)
                        appTile.Update(flipTileData);
                }
            }
            catch (Exception)
            {
            }

            NotifyComplete();
        }
    }
}