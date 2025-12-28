3 URLs to use
1. http://<server_ip>/tables - Full URL, demonstrates Scheduler
2. http://<server_ip> - Full URL, demonstrates Copilot interface (no scheduler)
3. http://localhost/suite_gpl/samples/calendar/calendar_popup_2.html - demonstrates scheduler UI


Actions are defined within each registered Application, but controlled through the Execution UI and the REST Listener. It is generally a good idea to define the functionality based on the Action names - but that is completely up to the Application Developer. See Playwright_Demo/Playwright_Java_Demo for examples

Available Actions are
1. Start: Start the Application
2. Stop: Stop the Application
3. Pause: Pause the Application
4. Resume: Resume a Paused Application
5. Abort: Abort the Application
6. Restart: Restart the Application from an Aborted state
7. Custom: Defined within the registered Application
8. Cleanup: Will remove the Client row from the UI
