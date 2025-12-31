import com.microsoft.playwright.Browser;
import com.microsoft.playwright.BrowserContext;
import com.microsoft.playwright.Page;
import com.microsoft.playwright.Playwright;
import org.junit.jupiter.api.*;

import java.io.File;
import java.io.IOException;
import java.net.InetAddress;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import static com.microsoft.playwright.assertions.PlaywrightAssertions.assertThat;
import static org.junit.jupiter.api.Assertions.*;

import java.util.Comparator;
import java.util.Random;
import java.util.regex.Pattern;
import main.java.cast.Java_Client_Service;

//Based on https://playwright.dev/java/docs/running-tests
public class CAST_Demo {
    static String rootDir = System.getProperty("user.dir");
    static String resultDirPath = rootDir + System.getProperty("file.separator") + "target" + System.getProperty("file.separator") + "test_results" + System.getProperty("file.separator");
    static String workingDirPath = rootDir + System.getProperty("file.separator") + "target" + System.getProperty("file.separator") + "working_directory" + System.getProperty("file.separator");
    static String resultFile = "current_results.txt";

    static String testsuiteName = "Playwright Java Demo";
    static String owner = System.getProperty("user.name");
    static String location = "";

    // Shared between all tests in this class.
    static Playwright playwright;
    static Browser browser;

    // New instance for each test method.
    BrowserContext context;
    Page page;
    static String resultFilePath = resultDirPath + resultFile;

    @BeforeAll
    static void classSetup() {
        playwright = Playwright.create();
        browser = playwright.chromium().launch();
        try {
            Java_Client_Service.startService();
            Random rnd = new Random();
            try {
                location = InetAddress.getLocalHost().getHostName();
            }
            catch (java.net.UnknownHostException uHE) {}
            Java_Client_Service.updateFrameworkFunctionality(true, true, true, true, true, false, true, "Playwright_Java_Demo_" + rnd.nextInt(1000000), testsuiteName, owner, location, "Not applicable");
            Java_Client_Service.updateState("ONLINE", "black");
            Java_Client_Service.registerAction("Snapshot", "Take screenshot of environment", false, false, false, "fa fa-camera");


            try {
                File resultDir = new File(resultDirPath);
                resultDir.mkdir();
                Path resultPath = Paths.get(resultDirPath);
                Files.walk(resultPath)
                        .sorted(Comparator.reverseOrder()) // Sort in reverse order to delete children before parents
                        .forEach(path -> {
                            try {
                                Files.delete(path);
                                System.out.println("Deleted: " + path);
                            } catch (IOException e) {
                                System.err.println("Failed to delete " + path + ": " + e.getMessage());
                                // Handle the exception as needed, e.g., rethrow, log, etc.
                            }
                        });
                resultDir.delete();
                resultDir.mkdir();


                File workingDir = new File(workingDirPath);
                workingDir.mkdir();
                Path workingPath = Paths.get(workingDirPath);
                Files.walk(workingPath)
                        .sorted(Comparator.reverseOrder()) // Sort in reverse order to delete children before parents
                        .forEach(path -> {
                            try {
                                Files.delete(path);
                                System.out.println("Deleted: " + path);
                            } catch (IOException e) {
                                System.err.println("Failed to delete " + path + ": " + e.getMessage());
                                // Handle the exception as needed, e.g., rethrow, log, etc.
                            }
                        });
                workingDir.delete();
                workingDir.mkdir();
            }
            catch (IOException ioE) {
                System.err.println(ioE.getMessage());
            }
            Java_Client_Service.updateState("READY (" + Java_Client_Service.uuidAsString + ")", "#22c55e;");
            String message = Java_Client_Service._message;
            while (!Java_Client_Service._startRun) {
                try {
                    Thread.sleep(5000);
                }
                catch (InterruptedException iE) {}
            }
        }
        catch (Exception e) {
            System.err.println(e.getMessage());
        }
        try {
            File newResultFile = new File(resultDirPath + resultFile);
            if (newResultFile.exists()) {
                newResultFile.delete();
            }
            newResultFile.createNewFile();
        }
        catch (IOException ioE) {
            ioE.printStackTrace();
        }
        Java_Client_Service.updateResult("Java Playwright: Started test suite " + testsuiteName);
    }

