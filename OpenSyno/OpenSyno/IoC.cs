using System;
using System.Collections.Generic;

namespace OpenSyno
{
    using MicroIoc;

    public class IoC
    {
        static public IMicroIocContainer Container { get; set; }

        static IoC()
        {
            Container = new MicroIocContainer();
        }
    }
}