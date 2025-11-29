using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Configuration;

/// <summary>
/// This class is used to handle all CRUD operations for the CAST services
/// </summary>


/// <summary>
/// The RabbitMQ Server pulled from app.config
/// </summary>
string rabbitmq_server = ConfigurationManager.AppSettings["rabbitmq_home"];
rabbitmq_server = rabbitmq_server.Trim();
/// <summary>
/// The RabbitMQ Port pulled from app.config
/// </summary>
string rabbitmq_port = ConfigurationManager.AppSettings["rabbitmq_port"];
rabbitmq_port = rabbitmq_port.Trim();
/// <summary>
/// The RabbitMQ Logger Account pulled from app.config
/// </summary>
string rabbitmq_user = ConfigurationManager.AppSettings["rabbitmq_user"];
rabbitmq_user = rabbitmq_user.Trim();
/// <summary>
/// The RabbitMQ Logger password pulled from app.config
/// </summary>
string rabbitmq_pwd = ConfigurationManager.AppSettings["rabbitmq_pwd"];
rabbitmq_pwd = rabbitmq_pwd.Trim();

/// <summary>
/// The Logger Service display name pulled from app.config
/// </summary>
string service_name = ConfigurationManager.AppSettings["service_name"];
service_name = service_name.Trim();

/// <summary>
/// The MySQL Server pulled from app.config
/// </summary>
string mysql_Server = ConfigurationManager.AppSettings["mysql_Server"];
mysql_Server = mysql_Server.Trim();
/// <summary>
/// The MySQL Port pulled from app.config
/// </summary>
string mysql_Port = ConfigurationManager.AppSettings["mysql_Port"];
mysql_Port = mysql_Port.Trim();
/// <summary>
/// The MySQL Database pulled from app.config
/// </summary>
string mysql_Database = ConfigurationManager.AppSettings["mysql_Database"];
mysql_Database = mysql_Database.Trim();
/// <summary>
/// The MySQL Account pulled from app.config
/// </summary>
string mysql_User = ConfigurationManager.AppSettings["mysql_User"];
mysql_User = mysql_User.Trim();
/// <summary>
/// The MySQL password pulled from app.config
/// </summary>
string mysql_Password = ConfigurationManager.AppSettings["mysql_Password"];
mysql_Password = mysql_Password.Trim();
string connectString = "Server=" + mysql_Server + "; Database=" + mysql_Database + "; Uid=" + mysql_User + "; Pwd=" + mysql_Password + "; Port=" + mysql_Port;

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

try
{
    using (MySqlConnection conn = new MySqlConnection(connectString))
    {
        string startLogger = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'logger_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";
        string cleanupLogger = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
        string registerLogger = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";
        conn.Open();

        /// <summary>
        /// Notify the CAST backend database that the Logger Service is ONLINE
        /// </summary>
        using (MySqlCommand command = new MySqlCommand(startLogger, conn))
        {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsAffected} row(s) inserted successfully.");
        }
        using (MySqlCommand command = new MySqlCommand(cleanupLogger, conn))
        {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsAffected} row(s) deleted successfully.");
        }
        using (MySqlCommand command = new MySqlCommand(registerLogger, conn))
        {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsAffected} row(s) inserted successfully.");
        }
    }
}
catch (MySqlException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}



/// <summary>
/// Create the Logger Service queue if it does not already exist
/// </summary>
await channel.QueueDeclareAsync(queue: "logger_service", durable: false, exclusive: false, autoDelete: false, arguments: null);


Console.WriteLine(" [*] Waiting for messages within logger_service.");

/// <summary>
/// Consume new RabbitMQ messages
/// </summary>
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var insertRecord = Encoding.UTF8.GetString(body);



    /// <summary>
    /// If the message contains an ACTION command do nothing
    /// </summary>
    if (insertRecord.Contains("ACTION"))
    {
        string action = insertRecord.Substring(insertRecord.IndexOf("'ACTION'") + 8);
        action = action.Substring(action.IndexOf("'") + 1);
        action = action.Substring(0, action.IndexOf("'"));
        Console.WriteLine("Action to be run locally: " + action);
    }
    else
    {
        /// <summary>
        /// Record the new request into the CAST backend database
        /// </summary>

        if (!insertRecord.EndsWith(";"))
        {
            insertRecord = insertRecord + ";";
        }


        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectString))
            {
                conn.Open();
                Console.WriteLine("SQL Statement is " + insertRecord);
                bool initialAttemptFailed = false;

                using (MySqlCommand command = new MySqlCommand(insertRecord, conn))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0 && !insertRecord.ToUpper().Contains(" IGNORE "))
                    {
                        initialAttemptFailed = true;
                    }
                    else
                    {
                        Console.WriteLine($"{rowsAffected} row(s) changed successfully.");
                    }
                }
                if (initialAttemptFailed)
                {
                    Thread.Sleep(5000);
                    using (MySqlCommand command = new MySqlCommand(insertRecord, conn))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("No changes were made with SQL statement " + insertRecord);
                        }
                        else
                        {
                            Console.WriteLine($"{rowsAffected} row(s) changed successfully.");
                        }
                    }

                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("logger_service", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
Guid stopmyuuid = Guid.NewGuid();
string stopmyuuidAsString = stopmyuuid.ToString();

/// <summary>
/// Update the status of the Logger Service to OFFLINE in the CAST backend database
/// </summary>
try
{
    //using (MySqlConnection conn = new MySqlConnection(builder.ConnectionString))
    using (MySqlConnection conn = new MySqlConnection(connectString))
    {
        string stopLogger = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + stopmyuuidAsString + "', '" + startmyuuidAsString + "', 'logger_service', 'INFO', 'Stopped " + service_name + "', NOW())";
        string registerState = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '" + service_name + "'";

        conn.Open();

        using (MySqlCommand command = new MySqlCommand(stopLogger, conn))
        {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsAffected} row(s) inserted successfully.");
        }
        using (MySqlCommand command = new MySqlCommand(registerState, conn))
        {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsAffected} row(s) updated successfully.");
        }
    }
}
catch (MySqlException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
