using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices.RssService;
using Davang.Utilities.Log;
using DocBao.ApplicationServices.Persistence;
using Microsoft.Phone.Shell;
using Davang.Utilities.Helpers;
using DocBao.WP.Helper;
using DocBao.ApplicationServices.UserBehavior;

namespace DocBao.ApplicationServices.Background
{
    public class BackgroundDownload
    {
        #region FeedManager

        /// <summary>
        /// Should create some smart logic here
        /// </summary>
        /// <param name="subscribedFeeds"></param>
        public static void CreateFeedsToDownload(IDictionary<Guid, Feed> subscribedFeeds)
        {
            var topScoredFeeds = UserBehaviorManager.Instance.ScoreFeeds(AppConfig.MAX_FEEDS_TO_DOWNLOAD_IN_BACKGROUND);
            var scoredFeedModels = new Dictionary<Feed, int>();
            topScoredFeeds.ForEach(sf =>
                {
                    if (subscribedFeeds.ContainsKey(sf.Key))
                        scoredFeedModels.Add(subscribedFeeds[sf.Key], sf.Value);
                });

            //This group contains feeds which are scored
            //They're prior than below group (UpdateTime=1 - odd number)
            var feedsToDownload = new List<FeedDownload>();
            scoredFeedModels.OrderByDescending(f => f.Value)
                .ThenBy(f=>f.Key.LastUpdatedTime)
                .ForEach(f =>
                    {
                        feedsToDownload.Add(new FeedDownload() 
                            {
                                Id = f.Key.Id,
                                PublisherId = f.Key.Publisher.Id,
                                LastUpdatedTime = f.Key.LastUpdatedTime,
                                Link = f.Key.Link,
                                UpdateTime = 1
                            });
                    });

            //There're not enough FeedDownload items so
            //we're adding more to it base on last update time
            //This group is lower prior than above group (UpdateTime=2 - even number)
            if (feedsToDownload.Count < AppConfig.MAX_FEEDS_TO_DOWNLOAD_IN_BACKGROUND)
            { 
                subscribedFeeds
                    .Where(f => feedsToDownload.FirstOrDefault(fd => fd.Id.Equals(f.Key)) == null)
                    .OrderBy(f => f.Value.LastUpdatedTime)
                    .Take(AppConfig.MAX_FEEDS_TO_DOWNLOAD_IN_BACKGROUND - feedsToDownload.Count)
                    .ForEach(f =>
                        {
                            feedsToDownload.Add(new FeedDownload()
                                {
                                    Id = f.Key,
                                    PublisherId = f.Value.Publisher.Id,
                                    LastUpdatedTime = f.Value.LastUpdatedTime,
                                    Link = f.Value.Link,
                                    UpdateTime = 2
                                });
                        });
            }

            AppConfig.FeedDownloads = feedsToDownload;
            feedsToDownload = null;
        }

        public static async Task<IDictionary<Guid, int>> LoadDownloadedFeedsAsync(IDictionary<Guid, Feed> subscribedFeeds, IPersistentManager dbContext)
        {
            if (subscribedFeeds == null) return null;

            var downloadedFiles = StorageHelper.GetLocalFilesStartWith(AppConfig.TEMP_DOWNLOAD_FILE_PATTERN);
            IDictionary<Guid, int> updatedFeeds = null;
            if (downloadedFiles != null && downloadedFiles.Count() > 0)
            {
                updatedFeeds = new Dictionary<Guid, int>();

                foreach (var fileName in downloadedFiles)
                {
                    var updated = await UpdateDownloadedFeedsAsync(subscribedFeeds, fileName, dbContext);
                    if (updated != null && updated.Count > 0)
                    {
                        updated.ForEach(u =>
                            {
                                if (updatedFeeds.ContainsKey(u.Key))
                                    updatedFeeds[u.Key] += u.Value;
                                else
                                    updatedFeeds.Add(u.Key, u.Value);
                            });
                    }

                    StorageHelper.DeleteFile(fileName);
                }
            }

            AppConfig.FeedDownloads = null;
            return updatedFeeds;
        }

        #endregion

        #region BackgroundAgent

