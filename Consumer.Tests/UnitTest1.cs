using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactNet;
using Refit;
using Xunit.Abstractions;

namespace Consumer.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var pact = Pact.V3("API Consumer", "TODO Api", new PactConfig
        {
            PactDir = @"..\..\..\pacts", // Save pact to project dir
            DefaultJsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        });

        var pactBuilder = pact.UsingNativeBackend();
        
        var clients = new List<Type> { typeof(ITodoApi) };
        var headers = new Dictionary<string, string>();
        foreach (var client in clients)
        {
            var properties = client.GetProperties();
            foreach (var prop in properties)
            {
                var attrs = prop.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    var headerAttribute = attr as HeaderAttribute;
                    if (headerAttribute is not null)
                    {
                        var s = headerAttribute.Header.Split(":");
                        headers.Add(s[0], s[1]);
                    }
                }
            }

            var methods = client.GetMethods();

            foreach (var method in methods)
            {
                /*
                 * var methodname = method.
                 * var returnType = method.GetReturnType() => Unpack from Task
                 * var params = method.GetParams()
                 * var httpMethod, url = method.GetHttpMethodAttribute();
                 * pactBuilder.UponReceiving($"A {httpMethod.toUpper()} request to retrieve a {returnType}")
                 * .WithRequest(httpMethod.toEnum, $"url/{defaultValue}"
                 * WillRespond:
                 * WithStatus(DefaultOk)
                 * WithHeader(ContentType json (if header is set)) 
                 * WithJsonBody(createDefaultObject fromReturnType)
                 */
            }
        }

        pactBuilder
            .UponReceiving("A GET request to retrieve a todo")
                .Given("There is a something with id '1'")
                .WithRequest(HttpMethod.Get, "/todo/1")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    // NOTE: These properties are case sensitive!
                    Id = 1,
                    Title = "Totally",
                    Done = false
                });

        await pactBuilder.VerifyAsync(async ctx =>
        {
            var gitHubApi = RestService.For<ITodoApi>(ctx.MockServerUri.ToString()); // Create a new client that points to the mockserver
            var todo = await gitHubApi.GetTodo(1); // A pact is made for each of the calls made against the mockserver

            Assert.NotNull(todo);
        });
    }
}