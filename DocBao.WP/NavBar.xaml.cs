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
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace DocBao.WP
{
    public partial class NavBar : UserControl, IDisposable
    {
        public Action<Uri, string> Navigation;
        public Action NavigateHome;
        public delegate Task BindingPageDelegate(BindingData bindingData);
        public event BindingPageDelegate SelectedEvent;

        public static readonly DependencyProperty FirstLPKFullModeHeaderProperty =
            DependencyProperty.Register("FirstLPKFullModeHeader", typeof(string), typeof(NavBar), null);

        public static readonly DependencyProperty SecondLPKFullModeHeaderProperty =
            DependencyProperty.Register("SecondLPKFullModeHeader", typeof(string), typeof(NavBar), null);

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

        public void BindingNavBar(NavBarViewModel viewModel)
        {
            if (viewModel == null) throw new ApplicationException("ViewModel is null");

            LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;

            var showBoth = viewModel.FirstBrothers != null && viewModel.FirstBrothers.Count > 1 && viewModel.SecondBrothers != null && viewModel.SecondBrothers.Count > 1;
            BindBrothers(lpkFirstBrothers, txtFirstBrother, viewModel.FirstBrothers, showBoth);
            BindBrothers(lpkSecondBrothers, txtSecondBrother, viewModel.SecondBrothers, showBoth);

            imgSeparator2.Visibility = showBoth                
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            txtHome.Tap += txtHome_Tap;
            LayoutRoot.Visibility = System.Windows.Visibility.Visible;
        }

        void BindBrothers(ListPicker listPicker, TextBlock textBlock, ObservableCollection<IBrother> brothers, bool showBoth)
        {
            var visibility = brothers == null || brothers.Count < 2 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            listPicker.SelectionChanged -= ListPicker_SelectionChanged;
            listPicker.ItemsSource = brothers;
            //listPicker.Visibility = visibility;
            if (visibility == System.Windows.Visibility.Visible)
                listPicker.SelectedItem = listPicker.Items.Select(x => x as IBrother).Where(x => x.Selected).FirstOrDefault();
            //listPicker.Width = showBoth ? 190 : 430;
            listPicker.SelectionChanged += ListPicker_SelectionChanged;

            textBlock.Visibility = visibility;
            var brother = listPicker.SelectedItem as IBrother;
            if (brother != null)
                textBlock.Text = brother.Name;
        }

        void txtHome_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigateHome();
        }

        private async void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listPicker = sender as ListPicker;
            if (listPicker == null) return;

            if (e.RemovedItems == null || e.RemovedItems.Count == 0 || listPicker.SelectedIndex == -1)
                return;

            var brother = listPicker.SelectedItem as IBrother;
            if (brother == null) return;
            await ExecutePostAction(brother);
        }

        private async Task ExecutePostAction(IBrother brother)
        {
            switch(brother.PostAction)
            {
                case PostAction.Navigation:
                    if (Navigation != null)
                        Navigation(brother.NavigateUri, brother.Id);
                    break;
                case PostAction.Binding:
                    if (SelectedEvent != null)
                        await SelectedEvent(brother.BindingData);
                    break;
            }
        }

        private void txtFirstBrother_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            lpkFirstBrothers.Open();
        }

        private void txtSecondBrother_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            lpkSecondBrothers.Open();
        }
    }
}
