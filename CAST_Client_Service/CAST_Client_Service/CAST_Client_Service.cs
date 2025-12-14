using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO.Compression;

namespace CAST_Client_Service;

/// <summary>
/// This class is the integration point into the Centralized Automation for Software Tools (CAST)
/// Add the DLL as a reference file to your custom framework
/// The key methods to call within your custom framework are
///    1. updateFrameworkFunctionality
///    2. updateState
///    3. updateResult
///    4. uploadResultFolder
/// The key fields to track CAST Action requests are
///    1. _startRun
///    2. _stopRun
///    3. _pauseRun
///    4. _resumeRun
///    5. _abortRun
/// </summary>
public static class CAST_Client_Service
{
    static Guid startmyuuid = Guid.NewGuid();

    /// <summary>
    /// startmyuuidAsString is the Client UUID
    /// Used to send Action Requests from the REST Listener to your framework
    /// </summary>
    static string startmyuuidAsString = startmyuuid.ToString();
    /// <summary>
    /// currentUUID is the RabbitMQ Queue name
    /// </summary>
    public static string currentUUID = "client_service_" + startmyuuidAsString;
    /// <summary>
    /// rootDir is the root directory for your framework
    /// </summary>
    static string rootDir = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "simulator" + Path.DirectorySeparatorChar;
    /// <summary>
    /// downloadQueueDir contains files downloaded from the File Storage Service
    /// </summary>
    static string downloadQueueDir = rootDir + "download_queue" + Path.DirectorySeparatorChar;
    /// <summary>
    /// uploadQueueDir contains files to be uploaded to the File Storage Service
    /// </summary>
    static string uploadQueueDir = rootDir + "upload_queue" + Path.DirectorySeparatorChar;
    /// <summary>
    /// _stopRun contains the current state of the STOP Action on the CAST Server
    /// </summary>
    static public Boolean _stopRun = false;
    /// <summary>
    /// _pauseRun contains the current state of the PAUSE Action on the CAST Server
    /// </summary>
    static public Boolean _pauseRun = false;
    /// <summary>
    /// _startRun contains the current state of the START Action on the CAST Server
    /// </summary>
    static public Boolean _startRun = false;
    /// <summary>
    /// _resumeRun contains the current state of the RESUME Action on the CAST Server
    /// </summary>
    static public Boolean _resumeRun = false;
    /// <summary>
    /// _abortRun contains the current state of the ABORT Action on the CAST Server
    /// </summary>
    static public Boolean _abortRun = false;
    /// <summary>
    /// _customAction needs work
    /// </summary>
    static public Boolean _customAction = false;
    /// <summary>
    /// customActionList is used to track custom actions
    /// </summary>
    static public List<string> customActionList = new List<string>();
    /// <summary>
    /// customActionStateList is used to track the state of custom actions
    /// </summary>
    static public List<bool> customActionStateList = new List<bool>();
    /// <summary>
    /// reloadUUID is intended to provide functionality around restarting the previous Run
    /// </summary>
    static public Boolean reloadUUID = false;
    /// <summary>
    /// tempLog is used for debugging purposes
    /// </summary>
    static string tempLog = "." + Path.DirectorySeparatorChar + "temp.log";
    /// <summary>
    /// inDebugMode is used to dump data to temp.log
    /// </summary>
    static Boolean inDebugMode = true;
    /// <summary>
    /// rabbitmq_hostname references the RabbitMQ Server. Your Administrator will provide this value
    /// </summary>
    static string rabbitmq_hostname = "";
    /// <summary>
    /// rabbitmq_port references the RabbitMQ Server. Your Administrator will provide this value
    /// </summary>
    static string rabbitmq_port = "";
    /// <summary>
    /// rabbitmq_user references the RabbitMQ Server. Your Administrator will provide this value
    /// </summary>
    static string rabbitmq_user = "";
    /// <summary>
    /// rabbitmq_pwd references the RabbitMQ Server. Your Administrator will provide this value
    /// </summary>
    static string rabbitmq_pwd = "";
    /// <summary>
    /// factory is used to generate the RabbitMQ Connection
    /// </summary>
    static ConnectionFactory factory;
    /// <summary>
    /// dllIsRegistered is used to track when your Client is registered with the CAST Server
    /// </summary>
    static bool dllIsRegistered = false;

