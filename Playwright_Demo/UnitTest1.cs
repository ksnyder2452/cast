using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CAST_Client_Service;

namespace PlaywrightTests;

//Based on https://playwright.dev/dotnet/docs/intro

[TestClass]
public class ExampleTest : PageTest
{
    static string resultFile = "";
    //Use the following folder references (as examples) to be relative to the root of this solution
    //The only likely reasons to change the values are
    //    If you already have a ./WorkingDirectory/ folder
    //    If you already have a ./TestResults/ folder
    //    If your test framework already uses a ./TestResults/ or ./WorkingDirectory/ folder
    //In these cases the easiest solution is to change just the folder names (and nothing else)
    //resultsDir should contain Test Result files (subdirectories are supported), and will be created at runtime (if necessary)
    //workingDir is used by the Test Client Service for temporary file storage, and will be created at runtime (if necessary)
    static string resultsDir = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "TestResults" + Path.DirectorySeparatorChar;
    static string workingDir = @Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "WorkingDirectory" + Path.DirectorySeparatorChar;
    static string testsuiteName = "Playwright Demo";
    static string owner = Environment.UserName;
    static string location = Environment.MachineName;
    static string sampleKeywords = "|playwright|sample|framework|";


    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        //No changes are likely required for this method
        Random rnd = new Random();
        ///Update the framework functionality as required - adjust the boolean parameters as necessary
        CAST_Client_Service.CAST_Client_Service.updateFrameworkFunctionality(true, true, true, true, true, false, true, "Playwright_Demo_" + rnd.Next(), testsuiteName, owner, location, sampleKeywords);
        ///Set the initial state of the Test Client Service
        Task<string> updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ONLINE");
        updatedState.Wait();
        ///Register any custom actions required for this Test Client Service
        await CAST_Client_Service.CAST_Client_Service.registerAction("MFA", "Authenticate with multi-factor authentication", false, true, true, "fa fa-id-card");
        await CAST_Client_Service.CAST_Client_Service.registerAction("Email", "Send email notification that the test completed", true, true, false, "fa fa-envelope");
        await CAST_Client_Service.CAST_Client_Service.registerAction("Snapshot", "Take screenshot of environment", false, false, false, "fa fa-camera");

