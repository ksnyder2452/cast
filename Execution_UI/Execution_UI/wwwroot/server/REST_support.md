# Introduction 
The following information can be used to control CAST Actions via REST API calls (rather than the Execution UI)
* Run the CAST REST Listener
  * cd ./CAST_Rest_Listener/
  * dotnet run
* The following API calls are supporoted
  * (POST) /api/start_client. Parameter = Client ID
  * (POST) /api/stop_client. Parameter = Client ID
  * (POST) /api/pause_client. Parameter = Client ID
  * (POST) /api/resume_client. Parameter = Client ID
  * (POST) /api/abort_client. Parameter = Client ID
  * (POST) /api/restart_client. Parameter = Client ID
* The .Net Client ID may be retrieved from a running Framework by calling CAST_Client_Service.CAST_Client_Service.startmyuuidAsString
* The Java Client ID may be retrieved from a running Framework by calling Java_Client_Service.uuidAsString
