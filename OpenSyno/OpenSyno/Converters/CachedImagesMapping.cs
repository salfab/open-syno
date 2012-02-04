namespace OpenSyno.Converters
{
    using System;
    
    public class CachedImagesMapping
    {
        public string ImageId { get; set; }

        public int TimesUsed { get; set; }

        public DateTime LastTimeUsed { get; set; }

        public string FilePath { get; set; }
    }
}