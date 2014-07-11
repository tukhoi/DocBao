using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using Davang.Utilities.Helpers;
using DocBao.ApplicationServices.RssService;
using DocBao.ApplicationServices.Persistence;
using DocBao.ApplicationServices.Bank;
using Davang.Utilities.Log;
using System.Threading;
using DocBao.ApplicationServices.Background;

namespace DocBao.ApplicationServices
{
    public sealed class FeedManager : BaseAppService, IFeedManager
    {
        #region Static

        private static FeedManager _feedManager;
        private static bool _loaded = false;

        private static string _lastItemId;
        private static string _lastFeedId;

        public static FeedManager GetInstance()
        {
            if (_feedManager == null)
                _feedManager = new FeedManager();

            return _feedManager;
        }

        private FeedManager()
        {
            _subscribedFeeds = new Dictionary<Guid, Feed>();
            _rssService = new RssParserService();
            _dbContext = new PersistentManager();
        }

        #endregion

        public async Task LoadAsync()
        {
            if (!_loaded)
            {
                try
                {
                    _subscribedFeeds = await _dbContext.ReadSerializedCopyAsync<IDictionary<Guid, Feed>>(AppConfig.SUBSCRIBED_FEED_FILE_NAME);
                    if (!await LoadStoredItemsAsync())
                        _storedItems = new Dictionary<string, Item>();

                    await VersionChecking();
                }
                catch (Exception ex)
                {
                    GA.LogException(ex);
                }


                if (_subscribedFeeds == null || _subscribedFeeds.Count == 0)
                {
                    _subscribedFeeds = new Dictionary<Guid, Feed>();
                    FeedBank.Feeds.Where(f => f.Default && f.Publisher.Default).ForEach(f =>
                    {
                        var cloned = f.Clone();
                        _subscribedFeeds.Add(cloned.Id, cloned);
                    });

                    await SaveAsync();
                }
                
                _loaded = true;
            }
        }

        public async Task<IDictionary<Guid, int>> LoadDownloadedFeeds()
        {
            var updated = await BackgroundDownload.LoadDownloadedFeedsAsync(_subscribedFeeds, _dbContext);
            //await SaveAsync();
            return updated;
        }

        public async Task VersionChecking()
        {
            if (AppConfig.AppUpdate == UpdateVersion.NotSet)
            {
                await UpdateForVersionBefore1_4();
                AppConfig.AppUpdate = UpdateVersion.V1_4;
            }

            if (AppConfig.AppUpdate == UpdateVersion.V1_4)
                AppConfig.AppUpdate = UpdateVersion.V1_5;

            if (AppConfig.AppUpdate == UpdateVersion.V1_5)
                AppConfig.AppUpdate = UpdateVersion.V1_6;
        }

        private IDictionary<Guid, Feed> _subscribedFeeds;
        private IDictionary<string, Item> _storedItems;
        private RssParserService _rssService;
        private IPersistentManager _dbContext;

        #region Get

        public AppResult<List<Publisher>> GetAllPublishers() 
        {
            return AppResult(FeedBank.Publishers.OrderBy(p => p.Order).ToList());
        }

        public AppResult<List<Publisher>> GetSubscribedPublishers() 
        {
            var subscribedPublishers = _subscribedFeeds.Values.Select(f => f.Publisher).Where(f => f != null).Distinct(new PublisherComparer()).ToList();
            subscribedPublishers.ForEach(p => UpdatePublisherFeedIds(p));
            return AppResult(subscribedPublishers.OrderBy(p => p.Order).ToList());
        }

        public AppResult<Publisher> GetPublisher(Guid publisherId)
        {
            if (default(Guid).Equals(publisherId)) return AppResult<Publisher>(ErrorCode.PublisherNotFound);
            
            var publisher = FeedBank.Publishers.FirstOrDefault(p => p.Id.Equals(publisherId));
            if (publisher == null) return AppResult<Publisher>(ErrorCode.PublisherNotFound);
            return AppResult(publisher);
        }

        public AppResult<Publisher> GetSubscribedPublisher(Guid publisherId)
        {
            if (default(Guid).Equals(publisherId)) return AppResult<Publisher>(ErrorCode.PublisherNotFound);

            var publisher = _subscribedFeeds.Values.Select(f => f.Publisher)
                .Distinct()
                .Where(f => f != null)
                .FirstOrDefault(p => p.Id.Equals(publisherId));
            if (publisher == null) return AppResult<Publisher>(ErrorCode.PublisherNotFound);
            UpdatePublisherFeedIds(publisher);
            return AppResult(publisher);
        }

