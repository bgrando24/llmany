var builder = WebApplication.CreateBuilder(args);

// Register connectors
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ILLMConnector, OpenAiConnector>();
builder.Services.AddSingleton<ILLMConnector, AnthropicConnector>();
builder.Services.AddSingleton<ILLMConnector, DeepSeekConnector>();

// Register service
builder.Services.AddScoped<IPromptService, PromptService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
