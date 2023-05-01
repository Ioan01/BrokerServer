using Server.Services;

namespace Server;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = new ServiceHost();

        host.AddService<TestService>(Lifespan.Transient);

        await host.Initialize("http://127.0.0.1:8000/registerHost", "test");


        await host.Start();
    }
}