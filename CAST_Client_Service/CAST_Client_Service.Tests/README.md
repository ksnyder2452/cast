# CAST Client Service Unit Tests

This folder contains comprehensive unit tests for the CAST_Client_Service class using xUnit.net testing framework.

## Test Coverage

The test suite covers the following functionality:

### 1. Action Processing Methods
- **StartRun Tests**: Validates the START action processing with various input formats
- **PauseRun Tests**: Validates the PAUSE action processing and state flag management  
- **ResumeRun Tests**: Validates the RESUME action processing
- **AbortRun Tests**: Validates the ABORT action processing and state clearing
- **CallCustomAction Tests**: Validates custom action processing

### 2. State Management Tests
- Verification of initial state flags (all false)
- Testing state flag modifications
- Custom action list management
- Custom action state list management

### 3. Configuration Tests
- UUID generation and validation
- Queue naming conventions
- Custom action list/state list initialization and modification

## Test Classes

### CAST_Client_Service_ActionTests
Tests for core action processing methods including:
- `StartRun()` - START action handling
- `PauseRun()` - PAUSE action handling and flag setting
- `ResumeRun()` - RESUME action handling
- `AbortRun()` - ABORT action handling
- `CallCustomAction()` - Custom action handling

### CAST_Client_Service_StateTests
Tests for state and configuration management:
- Initial state flag verification
- UUID format validation
- List initialization and modification

### CAST_Client_Service_ActionParameterTests
Tests for edge cases and parameter handling:
- Empty and whitespace handling
- Special characters in UUIDs
- Various input format variations

### CAST_Client_Service_ConfigurationTests
Tests for public configuration properties:
- UUID string validation
- Custom action list operations
- State flag manipulation

## Running the Tests

### Run all tests:
```bash
dotnet test
```

### Run tests with verbosity:
```bash
dotnet test -v normal
```

### Run specific test class:
```bash
dotnet test --filter "ClassName"
```

### Run tests matching a pattern:
```bash
dotnet test --filter "StartRun"
```

## Test Results

**Total Tests**: 25  
**Status**: All Passing ✓

## Framework and Dependencies

- **Test Framework**: xUnit.net 2.7.0
- **.NET Version**: .NET 9.0
- **Additional Packages**:
  - Moq 4.20.70 (for mocking, available for future use)
  - RabbitMQ.Client 7.2.0 (matches main project)
  - Microsoft.NET.Test.Sdk 17.9.0

## Key Testing Patterns

1. **Arrange-Act-Assert**: All tests follow the AAA pattern for clarity
2. **Theory Tests**: Parameterized tests for multiple input variations
3. **State Reset**: Tests reset state before assertions where applicable
4. **Case-Insensitive Testing**: Validates both uppercase and lowercase action inputs
5. **Edge Case Coverage**: Tests include null/empty strings and special characters

## Notes

- Debug mode file logging in the main service can interfere with parallel test execution. Tests avoid scenarios that trigger file I/O conflicts.
- The service uses static state flags that are shared across tests, so some tests explicitly reset state before running.
- Custom action substring parsing requires specific formatting (uppercase prefix with lowercase search).

## Future Test Enhancements

Potential areas for expansion:
- Integration tests for RabbitMQ communication
- Async method testing for file upload/download operations
- State transition validation (e.g., START -> PAUSE -> RESUME -> STOP)
- Error handling and exception scenarios
- Mock integration for external service calls
