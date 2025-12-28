using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.IO;

/// <summary>
/// This class encapsulates the Health Service functionality for easier testing
/// </summary>
public class HealthServiceManager
{
    private readonly string _rabbitmqServer;
    private readonly string _rabbitmqPort;
    private readonly string _rabbitmqUser;
    private readonly string _rabbitmqPwd;
    private readonly string _mysqlServer;
    private readonly string _mysqlPort;
    private readonly string _mysqlDatabase;
    private readonly string _mysqlUser;
    private readonly string _mysqlPassword;
    private readonly IConnectionFactory _connectionFactory;
    private readonly IDatabaseConnector _databaseConnector;
    private readonly IFileSystemHelper _fileSystemHelper;
    private readonly IProcessRunner _processRunner;

    public HealthServiceManager(
        string rabbitmqServer,
        string rabbitmqPort,
        string rabbitmqUser,
        string rabbitmqPwd,
        string mysqlServer,
        string mysqlPort,
        string mysqlDatabase,
        string mysqlUser,
        string mysqlPassword,
        IConnectionFactory? connectionFactory = null,
        IDatabaseConnector? databaseConnector = null,
        IFileSystemHelper? fileSystemHelper = null,
        IProcessRunner? processRunner = null)
    {
        _rabbitmqServer = rabbitmqServer;
        _rabbitmqPort = rabbitmqPort;
        _rabbitmqUser = rabbitmqUser;
        _rabbitmqPwd = rabbitmqPwd;
        _mysqlServer = mysqlServer;
        _mysqlPort = mysqlPort;
        _mysqlDatabase = mysqlDatabase;
        _mysqlUser = mysqlUser;
        _mysqlPassword = mysqlPassword;
        _connectionFactory = connectionFactory ?? new RabbitMQConnectionFactory(_rabbitmqServer, _rabbitmqUser, _rabbitmqPwd);
        _databaseConnector = databaseConnector ?? new MySqlDatabaseConnector(GetConnectionString());
        _fileSystemHelper = fileSystemHelper ?? new FileSystemHelper();
        _processRunner = processRunner ?? new ProcessRunner();
    }

    private string GetConnectionString()
    {
        return $"Server={_mysqlServer}; Database={_mysqlDatabase}; Uid={_mysqlUser}; Pwd={_mysqlPassword}; Port={_mysqlPort}";
    }

    /// <summary>
    /// Check whether a RabbitMQ queue exists
    /// </summary>
    public async Task<bool> QueueExists(string queueName)
    {
        return await _connectionFactory.CheckQueueExistsAsync(queueName);
    }

    /// <summary>
    /// Update database rows using the provided SQL statement
    /// </summary>
    public void UpdateRows(string updateStatement)
    {
        _databaseConnector.ExecuteUpdate(updateStatement);
    }

    /// <summary>
    /// Get the current state of a service
    /// </summary>
    public string GetServiceState(string serviceName)
    {
        return _databaseConnector.GetServiceState(serviceName);
    }

    /// <summary>
    /// Update service state to OFFLINE
    /// </summary>
    public void UpdateServiceOffline(string serviceName)
    {
        string sql = $"UPDATE cast_state_tracker SET state = 'OFFLINE', event_time_dt = NOW() WHERE name = '{EscapeSql(serviceName)}'";
        UpdateRows(sql);
    }

    /// <summary>
    /// Get list of client service UUIDs
    /// </summary>
    public List<string> GetClientServiceUUIDs()
    {
        return _databaseConnector.GetClientServiceUUIDs();
    }

    /// <summary>
    /// Get service state information
    /// </summary>
    public (string state, DateTime eventTime) GetServiceStateInfo(string referenceUUID)
    {
        return _databaseConnector.GetServiceStateInfo(referenceUUID);
    }

    /// <summary>
    /// Mark a service as offline in the state table
    /// </summary>
    public void MarkServiceOffline(string referenceUUID)
    {
        string stateuuid = Guid.NewGuid().ToString();
        string sql = $"INSERT INTO state (uuid, reference_uuid, state, event_time_dt) VALUES('{stateuuid}', '{EscapeSql(referenceUUID)}', 'OFFLINE', NOW())";
        UpdateRows(sql);
    }

