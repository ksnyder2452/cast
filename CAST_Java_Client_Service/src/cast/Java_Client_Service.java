package cast;

import com.rabbitmq.client.*;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.Map;
import java.util.Properties;
import java.io.BufferedWriter;
import java.io.FileWriter;
import java.nio.charset.StandardCharsets;
import java.util.UUID;
import java.util.ArrayList;
import java.util.HashMap;


/**
 * @author kevin.snyder
 * @version 0.9
 * This class is the integration point into the CAST framework. Use the following procedure to
 * integrate your Project into CAST
 * 1. Add Java_Client_Service.jar to your library/classpath
 * 2. Create a config.properties in [your root folder]/resources/ with the following values
 *       rabbitmq_home
 *       rabbitmq_port
 *       rabbitmq_user
 *       rabbitmq_pwd
 * 3. Start the Client Service by calling Java_Client_Service.startService()
 * 4. Register your Project by calling updateFrameworkFunctionality()
 * 5. Update your Project state by calling updateState()
 * 6. Update your Project results by calling updateResult()
 * 7. Upload your Project results folder by calling updateResultFolder()
 * 8. Check on queued Action Request by calling the pertinent Fields
 * 9. Close the message queue when your Project run completes
 */
public class Java_Client_Service {
    /**
     * uuid is a unique identifier used to track historical data and to identify each Message Channel
     */
    static UUID uuid = UUID.randomUUID();
    /**
     * uuidAsString is the Client UUID
     * Used to send Action Requests from the REST Listener to your framework
     */
    public static String uuidAsString = uuid.toString();
    static String currentUUID = "client_service_" + uuidAsString;
    /**
     * rootDir is used to identify where the config.properties is located as well as where files are stored
     */
    static String rootDir = System.getProperty("user.dir");
    static String downloadQueueDir = rootDir + "download_queue" + System.getProperty("file.separator");
    static String uploadQueueDir = rootDir + "upload_queue" + System.getProperty("file.separator");
    /**
     * _stopRun is used to track stop requests from the UTAF Service
     */
    public static Boolean _stopRun = false;
    /**
     * _pauseRUn is used to track pause requests from the UTAF Service
     */
    public static Boolean _pauseRun = false;
    /**
     * _startRun is used to track start requests from the UTAF Service
     */
    public static Boolean _startRun = false;
    /**
     * _resumeRun is used to track resume requests from the UTAF Service
     */
    public static Boolean _resumeRun = false;
    /**
     * _abortRun is used to track abort requests from the UTAF Service
     */
    public static Boolean _abortRun = false;

    /**
     * _customAction is used to track custom action requests from the UTAF Service
     */
    public static Boolean _customAction = false;

    public static ArrayList<String> customActionList = new ArrayList<String>();
    public static ArrayList<Boolean> customActionStateList = new ArrayList<Boolean>();



    public static String _message = "";
    static Boolean reloadUUID = false;
    static String tempLog = "." + System.getProperty("file.separator") + "temp.log";
    static Boolean inDebugMode = true;
    /**
     * rabbitmq_home is the RabbitMQ Server
     */
    static String rabbitmq_home = "";
    /**
     * rabbitmq_port is the RabbitMQ Port
     */
    static String rabbitmq_port = "";
    /**
     * username is the RabbitMQ Account provided by the UTAF Administrator
     */
    static String rabbitmq_username = "";
    /**
     * password is the RabbitMQ Password provided by the UTAF Administrator
     */
    static String rabbitmq_password = "";
    static Boolean dllIsRegistered = false;

    public static void main(String[] argv) throws Exception {

    }


