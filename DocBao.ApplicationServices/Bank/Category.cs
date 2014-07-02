using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices.Bank
{
    public class Category : BaseEntity<Guid>
    {
        public string Name { get; set; }
        public IList<Feed> Feeds { get; set; }
        public Uri ImageUri { get; set; }

        public Category()
        {
            Feeds = new List<Feed>();
        }

        public Category CloneWithoutFeeds()
        {
            var category = new Category()
            {
                Id = this.Id,
                Name = this.Name,
                ImageUri = this.ImageUri
            };

            return category;
        }
    }
}
