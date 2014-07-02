using DocBao.ApplicationServices.Bank;
using DocBao.WP.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.ViewModels
{
    public class CustomHubTileItem
    {
        public Uri ImageUri { get; set; }
        public string Title { get; set; }

        public string Notification
        {
            get
            {
                return CategoryHelper.GetStatsString(this.CategoryId);
            }
        }
        public bool DisplayNotification { get; set; }

        public string Message { get; set; }

        public string GroupTag { get; set; }
        public Guid CategoryId { get; set; }

        public CustomHubTileItem ConvertFromCustomCategory(Category category)
        {
            if (category == null) return null;

            this.Title = category.Name;
            this.DisplayNotification = true;
            this.ImageUri = category.ImageUri;
            this.CategoryId = category.Id;
            this.GroupTag = "Categories";

            return this;
        }
    }
}
