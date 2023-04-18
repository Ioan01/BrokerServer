using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static class ControllerManager
    {
        private static readonly Dictionary<string, object> _controllers = new Dictionary<string, object>();

        private static Socket listenerSocket = null;

        public static void AddService(object service)
        {
            if (service == null)
                throw new ArgumentNullException();

            var attributes = service.GetType().GetCustomAttributes(false);

            if (!attributes.Any(attr => attr.GetType() == typeof(ControllerAttribute)))
                throw new Exception("Service is not handler.");

            else
            {
                var attr = attributes.First(attr=>attr.GetType() == typeof(ControllerAttribute)) as ControllerAttribute;
                if (string.IsNullOrEmpty(attr.Route))
                    throw new Exception("Service route is null");

                _controllers.Add(attr.Route,service);
            }
        }

        public static void Initialize()
        {
            bool foundPort = false;

            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            int port = 8000;

            while (!foundPort)
            {
                
                try
                {
                    IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

                    listenerSocket = new Socket(ipAddr.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    listenerSocket.Bind(localEndPoint);
                    foundPort = true;

                    Console.WriteLine($"Server started on port {port}");

                }
                catch (Exception)
                {
                    Console.WriteLine($"Port {port} is already in use.");
                    port++;
                }
            }
            

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

            buffer
            


        }
    }
}
