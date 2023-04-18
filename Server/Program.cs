namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ControllerManager.Initialize();

            await ControllerManager.Start();
        }
    }
}