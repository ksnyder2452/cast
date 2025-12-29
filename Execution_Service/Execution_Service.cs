using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Configuration;
/// <summary>
/// This class is used to handle all communications between CAST clients and the CAST backend services
/// </summary>

/// <summary>
/// The RabbitMQ Server pulled from app.config
/// </summary>
string rabbitmq_server = ConfigurationManager.AppSettings["rabbitmq_home"] ?? "";
rabbitmq_server = rabbitmq_server.Trim();
/// <summary>
/// The RabbitMQ Port pulled from app.config
/// </summary>
string rabbitmq_port = ConfigurationManager.AppSettings["rabbitmq_port"] ?? "";
rabbitmq_port = rabbitmq_port.Trim();
/// <summary>
/// The RabbitMQ Execution Account pulled from app.config
/// </summary>
string rabbitmq_user = ConfigurationManager.AppSettings["rabbitmq_user"] ?? "";
rabbitmq_user = rabbitmq_user.Trim();
/// <summary>
/// The RabbitMQ Execution password pulled from app.config
/// </summary>
string rabbitmq_pwd = ConfigurationManager.AppSettings["rabbitmq_pwd"] ?? "";
rabbitmq_pwd = rabbitmq_pwd.Trim();
/// <summary>
/// The Execution Service display name pulled from app.config
/// </summary>
string service_name = ConfigurationManager.AppSettings["service_name"] ?? "";
service_name = service_name.Trim();
/// <summary>
/// All active client IIDs
/// </summary>
List<string> allClientUUIDs = new List<string>();
/// <summary>
/// The RabbitMQ Connection Factory
/// </summary>
var factory = new ConnectionFactory();
factory.HostName = rabbitmq_server;
factory.Port = int.Parse(rabbitmq_port);
factory.UserName = rabbitmq_user;
factory.Password = rabbitmq_pwd;
/// <summary>
/// The RabbitMQ Connection
/// </summary>
using var connection = await factory.CreateConnectionAsync();
/// <summary>
/// The RabbitMQ Channel
/// </summary>
using var channel = await connection.CreateChannelAsync();
Guid startmyuuid = Guid.NewGuid();
string startmyuuidAsString = startmyuuid.ToString();
/// <summary>
/// Update the Execution Service status to ONLINE
/// </summary>
string startExecutionService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'execution_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";
var body = Encoding.UTF8.GetBytes(startExecutionService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

string registerState = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
byte[] body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);


registerState = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

/// <summary>
/// Setup the RabbitMQ Execution Service Queue
/// </summary>
await channel.QueueDeclareAsync(queue: "execution_service", durable: false, exclusive: false, autoDelete: false, arguments: null);
Console.WriteLine(" [*] Waiting for messages within execution_service");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    /// <summary>
    /// If a regular message is received add the Client ID to the list of active clients and forward the message to the appropriate client queue
    /// </summary>
    if (message.ToUpper().StartsWith("MESSAGE FOR "))
    {
        string frameworkMessage = message.Substring(message.IndexOf(":") + 1).Trim();
        string currentUUID = message.Substring(12, message.IndexOf(":") - 12).Trim();
        Console.WriteLine("frameworkMessage = " + frameworkMessage);
        Console.WriteLine("currentUUID = " + currentUUID);
        allClientUUIDs.Add(currentUUID);

        var body2 = Encoding.UTF8.GetBytes(frameworkMessage);
        channel.BasicPublishAsync(exchange: string.Empty, routingKey: currentUUID, body: body2);
        Console.WriteLine($" [x] Queued message " + frameworkMessage + " for " + currentUUID);
    }
    /// <summary>
    /// If a file message is received the save the file locally
    /// </summary>
    else if (ea.BasicProperties.IsHeadersPresent())
    {
        Console.WriteLine("Received file");
        var inbound_props = ea.BasicProperties;
        string queueName = inbound_props.Headers != null && inbound_props.Headers.TryGetValue("serviceName", out var serviceNameObj)
            ? Encoding.UTF8.GetString((byte[]?)serviceNameObj ?? [])
            : string.Empty;
        string pathName = inbound_props.Headers != null && inbound_props.Headers.TryGetValue("pathName", out var pathNameObj)
            ? Encoding.UTF8.GetString((byte[]?)pathNameObj ?? [])
            : string.Empty;
        string fileName = inbound_props.Headers != null && inbound_props.Headers.TryGetValue("fileName", out var fileNameObj)
            ? Encoding.UTF8.GetString((byte[]?)fileNameObj ?? [])
            : string.Empty;
        Console.WriteLine("pathName = " + pathName);
        Console.WriteLine("fileName = " + fileName);
        Console.WriteLine("queueName = " + queueName);
        var outbound_props = new BasicProperties();
        outbound_props.Headers = new Dictionary<string, object?>();
        outbound_props.Headers.Add("pathName", pathName);
        outbound_props.Headers.Add("fileName", fileName);
        byte[] fileBytes = ea.Body.ToArray();

        channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, false, outbound_props, body: fileBytes);
    }
    /// <summary>
    /// If an INSERT message is received forward the request to the Logger Service
    /// </summary>
    else if (message.Trim().ToUpper().StartsWith("INSERT INTO "))
    {
        var body2 = Encoding.UTF8.GetBytes(message);
        channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);
    }
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("execution_service", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit");
Console.ReadLine();

/// <summary>
/// Set the Execution Service status to OFFLINE
/// </summary>
Guid stopmyuuid = Guid.NewGuid();
string stopmyuuidAsString = stopmyuuid.ToString();
string stopExecutionService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + stopmyuuidAsString + "', '" + startmyuuidAsString + "', 'execution_service', 'INFO', 'Stopped " + service_name + "', NOW())";
body = Encoding.UTF8.GetBytes(stopExecutionService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

registerState = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '" + service_name + "'";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

/// <summary>
/// Set all client statuses to OFFLINE (since they are all dependent on the Execution Service)
/// </summary>
foreach (string clientUUID in allClientUUIDs)
{
    Guid stopmyclientuuid = Guid.NewGuid();
    string stopmyclientuuidAsString = stopmyclientuuid.ToString();
    string currentclientuuid = clientUUID.Substring(20);
    string startClientService = "insert into state (uuid, reference_uuid, state, event_time_dt) values('" + stopmyclientuuidAsString + "', '" + currentclientuuid + "', 'OFFLINE', NOW())";
    byte[] clientBody = Encoding.UTF8.GetBytes(startClientService);
    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: clientBody);
}

