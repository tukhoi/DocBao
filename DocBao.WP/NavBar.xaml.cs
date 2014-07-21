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
        bool bound = false;
        public Action<Uri> Navigation;
        public Action NavigateHome;
        public static readonly DependencyProperty SecondLPKVisibilityProperty =
            DependencyProperty.Register("SecondLPKVisibility", typeof(Visibility), typeof(NavBar), null);

        public Visibility SecondLPKVisibility
        {
            get { return (System.Windows.Visibility)GetValue(SecondLPKVisibilityProperty); }
            set { SetValue(SecondLPKVisibilityProperty, value); }
        }

        public NavBar()
        {
            InitializeComponent();
            this.DataContext = this;
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
            if (bound) return;
            if (viewModel == null) throw new ApplicationException("ViewModel is null");

            lpkFirstBrothers.ItemsSource = viewModel.FirstBrothers;
            lpkSecondBrothers.ItemsSource = viewModel.SecondBrothers;

            if (viewModel.FirstBrothers.Count == 1) lpkFirstBrothers.IsHitTestVisible = false;
            if (viewModel.SecondBrothers.Count == 1) lpkSecondBrothers.IsHitTestVisible = false;

            lpkFirstBrothers.SelectedItem = lpkFirstBrothers.Items.Select(x => x as IBrother).Where(x => x.Selected).FirstOrDefault();
            lpkSecondBrothers.SelectedItem = lpkSecondBrothers.Items.Select(x => x as IBrother).Where(x => x.Selected).FirstOrDefault();

            
            txtHome.Tap += txtHome_Tap;
            lpkFirstBrothers.SelectionChanged += lpkBrotherPublishers_SelectionChanged;
            lpkSecondBrothers.SelectionChanged += lpkBrotherFeeds_SelectionChanged;
            bound = true;
        }

        void txtHome_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigateHome();
        }

        void SetLPKWidth(ListPicker lpk, double textBlockWidth)
        {
            lpk.Width = textBlockWidth + 20 > 190 ? 190 : textBlockWidth + 20;
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

        protected void ActionOnChildControls<T>(DependencyObject obj, DependencyProperty propName, string propValue, Action<DependencyObject> action)
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is T && child.GetValue(propName).Equals(propValue))
                    action(child);
                else
                    ActionOnChildControls<T>(child, propName, propValue, action);
            }
        }
    }
}
