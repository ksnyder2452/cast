import amqp from 'amqplib';
import * as fs from 'fs';
import * as path from 'path';
import { createWriteStream } from 'fs';
import archiver from 'archiver';
import { v4 as uuidv4 } from 'uuid';

/**
 * CAST Client Service - JavaScript Implementation
 * 
 * This class is the integration point into the Centralized Automation for Software Tools (CAST)
 * Add this module to your custom framework
 * 
 * The key methods to call within your custom framework are:
 *    1. updateFrameworkFunctionality
 *    2. updateState
 *    3. updateResult
 *    4. uploadResultFolder
 * 
 * The key fields to track CAST Action requests are:
 *    1. _startRun
 *    2. _stopRun
 *    3. _pauseRun
 *    4. _resumeRun
 *    5. _abortRun
 */

class CAST_Client_Service {
    constructor() {
        // Generate a unique UUID for this client instance
        this.startmyuuid = uuidv4();
        this.startmyuuidAsString = this.startmyuuid;

        /**
         * currentUUID is the RabbitMQ Queue name
         */
        this.currentUUID = 'client_service_' + this.startmyuuidAsString;

        /**
         * rootDir is the root directory for your framework
         */
        this.rootDir = path.join(process.cwd(), 'simulator');

        /**
         * downloadQueueDir contains files downloaded from the File Storage Service
         */
        this.downloadQueueDir = path.join(this.rootDir, 'download_queue');

        /**
         * uploadQueueDir contains files to be uploaded to the File Storage Service
         */
        this.uploadQueueDir = path.join(this.rootDir, 'upload_queue');

        /**
         * Action state flags from CAST Server
         */
        this._stopRun = false;
        this._pauseRun = false;
        this._startRun = false;
        this._resumeRun = false;
        this._abortRun = false;
        this._customAction = false;

        /**
         * Custom action tracking
         */
        this.customActionList = [];
        this.customActionStateList = [];

        /**
         * Last message value for compatibility with Java client behavior
         */
        this._message = '';

        /**
         * Reload UUID functionality
         */
        this.reloadUUID = false;

        /**
         * Debug logging
         */
        this.tempLog = path.join('.', 'temp.log');
        this.inDebugMode = true;

        /**
         * RabbitMQ Configuration - set from properties file
         */
        this.rabbitmq_hostname = '';
        this.rabbitmq_port = '';
        this.rabbitmq_user = '';
        this.rabbitmq_pwd = '';

        /**
         * RabbitMQ Connection Factory
         */
        this.connection = null;
        this.channel = null;

        /**
         * Service registration flag
         */
        this.dllIsRegistered = false;
    }

    /**
     * Thread-safe logging method to write to temp.log
     */
    logToFile(message) {
        if (!this.inDebugMode) return;

        try {
            fs.appendFileSync(this.tempLog, message + '\n', { flag: 'a' });
        } catch (error) {
            // Silently fail if logging fails to prevent application crashes
            console.error('Logging failed:', error.message);
        }
    }

    /**
     * Parse properties file in key=value format
     */
    parsePropertiesFile(filePath) {
        const data = {};
        try {
            const content = fs.readFileSync(filePath, 'utf-8');
            const lines = content.split('\n');
            for (const line of lines) {
                const trimmedLine = line.trim();
                if (trimmedLine && !trimmedLine.startsWith('#')) {
                    const [key, ...valueParts] = trimmedLine.split('=');
                    if (key) {
                        data[key.trim()] = valueParts.join('=').trim();
                    }
                }
            }
        } catch (error) {
            console.error('Error reading properties file:', error.message);
        }
        return data;
    }

    /**
     * Normalize RabbitMQ header values (Buffer|string) to plain strings
     */
    headerValueToString(value) {
        if (Buffer.isBuffer(value)) {
            return value.toString('utf-8');
        }
        if (value === undefined || value === null) {
            return '';
        }
        return String(value);
    }

    /**
     * Wait until registration has completed before sending updates
     */
    async waitForRegistration(maxAttempts = 300) {
        let attempts = 0;
        while (!this.dllIsRegistered && attempts < maxAttempts) {
            await new Promise((resolve) => setTimeout(resolve, 1000));
            attempts++;
        }
    }