    /// <summary>
    /// Find RabbitMQ control script directory
    /// </summary>
    public string FindRabbitMQControlDirectory(string startDirectory = @"C:\Program Files\Rabbitmq Server\")
    {
        return _fileSystemHelper.FindFile(startDirectory, "rabbitmqctl.bat");
    }

    /// <summary>
    /// Delete a RabbitMQ queue if it's empty
    /// </summary>
    public void DeleteEmptyQueue(string queueName, string rabbitmqDirectory)
    {
        try
        {
            _processRunner.Run(
                rabbitmqDirectory + Path.DirectorySeparatorChar + "rabbitmqctl.bat",
                $" delete_queue {queueName} --if-empty"
            );
        }
        catch (Exception e)
        {
            Console.WriteLine($"rabbitmqctl failed to run: {e.Message}");
        }
    }

    /// <summary>
    /// Check if a service state is stale
    /// </summary>
    public bool IsServiceStateStale(string state, DateTime eventTime, int completedThresholdMinutes = 30, int offlineThresholdMinutes = 720)
    {
        var difference = DateTime.Now.Subtract(eventTime);
        double totalMinutesDifference = difference.TotalMinutes;

        if (state.StartsWith("COMPLETED ", StringComparison.OrdinalIgnoreCase) && totalMinutesDifference > completedThresholdMinutes)
            return true;

        if (totalMinutesDifference > offlineThresholdMinutes)
            return true;

        return false;
    }

    private static string EscapeSql(string input)
    {
        return input.Replace("'", "''");
    }
}

/// <summary>
/// Interface for RabbitMQ connection operations
/// </summary>
public interface IConnectionFactory
{
    Task<bool> CheckQueueExistsAsync(string queueName);
}

/// <summary>
/// Interface for database operations
/// </summary>
public interface IDatabaseConnector
{
    void ExecuteUpdate(string updateStatement);
    string GetServiceState(string serviceName);
    List<string> GetClientServiceUUIDs();
    (string state, DateTime eventTime) GetServiceStateInfo(string referenceUUID);
}

/// <summary>
/// Interface for file system operations
/// </summary>
public interface IFileSystemHelper
{
    string FindFile(string startDirectory, string targetFileName);
}

/// <summary>
/// Interface for process operations
/// </summary>
public interface IProcessRunner
{
    void Run(string fileName, string arguments);
}

/// <summary>
/// RabbitMQ connection factory implementation
/// </summary>
public class RabbitMQConnectionFactory : IConnectionFactory
{
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;

    public RabbitMQConnectionFactory(string hostName, string userName, string password)
    {
        _hostName = hostName;
        _userName = userName;
        _password = password;
    }

    public async Task<bool> CheckQueueExistsAsync(string queueName)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = _hostName };
            factory.UserName = _userName;
            factory.Password = _password;
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
}

/// <summary>
/// MySQL database connector implementation
/// </summary>
public class MySqlDatabaseConnector : IDatabaseConnector
{
    private readonly string _connectionString;

    public MySqlDatabaseConnector(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ExecuteUpdate(string updateStatement)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            conn.Open();
            using (MySqlCommand command = new MySqlCommand(updateStatement, conn))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (MySqlException sqlE)
                {
                    Console.WriteLine($"SQL Error: {sqlE.Message}");
                }
            }
        }
    }

    public string GetServiceState(string serviceName)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            string select_framework_info = $"SELECT state FROM cast_state_tracker WHERE name = '{serviceName}'";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                using (MySqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return (string)rdr[0];
                    }
                }
            }
        }
        return string.Empty;
    }

    public List<string> GetClientServiceUUIDs()
    {
        var uuidList = new List<string>();
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            string select_framework_info = "SELECT reference_uuid FROM logger WHERE message LIKE 'Started Client Service%' AND display_name NOT LIKE 'SETUP New Framework - IGNORE THIS ENTRY' ORDER BY order_in_system";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                using (MySqlDataReader rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        uuidList.Add((string)rdr[0]);
                    }
                }
            }
        }
        return uuidList;
    }

    public (string state, DateTime eventTime) GetServiceStateInfo(string referenceUUID)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            string select_framework_info = $"SELECT state, event_time_dt FROM state WHERE reference_uuid = '{referenceUUID}' ORDER BY order_in_system DESC LIMIT 1";
            conn.Open();

            using (MySqlCommand command = new MySqlCommand(select_framework_info, conn))
            {
                using (MySqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        string state = ((string)rdr[0]).ToUpper();
                        DateTime eventTime = (DateTime)rdr[1];
                        return (state, eventTime);
                    }
                }
            }
        }
        return (string.Empty, DateTime.Now);
    }
}

/// <summary>
/// File system helper implementation
/// </summary>
public class FileSystemHelper : IFileSystemHelper
{
    public string FindFile(string startDirectory, string targetFileName)
    {
        try
        {
            string[] files = Directory.GetFiles(startDirectory, targetFileName, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return Path.GetDirectoryName(files[0]) ?? "";
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
        return "";
    }
}

/// <summary>
/// Process runner implementation
/// </summary>
public class ProcessRunner : IProcessRunner
{
    public void Run(string fileName, string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        Console.WriteLine($"Running: {fileName} {arguments}");
        process.Start();
    }
}
