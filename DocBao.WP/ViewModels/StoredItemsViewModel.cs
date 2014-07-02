using Davang.Parser.Dto;
using DocBao.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;

namespace DocBao.WP.ViewModels
{
    public class StoredItemsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ItemViewModel> ItemViewModels { get; set; }
        private bool _isLoading = false;
        FeedManager _feedManager = FeedManager.GetInstance();
        public IList<Item> Items { get; private set; }

        public StoredItemsViewModel()
        {
            ItemViewModels = new ObservableCollection<ItemViewModel>();
            Initialize();
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

        public string ReadStats
        {
            get
            {
                return string.Format("đã đọc {0}/{1}", Items.Count(i => i.Read).ToString(), Items.Count().ToString());

            }
        }

        public void Initialize()
        {
            var savedResult = _feedManager.GetStoredItems();
            if (!savedResult.HasError)
                Items = savedResult.Target;
        }

        public void LoadPage(int pageNumber, bool excludeReadItems)
        {
            try
            {
                this.IsLoading = true;

                UpdateReadItems(excludeReadItems);

                if (ItemViewModels.Count >= pageNumber * AppConfig.ITEM_COUNT_PER_FEED)
                    return;

                if (pageNumber == 1) ItemViewModels.Clear();

                int skip = (pageNumber - 1) * AppConfig.ITEM_COUNT_PER_FEED;
                var itemPage = excludeReadItems ?
                    Items.Where(i => !i.Read).Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList()
                    : Items.Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList();

                itemPage.ForEach(i => ItemViewModels.Add(new ItemViewModel(i)));
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
            if (Items == null || Items.Count == 0) return;

            Items.ForEach(r =>
            {
                var itemViewModel = ItemViewModels.FirstOrDefault(i => i.Id.Equals(r.Id));
                if (itemViewModel != null)
                {
                    itemViewModel.Read = r.Read;

                    if (excludeReadItems && itemViewModel.Read)
                        ItemViewModels.Remove(itemViewModel);
                }
            });
        }
    }
}
