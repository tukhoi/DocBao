using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.ViewModels
{
    public class NavBarViewModel
    {
        public ObservableCollection<IBrother> FirstBrothers { get; set; }        
        public ObservableCollection<IBrother> SecondBrothers { get; set; }
        
        public NavBarViewModel()
        {
            SecondBrothers = new ObservableCollection<IBrother>();
            FirstBrothers = new ObservableCollection<IBrother>();
        }
    }

    public interface IBrother
    {
        string Id { get; set; }
        Uri ImageUri { get; set; }
        string Name { get; set; }
        string Stats { get; set; }
        bool Selected { get; set; }
        Uri NavigateUri { get; set; }
        BindingData BindingData { get; set; }
    }

    public class Brother : IBrother
    {
        public string Id { get; set; }
        public Uri ImageUri { get; set; }
        public string Name { get; set; }
        public string Stats { get; set; }
        public bool Selected { get; set; }
        public Uri NavigateUri { get; set; }
        public BindingData BindingData { get; set; }
    }

    public class BindingData
    {
        public Guid PublisherId { get; set; }
        public Guid FeedId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
