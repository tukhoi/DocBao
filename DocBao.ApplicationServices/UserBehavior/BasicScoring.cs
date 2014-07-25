using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;

namespace DocBao.ApplicationServices.UserBehavior
{
    internal class BasicScoring : IScoring
    {
        IFeedManager _feedManager;

        public BasicScoring(IFeedManager feedManager = null)
        {
            _feedManager = feedManager ?? FeedManager.Instance;
        }

        public IDictionary<KeyValuePair<UserAction, string>, int> UserBehaviors { get; set; }

        public virtual IDictionary<Guid, int> ScorePublishers(short pubCount = 10)
        {
            if (UserBehaviors == null || UserBehaviors.Count == 0)
                throw new ApplicationException("No user behavior to score");

            IDictionary<Guid, int> scoredPubs = new Dictionary<Guid, int>();

            UserBehaviors.ForEach(ub =>
            {
                var action = ub.Key.Key;
                var entityId = ub.Key.Value;
                var weight = GetWeight(action);
                var pubIds = GetPubId(action, entityId);
                pubIds.Where(pId => !default(Guid).Equals(pId))
                    .ForEach(pId =>
                    {
                        var score = weight * ub.Value;
                        scoredPubs.Add(pId, score);
                    });
            });

            return scoredPubs.OrderByDescending(x => x.Value).Take(pubCount).ToDictionary(x => x.Key, x => x.Value);
        }

        public virtual IDictionary<Guid, int> ScoreFeeds(short feedCount = 50)
        {
            if (UserBehaviors == null || UserBehaviors.Count == 0)
                throw new ApplicationException("No user behavior to score");

            IDictionary<Guid, int> scoredFeeds = new Dictionary<Guid, int>();

            UserBehaviors.ForEach(ub =>
                {
                    var action = ub.Key.Key;
                    var entityId = ub.Key.Value;
                    var weight = GetWeight(action);
                    var feedIds = GetFeedIds(action, entityId);
                    feedIds.Where(fId => !default(Guid).Equals(fId))
                        .ForEach(fId =>
                            {
                                var score = weight * ub.Value;
                                scoredFeeds.AddTo(fId, score);
                            });
                });

            return scoredFeeds.OrderByDescending(x => x.Value).Take(feedCount).ToDictionary(x => x.Key, x => x.Value);
        }

        protected virtual short GetWeight(UserAction action)
        {
            short weight = 0;

            switch (action)
            {
                case UserAction.PubEnter:
                    weight = 1;
                    break;
                case UserAction.FeedEnter:
                    weight = 2;
                    break;
                case UserAction.ItemEnter:
                    weight = 3;
                    break;
                case UserAction.ItemEmail:
                    weight = 2;
                    break;
                case UserAction.ItemStore:
                    weight = 3;
                    break;
                case UserAction.ItemLink:
                    weight = 2;
                    break;
                case UserAction.ItemShare:
                    weight = 2;
                    break;
                case UserAction.CatEnter:
                    weight = 1;
                    break;
            }

            return weight;
        }

        protected virtual IList<Guid> GetPubId(UserAction action, string entityId)
        {
            IList<Guid> pubIds = new List<Guid>();

            switch (action)
            {
                case UserAction.PubEnter:
                    pubIds.Add(new Guid(entityId));
                    break;
                case UserAction.FeedEnter:
                case UserAction.ItemEnter:
                case UserAction.ItemEmail:
                case UserAction.ItemStore:
                case UserAction.ItemLink:
                case UserAction.ItemShare:
                    pubIds.Add(GetPubIdFromStore(entityId));
                    break;
                case UserAction.CatEnter:
                    var ids = GetPubIdsFromCat(entityId);
                    if (ids != null && ids.Count > 0)
                        ids.ForEach(pId => pubIds.Add(pId));
                    break;
            }

            return pubIds;
        }

        protected virtual IList<Guid> GetFeedIds(UserAction action, string entityId)
        {
            IList<Guid> feedIds = new List<Guid>();

            switch (action)
            { 
                case UserAction.PubEnter:
                    feedIds = GetFeedIdsFromPub(entityId);
                    break;
                case UserAction.FeedEnter:
                case UserAction.ItemEnter:
                case UserAction.ItemEmail:
                case UserAction.ItemStore:
                case UserAction.ItemLink:
                case UserAction.ItemShare:
                    feedIds.Add(new Guid(entityId));
                    break;
                case UserAction.CatEnter:
                    var ids = GetFeedIdsFromCat(entityId);
                    if (ids != null && ids.Count > 0)
                        ids.ForEach(pId => feedIds.Add(pId));
                    break;
            }

            return feedIds;
        }

        protected Guid GetPubIdFromStore(string feedId)
        {
            var feedResult = _feedManager.GetSubscribedFeed(new Guid(feedId));
            if (feedResult.HasError) return default(Guid);

            return feedResult.Target.Publisher.Id;
        }

        protected IList<Guid> GetPubIdsFromCat(string catId)
        {
            var catResult = _feedManager.GetCategory(new Guid(catId));
            if (catResult == null) return null;

            return catResult.Feeds.Select(f => f.Publisher.Id).Distinct().ToList();
        }

        protected IList<Guid> GetFeedIdsFromPub(string pubId)
        {
            var pubResult = _feedManager.GetSubscribedPublisher(new Guid(pubId));
            if (pubResult.HasError) return null;

            return pubResult.Target.FeedIds;
        }

        protected IList<Guid> GetFeedIdsFromCat(string catId)
        {
            var catResult = _feedManager.GetCategory(new Guid(catId));
            if (catResult == null) return null;

            return catResult.Feeds.Select(f => f.Id).Distinct().ToList();
        }
    }
}
