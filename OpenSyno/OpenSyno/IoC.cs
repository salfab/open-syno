using System;
using System.Collections.Generic;
using System.ComponentModel;
using Ninject;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
  

    public class IoC
    {
        static public IKernel Container { get; set; }

        static IoC()
        {
            // FIXME : Load from config files instead of  hard-coded bindings.

            Container = new StandardKernel();
            // Retrieve the type IAudioStationSession from a config file, so we can change it.
            // Also possible : RemoteFileMockAudioStationSession
            Container.Bind<IAudioStationSession>().To(typeof(AudioStationSession)).InSingletonScope();

            // Retrieve the type SearchService from a config file, so we can change it.
            // also possible: MockSearchService;
            Container.Bind<ISearchService>().To(typeof(SearchService)).InSingletonScope();

            Container.Bind<ISearchAllResultsViewModelFactory>().To(typeof(SearchAllResultsViewModelFactory)).InSingletonScope();

            // Retrieve the type PlaybackService from a config file, so we can change it.
            Container.Bind<IPlaybackService>().To(typeof(PlaybackService)).InSingletonScope();

            // When in design-time : For blendability, look in a config file to retrieve the bindings to load.
            if (DesignerProperties.IsInDesignTool)
            {
                Container.Bind<SearchViewModel>().ToConstant(new SearchViewModel(null, null, null));
            }
        }


    }
}