    /**
     * Publish to a queue via the default direct exchange (parity with C#/Java)
     */
    publishToQueue(queueName, payload, options = {}) {
        if (!this.channel) {
            return false;
        }
        return this.channel.publish('', queueName, Buffer.from(payload), options);
    }

    /**
     * Setup and register the CAST Client environment and listen for Action requests
     */
    async startService() {
        try {
            const propertiesFileReference = path.join(process.cwd(), 'cast.properties');
            const originalPropertiesFileReference = path.join(
                process.cwd(),
                'cast.properties'
            );
            const fallbackPropertiesFileReference = path.join(
                process.cwd(),
                'src',
                'cast.properties'
            );

            if (this.inDebugMode) {
                this.logToFile(`[x] propertiesFileReference = ${propertiesFileReference}`);
            }

            const propertyFileToRead = fs.existsSync(propertiesFileReference)
                ? propertiesFileReference
                : fallbackPropertiesFileReference;

            const data = this.parsePropertiesFile(propertyFileToRead);

            this.rabbitmq_hostname = data['rabbitmq_home'] || 'localhost';
            this.rabbitmq_port = data['rabbitmq_port'] || '5672';
            this.rabbitmq_user = data['rabbitmq_user'] || 'guest';
            this.rabbitmq_pwd = data['rabbitmq_pwd'] || 'guest';

            if (data['currentUUID'] && data['reloadUUID'] === 'yes') {
                this.currentUUID = data['currentUUID'];
                this.reloadUUID = true;
            } else if (data['reloadUUID'] === 'yes' && !data['currentUUID']) {
                fs.appendFileSync(
                    originalPropertiesFileReference,
                    `currentUUID=${this.currentUUID}\n`
                );
            }

            // Clean up temp log
            if (fs.existsSync(this.tempLog)) {
                fs.unlinkSync(this.tempLog);
            }

            // Create required directories
            fs.mkdirSync(this.rootDir, { recursive: true });
            fs.mkdirSync(this.downloadQueueDir, { recursive: true });
            fs.mkdirSync(this.uploadQueueDir, { recursive: true });

            // Connect to RabbitMQ
            const url = `amqp://${this.rabbitmq_user}:${this.rabbitmq_pwd}@${this.rabbitmq_hostname}:${this.rabbitmq_port}`;
            this.connection = await amqp.connect(url);
            this.channel = await this.connection.createChannel();

            // Notify Logger Service that we are awake
            let startClientService = '';
            if (!this.reloadUUID) {
                startClientService =
                    `insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) ` +
                    `values('${this.startmyuuidAsString}', '${this.startmyuuidAsString}', '${this.currentUUID}', 'INFO', ` +
                    `'Started Client Service for UUID ${this.startmyuuidAsString}', NOW(), 'SETUP New Framework - IGNORE THIS ENTRY')`;
            }

            if (startClientService) {
                this.publishToQueue('logger_service', startClientService);
            }

            this.dllIsRegistered = true;

            if (this.inDebugMode) {
                this.logToFile('[x] Started Client Service');
            }


            // Setup queue and start listening
            await this.channel.assertQueue(this.currentUUID, {
                durable: false,
                exclusive: false,
                autoDelete: false,
            });

            if (this.inDebugMode) {
                this.logToFile(`[x] Waiting for messages within ${this.currentUUID}`);
            }

            await this.channel.consume(this.currentUUID, async (message) => {
                if (message) {
                    await this.handleMessage(message);
                }
            });

            if (this.inDebugMode) {
                this.logToFile('[x] Consumer started, waiting for messages...');
            }
        } catch (error) {
            console.error('Error in startService:', error);
            this.logToFile(`Error in startService: ${error.message}`);
        }
    }

