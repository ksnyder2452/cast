import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import CAST_Client_Service from '../src/CAST_Client_Service.js';

describe('CAST_Client_Service', () => {
    let service;

    beforeEach(() => {
        service = new CAST_Client_Service();
        service.inDebugMode = false;
        service.channel = {
            ack: jest.fn(),
            publish: jest.fn(() => true),
            deleteQueue: jest.fn(async () => { }),
            close: jest.fn(async () => { }),
        };
        service.dllIsRegistered = true;
    });

    test('handles start action and clears conflicting flags', async () => {
        service._stopRun = true;
        service._pauseRun = true;
        service._resumeRun = true;
        service._abortRun = true;
        service._customAction = true;

        await service.handleMessage({
            content: Buffer.from('ACTION: START RUN'),
            properties: { headers: {} },
        });

        expect(service._startRun).toBe(true);
        expect(service._stopRun).toBe(false);
        expect(service._pauseRun).toBe(false);
        expect(service._resumeRun).toBe(false);
        expect(service._abortRun).toBe(false);
        expect(service._customAction).toBe(false);
        expect(service.channel.ack).toHaveBeenCalledTimes(1);
    });

    test('tracks and toggles custom action state', async () => {
        service.customActionList = ['Do Work'];
        service.customActionStateList = [false];

        await service.handleMessage({
            content: Buffer.from('action: custom action Do Work'),
            properties: { headers: {} },
        });

        expect(service._customAction).toBe(true);
        expect(service.customActionStateList[0]).toBe(true);

        const result = service.callCustomAction('abc', 'ACTION: CUSTOM ACTION Do Work');
        expect(result).toBe('Found CUSTOM action');
        expect(service._customAction).toBe(false);
        expect(service.customActionStateList[0]).toBe(false);
    });

    test('publishes state updates through default exchange', async () => {
        await service.updateState('READY', 'green');

        expect(service.channel.publish).toHaveBeenCalledTimes(1);
        const [exchange, routingKey, payload] = service.channel.publish.mock.calls[0];
        expect(exchange).toBe('');
        expect(routingKey).toBe('logger_service');
        expect(payload.toString()).toContain('insert into state');
        expect(payload.toString()).toContain("'READY'");
    });

    test('always updates filter_on_keyword in framework registration', async () => {
        await service.updateFrameworkFunctionality(
            true,
            true,
            true,
            true,
            true,
            false,
            true,
            'Framework',
            'Group',
            'Owner',
            'Location',
            null
        );

        const sqlPayloads = service.channel.publish.mock.calls
            .map((call) => call[2].toString());

        const keywordUpdate = sqlPayloads.find((sql) =>
            sql.includes('update logger set filter_on_keyword')
        );

        expect(keywordUpdate).toBeDefined();
        expect(keywordUpdate).toContain("filter_on_keyword = ''");
    });

    test('uploadResultFolder delegates with Java-compatible zip name', async () => {
        const spy = jest
            .spyOn(service, 'uploadFolderAsZip')
            .mockResolvedValue(undefined);

        await service.uploadResultFolder('results', './', true);

        expect(spy).toHaveBeenCalledWith('results', './', 'current_results.zip', true);
    });
});
