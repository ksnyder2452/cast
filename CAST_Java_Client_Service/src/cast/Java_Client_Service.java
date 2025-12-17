package cast;

import com.rabbitmq.client.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.nio.file.FileSystems;
import java.util.Map;
import java.util.Properties;
import java.io.BufferedWriter;
import java.io.FileWriter;
import java.nio.charset.StandardCharsets;
import java.util.UUID;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.zip.*;
import java.io.FileOutputStream;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;


import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;

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
    private static final Logger log = LoggerFactory.getLogger(Java_Client_Service.class);
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
    static String downloadQueueDir = rootDir + "download_queue" + FileSystems.getDefault().getSeparator();
    static String uploadQueueDir = rootDir + "upload_queue" + FileSystems.getDefault().getSeparator();
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
    static String tempLog = "." + FileSystems.getDefault().getSeparator() + "temp.log";
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

    static Boolean debugOn = false;
    static Path filePathForDebug = Paths.get("output.txt");


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
     */
    public static void startService() throws Exception {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In startService()" + System.lineSeparator());
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

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
            System.out.println(e.getMessage());
        } catch (NumberFormatException e) {
            System.err.println("Error parsing port number: " + e.getMessage());
            System.out.println(e.getMessage());
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
                String message = new String(delivery.getBody(), StandardCharsets.UTF_8);
                System.out.println(" [x] Received '" + message + "'");
                _message = message;
                channel.basicAck(delivery.getEnvelope().getDeliveryTag(), false); // Acknowledge message
                if (message.toUpperCase().endsWith("PUSH FILE: ")) {
                    Map<String, Object> fileReference = delivery.getProperties().getHeaders();
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
            };

            CancelCallback cancelCallback = consumerTag -> {
                System.out.println("Consumer " + consumerTag + " was cancelled.");
            };

            channel.basicConsume(currentUUID, false, deliverCallback, cancelCallback); // Set autoAck to false for manual acknowledgements
        }
        catch (Exception e) {
            System.out.println(e.getMessage());
        }
    }

    /**
     * Receive and process a Start Run Action Request from the UTAF Server
     * @param service_uuid Service UUID
     * @param action Set start to true
     * @return String
     */
    public static String startRun(String service_uuid, String action) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In startRun()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: START")) {
            result = "found START action";
        }
        return result;
    }

    /**
     * Receive and process a Pause Run Action Request from the UTAF Server
     * @param service_uuid Service_UUID
     * @param action Set Pause to true
     * @return String
     */
    public static String pauseRun(String service_uuid, String action) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In pauseRun()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: PAUSE")) {
            result = "found PAUSE action";
            _pauseRun = true;
        }
        return result;
    }

    /**
     * Receive and process a Resume Run Action Request from the UTAF Server
     * @param service_uuid Service UUID
     * @param action Set _resumeRun to false
     * @return result
     */
    public static String resumeRun(String service_uuid, String action) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In resumeRun()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: RESUME")) {
            result = "found RESUME action";
            _resumeRun = false;
        }
        return result;
    }

    /**
     * Receive and process an Abort Run Action Request from the UTAF Server
     * @param service_uuid Service UUID
     * @param action Set _abortRun to false
     * @return result
     */
    public static String abortRun(String service_uuid, String action) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In abortRun()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        String result = "";
        if (action.toUpperCase().startsWith("ACTION: ABORT")) {
            result = "found ABORT action";
            _abortRun = false;
        }
        return result;
    }

    /**
     * Receive and process a Custom Action Request from the UTAF Server
     * @param service_uuid Service UUID
     * @param action Set _customAction to false
     * @return result
     */
    public static String callCustomAction(String service_uuid, String action) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In callCustomAction()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
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
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In stopService()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
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
            System.out.println(e.getMessage());
        }
    }
    /**
     * Used to update the CAST Server with our supported Framework functionality as well as the framework display name and filter options
     * filterOnKeyword format is |keyword1|keyword2|...|keyword#|
     * @param startEnabled Set startEnabled for your framework
     * @param stopEnabled Set stopEnabled for your framework
     * @param pauseEnabled Set pauseEnabled for your framework
     * @param resumeEnabled Set resumeEnabled for your framework
     * @param abortEnabled Set abortEnabled for your framework
     * @param restartEnabled Set restartEnabled for your framework
     * @param uploadResultEnabled Set uploadResultEnabled for your framework
     * @param frameworkName Set your framework name
     * @param filterOnGroup Set your filter values
     * @param filterOnOwner Set your filter values
     * @param filterOnLocation Set your filter values
     * @param filterOnKeyword Set your filter values
     */
    public static void updateFrameworkFunctionality(Boolean startEnabled, Boolean stopEnabled, Boolean pauseEnabled, Boolean resumeEnabled, Boolean abortEnabled, Boolean restartEnabled, Boolean uploadResultEnabled, String frameworkName, String filterOnGroup, String filterOnOwner, String filterOnLocation, String filterOnKeyword) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In updateFrameworkFunctionality()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
                System.out.println(e.getMessage());
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
            System.out.println(e.getMessage());
        }
        if (frameworkName.contains("'")) {
            frameworkName = frameworkName.replace("'", "\\'");
        }
        startClientService2 = "update logger set display_name = '" + frameworkName + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {
            System.out.println(e.getMessage());
        }
        if (filterOnGroup.contains("'")) {
            filterOnGroup = filterOnGroup.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_group = '" + filterOnGroup + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {
            System.out.println(e.getMessage());
        }
        if (filterOnOwner.contains("'")) {
            filterOnOwner = filterOnOwner.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_owner = '" + filterOnOwner + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {
            System.out.println(e.getMessage());
        }
        if (filterOnLocation.contains("'")) {
            filterOnLocation = filterOnLocation.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_location = '" + filterOnLocation + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        }
        catch (Exception e) {
            System.out.println(e.getMessage());
        }
        if (filterOnKeyword.contains("'")) {
            filterOnKeyword = filterOnKeyword.replace("'", "\\'");
        }
        startClientService2 = "update logger set filter_on_keyword = '" + filterOnKeyword + "' where reference_uuid = '" + uuidAsString + "'";
        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.basicPublish("", "logger_service", null, startClientService2.getBytes(StandardCharsets.UTF_8));
        } catch (Exception e) {
            System.out.println(e.getMessage());
        }
    }

    /**
     * Used to update the CAST Server with the current state of our framework
     * @param state Record the state of your framework
     */
    public static void updateState(String state, String color) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In updateState()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
                System.out.println(e.getMessage());
            }
        }
        if (state.contains("'")) {
            state = state.replace("'", "\\'");
        }
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "state is " + state + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        UUID stateuuid = UUID.randomUUID();
        String stateuuidAsString = stateuuid.toString();
        String updateState = "insert into state (uuid, reference_uuid, state, event_time_dt, color) values('" + stateuuidAsString + "', '" + uuidAsString + "', '" + state + "', NOW(), '" + color + "')";
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "updateState is " + updateState + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
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
            System.out.println(e.getMessage());
        }
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "Published to Logger Service" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    /**
     * Used to update the CAST Server with current results at runtime
     * @param result Result from your framework
     */
    public static void updateResult(String result) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In updateResult()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
                System.out.println(e.getMessage());
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
            System.out.println(e.getMessage());
        }
    }

    /**
     * Used to upload result files to the CAST Server
     * @param pathReference Path to your folder
     * @param workingDirectory Location of your Zip file
     * @param cleanupZip Whether to delete the Zip after processing
     */
    public static void uploadResultFolder(String pathReference, String workingDirectory, Boolean cleanupZip) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In uploadResultFolder()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        String relativePath = pathReference.substring(0, pathReference.lastIndexOf(FileSystems.getDefault().getSeparator()));
        if (!pathReference.endsWith(FileSystems.getDefault().getSeparator()))
        {
            relativePath = pathReference.substring(pathReference.lastIndexOf(FileSystems.getDefault().getSeparator()) + 1);
            pathReference = pathReference + FileSystems.getDefault().getSeparator();
        }
        else
        {
            relativePath = relativePath.substring(relativePath.lastIndexOf(FileSystems.getDefault().getSeparator()) + 1);
        }
        String zipFileName = "current_results.zip";
        String zipFilePath = workingDirectory + zipFileName;
        String message = "Send file " + zipFileName;

        //Zip folder
        Path folderPath = Paths.get(pathReference);
        if (Files.exists(folderPath) && Files.isDirectory(folderPath)) {
            try (Stream<Path> paths = Files.list(folderPath)) {
                List<String> fileNameList = paths
                        .filter(Files::isRegularFile)
                        .map(Path::getFileName)
                        .map(Path::toString)
                        .collect(Collectors.toList());

                String[] fileNamesArray = fileNameList.toArray(new String[0]);
                createZipFile(zipFilePath,fileNamesArray);
            } catch (IOException e) {
                System.err.println("An I/O error occurred: " + e.getMessage());
            }
        } else {
            System.out.println("Folder not found or is not a directory.");
        }

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
            }
            catch (Exception e) {
                System.out.println(e.getMessage());
            }
        }
        catch (Exception e) {
            System.out.println(e.getMessage());
        }
        if (cleanupZip) {
            File zipFile = new File(zipFilePath);
            boolean deleteState = zipFile.delete();
        }
    }

    /**
     * Used to manually close the Message queue
     */
    public static void closeQueue() {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In closeQueue()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
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
            System.out.println(e.getMessage());
        }
    }

    /**
     * Used to register Custom Actions
     * @param actionName Name of the custom action
     * @param actionDescription Desciption of the custom action
     * @param hideBeforeStart Whether to display the action icon prior to starting your framework
     * @param hideAfterStart Whether to display the action icon after starting your framework
     * @param hideAfterComplete Whether to display the action icon after completing your framewoork
     * @param actionIcon The icon display
     */
    public static void registerAction(String actionName, String actionDescription, Boolean hideBeforeStart, Boolean hideAfterStart, Boolean hideAfterComplete, String actionIcon) {
        if (debugOn) {
            try {
                Files.writeString(filePathForDebug, "In registerAction()" + System.lineSeparator(), StandardOpenOption.APPEND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        while (!dllIsRegistered) {
            try {
                Thread.sleep(1000);
            }
            catch (Exception e) {
                System.out.println(e.getMessage());
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
            System.out.println(e.getMessage());
        }
    }

    public static void createZipFile(String zipFilePath, String[] filesToZip) throws IOException {
        try (FileOutputStream fos = new FileOutputStream(zipFilePath);
             ZipOutputStream zos = new ZipOutputStream(fos)) {

            for (String filePath : filesToZip) {
                File file = new File(filePath);
                try (FileInputStream fis = new FileInputStream(file)) {
                    ZipEntry zipEntry = new ZipEntry(file.getName());
                    zos.putNextEntry(zipEntry);

                    byte[] bytes = new byte[1024];
                    int length;
                    while ((length = fis.read(bytes)) >= 0) {
                        zos.write(bytes, 0, length);
                    }
                    zos.closeEntry();
                }
            }
        }
    }
}
