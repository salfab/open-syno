using System;
using System.Net;
using Microsoft.Practices.Prism.Events;
using Newtonsoft.Json.Linq;
using Ninject;
using Synology.AudioStationApi;

namespace OpenSyno.Services
{
    using OpenSyno.Helpers;

    public class SignInService : ISignInService
    {
        private readonly IOpenSynoSettings _openSynoSettings;
        private readonly IEventAggregator _eventAggregator;
        private INotificationService _notificationService;

        private ILogService _logService;

        public event EventHandler<SignInCompletedEventArgs> SignInCompleted;

        public SignInService(IOpenSynoSettings openSynoSettings, IEventAggregator eventAggregator, INotificationService notificationService, ILogService logService)
        {
            _openSynoSettings = openSynoSettings;
            _eventAggregator = eventAggregator;
            _notificationService = notificationService;
            _logService = logService;
        }

        public event EventHandler<CheckTokenValidityCompletedEventArgs> CheckTokenValidityCompleted;

        public void SignIn()
        {
            if (_openSynoSettings.UserName == null || _openSynoSettings.Password == null || _openSynoSettings.Host == null)
            {
                _logService.Trace("SignInService : Signing in - Empty credential parameters - Aborted");
                OnSignInCompleted(new SignInCompletedEventArgs { Token = string.Empty, IsBusy = false });
            }
            else
            {
                _logService.Trace("SignInService : Signing in");
                // only if we are really going to try, not if no credentials are set.
                IsSigningIn = true;

                var audioStation = IoC.Container.Get<IAudioStationSession>();
                audioStation.LoginAsync(
                    this._openSynoSettings.UserName,
                    this._openSynoSettings.Password,
                    token =>
                        {
                            this._openSynoSettings.Token = token;
                            this.OnSignInCompleted(new SignInCompletedEventArgs { Token = token, IsBusy =  false});
                        },
                   exception => { throw exception; }, this._openSynoSettings.UseSsl);
            }
        }

        public void CheckCachedTokenValidityAsync()
        {
            // no cached token
            if (_openSynoSettings.Token == null)
            {
                if (CheckTokenValidityCompleted != null)
                {
                    CheckTokenValidityCompleted(this, new CheckTokenValidityCompletedEventArgs { IsValid = false, Token = null });
                }
                return;
            }
            string token = _openSynoSettings.Token;
            WebClient client = new WebClient();
            client.Headers["Cookie"] = token;
            // client.Headers["Accept-Encoding"] = "gzip, deflate";
            client.DownloadStringCompleted += (s, e) =>
                                                  {
                                                      JObject jobject = null;
                                                      try
                                                      {
                                                          jobject = JObject.Parse(e.Result);
                                                          var isValid = jobject["success"].Value<bool>();
                                                          _logService.Trace("CheckCachedTokenValidityAsync : token is valid : " + isValid.ToString());
                                                          if (CheckTokenValidityCompleted != null)
                                                          {
                                                              CheckTokenValidityCompleted(this, new CheckTokenValidityCompletedEventArgs { IsValid = isValid, Token = token, Error = null });
                                                          }
                                                      }
                                                      catch (WebException exception)
                                                      {
                                                          _notificationService.Error("Please check that the specified hostname for the Disk Station is correct.", "We can't connect to your Disk Station");
                                                          CheckTokenValidityCompleted(this, new CheckTokenValidityCompletedEventArgs { IsValid = false, Token = null, Error = exception });
                                                      }

                                                  };
                // we pass the client object along just so it doesn't get garbage collected before the eventhandler is called.
            string uriString = string.Format("http://{0}:{1}/webman/modules/AudioStation/webUI/audio.cgi?action=avoid_timeout", _openSynoSettings.Host, _openSynoSettings.Port);
            client.DownloadStringAsync(new Uri(uriString),client);
            
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
