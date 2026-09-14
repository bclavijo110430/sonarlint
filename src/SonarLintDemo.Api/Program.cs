using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Registrar conexion SQLite (solo para propositos demostrativos)
builder.Services.AddSingleton(_ =>
{
    var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = "CREATE TABLE Items (Id INTEGER PRIMARY KEY, Name TEXT); INSERT INTO Items (Name) VALUES ('Apple'), ('Banana'), ('Cherry');";
    command.ExecuteNonQuery();

    return connection;
});

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

public partial class Program { }
