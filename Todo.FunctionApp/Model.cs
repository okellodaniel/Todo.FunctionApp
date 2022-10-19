using System;

namespace Todo.FunctionApp;

public class ToDo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public string TaskDescription { get; set; }
    public bool IsCompleted { get; set; }

}

public class ToDoCreateModel
{
    public string TaskDescription { get; set; }
}

public class TodoUpdateModel
{
    public string TaskDescription { get; set; }
    public bool IsCompleted { get; set; }
}