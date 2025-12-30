#!/bin/bash

cd ..
cd Execution_UI/Execution_UI.Tests
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ../..
cd CAST_Rest_Listener
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ..


cd Execution_Service
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ..
cd File_Storage_Service
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ..
cd Health_Service
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ..
cd Logger_Service
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ..
cd Scheduler_Service/Tests
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ../..


cd CAST_Client_Service/CAST_Client_Service.Tests
dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --verbosity normal --logger trx
cd ../..
cd CAST_Java_Client_Service
mvn test --file pom.xml
cd ..
