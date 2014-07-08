using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using Davang.Utilities.Helpers;
using Davang.Parser.Dto;

namespace DocBao.ApplicationServices.UserBehavior
{
    public class UserBehaviorStore : IUserBehaviorStore
    {
        private static string PUB_CLICKS_CONFIG_KEY = "PubClicks";
        private static string FEED_CLICKS_CONFIG_KEY = "FeedClicks";
        private static string ITEM_CLICKS_CONFIG_KEY = "ItemClicks";

        static UserBehaviorStore _instance;
        private IFeedManager _feedManager;
        private IDictionary<Guid, int> _pubClicks;
        private IDictionary<Guid, int> _feedClicks;
        private IDictionary<KeyValuePair<Guid, string>, int> _itemClicks;

        private short PubCount = 10;
        private short FeedCount = 50;
        private short ItemCount = 100;

        public static UserBehaviorStore GetInstance(IFeedManager feedManager = null)
        {
            if (_instance == null)
                _instance = new UserBehaviorStore(feedManager);

            return _instance;
        }

        public UserBehaviorStore(IFeedManager feedManager = null)
        {
            _feedManager = feedManager ?? FeedManager.GetInstance();
            _pubClicks = new Dictionary<Guid, int>();
            _feedClicks = new Dictionary<Guid, int>();
            _itemClicks = new Dictionary<KeyValuePair<Guid, string>, int>();

            Load();
        }

        public void Clear()
        {
            _pubClicks.Clear();
            _feedClicks.Clear();
            _itemClicks.Clear();
        }

        public void Load()
        {
            Clear();
            _pubClicks = AppConfig.GetPersistentConfig<IDictionary<Guid, int>>(ConfigKey.PublisherClicks, _pubClicks);
            _feedClicks = AppConfig.GetPersistentConfig<IDictionary<Guid, int>>(ConfigKey.FeedClicks, _feedClicks);
            _itemClicks = AppConfig.GetPersistentConfig<IDictionary<KeyValuePair<Guid, string>, int>>(ConfigKey.ItemClicks, _itemClicks);
        }

        public void Save()
        {
            AppConfig.SetPersistentConfig(ConfigKey.PublisherClicks, _pubClicks);
            AppConfig.SetPersistentConfig(ConfigKey.FeedClicks, _feedClicks);
            AppConfig.SetPersistentConfig(ConfigKey.ItemClicks, _itemClicks);
        }

        public void PublisherClick(Guid pubId, int value = 1)
        {
            AddToDictionary(_pubClicks, pubId, value);
        }

        public void FeedClick(Guid feedId, int value = 1)
        {
            AddToDictionary(_feedClicks, feedId, value);
        }

        public void ItemClick(Guid feedId, string itemId, int value = 1)
        {
            AddToDictionary(_itemClicks, new KeyValuePair<Guid, string>(feedId, itemId), value);
        }

        public IDictionary<Guid, int> ScorePublishers()
        {
            var interestingPublishersByItems = PublisherScoreByItemClicks();
            var interestingPublishersByFeeds = PublisherScoreByFeedClicks();
            
            var pubScore = new Dictionary<Guid, int>();
            _pubClicks.ForEach(pc =>
                {
                    AddToDictionary(pubScore, pc.Key, pc.Value);
                });
            interestingPublishersByFeeds.ForEach(x =>
                {
                    AddToDictionary(pubScore, x.Key, x.Value * 2);
                });
            interestingPublishersByItems.ForEach(x =>
                {
                    AddToDictionary(pubScore, x.Key, x.Value * 3);
                });

            return pubScore.OrderByDescending(p => p.Value).Take(PubCount).ToDictionary(x => x.Key, x => x.Value);
        }

        public IDictionary<Guid, int> ScoreFeeds()
        {
            throw new NotImplementedException();
        }

        public IDictionary<KeyValuePair<Guid, string>, int> ScoreItems()
        {
            throw new NotImplementedException();
        }

        public IList<Guid> GetFeedsToDownloadInBackground()
        {
            throw new NotImplementedException();
        }

        private void AddToDictionary<T>(IDictionary<T, int> dictionary, T id, int value = 1)
        {
            if (dictionary.ContainsKey(id))
                dictionary[id]+=value;
            else
                dictionary.Add(id, value);
        }

        private IDictionary<Guid, int> PublisherScoreByFeedClicks()
        {
            var pubScore = new Dictionary<Guid, int>();
            var feedModelClicks = FeedModelClicks();
            feedModelClicks.ForEach(fmc =>
                {
                    var pubId = fmc.Key.Publisher.Id;
                    AddToDictionary(pubScore, pubId, fmc.Value);
                });

            return pubScore;
        }

        private IDictionary<Guid, int> PublisherScoreByItemClicks()
        {
            var pubScore = new Dictionary<Guid, int>();
            var itemModelClicks = ItemModelClicks();
            itemModelClicks.ForEach(imc =>
                {
                    var pubId = imc.Key.Key.Publisher.Id;
                    AddToDictionary(pubScore, pubId, imc.Value);
                });

            return pubScore;
        }

        private IDictionary<Feed, int> FeedModelClicks()
        {
            return _feedClicks.ToDictionary(fc => GetSubscribedFeed(fc.Key), fc => fc.Value);
        }

        private IDictionary<KeyValuePair<Feed, Item>, int> ItemModelClicks()
        {
            return _itemClicks.ToDictionary(ic => GetItem(ic.Key), ic => ic.Value);
        }

        private Feed GetSubscribedFeed(Guid feedId)
        {
            var feedResult = _feedManager.GetSubscribedFeed(feedId);
            return feedResult.Target;
        }

        private KeyValuePair<Feed, Item> GetItem(KeyValuePair<Guid, string> feedItem)
        {
            var feed = GetSubscribedFeed(feedItem.Key);
            if (feed == null) return default(KeyValuePair<Feed, Item>);

            var item = feed.Items.FirstOrDefault(i => i.Id.Equals(feedItem.Value));
            if (item == null) return default(KeyValuePair<Feed, Item>);

            return new KeyValuePair<Feed,Item>(feed, item);
        }
    }
}
