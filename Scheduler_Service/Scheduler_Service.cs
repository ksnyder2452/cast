using System.Configuration;
using RabbitMQ.Client;
using System.Text;
using MySql.Data.MySqlClient;
/// <summary>
/// This class is used to schedule CAST Clients to run at specific times
/// </summary>

/// <summary>
/// Used for debugging purposes. Should never need to modify this variable
/// </summary>
bool readyToRun = true;
/// <summary>
/// Update the state of the Scheduler Service in the backend CAST Database
/// </summary>
bool updateServiceState = false;
/// <summary>
/// The RabbitMQ Server pulled from app.config
/// </summary>
string rabbitmq_server = ConfigurationManager.AppSettings["rabbitmq_home"] ?? "";
rabbitmq_server = rabbitmq_server.Trim();
/// <summary>
/// The RabbitMQ Port pulled from app.config
/// </summary>
string rabbitmq_port = ConfigurationManager.AppSettings["rabbitmq_port"] ?? "";
/// <summary>
/// The RabbitMQ Scheduler Account pulled from app.config
/// </summary>
rabbitmq_port = rabbitmq_port.Trim();
string rabbitmq_user = ConfigurationManager.AppSettings["rabbitmq_user"] ?? "";
/// <summary>
/// The RabbitMQ Scheduler Password pulled from app.config
/// </summary>
rabbitmq_user = rabbitmq_user.Trim();
string rabbitmq_pwd = ConfigurationManager.AppSettings["rabbitmq_pwd"] ?? "";
rabbitmq_pwd = rabbitmq_pwd.Trim();
/// <summary>
/// The Scheduler Service name displayed in the Controller UI
/// </summary>
string service_name = ConfigurationManager.AppSettings["service_name"] ?? "";
service_name = service_name.Trim();
service_name = service_name.Trim();
/// <summary>
/// The MySQL Server pulled from app.config
/// </summary>
string mysql_Server = ConfigurationManager.AppSettings["mysql_Server"] ?? "";
mysql_Server = mysql_Server.Trim();
/// <summary>
/// The MySQL Port pulled from app.config
/// </summary>
string mysql_Port = ConfigurationManager.AppSettings["mysql_Port"] ?? "";
mysql_Port = mysql_Port.Trim();
/// <summary>
/// The MySQL Databsae pulled from app.config
/// </summary>
string mysql_Database = ConfigurationManager.AppSettings["mysql_Database"] ?? "";
mysql_Database = mysql_Database.Trim();
/// <summary>
/// The MySQL Account pulled from app.config
/// </summary>
string mysql_User = ConfigurationManager.AppSettings["mysql_User"] ?? "";
mysql_User = mysql_User.Trim();
/// <summary>
/// The MySQL Password pulled from app.config
/// </summary>
string mysql_Password = ConfigurationManager.AppSettings["mysql_Password"] ?? "";
mysql_Password = mysql_Password.Trim();
string db_connect_string = "Server=" + mysql_Server + "; Database=" + mysql_Database + "; Uid=" + mysql_User + "; Pwd=" + mysql_Password + "; Port=" + mysql_Port;
/// <summary>
/// Contains all UUIDs of registered CAST clients
/// </summary>
var uuidList = new List<string> { };
/// <summary>
/// Contains all Scheduled CAST clients
/// </summary>
var scheduledClientList = new List<string> { };
/// <summary>
/// Contains all Scheduled times
/// </summary>
var scheduledClientTime = new List<DateTime> { };
/// <summary>
/// Contains all Scheduled CAST UUIDs
/// </summary>
var scheduledUUIDList = new List<string> { };

/// <summary>
/// The RabbitMQ Connection Factory
/// </summary>
var factory = new ConnectionFactory();
factory.UserName = rabbitmq_user;
factory.Password = rabbitmq_pwd;
factory.HostName = rabbitmq_server;
factory.Port = int.Parse(rabbitmq_port);
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
/// Register the Scheduler Service with the CAST backend database
/// </summary>
string startFileStorageService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'scheduler_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";
var body = Encoding.UTF8.GetBytes(startFileStorageService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

if (!updateServiceState)
{
    string registerState = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
    byte[] body2 = Encoding.UTF8.GetBytes(registerState);
    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

    registerState = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";
    body2 = Encoding.UTF8.GetBytes(registerState);
    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);
    updateServiceState = true;
}

Console.WriteLine(" Press [enter] to exit");
///Continually check for new Messages
while (true)
{
    if (readyToRun)
    {
        ///Build the clist of registered CAST clients
        uuidList.Clear();
        using (MySqlConnection conn = new MySqlConnection(db_connect_string))
        {
            string select_framework_info = "select reference_uuid from logger where message like 'Started Client Service%' and display_name NOT LIKE 'SETUP New Client - IGNORE THIS ENTRY' order by order_in_system";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                MySqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    uuidList.Add((string)rdr[0]);
                }
                rdr.Close();
            }
        }

        scheduledClientList.Clear();
        scheduledClientTime.Clear();
        scheduledUUIDList.Clear();
        foreach (string currentUUID in uuidList)
        {
            using (MySqlConnection conn = new MySqlConnection(db_connect_string))
            {
                string select_framework_info = "select reference_uuid, scheduled_time, uuid from state where reference_uuid = '" + currentUUID + "' and state = 'SCHEDULED'";
                conn.Open();

                using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
                {
                    MySqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        scheduledClientList.Add((string)rdr[0]);
                        scheduledClientTime.Add((DateTime)rdr[1]);
                        scheduledUUIDList.Add((string)rdr[2]);
                        Console.WriteLine("Client " + (string)rdr[0] + " is scheduled to run at " + rdr[1]);
                    }
                    rdr.Close();
                }
            }
        }

        ///Check for any clients with a seconds difference greater than 1 second past the current time. If any are found send a StartRun message to the Execution Service
        for (int counter = 0; counter < scheduledClientList.Count; counter++)
        {
            System.DateTime mySQLTime = scheduledClientTime[counter];
            System.DateTime dotnetTime = DateTime.Now;
            System.TimeSpan difference = dotnetTime.Subtract(mySQLTime);
            double totalSecondsDifference = difference.TotalSeconds;
            Console.WriteLine("Client " + scheduledClientList[counter] + " should start in " + totalSecondsDifference + " seconds");
            if (totalSecondsDifference > 1)
            {
                string message = "message for client_service_" + scheduledClientList[counter] + ": local: action: start run";
                var body3 = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "execution_service", body: body3);

                string cleanupSchedule = "delete ignore from state where uuid = '" + scheduledUUIDList[counter] + "'";
                byte[] body4 = Encoding.UTF8.GetBytes(cleanupSchedule);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body4);

            }
        }
    }
    if (Console.KeyAvailable)
    {
        Console.ReadKey(true);
        break;
    }
    Thread.Sleep(30000);
}


/// <summary>
/// Set the Execution Service status to OFFLINE
/// </summary>
Guid stopmyuuid = Guid.NewGuid();
string stopmyuuidAsString = stopmyuuid.ToString();
string stopExecutionService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + stopmyuuidAsString + "', '" + startmyuuidAsString + "', 'scheduler_service', 'INFO', 'Stopped " + service_name + "', NOW())";
body = Encoding.UTF8.GetBytes(stopExecutionService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

string registerState2 = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '" + service_name + "'";
byte[] body5 = Encoding.UTF8.GetBytes(registerState2);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body5);