        public static IList<Feed> DownloadFeeds()
        {
            var feedDownloads = AppConfig.FeedDownloads;
            if (feedDownloads == null || feedDownloads.Count == 0) return null;

            var feedsToDownload = feedDownloads.OrderBy(fd => fd.UpdateTime).Take(AppConfig.FeedCountPerBackgroundUpdate).ToList();
            if (feedsToDownload == null || feedsToDownload.Count == 0) return null;

            var rssParserService = RssParserService.Instance;
            var feeds = new List<Feed>();

            feedsToDownload.ForEach(fd =>
                {
                    IList<Item> items = null;
                    try
                    {
                        items = rssParserService.BackgroundUpdateItemsAsync(fd.Link, fd.PublisherId).Result;
                    }
                    catch (Exception ex)
                    {
                        GA.LogException(ex);
                    }

                    if (items != null && items.Count > 0)
                    {
                        feeds.Add(new Feed() 
                            { 
                                Id = fd.Id,
                                LastUpdatedTime = DateTime.Now,
                                Items = items
                            });
                    }

                    fd.LastUpdatedTime = DateTime.Now;
                    //Group which is prior (UpdateTime is odd) should take small step forward (2)
                    //Group which is lower (UpdateTime is even) should take longer step forward (4)
                    //But we need to maintain their state (odd/even)
                    fd.UpdateTime += fd.UpdateTime % 2 == 0 ? 4 : 2;
                });

            AppConfig.FeedDownloads = feedDownloads;
            return feeds;
        }

        public static void PostDownload(IList<Feed> downloadedFeeds)
        {
            if (downloadedFeeds != null && downloadedFeeds.Count > 0)
            {
                var dbContext = new PersistentManager();
                var downloadedFileName = string.Format("{0}-{1}.dat", AppConfig.TEMP_DOWNLOAD_FILE_PATTERN, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-tt"));

                if (dbContext.UpdateSerializedCopy(downloadedFeeds, downloadedFileName, false))
                {
                    if (AppConfig.ShowBackgroundUpdateResult)
                    {
                        ShellToast toast = new ShellToast();
                        toast.Title = "duyệt báo";
                        toast.Content = string.Format("tải {0} tin từ {1} mục", downloadedFeeds.Sum(f => f.Items.Count).ToString(), downloadedFeeds.Count);
                        toast.Show();
                    }

                    FlipTileData flipTileData = new FlipTileData()
                    {
                        Count = downloadedFeeds.Sum(f => f.Items.Count),
                        BackContent = string.Format("tải {0} tin", downloadedFeeds.Sum(f => f.Items.Count)).ToString(),
                        BackTitle = string.Format("cập nhật {0} mục", downloadedFeeds.Count),
                        BackBackgroundImage = new Uri("Resources/tile-med-back.png", UriKind.Relative)
                    };
                    ShellTile appTile = ShellTile.ActiveTiles.First();
                    if (appTile != null)
                        appTile.Update(flipTileData);
                }
            }
        }

        public static void CleanOldFiles()
        {
            var downloadedFiles = StorageHelper.GetLocalFilesStartWith(AppConfig.TEMP_DOWNLOAD_FILE_PATTERN);
            if (downloadedFiles.Count() < AppConfig.MAX_FILE_DOWNLOAD_ALLOW)
                return;

            Array.Sort(downloadedFiles, StringComparer.InvariantCulture);
            downloadedFiles.Take(downloadedFiles.Length - AppConfig.MAX_FILE_DOWNLOAD_ALLOW).ForEach(f =>
                {
                    StorageHelper.DeleteFile(f);
                });
        }

        #endregion

        #region Private

        private static async Task<IDictionary<Guid, int>> UpdateDownloadedFeedsAsync(IDictionary<Guid, Feed> subscribedFeeds, string fileName, IPersistentManager dbContext)
        {
            var feedsUpdated = await dbContext.ReadSerializedCopyAsync<List<Feed>>(fileName);
            if (feedsUpdated == null || feedsUpdated.Count == 0) return null;
            var updatedFeeds = new Dictionary<Guid, int>();
            
            feedsUpdated.ForEach(f =>
            {
                if (!subscribedFeeds.ContainsKey(f.Id)) return;
                var persistentFeed = subscribedFeeds[f.Id];

                var updatedItemCount = FeedHelper.UpdateFeedItems(persistentFeed, f.Items);
                if (updatedItemCount > 0)
                {
                    updatedFeeds.Add(persistentFeed.Id, updatedItemCount);
                }
                persistentFeed.LastUpdatedTime = f.LastUpdatedTime;
            });

            return updatedFeeds;
        }

        private static void DeleteDownloadedFiles()
        {
            var files = StorageHelper.GetLocalFilesStartWith("download");
            if (files == null || files.Count() == 0) return;

            files.ForEach(f =>
                {
                    StorageHelper.DeleteFile(f);
                });
        }

        #endregion
    }
}
