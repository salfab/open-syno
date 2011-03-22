namespace OpenSyno.ViewModels
{
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Phone.Tasks;
    using Microsoft.Practices.Prism.Commands;

    public class AboutBoxViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutBoxViewModel"/> class.
        /// </summary>
        public AboutBoxViewModel()
        {
            OpenWebsiteCommand = new DelegateCommand<string>(OnOpenWebsite);
            var versionRegex = new Regex("Version=([0-9\\.]+),");
            Version = versionRegex.Match(Assembly.GetExecutingAssembly().FullName).Groups[1].Value;
        }

        /// <summary>
        /// Called when the <see cref="OpenWebsiteCommand"/> command is invoked.
        /// </summary>
        /// <param name="url">The URL of the website to open.</param>
        private void OnOpenWebsite(string url)
        {
            var task = new WebBrowserTask { URL = url };
            task.Show();
        }

        /// <summary>
        /// Gets or sets the command to invoke to open a website in the default webbrowser.
        /// </summary>
        /// <value>The command to open the website.</value>
        public ICommand OpenWebsiteCommand { get; set; }

        public string Version { get; set; }
    }
}