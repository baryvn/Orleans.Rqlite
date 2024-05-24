using Microsoft.Extensions.Logging;
namespace TestGrain
{
    public class HelloGrain : Grain, IHelloGrain
    {
        private readonly ILogger _logger;

        public HelloGrain(ILogger<HelloGrain> logger) => _logger = logger;

        ValueTask<string> IHelloGrain.SayHello(string greeting)
        {
            _logger.LogInformation("""
            SayHello message received: greeting = "{Greeting}"
            """,
                greeting);

            return ValueTask.FromResult($"""

            Client said: "{greeting}", so HelloGrain says: Hello!
            """);
        }
    }
}
