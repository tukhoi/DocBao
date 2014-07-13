//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Davang.Utilities.Extensions;
//using Davang.Utilities.Helpers;
//using Davang.Parser.Dto;

//namespace DocBao.ApplicationServices.UserBehavior
//{
//    public class UserBehaviorStore : IUserBehavior
//    {
//        static UserBehaviorStore _instance;
//        private IFeedManager _feedManager;
//        private IDictionary<Guid, int> _pubClicks;
//        private IDictionary<Guid, int> _feedClicks;
//        private IDictionary<KeyValuePair<Guid, string>, int> _itemClicks;

//        internal const short PUB_COUNT = 10;
//        internal const short FEED_COUNT = 50;

//        public static UserBehaviorStore GetInstance(IFeedManager feedManager = null)
//        {
//            if (_instance == null)
//                _instance = new UserBehaviorStore(feedManager);

//            return _instance;
//        }

//        public UserBehaviorStore(IFeedManager feedManager = null)
//        {
//            _feedManager = feedManager ?? FeedManager.GetInstance();
//            _pubClicks = new Dictionary<Guid, int>();
//            _feedClicks = new Dictionary<Guid, int>();
//            _itemClicks = new Dictionary<KeyValuePair<Guid, string>, int>();

//            Load();
//        }

//        #region Data

//        public void Clear()
//        {
//            _pubClicks.Clear();
//            _feedClicks.Clear();
//            _itemClicks.Clear();
//        }

//        public void Load()
//        {
//            Clear();
//            _pubClicks = AppConfig.GetPersistentConfig<IDictionary<Guid, int>>(ConfigKey.PubClicks, _pubClicks);
//            _feedClicks = AppConfig.GetPersistentConfig<IDictionary<Guid, int>>(ConfigKey.FeedClicks, _feedClicks);
//            _itemClicks = AppConfig.GetPersistentConfig<IDictionary<KeyValuePair<Guid, string>, int>>(ConfigKey.ItemClicks, _itemClicks);
//        }

//        public void Save()
//        {
//            AppConfig.SetPersistentConfig(ConfigKey.PubClicks, _pubClicks);
//            AppConfig.SetPersistentConfig(ConfigKey.FeedClicks, _feedClicks);
//            AppConfig.SetPersistentConfig(ConfigKey.ItemClicks, _itemClicks);
//        }

//        public void PublisherClick(Guid pubId, int value = 1)
//        {
//            AddToDictionary(_pubClicks, pubId, value);
//        }

//        public void FeedClick(Guid feedId, int value = 1)
//        {
//            AddToDictionary(_feedClicks, feedId, value);
//        }

//        public void ItemClick(Guid feedId, string itemId, int value = 1)
//        {
//            AddToDictionary(_itemClicks, new KeyValuePair<Guid, string>(feedId, itemId), value);
//        }

//        #endregion

//        #region public

//        public IDictionary<Guid, int> ScorePublishers(int pubCount = PUB_COUNT)
//        {
//            var pubScore = new Dictionary<Guid, int>();

//            var pubBasicScoreByFeedClicks = PubBasicScoreByFeedClicks();
//            var pubBasicScoreByItemClicks = PubBasicScoreByItemClicks();
            
//            _pubClicks.ForEach(pc => AddToDictionary(pubScore, pc.Key, pc.Value));
//            pubBasicScoreByFeedClicks.ForEach(x => AddToDictionary(pubScore, x.Key, x.Value * 2));
//            pubBasicScoreByItemClicks.ForEach(x => AddToDictionary(pubScore, x.Key, x.Value * 3));

//            return pubScore.OrderByDescending(ps => ps.Value).Take(pubCount).ToDictionary(x => x.Key, x => x.Value);
//        }

//        public IDictionary<Guid, int> ScoreFeeds(int feedCount = FEED_COUNT)
//        {
//            var feedScore = new Dictionary<Guid, int>();

//            var feedScoreByPubClicks = FeedBasicScoreByPubClicks();
//            var feedScoreByItemClicks = FeedBasicScoreByItemClicks();

