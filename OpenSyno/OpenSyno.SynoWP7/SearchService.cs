﻿namespace OpenSyno
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

        public void SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>> callback)
        {
            _audioStationSession.SearchAllMusic(pattern, callback, OnOperationReturnedWithError);
        }

        public void SearchArtists(string pattern, Action<IEnumerable<SynoItem>> callback)
        {
            if (_audioStationSession.IsSignedIn)
            {
                _audioStationSession.SearchArtist(pattern, callback, OnOperationReturnedWithError);                
            }
            else
            {
                MessageBox.Show("You are not signed in. Please enter credentials in the settings.");
            }
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