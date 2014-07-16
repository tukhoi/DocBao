using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Davang.Parser.Dto;
using Davang.Utilities.Extensions;
using DocBao.ApplicationServices;
using DocBao.WP.Helper;

namespace DocBao.WP.ViewModels
{
    public class FeedPickupViewModel
    {
        FeedManager _feedManager = FeedManager.Instance;

        public string Name { get; set; }
        public ObservableCollection<FeedBankViewModel> FeedBankViewModels { get; set; }

        public FeedPickupViewModel(Guid publisherId)
        {
            var publisherResult = _feedManager.GetPublisher(publisherId);
            if (publisherResult.HasError) return;

            var publisher = publisherResult.Target;
            this.Name = publisher.Name;

            if (publisher.FeedIds != null && publisher.FeedIds.Count > 0)
            {
                var models = new List<FeedBankViewModel>();
                publisher.FeedIds.ForEach(fid =>
                {
                    var feedResult = _feedManager.GetFeed(fid);
                    if (!feedResult.HasError)
                    {
                        var feed = new FeedBankViewModel()
                        {
                            Id = feedResult.Target.Id,
                            Name = feedResult.Target.Name,
                            Link = feedResult.Target.Link,
                            Description = feedResult.Target.Description,
                        };
                        models.Add(feed);
                    }
                });
                FeedBankViewModels = new ObservableCollection<FeedBankViewModel>();
                models.OrderBy(f => f.Order).ToList().ForEach(m => FeedBankViewModels.Add(m));
            }
        }
    }

    public class FeedBankViewModel : Feed
    {
        FeedManager _feedManager = FeedManager.Instance;

        public bool Subscribed
        {
            get
            {
                var subscribedResult = _feedManager.GetSubscribedFeed(this.Id);
                return !subscribedResult.HasError && subscribedResult.Target != null;
            }
        }

        public Uri SubscribedImageUri
        {
            get
            {
                return this.Subscribed ? new Uri("/Resources/check.png", UriKind.Relative) : null;
            }
        }

        public Visibility SubscribedImageVisibility
        {
            get
            {
                return this.Subscribed ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SolidColorBrush ForegroundColor
        {
            get
            {
                return this.Subscribed ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.DarkGray);
            }
        }

        public string FeedStats
        {
            get {
                return FeedHelper.GetStatsString(this.Id);
            }
        }
    }
}
