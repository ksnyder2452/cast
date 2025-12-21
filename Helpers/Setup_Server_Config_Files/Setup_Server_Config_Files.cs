using System.IO;

string currentDirectory = Directory.GetCurrentDirectory();
string fileStorageServiceConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "app.config.File_Storage_Service";
string fileStorageServiceConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "File_Storage_Service" + Path.DirectorySeparatorChar + "app.config";
string executionServiceConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "app.config.Execution_Service";
string executionServiceConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Execution_Service" + Path.DirectorySeparatorChar + "app.config";
string healthServiceConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "app.config.Health_Service";
string healthServiceConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Health_Service" + Path.DirectorySeparatorChar + "app.config";
string loggerServiceConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "app.config.Logger_Service";
string loggerServiceConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Logger_Service" + Path.DirectorySeparatorChar + "app.config";
string schedulerServiceConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "app.config.Scheduler_Service";
string schedulerServiceConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Scheduler_Service" + Path.DirectorySeparatorChar + "app.config";
string restListenerConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "appsettings.json.CAST_Rest_Listener";
string restListenerConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "CAST_Rest_Listener" + Path.DirectorySeparatorChar + "appsettings.json";
string executionUIConfig_source = @currentDirectory + Path.DirectorySeparatorChar + "originals" + Path.DirectorySeparatorChar + "appsettings.json.Execution_UI";
string executionUIConfig_destination = @currentDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Execution_UI" + Path.DirectorySeparatorChar + "Execution_UI" + Path.DirectorySeparatorChar + "appsettings.json";

try
{
    Directory.CreateDirectory(Path.GetDirectoryName(fileStorageServiceConfig_destination));
    Directory.CreateDirectory(Path.GetDirectoryName(executionServiceConfig_destination));
    Directory.CreateDirectory(Path.GetDirectoryName(healthServiceConfig_destination));
    Directory.CreateDirectory(Path.GetDirectoryName(loggerServiceConfig_destination));
    Directory.CreateDirectory(Path.GetDirectoryName(schedulerServiceConfig_destination));
    Directory.CreateDirectory(Path.GetDirectoryName(restListenerConfig_destination));
    Directory.CreateDirectory(Path.GetDirectoryName(executionUIConfig_destination));

    // Copy the file, overwrite if it already exists
    Console.WriteLine("Copying files from " + fileStorageServiceConfig_source + " to " + fileStorageServiceConfig_destination);
    File.Copy(fileStorageServiceConfig_source, fileStorageServiceConfig_destination, true);
    Console.WriteLine("Copying files from " + executionServiceConfig_source + " to " + executionServiceConfig_destination);
    File.Copy(executionServiceConfig_source, executionServiceConfig_destination, true);
    Console.WriteLine("Copying files from " + healthServiceConfig_source + " to " + healthServiceConfig_destination);
    File.Copy(healthServiceConfig_source, healthServiceConfig_destination, true);
    Console.WriteLine("Copying files from " + loggerServiceConfig_source + " to " + loggerServiceConfig_destination);
    File.Copy(loggerServiceConfig_source, loggerServiceConfig_destination, true);
    Console.WriteLine("Copying files from " + schedulerServiceConfig_source + " to " + schedulerServiceConfig_destination);
    File.Copy(schedulerServiceConfig_source, schedulerServiceConfig_destination, true);
    Console.WriteLine("Copying files from " + restListenerConfig_source + " to " + restListenerConfig_destination);
    File.Copy(restListenerConfig_source, restListenerConfig_destination, true);
    Console.WriteLine("Copying files from " + executionUIConfig_source + " to " + executionUIConfig_destination);
    File.Copy(executionUIConfig_source, executionUIConfig_destination, true);
    Console.WriteLine("Files copied successfully!");
}
catch (IOException e)
{
    Console.WriteLine($"An error occurred: {e.Message}");
}