    /**
     * Handle incoming RabbitMQ messages
     */
    async handleMessage(message) {
        try {
            const body = message.content.toString();
            const headers = message.properties.headers || {};
            this._message = body;

            if (this.inDebugMode) {
                this.logToFile(`[x] Received: ${body}`);
            }

            if (body.toUpperCase().startsWith('PUSH FILE: ')) {
                const pathName = this.headerValueToString(headers['pathName']);
                const fileName = this.headerValueToString(headers['fileName']);

                const fullPath = path.join(this.downloadQueueDir, pathName);
                fs.mkdirSync(fullPath, { recursive: true });

                if (this.inDebugMode) {
                    this.logToFile(`pathName = ${pathName}`);
                    this.logToFile(`fileName = ${fileName}`);
                }

                fs.writeFileSync(path.join(fullPath, fileName), message.content);
            } else if (body.trim().toUpperCase().endsWith('START RUN')) {
                if (this.inDebugMode) {
                    this.logToFile('[x] Queued start message for the local framework');
                }
                this._startRun = true;
                this._stopRun = false;
                this._pauseRun = false;
                this._resumeRun = false;
                this._abortRun = false;
                this._customAction = false;
            } else if (body.trim().toUpperCase().endsWith('STOP RUN')) {
                if (this.inDebugMode) {
                    this.logToFile('[x] Queued stop message for the local framework');
                }
                this._stopRun = true;
                this._pauseRun = false;
                this._resumeRun = false;
                this._abortRun = false;
                this._customAction = false;
            } else if (body.trim().toUpperCase().endsWith('PAUSE RUN')) {
                if (this.inDebugMode) {
                    this.logToFile('[x] Queued pause message for the local framework');
                }
                this._stopRun = false;
                this._pauseRun = true;
                this._resumeRun = false;
                this._abortRun = false;
                this._customAction = false;
            } else if (body.trim().toUpperCase().endsWith('RESUME RUN')) {
                if (this.inDebugMode) {
                    this.logToFile('[x] Queued resume message for the local framework');
                }
                this._stopRun = false;
                this._pauseRun = false;
                this._resumeRun = true;
                this._abortRun = false;
                this._customAction = false;
            } else if (body.trim().toUpperCase().endsWith('ABORT RUN')) {
                if (this.inDebugMode) {
                    this.logToFile('[x] Queued abort message for the local framework');
                }
                this._stopRun = false;
                this._pauseRun = false;
                this._resumeRun = false;
                this._abortRun = true;
                this._customAction = false;
            } else if (body.toUpperCase().includes('CUSTOM ACTION')) {
                if (this.inDebugMode) {
                    this.logToFile('[x] Queued custom action message for the local framework');
                }
                this._customAction = true;
                const actionIndex = body.toLowerCase().indexOf('custom action ') + 14;
                const actionName = body.substring(actionIndex);
                for (let counter = 0; counter < this.customActionList.length; counter++) {
                    if (this.customActionList[counter] === actionName) {
                        this.customActionStateList[counter] = true;
                    }
                }
            } else {
                if (this.inDebugMode) {
                    this.logToFile('Received file');
                }
            }

            // Acknowledge the message
            this.channel.ack(message);
        } catch (error) {
            console.error('Error handling message:', error);
            this.logToFile(`Error handling message: ${error.message}`);
        }
    }

    /**
     * Process Start Action Requests
     */
    startRun(serviceUUID, action) {
        let result = '';
        if (action.toUpperCase().startsWith('ACTION: START ')) {
            if (this.inDebugMode) {
                this.logToFile(`${action} for ${serviceUUID}`);
            }
            result = 'Found START action';
        }
        return result;
    }

    /**
     * Process Pause Action Requests
     */
    pauseRun(serviceUUID, action) {
        let result = '';
        if (action.toUpperCase().startsWith('ACTION: PAUSE ')) {
            if (this.inDebugMode) {
                this.logToFile(`${action} for ${serviceUUID}`);
            }
            result = 'Found PAUSE action';
            this._pauseRun = true;
        }
        return result;
    }

    /**
     * Process Resume Action Requests
     */
    resumeRun(serviceUUID, action) {
        let result = '';
        if (action.toUpperCase().startsWith('ACTION: RESUME ')) {
            if (this.inDebugMode) {
                this.logToFile(`${action} for ${serviceUUID}`);
            }
            result = 'Found RESUME action';
            this._resumeRun = false;
        }
        return result;
    }

    /**
     * Process Abort Action Requests
     */
    abortRun(serviceUUID, action) {
        let result = '';
        if (action.toUpperCase().startsWith('ACTION: ABORT ')) {
            if (this.inDebugMode) {
                this.logToFile(`${action} for ${serviceUUID}`);
            }
            result = 'Found ABORT action';
            this._abortRun = false;
        }
        return result;
    }