        public AppResult<List<Feed>> GetAllFeeds(Guid publisherId) 
        {
            return AppResult(FeedBank.Feeds.Where(f => f.Publisher.Id.Equals(publisherId))
                .OrderBy(f=>f.Order)
                .ToList());
        }

        public AppResult<List<Feed>> GetSubscribedFeeds(Guid publisherId = default(Guid)) 
        {
            return AppResult(_subscribedFeeds.Values.Where(f => f.Publisher.Id.Equals(publisherId))
                .OrderBy(f=>f.Order)
                .ToList());
        }

        public IDictionary<Guid, Feed> GetSubscribedFeedsAsDictionary()
        {
            return _subscribedFeeds;
        }

        public AppResult<Feed> GetFeed(Guid feedId)
        {
            if (default(Guid).Equals(feedId)) return AppResult<Feed>(ErrorCode.FeedNotFound);

            var feed = FeedBank.Feeds.FirstOrDefault(f => f.Id.Equals(feedId));
            if (feed == null) return AppResult<Feed>(ErrorCode.FeedNotFound);
            return AppResult(feed);
        }

        public AppResult<Feed> GetSubscribedFeed(Guid feedId, bool autoSubscribe = false)
        {
            if (default(Guid).Equals(feedId)) return AppResult<Feed>(ErrorCode.FeedNotFound);
            if (!_subscribedFeeds.ContainsKey(feedId) && !autoSubscribe) return AppResult<Feed>(ErrorCode.FeedNotSubscribed);

            Feed feed = null;

            if (!_subscribedFeeds.TryGetValue(feedId, out feed) 
                && autoSubscribe 
                && (feed = FeedBank.Feeds.FirstOrDefault(f => f.Id.Equals(feedId))) != null)
                _subscribedFeeds.Add(feed.Id, feed);
             
            if (feed == null) return AppResult<Feed>(ErrorCode.FeedNotFound);
            return AppResult(feed);
        }

        public async Task<AppResult<int>> UpdateItems(Feed feed, bool refresh = false, bool saveToDisk = true)
        {
            if (!_subscribedFeeds.ContainsKey(feed.Id))
                return AppResult<int>(ErrorCode.FeedNotSubscribed);

            try
            {
                var persistentFeed = _subscribedFeeds.Values.FirstOrDefault(f => f.Id.Equals(feed.Id));
                var updated = 0;
                if (refresh)
                {
                    persistentFeed.LastUpdatedTime = DateTime.Now;
                    updated = await _rssService.UpdateItemsAsync(persistentFeed);
                    //if (updated > 0 && saveToDisk)
                    //    Task.Factory.StartNew(() => SaveAsync());
                }

                return AppResult(updated);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public AppResult<int> ReadCount(Guid publisherId)
        {
            var publisherResult = GetSubscribedPublisher(publisherId);
            if (publisherResult.HasError)
                return new AppResult<int>(publisherResult.Error);

            int readCount = 0;
            _subscribedFeeds.Values
                .Where(f => f.Publisher.Id.Equals(publisherId))
                .ForEach(f => readCount += f.Items.Count(i => i.Read));

            return AppResult(readCount);
        }

        public AppResult<int> ItemCount(Guid publisherId)
        {
            var publisherResult = GetSubscribedPublisher(publisherId);
            if (publisherResult.HasError)
                return new AppResult<int>(publisherResult.Error);

            int readCount = 0;
            _subscribedFeeds.Values
                .Where(f => f.Publisher.Id.Equals(publisherId))
                .ForEach(f => readCount += f.Items.Count());

            return AppResult(readCount);
        }

        public AppResult<int> FeedCount(Guid publisherId)
        {
            var publisherResult = GetPublisher(publisherId);
            if (publisherResult.HasError)
                return new AppResult<int>(publisherResult.Error);

            return AppResult(publisherResult.Target.FeedIds.Count());
        }

        public AppResult<int> SubscribedFeedCount(Guid publisherId)
        {
            var publisherResult = GetSubscribedPublisher(publisherId);
            if (publisherResult.HasError)
                return new AppResult<int>(publisherResult.Error);

            int readCount = _subscribedFeeds.Values.Where(f => f.Publisher.Id.Equals(publisherId)).Count();

            return AppResult(readCount);
        }

        public int AllSubscribedFeedCount()
        {
            return _subscribedFeeds.Values.Count;
        }

        #endregion

        #region Set

        public AppResult<bool> MarkItemAsRead(Guid feedId, string itemId, bool read)
        {
            if (!_subscribedFeeds.ContainsKey(feedId)) return AppResult<bool>(ErrorCode.FeedNotSubscribed);
            var feed = _subscribedFeeds[feedId];
            var item = feed.Items.FirstOrDefault(i => i.Id.Equals(itemId));
            if (item == null) return AppResult<bool>(ErrorCode.ItemNotFound);

            if (item.Read && read) return AppResult<bool>(ErrorCode.ItemAlreadyRead);
            if (!item.Read && !read) return AppResult<bool>(ErrorCode.ItemAlreadyUnread);

            item.Read = read;
            //Task.Factory.StartNew(() => Save());

            return AppResult(true);
        }

        public void SetLastId<TId>(string id)
        {
            if (typeof(TId) == typeof(string))
                _lastItemId = id;

            if (typeof(TId) == typeof(Guid))
                _lastFeedId = id;
        }

        public string GetLastId<TId>() 
        {
            if (typeof(TId) == typeof(string))
                return _lastItemId;

            if (typeof(TId) == typeof(Guid))
                return _lastFeedId;

            return string.Empty;
        }

        #endregion

        #region Subscribe/Unsubscribe

        public async Task<AppResult<bool>> SubscribeFeed(Guid feedId) 
        {
            var feed = FeedBank.Feeds.FirstOrDefault(f => f.Id.Equals(feedId));
            if (feed == null)
                return AppResult<bool>(ErrorCode.FeedNotFound);
            
            if (_subscribedFeeds.ContainsKey(feedId))
                return AppResult<bool>(ErrorCode.FeedAlreadySubscribed);

            int subscribedFeedCount = _subscribedFeeds.Values.Count;
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION) &&
                subscribedFeedCount >= AppConfig.UN_PAID_MAX_SUBSCRIBED_FEED_ALLOW)
            {
                return AppResult<bool>(ErrorCode.LicenseRequired);
            }

            _subscribedFeeds.Add(feed.Id, feed);
            await SaveAsync();

            return AppResult(true);
        }

