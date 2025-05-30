var builder = WebApplication.CreateBuilder(args);

// Register connectors
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ILLMConnector, OpenAiConnector>();
builder.Services.AddSingleton<ILLMConnector, AnthropicConnector>();
builder.Services.AddSingleton<ILLMConnector, DeepSeekConnector>();
builder.Services.AddSingleton<ILLMConnector, GoogleConnector>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register service
builder.Services.AddScoped<IPromptService, PromptService>();

builder.Services.AddMemoryCache();

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

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
