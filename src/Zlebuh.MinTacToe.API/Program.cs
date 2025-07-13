
using Zlebuh.MinTacToe.API.Services;
using Zlebuh.MinTacToe.GameSerialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<SupabaseCredentials>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddScoped<IGameProxy, GameProxy>();
builder.Services.AddScoped<IDatabase, SupabaseDatabase>();
builder.Services.AddSingleton<ISerializer, JsonSerializer>();
builder.Logging.AddConsole();
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAllOrigins",
            builder => builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());
    }
    else
    {
        string allowedOrigin = builder.Configuration.GetValue<string>("AllowedOrigin")
            ?? throw new Exception("No origin specified in environment data");
        options.AddPolicy("AllowSpecificOrigins",
            builder => builder.WithOrigins(allowedOrigin)
                              .AllowAnyMethod()
                              .AllowAnyHeader());
    }
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAllOrigins");
}
else
{
    app.UseCors("AllowSpecificOrigins");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
