using Server.Services;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ServiceManager.AddService(new TestService());

            ServiceManager.Initialize("127.0.0.1");

            

            await ServiceManager.Start();
        }
    }
}