using System;
using System.Collections.Generic;
using Ninject;

namespace OpenSyno
{
  

    public class IoC
    {
        static public IKernel Container { get; set; }

        static IoC()
        {
            Container = new StandardKernel();            
        }
    }
}