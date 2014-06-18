using Davang.Parser.Dto;
using DocBao.ApplicationServices;
using DocBao.WP.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.ViewModels
{
    public class HubTileItem
    {
        public Uri ImageUri { get; set; }
        public string Title { get; set; }

        public string Notification {
            get
            {
                return PublisherHelper.GetStatsString(this.PublisherId);

                //var totalFeedCount = _feedManager.FeedCount(this.PublisherId).Target;
                //var subscribedFeedCount = _feedManager.SubscribedFeedCount(this.PublisherId).Target;
                //var totalItems = _feedManager.ItemCount(this.PublisherId).Target;
                //var readItems = _feedManager.ReadCount(this.PublisherId).Target;

                //string readStats = string.Empty;
                //if (readItems == 0 && totalItems > 0)
                //    readStats = string.Format("chưa đọc/{0} tin", totalItems);
                //else
                //    if (readItems == 0 && totalItems == 0)
                //        readStats = "chưa đọc";
                //    else
                //        readStats = string.Format("đọc {0}/{1} tin", readItems, totalItems);


                //string followStats = string.Empty;
                //if (subscribedFeedCount == 0)
                //    followStats = "chưa theo dõi";
                //else
                //    followStats = string.Format("{0}/{1} mục", subscribedFeedCount, totalFeedCount);

                //return followStats + "\n" + readStats;
            }
        }
        public bool DisplayNotification { get; set; }

        public string Message { get; set; } 

        public string GroupTag { get; set; }
        public Guid PublisherId { get; set; }

        public HubTileItem ConvertFromPublisherModel(Publisher publisher)
        {
            if (publisher == null) return null;

            this.Title = publisher.Name;
            this.DisplayNotification = true;
            this.Message = publisher.Link;
            this.ImageUri = publisher.ImageUri;
            this.PublisherId = publisher.Id;
            this.GroupTag = "Publishers";

            return this;
        }
    }
}
