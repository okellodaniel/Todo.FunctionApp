using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Todo.FunctionApp;

public static class ToDoApi
{
    private static List<ToDo>items = new List<ToDo>();

    [FunctionName("CreateTodo")]
    public static async Task<IActionResult> CreateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req, TraceWriter log)
    {
        log.Info("C# a new To do item");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);

        var todo = new ToDo { TaskDescription = input.TaskDescription };

        items.Add(todo);

        return new OkObjectResult(todo);
 
    }

    [FunctionName("GetTodos")]
    public static IActionResult GetTodos(
        [HttpTrigger(AuthorizationLevel.Anonymous,"get",Route = "todo")]HttpRequest req, TraceWriter log)
    {
        log.Info("Getting todo list items");

        return new OkObjectResult(items);
    }

    [FunctionName("GetTodoById")]
    public static IActionResult GetTodoById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req, TraceWriter log, string id)
    {
        log.Info("Getting todo item");

        var item = items.FirstOrDefault(t => t.Id == id);

        if (item == null) throw new Exception($"Item with id: {id} not found");

        return new OkObjectResult(item);
    }

    [FunctionName("UpdateTodo")]
    public static async Task<IActionResult> UpdateTodo([HttpTrigger(AuthorizationLevel.Anonymous,"put",Route="todo/{id}")]HttpRequest req,string id,TraceWriter log)
    {
        log.Info("Updating todo item");
        var todo = items.FirstOrDefault(t => t.Id == id);

        if (todo == null) return new NotFoundResult();

        string reqBody = await new StreamReader(req.Body).ReadToEndAsync();

        var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(reqBody);

        if (!string.IsNullOrEmpty(updated.TaskDescription))
        {
            todo.TaskDescription = updated.TaskDescription;
        }

        return new OkObjectResult(todo);
    }

    [FunctionName("DeleteTodo")]
    public static IActionResult DeleteTodo(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req, TraceWriter log, string id)
    {
        log.Info("Deleting todo item");

        var item = items.FirstOrDefault(t => t.Id == id);

        if (item == null) throw new Exception($"Item with id: {id} not found");

        items.Remove(item);

        return new OkResult();
    }
}

