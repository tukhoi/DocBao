
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices.UserBehavior
{
    public interface IScoring
    {
        IDictionary<KeyValuePair<UserAction, string>, int> UserBehaviors { get; set; }

        IDictionary<Guid, int> ScorePublishers(short pubCount = 10);
        IDictionary<Guid, int> ScoreFeeds(short feedCount = 50);
    }
}
