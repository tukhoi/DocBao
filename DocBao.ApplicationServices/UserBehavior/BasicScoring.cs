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
                var score = weight * ub.Value;
                var pubId = GetPubId(action, entityId);
                scoredPubs.Add(pubId, score);
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
                    feedIds.Where(fid => !default(Guid).Equals(fid))
                        .ForEach(fid =>
                            {
                                var score = weight * ub.Value;
                                scoredFeeds.AddTo(fid, score);
                            });
                });

            return scoredFeeds.OrderByDescending(x => x.Value).Take(feedCount).ToDictionary(x => x.Key, x => x.Value);
        }

        protected virtual short GetWeight(UserAction action)
        {
            short weight = 0;

            switch (action)
            {
                case UserAction.PubClick:
                    weight = 1;
                    break;
                case UserAction.FeedClick:
                    weight = 2;
                    break;
                case UserAction.ItemClick:
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
            }

            return weight;
        }

        protected virtual Guid GetPubId(UserAction action, string entityId)
        {
            Guid pubId = default(Guid);

            switch (action)
            {
                case UserAction.PubClick:
                    pubId = new Guid(entityId);
                    break;
                case UserAction.FeedClick:
                case UserAction.ItemClick:
                case UserAction.ItemEmail:
                case UserAction.ItemStore:
                case UserAction.ItemLink:
                case UserAction.ItemShare:
                    pubId = GetPubIdFromStore(entityId);
                    break;
            }

            return pubId;
        }

        protected virtual IList<Guid> GetFeedIds(UserAction action, string entityId)
        {
            IList<Guid> feedIds = new List<Guid>();

            switch (action)
            { 
                case UserAction.PubClick:
                    feedIds = GetFeedIdsFromPub(entityId);
                    break;
                case UserAction.FeedClick:
                case UserAction.ItemClick:
                case UserAction.ItemEmail:
                case UserAction.ItemStore:
                case UserAction.ItemLink:
                case UserAction.ItemShare:
                    feedIds.Add(new Guid(entityId));
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

        protected IList<Guid> GetFeedIdsFromPub(string pubId)
        {
            var pubResult = _feedManager.GetSubscribedPublisher(new Guid(pubId));
            if (pubResult.HasError) return null;

            return pubResult.Target.FeedIds;
        }
    }
}
