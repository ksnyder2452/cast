# Introduction 
The Centralized Automation of Software Tools Framework (CAST) is intended to provide standard, hosted services around locally-defined applications (such as DiY Test Frameworks). The following core functionalities are supported

* Remote control of client actions
* Remote storage and distribution of files
* Integration with REST APIs

![UI Controller](./Execution_UI_screenshot.png)



This provides several key benefits

* Central control of all associated applications
* Centralized storage of reporting data (for use in dashboards)
* Simple integration into Pipelines
* The ability to add future Services into all applications with minimal development effort
* The ability to compare and contrast data across time, across clients and across platforms
* The opportunity to both integrate within newly-developed applications and within existing applications (with minimal changes to existing functionality)



# Getting Started
* Software dependencies
   * [RabbitMQ Server](https://www.rabbitmq.com/)
   * [MySQL Server](https://www.mysql.com/)
   * [DotNet 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
   * [openjdk-25](https://openjdk.org/projects/jdk/25/)
   * [Font Awesome](https://fontawesome.com/v4/icons/)
   * [DHTML Calendar](https://dhtmlx.com/docs/products/dhtmlxCalendar/download.shtml#download-standard)
* [Jira CAST Team](https://centralautomationsoftwaretool.atlassian.net/jira/software/projects/KAN/boards/2)



# Components
* mysql backend database
* RabbitMQ Server
* Logger Service. Used to push information to the mysql database
* File Storage Service. Used to queue outbound files and receive inbound files
* Execution Service. Used to handle all communications between the CAST Service and individual clients. Both Messages and Files are sent via the Execution Service
* Scheduler Service. Schedule the Start Action
* Health Check Service. Used to check the state of all Services (including registered clients) and update the database appropriately
* UI Controller. Used to manually control all clients, but also to demonstrate/simulate CAST functionality
* REST Listener. Used to push Actions to registered Clients via REST API calls
* Playwright Demo. Modification of the Playwright Tutorial to include hooks into the CAST framework. See [Playwright .Net demo](https://playwright.dev/dotnet/docs/intro) for the original source code
* Playwright Java Demo. Modification of the Playwright Tutorial to include hooks into the CAST framework. See [Playwright Java demo](https://playwright.dev/java/docs/api/) for the original source code
* Helper Apps. Used to help setup and configure a local CAST environment


# Configure and run the CAST Server (on a hosted environment)
* Install a MySQL Server instance
   * Create a database called cast_server with a remote-accessible account named cast_admin as well as the following accounts
     * create user 'cast_read'@'172.17.0.1' identified by '...';
	  * create user 'cast_write'@'172.17.0.1' identified by '...';
	  * grant SELECT on cast_server.* to 'cast_read'@'...';
     * grant INSERT, UPDATE, DELETE on cast_server.* to 'cast_write'@'...';
   * Create the following tables [Table definitions] (./Helpers/setup_tables.sql)
* Install a RabbitMQ Server
   * Configure the RabbitMQ Server [RabbitMQ configuration] (./Helpers/rabbimq_setup.txt)
* Configure all Services
   * Run Setup_Server_Config_Files
     * cd ./Helpers/Setup_Server_Config_Files/
     * Update all files under ./originals/
     * dotnet run
   * Manually configure all files
     * ./Logger_Service/app.config
     * ./File_Storage_Service/app.config
     * ./Execution_Service/app.config
     * ./Scheduler_Service/app.config
     * ./Health_Service/app.config
     * ./CAST_Rest_Listener/appsettings.json
     * ./Execution_UI/Execution_UI/appsettings.json
* Launch the RabbitMQ Server
* Launch the MySQL Server
* Launch the Logger Service
   * cd ./Logger_Service/
   * dotnet run
* Launch the File Storage Service
   * cd ./File_Storage_Service/
   * dotnet run
* Launch the Execution Service
   * cd ./Execution_Service
   * dotnet run
* Launch the Scheduler Service
   * cd ./Scheduler_Service
   * dotnet run
* Launch the Health Check Service
   * cd ./Health_Service
   * dotnet run
* Launch the UI Controller
   * cd ./Execution_UI/Execution_UI/
   * dotnet run

# Recommended Startup and Shutdown order (assuming all Services will be running)
* Startup
  * Logger Service. This Service should always be launched first
  * Execution Service
  * File Storage Service
  * Scheduler Service
  * Health Check Service
  * Execution UI/REST Listener
  * Clients
* Shutdown
  * Clients
  * Execution UI/REST Listener
  * Scheduler Service
  * File Storage Service
  * Execution Service
  * Logger Service. This Service should always be shutdown second-to-last
  * Health Check Service. This Service should always be shutdown last

# Run a .Net Test Framework Demo
* Setup Playwright Browsers
   * (.Net) .\bin\debug\net8.0\playwright.ps1 install
* Launch the Test Framework
   * cd ./Playwright_Demo/
   * Configure the cast.properties file
     * Note that client_service.dll is included in /Playwright_Demo/References/. A new version can be compiled from ./CAST_Client_Service/
   * Run the test suite using 'dotnet test'
* Select the top framework instance
* Start the Test Run
* Test the various Actions and Simulate a complete test run


# Run a Java Test Framework Demo
* Setup Playwright Browsers
   * [Java Playwright.create] (https://playwright.dev/java/docs/intro)
* Launch the Test Framework
   * cd ./Playwright_Java_Demo/
   * Configure the ./resources/config.properties file
     * Note that CAST_Java_Client_Service.jar is included in /Playwright_Java_Demo/lib/. A new version can be compiled from ./CAST_Java_Client_Service/
   * Run the test suite from your favorite tool. We used Intellij Community Edition
* Select the top framework instance
* Start the Test Run
* Test the various Actions and Simulate a complete test run


# Notes
* Every Client uses it's own unique Message Queue. The Queue is created upon loading CAST_client_service.dll or CAST_Java_Client_Service.jar
* The table logger is intended to be used in the following manner
   * reference_uuid can be thought of as a Session UUID. Which gives us the ability to easily filter all logs and events to a single reference
   * originator is the UUID of the Service that created the record
   * type is the log type (such as INFO, WARN, ERROR, ACTION)
   * code is intended to reference things like a Database Error Code
   * message is the log description
   * original_message is intended to reference things like Stack Traces
   * display_name is used to map UUID to an easily understood reference
   * event_time_dt is the date/timestamp (excluding timezone)
   * order_in_system is the Primary Key
* Client State data is stored within the table current_state
* Final Results data is stored within the table final_results
* Every .Net Client must include a cast.properties in the root folder containing the following values. See /Playwright_Demo/cast.properties as an example
* Every Java Client must include a config.properties in the ./resources/ folder. See /Playwright_Java_Demo/resources/config.properties as an example
* Health Check will automatically delete old Queues if the RabbitMQ Controller exists on the same machine (under c:\program files\Rabbitmq Server\)
* The File Storage Service is currently configured to receive inbound files from the frameworks
   * See /Playwright_Demo/UnitTest.cs and /Playwright_Java_Demo/src/main/java/CAST_Demo.java for an example (test results are sent to the File Storage Service)
   * Outbound sends (to Clients) have not been implemented yet
   * Inbound files will be saved in \File_Storage_Service\temp\inbound_queue\client_service_UUID\
   * Client folders will be Zipped prior to sending












