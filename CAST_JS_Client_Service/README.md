# CAST JavaScript Client Service

A JavaScript/Node.js implementation of the CAST (Centralized Automation for Software Tools) Client Service for integrating custom frameworks with CAST.

## Overview

This module provides a client-side integration point into the CAST infrastructure, enabling JavaScript-based frameworks and applications to communicate with the CAST Server via RabbitMQ messaging.

## Features

- **Action Processing**: Handle START, STOP, PAUSE, RESUME, and ABORT commands from CAST Server
- **Custom Actions**: Register and execute custom framework-specific actions
- **File Management**: Upload and download files with automatic compression
- **State Management**: Track and report framework state during execution
- **Result Tracking**: Send execution results back to CAST Server
- **RabbitMQ Integration**: Async messaging with CAST infrastructure
- **Debug Logging**: Thread-safe file logging for troubleshooting

## Installation

```bash
npm install
```

## Dependencies

- **amqplib**: RabbitMQ client library
- **uuid**: UUID generation
- **archiver**: File compression and archiving

## Configuration

Create a `cast.properties` file in your working directory with the following settings:

```properties
# RabbitMQ Configuration
rabbitmq_home=localhost
rabbitmq_port=5672
rabbitmq_user=guest
rabbitmq_pwd=guest

# Optional: Reload previous run configuration
reloadUUID=no
# currentUUID=your-uuid-here (auto-generated if reloadUUID=yes)
```

## Usage

### Basic Setup

```javascript
import CAST_Client_Service from './src/CAST_Client_Service.js';

// Create service instance
const castService = new CAST_Client_Service();

// Start the service (connects to RabbitMQ and listens for messages)
await castService.startService();

// Register framework capabilities
await castService.updateFrameworkFunctionality(
  true,      // startEnabled
  true,      // stopEnabled
  true,      // pauseEnabled
  true,      // resumeEnabled
  true,      // abortEnabled
  false,     // restartEnabled
  true,      // uploadResultEnabled
  'My Framework',     // frameworkName
  'TestGroup',        // filterOnGroup
  'TestOwner',        // filterOnOwner
  'TestLocation',     // filterOnLocation
  'TestKeyword'       // filterOnKeyword (optional)
);

// Update framework state during execution
await castService.updateState('Running tests', 'green');

// Send results
await castService.updateResult('Test execution completed successfully');

// Upload output files
await castService.uploadOutputFolder('./output/', './');

// Stop the service when done
await castService.stopService();
await castService.close();
```

### Action Handling

The service automatically sets state flags when actions are received from CAST:

```javascript
// Check action flags in your main loop
if (castService._startRun) {
  // Handle start action
  castService._startRun = false;
}

if (castService._stopRun) {
  // Handle stop action
  castService._stopRun = false;
}

if (castService._pauseRun) {
  // Handle pause action
  castService._pauseRun = false;
}

if (castService._resumeRun) {
  // Handle resume action
  castService._resumeRun = false;
}

if (castService._abortRun) {
  // Handle abort action
  castService._abortRun = false;
}
```

### Custom Actions

Register custom actions that your framework supports:

```javascript
await castService.registerAction(
  'MyCustomAction',
  'Description of what this action does',
  false,    // hideBeforeStart
  false,    // hideAfterStart
  false,    // hideAfterComplete
  'fa fa-cog' // actionIcon (FontAwesome)
);

// Check for custom action triggers
if (castService._customAction) {
  // Check which custom action was triggered
  for (let i = 0; i < castService.customActionList.length; i++) {
    if (castService.customActionStateList[i]) {
      const actionName = castService.customActionList[i];
      // Handle the custom action
      castService.customActionStateList[i] = false;
    }
  }
}
```

### File Operations

Upload files or entire directories:

```javascript
// Upload a single file (will be compressed as ZIP)
await castService.uploadFile('./results/', 'results.json');

// Upload entire output directory
await castService.uploadOutputFolder('./output/', './');
```

## API Reference

### Properties

- `startmyuuidAsString`: Unique UUID for this client instance
- `currentUUID`: RabbitMQ queue name
- `_startRun`: START action flag
- `_stopRun`: STOP action flag
- `_pauseRun`: PAUSE action flag
- `_resumeRun`: RESUME action flag
- `_abortRun`: ABORT action flag
- `_customAction`: Custom action flag
- `customActionList`: Array of registered custom action names
- `customActionStateList`: Array of custom action states

### Methods

#### `async startService()`
Initialize the service, connect to RabbitMQ, and start listening for messages.

#### `async stopService()`
Notify CAST Server that the service is stopping.

#### `async updateState(state, color = 'black')`
Update the current state of the framework.

**Parameters:**
- `state` (string): Description of current state
- `color` (string): Color for UI display (default: 'black')

#### `async updateResult(result)`
Send execution results to CAST Server.

**Parameters:**
- `result` (string): Result data to send

#### `async updateFrameworkFunctionality(...)`
Register framework capabilities and metadata.

**Parameters:**
- `startEnabled` (boolean)
- `stopEnabled` (boolean)
- `pauseEnabled` (boolean)
- `resumeEnabled` (boolean)
- `abortEnabled` (boolean)
- `restartEnabled` (boolean)
- `uploadResultEnabled` (boolean)
- `frameworkName` (string)
- `filterOnGroup` (string)
- `filterOnOwner` (string)
- `filterOnLocation` (string)
- `filterOnKeyword` (string, optional)

#### `async registerAction(actionName, actionDescription, hideBeforeStart, hideAfterStart, hideAfterComplete, actionIcon)`
Register a custom action.

#### `async uploadFile(pathReference, fileName, cleanupExistingZip = true)`
Upload a file (will be automatically compressed).

#### `async uploadOutputFolder(pathReference, workingDirectory, cleanupZip = true)`
Upload an entire directory as a ZIP file.

#### `async closeQueue()`
Close the RabbitMQ queue assigned to this service.

#### `async close()`
Close the RabbitMQ connection gracefully.

## Testing

Run unit tests:

```bash
npm test
```

Run tests in watch mode:

```bash
npm run test:watch
```

## Debugging

Debug logging is enabled by default. The service writes to `temp.log` file:

```javascript
// To disable debug logging
castService.inDebugMode = false;

// To enable debug logging
castService.inDebugMode = true;
```

Log entries include:
- Service startup/shutdown
- Received messages
- File operations
- Database update statements

## Architecture

### Message Flow

1. **Client Registration**: Service generates UUID and registers with CAST Server
2. **Action Listener**: Service listens on RabbitMQ queue for action messages
3. **Action Processing**: Receives START, STOP, PAUSE, RESUME, ABORT commands
4. **State Updates**: Framework sends state and result updates to CAST
5. **File Upload**: Compressed files sent to File Storage Service
6. **Cleanup**: Service terminates gracefully

### RabbitMQ Integration

- Exchanges: `logger`, `file_storage`
- Routing Keys: `logger_service`, `file_storage_service`
- Queue: Dynamically named with UUID
- Auto-acknowledgment enabled

## Compatibility

- **Node.js**: 16.0.0 or higher
- **RabbitMQ**: 3.x or higher

## Author

Kevin Snyder

## Version

1.0.5

## License

MIT

## Related Projects

- [CAST_Client_Service](../CAST_Client_Service/) - C# implementation
- CAST Server
- CAST File Storage Service
- CAST Logger Service
