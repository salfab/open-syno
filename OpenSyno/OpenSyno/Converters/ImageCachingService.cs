namespace OpenSyno.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using Ninject;

    using OpenSyno.Services;

    [DataContract]
    public class ImageCachingService
    {
        private static List<Task<string>> _tasksWritingOnDisk;
        public ImageCachingService(ILogService logService)
        {
            Initialize();
            _logService = logService;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// When the service gets deserialized, the constructor doesn't get called, therefore, this method can be called after deserialization in order to initialize the private non-persisted internals.
        /// </remarks>
        public void Initialize()
        {
            _logService = IoC.Container.Get<ILogService>();
            _tasksWritingOnDisk = new List<Task<string>>();
            this.CachedImagesMappings = new List<CachedImagesMapping>();
            this.MaxBindingsLimit = 100;
            internalIsolatedStorageAccessLock = new object();
        }

        public static string GetImageId(DependencyObject obj)
        {
            return (string)obj.GetValue(ImageIdProperty);
        }

        public static void SetImageId(DependencyObject obj, string value)
        {
            obj.SetValue(ImageIdProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageIdProperty =
            DependencyProperty.RegisterAttached("ImageId", typeof(string), typeof(ImageCachingService), new PropertyMetadata(null));        

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
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source", 
            typeof(string), 
            typeof(ImageCachingService), 
            new PropertyMetadata(
                null, (source, ea) => IoC.Container.Get<ImageCachingService>().OnSourcePropertyChanged(source, ea)));

        private static object internalIsolatedStorageAccessLock;

        private static ILogService _logService;

        private void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string albumCoverId = GetImageId(d);
            if (albumCoverId == null)
            {
                throw new ArgumentNullException("AlbumId", "The attached property 'AlbumIdProperty' must be set for an image to be cached.");
            }

            string uriString = (string)e.NewValue;
            WebClient wc = new WebClient();

            string fileName;

            var matchingMapping = this.CachedImagesMappings.FirstOrDefault(o => o.ImageId == albumCoverId);

            if (matchingMapping != null)
            {
                fileName = matchingMapping.FilePath;
            }
            else
            {
                fileName = Path.GetFileName(uriString);
            }
            this.TotalImageRequests++;

            if (matchingMapping != null)
            {
                matchingMapping.TimesUsed++;
                matchingMapping.LastTimeUsed = DateTime.Now;
                lock (internalIsolatedStorageAccessLock)
                {
                    using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!userStore.FileExists(fileName))
                        {
                            throw new NotImplementedException("Cache has been deleted. Automatic rebuild of a deleted cache is not yet implemented.");
                        }
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
                                        _logService.Trace(string.Format("ImageCachingService.OnSourcePropertyChanged : Network read completed with {0} bytes", readBytes));                                        
                                        image.SetSource(ms);
                                        ((Image)ar.AsyncState).Source = image;
                                    },
                                d);
                        }
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
                                    _logService.Trace(string.Format("ImageCachingService.OnSourcePropertyChanged : Network read completed with {0} bytes", bytes));

                                    image.SetSource(ms);
                                    ((Image)ea.UserState).Source = image;
                                    Task<string> taskWriteToDisk = new Task<string>(
                                        stream =>
                                            {
                                                try
                                                {
                                                    IEnumerable<string> paths = null;
                                                    if (this.CachedImagesMappings.Count >= this.MaxBindingsLimit)
                                                    {
                                                        paths = from mapping in this.CachedImagesMappings
                                                                where mapping.LastTimeUsed < DateTime.Now.AddDays(-14)
                                                                orderby mapping.TimesUsed descending
                                                                select mapping.FilePath;
                                                    }
                                                    lock (internalIsolatedStorageAccessLock)
                                                    {
                                                        using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                                                        {
                                                            if (paths != null && paths.Count() >= 1)
                                                            {
                                                                userStore.DeleteFile(paths.First());
                                                            }

                                                            using (var fs = userStore.CreateFile(fileName))
                                                            {
                                                                ms.Position = 0;
                                                                ms.CopyTo(fs);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                    throw;
                                                }
                                                return fileName;
                                            },
                                        ea.Result);
                                    _tasksWritingOnDisk.Add(taskWriteToDisk);
                                    taskWriteToDisk.Start();
                                    taskWriteToDisk.ContinueWith(
                                        taskUri =>
                                            {                                                
                                                var pathInIsolatedStorage = taskUri.Result;
                                        
                                                CachedImagesMapping cachedImagesMapping = new CachedImagesMapping
                                                    {
                                                        FilePath = pathInIsolatedStorage, 
                                                        ImageId = albumCoverId, 
                                                        LastTimeUsed = DateTime.Now, 
                                                        TimesUsed = 1
                                                    };
                                                this.CachedImagesMappings.Add(cachedImagesMapping);
                                                
                                                // let's try not to kill our storage by writing too often on it.
                                                Timer throttlingWriteToDisk = new Timer(t =>
                                                    {
                                                        _tasksWritingOnDisk.Remove(taskUri);
                                                        if (_tasksWritingOnDisk.Count > 0)
                                                        {
                                                            return;
                                                        }
                                                        
                                                        Debug.WriteLine("Actual write request.");
                                                        this.RequestSave(EventArgs.Empty);
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

        [DataMember]
        public int MaxBindingsLimit { get; set; }

        [DataMember]
        public List<CachedImagesMapping> CachedImagesMappings { get; set; }

        [DataMember]
        public long TotalImageRequests { get; set; }
    }
}