using System.ComponentModel;
using Ninject;

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
                // FIXME : use default values instead of null, and thro exception on null params.
                // FIXME : Load from config file : we don't want dependencies on OpenSyno.SynoWP7 here.
                // Container.Bind<SearchViewModel>().ToConstant(new SearchViewModel(null, null, null, null, null));
            }
        }


    }
}