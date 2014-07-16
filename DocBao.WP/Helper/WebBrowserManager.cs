using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.WP.Helper
{
    internal class WebBrowserManager
    {
        private static readonly Lazy<WebBrowser> _instance = new Lazy<WebBrowser>
            (() => new WebBrowser());

        public static WebBrowser WebBrowser { get { return _instance.Value; } }

        private WebBrowserManager()
        { }
    }
}
