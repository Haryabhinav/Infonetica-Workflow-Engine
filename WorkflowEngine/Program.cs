using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

// Set up the web application builder with default configuration
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<WorkflowService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Workflow Engine API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Engine API v1"));
}

app.UseHttpsRedirection();

// Endpoints
app.MapPost("/workflows", (WorkflowService service, WorkflowDef definition) =>
{
    try
    {
        var result = service.CreateDefinition(definition);                                    // Create new workflow definition
        return Results.Created($"/workflows/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);                                                // Handle validation errors
    }
});

app.MapGet("/workflows/{id}", (WorkflowService service, string id) =>
{
    var definition = service.GetDefinition(id);                                               // Retrieve specific workflow definition
    return definition != null ? Results.Ok(definition) : Results.NotFound();
});

app.MapGet("/workflows", (WorkflowService service) =>
{
    return Results.Ok(service.ListDefinitions());                                             // List all workflow definitions
});

app.MapPost("/instances", (WorkflowService service, string definitionId) =>
{
    try
    {
        var instance = service.StartInstance(definitionId);                                   // Start new workflow instance
        return Results.Created($"/instances/{instance.Id}", instance);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);                                                // Handle invalid definition errors
    }
});

app.MapPost("/instances/{instanceId}/actions", (WorkflowService service, string instanceId, string actionId) =>
{
    try
    {
        var instance = service.ExecuteAction(instanceId, actionId);                            // Execute action on instance
        return Results.Ok(instance);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);                                                // Handle invalid action errors
    }
});

app.MapGet("/instances/{id}", (WorkflowService service, string id) =>
{
    var instance = service.GetInstance(id);                                                    // Retrieve specific workflow instance
    return instance != null ? Results.Ok(instance) : Results.NotFound();
});

app.MapGet("/instances", (WorkflowService service) =>
{
    return Results.Ok(service.ListInstances());                                                // List all workflow instances
});

app.Run();                                                                                     // Start the application