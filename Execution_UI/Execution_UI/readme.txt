Primary URLs
1. http://<server_ip> - Index page, contains links to things such as Client plugin downloads, the Execution UI, etc
2. http://<server_ip>/cast - Execution UI


Actions are defined within each registered Application, but controlled through the Execution UI and the REST Listener. It is generally a good idea to define the functionality based on the Action names - but that is completely up to the Application Developer. See Playwright_Demo/Playwright_Java_Demo for examples

Available Actions in v1.0.0 are
1. Start: Start the Application
2. Stop: Stop the Application
3. Pause: Pause the Application
4. Resume: Resume a Paused Application
5. Abort: Abort the Application
6. Restart: Restart the Application from an Aborted state
7. Custom: Defined within the registered Application
8. Cleanup: Will remove the Client row from the UI