    /// <summary>
    /// Setup and register the CAST Client environment and listen for Action requests. This method gets called when the DLL is loaded
    /// </summary>
    [ModuleInitializer]
    public static async void startService()
    {
        string propertiesFileReference = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "cast.properties";
        string originalPropertiesFileReference = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cast.properties";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, $" [x] propertiesFileReference = " + propertiesFileReference + System.Environment.NewLine);
        }
        var data = new Dictionary<string, string>();
        foreach (var row in File.ReadAllLines(propertiesFileReference))
        {
            data.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));
        }
        rabbitmq_hostname = data["rabbitmq_home"];
        rabbitmq_port = data["rabbitmq_port"];
        rabbitmq_user = data["rabbitmq_user"];
        rabbitmq_pwd = data["rabbitmq_pwd"];
        factory = new ConnectionFactory();
        if (data.ContainsKey("currentUUID") && data["reloadUUID"].Equals("yes"))
        {
            currentUUID = data["currentUUID"];
            reloadUUID = true;
        }
        else if (data["reloadUUID"].Equals("yes") && !data.ContainsKey("currentUUID"))
        {
            System.IO.File.AppendAllText(originalPropertiesFileReference, "currentUUID=" + currentUUID + System.Environment.NewLine);
        }

        if (System.IO.File.Exists(tempLog))
        {
            System.IO.File.Delete(tempLog);
        }
        Directory.CreateDirectory(rootDir);
        Directory.CreateDirectory(downloadQueueDir);
        Directory.CreateDirectory(uploadQueueDir);
        factory.HostName = rabbitmq_hostname;
        factory.Port = int.Parse(rabbitmq_port);
        factory.UserName = rabbitmq_user;
        factory.Password = rabbitmq_pwd;
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        //Notify Logger Service that we are awake
        string startClientService = "";
        if (!reloadUUID)
        {
            startClientService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', '" + currentUUID + "', 'INFO', 'Started Client Service for UUID " + startmyuuidAsString + "', NOW(), 'SETUP New Framework - IGNORE THIS ENTRY')";
            dllIsRegistered = true;
        }
        byte[] body = Encoding.UTF8.GetBytes(startClientService);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
        dllIsRegistered = true;
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, $" [x] Started Client Service" + System.Environment.NewLine);
        }

        //Note that sending messages to the Framework is a Synchronous process
        await channel.QueueDeclareAsync(queue: currentUUID, durable: false, exclusive: false, autoDelete: false, arguments: null);
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, $" [x] Waiting for messages within " + currentUUID + System.Environment.NewLine);
        }
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (model, ea) =>
        {
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, $" Within consumer.ReceivedAsync for " + currentUUID + System.Environment.NewLine);
            }
            if (ea.BasicProperties.IsHeadersPresent())
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, "Trying to push down file");
                }
            }

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, $" [x] Received: {message}" + System.Environment.NewLine);
            }
            if (message.ToUpper().StartsWith("PUSH FILE: "))
            {
                var fileReference = ea.BasicProperties.Headers;
                string pathName = Encoding.UTF8.GetString((byte[])(fileReference["pathName"] ?? new byte[] { }));
                string fileName = Encoding.UTF8.GetString((byte[])(fileReference["fileName"] ?? new byte[] { }));
                if (!pathName.EndsWith(Path.DirectorySeparatorChar))
                {
                    pathName = pathName + Path.DirectorySeparatorChar;
                }
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, "pathName = " + pathName + System.Environment.NewLine);
                }
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, "fileName = " + fileName + System.Environment.NewLine);
                }
                File.WriteAllBytes(downloadQueueDir + pathName + fileName, body);
            }
            else if (message.Trim().ToUpper().EndsWith("START RUN"))
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, $" [x] Queued start message for the local DIY Framework" + System.Environment.NewLine);
                }
                _startRun = true;
                _stopRun = false;
                _pauseRun = false;
                _resumeRun = false;
                _abortRun = false;
            }
            else if (message.Trim().ToUpper().EndsWith("STOP RUN"))
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, $" [x] Queued stop message for the local DIY Framework" + System.Environment.NewLine);
                }
                _stopRun = true;
                _pauseRun = false;
                _resumeRun = false;
                _abortRun = false;
            }
            else if (message.Trim().ToUpper().EndsWith("PAUSE RUN"))
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, $" [x] Queued pause message for the local DIY Framework" + System.Environment.NewLine);
                }
                _stopRun = false;
                _pauseRun = true;
                _resumeRun = false;
                _abortRun = false;
            }
            else if (message.Trim().ToUpper().EndsWith("RESUME RUN"))
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, $" [x] Queued resume message for the local DIY Framework" + System.Environment.NewLine);
                }
                _stopRun = false;
                _pauseRun = false;
                _resumeRun = true;
                _abortRun = false;
            }
            else if (message.Trim().ToUpper().EndsWith("ABORT RUN"))
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, $" [x] Queued abort message for the local DIY Framework" + System.Environment.NewLine);
                }
                _stopRun = false;
                _pauseRun = false;
                _resumeRun = false;
                _abortRun = true;
            }
            else if (message.ToUpper().Contains("CUSTOM ACTION"))
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, $" [x] Queued custom action message for the local DIY Framework" + System.Environment.NewLine);
                }
                _customAction = true;
                for (int counter = 0; counter < customActionList.Count; counter++)
                {
                    if (customActionList[counter].Equals(message.Substring(message.IndexOf("custom action ") + 14)))
                    {
                        customActionStateList[counter] = true;
                    }
                }
            }
            else
            {
                if (inDebugMode)
                {
                    System.IO.File.AppendAllText(tempLog, "Received file" + System.Environment.NewLine);
                }
            }
            return Task.CompletedTask;
        };
        await channel.BasicConsumeAsync(currentUUID, autoAck: true, consumer: consumer);
        Thread.Sleep(Timeout.Infinite);
    }

    /// <summary>
    /// This method processes Start Action Requests from the RabbitMQ Server
    /// </summary>
    /// <param name="service_uuid"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    static public string startRun(ref string service_uuid, ref string action)
    {
        string result = "";
        if (action.ToUpper().StartsWith("ACTION: START "))
        {
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, action + " for " + service_uuid + System.Environment.NewLine);
            }
            result = "Found START action";
        }
        return result;
    }

    /// <summary>
    /// This method processes Pause Action Requests from the RabbitMQ Server
    /// </summary>
    /// <param name="service_uuid"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static string pauseRun(ref string service_uuid, ref string action)
    {
        string result = "";
        if (action.ToUpper().StartsWith("ACTION: PAUSE "))
        {
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, action + " for " + service_uuid + System.Environment.NewLine);
            }
            result = "Found PAUSE action";
            _pauseRun = true;
        }
        return result;
    }

    /// <summary>
    /// This method processes Resume Action Requests from the RabbitMQ Server
    /// </summary>
    /// <param name="service_uuid"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static string resumeRun(ref string service_uuid, ref string action)
    {
        string result = "";
        if (action.ToUpper().StartsWith("ACTION: RESUME "))
        {
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, action + " for " + service_uuid + System.Environment.NewLine);
            }
            result = "Found RESUME action";
            _resumeRun = false;
        }
        return result;
    }

    /// <summary>
    /// This method processes Abort Action Requests from the RabbitMQ Server
    /// </summary>
    /// <param name="service_uuid"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static string abortRun(ref string service_uuid, ref string action)
    {
        string result = "";
        if (action.ToUpper().StartsWith("ACTION: ABORT "))
        {
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, action + " for " + service_uuid + System.Environment.NewLine);
            }
            result = "Found ABORT action";
            _abortRun = false;
        }
        return result;
    }

    /// <summary>
    /// This method processes Custom Action Requests from the RabbitMQ Server
    /// </summary>
    /// <param name="service_uuid"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static string callCustomAction(ref string service_uuid, ref string action)
    {
        string result = "";
        if (action.ToUpper().StartsWith("ACTION: CUSTOM ACTION "))
        {
            if (inDebugMode)
            {
                System.IO.File.AppendAllText(tempLog, action + " for " + service_uuid + System.Environment.NewLine);
            }
            result = "Found CUSTOM action";
            _customAction = false;
            for (int counter = 0; counter < customActionList.Count; counter++)
            {
                if (customActionList[counter].Equals(action.Substring(action.IndexOf("custom action ") + 14)))
                {
                    customActionStateList[counter] = false;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// This method will update the CAST Service when the run completes
    /// </summary>
    static async public void stopService()
    {
        string startClientService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', '" + currentUUID + "', 'INFO', 'Stopped Client Service for UUID '" + startmyuuidAsString + "', NOW())";
        byte[] body = Encoding.UTF8.GetBytes(startClientService);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
    }

    /// <summary>
    /// This method is used to upload Zipped files to the CAST File Storage Service.
    /// Note that there is a 10mb size limit on the file
    /// </summary>
    /// <param name="pathReference"></param>
    /// <param name="fileName"></param>
    /// <param name="cleanupExistingZip"></param>
    static async public void uploadFile(string pathReference, string fileName, bool cleanupExistingZip = true)
    {
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Upload " + pathReference + fileName + System.Environment.NewLine);
        }
        string zipFileName = fileName.Substring(0, fileName.LastIndexOf(".")) + ".zip";
        if (cleanupExistingZip && System.IO.File.Exists(pathReference + zipFileName))
        {
            System.IO.File.Delete(pathReference + zipFileName);
        }

        string message = "Send file " + fileName;
        var props = new BasicProperties();
        props.Headers = new Dictionary<string, object?>();
        props.Headers.Add("pathName", currentUUID);
        props.Headers.Add("fileName", zipFileName);
        props.Headers.Add("originator", currentUUID);
        props.Headers.Add("type", "INFO");
        props.Headers.Add("message", message);

        using (var zip = ZipFile.Open(pathReference + zipFileName, ZipArchiveMode.Create))
            zip.CreateEntryFromFile(pathReference + fileName, fileName);
        byte[] fileBytes = File.ReadAllBytes(pathReference + zipFileName);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "file_storage_service", false, props, body: fileBytes);
        if (cleanupExistingZip && System.IO.File.Exists(pathReference + zipFileName))
        {
            System.IO.File.Delete(pathReference + zipFileName);
        }
    }

    /// <summary>
    /// This method is used to upload the contents of the output folder to the CAST File Storage Service
    /// </summary>
    /// <param name="pathReference"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="cleanupZip"></param>
    static async public void uploadOutputFolder(string pathReference, string workingDirectory, bool cleanupZip = true)
    {
        string relativePath = pathReference.Substring(0, pathReference.LastIndexOf(Path.DirectorySeparatorChar));
        if (!pathReference.EndsWith(Path.DirectorySeparatorChar))
        {
            relativePath = pathReference.Substring(pathReference.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            pathReference = pathReference + Path.DirectorySeparatorChar;
        }
        else
        {
            relativePath = relativePath.Substring(relativePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Upload contents of " + pathReference + System.Environment.NewLine);
        }

        string zipFileName = "current_output.zip";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "zipFileName = " + zipFileName + System.Environment.NewLine);
        }
        string zipFilePath = workingDirectory + zipFileName;
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "zipFilePath = " + zipFilePath + System.Environment.NewLine);
        }
        string message = "Send file " + zipFileName;
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, message + System.Environment.NewLine);
        }
        var props = new BasicProperties();
        props.Headers = new Dictionary<string, object?>();
        props.Headers.Add("pathName", currentUUID);
        props.Headers.Add("fileName", zipFileName);
        props.Headers.Add("originator", currentUUID);
        props.Headers.Add("type", "INFO");
        props.Headers.Add("message", message);
        ZipFile.CreateFromDirectory(pathReference, zipFilePath);

        byte[] fileBytes = File.ReadAllBytes(zipFilePath);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "file_storage_service", false, props, body: fileBytes);
        if (cleanupZip)
        {
            System.IO.File.Delete(zipFilePath);
        }
    }

    /// <summary>
    /// This method is used to update CAST with the current state of your framework
    /// </summary>
    /// <param name="state"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    static public async Task<string> updateState(string state, string color = "black")
    {
        while (!dllIsRegistered)
        {
            Thread.Sleep(1000);
        }
        if (state.Contains("'"))
        {
            state.Replace("'", "\\'");
        }
        Guid stateuuid = Guid.NewGuid();
        string stateuuidAsString = stateuuid.ToString();
        string startClientService = "insert into state (uuid, reference_uuid, state, event_time_dt, color) values('" + stateuuidAsString + "', '" + startmyuuidAsString + "', '" + state + "', NOW(), '" + color + "')";
        byte[] body = Encoding.UTF8.GetBytes(startClientService);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
        return "Queue updated";
    }

    /// <summary>
    /// This method is used to close the RabbitMQ Queue assigned to your framework
    /// </summary>
    static public async void closeQueue()
    {
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeleteAsync(currentUUID);
    }

    /// <summary>
    /// This method is used to update data on the CAST Server during your run
    /// </summary>
    /// <param name="result"></param>
    static public async void updateResult(string result)
    {
        while (!dllIsRegistered)
        {
            Thread.Sleep(1000);
        }
        if (result.Contains("'"))
        {
            result.Replace("'", "\\'");
        }
        Guid stateuuid = Guid.NewGuid();
        string stateuuidAsString = stateuuid.ToString();
        string startClientService = "insert into results (uuid, reference_uuid, result, event_time_dt) values('" + stateuuidAsString + "', '" + startmyuuidAsString + "', '" + result + "', NOW())";
        byte[] body = Encoding.UTF8.GetBytes(startClientService);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
    }

    /// <summary>
    /// This method is used to register your framework within the CAST Server
    /// </summary>
    /// <param name="startEnabled"></param>
    /// <param name="stopEnabled"></param>
    /// <param name="pauseEnabled"></param>
    /// <param name="resumeEnabled"></param>
    /// <param name="abortEnabled"></param>
    /// <param name="restartEnabled"></param>
    /// <param name="uploadResultEnabled"></param>
    /// <param name="frameworkName"></param>
    /// <param name="filterOnGroup"></param>
    /// <param name="filterOnOwner"></param>
    /// <param name="filterOnLocation"></param>
    /// <param name="filterOnKeyword"></param>
    static public async void updateFrameworkFunctionality(bool startEnabled, bool stopEnabled, bool pauseEnabled, bool resumeEnabled, bool abortEnabled, bool restartEnabled, bool uploadResultEnabled, string frameworkName, string filterOnGroup, string filterOnOwner, string filterOnLocation, string? filterOnKeyword = null)
    {
        while (!dllIsRegistered)
        {
            Thread.Sleep(1000);
        }
        Guid stateuuid = Guid.NewGuid();
        string stateuuidAsString = stateuuid.ToString();
        string start = "0";
        string stop = "0";
        string pause = "0";
        string resume = "0";
        string abort = "0";
        string uploadResult = "0";
        string restart = "0";
        if (startEnabled)
        {
            start = "1";
        }
        if (stopEnabled)
        {
            stop = "1";
        }
        if (pauseEnabled)
        {
            pause = "1";
        }
        if (resumeEnabled)
        {
            resume = "1";
        }
        if (abortEnabled)
        {
            abort = "1";
        }
        if (restartEnabled)
        {
            restart = "1";
        }
        if (uploadResultEnabled)
        {
            uploadResult = "1";
        }
        string startClientService = "insert into client_functionality (uuid, reference_uuid, start_supported, stop_supported, pause_supported, resume_supported, abort_supported, restart_supported, upload_supported, event_time_dt) values('" + stateuuidAsString + "', '" + startmyuuidAsString + "', " + start + ", " + stop + ", " + pause + ", " + resume + ", " + abort + ", " + restart + ", " + uploadResult + ", NOW())";
        byte[] body = Encoding.UTF8.GetBytes(startClientService);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

        //Update Framework Name
        if (frameworkName.Contains("'"))
        {
            frameworkName.Replace("'", "\\'");
        }
        startClientService = "update logger set display_name = '" + frameworkName + "' where reference_uuid = '" + startmyuuidAsString + "'";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Update Display Name using SQL " + startClientService + System.Environment.NewLine);
        }
        body = Encoding.UTF8.GetBytes(startClientService);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

        //Update Framework Group Filter
        if (filterOnGroup.Contains("'"))
        {
            filterOnGroup.Replace("'", "\\'");
        }
        startClientService = "update logger set filter_on_group = '" + filterOnGroup + "' where reference_uuid = '" + startmyuuidAsString + "'";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Update Filter On Group using SQL " + startClientService + System.Environment.NewLine);
        }
        body = Encoding.UTF8.GetBytes(startClientService);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

        //Update Framework Owner Filter
        if (filterOnOwner.Contains("'"))
        {
            filterOnOwner.Replace("'", "\\'");
        }
        startClientService = "update logger set filter_on_owner = '" + filterOnOwner + "' where reference_uuid = '" + startmyuuidAsString + "'";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Update Filter On Owner using SQL " + startClientService + System.Environment.NewLine);
        }
        body = Encoding.UTF8.GetBytes(startClientService);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

        //Update Framework Location Location
        if (filterOnLocation.Contains("'"))
        {
            filterOnLocation.Replace("'", "\\'");
        }
        startClientService = "update logger set filter_on_location = '" + filterOnLocation + "' where reference_uuid = '" + startmyuuidAsString + "'";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Update Filter On Location using SQL " + startClientService + System.Environment.NewLine);
        }
        body = Encoding.UTF8.GetBytes(startClientService);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);

        //Update Framework Keyword Location
        if (filterOnKeyword != null && filterOnKeyword.Contains("'"))
        {
            filterOnKeyword = filterOnKeyword.Replace("'", "\\'");
        }
        startClientService = "update logger set filter_on_keyword = '" + filterOnKeyword + "' where reference_uuid = '" + startmyuuidAsString + "'";
        if (inDebugMode)
        {
            System.IO.File.AppendAllText(tempLog, "Update Filter On Keyword using SQL " + startClientService + System.Environment.NewLine);
        }
        body = Encoding.UTF8.GetBytes(startClientService);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
    }


    /// <summary>
    /// This method is used to register custom actions
    /// </summary>
    static public async Task<string> registerAction(string actionName, string actionDescription, Boolean hideBeforeStart, Boolean hideAfterStart, Boolean hideAfterComplete, string actionIcon = "fa fa-check")
    {
        while (!dllIsRegistered)
        {
            Thread.Sleep(1000);
        }
        if (actionName.Contains("'"))
        {
            actionName.Replace("'", "\\'");
        }
        string originalActionName = actionName;
        actionName = startmyuuidAsString + "|" + actionName;
        Guid stateuuid = Guid.NewGuid();
        string stateuuidAsString = stateuuid.ToString();
        string registerAction = "insert into custom_actions (uuid, reference_uuid, name, description, icon, hide_before_start, hide_after_start, hide_after_complete, event_time_dt) values('" + stateuuidAsString + "', '" + currentUUID + "', '" + actionName + "', '" + actionDescription + "', '" + actionIcon + "', " + hideBeforeStart + ", " + hideAfterStart + ", " + hideAfterComplete + ", NOW())";
        byte[] body = Encoding.UTF8.GetBytes(registerAction);
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "logger_service", body: body);
        if (!customActionList.Contains(originalActionName))
        {
            customActionList.Add(originalActionName);
            customActionStateList.Add(false);
        }
        return "custom action defined";
    }
}