
using Zlebuh.MinTacToe.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<SupabaseCredentials>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddScoped<IGameProxy, GameProxy>();
builder.Services.AddScoped<IDatabase, SupabaseDatabase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
