using Refit;

namespace Consumer;


[Headers("Accept: application/json")]
public interface ITodoApi
{
    [Get("/todo/{id}")]
    Task<TodoItem> GetTodo(int id);
}

public record TodoItem(int Id, string Title, bool Done);