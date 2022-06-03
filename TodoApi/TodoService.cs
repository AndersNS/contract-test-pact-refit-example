namespace TodoApi;

public class TodoService
{
    private List<TodoModel> _todos = new()
    {
        new(Id: 1, Title: "Get this API finished", Done: false),
        new(Id: 2, Title: "Be cool", Done: true),
    };

    public ICollection<TodoModel> GetTodos()
    {
        return _todos;
    }

    public TodoModel? GetTodo(int id)
    {
        return _todos.FirstOrDefault(t => t.Id == id);
    }
}