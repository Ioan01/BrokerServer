using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class HandlerAttribute : Attribute
    {
        public string Function { get;  }

        public HandlerAttribute(string function)
        {
            Function = function;
        }


    }
}
