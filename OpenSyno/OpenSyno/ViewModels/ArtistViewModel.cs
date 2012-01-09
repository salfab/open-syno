using System.Collections.Generic;
using Seesharp.LastFmApi.Mango;

namespace OpenSyno.ViewModels
{
    using OpemSyno.Contracts.Domain;

    public class ArtistViewModel : IArtistViewModel
    {
        public string Name { get; set; }
        public string Mbid { get; set; }
        public string Url { get; set; }
        public double Match { get; set; }
        public IEnumerable<LastFmImage> Images { get; set; }

        public ArtistViewModel(string name, string mbid, string url, double match, IEnumerable<LastFmImage> images)
        {
            Name = name;
            Mbid = mbid;
            Url = url;
            Match = match;
            Images = images;
        }
    }
}