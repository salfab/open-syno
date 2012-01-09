using System;
using System.Runtime.Serialization;

namespace OpenSyno.Common
{
    [DataContract]
    public class AsciiUriFix
    {
        public AsciiUriFix(string res, string url) : this()
        {
            Res = res;
            Url = url;
        }

        public AsciiUriFix()
        {
            lockObjectForLoadingCompleteEvent = new object();            
        }

        [DataMember]
        public string Res { get; set; }

        private string _url;

        private object lockObjectForLoadingCompleteEvent;

        [DataMember]
        public string Url
        {
            get { return _url; }
            set
            {
                var oldUrl = _url;

                // Since the object is deserialized, the property setter might be called while the .ctor has not been called !!
                if (lockObjectForLoadingCompleteEvent == null)
                {
                    lockObjectForLoadingCompleteEvent = new object();
                }
                lock (lockObjectForLoadingCompleteEvent)
                {
                    _url = value;                    
                }
                if (oldUrl == null && LoadingComplete != null)
                {
                    LoadingComplete(this, EventArgs.Empty);
                }

            }
        }

        public event EventHandler LoadingComplete;

        public void CallbackWhenFixIsApplicable(Action<AsciiUriFix> callback)
        {
            // FIXME : Beware the race conditions !

            EventHandler OnLoadingComplete = null;
            OnLoadingComplete = (s, e) =>
                {
                    // unregister the event.
                    this.LoadingComplete -= OnLoadingComplete;
                    callback(this);
                };

            // here, we read the field, not the property, to avoid race conditions : since there is a lock block in the property as well,
            // the state of _url cannot change between its evaluation and the registration of the event.
            lock (lockObjectForLoadingCompleteEvent)
            {
                if (this._url != null)
                {
                    callback(this);
                    return;
                }
                this.LoadingComplete += OnLoadingComplete;
            }
        }
    }
}