using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Practices.Prism.Events;
using Ninject;
using OpenSyno.Helpers;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    public partial class App : Application
    {
     
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }


            // Standard Silverlight initialization
            InitializeComponent();

            // App specific settings initialization
            InitializeSettings();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        private void InitializeSettings()
        {
            EventAggregator eventAggregator = new EventAggregator();
            IoC.Container.Bind<IEventAggregator>().ToConstant(eventAggregator);

            string synoSettings = "SynoSettings";
            _openSynoSettings = IsolatedStorageSettings.ApplicationSettings.Contains(synoSettings) ? (OpenSynoSettings)IsolatedStorageSettings.ApplicationSettings[synoSettings] : new OpenSynoSettings();
            // FIXME : Try to get rid of thses registrations and use DI instead.
            IoC.Container.Bind<IOpenSynoSettings>().ToConstant(_openSynoSettings);

            // FIXME : Load from config files instead of  hard-coded bindings.

            // Retrieve the type IAudioStationSession from a config file, so we can change it.
            // Also possible : RemoteFileMockAudioStationSession
            IoC.Container.Bind<IAudioStationSession>().To(typeof(AudioStationSession)).InSingletonScope();

            // Retrieve the type SearchService from a config file, so we can change it.
            // also possible: MockSearchService;
            IoC.Container.Bind<ISearchService>().To(typeof(SearchService)).InSingletonScope();
            IoC.Container.Bind<ISignInService>().To(typeof(SignInService)).InSingletonScope();

            IoC.Container.Bind<ISearchAllResultsViewModelFactory>().To<SearchAllResultsViewModelFactory>().InSingletonScope();
            IoC.Container.Bind<SearchViewModel>().ToSelf().InSingletonScope();
            IoC.Container.Bind<SearchResultsViewModelFactory>().ToSelf().InSingletonScope();
            IoC.Container.Bind<ArtistPanoramaViewModelFactory>().ToSelf().InSingletonScope();
            IoC.Container.Bind<PlayQueueViewModel>().ToSelf().InSingletonScope();

            IoC.Container.Bind<ILogService>().To<IsolatedStorageLogService>().InSingletonScope();
           
            // Retrieve the type PlaybackService from a config file, so we can change it.
            IoC.Container.Bind<IPlaybackService>().To(typeof(PlaybackService)).InSingletonScope();

            ActivateEagerTypes();

        }

        private void ActivateEagerTypes()
        {
            IoC.Container.Get<SearchResultsViewModelFactory>();
            IoC.Container.Get<ArtistPanoramaViewModelFactory>();
            IoC.Container.Get<ISearchAllResultsViewModelFactory>();
            IoC.Container.Get<PlayQueueViewModel>();
        }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            IoC.Container.Get<ISignInService>().SignIn();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // TODO Ensure that application state is restored appropriately
          
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }



        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject;
            while (ex != null)
            {
                Exception exception = ex;

                if (!Deployment.Current.Dispatcher.CheckAccess())
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                  {
                                                                      MessageBox.Show(exception.Message, exception.GetType().Name, MessageBoxButton.OK);
                                                                      MessageBox.Show(exception.StackTrace, exception.GetType().Name, MessageBoxButton.OK);
                                                                  });
                }
                else
                {
                    MessageBox.Show(exception.Message, exception.GetType().Name, MessageBoxButton.OK);
                    MessageBox.Show(exception.StackTrace, exception.GetType().Name, MessageBoxButton.OK);
                }

                ex = ex.InnerException;
            }

            Console.Error.WriteLine(e.ExceptionObject.GetType().Name);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        private IOpenSynoSettings _openSynoSettings;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
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

        #endregion
    }

    internal class SynoTokenReceivedAggregatedEvent
    {
        public string Token { get; set; }
    }
}