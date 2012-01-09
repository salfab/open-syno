namespace OpenSyno.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using Ninject;

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
            var cachedImagesMapping = IoC.Container.Get<ImageCachingService>().CachedImagesMappings.FirstOrDefault(o => o.ImageId == (string)value);
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

    public class CachedImagesMapping
    {
        public string ImageId { get; set; }

        public int TimesUsed { get; set; }

        public DateTime LastTimeUsed { get; set; }

        public string FilePath { get; set; }
    }
}