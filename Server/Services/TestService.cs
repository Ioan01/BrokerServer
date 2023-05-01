using Server.Attributes;
using Server.Interfaces;

namespace Server.Services;

[Service]
internal class TestService : TestInterface
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