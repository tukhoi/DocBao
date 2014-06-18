using Davang.Parser.Dto;
using DocBao.ApplicationServices;
using DocBao.WP.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DocBao.WP.ViewModels
{
    public class PublisherPickupViewModel
    {
        FeedManager _feedManager = FeedManager.GetInstance();

        public ObservableCollection<PublisherBankViewModel> PublisherBankViewModels { get; set; }

        public PublisherPickupViewModel()
        {
            var publisherResult = _feedManager.GetAllPublishers();
            if (publisherResult.HasError) return;

            var publishers = publisherResult.Target;

            var models = new List<PublisherBankViewModel>();
            publishers.ForEach(p =>
            {
                var model = new PublisherBankViewModel()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Link = p.Link,
                    ImageUri = p.ImageUri,
                    Order = p.Order
                };
                models.Add(model);
            });
            PublisherBankViewModels = new ObservableCollection<PublisherBankViewModel>();
            models.OrderBy(f => f.Order).ToList().ForEach(m => PublisherBankViewModels.Add(m));
        }
    }

    public class PublisherBankViewModel : Publisher
    {
        FeedManager _feedManager = FeedManager.GetInstance();

        public bool Subscribed
        {
            get
            {
                var subscribedResult = _feedManager.GetSubscribedPublisher(this.Id);
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

        public string PublisherStats
        {
            get
            {
                return PublisherHelper.GetStatsString(this.Id);
            }
        }
    }
}
