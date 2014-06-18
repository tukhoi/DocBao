using DocBao.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.Helper
{
    public static class AppResultExtensions
    {
        public static string ErrorMessage<T>(this AppResult<T> appResult)
        {
            var message = string.Empty;
            switch (appResult.Error)
            { 
                case ErrorCode.None:
                    message = string.Empty;
                    break;
                case ErrorCode.FeedPassedInNull:
                    message = "không tìm thấy chuyên mục";
                    break;
                case ErrorCode.FeedAlreadySubscribed:
                    message = "đã đăng ký chuyên mục này";
                    break;
                case ErrorCode.FeedNotSubscribed:
                    message = "chưa đăng ký chuyên mục này";
                    break;
                case ErrorCode.FeedNotFound:
                    message = "không tìm thấy chuyên mục";
                    break;
                case ErrorCode.PublisherNotFound:
                    message = "không tìm thấy báo";
                    break;
                case ErrorCode.ItemNotFound:
                    message = "không tìm thấy tin";
                    break;
                case ErrorCode.ItemAlreadyRead:
                    message = "tin này đã đọc";
                    break;
                case ErrorCode.ItemAlreadyUnread:
                    message = "tin này chưa đọc";
                    break;
                case ErrorCode.LicenseRequired:
                    var subscribedFeedResult = FeedManager.GetInstance().AllSubscribedFeedCount();
                    message = "bản miễn phí không hỗ trợ lưu tin và giới hạn 90 mục. Vui lòng mua bản trả tiền ở mục giới thiệu...";
                    break;
                case ErrorCode.UnknownError:
                    message = "có lỗi xảy ra";
                    break;
                default:
                    message = "có lỗi xảy ra";
                    break;
            }

            return message;
        }
    }
}
