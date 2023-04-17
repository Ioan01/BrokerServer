using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class ServiceAttribute : Attribute
    {
        public string Route { get; }

        public ServiceAttribute(string route)
        {
            Route = route;
        }


    }
}
