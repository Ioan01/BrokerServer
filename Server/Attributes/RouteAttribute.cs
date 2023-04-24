using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Attributes
{
    internal class RouteAttribute : Attribute
    {
        public string Route { get; }

        public RouteAttribute(string route)
        {
            Route = route;
        }


    }
}
