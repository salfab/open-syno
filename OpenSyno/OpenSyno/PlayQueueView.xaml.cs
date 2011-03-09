namespace OpenSyno
{
    using Microsoft.Phone.Controls;

    using OpenSyno.ViewModels;

    public partial class PlayQueueView : PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayQueueView"/> class.
        /// </summary>
        public PlayQueueView()
        {
            DataContext = IoC.Current.Resolve<PlayQueueViewModel>();
            InitializeComponent();
        }
    }
}