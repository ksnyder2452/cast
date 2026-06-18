[![License: GPL v2](https://img.shields.io/badge/License-GPL_v2-blue.svg)](https://www.gnu.org/licenses/old-licenses/gpl-2.0.en.html)
[![Client Service](https://github.com/ksnyder2452/cast/actions/workflows/client_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/client_service.yml)
[![Java Client Service](https://github.com/ksnyder2452/cast/actions/workflows/java_client_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/java_client_service.yml)
[![JavaScript Client Service](https://github.com/ksnyder2452/cast/actions/workflows/javascript_client_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/javascript_client_service.yml)
[![Execution Service](https://github.com/ksnyder2452/cast/actions/workflows/execution_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/execution_service.yml)
[![File Storage Service](https://github.com/ksnyder2452/cast/actions/workflows/file_storage_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/file_storage_service.yml)
[![Health Service](https://github.com/ksnyder2452/cast/actions/workflows/health_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/health_service.yml)
[![Logger Service](https://github.com/ksnyder2452/cast/actions/workflows/logger_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/logger_service.yml)
[![Scheduler Service](https://github.com/ksnyder2452/cast/actions/workflows/scheduler_service.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/scheduler_service.yml)
[![Execution UI](https://github.com/ksnyder2452/cast/actions/workflows/execution_ui.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/execution_ui.yml)
[![REST Listener](https://github.com/ksnyder2452/cast/actions/workflows/rest_listener.yml/badge.svg)](https://github.com/ksnyder2452/cast/actions/workflows/rest_listener.yml)

# Authors
* Kevin Snyder
* Michael Wu

# Contents
* [Introduction](#introduction)
* [Getting Started](#getting-started)
* [Components](#components)
* [Folder structure](#folder-structure)
* [Configure and run the CAST Server (on a hosted environment)](#configure-and-run-the-cast-server-on-a-hosted-environment)
* [Recommended Startup and Shutdown order (assuming all Services will be running)](#recommended-startup-and-shutdown-order-assuming-all-services-will-be-running)
* [How to register (and interact with) your .NET application](#how-to-register-and-interact-with-your-net-application)
* [How to register (and interact with) your Java application](#how-to-register-and-interact-with-your-java-application)
* [How to register (and interact with) your JavaScript application](#how-to-register-and-interact-with-your-javascript-application)
* [Running the .NET Test Framework Demo](#running-the-net-test-framework-demo)
* [Running the Java Test Framework Demo](#running-the-java-test-framework-demo)
* [Running the JavaScript Test Framework Demo](#running-the-javascript-test-framework-demo)
* [Notes](#notes)

# Introduction
The Centralized Automation of Software Tools Framework (CAST) is intended to provide standard, central actions around locally-defined applications (such as DiY Test Frameworks). The following core functionalities are supported

* Remote control of client actions
* Remote storage and distribution of files
* Integration with REST APIs

![UI Controller](./Helpers/screenshots/Execution_UI_screenshot.png)



This provides several key benefits

* Centralized control of all associated applications
* Centralized storage of reporting data (for use in dashboards)
* Simple integration into Pipelines
* The ability to add new Services into registered applications with minimal application changes
* The ability to compare and contrast application data across time, across clients and across platforms
* The opportunity to integrate with newly-developed applications and with existing applications (with minimal changes to existing functionality)
* The opportunity to provide alternative control mechanisms and reporting functionality for existing applications

# Getting Started
* Software dependencies
   * [RabbitMQ Server](https://www.rabbitmq.com/)
   * [MySQL Server](https://www.mysql.com/)
   * [.NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
   * [openjdk-25](https://openjdk.org/projects/jdk/25/)
   * [Font Awesome](https://fontawesome.com/v4/icons/)
   * [DHTML Calendar](https://dhtmlx.com/docs/products/dhtmlxCalendar/download.shtml#download-standard)

# Components
* MySQL database instance
* RabbitMQ Server
* Logger Service. Used to push information to the mysql database
* File Storage Service. Used to queue outbound files and receive inbound files
* Execution Service. Used to handle all communications between the CAST Service and registered applications. Both Messages and Files are sent via the Execution Service
* Scheduler Service. Schedule the Start Action for registered applications
* Health Check Service. Used to check the state of all Services (including registered applications) and update the database appropriately
* UI Controller. Used to manually control all registered applications, but also to demonstrate/simulate CAST functionality
* REST Listener. Used to push Actions to registered applications via REST API calls
* Playwright Demo. Modification of the Playwright Tutorial to include hooks into the CAST framework that functions as a registered demo. See [Playwright .NET demo](https://playwright.dev/dotnet/docs/intro) for the original source code
* Playwright Java Demo. Modification of the Playwright Tutorial to include hooks into the CAST framework that functions as a registered demo. See [Playwright Java demo](https://playwright.dev/java/docs/api/) for the original source code
* Playwright JavaScript Demo. Modification of the Playwright Tutorial to include hooks into the CAST framework that functions as a registered demo. See [Playwright JavaScript demo](https://playwright.dev/docs/writing-tests) for the original source code
* Helper Apps. Used to help setup and configure a CAST environment

# Folder structure
```
├── .github/                         # CI workflows
├── .gitignore
├── LICENSE
├── README.md
├── CONTRIBUTING.md
├── CODE_OF_CONDUCT.md
├── QuickStartGuide.txt              # Setup a sample server instance from scratch
├── CAST_Client_Service/             # CAST .NET client
│   ├── CAST_Client_Service/
│   └── CAST_Client_Service.Tests/
├── CAST_Java_Client_Service/        # CAST Java client
│   └── src/
│       ├── main/
│       │   └── java/
│       │       └── cast/
│       └── test/
│           └── java/
├── CAST_JS_Client_Service/          # CAST JavaScript client
│   ├── src/
│   └── tests/
├── CAST_Rest_Listener/              # REST API execution service listener
│   ├── Pages/
│   ├── Properties/
│   ├── Tests/
│   └── wwwroot/
├── Execution_Service/               # CAST execution service
├── Execution_UI/                    # CAST execution controller UI
│   ├── Execution_UI/
│   └── Execution_UI.Tests/
├── File_Storage_Service/            # CAST file storage service
│   └── File_Storage_Service.Tests/
├── Health_Service/                  # CAST health service
├── Helpers/                         # Setup and support utilities
│   ├── Setup_Server_Config_Files/
│   ├── screenshots/
│   └── diagrams/
├── Logger_Service/                  # Primary CAST logging service
│   └── Logger_Service.Tests/
├── Playwright_Demo/                 # .NET CAST demo
│   └── References/
├── Playwright_Java_Demo/            # Java CAST demo
│   ├── lib/
│   ├── resources/
│   └── src/
├── Playwright_JS_Demo/              # JavaScript CAST demo
│   └── tests/
└── Scheduler_Service/               # CAST scheduler service
```

# Configure and run the CAST Server (on a hosted environment)
1. Install and configure MySQL.
   * Create a database called `cast_server` with a remote-accessible account named `cast_admin`.
   * Create application accounts:
     * `create user 'cast_read'@'...' identified by '...';`
     * `create user 'cast_write'@'...' identified by '...';`
     * `grant SELECT on cast_server.* to 'cast_read'@'...';`
     * `grant INSERT, UPDATE, DELETE, SELECT on cast_server.* to 'cast_write'@'...';`
       * `SELECT` is required to support `UPDATE`.
   * Create the required schema using [table definitions](./Helpers/setup_tables.sql).

2. Install and configure RabbitMQ.
   * Follow [RabbitMQ setup](./Helpers/rabbimq_setup.txt).

3. Configure CAST services and components.
   * Recommended DB and RabbitMQ accounts:
     * Logger Service: DB `cast_write`, RabbitMQ `logger_admin`
     * File Storage Service: RabbitMQ `file_store_admin`
     * Execution Service: RabbitMQ `exec_admin`
     * Scheduler Service: DB `cast_read`, RabbitMQ `scheduler_admin`
     * Health Service: DB `cast_write`, RabbitMQ `health_admin`
     * Execution UI: DB `cast_read`, RabbitMQ `ui_control_admin`
     * REST Listener: RabbitMQ `ui_control_admin`
     * Client Application: RabbitMQ `client_admin`
   * JavaScript Client Application: RabbitMQ `js_client_admin`
   * Automatic local configuration:
     * `cd ./Helpers/Setup_Server_Config_Files/`
     * Update files under `./originals/`
     * `dotnet run`
     * Update application properties:
      * `cast.properties` for .NET
       * `resources/config.properties` for Java
      * `cast.properties` for JavaScript
   * Manual configuration targets:
     * `./Logger_Service/app.config`
     * `./File_Storage_Service/app.config`
     * `./Execution_Service/app.config`
     * `./Scheduler_Service/app.config`
     * `./Health_Service/app.config`
     * `./CAST_Rest_Listener/appsettings.json`
     * `./Execution_UI/Execution_UI/appsettings.json`
   * `./Application root/cast.properties` (.NET and JavaScript)
     * `./Application root/resources/config.properties` (Java)

4. Start server components in order.
   * Launch RabbitMQ Server.
   * Launch MySQL Server.
   * Launch Logger Service:
     * `cd ./Logger_Service/`
     * `dotnet clean`
     * `dotnet run`
   * Launch File Storage Service:
     * `cd ./File_Storage_Service/`
     * `dotnet clean`
     * `dotnet run`
   * Launch Execution Service:
     * `cd ./Execution_Service`
     * `dotnet clean`
     * `dotnet run`
   * Launch Scheduler Service:
     * `cd ./Scheduler_Service`
     * `dotnet clean`
     * `dotnet run`
   * Launch Health Check Service:
     * `cd ./Health_Service`
     * `dotnet clean`
     * `dotnet run`
   * Launch UI Controller:
     * `cd ./Execution_UI/Execution_UI/`
     * `dotnet clean`
     * `dotnet run`
   * Launch your remote application instance.

5. Access the UI.
   * Open `http://CAST_Server_IP/` for index/docs/links.
   * Open `http://CAST_Server_IP/cast` directly for the Execution UI.

# Recommended Startup and Shutdown order (assuming all Services will be running)
* Startup order
   1. Logger Service (always first)
   2. Execution Service
   3. File Storage Service
   4. Scheduler Service
   5. Health Check Service
   6. Execution UI / REST Listener
   7. Clients
* Shutdown order
   1. Clients
   2. Execution UI / REST Listener
   3. Scheduler Service
   4. File Storage Service
   5. Execution Service
   6. Logger Service (always second-to-last)
   7. Health Check Service (always last)

# How to register (and interact with) your .NET application
* **See ./Playwright_Demo/UnitTest1.cs for an example**
* Set cast.properties to the correct values
* Add a reference to CAST_Client_Service.dll
* Call CAST_Client_Service.CAST_Client_Service.updateFrameworkFunctionality() at the beginning of your application to register it
   * updateFrameworkFunctionality() is required
* Call CAST_Client_Service.CAST_Client_Service.updateState("ONLINE") to tell CAST that your application is online.
   * The Execution UI keys on a state that starts with 'ONLINE', and therefore ONLINE is required and reserved
* Call CAST_Client_Service.CAST_Client_Service.updateState("READY", "green") to tell CAST that your application is ready to start
   * The Execution UI keys on a state that starts with 'READY', and therefore READY* is required and reserved
* Call CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED", "green") to tell CAST that your application is finished
   * The Execution UI keys on a state that starts with 'COMPLETED', and therefore COMPLETED* is required and reserved
* Call CAST_Client_Service.CAST_Client_Service.updateResult() to update your application results on the CAST database. This can be used for reporting
* Call CAST_Client_Service.CAST_Client_Service.updateState() to tell CAST the state of your application. This will impact the available Actions on the UI
* Call CAST_Client_Service.CAST_Client_Service.registerAction() to create custom Actions for your application
* Call CAST_Client_Service.CAST_Client_Service.uploadOutputFolder() to upload the contents of the output folder to the File Storage Service
* Call CAST_Client_Service.CAST_Client_Service.closeQueue() to close the Message Queue once your application run is complete
* Check for CAST Action state by retrieving CAST_Client_Service.CAST_Client_Service._* (boolean)
* Retrieve your application UUID by retrieving CAST_Client_Service.CAST_Client_Service.startmyuuidAsString

Minimal .NET integration snippet:

```csharp
using System.Threading.Tasks;
using CAST_Client_Service;

public class CastDemo
{
   public static async Task RunAsync()
   {
      // Registers functionality and framework metadata
      CAST_Client_Service.CAST_Client_Service.updateFrameworkFunctionality(
         startEnabled: true,
         stopEnabled: true,
         pauseEnabled: true,
         resumeEnabled: true,
         abortEnabled: true,
         restartEnabled: false,
         uploadResultEnabled: true,
            frameworkName: "My .NET Framework",
         filterOnGroup: "GroupA",
         filterOnOwner: "OwnerA",
         filterOnLocation: "Lab1",
         filterOnKeyword: "|ui|smoke|"
      );

      await CAST_Client_Service.CAST_Client_Service.updateState("ONLINE", "black");
      await CAST_Client_Service.CAST_Client_Service.updateState("READY", "green");

      // Your test or automation run here...

      CAST_Client_Service.CAST_Client_Service.updateResult("Run completed successfully");
      await CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED", "green");
      CAST_Client_Service.CAST_Client_Service.closeQueue();
      CAST_Client_Service.CAST_Client_Service.stopService();
   }
}
```

Example cast.properties:

```properties
rabbitmq_home=localhost
rabbitmq_port=5672
rabbitmq_user=client_admin
rabbitmq_pwd=your_password
reloadUUID=no
```

# How to register (and interact with) your Java application
* **See ./Playwright_Java_Demo/src/main/java/CAST_Demo.java for an example**
* Set ./resources/config.properties to the correct values
* Add the library CAST_Java_Client_Service.jar
* import main.java.cast.Java_Client_Service;
* Call Java_Client_Service.updateFrameworkFunctionality() at the beginning of your application to register it
   * updateFrameworkFunctionality() is required
* Call Java_Client_Service.updateState("ONLINE", "black") to tell CAST that your application is online
   * The Execution UI keys on a state that starts with 'ONLINE', and therefore ONLINE is required and reserved
* Call Java_Client_Service.updateState("READY", "green") to tell CAST that your application is ready to start
   * The Execution UI keys on a state that starts with 'READY', and therefore READY* is required and reserved
* Call Java_Client_Service.updateState("COMPLETED", "green") to tell CAST that your application is is finished
   * The Execution UI keys on a state that starts with 'COMPLETED', and therefore COMPLETED* is required and reserved
* Call Java_Client_Service.updateResult() to update your application results on the CAST database. This can be used for reporting
* Call Java_Client_Service.updateState() to tell CAST the state of your application. This will impact the available Actions on the UI
* Call Java_Client_Service.registerAction() to create custom Actions for your application
* Call Java_Client_Service.uploadResultFolder() to uplaod the contents of the output folder to the File Storage Service
* Check for CAST Action state by retrieving Java_Client_Service._* (boolean)
* Retrieve your application UUID by retrieving Java_Client_Service.uuidAsString

Minimal Java integration snippet:

```java
import main.java.cast.Java_Client_Service;

public class CastDemo {
   public static void main(String[] args) throws Exception {
      Java_Client_Service.startService();

      Java_Client_Service.updateFrameworkFunctionality(
         true,   // startEnabled
         true,   // stopEnabled
         true,   // pauseEnabled
         true,   // resumeEnabled
         true,   // abortEnabled
         false,  // restartEnabled
         true,   // uploadResultEnabled
         "My Java Framework",
         "GroupA",
         "OwnerA",
         "Lab1",
         "|ui|smoke|"
      );

      Java_Client_Service.updateState("ONLINE", "black");
      Java_Client_Service.updateState("READY", "green");

      // Your test or automation run here...

      Java_Client_Service.updateResult("Run completed successfully");
      Java_Client_Service.updateState("COMPLETED", "green");
      Java_Client_Service.closeQueue();
      Java_Client_Service.stopService();
   }
}
```

Example resources/config.properties:

```properties
rabbitmq_home=localhost
rabbitmq_port=5672
rabbitmq_user=client_admin
rabbitmq_pwd=your_password
```

# How to register (and interact with) your JavaScript application
JavaScript client registration is supported using the steps below.

* **See ./Playwright_JS_Demo/tests/example.spec.js for an example**
* Set cast.properties to the correct values
* Add CAST_Client_Service.js to your project root (or import from ./CAST_JS_Client_Service/src/CAST_Client_Service.js)
* import CAST_Client_Service from './CAST_Client_Service.js';
* Create the CAST client and call await castService.startService() at the beginning of your application to connect and register
* Call await castService.updateFrameworkFunctionality() at the beginning of your application to register functionality
   * updateFrameworkFunctionality() is required
* Call await castService.updateState("ONLINE", "black") to tell CAST that your application is online
   * The Execution UI keys on a state that starts with 'ONLINE', and therefore ONLINE is required and reserved
* Call await castService.updateState("READY", "green") to tell CAST that your application is ready to start
   * The Execution UI keys on a state that starts with 'READY', and therefore READY* is required and reserved
* Call await castService.updateState("COMPLETED", "green") to tell CAST that your application is finished
   * The Execution UI keys on a state that starts with 'COMPLETED', and therefore COMPLETED* is required and reserved
* Call await castService.updateResult() to update your application results on the CAST database. This can be used for reporting
* Call await castService.updateState() to tell CAST the state of your application. This will impact the available Actions on the UI
* Call await castService.registerAction() to create custom Actions for your application
* Call await castService.uploadOutputFolder() or await castService.uploadResultFolder() to upload the contents of the output folder to the File Storage Service
* Call await castService.closeQueue() to close the Message Queue once your application run is complete
* Call await castService.stopService() and await castService.close() when your run is complete
* Check for CAST Action state by retrieving castService._* (boolean)
* Retrieve your application UUID by retrieving castService.startmyuuidAsString

Minimal JavaScript integration snippet:

```javascript
import CAST_Client_Service from './CAST_Client_Service.js';

const castService = new CAST_Client_Service();
await castService.startService();

await castService.updateFrameworkFunctionality(
   true,   // startEnabled
   true,   // stopEnabled
   true,   // pauseEnabled
   true,   // resumeEnabled
   true,   // abortEnabled
   false,  // restartEnabled
   true,   // uploadResultEnabled
   'My JavaScript Framework',
   'GroupA',
   'OwnerA',
   'Lab1',
   '|ui|smoke|'
);

await castService.updateState('ONLINE', 'black');
await castService.updateState('READY', 'green');

// Your test or automation run here...

await castService.updateResult('Run completed successfully');
await castService.updateState('COMPLETED', 'green');
await castService.closeQueue();
await castService.stopService();
await castService.close();
```

Example cast.properties:

```properties
rabbitmq_home=localhost
rabbitmq_port=5672
rabbitmq_user=js_client_admin
rabbitmq_pwd=your_password
reloadUUID=no
```

# Running the .NET Test Framework Demo
1. Setup Playwright browsers.
   * (.NET) `.\bin\debug\net9.0\playwright.ps1 install`
2. Launch the test framework.
   * `cd ./Playwright_Demo/`
   * Configure `cast.properties`.
   * `client_service.dll` is included in `/Playwright_Demo/References/`.
   * A new DLL version can be compiled from `./CAST_Client_Service/`.
   * Run the suite with `dotnet test`.
3. Open the Execution UI at `http://CAST_Server_IP/cast`.
4. Select the top framework instance and start the run.
5. Exercise Actions and simulate a complete run.
6. Verify results (assuming the run was not Aborted/Stopped early).
   * UI state shows `COMPLETED TESTSUITE Playwright Demo`.
   * No errors in Logger Service console.
   * File Storage Service receives result file.
   * Result file `current_results.csv` is in `.\File_Storage_Service\temp\inbound_queue\client_service_*\`.

# Running the Java Test Framework Demo
1. Setup Playwright browsers.
   * See [Java Playwright.create()](https://playwright.dev/java/docs/intro).
2. Launch the test framework.
   * `cd ./Playwright_Java_Demo/`
   * Configure `./resources/config.properties`.
   * `CAST_Java_Client_Service.jar` is included in `/Playwright_Java_Demo/lib/`.
   * A new JAR version can be compiled from `./CAST_Java_Client_Service/`.
   * Run tests from your preferred tool (IntelliJ Community Edition was used).
3. Open the Execution UI at `http://CAST_Server_IP/cast`.
4. Select the top framework instance and start the run.
5. Exercise Actions and simulate a complete run.
6. Verify results (assuming the run was not Aborted/Stopped early).
   * UI state shows `COMPLETED TESTSUITE Playwright Java Demo`.
   * No errors in Logger Service console.
   * File Storage Service receives result file.
   * Result file `current_results.csv` is in `.\File_Storage_Service\temp\inbound_queue\client_service_*\`.

# Running the JavaScript Test Framework Demo
1. Setup Playwright browsers.
   * `cd ./Playwright_JS_Demo/`
   * `npm install`
   * `npx playwright install`
2. Launch the test framework.
   * `cd ./Playwright_JS_Demo/`
   * Configure `cast.properties`.
   * The demo loads `CAST_Client_Service.js` from `/Playwright_JS_Demo/CAST_Client_Service.js`.
   * Reference implementation is available at `./CAST_JS_Client_Service/src/CAST_Client_Service.js`.
   * Run the suite with `npx playwright test`.
3. Open the Execution UI at `http://CAST_Server_IP/cast`.
4. Select the top framework instance and start the run.
5. Exercise Actions and simulate a complete run.
6. Verify results (assuming the run was not Aborted/Stopped early).
   * UI lists `Playwright JavaScript Framework` while run is active.
   * No errors in Logger Service console.
   * Framework receives Start/Stop/Pause/Resume/Abort actions.
   * If your test uploads output, File Storage Service receives the zip file.
   * Uploaded files are in `.\File_Storage_Service\temp\inbound_queue\client_service_*\`.

# Notes
* Every Client uses it's own unique Message Queue
   * Every Client should use it's own RabbitMQ Account. See [rabbimq_setup.txt](./Helpers/rabbimq_setup.txt) for recommended Client configurations
   * The Message Queue is created upon loading the client service library (.NET DLL, Java JAR, or JavaScript CAST_Client_Service.js)
* For Dashboard analysis
   * All CAST details are stored within the table logger
      * reference_uuid can be thought of as a Session UUID. Which gives us the ability to easily filter all logs and events to a single reference
      * originator is the UUID of the Service that created the record
      * display_name is used to map UUID to an easily understood reference
      * event_time_dt is the date/timestamp (excluding timezone)
      * order_in_system is the Primary Key
   * Client State data is stored within the table state
   * Final Results data is stored within the table results
* Every .NET Client must include a cast.properties in the root folder. See /Playwright_Demo/cast.properties as an example
* Every Java Client must include a config.properties in the ./resources/ folder. See /Playwright_Java_Demo/resources/config.properties as an example
* Every JavaScript Client must include a cast.properties in the root folder. See /Playwright_JS_Demo/cast.properties as an example
* Health Check will automatically delete old Queues if the RabbitMQ Controller exists on the same machine (under c:\program files\Rabbitmq Server\)
* The File Storage Service is currently configured to receive inbound files from the frameworks
   * See /Playwright_Demo/UnitTest1.cs and /Playwright_Java_Demo/src/main/java/CAST_Demo.java for an example (test results are sent to the File Storage Service)
   * Outbound sends (to Clients) have not been implemented yet
   * Inbound files will be saved in \File_Storage_Service\temp\inbound_queue\client_service_UUID\
   * Client folders will be Zipped prior to sending
* Both Scheduler Service and the Health Service can take a few seconds to shutdown properly. Please be patient
* The API call updateFrameworkFunctionality() will register your application instance with the CAST Server. Once this call occurs you will see the instance on the Execution UI and will be able to reference it through our REST Listener
* The default RabbitMQ values are guest/guest within the property files. (If you accidentally run one of the Components before setting it to the proper value) you may need to delete the \bin\ and \obj\ folders to get rid of the guest references. You'll see the following error in the RabbitMQ Console: PLAIN login refused: user 'guest' - invalid credentials
* If you notice that Services are not showing on the Execution UI page make sure you are not running multiple instances of the Service. That functionality is not supported (yet)













































































































































