        public async Task<AppResult<bool>> UnsubscribeFeed(Guid feedId) 
        {
            var feed = FeedBank.Feeds.FirstOrDefault(f => f.Id.Equals(feedId));
            if (feed == null)
                return AppResult<bool>(ErrorCode.FeedNotFound);

            if (!_subscribedFeeds.ContainsKey(feedId))
                return AppResult<bool>(ErrorCode.FeedNotSubscribed);

            _subscribedFeeds.Remove(feedId);
            await SaveAsync();

            return AppResult(true);
        }

        public async Task<AppResult<bool>> SubscribePublisher(Guid publisherId) 
        {
            var publisher = FeedBank.Publishers.FirstOrDefault(p => p.Id.Equals(publisherId));
            if (publisher == null)
                return AppResult<bool>(ErrorCode.PublisherNotFound);

            var feeds = FeedBank.Feeds.Where(f => f.Publisher.Id.Equals(publisherId) 
                && !_subscribedFeeds.ContainsKey(f.Id)).ToList();

            feeds.Where(f => f.Default).ForEach(f => _subscribedFeeds.Add(f.Id, f));
            await SaveAsync();

            return AppResult(true);
        }

        public async Task<AppResult<bool>> UnsubscribePublisher(Guid publisherId) 
        {
            var publisher = FeedBank.Publishers.FirstOrDefault(p => p.Id.Equals(publisherId));
            if (publisher == null)
                return AppResult<bool>(ErrorCode.PublisherNotFound);

            var feeds = FeedBank.Feeds.Where(f => f.Publisher.Id.Equals(publisherId)
                && _subscribedFeeds.ContainsKey(f.Id)).ToList();

            feeds.ForEach(f => _subscribedFeeds.Remove(f.Id));
            await SaveAsync();

            return AppResult(true);
        }

        #endregion

        #region Stored Feeds

        public async Task<AppResult<bool>> StoreItemAsync(Item item)
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                return AppResult<bool>(ErrorCode.LicenseRequired);

            if (item == null)
                return AppResult<bool>(ErrorCode.ItemNotFound);

            if (_storedItems == null)
                _storedItems = new Dictionary<string, Item>();

            if (_storedItems.ContainsKey(item.Id))
                return AppResult<bool>(ErrorCode.ItemAlreadyStored);

            _storedItems.Add(item.Id, item);
            await SaveItemsAsync();
            return AppResult(true);
        }

        public AppResult<Item> GetStoredItem(string itemId)
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                return AppResult<Item>(ErrorCode.LicenseRequired);

            if (string.IsNullOrEmpty(itemId))
                return AppResult<Item>(ErrorCode.ItemNotFound);

