using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static class ServiceManager
    {
        private static readonly Dictionary<string, object> routeHandlers = new Dictionary<string, object>();

        private static Socket listenerSocket;

        public static void AddService(object service)
        {
            if (service == null)
                throw new ArgumentNullException();

            var attributes = service.GetType().GetCustomAttributes(false);

            if (!attributes.Any(attr => attr.GetType() == typeof(ServiceAttribute)))
                throw new Exception("Service is not handler.");

            else
            {
                var attr = attributes.First(attr=>attr.GetType() == typeof(ServiceAttribute)) as ServiceAttribute;
                if (string.IsNullOrEmpty(attr.Route))
                    throw new Exception("Service route is null");

                routeHandlers.Add(attr.Route,service);
            }
        }

        public static void Initialize()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 8000);

            listenerSocket = new Socket(ipAddr.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            listenerSocket.Bind(localEndPoint);

            listenerSocket.Listen(2);

        }

        public static async Task Start()
        {
            while (true)
            {
                var client = await listenerSocket.AcceptAsync();

                new Thread(async () => await HandleClient(client)).Start();



            }
        }

        private static async Task HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];


            await clientSocket.ReceiveAsync(buffer,SocketFlags.None);

            Console.WriteLine(Encoding.UTF8.GetString(buffer));

        }
    }
}
