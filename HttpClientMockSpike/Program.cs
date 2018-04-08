using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMockSpike
{
    class Program
    {
        public static HttpClient client; 

        public Program()
        {
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected() // add using Moq.Protected;
                  .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()) // IExpr is for Protected.
                  .Returns((HttpRequestMessage request, CancellationToken cancellation) =>
                  {
                      var json = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                      Console.WriteLine($"Mock: Body: {json}");
                      var content = new StringContent("{\"message\":\"ok!\"}");
                      var message = new HttpResponseMessage(HttpStatusCode.OK);
                      message.Content = content;
                      return Task.FromResult(message);   // Task.FromResult(newHttpResponseMessage(HttpStatusCode.OK); cause null reference exception.
                  });

            client = new HttpClient(mock.Object);
        }
        static void Main(string[] args)
        {
            new Program().MockTesting().GetAwaiter().GetResult();
            Console.ReadLine();
        }

        public async Task MockTesting()
        {
            StringContent content = new StringContent("{\"message\": \"hello\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage result = await client.PostAsync("https://abc.com", content);
            var body = await result.Content.ReadAsStringAsync();
            Console.WriteLine($"HttpClient called: {body}");
        }
    }
}
