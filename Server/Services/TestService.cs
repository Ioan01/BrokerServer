using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Attributes;

namespace Server.Services
{
    [Service("test")]
    internal class TestService
    {
        [Route("hello")]
        public string Hello()
        {
            return "hello";
        }



        [Route("testRoute")]
        public void route1()
        {
            Console.WriteLine("test route called");
        }

        [Route("testRoute2")]
        public string route2()
        {
            return "asdads";
        }

        [Route("testRoute3")]
        public string route3(string a,int b,float c)
        {
            return $"{a} --- {b} --- {c}";
        }

        [Route("test")]
        public int test(int a,int b,float c)
        {
            return (int)(a+b+c);
        }
    }
}