    @AfterAll
    static void classTeardown() throws IOException {
        playwright.close();
        Java_Client_Service.updateResult("Java Playwright: Upload current_result.txt to File Storage Service");
        Java_Client_Service.uploadResultFolder(resultDirPath, workingDirPath, true);
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("TESTSUITE " + testsuiteName + " was STOPPED", "orange");
        }
        if (Java_Client_Service._abortRun) {
            Java_Client_Service.updateState("TESTSUITE " + testsuiteName + " was ABORTED", "red");
        }
        else {
            Java_Client_Service.updateState("COMPLETED TESTSUITE " + testsuiteName, "blue");
        }
        Java_Client_Service.closeQueue();
    }

    @BeforeEach
    void createContextAndPage() {
        context = browser.newContext();
        page = context.newPage();
    }

    @BeforeEach
    void setUp() {
    }


    @AfterEach
    void teardown() {
        context.close();
    }

    @Test
    @DisplayName("Navigate to page")
    void checkPlaywrightWeb(TestInfo testInfo) {
        String testDesc = "Navigate to page";
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            page.navigate("https://playwright.dev");

            for (int counter = 0; counter < Java_Client_Service.customActionList.size(); counter++)
            {
                if (Java_Client_Service.customActionList.get(counter).endsWith("Snapshot") && Java_Client_Service.customActionStateList.get(counter))
                {
                    Java_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT " + testDesc, "black");
                    page.screenshot(new Page.ScreenshotOptions().setPath(Paths.get(testDesc + ".png")));
                    Java_Client_Service.customActionStateList.set(counter, false);
                }
            }

            // Expect a title "to contain" a substring.
            checkTestState(testDesc);
            try {
                assertThat(page).hasTitle(Pattern.compile("Playwright"));
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Title Playwright was found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Title Playwright was not found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    @Test
    @DisplayName("Button should be clicked")
    void shouldClickButton(TestInfo testInfo) {
        String testDesc = "Should click button";
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            checkTestState(testDesc);
            page.navigate("data:text/html,<script>var result;</script><button onclick='result=\"Clicked\"'>Go</button>");
            page.locator("button").click();

            for (int counter = 0; counter < Java_Client_Service.customActionList.size(); counter++)
            {
                if (Java_Client_Service.customActionList.get(counter).endsWith("Snapshot") && Java_Client_Service.customActionStateList.get(counter))
                {
                    Java_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT " + testDesc, "black");
                    page.screenshot(new Page.ScreenshotOptions().setPath(Paths.get(testDesc + ".png")));
                    Java_Client_Service.customActionStateList.set(counter, false);
                }
            }
            //assertEquals("Clicked", page.evaluate("result"));
            try {
                assertEquals("Clicked", page.evaluate("result"));
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Button was clicked" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Button was not clicked" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    @Test
    @DisplayName("Box should be checked")
    void shouldCheckTheBox(TestInfo testInfo) {
        String testDesc = "Should check box";
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            checkTestState(testDesc);
            page.setContent("<input id='checkbox' type='checkbox'></input>");
            page.locator("input").check();

            for (int counter = 0; counter < Java_Client_Service.customActionList.size(); counter++)
            {
                if (Java_Client_Service.customActionList.get(counter).endsWith("Snapshot") && Java_Client_Service.customActionStateList.get(counter))
                {
                    Java_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT " + testDesc, "black");
                    page.screenshot(new Page.ScreenshotOptions().setPath(Paths.get(testDesc + ".png")));
                    Java_Client_Service.customActionStateList.set(counter, false);
                }
            }
            //assertTrue((Boolean) page.evaluate("() => window['checkbox'].checked"));
            try {
                assertTrue((Boolean) page.evaluate("() => window['checkbox'].checked"));
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Box was checked" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Box was not checked" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    @Test
    @DisplayName("Wiki should be searched")
    void shouldSearchWiki(TestInfo testInfo) {
        String testDesc = "Should search Wiki";
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            checkTestState(testDesc);
            page.navigate("https://www.wikipedia.org/");
            page.locator("input[name=\"search\"]").click();
            page.locator("input[name=\"search\"]").fill("playwright");
            page.locator("input[name=\"search\"]").press("Enter");

            for (int counter = 0; counter < Java_Client_Service.customActionList.size(); counter++)
            {
                if (Java_Client_Service.customActionList.get(counter).endsWith("Snapshot") && Java_Client_Service.customActionStateList.get(counter))
                {
                    Java_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT " + testDesc, "black");
                    page.screenshot(new Page.ScreenshotOptions().setPath(Paths.get(testDesc + ".png")));
                    Java_Client_Service.customActionStateList.set(counter, false);
                }
            }
            //assertEquals("https://en.wikipedia.org/wiki/Playwright", page.url());
            try {
                assertEquals("https://en.wikipedia.org/wiki/Playwright", page.url());
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Wiki was opened" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Wiki did not open" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    @Test
    @DisplayName("Verify invalid page check")
    void invalidPage(TestInfo testInfo) {
        String testDesc = "Invalid page check";
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            page.navigate("https://playwright.dev");

            // Expect a title "to contain" a substring.
            checkTestState(testDesc);

            for (int counter = 0; counter < Java_Client_Service.customActionList.size(); counter++)
            {
                if (Java_Client_Service.customActionList.get(counter).endsWith("Snapshot") && Java_Client_Service.customActionStateList.get(counter))
                {
                    Java_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT " + testDesc, "black");
                    page.screenshot(new Page.ScreenshotOptions().setPath(Paths.get(testDesc + ".png")));
                    Java_Client_Service.customActionStateList.set(counter, false);
                }
            }
            //assertThat(page).hasTitle(Pattern.compile("WRONG PAGE"));
            try {
                assertThat(page).hasTitle(Pattern.compile("WRONG PAGE"));
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Page was found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Page was not found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    @org.junit.jupiter.api.Test
    @org.junit.jupiter.api.DisplayName("Test Abort action")
    void abortTest(TestInfo testInfo) {
        try {
            Thread.sleep(20000);
        }
        catch (InterruptedException iE) {}

        String testDesc = "Verify abort";
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            page.navigate("https://playwright.dev");

            // Expect a title "to contain" a substring.
            checkTestState(testDesc);
            //assertThat(page).hasTitle(Pattern.compile("Playwright"));
            try {
                assertThat(page).hasTitle(Pattern.compile("Playwright"));
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Playwright was found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Playwright was not found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    @org.junit.jupiter.api.Test
    @org.junit.jupiter.api.DisplayName("Test Pause/Resume actions")
    void pauseResumeTest(TestInfo testInfo) {
        String testDesc = "Verify pause";
        try {
            Thread.sleep(20000);
        }
        catch (InterruptedException iE) {}
        if (Java_Client_Service._pauseRun) {
            while (!Java_Client_Service._resumeRun) {
                try {
                    Thread.sleep(5000);
                }
                catch (InterruptedException iE) {}
            }
        }

        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateState("STOPPED", "orange");
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test '" + testDesc + "' has been skipped");
            fail();
        }
        else if (Java_Client_Service._abortRun) {
            Assumptions.abort();
        }
        else {
            page.navigate("https://playwright.dev");

            // Expect a title "to contain" a substring.
            checkTestState(testDesc);
            //assertThat(page).hasTitle(Pattern.compile("Playwright"));
            try {
                assertThat(page).hasTitle(Pattern.compile("Playwright"));
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Playwright was found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE2) {
                    ioE2.printStackTrace();
                }
            }
            catch (AssertionError e) {
                try {
                    Files.writeString(Paths.get(resultFilePath), testInfo.getDisplayName() + ": Playwright was not found" + System.lineSeparator(), java.nio.file.StandardOpenOption.APPEND);
                }
                catch (IOException ioE) {
                    ioE.printStackTrace();
                }
                throw e;
            }
        }
    }

    static void checkTestState(String testMethodName) {
        if (Java_Client_Service._stopRun) {
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test " + testMethodName + " has been skipped");
            Java_Client_Service.updateState("STOPPED", "orange");
        } else if (Java_Client_Service._abortRun) {
            Java_Client_Service.updateResult("JAVA PLAYWRIGHT: Test " + testMethodName + " has been aborted");
            Java_Client_Service.updateState("ABORTED", "red");
        } else {
            while (!Java_Client_Service._startRun) {
                try {
                    Thread.sleep(5000);
                } catch (InterruptedException iE) {
                }
            }
            Java_Client_Service.updateState("RUNNING TEST " + testMethodName, "#22c55e;");
            try {
                Thread.sleep(20000);
            } catch (InterruptedException iE) {
            }
        }
    }

}
