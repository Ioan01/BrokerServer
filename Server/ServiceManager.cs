using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Server.Attributes;

namespace Server
{
    internal static class ServiceManager
    {
        private static readonly Dictionary<string, ServiceRegistration> _controllers = new ();

        private static Socket listenerSocket = null;

        public static void AddService(object service)
        {

            if (service == null)
                throw new ArgumentNullException();

            var attributes = service.GetType().GetCustomAttributes(false);

            var serviceAttribute = attributes.FirstOrDefault(attr=>attr is ServiceAttribute) as ServiceAttribute;

            if (serviceAttribute == null)
                throw new Exception("Service is not handler.");

            _controllers.Add(serviceAttribute.ServiceName.ToLower(),new ServiceRegistration(service));
            
        }

        private static void RegisterServer(string dispatcherAdress)
        {

        }

        public static void Initialize(string dispatcherAdress)
        {
            RegisterServer(dispatcherAdress);


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


        private static KeyValuePair<string, string> GetServiceAndRoute(string fullRoute)
        {
            var slashIndex = fullRoute.IndexOf('/');

            if (slashIndex == -1)
                throw new Exception($"Invalid full path : {fullRoute}");

            var pair = new KeyValuePair<string, string>(fullRoute.Substring(0, slashIndex),
                fullRoute.Substring(slashIndex + 1));

            if (!_controllers.ContainsKey(pair.Key))
                throw new Exception($"Controller {pair.Key} not found");

            return pair;

        }

        private static async Task HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];


            await clientSocket.ReceiveAsync(buffer,SocketFlags.None);


            // very inefficient duplication of strings and byte array manipulation
            try
            {
                var fullPathEnd = Array.IndexOf(buffer, (byte)'\n');

                var messageEnd = Array.LastIndexOf(buffer, (byte)3);


                string fullPath = Encoding.Default.GetString(buffer, 0, fullPathEnd).Trim().ToLower();



                var message = new ArraySegment<byte>(buffer, fullPathEnd + 1, messageEnd - fullPathEnd - 1);

                var serviceAndRoute = GetServiceAndRoute(fullPath);

                Console.WriteLine(fullPath);
                Console.WriteLine(message);


                var response = _controllers[serviceAndRoute.Key].CallRoute(serviceAndRoute.Value, message);
                if (response != null)
                {
                    if (response.GetType() == typeof(string))
                        await clientSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(response), SocketFlags.None);
                    else if (response.GetType() == typeof(int))
                        await clientSocket.SendAsync(BitConverter.GetBytes((int)response),SocketFlags.None);
                    else if (response.GetType() == typeof(float))
                        await clientSocket.SendAsync(BitConverter.GetBytes((float)response), SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                await clientSocket.SendAsync(Encoding.Default.GetBytes(e.Message + e.StackTrace),SocketFlags.None);
            }
            finally
            {
                clientSocket.Close();
            }
        }
    }
}
