using Microsoft.WindowsAzure.Storage.Table;
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

public class TodoTableEntity : TableEntity
{
    public string TaskDesciption { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedTime { get; set; }
}

public static class Mappings
{
    public static TodoTableEntity ToTableEntity(this ToDo todo)
    {
        return new TodoTableEntity
        {
            PartitionKey = "TODO",
            RowKey = todo.Id,
            CreatedTime = todo.CreatedTime,
            TaskDesciption = todo.TaskDescription,
            IsCompleted = todo.IsCompleted,
        };
    }

    public static ToDo ToToDo(this TodoTableEntity todo)
    {
        return new ToDo
        {
            Id = todo.RowKey,
            CreatedTime = todo.CreatedTime,
            TaskDescription = todo.TaskDesciption,
            IsCompleted = todo.IsCompleted
        };
    }
}