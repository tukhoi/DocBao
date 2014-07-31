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

        private UserBehaviorManager(IScoring scorer = null)
        {
            _scorer = scorer ?? new BasicScoring();
            _userBehavior = new Dictionary<KeyValuePair<UserAction, string>, int>();
            Load();
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
            _userBehavior.AppendValue(key, (int)value);
        }

        public IDictionary<Guid, int> ScorePublishers()
        {
            try
            {
                _scorer.UserBehaviors = _userBehavior;
                return _scorer.ScorePublishers();
            }
            catch (Exception ex)
            {
                int i = 1;
            }

            return new Dictionary<Guid, int>();
        }

        public IDictionary<Guid, int> ScoreFeeds()
        {
            try
            {
                _scorer.UserBehaviors = _userBehavior;
                return _scorer.ScoreFeeds();
            }
            catch (Exception ex)
            {
                int i = 1;
            }

            return new Dictionary<Guid, int>();
        }
    }

    public enum UserAction
    { 
        PubEnter,
        FeedEnter,
        ItemEnter,
        ItemEmail,
        ItemShare,
        ItemLink,
        ItemStore,
        CatEnter
    }
}
