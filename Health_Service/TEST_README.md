# Health Service Unit Tests

This folder contains comprehensive unit tests for the Health Service application.

## Test Structure

### Test Projects

1. **Health_Service.Tests.csproj** - Main test project file using xUnit and Moq frameworks

### Test Files

#### 1. HealthServiceManagerTests.cs
Main unit tests for the `HealthServiceManager` class. Organized into test categories:

- **QueueExists Tests** (3 tests)
  - Test checking if RabbitMQ queues exist
  - Tests with existing and non-existing queues
  - Tests multiple queue checks

- **UpdateRows Tests** (2 tests)
  - Test database update functionality
  - Tests single and multiple statement executions

- **GetServiceState Tests** (3 tests)
  - Tests retrieving service states
  - Tests ONLINE, OFFLINE, and UNDER CONSTRUCTION states

- **UpdateServiceOffline Tests** (1 test)
  - Tests updating service state to OFFLINE in database

- **GetClientServiceUUIDs Tests** (2 tests)
  - Tests retrieving list of client service UUIDs
  - Tests with existing services and empty results

- **GetServiceStateInfo Tests** (1 test)
  - Tests retrieving state information with timestamps

- **MarkServiceOffline Tests** (1 test)
  - Tests inserting OFFLINE state into state table

- **FindRabbitMQControlDirectory Tests** (2 tests)
  - Tests finding RabbitMQ control script directory
  - Tests with existing and missing directories

- **DeleteEmptyQueue Tests** (1 test)
  - Tests RabbitMQ queue deletion command execution

- **IsServiceStateStale Tests** (6 tests)
  - Tests service state staleness detection
  - Tests with different time thresholds
  - Theory tests with multiple parameter combinations

- **Integration Tests** (2 tests)
  - Tests complete service health check workflow
  - Tests client service state checking workflow

#### 2. ImplementationTests.cs
Tests for individual implementation classes:

- **RabbitMQConnectionFactoryTests** (3 tests)
  - Tests RabbitMQ connection validation
  - Tests with valid/invalid hosts and queues

- **FileSystemHelperTests** (3 tests)
  - Tests file discovery functionality
  - Tests with valid, non-existent, and invalid paths

- **ProcessRunnerTests** (1 test)
  - Tests process execution

- **MySqlDatabaseConnectorTests** (4 tests)
  - Tests MySQL database operations
  - Tests service state retrieval and updates

#### 3. EdgeCaseTests.cs
Edge cases and boundary condition tests:

- **Empty/Null Input Tests** (2 tests)
  - Tests handling of empty queue names and service names

- **Time Boundary Tests** (4 tests)
  - Tests exact boundary conditions for time thresholds
  - Tests transitions at 30-minute and 720-minute marks

- **Special Characters Tests** (2 tests)
  - Tests handling of special characters in input

- **Large Data Set Tests** (1 test)
  - Tests with 1000+ UUIDs

- **Case Sensitivity Tests** (3 tests)
  - Tests case-insensitive state comparisons

- **Concurrent Access Tests** (1 test)
  - Tests concurrent queue checks

- **State Transition Tests** (1 test)
  - Tests service state transitions

## Running the Tests

### Using dotnet CLI

```bash
# Run all tests
dotnet test Health_Service.Tests.csproj

# Run tests with verbose output
dotnet test Health_Service.Tests.csproj -v

# Run specific test class
dotnet test Health_Service.Tests.csproj --filter ClassName=HealthServiceManagerTests

# Run with code coverage
dotnet test Health_Service.Tests.csproj /p:CollectCoverageMetrics=true
```

### Using Visual Studio

1. Open Test Explorer: Test > Test Explorer
2. Build the solution
3. Click "Run All Tests"
4. Review test results in Test Explorer

## Test Coverage

The test suite covers:
- ✅ Queue existence checking
- ✅ Database operations (read/write)
- ✅ Service state management
- ✅ Client service UUID retrieval
- ✅ File system operations
- ✅ Process execution
- ✅ Time-based state staleness detection
- ✅ Error handling and edge cases
- ✅ Integration scenarios

## Mocking Strategy

The tests use **Moq** to mock external dependencies:
- `IConnectionFactory` - RabbitMQ operations
- `IDatabaseConnector` - MySQL database operations
- `IFileSystemHelper` - File system operations
- `IProcessRunner` - Process execution

This allows tests to run without requiring:
- Running RabbitMQ instance
- Running MySQL instance
- File system access
- Process execution

## Refactored Code

The original `Health_Service.cs` has been refactored into `Health_Service.Refactored.cs` to:
- Extract functionality into a testable `HealthServiceManager` class
- Define interfaces for dependency injection
- Implement concrete implementations for each interface
- Make the code more modular and maintainable

## Key Testing Principles

1. **Unit Testing** - Each test focuses on a single method or behavior
2. **Mocking** - External dependencies are mocked to isolate units
3. **Arrange-Act-Assert** - Tests follow the AAA pattern for clarity
4. **Theory Tests** - Using xUnit's `[Theory]` for parameterized tests
5. **Edge Cases** - Comprehensive testing of boundary conditions
6. **Integration Tests** - Tests of complete workflows

## Dependencies

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **Microsoft.NET.Test.Sdk** - Test SDK for .NET
- **RabbitMQ.Client** - For connection factory implementation
- **MySql.Data** - For database connector implementation

## Future Improvements

1. Add integration tests with real RabbitMQ and MySQL instances
2. Add performance/load tests
3. Add mutation testing for test quality assessment
4. Add continuous integration/deployment pipeline tests
5. Increase code coverage to 90%+

## Notes

- All tests are isolated and can run in any order
- No test data cleanup is required (due to mocking)
- Tests are deterministic and repeatable
- Tests should complete in < 1 second total
