using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Practices.Prism.Events;
using Ninject;
using OpenSyno.Common;
using OpenSyno.Helpers;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using System.Text;
    using System.Threading;
    using System.Windows.Threading;

    using Microsoft.Phone.Tasks;

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
            IAudioStationSession audioStationSession;
            if (PhoneApplicationService.Current.State.ContainsKey("IAudioStationSession"))
            {
                audioStationSession = (IAudioStationSession)PhoneApplicationService.Current.State["IAudioStationSession"];
            }
            else
            {
                audioStationSession = new AudioStationSession { Host = _openSynoSettings.Host, Port = _openSynoSettings.Port, Token = _openSynoSettings.Token };
            }

            IoC.Container.Bind<IAudioStationSession>().ToConstant(audioStationSession).InSingletonScope();

            // Retrieve the type SearchService from a config file, so we can change it.
            // also possible: MockSearchService;
            // IoC.Container.Bind<ISearchService>().To(typeof(MockSearchService)).InSingletonScope();
            IoC.Container.Bind<ISearchService>().To(typeof(SearchService)).InSingletonScope();
            IoC.Container.Bind<ISignInService>().To(typeof(SignInService)).InSingletonScope();

            IoC.Container.Bind<IPageSwitchingService>().To<PageSwitchingService>().InSingletonScope();
            IoC.Container.Bind<INavigatorService>().To<NavigatorService>().InSingletonScope();
            IoC.Container.Bind<ISearchAllResultsViewModelFactory>().To<SearchAllResultsViewModelFactory>().InSingletonScope();
            IoC.Container.Bind<SearchViewModel>().ToSelf().InSingletonScope();
            IoC.Container.Bind<SearchResultsViewModelFactory>().ToSelf().InSingletonScope();
            IoC.Container.Bind<ArtistPanoramaViewModelFactory>().ToSelf().InSingletonScope();
            IoC.Container.Bind<PlayQueueViewModel>().ToSelf().InSingletonScope();
            IoC.Container.Bind<SearchResultsViewModel>().ToSelf().InSingletonScope();
            IoC.Container.Bind<ILogService>().To<IsolatedStorageLogService>().InSingletonScope();
            IoC.Container.Bind<ISearchResultItemViewModelFactory>().To<SearchResultItemViewModelFactory>().InSingletonScope();
            IoC.Container.Bind<IUrlParameterToObjectsPlateHeater>().To<UrlParameterToObjectsPlateHeater>().InSingletonScope();
            IoC.Container.Bind<IAudioTrackFactory>().To<AudioTrackFactory>().InSingletonScope();
            
            _notificationService = new NotificationService();
            IoC.Container.Bind<INotificationService>().ToConstant(_notificationService).InSingletonScope();
            
            IoC.Container.Bind<IPlaybackService>().To<PlaybackService>().InSingletonScope();
            //IoC.Container.Bind<IAudioRenderingService>().To<PlaybackService>().InSingletonScope();

            ActivateEagerTypes();

        }

        private void ActivateEagerTypes()
        {
            IoC.Container.Get<SearchResultsViewModel>();
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
            var audioStationSssion = IoC.Container.Get<IAudioStationSession>();
            PhoneApplicationService.Current.State["IAudioStationSession"] = audioStationSssion;
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
            //this.HandleException(e.Exception, e.Handled);

          //  e.Handled = true;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            ILogService logService = IoC.Container.Get<ILogService>();
        
            logService.Error(e.ExceptionObject.GetType().FullName);
            logService.Error(e.ExceptionObject.Message);

            e.Handled = HandleException(e.ExceptionObject, e.Handled);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        private bool HandleException(Exception e, bool handled)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            try
            {
                throw e;
            }
            catch (SynoNetworkException exception)
            {
                this._notificationService.Error(exception.Message, "Network error");
                handled = true;
            }
            catch (SynoLoginException exception)
            {
                this._notificationService.Error(exception.Message, "Login error");
                handled = true;
            }
            catch (SynoSearchException exception)
            {
                this._notificationService.Error(exception.Message, "Search error");
                handled = true;
            }
            finally
            {
                if (handled == false)
                {
                    System.Threading.ManualResetEvent mre = new ManualResetEvent(false);

                    Action errorFeedback = () =>
                                                      {
                                                          var helpDebug =
                       MessageBox.Show(
                           "Open syno encountered an error. The app will have to close, but you can help us to fix it for the next release by sending us anonymous information. Would you like to do so ?",
                           "Ooops !",
                           MessageBoxButton.OKCancel);
                                                          string exceptionName = e.GetType().Name;
                                                          if (helpDebug == MessageBoxResult.OK)
                                                          {
                                                              var mailContent = new StringBuilder();
                                                              while (e != null)
                                                              {
                                                                  mailContent.AppendFormat("Exception name : {0}\r\n", e.GetType().Name);
                                                                  mailContent.AppendFormat("Exception Message : {0}\r\n", e.Message);
                                                                  mailContent.AppendFormat("Exception StackTrace : {0}\r\n\r\n", e.StackTrace);
                                                                  mailContent.AppendLine("Inner exception : \r\n");
                                                                  e = e.InnerException;
                                                              }

                                                              ILogService logService = IoC.Container.Get<ILogService>();
                                                              logService.Error(mailContent.ToString());

                                                              EmailComposeTask emailComposeTask = new EmailComposeTask();
                                                              emailComposeTask.To = "opensyno@seesharp.ch";
                                                              emailComposeTask.Body = mailContent.ToString();
                                                              emailComposeTask.Subject = "Open syno Unhandled exception - " + exceptionName;
                                                              emailComposeTask.Show();
                                                              mre.Set();
                                                          }
                                                      };

                            if (!Deployment.Current.Dispatcher.CheckAccess())
                            {
                                DispatcherOperation dop;
                                dop = Deployment.Current.Dispatcher.BeginInvoke(errorFeedback);
                                mre.WaitOne();
                            }
                            else
                            {
                                errorFeedback();
                            }
                    handled = true;

                }                
            }

            return handled;
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        private IOpenSynoSettings _openSynoSettings;

        private INotificationService _notificationService;

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

    public class SynoTokenReceivedAggregatedEvent
    {
        public string Token { get; set; }
    }
}