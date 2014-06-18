using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices
{
    public interface IFeedManager
    {
        #region Get

        AppResult<List<Publisher>> GetAllPublishers();
        AppResult<List<Publisher>> GetSubscribedPublishers();
        AppResult<Publisher> GetPublisher(Guid publisherId);
        AppResult<Publisher> GetSubscribedPublisher(Guid publisherId);
        AppResult<List<Feed>> GetAllFeeds(Guid publisherId);
        AppResult<List<Feed>> GetSubscribedFeeds(Guid publisherId = default(Guid));
        AppResult<Feed> GetFeed(Guid feed);
        AppResult<Feed> GetSubscribedFeed(Guid feedId);
        Task<AppResult<int>> UpdateItems(Feed feed, bool refresh = false);

        AppResult<int> ReadCount(Guid publisherId);
        AppResult<int> ItemCount(Guid publisherId);
        AppResult<int> FeedCount(Guid publisherId);
        AppResult<int> SubscribedFeedCount(Guid publisherId);

        int AllSubscribedFeedCount();

        #endregion

        #region Set

        AppResult<bool> MarkItemAsRead(Guid feedId, string itemId, bool read);

        void SetLastId<TId>(string id);
        string GetLastId<TId>();
        
        #endregion
        
        #region Subscribe/Unsubscribe

        Task<AppResult<bool>> SubscribeFeed(Guid feedId);
        Task<AppResult<bool>> UnsubscribeFeed(Guid feedId);
        Task<AppResult<bool>> SubscribePublisher(Guid publisherId);
        Task<AppResult<bool>> UnsubscribePublisher(Guid publisherId);

        #endregion

<<<<<<< HEAD
        #region Stored Items

        Task<AppResult<bool>> StoreItemAsync(Item item);
        AppResult<Item> GetStoredItem(string itemId);
        AppResult<List<Item>> GetStoredItems();
        bool IsStored(string itemId);
        Task<AppResult<bool>> DeleteStoredItemAsync(string itemId);
        Task<AppResult<bool>> ClearStoredItemsAsync();

        AppResult<bool> MarkStoredItemAsRead(string itemId, bool read);

        #endregion

=======
>>>>>>> parent of db4037a... Stored item feature
        //#region Reading

        //void SetReading<T, TId>(T readingOne) where T : BaseEntity<TId>;
        //T GetReading<T, TId>(IList<T> lookupList) where T : BaseEntity<TId>;

        //#endregion
    }
}
