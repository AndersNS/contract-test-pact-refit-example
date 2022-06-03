using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<TodoService>();

var app = builder.Build();

app.MapGet("/todo/{id:int}", (int id, TodoService todoService) =>
{
    var todo = todoService.GetTodo(id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

app.MapGet("/todo", (TodoService todoService) => Results.Ok(todoService.GetTodos()));

app.Run();