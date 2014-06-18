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

namespace DocBao.WP.ViewModels
{
    public class PublisherViewModel : Publisher, INotifyPropertyChanged
    {
        FeedManager _feedManager = FeedManager.GetInstance();
        private bool _isLoading = false;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<FeedViewModel> FeedViewModels { get; set; }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public PublisherViewModel()
        {
            FeedViewModels = new ObservableCollection<FeedViewModel>();
        }

        public void Initialize(Guid publisherId)
        {
            try
            {
                this.IsLoading = true;

                var publisherResult = _feedManager.GetSubscribedPublisher(publisherId);
                if (publisherResult.HasError) return;

                var publisher = publisherResult.Target;
                UpdateFromDomainModel(publisher);
                //var feedResult = _feedManager.GetSubscribedFeeds(publisher.Id);
                //if (feedResult.HasError) return;
                //this.FeedIds.ForEach(fid =>
                //{
                //    var feedResult = _feedManager.GetSubscribedFeed(fid);
                //    if (feedResult.HasError) return;
                //    var feedViewModel = new FeedViewModel();
                //    feedViewModel.UpdateFromDomainModel(feedResult.Target);
                //    if (feedViewModel == null) return;
                //    FeedViewModels.Add(feedViewModel);
                //});
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

        public void LoadPage(int pageNumber)
        {
            try
            {
                this.IsLoading = true;

                UpdateStats();

                if (FeedViewModels.Count >= pageNumber * AppConfig.FEED_COUNT_PER_PUBLISHER)
                    return;

                if (pageNumber == 1) FeedViewModels.Clear();

                int skip = (pageNumber - 1) * AppConfig.ITEM_COUNT_PER_FEED;
                var feedIds = this.FeedIds.Skip(skip).Take(AppConfig.FEED_COUNT_PER_PUBLISHER).ToList();
                feedIds.ForEach(fid => 
                {
                    var feedResult = _feedManager.GetSubscribedFeed(fid);
                    if (feedResult.HasError) return;
                    var feedViewModel = new FeedViewModel();
                    feedViewModel.UpdateFromDomainModel(feedResult.Target);
                    if (feedViewModel == null) return;
                    FeedViewModels.Add(feedViewModel);
                });
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

        //public void LoadAllPage()
        //{
        //    FeedViewModels.Clear();

        //    this.FeedIds.ForEach(fid =>
        //    {
        //        var feedResult = _feedManager.GetSubscribedFeed(fid);
        //        if (feedResult.HasError) return;
        //        var feedViewModel = new FeedViewModel();
        //        feedViewModel.UpdateFromDomainModel(feedResult.Target);
        //        if (feedViewModel == null) return;
        //        FeedViewModels.Add(feedViewModel);
        //    });
        //}

        private void UpdateFromDomainModel(Publisher publisher)
        {
            this.Name = publisher.Name;
            this.Id = publisher.Id;
            this.Link = publisher.Link;
            this.ImageUri = publisher.ImageUri;
            this.FeedIds = publisher.FeedIds;
        }

        private void UpdateStats()
        {
            this.FeedViewModels.ForEach(f => { 
                var feedResult = _feedManager.GetSubscribedFeed(f.Id);
                if (!feedResult.HasError)
                    f.UpdateFromDomainModel(feedResult.Target);
            });
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
