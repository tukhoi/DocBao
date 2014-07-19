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
    public class StoredItemsViewModel
    {
        public ObservableCollection<ItemViewModel> PagedItemViewModels { get; set; }
        private bool _isLoading = false;
        FeedManager _feedManager = FeedManager.Instance;
        public IList<ItemViewModel> AllItemViewModels { get; private set; }

        public StoredItemsViewModel()
        {
            PagedItemViewModels = new ObservableCollection<ItemViewModel>();
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

        public string ReadStats
        {
            get
            {
                return string.Format("đã đọc {0}/{1}", AllItemViewModels.Count(i => i.Read).ToString(), AllItemViewModels.Count().ToString());
            }
        }

        public void Initialize()
        {
            AllItemViewModels.Clear();
            var savedResult = _feedManager.GetStoredItems();
            if (!savedResult.HasError)
                savedResult.Target.ForEach(i => AllItemViewModels.Add(new ItemViewModel(i)));
        }

        public void LoadPage(int pageNumber, bool excludeReadItems)
        {
            try
            {
                this.IsLoading = true;

                UpdateReadItems(excludeReadItems);

                if (PagedItemViewModels.Count >= pageNumber * AppConfig.ITEM_COUNT_PER_FEED)
                    return;

                if (pageNumber == 1) PagedItemViewModels.Clear();

                int skip = (pageNumber - 1) * AppConfig.ITEM_COUNT_PER_FEED;
                var itemPage = excludeReadItems ?
                    AllItemViewModels.Where(i => !i.Read).Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList()
                    : AllItemViewModels.Skip(skip).Take(AppConfig.ITEM_COUNT_PER_FEED).ToList();

                itemPage.ForEach(i => PagedItemViewModels.Add(i));
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
            if (AllItemViewModels == null || AllItemViewModels.Count == 0) return;

            AllItemViewModels.ForEach(r =>
            {
                var itemViewModel = PagedItemViewModels.FirstOrDefault(i => i.Id.Equals(r.Id));
                if (itemViewModel != null)
                {
                    itemViewModel.Read = r.Read;

                    if (excludeReadItems && itemViewModel.Read)
                        PagedItemViewModels.Remove(itemViewModel);
                }
            });
        }
    }
}
