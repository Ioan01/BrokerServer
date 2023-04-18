using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class ControllerAttribute : Attribute
    {
        public string Route { get; }

        public ControllerAttribute(string route)
        {
            Route = route;
        }


    }
}
