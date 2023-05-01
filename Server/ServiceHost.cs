using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.Attributes;

namespace Server;





internal class ServiceHost
{
    private readonly Dictionary<string, ServiceRegistration> _controllers = new();

    private readonly HttpClient _httpClient = new();

    private Socket listenerSocket;

    public void AddService<T>(Lifespan lifespan) where T : class
    {
        var attributes = typeof(T).GetCustomAttributes(false);

        var serviceAttribute = attributes.FirstOrDefault(attr => attr is ServiceAttribute) as ServiceAttribute;

        if (serviceAttribute == null)
            throw new Exception("Service is not handler.");

        _controllers.Add(typeof(T).Name.Replace("Service","",true,CultureInfo.CurrentCulture).ToLower()
            , new ServiceRegistration(typeof(T),lifespan));
    }

    private Task<HttpResponseMessage> RegisterServer(string dispatcherAdress, string serviceName)
    {
        var registerForm = new MultipartFormDataContent();

        registerForm.Add(new StringContent(serviceName), "hostName");

        return _httpClient.PostAsync(dispatcherAdress, registerForm);
    }

    public async Task Initialize(string dispatcherAdress, string hostName)
    {
        var response = await RegisterServer(dispatcherAdress, hostName);

        var responseMsg = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Can't start server : {responseMsg}");


        var ipAddr = IPAddress.Parse("127.0.0.1");
        var port = Convert.ToInt32(responseMsg);

        var localEndPoint = new IPEndPoint(ipAddr, port);

        listenerSocket = new Socket(ipAddr.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        listenerSocket.Bind(localEndPoint);

        Console.WriteLine($"Server started on port {port}");


        listenerSocket.Listen(2);
    }

    public async Task Start()
    {
        while (true)
        {
            var client = await listenerSocket.AcceptAsync();

            new Thread(async () => await HandleClient(client)).Start();
        }
    }


    private KeyValuePair<string, string> GetServiceAndRoute(string fullRoute)
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

    private async Task HandleClient(Socket clientSocket)
    {
        var buffer = new byte[1024];

        while (clientSocket.Connected)
        {
            try
            {
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
            }
            catch (Exception ex)
            {
                clientSocket.Close();
                return;
            }


            // very inefficient duplication of strings and byte array manipulation
            try
            {
                var fullPathEnd = Array.IndexOf(buffer, (byte)'\n');

                var messageEnd = Array.LastIndexOf(buffer, (byte)3);


                var fullPath = Encoding.Default.GetString(buffer, 0, fullPathEnd).Trim().ToLower();


                var message = new ArraySegment<byte>(buffer, fullPathEnd + 1, messageEnd - fullPathEnd - 1);

                var serviceAndRoute = GetServiceAndRoute(fullPath);

                Console.WriteLine(fullPath);


                var response = _controllers[serviceAndRoute.Key].CallRoute(serviceAndRoute.Value, message);
                if (response != null)
                {
                    if (response.GetType() == typeof(string))
                        await clientSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(response), SocketFlags.None);
                    else if (response.GetType() == typeof(int))
                        await clientSocket.SendAsync(BitConverter.GetBytes((int)response), SocketFlags.None);
                    else if (response.GetType() == typeof(float))
                        await clientSocket.SendAsync(BitConverter.GetBytes((float)response), SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                await clientSocket.SendAsync(Encoding.Default.GetBytes(e.Message + e.StackTrace), SocketFlags.None);
            }
        }

        clientSocket.Close();
        
    }

    ~ServiceHost()
    {
        
    }
}