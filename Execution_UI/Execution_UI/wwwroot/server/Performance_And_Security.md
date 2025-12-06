(To some extent) both security and performance can be addressed by how you configure your environment. Different recommendations and requirements are included here to help you pick the best configuration





Basic Concepts

1. Each Service is an independent .Net Application. They all use RabbitMQ to serve Messages between Services
2. All information is stored in the central (MySQL) database. That includes both CAST-specific information (such as Service State) and Client information (such as Status)
3. Each Service can run on an isolated environment (such as a Hosted VM). All Services need RabbitMQ connectivity but some do not need a direct database connection
4. The Health Service will run some scheduled cleanup tasks if it is running on the same hosted environment as your RabbitMQ instance. Otherwise those cleanup steps will fail - but (otherwise) the Health Service will continue running as normal
5. The recommended RabbitMQ and MySQL configurations (in the README.md file) try to adhere to Least Privilege
6. Every Service (and Client) has a properties file that must be configured for your environment





Services

* Logger Service. Primary purpose is to support communications (messages) between the backend database and Services
* &nbsp;	Requires both RabbitMQ and MySQL Connections
* &nbsp;	RabbitMQ
* &nbsp;		Maintains a Queue called logger\_service. This Queue will be very active (since no other Service will submit ExecuteUpdate statements against the database)		
* &nbsp;	MySQL
* &nbsp;		Only submits ExecuteUpdates against the database (no queries)
* Execution Service. Primary purpose is to support communications (messages and files) between Services and all Clients
* &nbsp;	Requires a RabbitMQ Connection
* &nbsp;	RabbitMQ
* &nbsp;		Maintains a Queue called execution\_service
* &nbsp;		Will push messages to the logger service
* &nbsp;		Will push messages to each Client
* &nbsp;		Is the primary channel for pushing messages and files to all Clients through distinct Client Queues
* Scheduler Service. Primary purpose is to schedule future Runs
* &nbsp;	Requires both RabbitMQ and MySQL Connections
* &nbsp;	RabbitMQ
* &nbsp;		Maintains a Queue called scheduler\_service
* &nbsp;		Will push messages to the logger service
* &nbsp;		When a schedule has been hit will submit a message to the execution service Queue starting the Client run
* &nbsp;	MySQL
* &nbsp;		Poll the Client state for Scheduled runs		
* File Storage Service. Primary purpose is to support moving files between itself and all Clients
* &nbsp;	Requires a RabbitMQ Connection
* &nbsp;	RabbitMQ
* &nbsp;		Maintains a Queue called file\_storage\_service
* &nbsp;		Will push messages to the logger service
* &nbsp;		Files are sent as binary messaages (and therefore there is a max file size limitation of 10m
* &nbsp;		All files are Zipped prior to sending
* Health Service. Primary purpose is to scan for Service status (and update the MySQL database when the status changes). It will also cleanup old information, as well as remove old Message Queues if they are inactive
* &nbsp;	Requires both RabbitMQ and MySQL Connections
* &nbsp;		The RabbitMQ connection is strictly used to determine whether a Queue is unavailable. If a Queue is unavailable the MySQL database will be updated to reflect this
* &nbsp;			All Queues are monitored in this fashion (Services and Clients)
* &nbsp;			If a Client state is set to COMPLETED (and it's been over 30 minutes) the Health Service will attempt to manually delete the Queue
* &nbsp;			If it's been over 720 minutes (regardless of Client state) the Health Service will attempt to manually delete the Queue			





Client Services

* Every Client instance will maintain it's own (distinct) Queue
* Will push messages to the logger service
* Will push and pull messages with the execution service
* Will push and pull messages with the file storage service





Execution UI

* Primary purposes are to display the state of CAST Services (and all Clients) and to submit Actions to each Client
* &nbsp;	Requires both RabbitMQ and MySQL Connections
* &nbsp;	RabbitMQ
* &nbsp;		Submit Action requests to the Execution Service (for each Client)
* &nbsp;	MySQL
* &nbsp;		Retrieves the state of everything to be displayed





RabbitMQ

The recommended configuration is defined in README.md





MySQL

The recommended configuration is defined in README.md







Based on the above information these are the general security and performance considerations. Note the assumption that (in every case) you can only run 1 Client per machine. Also note the assumption that your MySQL Server instance is generally hosted separately

* The primary decision you need to make is where you want to host the Services, RabbitMQ Server and MySQL instance. For example
* &nbsp;	Hosting all Services on a single VM, RabbitMQ hosted separately, MySQL hosted separately has the following benefits and constraints
* &nbsp;		Benefits
* &nbsp;			Reduced network traffic
* &nbsp;			Ability to lock down access to and from the VM
* &nbsp;		Constraints
* &nbsp;			Health Service will not support automatic cleanup
* &nbsp;	Hosting all services on the same VM as a RabbitMQ Server, MySQL hosted separately has the following benefits and constraints
* &nbsp;		Benefits
* &nbsp;			Reduced network traffic
* &nbsp;			Ability to lock down access to and from the VM
* &nbsp;			Health Service will support automatic cleanup
* &nbsp;		Constraints
* &nbsp;			Process load
* &nbsp;	Hosting each service on a distinct VM, RabbitMQ hosted separately, MySQL hosted separately
* &nbsp;		Benefits
* &nbsp;			Ability to lock down access to and from the VMs
* &nbsp;			Each service can support load up-to the hosted VM
* &nbsp;		Constraints
* &nbsp;			Network traffic
* &nbsp;			Health Service will not support automatic cleanup
* &nbsp;			Amount of setup
* &nbsp;	Hosting each service on a distinct VM, RabbitMQ hosted on same VM as Health Service, MySQL hosted separately
* &nbsp;		Benefits
* &nbsp;			Ability to lock down access to and from the VMs
* &nbsp;			Each service can support load up-to the hosted VM. Note that the VM hosting both RabbitMQ and the Health Service will have more load than normal
* &nbsp;		Constraints
* &nbsp;			Network traffic
* &nbsp;			Amount of setup
* &nbsp;	Hosting everything on a single Docker Container
* &nbsp;		Benefits
* &nbsp;			Reduced network traffic
* &nbsp;			Ability to lock down access to and from the Container
* &nbsp;		Constraints
* &nbsp;			Performance is constrained by the Container
* &nbsp;			Not really production-worthy
