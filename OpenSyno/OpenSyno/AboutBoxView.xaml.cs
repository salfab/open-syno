namespace OpenSyno
{
    using OpenSyno.ViewModels;

    public partial class AboutBoxView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutBoxView"/> class.
        /// </summary>
        public AboutBoxView()
        {
            DataContext = new AboutBoxViewModel();
            InitializeComponent();
        }
    }
}