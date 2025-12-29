using System.Text;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
/// <summary>
/// The RabbitMQ Server pulled from appsettings.json
/// </summary>
string rabbitmq_home = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_home"];
/// <summary>
/// The RabbitMQ Port pulled from appsettings.json
/// </summary>
string rabbitmq_port = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_port"];
/// <summary>
/// The RabbitMQ UI Account pulled from appsettings.json
/// </summary>
string rabbitmq_user = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_user"];
/// <summary>
/// The RabbitMQ UI password pulled from appsettings.json
/// </summary>
string rabbitmq_pwd = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["rabbitmq_pwd"];

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();



/// <summary>
/// The RabbitMQ Connection Factory
/// </summary>
ConnectionFactory factory = new ConnectionFactory();
factory.HostName = rabbitmq_home;
factory.Port = int.Parse(rabbitmq_port);
factory.UserName = rabbitmq_user;
factory.Password = rabbitmq_pwd;
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

///Start Client Application
app.MapPost("/api/start_client", (string id) =>
{
    string message = "message for " + id + ": local: action: start run";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
    return Results.Ok("Client application started.");
});

///Stop Client Application
app.MapPost("/api/stop_client", (string id) =>
{
    string message = "message for " + id + ": local: action: stop run";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
    return Results.Ok("Client application stopped.");
});

///Pause Client Application
app.MapPost("/api/pause_client", (string id) =>
{
    string message = "message for " + id + ": local: action: pause run";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
    return Results.Ok("Client application paused.");
});

///Resume Client Application
app.MapPost("/api/resume_client", (string id) =>
{
    string message = "message for " + id + ": local: action: resume run";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
    return Results.Ok("Client application resumed.");
});

///Abort Client Application
app.MapPost("/api/abort_client", (string id) =>
{
    string message = "message for " + id + ": local: action: abort run";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
    return Results.Ok("Client application aborted.");
});

///Restart Client Application
app.MapPost("/api/restart_client", (string id) =>
{
    string message = "message for " + id + ": local: action: restart run";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body);
    return Results.Ok("Client application restarted.");
});

app.Run();

