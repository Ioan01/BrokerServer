namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ServiceManager.Initialize();

            await ServiceManager.Start();
        }
    }
}