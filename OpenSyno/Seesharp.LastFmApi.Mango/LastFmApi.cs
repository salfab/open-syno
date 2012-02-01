using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Seesharp.LastFmApi.Mango
{
    public class LastFmApi
    {
        public Task<IEnumerable<LastFmArtist>> GetSimilarArtistsAsync(string artist)
        {
            var taskCompletionSource = new TaskCompletionSource<IEnumerable<LastFmArtist>>();
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (s, ea) =>
                                              {
                                                  var xdoc = XDocument.Parse(ea.Result);
                                                  var similarArtists =
                                                      from a in xdoc.Element("lfm").Element("similarartists").Elements("artist")
                                                      select new LastFmArtist(  a.Element("name").Value,
                                                                                a.Element("mbid").Value,
                                                                                a.Element("match").Value,
                                                                                a.Element("url").Value,
                                                                                a.Elements("Image").Select(o=> new LastFmImage(){Url = o.Value}), 
                                                                                a.Element("streamable").Value );

                                                  taskCompletionSource.SetResult(similarArtists);
                                              };
            wc.DownloadStringAsync(new Uri(string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getsimilar&artist={0}&api_key=b25b959554ed76058ac220b7b2e0a026", HttpUtility.UrlEncode(artist))));

            return taskCompletionSource.Task;
        }
    }

    public class LastFmArtist
    {

        public LastFmArtist(string name, string mbid, string match, string url, IEnumerable<LastFmImage> images, string streamable)
        {
            Name = name;
            Mbid = mbid;
            double result;
            double.TryParse(match, out result);
            Match = result;
            Url = url;
            Images = images;
            Streamable = streamable == "1";
        }

        public string Name { get; set; }
        public string Mbid { get; set; }
        public double Match { get; set; }
        public string Url { get; set; }
        public IEnumerable<LastFmImage> Images { get; set; }
        public bool Streamable { get; set; }
    }

    public class LastFmImage
    {
        public LastFmImageSize Size { get; set; }
        public string Url { get; set; }
    }

    public enum LastFmImageSize
    {
        Small, 
        Medium, 
        Large, 
        Extralarge, 
        Mega
    }
}
