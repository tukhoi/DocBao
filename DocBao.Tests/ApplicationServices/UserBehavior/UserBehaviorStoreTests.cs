//using DocBao.ApplicationServices.UserBehavior;
//using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Davang.Utilities.Extensions;
//using DocBao.ApplicationServices;
//using Davang.Parser.Dto;

//namespace DocBao.Tests.ApplicationServices.UserBehavior
//{
//    [TestClass]
//    public class UserBehaviorStoreTests
//    {
//        [TestMethod]
//        public void GetPublisherScore()
//        {
//            var feedManager = new MockFeedManager();
//            var userBehavior = UserBehaviorStore.GetInstance(feedManager);
//            InitializeData(userBehavior, feedManager);

//            var result = userBehavior.ScorePublishers();
//            Assert.IsNotNull(result);
//            Assert.IsTrue(result.Count > 0);
//            result.ForEach(r =>
//                {
//                    Console.WriteLine(string.Format("{0}: {1}", r.Key, r.Value));
//                });

//            result.ForEach(r =>
//                { 
                    
//                });
//        }

//        private void InitializeData(IUserBehaviorLog userBehavior, MockFeedManager feedManager)
//        {
//            userBehavior.Clear();
//            var random = new Random((int)DateTime.Now.Ticks);
//            for (int i = 0; i < 15; i++)
//            {
//                var pubId = Guid.NewGuid();
//                var pubClicks = random.Next(1, 5);
//                userBehavior.PublisherClick(pubId, pubClicks);

//                for (int j = 0; j < 10; j++)
//                {
//                    var feedId = Guid.NewGuid();
//                    var feedClicks = random.Next(1, 10);
//                    userBehavior.FeedClick(feedId, feedClicks);
//                    feedManager.AddFeed(pubId, feedId);

//                    for (int k = 0; k < 20; k++)
//                    {
//                        var itemId = Guid.NewGuid().ToString();
//                        userBehavior.ItemClick(feedId, itemId);
//                        feedManager.AddItem(feedId, itemId);
//                    }
//                }
//            }
//        }

//    }

//    class MockFeedManager : IFeedManager
//    {
//        IDictionary<Guid, Feed> _feeds = new Dictionary<Guid, Feed>();

//        public void AddFeed(Guid pubId, Guid feedId)
//        {
//            if (!_feeds.ContainsKey(feedId))
//            {
//                var feed = new Feed()
//                {
//                    Id = feedId,
//                    Publisher = new Publisher() { Id = pubId }
//                };
//                _feeds.Add(feedId, feed);
//            }
//        }

//        public void AddItem(Guid feedId, string itemId)
//        {
//            if (_feeds.ContainsKey(feedId))
//            {
//                _feeds[feedId].Items.Add(new Item() { Id = itemId });
//            }
//        }

//        public AppResult<List<Publisher>> GetAllPublishers()
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<List<Publisher>> GetSubscribedPublishers()
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<Publisher> GetPublisher(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<Publisher> GetSubscribedPublisher(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<List<Feed>> GetAllFeeds(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<List<Feed>> GetSubscribedFeeds(Guid publisherId = default(Guid))
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<Feed> GetFeed(Guid feed)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<Feed> GetSubscribedFeed(Guid feedId, bool autoSubscribe = false)
//        {
//            var feed = _feeds[feedId];
//            return new AppResult<Feed>(feed);
//        }

//        public Task<AppResult<int>> UpdateItems(Feed feed, bool refresh = false, bool saveToDisk = true)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<int> ReadCount(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<int> ItemCount(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<int> FeedCount(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<int> SubscribedFeedCount(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public int AllSubscribedFeedCount()
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<bool> MarkItemAsRead(Guid feedId, string itemId, bool read)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetLastId<TId>(string id)
//        {
//            throw new NotImplementedException();
//        }

//        public string GetLastId<TId>()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> SubscribeFeed(Guid feedId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> UnsubscribeFeed(Guid feedId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> SubscribePublisher(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> UnsubscribePublisher(Guid publisherId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> StoreItemAsync(Item item)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<Item> GetStoredItem(string itemId)
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<List<Item>> GetStoredItems()
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsStored(string itemId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> DeleteStoredItemAsync(string itemId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AppResult<bool>> ClearStoredItemsAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public AppResult<bool> MarkStoredItemAsRead(string itemId, bool read)
//        {
//            throw new NotImplementedException();
//        }

//        public DocBao.ApplicationServices.Bank.Category GetCategory(Guid categoryId)
//        {
//            throw new NotImplementedException();
//        }

//        public IList<DocBao.ApplicationServices.Bank.Category> GetCategories()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<KeyValuePair<int, IList<Item>>> GetItemsByCategory(Guid categoryId, int top, bool refresh = false)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<bool> SaveAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public bool Save()
//        {
//            throw new NotImplementedException();
//        }

//        public void CreateFeedsToUpdate()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
