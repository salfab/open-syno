namespace OpenSyno
{
    using System;
    using System.IO.IsolatedStorage;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;
    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public class LoginViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private IAudioStationSession _audioStationSession;
        private readonly IOpenSynoSettings _synoSettings;

        private IPageSwitchingService _pageSwitchingService;

        public LoginViewModel(IPageSwitchingService pageSwitchingService, IEventAggregator eventAggregator, IAudioStationSession audioStationSession, IOpenSynoSettings synoSettings)
        {
            if (pageSwitchingService == null) throw new ArgumentNullException("pageSwitchingService");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (audioStationSession == null) throw new ArgumentNullException("audioStationSession");
            SignInCommand = new DelegateCommand(OnSignIn);
            _pageSwitchingService = pageSwitchingService;
            _eventAggregator = eventAggregator;
            _audioStationSession = audioStationSession;
            _synoSettings = synoSettings;
            UserName = _synoSettings.UserName;
            Password = _synoSettings.Password;
            Host = _synoSettings.Host;
            Port = _synoSettings.Port;
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }
        
        public int Port { get; set; }

        private void OnSignIn()
        {
            // Store it in SynoSettings and serialize SynoSettings in the Isolated Storage.
            _synoSettings.UserName = UserName;
            _synoSettings.Password = Password;
            _synoSettings.Host = Host;
            _synoSettings.Port = Port;
            _audioStationSession.LoginAsync(UserName, Password, OnLoginAsyncCompleted, OnLoginAsyncException);
        }

        private void OnLoginAsyncException(Exception exception)
        {
            throw exception;
        }

        private void OnLoginAsyncCompleted(string token)
        {
            _synoSettings.Token = token;

            // if it worked : let's save the credentials.
            IsolatedStorageSettings.ApplicationSettings["SynoSettings"] = _synoSettings;
            IsolatedStorageSettings.ApplicationSettings.Save();

            _pageSwitchingService.NavigateToPreviousPage();
        }

        public ICommand SignInCommand { get; set; }
    }
}