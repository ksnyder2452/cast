using System.Configuration;
using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Diagnostics.Metrics;

bool readyToRun = true;

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

string mysql_Server = ConfigurationManager.AppSettings["mysql_Server"];
mysql_Server = mysql_Server.Trim();
string mysql_Port = ConfigurationManager.AppSettings["mysql_Port"];
mysql_Port = mysql_Port.Trim();
string mysql_Database = ConfigurationManager.AppSettings["mysql_Database"];
mysql_Database = mysql_Database.Trim();
string mysql_User = ConfigurationManager.AppSettings["mysql_User"];
mysql_User = mysql_User.Trim();
string mysql_Password = ConfigurationManager.AppSettings["mysql_Password"];
mysql_Password = mysql_Password.Trim();
string db_connect_string = "Server=" + mysql_Server + "; Database=" + mysql_Database + "; Uid=" + mysql_User + "; Pwd=" + mysql_Password + "; Port=" + mysql_Port;
var uuidList = new List<string> { };
var scheduledClientList = new List<string> { };
var scheduledClientTime = new List<DateTime> { };
var scheduledUUIDList = new List<string> { };

var factory = new ConnectionFactory();
factory.UserName = rabbitmq_user;
factory.Password = rabbitmq_pwd;
factory.HostName = rabbitmq_server;
factory.Port = int.Parse(rabbitmq_port);
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
Guid startmyuuid = Guid.NewGuid();
string startmyuuidAsString = startmyuuid.ToString();
string startFileStorageService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'scheduler_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";
var body = Encoding.UTF8.GetBytes(startFileStorageService);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

string registerState = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
byte[] body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

registerState = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";
body2 = Encoding.UTF8.GetBytes(registerState);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body2);

Console.WriteLine(" Press [enter] to exit");
while (true)
{
    if (readyToRun)
    {
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
                string state = "";
                string select_framework_info = "select reference_uuid, scheduled_time, uuid from current_state where reference_uuid = '" + currentUUID + "' and state = 'SCHEDULED'";
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

                string cleanupSchedule = "delete ignore from current_state where uuid = '" + scheduledUUIDList[counter] + "'";
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(db_connect_string))
                    {
                        conn.Open();
                        using (MySqlCommand command = new MySqlCommand(cleanupSchedule, conn))
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
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
