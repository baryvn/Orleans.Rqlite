namespace SiloTest
{
    public interface IHelloGrain : IGrainWithIntegerKey
    {
        ValueTask<string> SayHello(string greeting);
    }
}
