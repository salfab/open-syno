namespace OpenSyno.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using Ninject;

    public class ImageCachingService
    {
        private static List<Task<Uri>> _tasksWritingOnDisk;
        public ImageCachingService()
        {
            _tasksWritingOnDisk = new List<Task<Uri>>();
            this.CachedImagesMappings = new List<CachedImagesMapping>();
            this.MaxBindingsLimit = 100;
        }
        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        public event EventHandler SaveRequested;

        public void RequestSave(EventArgs e)
        {
            OnSaveRequested(e);
        }

        private void OnSaveRequested(EventArgs e)
        {
            EventHandler handler = this.SaveRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(string), typeof(ImageCachingService), new PropertyMetadata(null, OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageCachingService = IoC.Container.Get<ImageCachingService>();
            string uriString = (string)e.NewValue;
            WebClient wc = new WebClient();

            var fileName = Path.GetFileName(uriString);
            imageCachingService.TotalImageRequests++;
            string albumCoverId = uriString;
            var matchingMapping = imageCachingService.CachedImagesMappings.FirstOrDefault(o => o.ImageId == albumCoverId);
            //using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    fileExists = userStore.FileExists(fileName);
            //}
            if (matchingMapping != null)
            {
                matchingMapping.TimesUsed++;
                matchingMapping.LastTimeUsed = DateTime.Now;
                using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fs = userStore.OpenFile(fileName, FileMode.Open))
                    {
                        var image = new BitmapImage();
                        byte[] buffer = new byte[fs.Length];
                        fs.BeginRead(
                            buffer,
                            0,
                            (int)fs.Length,
                            ar =>
                                {
                                    var readBytes = fs.EndRead(ar);
                                    MemoryStream ms = new MemoryStream(buffer);
                                    image.SetSource(ms);
                                    ((Image)ar.AsyncState).Source = image;
                                },
                            d);
                    }
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
                                                try
                                                {
                                                    IEnumerable<Uri> paths = null;
                                                    if (imageCachingService.CachedImagesMappings.Count >= imageCachingService.MaxBindingsLimit)
                                                    {
                                                        paths = from mapping in imageCachingService.CachedImagesMappings
                                                                where mapping.LastTimeUsed < DateTime.Now.AddDays(-14)
                                                                orderby mapping.TimesUsed descending
                                                                select mapping.FilePath;
                                                    }

                                                    using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                                                    {
                                                        if (paths != null && paths.Count() >= 1)
                                                        {
                                                            userStore.DeleteFile(paths.First().AbsoluteUri);
                                                        }

                                                        using (var fs = userStore.CreateFile(fileName))
                                                        {
                                                            ms.Position = 0;
                                                            ms.CopyTo(fs);
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                    throw;
                                                }
                                                return new Uri(fileName, UriKind.RelativeOrAbsolute);
                                            },
                                        ea.Result);
                                    _tasksWritingOnDisk.Add(taskWriteToDisk);
                                    taskWriteToDisk.Start();
                                    taskWriteToDisk.ContinueWith(
                                        taskUri =>
                                            {                                                
                                                Uri pathInIsolatedStorage = taskUri.Result;
                                        
                                                CachedImagesMapping cachedImagesMapping = new CachedImagesMapping
                                                    {
                                                        FilePath = pathInIsolatedStorage, 
                                                        ImageId = uriString, 
                                                        LastTimeUsed = DateTime.Now, 
                                                        TimesUsed = 1
                                                    };
                                                imageCachingService.CachedImagesMappings.Add(cachedImagesMapping);
                                                
                                                // let's try not to kill our storage by writing too often on it.
                                                Timer throttlingWriteToDisk = new Timer(t =>
                                                    {
                                                        _tasksWritingOnDisk.Remove(taskUri);
                                                        if (_tasksWritingOnDisk.Count > 0)
                                                        {
                                                            return;
                                                        }
                                                        
                                                        Debug.WriteLine("Actual write request.");
                                                        imageCachingService.RequestSave(EventArgs.Empty);
                                                    }, 
                                                    null, 
                                                    2000, 
                                                    uint.MaxValue);
                                                                                                                                               
                                            },
                                        TaskScheduler.FromCurrentSynchronizationContext());
                                },
                            d);
                    };
            }


            Uri imageUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            wc.OpenReadAsync(imageUri, d);
        }

        public int MaxBindingsLimit { get; set; }

        public List<CachedImagesMapping> CachedImagesMappings { get; set; }

        public long TotalImageRequests { get; set; }
    }
}