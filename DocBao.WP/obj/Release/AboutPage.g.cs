﻿#pragma checksum "G:\gitdev\DocBao\DocBao.WP\AboutPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E4C3A142C868FE81142199C3472EF636"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace DocBao.WP {
    
    
    public partial class AboutPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBlock txtAppName;
        
        internal System.Windows.Controls.Image firstNextIcon;
        
        internal System.Windows.Controls.TextBlock txtPageName;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.TextBlock abtVersion;
        
        internal System.Windows.Controls.Button btnRating;
        
        internal System.Windows.Controls.Button btnPro;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/DocBao.WP;component/AboutPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.txtAppName = ((System.Windows.Controls.TextBlock)(this.FindName("txtAppName")));
            this.firstNextIcon = ((System.Windows.Controls.Image)(this.FindName("firstNextIcon")));
            this.txtPageName = ((System.Windows.Controls.TextBlock)(this.FindName("txtPageName")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.abtVersion = ((System.Windows.Controls.TextBlock)(this.FindName("abtVersion")));
            this.btnRating = ((System.Windows.Controls.Button)(this.FindName("btnRating")));
            this.btnPro = ((System.Windows.Controls.Button)(this.FindName("btnPro")));
        }
    }
}

