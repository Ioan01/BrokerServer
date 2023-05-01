using System.Reflection;
using Server.Attributes;

namespace Server;

internal enum Lifespan
{
    Singleton,
    Transient
}


internal class ServiceRegistration
{
    private readonly Lifespan _lifespan;

    private readonly Dictionary<string, MethodInfo> routes = new();

    private object _service;

    private Type _serviceType;


    public ServiceRegistration(Type serviceType, Lifespan lifespan)
    {
        _lifespan = lifespan;
        _serviceType = serviceType;

        if (lifespan == Lifespan.Singleton)
            _service = Activator.CreateInstance(serviceType);


        foreach (var methodInfo in serviceType.GetMethods())
        {
            var attribute = methodInfo.GetCustomAttributes()
                .FirstOrDefault(attr => attr is RouteAttribute) as RouteAttribute;

            if (attribute != null) routes.Add(methodInfo.Name.ToLower(), methodInfo);
        }
    }

    public object Service
    {
        get
        {
            if (_lifespan == Lifespan.Transient)
                _service = Activator.CreateInstance(_serviceType);
            return _service;
        }
    }

    public object? CallRoute(string route, ArraySegment<byte> message)
    {
        if (!routes.ContainsKey(route))
            return null;

        var routeMethod = routes[route];

        var paramArray = Unmarshaller.unmarshall(message, routeMethod.GetParameters());


        return routeMethod.Invoke(Service, paramArray);
    }
}