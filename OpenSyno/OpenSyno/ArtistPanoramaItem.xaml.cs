using OpenSyno.ViewModels;

namespace OpenSyno
{
    public class ArtistPanoramaItem : ViewModelBase
    {
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