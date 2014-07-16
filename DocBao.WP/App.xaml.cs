using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DocBao.WP.Resources;
using DocBao.WP.ViewModels;
using DocBao.ApplicationServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Phone.Scheduler;
using DocBao.WP.Helper;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
using Davang.Utilities.Log;
using Davang.Utilities;
using DocBao.ApplicationServices.Background;
using Davang.WP.Utilities;
using DocBao.ApplicationServices.UserBehavior;
#else
using Windows.ApplicationModel.Store;
using Store = Windows.ApplicationModel.Store;
using Davang.Utilities.Log;
using Davang.WP.Utilities;
using DocBao.ApplicationServices.UserBehavior;
#endif

namespace DocBao.WP
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }
        public static Store.ListingInformation IAPListingInformation { get; private set; }
        PeriodicTask periodicTask = null;
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            InitializeApp();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = false;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            SetupMockIAP();
#if DEBUG
            MemoryDiagnostic.BeginRecording();
#endif
            //BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(Current), RootFrame, "API_KEY");
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            InitializeBackgroundUpdater();
            GA.LogStartSession();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
                InitializeApp();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SaveData();
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveData();
            GA.LogEndSession();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            SaveData();

            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }

            GA.LogException(e.Exception, true);
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            SaveData();

            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }

            GA.LogException(e.ExceptionObject, true);
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            var startUri = AppConfig.UseCustomView
                ? new Uri("/CustomViewPage.xaml", UriKind.Relative)
                : new Uri("/HubTilePage.xaml", UriKind.Relative);
            
            RootFrame.Navigate(startUri);
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        private void InitializeApp()
        {
            BasePage.Initialize(mainPage:AppConfig.UseCustomView ? "CustomViewPage.xaml" : "HubTilePage.xaml");

            GA.Initialize(AppConfig.ClientId.ToString(), AppConfig.GA_ID, AppConfig.GA_APP_NAME, AppConfig.GA_APP_VERSION);

            Messenger.Initialize(AppResources.ApplicationTitle, 
                "/Resources/message.png",
                "Images/background2.png",
                new SolidColorBrush(Colors.White));
        }

        private void InitializeBackgroundUpdater()
        {
            if (!AppConfig.AllowBackgroundUpdate)
                return;
            try
            {
                periodicTask = ScheduledActionService.Find(AppConfig.BACKGROUND_UPDATE_TASK_NAME) as PeriodicTask;
                if (periodicTask != null && !periodicTask.IsEnabled) //fuck this is disabed by user
                    return;

                if (periodicTask != null && periodicTask.IsEnabled)
                    RemoveAgent(AppConfig.BACKGROUND_UPDATE_TASK_NAME);
                periodicTask = new PeriodicTask(AppConfig.BACKGROUND_UPDATE_TASK_NAME);
                periodicTask.Description = "Tự động cập nhật tin tức mới...";
                ScheduledActionService.Add(periodicTask);
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
            }

#if(DEBUG)
            ScheduledActionService.LaunchForTest(AppConfig.BACKGROUND_UPDATE_TASK_NAME, TimeSpan.FromSeconds(30));
#endif
        }

        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception ex)
            {
                GA.LogException(ex);
            }
        }

        private void SetupMockIAP()
        {
#if DEBUG
            MockIAP.Init();
            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "vi-VN", "phiên bản trả tiền", "1", "app duyệt báo");

            ProductListing p = new ProductListing
            {
                Name = "phiên bản trả tiền",
                ImageUri = new Uri("/Resources/paid-version.png", UriKind.Relative),
                ProductId = AppConfig.PAID_VERSION,
                ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                Keywords = new string[] { "phiên bản trả tiền" },
                Description = "app duyệt báo - phiên bản trả tiền",
                FormattedPrice = "1.0",
                Tag = string.Empty
            };

            MockIAP.AddProductListing(AppConfig.PAID_VERSION, p);
#endif
        }

        private void SaveData()
        {
            FeedManager.Instance.Save();
            FeedManager.Instance.CreateFeedsToUpdate();
            UserBehaviorManager.Instance.Save();
        }
    }
}