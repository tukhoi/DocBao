using DocBao.ApplicationServices;
using DocBao.ApplicationServices.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;

namespace DocBao.WP.Helper
{
    public class CategoryHelper
    {
        public static bool ShouldUpdate(Category category)
        {
            var latestUpdate = category.Feeds.Max(f => f.LastUpdatedTime);
            if (latestUpdate.AddHours(1) < DateTime.Now)
                return true;

            return false;
        }

        public static string GetUpdateStats(Guid categoryId)
        {
            var feedManager = FeedManager.Instance;

            var category = feedManager.GetCategory(categoryId);
            if (category == null) return string.Empty;

            string updateStats = string.Empty;
            if (category.Feeds.Max(f=>f.LastUpdatedTime).Equals(default(DateTime)))
                updateStats = "chưa cập nhật";
            else
                updateStats = "cập nhật " + category.Feeds.Max(f => f.LastUpdatedTime).ToString("dd/MM/yyyy hh:mm:ss tt");

            return updateStats;
        }

        /// <summary>
        /// This is a feed based app 
        /// so everything should be pulled from subscribed feed
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static string GetReadStats(Guid categoryId)
        {
            var feedManager = FeedManager.Instance;
            var category = feedManager.GetCategory(categoryId);

            if (category == null) return string.Empty;
            var readItemCount = 0;
            var itemCount = 0;
            category.Feeds.ForEach(f =>
                {
                    var feedResult = feedManager.GetSubscribedFeed(f.Id);
                    if (!feedResult.HasError)
                    {
                        itemCount += feedResult.Target.Items.Count;
                        readItemCount += feedResult.Target.Items.Count(i => i.Read);
                    }
                });

            string readStats = string.Empty;
            if (itemCount == 0) readStats = "chưa đọc";
            else if (readItemCount == 0 && itemCount > 0)
                readStats = string.Format("chưa đọc/{0} tin", itemCount);
            else
                readStats = string.Format("đã đọc {0}/{1} tin",
                    readItemCount,
                    itemCount);

            return readStats;
        }

        public static string GetStatsString(Guid categoryId)
        {
            var updateStats = GetUpdateStats(categoryId);
            var readStats = GetReadStats(categoryId);

            return updateStats + "\n" + readStats;
        }
    }
}
