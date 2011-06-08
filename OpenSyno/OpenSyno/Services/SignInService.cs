using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Events;
using Ninject;
using Synology.AudioStationApi;

namespace OpenSyno.Services
{
    public class SignInService : ISignInService
    {
        private readonly IOpenSynoSettings _openSynoSettings;
        private readonly IEventAggregator _eventAggregator;
        public event EventHandler<SignInCompletedEventArgs> SignInCompleted;

        public SignInService(IOpenSynoSettings openSynoSettings, IEventAggregator eventAggregator)
        {
            _openSynoSettings = openSynoSettings;
            _eventAggregator = eventAggregator;
        }

        public void SignIn()
        {
            if (_openSynoSettings.UserName == null || _openSynoSettings.Password == null || _openSynoSettings.Host == null)
            {
                OnSignInCompleted(new SignInCompletedEventArgs { Token = string.Empty, IsBusy = false});
                _eventAggregator.GetEvent<CompositePresentationEvent<SynoTokenReceivedAggregatedEvent>>().Publish(new SynoTokenReceivedAggregatedEvent { Token = string.Empty });
            }
            else
            {
                // only if we are really going to try, not if no credentials are set.
                IsSigningIn = true;

                var audioStation = IoC.Container.Get<IAudioStationSession>();
                audioStation.LoginAsync(
                    _openSynoSettings.UserName,
                    _openSynoSettings.Password,
                    token =>
                    {
                        _openSynoSettings.Token = token;
                        OnSignInCompleted(new SignInCompletedEventArgs { Token = token, IsBusy =  false});
                        _eventAggregator.GetEvent<CompositePresentationEvent<SynoTokenReceivedAggregatedEvent>>().Publish(new SynoTokenReceivedAggregatedEvent { Token = token });
                    },
                   exception => { throw exception; });
            }
        }

        public bool IsSigningIn { get; set; }

        protected void OnSignInCompleted(SignInCompletedEventArgs eventArgs)
        {
            IsSigningIn = false;
            if (SignInCompleted != null)
            {
                SignInCompleted(this, eventArgs);
            }
        }
    }
}
