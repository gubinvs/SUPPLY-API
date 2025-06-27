using Microsoft.Extensions.Options;

namespace SUPPLY_API
{
    public class SomeServiceUsingToken
    {
        private readonly string _token;

        public SomeServiceUsingToken(IOptions<RuTokenSettings> options)
        {
            _token = options.Value.Token;
        }

        public void DoSomething()
        {
            Console.WriteLine("Токен: " + _token);
        }
    }
}
