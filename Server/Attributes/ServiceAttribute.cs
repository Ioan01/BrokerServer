using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Attributes
{
    internal class ServiceAttribute : Attribute
    {
        public string ServiceName { get; }

        public ServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }


    }
}
