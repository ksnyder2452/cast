using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Configuration;

string rabbitmq_server = ConfigurationManager.AppSettings["rabbitmq_home"];
rabbitmq_server = rabbitmq_server.Trim();
string rabbitmq_port = ConfigurationManager.AppSettings["rabbitmq_port"];
rabbitmq_port = rabbitmq_port.Trim();
string rabbitmq_user = ConfigurationManager.AppSettings["rabbitmq_user"];
rabbitmq_user = rabbitmq_user.Trim();
string rabbitmq_pwd = ConfigurationManager.AppSettings["rabbitmq_pwd"];
rabbitmq_pwd = rabbitmq_pwd.Trim();
string service_name = ConfigurationManager.AppSettings["service_name"];
service_name = service_name.Trim();
List<string> allTestClientUUIDs = new List<string>();
var factory = new ConnectionFactory();
factory.HostName = rabbitmq_server;
factory.Port = int.Parse(rabbitmq_port);
factory.UserName = rabbitmq_user;
factory.Password = rabbitmq_pwd;
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
Guid startmyuuid = Guid.NewGuid();
string startmyuuidAsString = startmyuuid.ToString();
string startExecutionService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'execution_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";
var body = Encoding.UTF8.GetBytes(startExecutionService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

string registerState = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
byte[] body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);


registerState = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

await channel.QueueDeclareAsync(queue: "execution_service", durable: false, exclusive: false, autoDelete: false, arguments: null);
Console.WriteLine(" [*] Waiting for messages within execution_service");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    if (message.ToUpper().StartsWith("MESSAGE FOR "))
    {
        string frameworkMessage = message.Substring(message.IndexOf(":") + 1).Trim();
        string currentUUID = message.Substring(12, message.IndexOf(":") - 12).Trim();
        Console.WriteLine("frameworkMessage = " + frameworkMessage);
        Console.WriteLine("currentUUID = " + currentUUID);
        allTestClientUUIDs.Add(currentUUID);

        var body2 = Encoding.UTF8.GetBytes(frameworkMessage);
        channel.BasicPublishAsync(exchange: string.Empty, routingKey: currentUUID, body: body2);
        Console.WriteLine($" [x] Queued message " + frameworkMessage + " for " + currentUUID);
    }
    else if (ea.BasicProperties.IsHeadersPresent())
    {
        Console.WriteLine("Received file");
        //Console.WriteLine("Header count is " + ea.BasicProperties.Headers.Count);
        var inbound_props = ea.BasicProperties;
        string queueName = Encoding.UTF8.GetString((byte[])inbound_props.Headers["serviceName"]);
        string pathName = Encoding.UTF8.GetString((byte[])inbound_props.Headers["pathName"]);
        string fileName = Encoding.UTF8.GetString((byte[])inbound_props.Headers["fileName"]);
        Console.WriteLine("pathName = " + pathName);
        Console.WriteLine("fileName = " + fileName);
        Console.WriteLine("queueName = " + queueName);
        var outbound_props = new BasicProperties();
        outbound_props.Headers = new Dictionary<string, object>();
        outbound_props.Headers.Add("pathName", pathName);
        outbound_props.Headers.Add("fileName", fileName);
        byte[] fileBytes = ea.Body.ToArray();

        channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, false, outbound_props, body: fileBytes);
    }
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

Guid stopmyuuid = Guid.NewGuid();
string stopmyuuidAsString = stopmyuuid.ToString();
string stopTestExecutionService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + stopmyuuidAsString + "', '" + startmyuuidAsString + "', 'execution_service', 'INFO', 'Stopped " + service_name + "', NOW())";
body = Encoding.UTF8.GetBytes(stopTestExecutionService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

registerState = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '" + service_name + "'";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

foreach (string testClientUUID in allTestClientUUIDs)
{
    Guid stopmyclientuuid = Guid.NewGuid();
    string stopmyclientuuidAsString = stopmyclientuuid.ToString();
    string currentclientuuid = testClientUUID.Substring(20);
    string startClientService = "insert into current_state (uuid, reference_uuid, state, event_time_dt) values('" + stopmyclientuuidAsString + "', '" + currentclientuuid + "', 'OFFLINE', NOW())";
    byte[] clientBody = Encoding.UTF8.GetBytes(startClientService);
    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: clientBody);
}
