using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Todo.FunctionApp;

namespace ToDoFunction;

public static class ToDoApi
{
    [FunctionName("CreateTodo")]
    public static async Task<IActionResult> CreateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req, TraceWriter log,
        [Table("todos", Connection="AzureWebJobStorage")] IAsyncCollector<TodoTableEntity> todoTable
        )
    {
        log.Info("C# a new To do item");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);

        var todo = new ToDo { TaskDescription = input.TaskDescription };

        await todoTable.AddAsync(todo.ToTableEntity());

        return new OkObjectResult(todo);
 
    }

    [FunctionName("GetTodos")]
    public static async Task<IActionResult> GetTodos(
        [HttpTrigger(AuthorizationLevel.Anonymous,"get",Route = "todo")]HttpRequest req, 
        [Table("todos", Connection="AzureWebJobStorage")] CloudTable todoTable
        ,TraceWriter log)
    {
        log.Info("Getting todo list items");
        
        var query = new TableQuery<TodoTableEntity>();
        
        var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
        
        return new OkObjectResult(segment.Select(Mappings.ToToDo));
    }

    [FunctionName("GetTodoById")]
    public static IActionResult GetTodoById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
        [Table("todos","TODO","{id}",Connection = "AzureWebJobStorage")] TodoTableEntity todo,
        TraceWriter log, string id)
    {
        log.Info("Getting todo item");
        
        if (todo == null) throw new Exception($"Item with id: {id} not found");

        return new OkObjectResult(todo.ToToDo());
    }

    [FunctionName("UpdateTodo")]
    public static async Task<IActionResult> UpdateTodo([HttpTrigger(AuthorizationLevel.Anonymous,"put",Route="todo/{id}")]HttpRequest req,string id,
        [Table("todos",Connection = "AzureWebJobStorage")] CloudTable todoTable
        ,TraceWriter log)
    {
        string reqBody = await new StreamReader(req.Body).ReadToEndAsync();

        var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(reqBody);

        var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
        var findResult = await todoTable.ExecuteAsync(findOperation);

        if (findResult.Result == null) return new NotFoundResult();

        var existingRow = (TodoTableEntity)findResult.Result;
        
        existingRow.IsCompleted = updated.IsCompleted;
        
        if (!string.IsNullOrEmpty(updated.TaskDescription)) existingRow.TaskDesciption = updated.TaskDescription;

        var replaceOperation = TableOperation.Replace(existingRow);
        
        await todoTable.ExecuteAsync(replaceOperation);

        return new OkObjectResult(existingRow.ToToDo());
    }

    [FunctionName("DeleteTodo")]
    public static async Task<IActionResult> DeleteTodo(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req, TraceWriter log,
      [Table("todos",Connection = "AzureWebJobStorage")] CloudTable todoTable
      , string id)
    {
        var deleteOperation = TableOperation.Delete(new TableEntity()
        {
            PartitionKey = "TODO",
            RowKey = id,
            ETag = "*"
        });
        try
        {
            var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
        }
        catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
        {
            return new NotFoundResult();
            
        }

        return new OkResult();
    }
}

