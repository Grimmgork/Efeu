using WorkflowEngineIntegration;
using Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

DefaultWorkflowFunctionInstanceFactory instanceFactory = new DefaultWorkflowFunctionInstanceFactory();

builder.Services.AddSingleton<IDefaultWorkflowActionInstanceFactory>(x => instanceFactory);
builder.Services.AddSingleton<IWorkflowActionInstanceFactory>(x => instanceFactory);
builder.Services.AddScoped<IWorkflowSignalHandler, ConsoleSignalHandler>();
builder.Services.AddHostedService<WorkflowEngineService>();

ModuleActivator activator = new ModuleActivator();
activator.RegisterServices(builder.Services);
var app = builder.Build();
activator.RegisterWorkflowFunctions(
    app.Services.GetRequiredService<IDefaultWorkflowActionInstanceFactory>(), 
    app.Services.GetRequiredService<IServiceScopeFactory>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
