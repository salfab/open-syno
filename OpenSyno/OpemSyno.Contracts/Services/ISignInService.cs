using System;

namespace OpenSyno.Services
{
    public interface ISignInService
    {
        event EventHandler<SignInCompletedEventArgs> SignInCompleted;
        bool IsSigningIn { get; set; }
        event EventHandler<CheckTokenValidityCompletedEventArgs> CheckTokenValidityCompleted;
        void SignIn();
        void CheckCachedTokenValidityAsync();
    }
}