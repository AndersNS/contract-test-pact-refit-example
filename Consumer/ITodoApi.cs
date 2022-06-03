using Refit;

namespace Consumer;


[Headers("Accept: application/json")]
public interface ITodoApi
{
    [Get("/todo/{id}")]
    Task<TodoItem> GetTodo(int id);
}

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool Done { get; set; }
}