namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public interface ISearchService
    {
        bool SearchAllMusic(string pattern, Action<IEnumerable<ISynoTrack>, string> callback);
        /// <summary>
        /// Searches the artists.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="callback">The callback.</param>
        /// <returns><c>true</c> if the search could be issued; <c>false</c> if it had to be canceled.</returns>
        bool SearchArtists(string pattern, Action<IEnumerable<ISynoItem>> callback);
        void GetAllArtists(Action<IEnumerable<ISynoItem>> callback);

        /// <summary>
        /// Gets the albums for artist.
        /// </summary>
        /// <param name="artist">The artist.</param>
        /// <param name="callback">The callback : a method which will receive a list of albums, the total number of albums available (might be greater than the number of items in the list of albums if it is really big) and the artist SynoItem it belongs to.</param>
        void GetAlbumsForArtist(ISynoItem artist, Action<IEnumerable<ISynoItem>, long, ISynoItem> callback);

        void GetTracksForAlbum(ISynoItem album, Action<IEnumerable<ISynoTrack>, long, ISynoItem> callback);
    }
}