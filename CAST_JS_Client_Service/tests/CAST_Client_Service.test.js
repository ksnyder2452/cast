import CAST_Client_Service from '../src/CAST_Client_Service.js';

/**
 * Unit tests for the CAST_Client_Service class
 * Tests cover action processing methods and state management
 */
describe('CAST_Client_Service_ActionTests', () => {
    let service;

    beforeEach(() => {
        service = new CAST_Client_Service();
    });

    describe('startRun', () => {
        test('should return "Found START action" with valid START action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: START run_id_456';

            // Act
            const result = service.startRun(serviceUUID, action);

            // Assert
            expect(result).toBe('Found START action');
        });

        test('should return empty string with invalid action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: STOP run_id_456';

            // Act
            const result = service.startRun(serviceUUID, action);

            // Assert
            expect(result).toBe('');
        });

        test('should return "Found START action" with lowercase action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'action: start run_id_456';

            // Act
            const result = service.startRun(serviceUUID, action);

            // Assert
            expect(result).toBe('Found START action');
        });

        test('should return "Found START action" with mixed case action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'Action: Start run_id_456';

            // Act
            const result = service.startRun(serviceUUID, action);

            // Assert
            expect(result).toBe('Found START action');
        });
    });

    describe('pauseRun', () => {
        test('should return empty string with invalid action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: RESUME run_id_456';
            service._pauseRun = false;

            // Act
            const result = service.pauseRun(serviceUUID, action);

            // Assert
            expect(result).toBe('');
            expect(service._pauseRun).toBe(false);
        });

        test('should set _pauseRun to true with valid PAUSE action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: PAUSE run_id_456';
            service._pauseRun = false;

            // Act
            const result = service.pauseRun(serviceUUID, action);

            // Assert
            expect(result).toBe('Found PAUSE action');
            expect(service._pauseRun).toBe(true);
        });
    });

    describe('resumeRun', () => {
        test('should return "Found RESUME action" with valid RESUME action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: RESUME run_id_456';
            service._resumeRun = true;

            // Act
            const result = service.resumeRun(serviceUUID, action);

            // Assert
            expect(result).toBe('Found RESUME action');
            expect(service._resumeRun).toBe(false);
        });

        test('should return empty string with invalid action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: START run_id_456';

            // Act
            const result = service.resumeRun(serviceUUID, action);

            // Assert
            expect(result).toBe('');
        });
    });

    describe('abortRun', () => {
        test('should return "Found ABORT action" with valid ABORT action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: ABORT run_id_456';
            service._abortRun = false;

            // Act
            const result = service.abortRun(serviceUUID, action);

            // Assert
            expect(result).toBe('Found ABORT action');
            expect(service._abortRun).toBe(false);
        });

        test('should return empty string with invalid action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: START run_id_456';

            // Act
            const result = service.abortRun(serviceUUID, action);

            // Assert
            expect(result).toBe('');
        });
    });

    describe('callCustomAction', () => {
        test('should return "Found CUSTOM action" with valid CUSTOM ACTION', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: CUSTOM ACTION test-action';
            service._customAction = true;
            service.customActionList.push('test-action');
            service.customActionStateList.push(true);

            // Act
            const result = service.callCustomAction(serviceUUID, action);

            // Assert
            expect(result).toBe('Found CUSTOM action');
            expect(service._customAction).toBe(false);
            expect(service.customActionStateList[0]).toBe(false);
        });

        test('should return empty string with invalid action', () => {
            // Arrange
            const serviceUUID = 'test-uuid-123';
            const action = 'ACTION: START run_id_456';

            // Act
            const result = service.callCustomAction(serviceUUID, action);

            // Assert
            expect(result).toBe('');
        });
    });

    describe('State Management', () => {
        test('should initialize with default state values', () => {
            // Assert
            expect(service._stopRun).toBe(false);
            expect(service._pauseRun).toBe(false);
            expect(service._startRun).toBe(false);
            expect(service._resumeRun).toBe(false);
            expect(service._abortRun).toBe(false);
            expect(service._customAction).toBe(false);
        });

        test('should have empty custom action lists', () => {
            // Assert
            expect(service.customActionList).toEqual([]);
            expect(service.customActionStateList).toEqual([]);
        });

        test('should generate unique UUIDs', () => {
            // Arrange
            const service2 = new CAST_Client_Service();

            // Assert
            expect(service.startmyuuidAsString).not.toBe(
                service2.startmyuuidAsString
            );
        });
    });
});
