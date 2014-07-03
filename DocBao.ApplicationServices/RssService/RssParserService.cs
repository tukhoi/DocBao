using Davang.Parser;
using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using Davang.Utilities.Log;

namespace DocBao.ApplicationServices.RssService
{
    public class RssParserService : RssParser
    {
        private static RssParserService _instance;
        
        public static RssParserService GetInstance()
        {
            if (_instance == null)
                _instance = new RssParserService();

            return _instance;
        }

        #region Public

        public async Task<int> UpdateItemsAsync(Feed feed)
        {
            try
            {
                var updatedFeed = await GetFeedResultAsync(feed.Link.ToString());
                if (updatedFeed == null || updatedFeed.Items.Count == 0) return 0;
                updatedFeed.Publisher.Id = feed.Publisher.Id;
                TailorFeed(updatedFeed);
                return UpdateFeedItems(feed, updatedFeed.Items);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("feedUrl: " + feed.Link, ex);
            }
        }

        public async Task<IList<Item>> BackgroundUpdateItemsAsync(string feedUrl, Guid publisherId)
        {
            try
            {
                var updatedFeed = await GetFeedResultAsync(feedUrl);
                if (updatedFeed == null || updatedFeed.Items.Count == 0) return null;
                updatedFeed.Publisher.Id = publisherId;
                TailorFeed(updatedFeed);
                return updatedFeed.Items;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("feedUrl: " + feedUrl, ex);
            }
        }

        #endregion

        #region Static

        internal static int UpdateFeedItems(Feed feed, IList<Item> items)
        {
            int updated = 0;
            items.OrderBy(i=>i.PublishDate).ToList().ForEach(item => 
            {
                var loadedItem = feed.Items.FirstOrDefault(i => i.Id.Equals(item.Id));
                if (loadedItem == null)
                {
                    if (feed.AddItem(item))
                        updated++;
                }
                else
                {
                    loadedItem.Title = item.Title;
                    loadedItem.Summary = item.Summary;
                    loadedItem.PublishDate = item.PublishDate;
                    loadedItem.Link = item.Link;
                }
            });

            return updated;
        }

        #endregion

        #region Private

        private async Task<Feed> GetFeedResultAsync(string baseUrl)
        {
            _baseUrl = baseUrl;
            bool firstSuccess = true;
            try
            {
                return await GetFeedAsync(string.Empty);
            }
            catch (System.Xml.XmlException)
            {
                firstSuccess = false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (!firstSuccess)
            {
                try
                {
                    return await GetFeedAsync(string.Empty);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return null;
        }

        private void TailorFeed(Feed updatedFeed)
        {
            if (updatedFeed.Publisher.Id.Equals(new Guid("7b420408-13bf-4340-81f7-a6430c538686")))
                TailorTuoiTreFeed(updatedFeed);
            if (updatedFeed.Publisher.Id.Equals(new Guid("ae68f542-48d6-4622-9419-1ee4461babf4")))
                TailorVNNFeed(updatedFeed);
            if (updatedFeed.Publisher.Id.Equals(new Guid("ddc17557-3e9c-4937-98fa-bc3c0e514bcd")))
                TailorDanTriFeed(updatedFeed);
        }

        /// <summary>
        /// Their image links are invalid so just remove them out
        /// </summary>
        /// <param name="updatedFeed"></param>
        private void TailorTuoiTreFeed(Feed updatedFeed)
        {
            updatedFeed.Items.ForEach(i => 
            {
                var summary = new StringBuilder(i.Summary.Substring(0, i.Summary.LastIndexOf("<img")));
                summary.Replace("<head>", string.Empty);
                summary.Replace("</head>", string.Empty);
                summary.Replace("\n", string.Empty);
                summary.Replace("&lt;EM&gt;", string.Empty);
                summary.Replace("&lt;/EM&gt;", string.Empty);
                summary.Replace(" style=\"float:right;\"", string.Empty);
                i.Summary = summary.ToString().Trim();
            });
        }

        /// <summary>
        /// Their links are invalid, correct them
        /// </summary>
        /// <param name="updatedFeed"></param>
        private void TailorVNNFeed(Feed updatedFeed)
        {
            updatedFeed.Items.ForEach(i =>
            {
                if (i.Link.StartsWith("http://vietnamnet.vnhttp"))
                    i.Link = i.Link.Replace("http://vietnamnet.vnhttp", "http");
            });
        }

        /// <summary>
        /// Fix links
        /// </summary>
        /// <param name="updatedFeed"></param>
        private void TailorDanTriFeed(Feed updatedFeed)
        {
            updatedFeed.Items.ForEach(i =>
            {
                if (i.Link.StartsWith("http://dantri.com.vn/http://"))
                    i.Link = i.Link.Replace("http://dantri.com.vn/http://", "http://");
            });
        }

        #endregion
    }
}
