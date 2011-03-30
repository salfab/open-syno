using OpenSyno.ViewModels;

namespace OpenSyno
{
    using OpenSyno.Converters;

    public class ArtistPanoramaItem : ViewModelBase, IBusyable
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged(IsBusyPropertyName);
            }
        }

        private ArtistPanoramaItemKind _panoramaItemKind;
        public const string IsBusyPropertyName = "IsBusy";

        private const string PanoramaItemKindPropertyName = "PanoramaItemKind";

        public ArtistPanoramaItemKind PanoramaItemKind
        {
            get
            {
                return this._panoramaItemKind;
            }

            set
            {
                this._panoramaItemKind = value;
                OnPropertyChanged(PanoramaItemKindPropertyName);
            }
        }

        protected ArtistPanoramaItem(ArtistPanoramaItemKind panoramaItemKind)
        {
            PanoramaItemKind = panoramaItemKind;
        }

        public string Header { get; set; }
    }
}