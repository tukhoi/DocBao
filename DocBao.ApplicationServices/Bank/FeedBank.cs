using Davang.Parser.Dto;
using Davang.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices.Bank
{
    public sealed class FeedBank
    {
        private static IList<Feed> _feeds;
        private static IList<Publisher> _publishers;

        private static bool _initialized = false;

        static FeedBank()
        {
            _feeds = new List<Feed>();
            _publishers = new List<Publisher>();
        }

        public static IList<Feed> Feeds { 
            get {
                if (!_initialized)
                    Intialize();

                return _feeds.Where(f => f.Enabled).ToList(); 
            } 
        }
        public static IList<Publisher> Publishers { 
            get 
            {
                if (!_initialized)
                    Intialize();

                return _publishers.Where(p => p.Enabled).ToList(); 
            } 
        }

        private static void Intialize()
        {
            InitializePublishers();
            InitializeFeeds();

            _initialized = true;
        }

        private static void InitializeFeeds()
        {
            using (var stream = new FileStream(AppConfig.FEED_BANK_FILE_NAME, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    int order = 1;
                    while (!reader.EndOfStream)
                    {
                        var feedData = reader.ReadLine().Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                        
                        if (feedData.Length != 6) continue;
                        var feed = new Feed()
                        {
                            Id = Guid.Parse(feedData[0].Trim()),
                            Name = feedData[2].Trim(),
                            Link = feedData[3].Trim(),
                            Enabled = feedData[4].Trim().Equals("1") ? true : false,
                            Default = feedData[5].Trim().Equals("1") ? true : false,
                            Order = order++
                        };

                        var publisher = _publishers.FirstOrDefault(p => p.Id.Equals(Guid.Parse(feedData[1].Trim())));
                        if (publisher != null && publisher.Enabled)
                        {
                            publisher.AddFeedId(feed.Id);
                            feed.Publisher = publisher;
                            _feeds.Add(feed);
                        }
                    }
                }
            }
        }

        private static void InitializePublishers()
        {
            try
            {
                using (var stream = new FileStream(AppConfig.PUBLISHER_BANK_FILE_NAME, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        int order = 1;
                        while (!reader.EndOfStream)
                        {
                            var publisherData = reader.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (publisherData.Length != 6) break;
                            var publisher = new Publisher()
                            {
                                Id = Guid.Parse(publisherData[0].Trim()),
                                Name = publisherData[1].Trim(),
                                Link = publisherData[2].Trim(),
                                ImageUri = new Uri(publisherData[3].Trim(), UriKind.RelativeOrAbsolute),
                                Enabled = publisherData[4].Trim().Equals("1") ? true : false,
                                Default = publisherData[5].Trim().Equals("1") ? true : false,
                                Order = order++
                            };

                            _publishers.Add(publisher);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