//            feedScoreByPubClicks.ForEach(x => AddToDictionary(feedScore, x.Key, x.Value));
//            _feedClicks.ForEach(fc => AddToDictionary(feedScore, fc.Key, fc.Value * 2));
//            feedScoreByItemClicks.ForEach(x => AddToDictionary(feedScore, x.Key, x.Value * 3));

//            return feedScore.OrderByDescending(fs => fs.Value).Take(feedCount).ToDictionary(x => x.Key, x => x.Value);
//        }

//        #endregion

//        #region private

//        private void AddToDictionary<T>(IDictionary<T, int> dictionary, T id, int value = 1)
//        {
//            if (dictionary.ContainsKey(id))
//                dictionary[id]+=value;
//            else
//                dictionary.Add(id, value);
//        }

//        private IDictionary<Guid, int> PubBasicScoreByFeedClicks()
//        {
//            var pubBasicScore = new Dictionary<Guid, int>();
//            var feedModelClicks = FeedModelClicks();
//            feedModelClicks.ForEach(fmc =>
//                {
//                    var pubId = fmc.Key.Publisher.Id;
//                    AddToDictionary(pubBasicScore, pubId, fmc.Value);
//                });

//            return pubBasicScore;
//        }

//        private IDictionary<Guid, int> PubBasicScoreByItemClicks()
//        {
//            var pubBasicScore = new Dictionary<Guid, int>();
//            var itemModelClicks = ItemModelClicks();
//            itemModelClicks.ForEach(imc =>
//                {
//                    var pubId = imc.Key.Key.Publisher.Id;
//                    AddToDictionary(pubBasicScore, pubId, imc.Value);
//                });

//            return pubBasicScore;
//        }

//        /// <summary>
//        /// When a pub is clicked, all feeds belong to that pub should get the same score
//        /// </summary>
//        /// <returns></returns>
//        private IDictionary<Guid, int> FeedBasicScoreByPubClicks()
//        {
//            var feedBasicScore = new Dictionary<Guid, int>();
//            _pubClicks.ForEach(pc =>
//                {
//                    var pubResult = _feedManager.GetSubscribedPublisher(pc.Key);
//                    if (pubResult.HasError) return;
//                    pubResult.Target.FeedIds.ForEach(fid =>
//                        {
//                            AddToDictionary(feedBasicScore, fid, pc.Value);
//                        });
//                });

//            return feedBasicScore;
//        }

//        private IDictionary<Guid, int> FeedBasicScoreByItemClicks()
//        {
//            var feedBasicScore = new Dictionary<Guid, int>();
//            _itemClicks.ForEach(ic =>
//                {
//                    var fid = ic.Key.Key;
//                    AddToDictionary(feedBasicScore, fid, ic.Value);
//                });

//            return feedBasicScore;
//        }

//        private IDictionary<Feed, int> FeedModelClicks()
//        {
//            return _feedClicks.ToDictionary(fc => GetSubscribedFeed(fc.Key), fc => fc.Value);
//        }

//        private IDictionary<KeyValuePair<Feed, Item>, int> ItemModelClicks()
//        {
//            return _itemClicks.ToDictionary(ic => GetItem(ic.Key), ic => ic.Value);
//        }

//        private Feed GetSubscribedFeed(Guid feedId)
//        {
//            var feedResult = _feedManager.GetSubscribedFeed(feedId);
//            return feedResult.Target;
//        }

//        private KeyValuePair<Feed, Item> GetItem(KeyValuePair<Guid, string> feedItem)
//        {
//            var feed = GetSubscribedFeed(feedItem.Key);
//            if (feed == null) return default(KeyValuePair<Feed, Item>);

//            var item = feed.Items.FirstOrDefault(i => i.Id.Equals(feedItem.Value));
//            if (item == null) return default(KeyValuePair<Feed, Item>);

//            return new KeyValuePair<Feed,Item>(feed, item);
//        }

//        #endregion
//    }
//}
