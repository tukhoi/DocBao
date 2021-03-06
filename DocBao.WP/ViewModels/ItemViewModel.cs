﻿
using Davang.Parser.Dto;
using Davang.Utilities.ApplicationServices;
using DocBao.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DocBao.WP.ViewModels
{
    public class ItemViewModel : BaseEntity<string>
    {
        //public string Id { get; set; }
        public Guid FeedId { get; set; }
        public bool Read { get; set; }

        public DateTime PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }

        public string MarkReadTitle {
            get
            {
                return this.Read ? "đánh dấu chưa đọc" : "đánh dấu đã đọc";    
            }
        }

        public SolidColorBrush ForegroundColor { 
            get 
            {
                //return this.Read ? new SolidColorBrush(Colors.Gray) : App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
                return this.Read ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Black);
            }
        }

        public string PublishDateString {
            get {
                return "cập nhật: " + this.PublishDate.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
        }

        public Visibility SummaryVisibility
        {
            get;
            set;
        }

        public ItemViewModel(Item item)
        {
            this.Id = item.Id;
            this.FeedId = item.FeedId;
            this.Read = item.Read;
            this.PublishDate = item.PublishDate;
            this.Title = item.Title;
            this.Summary = item.Summary;
            this.Link = item.Link;
        }

        public Item ToItem()
        {
            var item = new Item()
            {
                Id = this.Id,
                FeedId = this.FeedId,
                Read = this.Read,
                PublishDate = this.PublishDate,
                Title = this.Title,
                Summary = this.Summary,
                Link = this.Link
            };

            return item;
        }
    }
}
