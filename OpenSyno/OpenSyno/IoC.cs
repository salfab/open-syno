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
            Container = new StandardKernel();

            // When in design-time : For blendability, look in a config file to retrieve the bindings to load.
            if (DesignerProperties.IsInDesignTool)
            {
                Container.Bind<SearchViewModel>().ToConstant(new SearchViewModel(null, null, null));
            }
        }


    }
}