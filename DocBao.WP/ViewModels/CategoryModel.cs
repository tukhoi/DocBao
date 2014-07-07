using Davang.Parser.Dto;
using DocBao.ApplicationServices;
using DocBao.ApplicationServices.Bank;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using DocBao.WP.Helper;
using Davang.Utilities.Log;

namespace DocBao.WP.ViewModels
{
    public class CategoryModel : Category, INotifyPropertyChanged
    {
        FeedManager _feedManager = FeedManager.GetInstance();
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isLoading = false;
        public List<Item> Items { get; set; }
        private ObservableCollection<ItemViewModel> _itemViewModels { get; set; }

        public CategoryModel()
        {
            Items = new List<Item>();
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

        public string ReadStats
        {
            get
            {
                return CategoryHelper.GetReadStats(this.Id);
            }
        }

        public async Task<int> Initialize(Guid categoryId, bool refresh)
        {
            try
            {
                IsLoading = true;

                if (categoryId.Equals(default(Guid))) return 0;

                var category = _feedManager.GetCategory(categoryId);
                UpdateFromDomainModel(category);
                var shouldUpdate = CategoryHelper.ShouldUpdate(category) || refresh;
                var result = await _feedManager.GetItemsByCategory(categoryId, AppConfig.MaxItemStored, shouldUpdate);
                if (result.Value == null || result.Value.Count == 0) return 0;

                Items.Clear();
                result.Value.ForEach(i => Items.Add(new ItemViewModel(i)));
                
                IsLoading = false;
                return result.Key;
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

                if (_itemViewModels.Count >= pageNumber * AppConfig.ITEM_COUNT_PER_FEED) return;
                if (_itemViewModels.Count >= this.Items.Count) return;

                if (pageNumber == 1) _itemViewModels.Clear();

                int skip = (pageNumber - 1) * AppConfig.ITEM_COUNT_PER_FEED;
                var items = excludeReadItems ?
                    this.Items.Where(i => !i.Read).Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList()
                    : this.Items.Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList();
                items.ForEach(i => i.Title = _feedManager.GetFeed(i.FeedId).Target.Publisher.Name.Trim() + ": " + i.Title);
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

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void UpdateReadItems(bool excludeReadItems)
        {
            if (this.Items == null || this.Items.Count == 0) return;

            this.Items.ForEach(r =>
            {
                var itemViewModels = _itemViewModels.Where(i => i.Id.Equals(r.Id)).ToList();
                if (itemViewModels != null && itemViewModels.Count > 0)
                {
                    itemViewModels.ForEach(i =>
                    {
                        i.Read = r.Read;

                        if (excludeReadItems && i.Read)
                            _itemViewModels.Remove(i);
                    });
                }
            });
        }

        private void UpdateFromDomainModel(Category category)
        { 
            this.Id = category.Id;
            this.Name = category.Name;
        }
    }
}
