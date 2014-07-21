using Davang.Parser.Dto;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DocBao.WP.Helper;
using Davang.Utilities.Log;
using Davang.Utilities.ApplicationServices;

namespace DocBao.WP.ViewModels
{
    public class FeedViewModel : BaseEntity<Guid>, IDisposable
    {
        FeedManager _feedManager = FeedManager.Instance;
        private bool _isLoading = false;
        private ObservableCollection<ItemViewModel> _pagedItemViewModel { get; set; }

        //public Guid Id { get; set; }
        public string Name { get; set; }
        public PublisherViewModel Publisher { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string Link { get; set; }

        public IList<ItemViewModel> AllItemViewModels { get; set; }

        #region View properties

        public string LastUpdatedTimeString {
            get {
                return FeedHelper.GetUpdateStats(this.Id);
            }
        }

        public string ReadStats {
            get 
            {
                return FeedHelper.GetReadStats(this.Id);
            }
        }

        public SolidColorBrush ForegroundColor
        {
            get
            {
                return new SolidColorBrush(Colors.White);
            }
        }

        #endregion

        public FeedViewModel()
        {
            _pagedItemViewModel = new ObservableCollection<ItemViewModel>();
            AllItemViewModels = new List<ItemViewModel>();
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
            }
        }

        public ObservableCollection<ItemViewModel> PagedItemViewModels
        {
            get { return _pagedItemViewModel; }
            private set
            {
                _pagedItemViewModel = value;
            }
        }

        public async Task<int> RefreshLatestData(Guid feedId, Guid publisherId, bool refresh)
        {
            try
            {
                int updated = 0;
                IsLoading = true;

                if (feedId.Equals(default(Guid)) || publisherId.Equals(default(Guid))) return 0;

                var feedResult = _feedManager.GetSubscribedFeed(feedId);
                if (feedResult.HasError || !feedResult.Target.Publisher.Id.Equals(publisherId)) return 0;

                if (FeedHelper.ShouldUpdateItems(feedResult.Target) || refresh)
                    try
                    {
                        var updateResult = await _feedManager.UpdateItems(feedResult.Target, true);
                        updated = updateResult.HasError ? 0 : updateResult.Target;
                    }
                    catch (Exception ex) 
                    {
                        updated = -1;
                        GA.LogException(ex);
                    }

                UpdateFromDomainModel(feedResult.Target);
                IsLoading = false;
                return updated;
            }
            catch (Exception ex)
            {
                IsLoading = false;
                throw ex;
            }
        }

        public void LoadPage(int pageNumber, bool excludeReadItems)
        {
            try
            {
                this.IsLoading = true;

                UpdateReadItems(excludeReadItems);

                if (_pagedItemViewModel.Count >= pageNumber * AppConfig.ITEM_COUNT_PER_FEED) return;
                if (_pagedItemViewModel.Count >= this.AllItemViewModels.Count) return; 

                if (pageNumber == 1) _pagedItemViewModel.Clear();

                int skip = (pageNumber - 1) * AppConfig.ITEM_COUNT_PER_FEED;
                var items = excludeReadItems ?
                    this.AllItemViewModels.Where(i=>!i.Read).Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList()
                    : this.AllItemViewModels.Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList();
                items.ForEach(i => _pagedItemViewModel.Add(i));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public void UpdateReadItems(bool excludeReadItems)
        {
            if (this.AllItemViewModels == null || this.AllItemViewModels.Count == 0) return;

            this.AllItemViewModels.ForEach(r => 
                {
                    var itemViewModel = _pagedItemViewModel.FirstOrDefault(i => i.Id.Equals(r.Id));
                    if (itemViewModel != null)
                    {
                        itemViewModel.Read = r.Read;

                        if (excludeReadItems && itemViewModel.Read)
                            _pagedItemViewModel.Remove(itemViewModel);
                    }
                });
        }

        public void UpdateFromDomainModel(Feed feed)
        {
            this.IsLoading = true;

            this.Id = feed.Id;
            this.Name = feed.Name;
            this.Title = feed.Title;
            this.Description = feed.Description;
            this.LastUpdatedTime = feed.LastUpdatedTime;
            this.Link = feed.Link;
            this.AllItemViewModels.Clear();
            feed.Items.ForEach(i => this.AllItemViewModels.Add(new ItemViewModel(i)));
            var pubViewModel = new PublisherViewModel();
            pubViewModel.UpdateFromDomainModel(feed.Publisher);
            this.Publisher = pubViewModel;
            this.IsLoading = false;
        }

        public void Dispose()
        {
            this.PagedItemViewModels = null;
            this.AllItemViewModels = null;
        }
    }
}