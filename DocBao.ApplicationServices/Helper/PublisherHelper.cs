using DocBao.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.Helper
{
    public class PublisherHelper
    {
        public static string GetStatsString(Guid publisherId)
        {
            var feedManager = FeedManager.Instance;

            var totalFeedCount = feedManager.FeedCount(publisherId).Target;
            var subscribedFeedCount = feedManager.SubscribedFeedCount(publisherId).Target;
            var totalItems = feedManager.ItemCount(publisherId).Target;
            var readItems = feedManager.ReadCount(publisherId).Target;

            string readStats = string.Empty;
            if (readItems == 0 && totalItems > 0)
                readStats = string.Format("chưa đọc/{0} tin", totalItems);
            else
                if (readItems == 0 && totalItems == 0)
                    readStats = "chưa đọc";
                else
                    readStats = string.Format("đã đọc {0}/{1} tin", readItems, totalItems);


            string followStats = string.Empty;
            if (subscribedFeedCount == 0)
                followStats = "chưa cài chuyên mục";
            else
                followStats = string.Format("cài {0}/{1} chuyên mục", subscribedFeedCount, totalFeedCount);

            return followStats + "\n" + readStats;
        }

        public static string GetAllStatsString()
        {
            int allPubCount = 0;
            int subscribedPubCount = 0;

            var feedManager = FeedManager.Instance;
            var allPubResult = feedManager.GetAllPublishers();
            var subscribedPubResult = feedManager.GetSubscribedPublishers();

            if (!allPubResult.HasError)
                allPubCount = allPubResult.Target.Count;

            if (!subscribedPubResult.HasError)
                subscribedPubCount = subscribedPubResult.Target.Count;

            return string.Format("cài {0}/{1} báo", subscribedPubCount, allPubCount);

        }
    }
}
