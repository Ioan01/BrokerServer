# Remote Method Invocation Project - Server Side
Client-Side portion available [here](https://github.com/Ioan01/BrokerClient)
## Overview
- A simple implementation of a service-host allowing clients to invoke functions remotely
- Utilizes reflection to route the packet to the appropriate service and call the desired function in the service
- Uses a dispatcher to register as a service provider with a unique string identifier. 

## Usage
- Create a new Service Host, and add all services
``
var host = new ServiceHost();
``
This is also the interface used by the client application to invoke these methods
```csharp
interface TestInterface
{
    public void A();

    public string B();

    public string C(int a, float b, string c);
}
```
```csharp
[Service]
class TestService : TestInterface
{
    private int called = 0;


    [Route]
    public void A()
    {
        Console.WriteLine("A called");
    }

    [Route]
    public string B()
    {
        return $"B called {called++}";
    }

    [Route]
    public string C(int a, float b, string c)
    {
        return a.ToString() + b + c;
    }
}
```

``
host.AddService<TestService>(Lifespan.Transient);
``

- Services following the naming convention <x>Service will be added to the service host as the service <x>, hence TestService will be registered as test

- The lifespan indicates how the service host will handle instances of this service. If set as Transient, it will create a new instance of the service on each service invocation, otherwise, for Singleton, it will create a single instance used for every subsequent call.

- Upon adding a service to the service host, using reflection, the service's functions annotated with [Route] will be added as callable functions.
- Connect to the dispatcher and register this server as a service host with an service host name
``
        await host.Initialize("http://127.0.0.1:8000/registerHost", "test");
``
- Start the endless loop handling incoming connections
``
await host.Start();
``

## Details 
- Remote method invocations are done via TCP, using the following packet structure 
```
<serviceName>/<serviceFunction>\n
<messageBytes ended by ETX ( ascii code 3)> 
```
- If packets contain strings, which are variable in length, they must be ended by null
- Example
``
public void A(); 
``
```
test/a
ETX
```
``
public string C(int a, float b, string c);
``
```
test/c
abcdaaaathis is a string\0ETX
```
- Since a and b will be the concatenated bit values of the corresponding ascii characters, while c will become 'this is a string'
- In both cases, the packets will be forwarded to the service "test", from which the a and b functions will be dynamically invoked with the unmarshalled arguments
- After the dynamic invokation, the return value (if applicable) will be marshalled and sent as a TCP packet via the opened socket




