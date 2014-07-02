using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices.Background
{
    public class FeedDownload
    {
        public Guid Id { get; set; }
        public Guid PublisherId { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string Link { get; set; }
    }
}