            if (_storedItems == null || _storedItems.Count == 0 || !_storedItems.ContainsKey(itemId))
                return AppResult<Item>(ErrorCode.NoStoredItemFound);

            return AppResult(_storedItems[itemId]);
        }

        public AppResult<List<Item>> GetStoredItems()
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                return AppResult<List<Item>>(ErrorCode.LicenseRequired);

            if (_storedItems == null)
                return AppResult<List<Item>>(ErrorCode.NoStoredItemFound);

            return AppResult(_storedItems.Values.ToList());
        }

        public bool IsStored(string itemId)
        {
            return _storedItems.ContainsKey(itemId);
        }

        public async Task<AppResult<bool>> DeleteStoredItemAsync(string itemId)
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                return AppResult<bool>(ErrorCode.LicenseRequired);

            if (_storedItems == null || _storedItems.Count == 0 || !_storedItems.ContainsKey(itemId))
                return AppResult<bool>(ErrorCode.NoStoredItemFound);

            _storedItems.Remove(itemId);
            await SaveItemsAsync();
            return AppResult(true);
        }

        public async Task<AppResult<bool>> ClearStoredItemsAsync()
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                return AppResult<bool>(ErrorCode.LicenseRequired);

            if (_storedItems == null || _storedItems.Count == 0)
                return AppResult<bool>(ErrorCode.NoStoredItemFound);

            _storedItems.Clear();
            await SaveItemsAsync();
            return AppResult(true);
        }

        public AppResult<bool> MarkStoredItemAsRead(string itemId, bool read)
        {
            if (!LicenseHelper.Purchased(AppConfig.PAID_VERSION))
                return AppResult<bool>(ErrorCode.LicenseRequired);

            if (string.IsNullOrEmpty(itemId))
                return AppResult<bool>(ErrorCode.ItemNotFound);

            var item = _storedItems.Values.FirstOrDefault(i => i.Id.Equals(itemId));
            if (item == null) return AppResult<bool>(ErrorCode.ItemNotFound);

            if (item.Read && read) return AppResult<bool>(ErrorCode.ItemAlreadyRead);
            if (!item.Read && !read) return AppResult<bool>(ErrorCode.ItemAlreadyUnread);

            item.Read = read;
            Task.Factory.StartNew(() => SaveItemsAsync());

            return AppResult(true);
        }

        #endregion

        #region CustomView

        public Category GetCategory(Guid categoryId)
        {
            return CategoryBank.Categories.FirstOrDefault(c => c.Id.Equals(categoryId));
        }

        public IList<Category> GetCategories() 
        {
            return CategoryBank.Categories;
        }

        public async Task<KeyValuePair<int, IList<Item>>> GetItemsByCategory(Guid categoryId, int top, bool refresh = false) 
        {
            var updatedItemCount = 0;
            var category = CategoryBank.Categories.FirstOrDefault(c => c.Id.Equals(categoryId));
            if (category == null) return new KeyValuePair<int,IList<Item>>(0, null);

            var items = new List<Item>();

            foreach (var feed in category.Feeds)
            {
                if (!_subscribedFeeds.ContainsKey(feed.Id))
                    _subscribedFeeds.Add(feed.Id, feed);

                var feedToUpdate = _subscribedFeeds[feed.Id];
                if (feedToUpdate != null)
                {
                    if (refresh)
                    {
                        try
                        {
                            var result = await UpdateItems(feedToUpdate, true, false);
                            if (!result.HasError)
                                updatedItemCount += result.Target;
                        }
                        catch (ApplicationException ex) 
                        {
                            GA.LogException(ex);
                        }
                    }
                    items.AddRange(feedToUpdate.Items.OrderByDescending(i => i.PublishDate).ToList());
                }
                Thread.Sleep(100);
            }

            //Task.Factory.StartNew(() => Save());

            items = items.OrderByDescending(i => i.PublishDate).Take(top * category.Feeds.Count()).ToList();
            return new KeyValuePair<int, IList<Item>>(updatedItemCount, items);
        }

        #endregion

        public async Task<bool> SaveAsync()
        {
            if (_subscribedFeeds == null || _subscribedFeeds.Count == 0) return false;
            var savingFeeds = TailorSavingFeeds(_subscribedFeeds);

            try
            {
                return await _dbContext.UpdateSerializedCopyAsync(savingFeeds, AppConfig.SUBSCRIBED_FEED_FILE_NAME);
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
                return false;
            }
        }

        public bool Save()
        {
            if (_subscribedFeeds == null || _subscribedFeeds.Count == 0) return false;
            var savingFeeds = TailorSavingFeeds(_subscribedFeeds);

            try
            {
                return _dbContext.UpdateSerializedCopy(savingFeeds, AppConfig.SUBSCRIBED_FEED_FILE_NAME, false);
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
                return false;
            }
        }

        public void CreateFeedsToUpdate()
        {
            BackgroundDownload.CreateFeedsToDownload(_subscribedFeeds);
        }

        #region private

        public static IDictionary<Guid, Feed> TailorSavingFeeds(IDictionary<Guid, Feed> bareFeeds)
        {
            if (bareFeeds == null || bareFeeds.Count == 0)
                return null;

            var savingFeeds = new Dictionary<Guid, Feed>();
            bareFeeds.Values.ForEach(feed =>
            {
                var savingFeed = feed;
                savingFeed.Items = feed.Items.OrderByDescending(i => i.PublishDate).Take(AppConfig.MaxItemStored).ToList();
                savingFeeds.Add(savingFeed.Id, savingFeed);
            });

            return savingFeeds;
        }

        public static async Task SyncFeedsAsync(IDictionary<Guid, Feed> newFeeds)
        {
            if (newFeeds == null || newFeeds.Count == 0) return;

            var dbContext = new PersistentManager();
            var savedFeeds = await dbContext.ReadSerializedCopyAsync<IDictionary<Guid, Feed>>(AppConfig.SUBSCRIBED_FEED_FILE_NAME);
            if (savedFeeds == null || savedFeeds.Count == 0) return;

            SyncFeeds(newFeeds, savedFeeds);
        }

        public static void SyncFeeds(IDictionary<Guid, Feed> newFeeds)
        {
            var dbContext = new PersistentManager();
            var savedFeeds = dbContext.ReadSerializedCopy<IDictionary<Guid, Feed>>(AppConfig.SUBSCRIBED_FEED_FILE_NAME);
            SyncFeeds(newFeeds, savedFeeds);
        }

        private static void SyncFeeds(IDictionary<Guid, Feed> newFeeds, IDictionary<Guid, Feed> savedFeeds)
        {
            savedFeeds.Values.ForEach(savedFeed =>
            {
                var newFeed = newFeeds.Values.FirstOrDefault(nf => nf.Id.Equals(savedFeed.Id));
                if (newFeed != null)
                    SyncItems(newFeed, savedFeed);
                else
                    newFeeds.Add(savedFeed.Id, savedFeed);
            });
        }

        private static void SyncItems(Feed newFeed, Feed savedFeed)
        {
            savedFeed.Items.ForEach(savedItem =>
            {
                var newItem = newFeed.Items.FirstOrDefault(ni => ni.Id.Equals(savedItem.Id));
                if (newItem == null)
                    newFeed.Items.Add(savedItem);
            });
        }

        private void UpdatePublisherFeedIds(Publisher publisher)
        {
            publisher.FeedIds.Clear();

            var feeds = new List<Feed>();
            _subscribedFeeds.Values.ForEach(f => 
            {
                if (f.Publisher.Id.Equals(publisher.Id))
                    feeds.Add(f);
                    //publisher.FeedIds.Add(f.Id);
            });

            feeds.OrderBy(f =>f.Order).ToList().ForEach(f=>publisher.FeedIds.Add(f.Id));
            feeds.ForEach(f => f.Publisher = publisher);
        }

        private async Task<bool> LoadStoredItemsAsync()
        {
            if (_storedItems == null || _storedItems.Count == 0)
                _storedItems = await _dbContext.ReadSerializedCopyAsync<IDictionary<string, Item>>(AppConfig.STORED_ITEM_FILE_NAME);
            return _storedItems != null && _storedItems.Count > 0;
        }

        private async Task<bool> SaveItemsAsync()
        {
            if (_storedItems == null) return false;
            return await _dbContext.UpdateSerializedCopyAsync(_storedItems, AppConfig.STORED_ITEM_FILE_NAME);
        }

        private async Task<bool> UpdateForVersionBefore1_4()
        {
            if (_subscribedFeeds == null || _subscribedFeeds.Values.Count() == 0)
                return false;

            try
            {
                _subscribedFeeds.Values
                    .Where(f => f.Publisher.Id.Equals(new Guid("455b6156-77ba-4023-a057-9c06c7f60849")))
                    .ForEach(f => 
                        {
                            var feedFromBank = FeedBank.Feeds.FirstOrDefault(ffb => ffb.Id.Equals(f.Id));
                            if (feedFromBank != null)
                                f.Link = feedFromBank.Link;
                        });

                await SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
                return false;
            }
        }

        #endregion
    }
}
