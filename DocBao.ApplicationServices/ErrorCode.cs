using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices
{
    public enum ErrorCode
    {
        None,
        FeedPassedInNull,
        FeedAlreadySubscribed,
        FeedNotSubscribed,
        FeedNotFound,
        PublisherNotFound,
        ItemNotFound,
        ItemAlreadyRead,
        ItemAlreadyUnread,
        LicenseRequired,
        CouldNotLoadStoredItem,
        NoStoredItemFound,
        ItemAlreadyStored,
        UnknownError
    }
}
