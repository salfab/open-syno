using OpenSyno.ViewModels;

namespace OpenSyno
{
    using OpenSyno.Converters;

    public class ArtistPanoramaItem : ViewModelBase, IBusyable
    {
        public bool IsBusy { get; set; }

        private ArtistPanoramaItemKind _panoramaItemKind;

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