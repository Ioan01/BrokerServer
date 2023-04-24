using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Server.Attributes;

namespace Server
{
    internal class ServiceRegistration
    {
        public object Service { get; }

        private readonly Dictionary<string,MethodInfo> routes = new();


        public ServiceRegistration(object service)
        {
            Service = service;

            foreach (var methodInfo in service.GetType().GetMethods())
            {
                var attribute = methodInfo.GetCustomAttributes()
                    .FirstOrDefault(attr=>attr is RouteAttribute) as RouteAttribute;

                if (attribute != null)
                {
                    if (string.IsNullOrEmpty(attribute.Route))
                        throw new Exception($"{methodInfo.Name} route cannot be empty");

                    if (routes.ContainsKey(attribute.Route))
                        throw new Exception($"Duplicate route exception {methodInfo.Name}");
                    routes.Add(attribute.Route.ToLower(), methodInfo);
                }

            }
        }

        public object? CallRoute(string route,ArraySegment<byte> message)
        {
            if (!routes.ContainsKey(route))
                return null;

            var routeMethod = routes[route];

            object[] paramArray = Unmarshaller.unmarshall(message, routeMethod.GetParameters());


            return routeMethod.Invoke(Service, paramArray);
        }
    }
}
