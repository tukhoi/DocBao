using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DocBao.WP.ViewModels;
using DocBao.ApplicationServices;
using System.Windows.Media;

namespace DocBao.WP
{
    public partial class NavBar : UserControl, IDisposable
    {
        public Action<Uri> Navigation;
        public Action NavigateHome;

        public static readonly DependencyProperty FirstLPKVisibilityProperty =
            DependencyProperty.Register("FirstLPKVisibility", typeof(Visibility), typeof(NavBar), null);

        public static readonly DependencyProperty SecondLPKVisibilityProperty =
            DependencyProperty.Register("SecondLPKVisibility", typeof(Visibility), typeof(NavBar), null);

        public static readonly DependencyProperty FirstLPKFullModeHeaderProperty =
            DependencyProperty.Register("FirstLPKFullModeHeader", typeof(string), typeof(NavBar), null);

        public static readonly DependencyProperty SecondLPKFullModeHeaderProperty =
            DependencyProperty.Register("SecondLPKFullModeHeader", typeof(string), typeof(NavBar), null);

        public Visibility FirstLPKVisibility
        {
            get { return (System.Windows.Visibility)GetValue(FirstLPKVisibilityProperty); }
            set { SetValue(FirstLPKVisibilityProperty, value); }
        }

        public Visibility SecondLPKVisibility
        {
            get { return (System.Windows.Visibility)GetValue(SecondLPKVisibilityProperty); }
            set { SetValue(SecondLPKVisibilityProperty, value); }
        }

        public string FirstLPKFullModeHeader
        {
            get { return (string)GetValue(FirstLPKFullModeHeaderProperty); }
            set { SetValue(FirstLPKFullModeHeaderProperty, value); } 
        }

        public string SecondLPKFullModeHeader
        {
            get { return (string)GetValue(SecondLPKFullModeHeaderProperty); }
            set { SetValue(SecondLPKFullModeHeaderProperty, value); }
        }

        public NavBar()
        {
            InitializeComponent();
            this.DataContext = this;
            LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void Dispose()
        {
            lpkFirstBrothers.ItemsSource = null;
            lpkFirstBrothers.ItemTemplate = null;

            lpkSecondBrothers.ItemsSource = null;
            lpkSecondBrothers.ItemTemplate = null;
        }

        public void Binding(NavBarViewModel viewModel)
        {
            if (viewModel == null) throw new ApplicationException("ViewModel is null");
            lpkFirstBrothers.SelectionChanged -= lpkBrotherPublishers_SelectionChanged;
            lpkSecondBrothers.SelectionChanged -= lpkBrotherFeeds_SelectionChanged;
            

            lpkFirstBrothers.ItemsSource = viewModel.FirstBrothers;
            lpkSecondBrothers.ItemsSource = viewModel.SecondBrothers;

            //if (viewModel.FirstBrothers.Count == 1) lpkFirstBrothers.IsHitTestVisible = false;
            //if (viewModel.SecondBrothers.Count == 1) lpkSecondBrothers.IsHitTestVisible = false;

            if (viewModel.FirstBrothers.Count == 1) FirstLPKVisibility = System.Windows.Visibility.Collapsed;
            if (viewModel.SecondBrothers.Count == 1) SecondLPKVisibility = System.Windows.Visibility.Collapsed;

            lpkFirstBrothers.SelectedItem = lpkFirstBrothers.Items.Select(x => x as IBrother).Where(x => x.Selected).FirstOrDefault();
            lpkSecondBrothers.SelectedItem = lpkSecondBrothers.Items.Select(x => x as IBrother).Where(x => x.Selected).FirstOrDefault();

            txtHome.Tap += txtHome_Tap;
            lpkFirstBrothers.SelectionChanged += lpkBrotherPublishers_SelectionChanged;
            lpkSecondBrothers.SelectionChanged += lpkBrotherFeeds_SelectionChanged;

            LayoutRoot.Visibility = System.Windows.Visibility.Visible;
            imgSeparator2.Visibility =
                FirstLPKVisibility == System.Windows.Visibility.Visible && SecondLPKVisibility == System.Windows.Visibility.Visible
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            if (FirstLPKVisibility == System.Windows.Visibility.Collapsed)
            lpkSecondBrothers.Width = 430;
            
            if (SecondLPKVisibility == System.Windows.Visibility.Collapsed)
                lpkFirstBrothers.Width = 430;
        }

        void txtHome_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigateHome();
        }

        private void lpkBrotherFeeds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems == null || e.RemovedItems.Count == 0 || lpkSecondBrothers.SelectedIndex == -1)
                return;

            var brother = lpkSecondBrothers.SelectedItem as IBrother;
            if (brother == null) return;

            Navigation(brother.NavigateUri);

        }

        private void lpkBrotherPublishers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems == null || e.RemovedItems.Count == 0 || lpkFirstBrothers.SelectedIndex == -1)
                return;

            var brother = lpkFirstBrothers.SelectedItem as IBrother;
            if (brother == null) return;

            Navigation(brother.NavigateUri);
        }

        //protected void ActionOnChildControls<T>(DependencyObject obj, DependencyProperty propName, string propValue, Action<DependencyObject> action)
        //{
        //    var count = VisualTreeHelper.GetChildrenCount(obj);
        //    if (count == 0)
        //        return;

        //    for (int i = 0; i < count; i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(obj, i);

        //        if (child != null && child is T && child.GetValue(propName).Equals(propValue))
        //            action(child);
        //        else
        //            ActionOnChildControls<T>(child, propName, propValue, action);
        //    }
        //}
    }
}
