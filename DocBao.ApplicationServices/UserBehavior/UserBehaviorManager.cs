using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;

namespace DocBao.ApplicationServices.UserBehavior
{
    public class UserBehaviorManager
    {   
        private static Lazy<UserBehaviorManager> _lazyInstance = new Lazy<UserBehaviorManager>(() => new UserBehaviorManager());
        private IScoring _scorer;
        private IDictionary<KeyValuePair<UserAction, string>, int> _userBehavior;

        public static UserBehaviorManager Instance { get { return _lazyInstance.Value; } }

        public UserBehaviorManager(IScoring scorer = null)
        {
            _scorer = scorer ?? new BasicScoring();
            _userBehavior = new Dictionary<KeyValuePair<UserAction, string>, int>();
        }

        protected virtual void Clear()
        {
            _userBehavior.Clear();
        }

        public virtual void Load()
        {
            Clear();
            _userBehavior = AppConfig.GetPersistentConfig<IDictionary<KeyValuePair<UserAction, string>, int>>(ConfigKey.UserBehavior, _userBehavior);
        }

        public virtual void Save()
        {
            AppConfig.SetPersistentConfig(ConfigKey.UserBehavior, _userBehavior);
        }

        public virtual void Log(UserAction userAction, string entityId, short value = 1)
        {
            var key = new KeyValuePair<UserAction, string>(userAction, entityId);
            _userBehavior.AddTo(key, (int)value);
        }

        public IDictionary<Guid, int> ScorePublishers(short pubCount = 10)
        {
            try
            {
                _scorer.UserBehaviors = _userBehavior;
                return _scorer.ScorePublishers(pubCount);
            }
            catch (Exception)
            { }

            return new Dictionary<Guid, int>();
        }

        public IDictionary<Guid, int> ScoreFeeds(short feedCount = 50)
        {
            try
            {
                _scorer.UserBehaviors = _userBehavior;
                return _scorer.ScoreFeeds(feedCount);
            }
            catch (Exception)
            { }

            return new Dictionary<Guid, int>();
        }
    }

    public enum UserAction
    { 
        PubClick,
        FeedClick,
        ItemClick,
        ItemEmail,
        ItemShare,
        ItemLink,
        ItemStore
    }
}
