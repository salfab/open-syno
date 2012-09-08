using System.Collections.Generic;
using System.Net;

namespace OpenSyno
{
    using System;
    using System.IO.IsolatedStorage;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.Events;

    using OpemSyno.Contracts;

    using OpenSyno.Services;
    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public class LoginViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private IAudioStationSession _audioStationSession;
        private readonly IOpenSynoSettings _synoSettings;

        private IPageSwitchingService _pageSwitchingService;
        private ISignInService _signInService;
        private readonly IPlaybackService _playbackService;

        public LoginViewModel(IPageSwitchingService pageSwitchingService, IEventAggregator eventAggregator, IAudioStationSession audioStationSession, IOpenSynoSettings synoSettings, ISignInService signInService, IPlaybackService playbackService)
        {
            if (pageSwitchingService == null) throw new ArgumentNullException("pageSwitchingService");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (audioStationSession == null) throw new ArgumentNullException("audioStationSession");
            SignInCommand = new DelegateCommand(OnSignIn);
            _pageSwitchingService = pageSwitchingService;
            _eventAggregator = eventAggregator;
            _audioStationSession = audioStationSession;
            _synoSettings = synoSettings;
            _signInService = signInService;
            _signInService.SignInCompleted += OnSignInCompleted;

            // Unregister the registered events to make sure we don't execute the event handler twice in case of exceptions
            _signInService.CheckTokenValidityCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    _signInService.CheckTokenValidityCompleted -= OnCheckTokenValidityCompleted;
                }
            };

            _playbackService = playbackService;
            UserName = _synoSettings.UserName;
            UseSsl = _synoSettings.UseSsl;
            Password = _synoSettings.Password;
            Host = _synoSettings.Host;
            Port = _synoSettings.Port;
        }

        private void OnCheckTokenValidityCompleted(object sender, CheckTokenValidityCompletedEventArgs e)
        {
            _signInService.CheckTokenValidityCompleted -= OnCheckTokenValidityCompleted;
            if (e.Error != null)
            {
                return;
            }
            // current token is still valid.
            if (e.IsValid)
            {
                this.OnLoginAsyncCompleted(e.Token);
            }
            else
            {
                // need to get a new token.

                // we need to check if the settings are okay too because this takes place before the check performed in the OnSignIn command handler.
                var formatValidity = _synoSettings.GetCredentialFormatValidationStatus();

                if (formatValidity != CredentialFormatValidationStatus.Valid)
                {
                    _signInService.ShowCredentialErrorMessage(formatValidity);
                    return;
                }

                _audioStationSession.Host = Host;
                _audioStationSession.Port = Port;

                try
                {
                    _audioStationSession.LoginAsync(this.UserName, this.Password, this.OnLoginAsyncCompleted,
                                                    this.OnLoginAsyncException, this._synoSettings.UseSsl);
                }
                catch (ArgumentNullException exception)
                {
                    // FIXME : Use noification service instead
                    MessageBox.Show(
                        "The connection settings don't look valid, please make sure they are entered correctly.",
                        "Credentials not valid",
                        MessageBoxButton.OK);
                }
                catch (UriFormatException exception)
                {
                    // FIXME : Use noification service instead
                    MessageBox.Show(
                        "The format of the provided hostname is not valid. Check that it is not prefixed it with http:// or https://",
                        "The host name is not valid",
                        MessageBoxButton.OK);
                }
            }
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }
        
        public int Port { get; set; }

        public List<Type> RendererType { get; set; }

        private void OnSignIn()
        {
            _synoSettings.UserName = UserName;
            _synoSettings.Password = Password;
            _synoSettings.UseSsl = UseSsl;
            // FIXME : detect or ask version
            _synoSettings.DsmVersion = DsmVersions.V4_0;
            _synoSettings.Host = Host;
            _synoSettings.Port = Port;

            // NOTE - is it still needed here, since we do a check cached token validity async just after ?
            var formatValidity = _synoSettings.GetCredentialFormatValidationStatus();

            if (formatValidity != CredentialFormatValidationStatus.Valid)
            {
                _signInService.ShowCredentialErrorMessage(formatValidity);               
                return;
            }

            // FIXME : we should use the SignIn method of the signInService, so we don't duplicate the logic !
            _signInService.CheckTokenValidityCompleted += OnCheckTokenValidityCompleted;


            // result of the following call is returned asynchronously as an event in which the actual log-in will take place if needed.

            _signInService.CheckCachedTokenValidityAsync();                    


        }

        private void OnSignInCompleted(object s, SignInCompletedEventArgs e)
        {
            OnLoginAsyncCompleted(e.Token);

            // if we cannot 
            // _playbackService.InvalidateCachedTokens();
            return;
        }

        public bool UseSsl { get; set; }

        private void OnLoginAsyncException(Exception exception)
        {
            throw exception;
        }

        private void OnLoginAsyncCompleted(string token)
        {
            _synoSettings.Token = token;

            _playbackService.InvalidateCachedTokens();


            // if it worked : let's save the credentials.
            IsolatedStorageSettings.ApplicationSettings["SynoSettings"] = _synoSettings;
            IsolatedStorageSettings.ApplicationSettings.Save();

            _pageSwitchingService.NavigateToPreviousPage();
        }

        public ICommand SignInCommand { get; set; }
    }
}