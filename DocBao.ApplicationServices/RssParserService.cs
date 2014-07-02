using Davang.Parser;
using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;

namespace DocBao.ApplicationServices
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

        public async Task<Feed> GetFeedResultAsync(string baseUrl)
        {
            _baseUrl = baseUrl;
            return await GetFeedAsync(string.Empty);
        }

        public Feed GetFeedResult(string baseUrl)
        {
            throw new NotImplementedException();
            //_baseUrl = baseUrl;
            //return GetFeedAsync(string.Empty);
        }

        public async Task<int> UpdateItemsAsync(Feed feed)
        {
            try
            {
                var updatedFeed = await GetFeedResultAsync(feed.Link.ToString());
                if (updatedFeed == null || updatedFeed.Items.Count == 0) return 0;
                return UpdateFeedItems(feed, updatedFeed.Items);
            }
            catch (Exception ex)
            {
                return 0;
                //throw ex;
            }
        }

        public int UpdateItems(Feed feed)
        {
            try
            {
                var updatedFeed = GetFeedResult(feed.Link.ToString());
                if (updatedFeed == null || updatedFeed.Items.Count == 0) return 0;
                return UpdateFeedItems(feed, updatedFeed.Items);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int UpdateFeedItems(Feed feed, IList<Item> items)
        {
            int updated = 0;
            items.OrderBy(i=>i.PublishDate).ToList().ForEach(item => {
                var loadedItem = feed.Items.FirstOrDefault(i => i.Id.Equals(item.Id));
                if (loadedItem == null)
                {
                    feed.AddItem(item);
                    updated++;
                }
                else
                    loadedItem = item;
            });

            return updated;
        }
    }
}
