using SOMAWP8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.Helper
{
    internal class AdViewerManager
    {
        private static readonly Lazy<SomaAdViewer> _lazyInstance =
            new Lazy<SomaAdViewer>(() => new SomaAdViewer() 
                { 
                    PopupAd = true,
                    Pub = 923880017,
                    Adspace = 65836846,
                    AdInterval = 30000,
                    PopupAdDuration = 20000
                });

        public static SomaAdViewer AdViewer { get { return _lazyInstance.Value; } }

        private AdViewerManager()
        { }
    }
}