    /**
     * Process Custom Action Requests
     */
    callCustomAction(serviceUUID, action) {
        let result = '';
        if (action.toUpperCase().startsWith('ACTION: CUSTOM ACTION ')) {
            if (this.inDebugMode) {
                this.logToFile(`${action} for ${serviceUUID}`);
            }
            result = 'Found CUSTOM action';
            this._customAction = false;
            const actionIndex = action.toLowerCase().indexOf('custom action ') + 14;
            const actionName = action.substring(actionIndex);
            for (let counter = 0; counter < this.customActionList.length; counter++) {
                if (this.customActionList[counter] === actionName) {
                    this.customActionStateList[counter] = false;
                }
            }
        }
        return result;
    }

    /**
     * Stop the service and notify CAST Server
     */
    async stopService() {
        try {
            const startClientService =
                `insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) ` +
                `values('${this.startmyuuidAsString}', '${this.startmyuuidAsString}', '${this.currentUUID}', 'INFO', ` +
                `'Stopped Client Service for UUID ${this.startmyuuidAsString}', NOW())`;

            this.publishToQueue('logger_service', startClientService);
        } catch (error) {
            console.error('Error in stopService:', error);
            this.logToFile(`Error in stopService: ${error.message}`);
        }
    }

    /**
     * Upload a file to the CAST File Storage Service (zipped)
     * Note: There is a 10MB size limit on the file
     */
    async uploadFile(pathReference, fileName, cleanupExistingZip = true) {
        try {
            if (this.inDebugMode) {
                this.logToFile(`Upload ${pathReference}${fileName}`);
            }

            const fileNameWithoutExt = fileName.substring(
                0,
                fileName.lastIndexOf('.')
            );
            const zipFileName = `${fileNameWithoutExt}.zip`;
            const zipFilePath = path.join(pathReference, zipFileName);

            if (cleanupExistingZip && fs.existsSync(zipFilePath)) {
                fs.unlinkSync(zipFilePath);
            }

            // Create zip file
            await this.createZipFile(
                path.join(pathReference, fileName),
                zipFilePath,
                fileName
            );

            // Read zip file
            const fileBytes = fs.readFileSync(zipFilePath);

            // Prepare properties
            const props = {
                headers: {
                    pathName: this.currentUUID,
                    fileName: zipFileName,
                    originator: this.currentUUID,
                    type: 'INFO',
                    message: `Send file ${fileName}`,
                },
            };

            if (this.channel) {
                await this.channel.publish(
                    '',
                    'file_storage_service',
                    fileBytes,
                    props
                );
            }

            // Cleanup
            if (cleanupExistingZip && fs.existsSync(zipFilePath)) {
                fs.unlinkSync(zipFilePath);
            }
        } catch (error) {
            console.error('Error in uploadFile:', error);
            this.logToFile(`Error in uploadFile: ${error.message}`);
        }
    }

    /**
     * Upload the contents of the output folder to CAST File Storage Service
     */
    async uploadOutputFolder(pathReference, workingDirectory, cleanupZip = true) {
        return this.uploadFolderAsZip(pathReference, workingDirectory, 'current_output.zip', cleanupZip);
    }

    /**
     * Upload result folder using the Java client zip name for compatibility
     */
    async uploadResultFolder(pathReference, workingDirectory, cleanupZip = true) {
        return this.uploadFolderAsZip(pathReference, workingDirectory, 'current_results.zip', cleanupZip);
    }

    /**
     * Shared folder upload implementation used by output/result variants
     */
    async uploadFolderAsZip(pathReference, workingDirectory, zipFileName, cleanupZip = true) {
        try {
            if (!pathReference.endsWith(path.sep)) {
                pathReference = pathReference + path.sep;
            }

            if (this.inDebugMode) {
                this.logToFile(`Upload contents of ${pathReference}`);
            }

            const zipFilePath = path.join(workingDirectory, zipFileName);

            if (this.inDebugMode) {
                this.logToFile(`zipFileName = ${zipFileName}`);
                this.logToFile(`zipFilePath = ${zipFilePath}`);
            }

            // Create zip from directory
            await this.createZipFromDirectory(pathReference, zipFilePath);

            // Read zip file
            const fileBytes = fs.readFileSync(zipFilePath);

            // Prepare properties
            const props = {
                headers: {
                    pathName: this.currentUUID,
                    fileName: zipFileName,
                    originator: this.currentUUID,
                    type: 'INFO',
                    message: `Send file ${zipFileName}`,
                },
            };

            if (this.channel) {
                await this.channel.publish(
                    '',
                    'file_storage_service',
                    fileBytes,
                    props
                );
            }

            // Cleanup
            if (cleanupZip && fs.existsSync(zipFilePath)) {
                fs.unlinkSync(zipFilePath);
            }
        } catch (error) {
            console.error('Error in uploadFolderAsZip:', error);
            this.logToFile(`Error in uploadFolderAsZip: ${error.message}`);
        }
    }

