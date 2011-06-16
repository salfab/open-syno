namespace OpenSyno
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using Synology.AudioStationApi;

    public class SearchService : ISearchService
    {
        private readonly IAudioStationSession _audioStationSession;

        public SearchService(IAudioStationSession audioStationSession)
        {
            _audioStationSession = audioStationSession;            
        }

        public bool SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>, string> callback)
        {
            CheckIsSignedIn();
            this._audioStationSession.SearchAllMusic(pattern, o => callback(o, pattern), this.OnOperationReturnedWithError);
            return true;
        }

        private void CheckIsSignedIn()
        {
            if (!this._audioStationSession.IsSignedIn)
            {
                throw new SynoLoginException("Open Syno is not signed in. Please make sure the info provided in the credentials page is correct.", null);
            }            
        }

        public bool SearchArtists(string pattern, Action<IEnumerable<SynoItem>> callback)
        {
            this.CheckIsSignedIn();
            _audioStationSession.SearchArtist(pattern, callback, OnOperationReturnedWithError);
            return true;
           
        }

        public void GetAllArtists(Action<IEnumerable<SynoItem>> callback)
        {
            throw new NotImplementedException();
        }

        public void GetAlbumsForArtist(SynoItem artist, Action<IEnumerable<SynoItem>, long, SynoItem> callback)
        {
            // an artist id looks like that (item_id): musiclib_music_aa/852502
            _audioStationSession.GetAlbumsForArtist(artist, callback, OnOperationReturnedWithError);
        }

        public void GetTracksForAlbum(SynoItem album,  Action<IEnumerable<SynoTrack>, long, SynoItem> callback)
        {
            _audioStationSession.GetTracksForAlbum(album, callback, OnOperationReturnedWithError);
        }

        private void OnOperationReturnedWithError(Exception e)
        {
            throw new NotImplementedException("Error was thrown during the last operation.", e);
        }
    }
}
