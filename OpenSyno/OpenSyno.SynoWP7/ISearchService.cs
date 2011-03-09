namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    using Synology.AudioStationApi;

    public interface ISearchService
    {
        void SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>> callback);
        void SearchArtists(string pattern, Action<IEnumerable<SynoItem>> callback);
        void GetAllArtists(Action<IEnumerable<SynoItem>> callback);

        /// <summary>
        /// Gets the albums for artist.
        /// </summary>
        /// <param name="artist">The artist.</param>
        /// <param name="callback">The callback : a method which will receive a list of albums, the total number of albums available (might be greater than the number of items in the list of albums if it is really big) and the artist SynoItem it belongs to.</param>
        void GetAlbumsForArtist(SynoItem artist, Action<IEnumerable<SynoItem>, long, SynoItem> callback);

        void GetTracksForAlbum(SynoItem album, Action<IEnumerable<SynoTrack>, long, SynoItem> callback);
    }
}