    /**
     * Create a zip file from a single file
     */
    createZipFile(filePath, zipPath, fileName) {
        return new Promise((resolve, reject) => {
            const output = createWriteStream(zipPath);
            const archive = archiver('zip', { zlib: { level: 9 } });

            output.on('close', () => resolve());
            archive.on('error', (err) => reject(err));

            archive.pipe(output);
            archive.file(filePath, { name: fileName });
            archive.finalize();
        });
    }

    /**
     * Create a zip file from a directory
     */
    createZipFromDirectory(dirPath, zipPath) {
        return new Promise((resolve, reject) => {
            const output = createWriteStream(zipPath);
            const archive = archiver('zip', { zlib: { level: 9 } });

            output.on('close', () => resolve());
            archive.on('error', (err) => reject(err));

            archive.pipe(output);
            archive.directory(dirPath, false);
            archive.finalize();
        });
    }

    /**
     * Update CAST with the current state of the framework
     */
    async updateState(state, color = 'black') {
        try {
            await this.waitForRegistration();

            const sanitizedState = state.replace(/'/g, "\\'");
            const stateUUID = uuidv4();

            const startClientService =
                `insert into state (uuid, reference_uuid, state, event_time_dt, color) ` +
                `values('${stateUUID}', '${this.startmyuuidAsString}', '${sanitizedState}', NOW(), '${color}')`;

            this.publishToQueue('logger_service', startClientService);

            return 'Queue updated';
        } catch (error) {
            console.error('Error in updateState:', error);
            this.logToFile(`Error in updateState: ${error.message}`);
            throw error;
        }
    }

    /**
     * Close the RabbitMQ Queue assigned to the framework
     */
    async closeQueue() {
        try {
            if (this.channel) {
                await this.channel.deleteQueue(this.currentUUID);
            }
        } catch (error) {
            console.error('Error in closeQueue:', error);
            this.logToFile(`Error in closeQueue: ${error.message}`);
        }
    }

    /**
     * Update data on the CAST Server during a run
     */
    async updateResult(result) {
        try {
            await this.waitForRegistration();

            const sanitizedResult = result.replace(/'/g, "\\'");
            const stateUUID = uuidv4();

            const startClientService =
                `insert into results (uuid, reference_uuid, result, event_time_dt) ` +
                `values('${stateUUID}', '${this.startmyuuidAsString}', '${sanitizedResult}', NOW())`;

            this.publishToQueue('logger_service', startClientService);
        } catch (error) {
            console.error('Error in updateResult:', error);
            this.logToFile(`Error in updateResult: ${error.message}`);
        }
    }

    /**
     * Register the framework within the CAST Server
     */
    async updateFrameworkFunctionality(
        startEnabled,
        stopEnabled,
        pauseEnabled,
        resumeEnabled,
        abortEnabled,
        restartEnabled,
        uploadResultEnabled,
        frameworkName,
        filterOnGroup,
        filterOnOwner,
        filterOnLocation,
        filterOnKeyword = null
    ) {
        try {
            await this.waitForRegistration();

            const stateUUID = uuidv4();
            const start = startEnabled ? '1' : '0';
            const stop = stopEnabled ? '1' : '0';
            const pause = pauseEnabled ? '1' : '0';
            const resume = resumeEnabled ? '1' : '0';
            const abort = abortEnabled ? '1' : '0';
            const uploadResult = uploadResultEnabled ? '1' : '0';
            const restart = restartEnabled ? '1' : '0';

            let startClientService =
                `insert into client_functionality (uuid, reference_uuid, start_supported, stop_supported, ` +
                `pause_supported, resume_supported, abort_supported, restart_supported, upload_supported, event_time_dt) ` +
                `values('${stateUUID}', '${this.startmyuuidAsString}', ${start}, ${stop}, ${pause}, ${resume}, ${abort}, ${restart}, ${uploadResult}, NOW())`;

            if (this.channel) {
                this.publishToQueue('logger_service', startClientService);

                // Update Framework Name
                const sanitizedName = frameworkName.replace(/'/g, "\\'");
                startClientService =
                    `update logger set display_name = '${sanitizedName}' where reference_uuid = '${this.startmyuuidAsString}'`;
                if (this.inDebugMode) {
                    this.logToFile(`Update Display Name using SQL ${startClientService}`);
                }
                this.publishToQueue('logger_service', startClientService);

                // Update Framework Group Filter
                const sanitizedGroup = filterOnGroup.replace(/'/g, "\\'");
                startClientService =
                    `update logger set filter_on_group = '${sanitizedGroup}' where reference_uuid = '${this.startmyuuidAsString}'`;
                if (this.inDebugMode) {
                    this.logToFile(`Update Filter On Group using SQL ${startClientService}`);
                }
                this.publishToQueue('logger_service', startClientService);

                // Update Framework Owner Filter
                const sanitizedOwner = filterOnOwner.replace(/'/g, "\\'");
                startClientService =
                    `update logger set filter_on_owner = '${sanitizedOwner}' where reference_uuid = '${this.startmyuuidAsString}'`;
                if (this.inDebugMode) {
                    this.logToFile(`Update Filter On Owner using SQL ${startClientService}`);
                }
                this.publishToQueue('logger_service', startClientService);

                // Update Framework Location
                const sanitizedLocation = filterOnLocation.replace(/'/g, "\\'");
                startClientService =
                    `update logger set filter_on_location = '${sanitizedLocation}' where reference_uuid = '${this.startmyuuidAsString}'`;
                if (this.inDebugMode) {
                    this.logToFile(`Update Filter On Location using SQL ${startClientService}`);
                }
                this.publishToQueue('logger_service', startClientService);

                // Update Framework Keyword (always send this update for parity)
                const sanitizedKeyword = (filterOnKeyword ?? '').replace(/'/g, "\\'");
                startClientService =
                    `update logger set filter_on_keyword = '${sanitizedKeyword}' where reference_uuid = '${this.startmyuuidAsString}'`;
                if (this.inDebugMode) {
                    this.logToFile(`Update Filter On Keyword using SQL ${startClientService}`);
                }
                this.publishToQueue('logger_service', startClientService);
            }
        } catch (error) {
            console.error('Error in updateFrameworkFunctionality:', error);
            this.logToFile(
                `Error in updateFrameworkFunctionality: ${error.message}`
            );
        }
    }

    /**
     * Register a custom action
     */
    async registerAction(
        actionName,
        actionDescription,
        hideBeforeStart,
        hideAfterStart,
        hideAfterComplete,
        actionIcon = 'fa fa-check'
    ) {
        try {
            await this.waitForRegistration();

            const sanitizedActionName = actionName.replace(/'/g, "\\'");
            const originalActionName = sanitizedActionName;
            const qualifiedActionName = `${this.startmyuuidAsString}|${sanitizedActionName}`;
            const stateUUID = uuidv4();

            const registerActionSQL =
                `insert into custom_actions (uuid, reference_uuid, name, description, icon, hide_before_start, hide_after_start, hide_after_complete, event_time_dt) ` +
                `values('${stateUUID}', '${this.currentUUID}', '${qualifiedActionName}', '${actionDescription}', '${actionIcon}', ${hideBeforeStart}, ${hideAfterStart}, ${hideAfterComplete}, NOW())`;

            this.publishToQueue('logger_service', registerActionSQL);

            if (!this.customActionList.includes(originalActionName)) {
                this.customActionList.push(originalActionName);
                this.customActionStateList.push(false);
            }

            return 'custom action defined';
        } catch (error) {
            console.error('Error in registerAction:', error);
            this.logToFile(`Error in registerAction: ${error.message}`);
            throw error;
        }
    }

    /**
     * Close the connection gracefully
     */
    async close() {
        try {
            if (this.channel) {
                await this.channel.close();
            }
            if (this.connection) {
                await this.connection.close();
            }
        } catch (error) {
            console.error('Error closing connection:', error);
        }
    }
}

// Export as default and named export for flexibility
export default CAST_Client_Service;
export { CAST_Client_Service };