        if (!System.IO.Directory.Exists(resultsDir))
        {
            System.IO.Directory.CreateDirectory(resultsDir);
        }
        if (!System.IO.Directory.Exists(workingDir))
        {
            System.IO.Directory.CreateDirectory(workingDir);
        }
        else
        {
            System.IO.Directory.Delete(workingDir, true);
            System.IO.Directory.CreateDirectory(workingDir);
        }
        resultFile = "current_results.csv";
        if (System.IO.File.Exists(resultsDir + resultFile))
        {
            System.IO.File.Delete(resultsDir + resultFile);
        }
        updatedState = CAST_Client_Service.CAST_Client_Service.updateState("READY (" + CAST_Client_Service.CAST_Client_Service.startmyuuidAsString + ")", "green");
        updatedState.Wait();
        CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Started test suite " + testsuiteName);
    }

    /// <summary>
    /// Class Cleanup - upload test results and close the Test Client Service queue
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        Task<string> updatedState = null;
        //No changes are likely required for this method
        if (!System.IO.File.Exists(resultsDir + resultFile))
        {
            throw new FileNotFoundException("The specified file was not found.", resultsDir + resultFile);
        }
        CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Upload current_result.csv to File Storage Service");
        //Define the test results folder and the working directory folder for the Test Client Service
        CAST_Client_Service.CAST_Client_Service.uploadOutputFolder(resultsDir, workingDir);
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("TESTSUITE " + testsuiteName + " was STOPPED", "orange");
            updatedState.Wait();
        }
        if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("TESTSUITE " + testsuiteName + " was ABORTED", "red");
            updatedState.Wait();
        }
        else
        {
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TESTSUITE " + testsuiteName, "blue");
            updatedState.Wait();
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Completed test suite " + testsuiteName);
        }
        CAST_Client_Service.CAST_Client_Service.closeQueue();
    }

    /// <summary>
    /// Test Cleanup - log test results to CSV file
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        //No changes are likely required for this method
        if (TestContext.TestException == null)
        {
            System.IO.File.AppendAllText(resultsDir + resultFile, TestContext.TestName + " , " + TestContext.CurrentTestOutcome.ToString() + "," + System.Environment.NewLine);
        }
        else
        {
            ///On failure, capture a screenshot and log the exception message
            grabFailureScreenshot(TestContext.TestName);
            string testException = TestContext.TestException.GetBaseException().Message.Replace(",", " ");
            testException = testException.Replace(System.Environment.NewLine, "");
            System.IO.File.AppendAllText(resultsDir + resultFile, TestContext.TestName + ", " + TestContext.CurrentTestOutcome.ToString() + ", " + testException + System.Environment.NewLine);
        }
    }


    /// <summary>
    /// Sample Test Method - verifies that the title contains "Playwright"
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlaywrightException"></exception>
    [TestMethod]
    public async Task HasTitle()
    {
        Task<string> updatedState = null;
        ///Get the current test name
        string? testName = TestContext.TestName;
        ///Check the CAST backend services for Stop or Abort requests
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            ///Update the test results table with a STOPPED status
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            ///Update the test results table with an ABORTED status
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
            Assert.Fail();
        }
        else
        {
            ///Take a screenshot before the test action
            grabScreenshot(testName, "before");
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            try
            {
                await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));
                ///Update the test results table with a PASSED status
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found Title Playwright within " + testName + ". Passed");
                grabScreenshot(testName, "after");
            }
            catch (PlaywrightException ex)
            {
                grabFailureScreenshot(testName);
                ///Update the test results table with a FAILED status
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Title Playwright within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                ///Update the Test Execution Controller UI to indicate that the test has completed
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()", "green");
                updatedState.Wait();
            }
        }
    }

    /// <summary>
    /// Sample Test Method - verifies that the Get Started link works
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlaywrightException"></exception>
    [TestMethod]
    public async Task GetStartedLink()
    {
        Task<string> updatedState = null;
        string? testName = TestContext.TestName;
        ///Check the CAST backend services for Stop or Abort requests
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
            Assert.Fail();
        }
        else
        {
            ///Take a screenshot before the test action
            grabScreenshot(testName, "before");
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found Installation heading within " + testName + ". Passed");
                ///Take a screenshot after the test action
                grabScreenshot(testName, "after");
            }
            catch (PlaywrightException ex)
            {
                ///Take a screenshot on failure
                grabFailureScreenshot(testName);
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()", "green");
                updatedState.Wait();
            }
        }
    }

    /// <summary>
    /// Sample Test Method - demonstrates a test failure by looking for an incorrect title
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlaywrightException"></exception>
    [TestMethod]
    public async Task WrongTitle()
    {
        //Demonstrates a test failure
        Task<string> updatedState = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
            Assert.Fail();
        }
        else
        {
            grabScreenshot(testName, "before");
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            try
            {
                await Expect(Page).ToHaveTitleAsync(new Regex("WRONGTITLE"));
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found title WRONGTITLE within " + testName + ". Failed");
                grabScreenshot(testName, "after");
            }
            catch (PlaywrightException ex)
            {
                grabFailureScreenshot(testName);
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find title WRONGTITLE within " + testName + ". Passed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()", "green");
                updatedState.Wait();
            }
        }
    }

    /// <summary>
    /// Sample Test Method - demonstrates a Stop action during a test run
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlaywrightException"></exception>
    [TestMethod]
    public async Task StopExample()
    {
        //Demonstrates a Stop action during a test run
        Task<string> updatedState = null;
        string? testName = TestContext.TestName;
        ///Hardcoded sleep just to provide opportunity to click on Stop within the Test Execution Controller UI (for demo purposes)
        ///Not for Production
        Thread.Sleep(20000);
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
            ///Hardcoded sleep just to provide opportunity to verify that the Test Execution Controller UI has been updated to diplay Stopped (for demo purposes)
            ///Not for Production
            Thread.Sleep(20000);
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
            Assert.Fail();
        }
        else
        {
            grabScreenshot(testName, "before");
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found Installation heading within " + testName + ". Passed");
                grabScreenshot(testName, "after");
            }
            catch (PlaywrightException ex)
            {
                grabFailureScreenshot(testName);
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()", "green");
                updatedState.Wait();
            }
        }
    }

    /// <summary>
    /// Sample Test Method - demonstrates an Abort action during a test run
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlaywrightException"></exception>
    [TestMethod]
    public async Task AbortExample()
    {
        //Demonstrates an Abort action during a test run
        Task<string> updatedState = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
            Assert.Fail();
        }
        else
        {
            grabScreenshot(testName, "before");
            CheckTestState(testName);
            ///Hardcoded sleep just to provide opportunity to click on Abort within the Test Execution Controller UI (for demo purposes)
            ///Not recommended for Production
            Thread.Sleep(20000);
            await Page.GotoAsync("https://playwright.dev");
            if (CAST_Client_Service.CAST_Client_Service._abortRun)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
                updatedState.Wait();
                ///Hardcoded sleep just to provide opportunity to verify that the Test Execution Controller UI has been updated to diplay Aborted (for demo purposes)
                ///Not recommended for Production
                Thread.Sleep(20000);
                Assert.Fail();
            }
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found heading Installation within " + testName + ". Passed");
                grabScreenshot(testName, "after");
            }
            catch (PlaywrightException ex)
            {
                grabFailureScreenshot(testName);
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()", "green");
                updatedState.Wait();
            }
        }
    }

    /// <summary>
    /// Sample Test Method - demonstrates Pause and Resume actions during a test run
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlaywrightException"></exception>
    [TestMethod]
    public async Task PauseAndResumeExample()
    {
        //Demonstrates Pause/Resume actions during a test run
        Task<string> updatedState = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
            Assert.Fail();
        }
        else
        {
            grabScreenshot(testName, "before");
            CheckTestState(testName);
            ///Hardcoded sleep just to provide opportunity to click on Pause within the Test Execution Controller UI (for demo purposes)
            ///Not recommended for Production
            Thread.Sleep(20000);
            await Page.GotoAsync("https://playwright.dev");
            PauseTest(testName);
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found heading Installation within " + testName + ". Passed");
                grabScreenshot(testName, "after");
            }
            catch (PlaywrightException ex)
            {
                grabFailureScreenshot(testName);
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedState = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()", "green");
                updatedState.Wait();
            }
        }
    }


    /// <summary>
    /// Sample Test Method - checks for Stop, Abort, Start actions before proceeding with the test
    /// </summary>
    /// <param name="testMethodName"></param>
    public async void CheckTestState(string? testMethodName)
    {
        //Wait until Action Start Run has been received to begin testing
        Task<string> updatedState = null;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testMethodName + " has been skipped");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED", "orange");
            updatedState.Wait();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED", "red");
            updatedState.Wait();
        }
        else
        {
            while (!CAST_Client_Service.CAST_Client_Service._startRun)
            {
                Thread.Sleep(5000);
            }
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("RUNNING TEST " + testMethodName + "()", "green");
            updatedState.Wait();
            Thread.Sleep(20000);
        }
    }

    /// <summary>
    /// Sample Test Method - checks for Pause and Resume actions during the test run
    /// </summary>
    /// <param name="testMethodName"></param>
    public async void PauseTest(string? testMethodName)
    {
        //If Pause Action is received then go to Sleep until Resume Action is received
        Task<string> updatedState = null;
        if (CAST_Client_Service.CAST_Client_Service._pauseRun)
        {
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("PAUSED", "orange");
            updatedState.Wait();
            //The Test Execution Controller UI has been updated to diplay PAUSED
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test run for " + testMethodName + " has been paused");
            while (!CAST_Client_Service.CAST_Client_Service._resumeRun)
            {
                //Check every 5 seconds to see if Resume has been clicked
                Thread.Sleep(5000);
            }
            updatedState = CAST_Client_Service.CAST_Client_Service.updateState("RESUMED", "green");
            updatedState.Wait();
            //Hardcoded sleep just to provide opportunity to verify that RESUMED is displayed within the Test Execution Controller UI (for demo purposes)
            //Not recommended for Production
            Thread.Sleep(20000);
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test run for " + testMethodName + " has resumed");
        }
    }

    /// <summary>
    /// Sample Test Method - captures screenshots based on custom action settings
    /// </summary>
    /// <param name="testMethodName"></param>
    /// <param name="state"></param>
    public async void grabScreenshot(string? testMethodName, string state)
    {
        if (!System.IO.Directory.Exists(resultsDir))
        {
            System.IO.Directory.CreateDirectory(resultsDir);
        }
        if (System.IO.File.Exists(resultsDir + Path.DirectorySeparatorChar + testMethodName + "_" + state + "_screenshot.png"))
        {
            System.IO.File.Delete(resultsDir + Path.DirectorySeparatorChar + testMethodName + "_" + state + "_screenshot.png");
        }
        for (int counter = 0; counter < CAST_Client_Service.CAST_Client_Service.customActionList.Count; counter++)
        {
            if (CAST_Client_Service.CAST_Client_Service.customActionList[counter].EndsWith("Snapshot") && CAST_Client_Service.CAST_Client_Service.customActionStateList[counter])
            {
                Task<string> updatedState = CAST_Client_Service.CAST_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT " + state + " " + testMethodName);
                updatedState.Wait();
                await Page.ScreenshotAsync(new() { Path = resultsDir + Path.DirectorySeparatorChar + testMethodName + "_" + state + "_screenshot.png" });
                CAST_Client_Service.CAST_Client_Service.customActionStateList[counter] = false;
            }
        }
    }
    /// <summary>
    /// Sample Test Method - captures a screenshot on test failure
    /// </summary>
    /// <param name="testMethodName"></param>
    public void grabFailureScreenshot(string? testMethodName)
    {
        if (!System.IO.Directory.Exists(resultsDir))
        {
            System.IO.Directory.CreateDirectory(resultsDir);
        }
        if (System.IO.File.Exists(resultsDir + Path.DirectorySeparatorChar + testMethodName + "_failure_screenshot.png"))
        {
            System.IO.File.Delete(resultsDir + Path.DirectorySeparatorChar + testMethodName + "_failure_screenshot.png");
        }
        Page.ScreenshotAsync(new() { Path = resultsDir + Path.DirectorySeparatorChar + testMethodName + "_failure_screenshot.png" });
    }

}
