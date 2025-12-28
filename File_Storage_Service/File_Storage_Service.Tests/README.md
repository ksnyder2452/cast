# File Storage Service Unit Tests

This project contains comprehensive unit tests for the File Storage Service.

## Test Structure

### FileStorageServiceCoreTests
Contains unit tests for the core file storage functionality:
- **Constructor Tests**: Validates proper initialization and parameter validation
- **Directory Initialization Tests**: Tests directory structure creation
- **Log Message Creation Tests**: Tests SQL statement generation for logging
- **State Tracker Message Tests**: Tests state management SQL statements
- **File Processing Tests**: Comprehensive tests for file storage operations including:
  - Header validation
  - File creation and storage
  - Directory creation
  - ZIP file extraction
  - Error handling

### FileStorageResultTests
Tests for the FileStorageResult data transfer object:
- Default values validation
- Property setting and retrieval
- Success and failure scenarios

## Running the Tests

### Using .NET CLI
```bash
dotnet test
```

### Using Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Click "Run All Tests"

### Using VS Code
1. Install the C# Dev Kit extension
2. Open the Testing panel
3. Click "Run All Tests"

## Test Coverage

The test suite includes:
- ✅ 25+ unit tests
- ✅ Constructor validation
- ✅ Null/empty input handling
- ✅ File I/O operations
- ✅ Directory management
- ✅ ZIP file extraction
- ✅ SQL statement generation
- ✅ Error scenarios

## Dependencies

- xUnit 2.6.2 - Testing framework
- Moq 4.20.70 - Mocking framework for dependencies
- Microsoft.NET.Test.Sdk 17.8.0 - Test runner
- coverlet.collector 6.0.0 - Code coverage collection

## Notes

- Tests use temporary directories that are automatically cleaned up after each test
- Mock objects are used for RabbitMQ dependencies to avoid external dependencies
- Each test is isolated and can run independently
