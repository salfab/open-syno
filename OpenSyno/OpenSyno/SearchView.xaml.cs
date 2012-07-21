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
using Microsoft.Practices.Prism.Events;
using Ninject;
using Ninject.Activation;
using Ninject.Planning.Bindings;
using OpemSyno.Contracts.Services;
using OpenSyno.Helpers;
using OpenSyno.Services;
using OpenSyno.ViewModels;

namespace OpenSyno
{
    using System;

    using OpemSyno.Contracts;

    public class ViewNames
    {
        public const string SearchView = "SearchView";

        public const string StartupView = "StartupView";
    }

    public partial class SearchView
    {
        public SearchView()
        {
            Loaded += PageLoaded;
            InitializeComponent();
        }

        // HACK : If NavigationService in silverlight was the same as in WPF, we wouldn't have to put this logic in the view : we could have it in a view factory and have it more decoupled, unfortunately
        // when navigating in a SL / WP7 application there is no way for us to control the lifecycle of the page being navigated to.
        private void PageLoaded(object sender, RoutedEventArgs e)
        {

            // the page is an humble object, and the navigatorService, its sole dependency.
            var navigator = IoC.Container.Get<INavigatorService>();
            navigator.ActivateNavigationService(NavigationService, true);            
            
            // string viewName = ViewNames.SearchView;// "SearchView"; // GetType().FullName;
            //// Don't register if it has been already registered in the past.
            //if (IoC.Container.GetBindings(typeof(IPageSwitchingService)).Count(o => o.Metadata.Name == viewName) == 0)
            //{
            //    // register the type binding...
            //    IoC.Container.Bind<IPageSwitchingService>().ToConstant(new PageSwitchingService(NavigationService)).Named(viewName);                
            //}
        }

        /// <summary>
        /// TEMPORARY EVENT HANDLER JUST TO BE ABLE TO SET THE CREDENTIALS BEFORE WE PUT IT AT THE RIGHT PLACE IN A PROPER SETTINGS VIEW.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CredentialsApplicationBarButtonClicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoginView.xaml", UriKind.RelativeOrAbsolute));
        }

        private void AboutBoxApplicationBarButtonClicked(object sender, EventArgs e)
        {
            var viewModel = (SearchViewModel)DataContext;
            viewModel.ShowAboutBoxCommand.Execute(null);
        }

        private void PlayqueueApplicationBarButtonClicked(object sender, EventArgs e)
        {
            var viewModel = (SearchViewModel)DataContext;
            viewModel.ShowPlayQueueCommand.Execute(null);
        }
    }

    public interface INavigatorService
    {
        void ActivateNavigationService(NavigationService navigationService, bool deactivateAfterNavigation);

        IUrlParameterToObjectsPlateHeater UrlParameterToObjectsPlateHeater { get; }
    }

    public class NavigatorService : INavigatorService
    {
        private readonly IEventAggregator _eventAggregator;

        private readonly IUrlParameterToObjectsPlateHeater _urlParameterToObjectsPlateHeater;

        private readonly INotificationService _notificationService;

        private ILogService _logService;

        #region Implementation of INavigatorService

        public NavigatorService(IEventAggregator eventAggregator, IUrlParameterToObjectsPlateHeater urlParameterToObjectsPlateHeater, ILogService logService, INotificationService notificationService)
        {
            _eventAggregator = eventAggregator;
            this._notificationService = notificationService;
            this._logService = logService;
            _urlParameterToObjectsPlateHeater = urlParameterToObjectsPlateHeater;
        }

        public void ActivateNavigationService(NavigationService navigationService, bool deactivateAfterNavigation)
        {
            Action<PageSwitchedAggregatedEvent> callback = null;

            callback = ae =>
                           {
                               try
                               {
                                   this._logService.Trace(string.Format("About to navigate to: {0}. Use navigate service operation : {1}", ae.NavigationServiceOperation, ae.UseNavigationServiceOperation));
                                   
                                   this._logService.Trace(string.Format("navigationService: {0}.", navigationService));

                                   if (!ae.UseNavigationServiceOperation)
                                   {
                                       navigationService.Navigate(ae.Uri);
                                   }
                                   else
                                   {
                                       switch (ae.NavigationServiceOperation)
                                       {
                                           case PageSwitchedAggregatedEvent.NavigationServiceOperations.GoBack:
                                               navigationService.GoBack();
                                               break;
                                           case PageSwitchedAggregatedEvent.NavigationServiceOperations.GoForward:
                                               navigationService.GoForward();
                                               break;
                                           case PageSwitchedAggregatedEvent.NavigationServiceOperations.StopLoading:
                                               navigationService.StopLoading();
                                               break;
                                           default:
                                               throw new ArgumentOutOfRangeException();
                                       }
                                   }
                                   this._logService.Trace(string.Format("Call to navigationServie issued. Deactivate after navigation : {0}", deactivateAfterNavigation));

                                   if (deactivateAfterNavigation)
                                   {
                                       // callback will not be modified, therefore : no need to make a copy to avoid accessing a modified closure.
                                       _eventAggregator.GetEvent<CompositePresentationEvent<PageSwitchedAggregatedEvent>>().Unsubscribe(callback);
                                   }
                               }
                               catch (InvalidOperationException e)
                               {
                                    _notificationService.Warning("Open syno is already navigating to a page. Please wait until the current navigation is over before switching to an other page.", "Just a second...");
                               }

                           };

            _eventAggregator.GetEvent<CompositePresentationEvent<PageSwitchedAggregatedEvent>>().Subscribe(callback, true);
        }

        public IUrlParameterToObjectsPlateHeater UrlParameterToObjectsPlateHeater
        {
            get
            {
                return _urlParameterToObjectsPlateHeater;
            }            
        }

        #endregion
    }

    public class PageSwitchedAggregatedEvent
    {
        public Uri Uri { get; set; }
        public bool UseNavigationServiceOperation { get; set; }
        public NavigationServiceOperations NavigationServiceOperation { get; set; }

        public enum NavigationServiceOperations
        {
            GoBack,
            GoForward,
            StopLoading
        }
    }
}