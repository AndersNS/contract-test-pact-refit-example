using Consumer;
using Refit;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddRefitClient<ITodoApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7010"));

var app = builder.Build();

app.MapGet("/", async (ITodoApi todoApi) => Results.Ok(await todoApi.GetTodo(1)));

app.Run();