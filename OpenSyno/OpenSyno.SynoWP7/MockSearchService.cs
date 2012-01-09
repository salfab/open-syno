namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    using Synology.AudioStationApi;

    public class MockSearchService : ISearchService
    {
        public bool SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>, string> callback)
        {
            var items = new List<SynoTrack>();
            items.Add(new SynoTrack
                {
                    Artist = "Tom Waits",
                    Album = "Alice", 
                    Title = "Poor Edward",
                });

            items.Add(new SynoTrack
                {
                    Artist = "Tom Waits",
                    Album = "Alice",
                    Title = "Flowers Grave",                            
                });

            items.Add(new SynoTrack
                {
                    Artist = "Tom Waits",
                    Album = "Alice",
                    Title = "Fawn",
                });

            callback((IEnumerable<SynoTrack>) items, "my keyword");
            return true;
        }

        public bool SearchArtists(string pattern, Action<IEnumerable<SynoItem>> callback)
        {
            var results = new List<SynoItem>();

            results.Add(new SynoItem
                {
                    Title = "Tom Waits",
                    ItemPid = "musiclib_music_aa",
                    ItemID = "1"

                });

            results.Add(new SynoItem
                {
                    Title = "Mike Patton",
                    ItemPid = "musiclib_music_aa",
                    ItemID = "2"

                });

            results.Add(new SynoTrack
                {
                    Title = "65daysofstatic",
                    ItemPid = "musiclib_music_aa",
                    ItemID = "3"
                });

            callback(results);
            return true;
        }

        public void GetAllArtists(Action<IEnumerable<SynoItem>> callback)
        {
            var results = new List<SynoItem>();
            results.Add(new SynoItem
                {
                    Title = "Tom Waits",
                    ItemPid = "musiclib_music_aa"
                });

            results.Add(new SynoItem
                {
                    Title = "Mike Patton",
                    ItemPid = "musiclib_music_aa"
                });

            results.Add(new SynoItem
                {
                    Title = "Panic! at the disco",
                    ItemPid = "musiclib_music_aa"
                });

            callback(results);
        }

        public void GetAlbumsForArtist(SynoItem artist, Action<IEnumerable<SynoItem>, long, SynoItem> callback)
        {
            var results = new List<SynoItem>();                       
            // All music
            results.Add(new SynoItem
                {
                    AlbumArtUrl = "11-18-04.jpg",
                    Icon = "icon_container.png",
                    IsContainer = true,
                    IsTrack = false,
                    ItemID = "musiclib_music_artist",
                    ItemPid = "musiclib_music_aa/852502",
                    Sequence = 0,
                    Support = false,
                    Title = "All Music"
                });

            results.Add(new SynoItem
                {
                    AlbumArtUrl = "11-18-04.jpg",
                    Icon = "icon_container.png",
                    IsContainer = true,
                    IsTrack = false,
                    ItemID = "musiclib_music_aa/852502/852502",
                    ItemPid = "musiclib_music_aa/852502",
                    Sequence = 1,
                    Support = false,
                    Title = "Bleach"
                });

            results.Add(new SynoItem
                {
                    AlbumArtUrl = "11-18-04.jpg",                    
                    Icon = "icon_container.png",
                    IsContainer = true,
                    IsTrack = false,
                    ItemID = "musiclib_music_aa/852502/852553",
                    ItemPid = "musiclib_music_aa/852502",
                    Sequence = 2,
                    Support = false,
                    Title = "From the Muddy Banks of the Wi"
                });

            results.Add(new SynoItem
                {
                    AlbumArtUrl = "11-18-04.jpg",                   
                    Icon = "icon_container.png",
                    IsContainer = true,
                    IsTrack = false,
                    ItemID = "musiclib_music_aa/852502/852529",
                    ItemPid = "musiclib_music_aa/852502",
                    Sequence = 3,
                    Support = false,
                    Title = "In Utero"
                });

            results.Add(new SynoItem
                {
                    AlbumArtUrl = "11-18-04.jpg",                    
                    Icon = "icon_container.png",
                    IsContainer = true,
                    IsTrack = false,
                    ItemID = "musiclib_music_aa/852502/852515",
                    ItemPid = "musiclib_music_aa/852502",
                    Sequence = 4,
                    Support = false,
                    Title = "MTV Unplugged In New York"
                });

            results.Add(new SynoItem
                {
                    AlbumArtUrl = "11-18-04.jpg",                    
                    Icon = "icon_container.png",
                    IsContainer = true,
                    IsTrack = false,
                    ItemID = "musiclib_music_aa/852502/885030",
                    ItemPid = "musiclib_music_aa/852502",
                    Sequence = 5,
                    Support = false,
                    Title = "Nevermind"
                });

            callback(results, 6, artist);
        }

        public void GetTracksForAlbum(SynoItem album, Action<IEnumerable<SynoTrack>, long, SynoItem> callback)
        {
            List<SynoTrack> tracks = new List<SynoTrack>();
            tracks.Add(new SynoTrack { Album = "Alice", Artist = "Tom Waits", Title = "Alice" });
            tracks.Add(new SynoTrack { Album = "Alice", Artist = "Tom Waits", Title = "Flower's grave" });
            tracks.Add(new SynoTrack { Album = "Alice", Artist = "Tom Waits", Title = "Kommeniedzuspedt" });
            tracks.Add(new SynoTrack { Album = "Alice", Artist = "Tom Waits", Title = "Poor Edward" });
            tracks.Add(new SynoTrack { Album = "Alice", Artist = "Tom Waits", Title = "Bacarole" });
            tracks.Add(new SynoTrack { Album = "Alice", Artist = "Tom Waits", Title = "Fawn" });
            callback(tracks, tracks.Count, album);
        }


    }
}