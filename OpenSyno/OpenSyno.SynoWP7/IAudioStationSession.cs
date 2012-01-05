namespace Synology.AudioStationApi
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    public interface IAudioStationSession
    {
        /// <summary>
        /// Gets the remote file network stream.
        /// </summary>
        /// <param name="synoTrack">The track wor which to retrieve the stream.</param>
        /// <param name="callback">The method to call after the stream is open. The HttpResponse and the related <see cref="SynoTrack"/>are passed as argument</param>
        /// <remarks>The caller is responsible for closing the stream after the call to DownloadFile returns</remarks>
        void GetFileStream(SynoTrack synoTrack, Action<WebResponse, SynoTrack> callback);

        void LoginAsync(string login, string password, Action<string> callback, Action<Exception> callbackError, bool useSsl);

        void SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>> callback, Action<Exception> callbackError);
        void SearchArtist(string pattern, Action<IEnumerable<SynoItem>> callback, Action<Exception> callbackError);
        void GetAlbumsForArtist(SynoItem artist, Action<IEnumerable<SynoItem>, long, SynoItem> callback, Action<Exception> callbackError);
        void GetTracksForAlbum(SynoItem album, Action<IEnumerable<SynoTrack>, long, SynoItem> callback, Action<Exception> callbackError);

        bool IsSignedIn { get; }

        [DataMember]
        string Host { get; set; }

        [DataMember]
        int Port { get; set; }

        [DataMember]
        string Token { get; }

        Task<IEnumerable<SynoItem>> SearchAlbums(string album);
        Task<IEnumerable<SynoItem>> SearchArtistAsync(string artistName);
        Task<IEnumerable<SynoItem>> GetAlbumsForArtistAsync(SynoItem artist);

        Task<IEnumerable<SynoTrack>> GetTracksForAlbumAsync(SynoItem album);
    }
}