    /**
     * startService must be called by your Project to connect to the CAST backend service
     * The core functionality is
     *    1. Configure the connection to the RabbitMQ Server
     *    2. Register your Project with the CAST Server
     *    3. Continually poll for messages from other CAST Services
     *    4. Translate Action requests into public fields
     * This method is required
     * @throws Exception
     */
    public static void startService() throws Exception {
        Properties properties = new Properties();
        String filePath = "resources" + File.separator + "config.properties";

        try (FileInputStream fis = new FileInputStream(filePath)) {
            properties.load(fis); // Load the properties from the file
            rabbitmq_username = properties.getProperty("rabbitmq_user");
            rabbitmq_password = properties.getProperty("rabbitmq_pwd");
            rabbitmq_home = properties.getProperty("rabbitmq_home");
            rabbitmq_port = properties.getProperty("rabbitmq_port");

        } catch (IOException e) {
            System.err.println("Error reading properties file: " + e.getMessage());
            e.printStackTrace();
        } catch (NumberFormatException e) {
            System.err.println("Error parsing port number: " + e.getMessage());
            e.printStackTrace();
        }

        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        Connection connection = null;

        try {
            connection = factory.newConnection();
            final Channel channel = connection.createChannel();
            String startClientService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + uuidAsString + "', '" + uuidAsString + "', '" + currentUUID + "', 'INFO', 'Started Client Service for UUID " + uuidAsString + "', NOW(), 'SETUP New Framework - IGNORE THIS ENTRY')";
            channel.basicPublish("", "logger_service", null, startClientService.getBytes(StandardCharsets.UTF_8));
            dllIsRegistered = true;

            channel.queueDeclare(currentUUID, false, false, false, null);
            DeliverCallback deliverCallback = (consumerTag, delivery) -> {
                String message = new String(delivery.getBody(), "UTF-8");
                System.out.println(" [x] Received '" + message + "'");
                _message = message;
                channel.basicAck(delivery.getEnvelope().getDeliveryTag(), false); // Acknowledge message
                if (message.toUpperCase().endsWith("PUSH FILE: ")) {
                    Map fileReference = delivery.getProperties().getHeaders();
                    String pathName = (String)fileReference.get("pathName");
                    if (!pathName.endsWith(File.separator)) {
                        pathName = pathName + File.separator;
                    }
                    String fileName = (String)fileReference.get("fileName");
                    File uploadFile = new File(downloadQueueDir + pathName + fileName);
                    try (BufferedWriter writer = new BufferedWriter(new FileWriter(uploadFile))) {
                        writer.write(message);
                        System.out.println("Successfully wrote to " + fileName);
                    } catch (IOException e) {
                        System.err.println("Error writing to file: " + e.getMessage());
                    }
                }
                else if (message.toUpperCase().endsWith("START RUN")) {
                    _startRun = true;
                    _stopRun = false;
                    _pauseRun = false;
                    _resumeRun = false;
                    _abortRun = false;
                    _customAction = false;
                }
                else if (message.toUpperCase().endsWith("STOP RUN")) {
                    _stopRun = true;
                    _pauseRun = false;
                    _resumeRun = false;
                    _abortRun = false;
                    _customAction = false;
                }
                else if (message.toUpperCase().endsWith("PAUSE RUN")) {
                    _stopRun = false;
                    _pauseRun = true;
                    _resumeRun = false;
                    _abortRun = false;
                    _customAction = false;
                }
                else if (message.toUpperCase().endsWith("RESUME RUN")) {
                    _stopRun = false;
                    _pauseRun = false;
                    _resumeRun = true;
                    _abortRun = false;
                    _customAction = false;
                }
                else if (message.toUpperCase().endsWith("ABORT RUN")) {
                    _stopRun = false;
                    _pauseRun = false;
                    _resumeRun = false;
                    _abortRun = true;
                    _customAction = false;
                }
                else if (message.toUpperCase().contains("CUSTOM ACTION")) {
                    _customAction = true;
                    for (int counter = 0; counter < customActionList.size(); counter++) {
                        if (customActionList.get(counter).equals(message.substring(message.indexOf("custom action ") + 14))) {
                            customActionStateList.set(counter, true);
                        }
                    }
                }
                else {

                }
            };

            CancelCallback cancelCallback = consumerTag -> {
                System.out.println("Consumer " + consumerTag + " was cancelled.");
            };

            channel.basicConsume(currentUUID, false, deliverCallback, cancelCallback); // Set autoAck to false for manual acknowledgements
        }
        catch (Exception e) {

        }
    }

