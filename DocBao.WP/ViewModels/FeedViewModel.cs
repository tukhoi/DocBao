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

namespace DocBao.WP.ViewModels
{
    public class FeedViewModel : Feed, INotifyPropertyChanged
    {
        FeedManager _feedManager = FeedManager.GetInstance();
        private bool _isLoading = false;
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<ItemViewModel> _itemViewModels { get; set; }

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
            _itemViewModels = new ObservableCollection<ItemViewModel>();
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public ObservableCollection<ItemViewModel> ItemViewModels
        {
            get { return _itemViewModels; }
            private set
            {
                _itemViewModels = value;
                //NotifyPropertyChanged("ItemViewModels");
            }
        }

        public async Task<int> Initialize(Guid feedId, Guid publisherId, bool refresh)
        {
            try
            {
                int updated = 0;
                IsLoading = true;

                if (feedId.Equals(default(Guid)) || publisherId.Equals(default(Guid))) return 0;

                //var feedResult = _feedManager.GetSubscribedFeeds(publisherId);
                var feedResult = _feedManager.GetSubscribedFeed(feedId);
                if (feedResult.HasError || !feedResult.Target.Publisher.Id.Equals(publisherId)) return 0;

                //var feed = feedResult.Target.FirstOrDefault(f => f.Id.Equals(feedId));
                if (FeedHelper.ShouldUpdateItems(feedResult.Target) || refresh)
                    try
                    {
                        var updateResult = await _feedManager.UpdateItems(feedResult.Target, true);
                        updated = updateResult.HasError ? 0 : updateResult.Target;
                    }
                    catch (Exception ex) 
                    {
                        throw ex;
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

                if (_itemViewModels.Count >= pageNumber * AppConfig.ITEM_COUNT_PER_FEED)
                    return;

                if (pageNumber == 1) _itemViewModels.Clear();

                int skip = (pageNumber - 1) * AppConfig.ITEM_COUNT_PER_FEED;
                var items = excludeReadItems ?
                    this.Items.Where(i=>!i.Read).Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList()
                    : this.Items.Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList();
                items.ForEach(i => _itemViewModels.Add(new ItemViewModel(i)));
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
            if (this.Items == null || this.Items.Count == 0) return;

            this.Items.ForEach(r => {
                var itemViewModel = _itemViewModels.FirstOrDefault(i => i.Id.Equals(r.Id));
                if (itemViewModel != null)
                {
                    itemViewModel.Read = r.Read;

                    if (excludeReadItems && itemViewModel.Read)
                        _itemViewModels.Remove(itemViewModel);
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
            this.Items = feed.Items;
            //this.Items = feed.Items.OrderByDescending(i => i.PublishDate).ToList();
            //if (excludeReadItems) this.Items = this.Items.Where(i => !i.Read).ToList();
            this.Publisher = feed.Publisher;

            this.IsLoading = false;
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
