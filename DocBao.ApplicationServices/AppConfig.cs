using Davang.Parser.Dto;
using Davang.Utilities.Helpers;
using Davang.Utilities.Helpers.Serialization;
using DocBao.ApplicationServices.Background;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices
{
    public class AppConfig
    {
        public static SerializationType DEFAULT_SERIALIZATION_TYPE = SerializationType.JsonSerialization;

        public static string SUBSCRIBED_FEED_FILE_NAME = "subscribedFeeds.dat";
        public static string STORED_ITEM_FILE_NAME = "storedFeeds.dat";
        public static string FEED_BANK_FILE_NAME = @"Data\Feeds.txt";
        public static string PUBLISHER_BANK_FILE_NAME = @"Data\Publishers.txt";

        public static string APP_NAME = "duyệt báo";

        public static int ITEM_COUNT_PER_FEED = 10;
        public static int FEED_COUNT_PER_PUBLISHER = 10;
        public static int ITEM_COUNT_BEFORE_NEXT_LOADING = 1;

        public static string PAID_VERSION = "paid_version";
        public static int UN_PAID_MAX_SUBSCRIBED_FEED_ALLOW = 90;

        public static string BACKGROUND_UPDATE_TASK_NAME = "DocBaoUpdater";

        public static string GA_ID = "UA-52115271-1";
        public static string GA_APP_NAME = "duyetbao";
        public static string GA_APP_VERSION = "1.6.1";

        public static IDictionary<string, short> MaxItemStoredList;
        public static IDictionary<string, short> FeedCountPerBackgroundUpdateList;

        public static short MAX_FEEDS_TO_DOWNLOAD_IN_BACKGROUND = 50;
        public static string TEMP_DOWNLOAD_FILE_PATTERN = "temp-download";
        public static short MAX_NEW_FEED_UPDATED_SHOW = 10;
        public static short MAX_FILE_DOWNLOAD_ALLOW = 72;

        private static IDictionary<ConfigKey, object> _memConfigs;

        static AppConfig()
        {
            _memConfigs = new Dictionary<ConfigKey, object>();
            InitializeConfigList();
        }

        #region Settings

        public static Guid ClientId
        {
            get
            {
                var clientId = GetConfig<Guid>(ConfigKey.ClientId, default(Guid));
                if (default(Guid).Equals(clientId))
                {
                    clientId = Guid.NewGuid();
                    SetConfig<Guid>(ConfigKey.ClientId, clientId);
                }

                return clientId;
            }
        }

        public static bool Backup
        {
            get
            {
                return GetConfig<bool>(ConfigKey.Backup, true);
            }
            set
            {
                SetConfig<bool>(ConfigKey.Backup, value);
            }
        }

        public static bool ShowTitleOnly
        {
            get
            {
                return GetConfig<bool>(ConfigKey.ShowTitleOnly, false);
            }
            set
            {
                SetConfig<bool>(ConfigKey.ShowTitleOnly, value);
            }
        }

        public static bool ShowUnreadItemOnly
        {
            get
            {
                return GetConfig<bool>(ConfigKey.ShowUnreadItemOnly, false);
            }
            set
            {
                SetConfig<bool>(ConfigKey.ShowUnreadItemOnly, value);
            }
        }

        public static short MaxItemStored
        {
            get
            {
                return GetConfig<short>(ConfigKey.MaxItemStored, 200);
            }
            set
            {
                SetConfig<short>(ConfigKey.MaxItemStored, value);
            }
        }

        public static bool ShowItemTitle
        {
            get
            {
                return GetConfig<bool>(ConfigKey.ShowItemTitle, true);
            }
            set
            {
                SetConfig<bool>(ConfigKey.ShowItemTitle, value);
            }
        }

        public static bool AllowBackgroundUpdate
        {
            get
            {
                return GetConfig<bool>(ConfigKey.AllowBackgroundUpdate, true);
            }
            set
            {
                SetConfig<bool>(ConfigKey.AllowBackgroundUpdate, value);
            }
        }

        public static short FeedCountPerBackgroundUpdate
        {
            get
            {
                return GetConfig<short>(ConfigKey.FeedCountPerBackgroundUpdate, 5);
            }
            set
            {
                SetConfig<short>(ConfigKey.FeedCountPerBackgroundUpdate, value);
            }
        }

        public static bool ShowBackgroundUpdateResult
        {
            get
            {
                return GetConfig<bool>(ConfigKey.ShowBackgroundUpdateResult, false);
            }
            set
            {
                SetConfig<bool>(ConfigKey.ShowBackgroundUpdateResult, value);
            }
        }

        public static bool JustUpdateOverWifi
        {
            get
            {
                return GetConfig<bool>(ConfigKey.JustUpdateOverWifi, true);
            }
            set
            {
                SetConfig<bool>(ConfigKey.JustUpdateOverWifi, value);
            }
        }

        public static bool UseCustomView
        {
            get
            {
                return GetConfig<bool>(ConfigKey.UseCustomView, false);
            }
            set
            {
                SetConfig<bool>(ConfigKey.UseCustomView, value);
            }
        }

        public static UpdateVersion AppUpdate
        {
            get
            {
                return GetConfig<UpdateVersion>(ConfigKey.AppUpdate, UpdateVersion.NotSet);
            }
            set
            {
                SetConfig<UpdateVersion>(ConfigKey.AppUpdate, value);
            }
        }

        public static IList<FeedDownload> FeedDownloads
        {
            get
            {
                return GetConfig<IList<FeedDownload>>(ConfigKey.FeedDownloads, null);
            }
            set
            {
                SetConfig<IList<FeedDownload>>(ConfigKey.FeedDownloads, value);
            }
        }

        public static bool ShowAllPublisher
        {
            get
            {
                return GetConfig<bool>(ConfigKey.ShowAllPublisher, false);
            }
            set
            {
                SetConfig<bool>(ConfigKey.ShowAllPublisher, value);
            }
        }

        public static bool DisAllowBackgroundInMidNight
        {
            get
            {
                return GetConfig<bool>(ConfigKey.DisAllowBackgroundInMidNight, true);
            }
            set
            {
                SetConfig<bool>(ConfigKey.DisAllowBackgroundInMidNight, value);
            }
        }

        #endregion

        internal static T GetPersistentConfig<T>(ConfigKey key, T defaultValue)
        {
            object value = StorageHelper.LoadConfig(key.ToString());
            if (value != null)
                return (T)value;
            return defaultValue;
        }

        internal static void SetPersistentConfig<T>(ConfigKey key, T value)
        {
            StorageHelper.SaveConfig(key.ToString(), value);
        }

        #region private

        private static T GetConfig<T>(ConfigKey key, T defaultValue)
        {
            if (!_memConfigs.ContainsKey(key))
            {
                var persistentValue = GetPersistentConfig<T>(key, defaultValue);
                _memConfigs[key] = persistentValue;
            }

            return (T)_memConfigs[key];
        }

        private static void SetConfig<T>(ConfigKey key, T value)
        {
            if (!_memConfigs.ContainsKey(key))
                _memConfigs.Add(key, value);
            else
                _memConfigs[key] = value;
            SetPersistentConfig(key, value);
        }

        private static void InitializeConfigList()
        {
            MaxItemStoredList = new Dictionary<string, short>();
            FeedCountPerBackgroundUpdateList = new Dictionary<string, short>();

            MaxItemStoredList.Add("100 tin", 100);
            MaxItemStoredList.Add("200 tin", 200);
            MaxItemStoredList.Add("300 tin", 300);
            MaxItemStoredList.Add("400 tin", 400);

            FeedCountPerBackgroundUpdateList.Add("2 mục", 2);
            FeedCountPerBackgroundUpdateList.Add("5 mục", 5);
            FeedCountPerBackgroundUpdateList.Add("7 mục", 7);
        }

        #endregion
    }

    public enum ConfigKey
    { 
        ClientId,
        Backup,
        ShowTitleOnly,
        ShowUnreadItemOnly,
        MaxItemStored,
        ShowItemTitle,
        AllowBackgroundUpdate,
        FeedCountPerBackgroundUpdate,
        ShowBackgroundUpdateResult,
        JustUpdateOverWifi,
        UseCustomView,
        AppUpdate,
        FeedDownloads,
        ShowAllPublisher,
        DisAllowBackgroundInMidNight,
        UserBehavior
    }

    public enum UpdateVersion
    { 
        NotSet = 0,
        V1_4,
        V1_5,
        V1_6
    }
}