    /**
     * Receive and process a Start Run Action Request from the UTAF Server
     * @param service_uuid
     * @param action
     * @return
     */
    public static String startRun(String service_uuid, String action) {
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: START")) {
            result = "found START action";
        }
        return result;
    }

    /**
     * Receive and process a Pause Run Action Request from the UTAF Server
     * @param service_uuid
     * @param action
     * @return
     */
    public static String pauseRun(String service_uuid, String action) {
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: PAUSE")) {
            result = "found PAUSE action";
            _pauseRun = true;
        }
        return result;
    }

    /**
     * Receive and process a Resume Run Action Request from the UTAF Server
     * @param service_uuid
     * @param action
     * @return
     */
    public static String resumeRun(String service_uuid, String action) {
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: RESUME")) {
            result = "found RESUME action";
            _resumeRun = false;
        }
        return result;
    }

    /**
     * Receive and process an Abort Run Action Request from the UTAF Server
     * @param service_uuid
     * @param action
     * @return
     */
    public static String abortRun(String service_uuid, String action) {
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: ABORT")) {
            result = "found ABORT action";
            _abortRun = false;
        }
        return result;
    }

    /**
     * Receive and process a Custom Action Request from the UTAF Server
     * @param service_uuid
     * @param action
     * @return
     */
    public static String callCustomAction(String service_uuid, String action) {
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: CUSTOM")) {
            result = "found CUSTOM action";
            _customAction = false;
            for (int counter = 0; counter < customActionList.size(); counter++)
            {
                if (customActionList.get(counter).equals(action.substring(action.indexOf("custom action ") + 14)))
                {
                    customActionStateList.set(counter, false);
                }
            }

        }
        return result;
    }

    /**
     * Used to update the UTAF Server with our new state (offline)
     */
    public static void stopService() {
        String startClientService = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + uuidAsString + "', '" + uuidAsString + "', '" + currentUUID + "', 'INFO', 'Stopped Client Service for UUID '" + uuidAsString + "', NOW())";
        byte[] body = startClientService.getBytes(StandardCharsets.UTF_8);
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
    }

    /**
     * Used to update the CAST Server with our supported Framework functionality as well as the framework display name and filter options
     * @param startEnabled
     * @param stopEnabled
     * @param pauseEnabled
     * @param resumeEnabled
     * @param abortEnabled
     * @param restartEnabled
     * @param uploadResultEnabled
     * @param frameworkName
     * @param filterOnGroup
     * @param filterOnOwner
     * @param filterOnLocation
     */
    public static void updateFrameworkFunctionality(Boolean startEnabled, Boolean stopEnabled, Boolean pauseEnabled, Boolean resumeEnabled, Boolean abortEnabled, Boolean restartEnabled, Boolean uploadResultEnabled, String frameworkName, String filterOnGroup, String filterOnOwner, String filterOnLocation) {
        updateFrameworkFunctionality(startEnabled, stopEnabled, pauseEnabled, resumeEnabled, abortEnabled, restartEnabled, uploadResultEnabled, frameworkName, filterOnGroup, filterOnOwner, filterOnLocation, null);
    }

    /**
     * Used to update the CAST Server with our supported Framework functionality as well as the framework display name and filter options
     * filterOnKeyword format is |keyword1|keyword2|...|keyword#|
     * @param startEnabled
     * @param stopEnabled
     * @param pauseEnabled
     * @param resumeEnabled
     * @param abortEnabled
     * @param restartEnabled
     * @param uploadResultEnabled
     * @param frameworkName
     * @param filterOnGroup
     * @param filterOnOwner
     * @param filterOnLocation
     * @param filterOnKeyword
     */
    public static void updateFrameworkFunctionality(Boolean startEnabled, Boolean stopEnabled, Boolean pauseEnabled, Boolean resumeEnabled, Boolean abortEnabled, Boolean restartEnabled, Boolean uploadResultEnabled, String frameworkName, String filterOnGroup, String filterOnOwner, String filterOnLocation, String filterOnKeyword) {
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
            }
        }
        UUID stateuuid = UUID.randomUUID();
        String stateuuidAsString = stateuuid.toString();
        String start = "0";
        String stop = "0";
        String pause = "0";
        String resume = "0";
        String abort = "0";
        String restart = "0";
        String uploadResult = "0";
        if (startEnabled)
        {
            start = "1";
        }
        if (stopEnabled)
        {
            stop = "1";
        }
        if (pauseEnabled)
        {
            pause = "1";
        }
        if (resumeEnabled)
        {
            resume = "1";
        }
        if (abortEnabled)
        {
            abort = "1";
        }
        if (restartEnabled)
        {
            restart = "1";
        }
        if (uploadResultEnabled)
        {
            uploadResult = "1";
        }
        String startClientService2 = "insert into client_functionality (uuid, reference_uuid, start_supported, stop_supported, pause_supported, resume_supported, abort_supported, restart_supported, upload_supported, event_time_dt) values('" + stateuuidAsString + "', '" + uuidAsString + "', " + start + ", " + stop + ", " + pause + ", " + resume + ", " + abort + ", " + restart + ", " + uploadResult + ", NOW())";
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
        if (frameworkName.contains("'")) {
            frameworkName.replace("'", "\\'");
        }
        startClientService2 = "update logger set display_name = '" + frameworkName + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
        if (filterOnGroup.contains("'")) {
            filterOnGroup.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_group = '" + filterOnGroup + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
        if (filterOnOwner.contains("'")) {
            filterOnOwner.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_owner = '" + filterOnOwner + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
        if (filterOnLocation.contains("'")) {
            filterOnLocation.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_location = '" + filterOnLocation + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
        if (filterOnKeyword.contains("'")) {
            filterOnKeyword.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_keyword = '" + filterOnKeyword + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
    }

    /**
     * Used to update the CAST Server with the current state of our framework
     * @param state
     */
    public static void updateState(String state) {
        updateState(state, "black");
    }

    /**
     * Used to update the CAST Server with the current state of our framework
     * @param state
     */
    public static void updateState(String state, String color) {
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
            }
        }
        if (state.contains("'")) {
            state.replace("'", "\\'");
        }
        UUID stateuuid = UUID.randomUUID();
        String stateuuidAsString = stateuuid.toString();
        String updateState = "insert into state (uuid, reference_uuid, state, event_time_dt, color) values('" + stateuuidAsString + "', '" + uuidAsString + "', '" + state + "', NOW(), '" + color + "')";
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, updateState.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
    }

    /**
     * Used to update the CAST Server with current results at runtime
     * @param result
     */
    public static void updateResult(String result) {
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
            }
        }
        UUID stateuuid = UUID.randomUUID();
        String stateuuidAsString = stateuuid.toString();
        String updateResult = "insert into results(uuid, reference_uuid, result, event_time_dt) values('" + stateuuidAsString + "', '" + uuidAsString + "', '" + result + "', NOW())";
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, updateResult.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
    }

    /**
     * Used to uplaod result files to the CAST Server
     * @param pathReference
     * @param workingDirectory
     */

    public static void uploadResultFolder(String pathReference, String workingDirectory) {
        uploadResultFolder(pathReference, workingDirectory, true);
    }

    /**
     * Used to upload result files to the CAST Server
     * @param pathReference
     * @param workingDirectory
     * @param cleanupZip
     */
    public static void uploadResultFolder(String pathReference, String workingDirectory, Boolean cleanupZip) {
        String relativePath = pathReference.substring(0, pathReference.lastIndexOf(System.getProperty("file.separator")));
        if (!pathReference.endsWith(System.getProperty("file.separator")))
        {
            relativePath = pathReference.substring(pathReference.lastIndexOf(System.getProperty("file.separator")) + 1);
            pathReference = pathReference + System.getProperty("file.separator");
        }
        else
        {
            relativePath = relativePath.substring(relativePath.lastIndexOf(System.getProperty("file.separator")) + 1);
        }
        String zipFileName = "current_results.zip";
        String zipFilePath = workingDirectory + zipFileName;
        String message = "Send file " + zipFileName;

        Map<String, Object> headers = new HashMap<>();
        headers.put("pathName", currentUUID);
        headers.put("fileName", zipFileName);
        headers.put("originator", currentUUID);
        headers.put("type", "INFO");
        headers.put("message", message);

        AMQP.BasicProperties properties = new AMQP.BasicProperties.Builder()
                .contentType("text/plain")
                .contentEncoding("UTF-8")
                .deliveryMode(2) // Persistent message
                .priority(5)
                .headers(headers)
                .build();
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try {
            byte[] fileBytes = java.nio.file.Files.readAllBytes(java.nio.file.Paths.get(zipFilePath));
            try (Connection connection = factory.newConnection();
                 Channel channel = connection.createChannel()) {
                channel.basicPublish("", "file_storage_service", properties, fileBytes);
            } catch (Exception e) {

            }
        }
        catch (Exception e) {}
        if (cleanupZip) {
            File zipFile = new File(zipFilePath);
            zipFile.delete();
        }
    }

    /**
     * Used to manually close the Message queue
     */
    public static void closeQueue() {
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.queueDelete(currentUUID);
        }
        catch (Exception e) {

        }
    }

    /**
     * Used to register Custom Actions
     * @param actionName
     * @param actionDescription
     * @param hideBeforeStart
     * @param hideAfterStart
     * @param hideAfterComplete
     */
    public static void registerAction(String actionName, String actionDescription, Boolean hideBeforeStart, Boolean hideAfterStart, Boolean hideAfterComplete) {
        registerAction(actionName, actionDescription, hideBeforeStart, hideAfterStart, hideAfterComplete, "fa fa-check");
    }

    /**
     * Used to register Custom Actions
     * @param actionName
     * @param actionDescription
     * @param hideBeforeStart
     * @param hideAfterStart
     * @param hideAfterComplete
     * @param actionIcon
     */
    public static void registerAction(String actionName, String actionDescription, Boolean hideBeforeStart, Boolean hideAfterStart, Boolean hideAfterComplete, String actionIcon) {
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
            }
        }
        actionName = uuidAsString + "|" + actionName;
        UUID stateuuid = UUID.randomUUID();
        String stateuuidAsString = stateuuid.toString();
        String addCustomAction = "insert into custom_actions (uuid, reference_uuid, name, description, icon, hide_before_start, hide_after_start, hide_after_complete, event_time_dt) values('" + stateuuidAsString + "', '" + currentUUID + "', '" + actionName + "', '" + actionDescription + "', '" + actionIcon + "', " + hideBeforeStart + ", " + hideAfterStart + ", " + hideAfterComplete + ", NOW())";
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(rabbitmq_home);
        factory.setPort(Integer.parseInt(rabbitmq_port));
        factory.setUsername(rabbitmq_username);
        factory.setPassword(rabbitmq_password);
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, addCustomAction.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {

        }
    }
}
