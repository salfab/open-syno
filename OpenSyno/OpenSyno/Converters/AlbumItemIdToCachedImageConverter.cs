namespace OpenSyno.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class AlbumItemIdToCachedImageConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var albumItemId = (string)value;
            var cachedImagesMapping = ((ImageCachingService)Application.Current.Resources["ImageCachingService"]).CachedImagesMappings.FirstOrDefault(o => o.ImageId == (string)value);
            if (cachedImagesMapping != null)
            {
                return cachedImagesMapping;
            }
            else
            {
                // not cached yet. TODO : Start caching
                return value;
            }
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageCachingService
    {
        public ImageCachingService()
        {
            this.CachedImagesMappings = new List<CachedImagesMapping>();
        }
        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string), typeof(ImageCachingService), new PropertyMetadata(null, OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string uriString = (string)e.NewValue;
            WebClient wc = new WebClient();

            var fileName = Path.GetFileName(uriString);
            using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (userStore.FileExists(fileName))
                {

                    using (var fs = userStore.OpenFile(fileName, FileMode.Open))
                    {
                        var image = new BitmapImage();
                        byte[] buffer = new byte[fs.Length];
                        fs.BeginRead(
                            buffer,
                            0,
                            (int)fs.Length,
                            (ar) =>
                                {
                                    fs.EndRead(ar);
                                    MemoryStream ms = new MemoryStream(buffer);
                                    image.SetSource(ms);
                                    ((Image)ar.AsyncState).Source = image;
                                },
                            d);
                    }
                }
                else
                {
                    wc.OpenReadCompleted += (s, ea) =>
                        {
                            var image = new BitmapImage();

                            if (wc.ResponseHeaders == null)
                            {
                                throw new WebException(
                                    "Could not retrieve album cover. Please check your internet connection.");
                            }

                            // download image to a local memory stream
                            var contentLength = int.Parse(wc.ResponseHeaders["content-length"]);
                            byte[] buffer = new byte[contentLength];
                            ea.Result.BeginRead(
                                buffer,
                                0,
                                contentLength,
                                ar =>
                                    {
                                        var bytes = ea.Result.EndRead(ar);
                                        MemoryStream ms = new MemoryStream(buffer);
                                        image.SetSource(ms);
                                        ((Image)ea.UserState).Source = image;
                                        Task<Uri> taskWriteToDisk = new Task<Uri>(
                                            stream =>
                                                {
                                                    using (var fs = userStore.CreateFile(fileName))
                                                    {
                                                        ms.Position = 0;
                                                        ms.CopyTo(fs);
                                                    }
                                                    return new Uri(fileName, UriKind.RelativeOrAbsolute);
                                                },
                                            ea.Result);
                                        taskWriteToDisk.Start();
                                        taskWriteToDisk.ContinueWith(
                                            taskUri =>
                                                {
                                                    Debug.WriteLine("Task write to disk : continue with (entering)");
                                                    Uri pathInIsolatedStorage = taskUri.Result;
                                                    var imageCachingService =
                                                        (ImageCachingService)
                                                        Application.Current.Resources["ImageCachingService"];
                                                    CachedImagesMapping cachedImagesMapping = new CachedImagesMapping
                                                        {
                                                            FilePath = pathInIsolatedStorage,
                                                            ImageId = uriString,
                                                            LastTimeUsed = DateTime.Now,
                                                            TimesUsed = 1
                                                        };
                                                    imageCachingService.CachedImagesMappings.Add(cachedImagesMapping);
                                                    Debug.WriteLine("Task write to disk : continue with (leaving)");

                                                },
                                            TaskScheduler.FromCurrentSynchronizationContext());
                                    },
                                d);
                        };
                }
            }

            Uri imageUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            wc.OpenReadAsync(imageUri, d);
        }

        public List<CachedImagesMapping> CachedImagesMappings { get; set; }

        public long TotalImageRequests { get; set; }
    }

    public class CachedImagesMapping
    {
        public string ImageId { get; set; }

        public int TimesUsed { get; set; }

        public DateTime LastTimeUsed { get; set; }

        public Uri FilePath { get; set; }
    }
}