using Azure.Storage.Blobs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
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
service_name = service_name.Trim();
var factory = new ConnectionFactory();
factory.UserName = rabbitmq_user;
factory.Password = rabbitmq_pwd;
factory.HostName = rabbitmq_server;
factory.Port = int.Parse(rabbitmq_port);
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
Guid startmyuuid = Guid.NewGuid();
string startmyuuidAsString = startmyuuid.ToString();
string startFileStorageService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'file_storage_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";
var body = Encoding.UTF8.GetBytes(startFileStorageService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

string registerState = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
byte[] body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

registerState = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

string rootDir = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "temp" + Path.DirectorySeparatorChar;
Directory.CreateDirectory(rootDir + "inbound_queue");
Directory.CreateDirectory(rootDir + "outbound_queue");
Directory.CreateDirectory(rootDir + "working_queue");



await channel.QueueDeclareAsync(queue: "file_storage_service", durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine(" [*] Waiting for files within file_storage_service.");
string pathName = "";
string fileName = "";
string originator = "";
string type = "";
string message = "";

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    string message = Encoding.UTF8.GetString(body);
    if (message.ToUpper().StartsWith("SIMULATE TEST RUN FOR LOCAL FILE "))
    {
        string localPathName = message.Substring(message.ToUpper().IndexOf("FILE ") + 5);
        localPathName = localPathName.Substring(0, localPathName.ToUpper().IndexOf(" WITH REMOTE REFERENCE "));
        localPathName = localPathName.Substring(0, localPathName.LastIndexOf(Path.DirectorySeparatorChar));
        string remotePathName = message.Substring(message.ToUpper().IndexOf(" WITH REMOTE REFERENCE ") + 23);
        remotePathName = remotePathName.Substring(0, remotePathName.ToUpper().IndexOf(" FOR CLIENT "));
        string fileName = remotePathName.Substring(remotePathName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        remotePathName = remotePathName.Substring(0, remotePathName.LastIndexOf(Path.DirectorySeparatorChar));
        string serviceName = message.Substring(message.ToUpper().IndexOf(" FOR CLIENT ") + 12);
        localPathName = localPathName + Path.DirectorySeparatorChar;
        remotePathName = remotePathName + Path.DirectorySeparatorChar;
        var props = new BasicProperties();
        props.Headers = new Dictionary<string, object>();
        props.Headers.Add("pathName", remotePathName);
        props.Headers.Add("fileName", fileName);
        props.Headers.Add("serviceName", serviceName);
        Console.WriteLine("Header count is " + props.Headers.Count);
        Console.WriteLine("pathName = " + props.Headers["pathName"]);
        Console.WriteLine("fileName = " + props.Headers["fileName"]);
        Console.WriteLine("serviceName = " + props.Headers["serviceName"]);
        byte[] fileBytes = File.ReadAllBytes(localPathName + fileName);
        channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", false, props, body: fileBytes);
        Console.WriteLine("Published file to execution_service");
        File.Delete(localPathName + fileName);
        Directory.Delete(localPathName);
    }
    else
    {
        var fileReference = ea.BasicProperties.Headers;
        pathName = Encoding.UTF8.GetString((byte[])fileReference["pathName"]);
        fileName = Encoding.UTF8.GetString((byte[])fileReference["fileName"]);
        if (!pathName.EndsWith(Path.DirectorySeparatorChar))
        {
            pathName = pathName + Path.DirectorySeparatorChar;
        }
        originator = Encoding.UTF8.GetString((byte[])fileReference["originator"]);
        type = Encoding.UTF8.GetString((byte[])fileReference["type"]);
        message = Encoding.UTF8.GetString((byte[])fileReference["message"]);
        Console.WriteLine("pathName = " + pathName);
        Console.WriteLine("fileName = " + fileName);
        Console.WriteLine("Full directory path = " + rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName);
        if (!System.IO.Directory.Exists(rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName))
        {
            System.IO.Directory.CreateDirectory(rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName);
        }
        File.WriteAllBytes(rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName + fileName, body);
        if (fileName.EndsWith(".zip"))
        {
            Console.WriteLine("Unzip file " + fileName);
            System.IO.Compression.ZipFile.ExtractToDirectory(rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName + fileName, rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName + Path.DirectorySeparatorChar, true);
            File.Delete(rootDir + "inbound_queue" + Path.DirectorySeparatorChar + pathName + fileName);
        }
        Guid receivedfilemyuuid = Guid.NewGuid();
        string receivedfilemyuuidAsString = receivedfilemyuuid.ToString();
        string receivedFile = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + receivedfilemyuuidAsString + "', '" + startmyuuidAsString + "', '" + originator + "', '" + type + "', '" + message + "', NOW())";
        byte[] recordFileReceived = Encoding.UTF8.GetBytes(receivedFile);


        //Async complications
        channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: recordFileReceived);
    }
    return System.Threading.Tasks.Task.CompletedTask;
};

await channel.BasicConsumeAsync("file_storage_service", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

Guid stopmyuuid = Guid.NewGuid();
string stopmyuuidAsString = stopmyuuid.ToString();
string stopFileStorageService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + stopmyuuidAsString + "', '" + startmyuuidAsString + "', 'file_storage_service', 'INFO', 'Stopped " + service_name + "', NOW())";
body = Encoding.UTF8.GetBytes(stopFileStorageService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

registerState = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '" + service_name + "'";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);
