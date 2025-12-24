using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.IO;
/// <summary>
/// This class is used to monitor the health of various CAST Services
/// if they go offline it will update their state in the CAST backend database
/// It will also attempt to cleanup empty RabbitMQ queues (though it is not a catastrophic failure if it cannot)
/// For the queue cleanup to work the RabbitMQ Server must be installed on the same machine as this Health Service
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
/// The RabbitMQ Health Service Account pulled from app.config
/// </summary>
string rabbitmq_user = ConfigurationManager.AppSettings["rabbitmq_user"] ?? "";
rabbitmq_user = rabbitmq_user.Trim();
/// <summary>
/// The RabbitMQ Health Service Password pulled from app.config
/// </summary>
string rabbitmq_pwd = ConfigurationManager.AppSettings["rabbitmq_pwd"] ?? "";
rabbitmq_pwd = rabbitmq_pwd.Trim();
/// <summary>
/// The Health Service Name pulled from app.config
/// </summary>
string service_name = ConfigurationManager.AppSettings["service_name"] ?? "";
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
/// The MySQL Database pulled from app.config
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

var uuidList = new List<string> { };

Console.WriteLine(" Press [enter] to exit.");
while (true)
{
    ///Check if Logger Service is available. If it isn't update the status to OFFLINE
    if (await QueueExists("logger_service", rabbitmq_server))
    {

    }
    else
    {
        string currentState = "";
        using (MySqlConnection conn = new MySqlConnection(db_connect_string))
        {
            string select_framework_info = "select state from cast_state_tracker where name = 'logger_service'";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                MySqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    currentState = (string)rdr[0];
                }
                rdr.Close();
            }
        }
        if (!currentState.Equals("OFFLINE") && !currentState.Equals("UNDER CONSTRUCTION"))
        {
            string update_cast_state_tracker = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = 'logger_service'";
            updateRows(update_cast_state_tracker);
        }
    }

    ///Check if Execution Service is available. If it isn't update the status to OFFLINE
    if (await QueueExists("execution_service", rabbitmq_server))
    {

    }
    else
    {
        string currentState = "";
        using (MySqlConnection conn = new MySqlConnection(db_connect_string))
        {
            string select_framework_info = "select state from cast_state_tracker where name = 'execution_service'";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                MySqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    currentState = (string)rdr[0];
                }
                rdr.Close();
            }
        }
        if (!currentState.Equals("OFFLINE") && !currentState.Equals("UNDER CONSTRUCTION"))
        {
            string update_cast_state_tracker = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = 'execution_service'";
            updateRows(update_cast_state_tracker);
        }
    }

    ///Check if File Storage Service is available. If it isn't update the status to OFFLINE
    if (await QueueExists("file_storage_service", rabbitmq_server))
    {

    }
    else
    {
        string currentState = "";
        using (MySqlConnection conn = new MySqlConnection(db_connect_string))
        {
            string select_framework_info = "select state from cast_state_tracker where name = 'file_storage_service'";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                MySqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    currentState = (string)rdr[0];
                }
                rdr.Close();
            }
        }
        if (!currentState.Equals("OFFLINE") && !currentState.Equals("UNDER CONSTRUCTION"))
        {
            string update_cast_state_tracker = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = 'file_storage_service'";
            updateRows(update_cast_state_tracker);
        }
    }

    ///Check if Scheduler Service is available. If it isn't update the status to OFFLINE
    if (await QueueExists("scheduler_service", rabbitmq_server))
    {

    }
    else
    {
        string currentState = "";
        using (MySqlConnection conn = new MySqlConnection(db_connect_string))
        {
            string select_framework_info = "select state from cast_state_tracker where name = 'scheduler_service'";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                MySqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    currentState = (string)rdr[0];
                }
                rdr.Close();
            }
        }
        if (!currentState.Equals("OFFLINE") && !currentState.Equals("UNDER CONSTRUCTION"))
        {
            string update_cast_state_tracker = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = 'scheduler_service'";
            updateRows(update_cast_state_tracker);
        }
    }


    ///Retrieve a list of all Client Services
    uuidList.Clear();
    using (MySqlConnection conn = new MySqlConnection(db_connect_string))
    {
        string select_framework_info = "select reference_uuid from logger where message like 'Started Client Service%' and display_name NOT LIKE 'SETUP New Framework - IGNORE THIS ENTRY' order by order_in_system";
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
    ///Check if each Client Service is available. If it isn't delete the assoociated RabbitMQ queue and update the status to OFFLINE
    foreach (string currentUUID in uuidList)
    {
        Guid stateuuid = Guid.NewGuid();
        string stateuuidAsString = stateuuid.ToString();
        bool updateState = false;
        using (MySqlConnection conn = new MySqlConnection(db_connect_string))
        {
            string state = "";
            string select_framework_info = "select state, event_time_dt from state where reference_uuid = '" + currentUUID + "' order by order_in_system DESC limit 1";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                MySqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    state = (string)rdr[0];
                    state = state.ToUpper();
                    System.DateTime mySQLTime = (DateTime)rdr[1];
                    System.DateTime dotnetTime = DateTime.Now;
                    System.TimeSpan difference = dotnetTime.Subtract(mySQLTime);
                    double totalMinutesDifference = difference.TotalMinutes;
                    if ((state.StartsWith("COMPLETED ") && totalMinutesDifference > 30) || totalMinutesDifference > 720)
                    {
                        updateState = true;
                    }
                }
                rdr.Close();
            }
        }
        if (updateState)
        {
            string update_client_state_to_offline = "insert into state (uuid, reference_uuid, state, event_time_dt) values('" + stateuuidAsString + "', '" + currentUUID + "', 'OFFLINE', NOW())";
            updateRows(update_client_state_to_offline);


            string startDirectory = @"C:\Program Files\Rabbitmq Server\";
            string targetFileName = "rabbitmqctl.bat";
            string rabbitmqServerDirectory = "";

            try
            {
                string[] files = Directory.GetFiles(startDirectory, targetFileName, SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    string filePath = files[0];
                    rabbitmqServerDirectory = Path.GetDirectoryName(filePath) ?? "";
                }
                else
                {
                    Console.WriteLine($"File '{targetFileName}' not found in '{startDirectory}' or its subdirectories.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            try
            {
                Process rabbitMQCTL = new Process();
                rabbitMQCTL.StartInfo.FileName = rabbitmqServerDirectory + Path.DirectorySeparatorChar + "rabbitmqctl.bat";
                rabbitMQCTL.StartInfo.Arguments = " delete_queue client_service_" + currentUUID + " --if-empty";
                Console.WriteLine("Run rabbitmqctl.bat delete_queue client_service_" + currentUUID + " --if-empty");
                rabbitMQCTL.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("rabbitmqctl failed to run, should not be considered a catastrophic failure. " + e.Message);
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
/// Check whether a RabbitMQ queue exists
/// </summary>
async Task<bool> QueueExists(string queueName, string hostName)
{
    try
    {
        var factory = new ConnectionFactory { HostName = hostName };
        factory.UserName = rabbitmq_user;
        factory.Password = rabbitmq_pwd;
        using var connection = await factory.CreateConnectionAsync();
        using (var channel = await connection.CreateChannelAsync())
        {
            await channel.QueueDeclarePassiveAsync(queueName);
            return true;
        }
    }
    catch (Exception)
    {
        return false;
    }
}


/// <summary>
/// Update database rows using the provided SQL statement (since Logger Service may be offline)
/// </summary>
void updateRows(string updateStatement)
{
    using (MySqlConnection conn = new MySqlConnection(db_connect_string))
    {
        conn.Open();

        int rowsAffected = 0;
        using (MySqlCommand command = new MySqlCommand(updateStatement, conn))
        {
            try
            {
                rowsAffected = command.ExecuteNonQuery();
            }
            catch (MySqlException sqlE)
            {
                Console.WriteLine(sqlE.Message);
            }
        }
    }
}
