using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactNet;
using PactNet.Infrastructure.Outputters;
using Refit;
using Xunit.Abstractions;

namespace Consumer.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public async Task Test1()
    {
        var pact = Pact.V3("API Consumer", "TODO Api", new PactConfig
        {
            PactDir = @"..\..\..\pacts", // Save pact to project dir
            Outputters = new []
            {
                new XUnitOutput(_output)
            },
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
                    if (attr is HeaderAttribute headerAttribute)
                    {
                        var s = headerAttribute.Header.Split(":");
                        headers.Add(s[0], s[1]);
                    }
                }
            }

            var methods = client.GetMethods();

            foreach (var method in methods)
            {
                var returnType = method.ReturnType;
                if (returnType.BaseType?.FullName == (typeof(Task).FullName))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }

                var parameters = method.GetParameters();
                var httpMethodAttr =
                    method.GetCustomAttributes(true).FirstOrDefault(a => a is HttpMethodAttribute) as
                        HttpMethodAttribute;
                if (httpMethodAttr is null) continue;
                var path = ReplaceParams(httpMethodAttr.Path, parameters);
                
                pactBuilder
                    .UponReceiving($"A {httpMethodAttr.Method.Method} request to retrieve {returnType.Name}")
                    .Given("There is something matching all params") // TODO Formulate this better
                    .WithRequest(httpMethodAttr.Method, path)
                    .WithHeader("Accept", "application/json") // TODO Use the dictionary
                    .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(CreateDefaultObject(returnType));
            }
        }

        await pactBuilder.VerifyAsync(async ctx =>
        {
            var gitHubApi =
                RestService.For<ITodoApi>(ctx.MockServerUri
                    .ToString()); // Create a new client that points to the mockserver
            var todo = await gitHubApi.GetTodo(0); // A pact is made for each of the calls made against the mockserver

            Assert.NotNull(todo);
        });
    }

    private string ReplaceParams(string path, ParameterInfo[] parameterInfos)
    {
        var p = path.ToString();
        foreach (var info in parameterInfos)
        {
            p = Regex.Replace(p, $"{{{info.Name}}}", Activator.CreateInstance(info.ParameterType)!.ToString());
        }

        return p;
    }

    // TODO Use Bogus instead to generate these
    private object CreateDefaultObject(Type targetType)
    {
        var obj = Activator.CreateInstance(targetType)!;

        var props = targetType.GetProperties();

        foreach (var property in props)
        {
            if (property.PropertyType == typeof(String))
            {
                property.SetValue(obj, "default string");
            }
            else
            {
                var instance = Activator.CreateInstance(property.PropertyType);
                property.SetValue(obj, instance);
            }
        }

        return obj;
    }
}

// Needed for the PactServer to output to console (xunit doesnt capture console by default)
public class XUnitOutput : IOutput
{
    private readonly ITestOutputHelper _output;

    public XUnitOutput(ITestOutputHelper output)
    {
        _output = output;
    }

    public void WriteLine(string line)
    {
        _output.WriteLine(line);
    }
}