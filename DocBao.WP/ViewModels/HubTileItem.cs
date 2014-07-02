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
