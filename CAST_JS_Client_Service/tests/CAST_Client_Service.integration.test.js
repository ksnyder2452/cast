import { describe, test, expect, beforeEach, afterEach, jest } from '@jest/globals';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';

describe('CAST_Client_Service integration smoke', () => {
    let originalCwd;
    let tempDir;

    beforeEach(() => {
        originalCwd = process.cwd();
        tempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'cast-js-client-'));
        process.chdir(tempDir);

        fs.writeFileSync(
            path.join(tempDir, 'cast.properties'),
            [
                'rabbitmq_home=localhost',
                'rabbitmq_port=5672',
                'rabbitmq_user=js_client_admin',
                'rabbitmq_pwd=secret_pwd',
                'reloadUUID=no',
            ].join('\n'),
            'utf-8'
        );
    });

    afterEach(() => {
        process.chdir(originalCwd);
        fs.rmSync(tempDir, { recursive: true, force: true });
        jest.resetModules();
    });

    test('startService wires RabbitMQ, publishes setup SQL, and consumes actions', async () => {
        const mockChannel = {
            publish: jest.fn(() => true),
            assertQueue: jest.fn(async () => { }),
            consume: jest.fn(async () => { }),
            ack: jest.fn(),
            close: jest.fn(async () => { }),
        };

        const mockConnection = {
            createChannel: jest.fn(async () => mockChannel),
            close: jest.fn(async () => { }),
        };

        const connectMock = jest.fn(async () => mockConnection);

        await jest.unstable_mockModule('amqplib', () => ({
            default: {
                connect: connectMock,
            },
        }));

        const { default: CAST_Client_Service } = await import('../src/CAST_Client_Service.js');
        const service = new CAST_Client_Service();
        service.inDebugMode = false;

        await service.startService();

        expect(connectMock).toHaveBeenCalledWith(
            'amqp://js_client_admin:secret_pwd@localhost:5672'
        );
        expect(mockConnection.createChannel).toHaveBeenCalledTimes(1);
        expect(mockChannel.assertQueue).toHaveBeenCalledWith(service.currentUUID, {
            durable: false,
            exclusive: false,
            autoDelete: false,
        });

        const publishedSql = mockChannel.publish.mock.calls[0][2].toString();
        expect(mockChannel.publish.mock.calls[0][0]).toBe('');
        expect(mockChannel.publish.mock.calls[0][1]).toBe('logger_service');
        expect(publishedSql).toContain('insert into logger');
        expect(publishedSql).toContain('Started Client Service for UUID');

        const consumeHandler = mockChannel.consume.mock.calls[0][1];
        await consumeHandler({
            content: Buffer.from('ACTION: START RUN'),
            properties: { headers: {} },
        });

        expect(service._message).toBe('ACTION: START RUN');
        expect(service._startRun).toBe(true);
        expect(service._stopRun).toBe(false);
        expect(mockChannel.ack).toHaveBeenCalledTimes(1);

        await service.close();
        expect(mockChannel.close).toHaveBeenCalledTimes(1);
        expect(mockConnection.close).toHaveBeenCalledTimes(1);
    });
});
