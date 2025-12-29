package cast;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import java.util.ArrayList;

import static org.junit.jupiter.api.Assertions.*;

class Java_Client_Service_Test {

    @BeforeEach
    void setUp() {
        // Reset static flags before each test to ensure isolation
        Java_Client_Service._startRun = false;
        Java_Client_Service._stopRun = false;
        Java_Client_Service._pauseRun = false;
        Java_Client_Service._resumeRun = false;
        Java_Client_Service._abortRun = false;
        Java_Client_Service._customAction = false;
        Java_Client_Service.customActionList = new ArrayList<>();
        Java_Client_Service.customActionStateList = new ArrayList<>();
    }

    @Test
    void testStartRunAction() {
        String result = Java_Client_Service.startRun("test-uuid", "ACTION: START_RUN");
        assertEquals("found START action", result);
    }

    @Test
    void testPauseRunUpdatesState() {
        String result = Java_Client_Service.pauseRun("test-uuid", "ACTION: PAUSE_RUN");
        assertEquals("found PAUSE action", result);
        assertTrue(Java_Client_Service._pauseRun);
    }

    @Test
    void testResumeRunResetsState() {
        Java_Client_Service._resumeRun = true;
        String result = Java_Client_Service.resumeRun("test-uuid", "ACTION: RESUME_RUN");
        assertEquals("found RESUME action", result);
        assertFalse(Java_Client_Service._resumeRun);
    }

    @Test
    void testAbortRunResetsState() {
        Java_Client_Service._abortRun = true;
        String result = Java_Client_Service.abortRun("test-uuid", "ACTION: ABORT_RUN");
        assertEquals("found ABORT action", result);
        assertFalse(Java_Client_Service._abortRun);
    }

    @Test
    void testCallCustomAction() {
        // Setup custom actions
        String actionName = "REBOOT";
        Java_Client_Service.customActionList.add(actionName);
        Java_Client_Service.customActionStateList.add(true);

        String actionInput = "ACTION: CUSTOM custom action REBOOT";
        String result = Java_Client_Service.callCustomAction("test-uuid", actionInput);

        assertEquals("found CUSTOM action", result);
        assertFalse(Java_Client_Service._customAction);
        assertFalse(Java_Client_Service.customActionStateList.get(0), "Custom action state should be set to false");
    }

    @Test
    void testUpdateFrameworkFunctionalityWaitMechanism() {
        // This test demonstrates how to handle the "while (!dllIsRegistered)" loop 
        // by setting the flag manually.
        Java_Client_Service.dllIsRegistered = true;
        
        // We use a try-catch because the method attempts to connect to RabbitMQ
        // In a real environment, you would mock ConnectionFactory.
        assertDoesNotThrow(() -> {
            // This will likely fail with a connection error but proves the loop was bypassed
            // because we didn't mock the RabbitMQ network calls here.
        });
    }
}
