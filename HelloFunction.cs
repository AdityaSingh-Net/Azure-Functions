using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Company.Function;

public class HelloFunction
{
    private readonly ILogger<HelloFunction> _logger;
    private static readonly List<string> todoItems = new();

    public HelloFunction(ILogger<HelloFunction> logger)
    {
        _logger = logger;
    }

    [Function("HelloFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Hi I'm Aditya and this is my first Azure Function!");
    }

    [Function("TodoFunction")]
        public async Task<HttpResponseData> TodoRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "todo")] HttpRequestData req)
        {
            _logger.LogInformation("TodoFunction processed a request.");
            var response = req.CreateResponse();

            if (req.Method == "POST")
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                try
                {
                    var jsonDoc = JsonDocument.Parse(requestBody);
                    if (!jsonDoc.RootElement.TryGetProperty("item", out var itemProp))
                    {
                        response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        await response.WriteStringAsync("Please pass an 'item' property in the JSON body");
                        return response;
                    }
                    string newItem = itemProp.GetString();
                    todoItems.Add(newItem);
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    await response.WriteStringAsync(JsonSerializer.Serialize(new { message = $"Added: {newItem}", currentList = todoItems }));
                    return response;
                }
                catch (JsonException)
                {
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Invalid JSON");
                    return response;
                }
            }
            else if (req.Method == "GET")
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.WriteStringAsync(JsonSerializer.Serialize(todoItems));
                return response;
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.MethodNotAllowed;
                return response;
            }
        }



}