using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices.UserBehavior
{
    public interface IUserBehaviorStore
    {
        void Clear();
        void Load();
        void Save();

        void PublisherClick(Guid pubId, int value = 1);
        void FeedClick(Guid feedId, int value = 1);
        void ItemClick(Guid feedId, string itemId, int value = 1);

        IDictionary<Guid, int> ScorePublishers();
        IDictionary<Guid, int> ScoreFeeds();
        IDictionary<KeyValuePair<Guid, string>, int> ScoreItems();

        IList<Guid> GetFeedsToDownloadInBackground();
    }
}
