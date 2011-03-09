using System.Windows.Markup;

namespace OpenSyno.Behaviors
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    using Microsoft.Phone.Controls;

    public class ArtistPanoramaItemKindToStyleConverter
    {


        public static Style GetTracksStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(TracksStyleProperty);
        }

        public static void SetTracksStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(TracksStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for TracksStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TracksStyleProperty =
            DependencyProperty.RegisterAttached("TracksStyle", typeof(Style), typeof(ArtistPanoramaItemKindToStyleConverter), new PropertyMetadata(null));

                


        public static Style GetAlbumsTemplate(DependencyObject obj)
        {
            return (Style)obj.GetValue(AlbumsTemplateProperty);
        }

        public static void SetAlbumsTemplate(DependencyObject obj, Style value)
        {
            obj.SetValue(AlbumsTemplateProperty, value);
        }

        // Using a DependencyProperty as the backing store for AlbumsTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlbumsTemplateProperty =
            DependencyProperty.RegisterAttached("AlbumsTemplate", typeof(Style), typeof(ArtistPanoramaItemKindToStyleConverter), new PropertyMetadata(null, AlbumsTemplatePropertyChanged));

        private static void AlbumsTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
   

        public static ArtistPanoramaItemKind GetPanoramaItemKind(DependencyObject obj)
        {
            return (ArtistPanoramaItemKind)obj.GetValue(PanoramaItemKindProperty);
        }

        public static void SetPanoramaItemKind(DependencyObject obj, ArtistPanoramaItemKind value)
        {
            obj.SetValue(PanoramaItemKindProperty, value);
        }

        // Using a DependencyProperty as the backing store for PanoramaItemKind.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanoramaItemKindProperty =
            DependencyProperty.RegisterAttached("PanoramaItemKind", typeof(ArtistPanoramaItemKind), typeof(ArtistPanoramaItemKindToStyleConverter), new PropertyMetadata(ArtistPanoramaItemKind.Biography, ArtistPanoramaItemKindPropertyChanged));

        private static void ArtistPanoramaItemKindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Style style = null;

            var styleTarget = (FrameworkElement)d;
            var kind = (ArtistPanoramaItemKind)e.NewValue;
            switch (kind)
            {
                case ArtistPanoramaItemKind.Biography:
                    break;
                case ArtistPanoramaItemKind.AlbumsList:
                    style = GetAlbumsTemplate(d);
                    break;
                case ArtistPanoramaItemKind.AlbumDetail:
                    style = GetTracksStyle(d);
                    break;
                case ArtistPanoramaItemKind.SimilarArtists:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();                
            }
            styleTarget.Style = style;

        }
    }
}