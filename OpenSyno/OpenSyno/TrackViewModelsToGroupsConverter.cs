using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using OpenSyno.ViewModels;

namespace OpenSyno
{
    public class TrackViewModelsToGroupsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new List<Group<TrackViewModel>>();
            }

            var typedValue = (IEnumerable<TrackViewModel>) value;

            // var artists = new List<Group<SearchResultItemViewModel>>(from artist in SearchResults.Where(o => o.ItemInfo.ItemPid == "musiclib_music_aa") group artist by artist.ItemInfo.HeaderContent.FirstOrDefault() into c select new Group<SearchResultItemViewModel>(char.IsLetter(c.Key) ? c.Key.ToString().ToLower() : "#", c));
            var groups = from track in typedValue group track by track.ConsecutiveAlbumIdentifier ;

            var groupedTracks = new List<Group<TrackViewModel>>();
            foreach (var group in groups)
            {                          
                // Add a new group
                groupedTracks.Add(new Group<TrackViewModel>(typedValue.First(o=>o.ConsecutiveAlbumIdentifier == group.Key), group));
                
            }

           

            //var tempArtists = from a in SearchResults.Where(o => o.Class == "Artist") select a;

            //new { StartsWith = a.Artist.FirstOrDefault(), Artist = a.Artist, Class = a.Class, a.ItemID, a.ItemPid };
            //var artists = tempArtists.GroupBy(o => o.);
            //from city in source 
            //    group city by city.Country into c   
            //        orderby c.Key     
            //            select new Group<City>(c.Key, c);
            
            return groupedTracks;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}