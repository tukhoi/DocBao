using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Parser.Dto;
using DocBao.ApplicationServices;

namespace DocBao.WP.Helper
{
    public class FeedHelper
    {
        public static bool ShouldUpdateItems(Feed feed)
        {
            if (feed.Items.Count == 0) return true;
            var lastestUpdate = feed.Items.Max(i => i.PublishDate);
            if (lastestUpdate.AddHours(1) < DateTime.Now)
                return true;

            return false;
        }

        public static string GetUpdateStats(Guid feedId)
        {
            var feedManager = FeedManager.GetInstance();

            var feedResult = feedManager.GetSubscribedFeed(feedId);
            if (feedResult.HasError) return string.Empty;

            var feed = feedResult.Target;

            string updateStats = string.Empty;
            if (feed.LastUpdatedTime.Equals(default(DateTime)))
                updateStats = "chưa cập nhật";
            else
            updateStats = "cập nhật " + feed.LastUpdatedTime.ToString("dd/MM/yyyy hh:mm:ss");

            return updateStats;
        }

        public static string GetReadStats(Guid feedId)
        {
            var feedManager = FeedManager.GetInstance();

            var feedResult = feedManager.GetSubscribedFeed(feedId);
            if (feedResult.HasError) return string.Empty;

            var feed = feedResult.Target;

            string readStats = string.Empty;
            if (feed.Items.Count(i => i.Read) == 0 && feed.Items.Count() == 0) readStats = "chưa đọc";
            else if (feed.Items.Count(i => i.Read) == 0 && feed.Items.Count() > 0)
                readStats = string.Format("chưa đọc/{0} tin", feed.Items.Count());
            else
                readStats = string.Format("đã đọc {0}/{1} tin",
                    feed.Items.Count(i => i.Read),
                    feed.Items.Count);

            return readStats;
        }

        public static string GetStatsString(Guid feedId)
        {
            var updateStats = GetUpdateStats(feedId);
            var readStats = GetReadStats(feedId);

            return updateStats + "\n" + readStats;
        }
    }
}
