﻿using Davang.Parser.Dto;
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
    public class PublisherViewModel
    {
        FeedManager _feedManager = FeedManager.Instance;
        private bool _isLoading = false;
        public ObservableCollection<FeedViewModel> FeedViewModels { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public IList<Guid> FeedIds { get; set; }
        public Uri ImageUri { get; set; }
        //public int Order { get; set; }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
            }
        }

        public PublisherViewModel()
        {
            FeedViewModels = new ObservableCollection<FeedViewModel>();
            FeedIds = new List<Guid>();
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

                if (FeedViewModels.Count >= pageNumber * AppConfig.FEED_COUNT_PER_PUBLISHER) return;
                if (FeedViewModels.Count >= this.FeedIds.Count) return;

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

        public void UpdateFromDomainModel(Publisher publisher)
        {
            this.Name = publisher.Name;
            this.Id = publisher.Id;
            this.Link = publisher.Link;
            this.ImageUri = publisher.ImageUri;
            this.FeedIds = publisher.FeedIds;
        }

        private void UpdateStats()
        {
            this.FeedViewModels.ForEach(f => 
                { 
                    var feedResult = _feedManager.GetSubscribedFeed(f.Id);
                    if (!feedResult.HasError)
                        f.UpdateFromDomainModel(feedResult.Target);
                });
        }
    }
}
