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
        CAST_Client_Service.CAST_Client_Service.updateFrameworkFunctionality(true, true, true, true, true, false, true, "Playwright_Demo_" + rnd.Next(), testsuiteName, owner, location, sampleKeywords);
        Task<string> updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ONLINE");
        updatedResult.Wait();
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
        updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("READY");
        updatedResult.Wait();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Task<string> updatedResult = null;
        //No changes are likely required for this method
        if (!System.IO.File.Exists(resultsDir + resultFile))
        {
            throw new FileNotFoundException("The specified file was not found.", resultsDir + resultFile);
        }
        CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Upload current_result.csv to File Storage Service");
        //Define the test results folder and the working directory folder for the Test Client Service
        CAST_Client_Service.CAST_Client_Service.uploadOutputFolder(resultsDir, workingDir);

        if (CAST_Client_Service.CAST_Client_Service._customAction)
        {
            foreach (String currentCurrentAction in CAST_Client_Service.CAST_Client_Service.customActionList)
            {
                if (currentCurrentAction.EndsWith("Snapshot"))
                {
                    updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("TAKE SNAPSHOT OF TEST ENVIRONMENT");
                    updatedResult.Wait();
                }
            }
        }
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("TESTSUITE " + testsuiteName + " was STOPPED");
            updatedResult.Wait();
        }
        if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("TESTSUITE " + testsuiteName + " was ABORTED");
            updatedResult.Wait();
        }
        else
        {
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TESTSUITE " + testsuiteName, true);
            updatedResult.Wait();
        }
        CAST_Client_Service.CAST_Client_Service.closeQueue();
    }

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
            string testException = TestContext.TestException.GetBaseException().Message.Replace(",", " ");
            testException = testException.Replace(System.Environment.NewLine, "");
            System.IO.File.AppendAllText(resultsDir + resultFile, TestContext.TestName + ", " + TestContext.CurrentTestOutcome.ToString() + ", " + testException + System.Environment.NewLine);
        }
    }


    [TestMethod]
    public async Task HasTitle()
    {
        Task<string> updatedResult = null;
        string? testName = TestContext.TestName;
        //If Action Stop Run is received then skip this test
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            //Update the test_results table
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
            Assert.Fail();
        }
        else
        {
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            try
            {
                await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found Title Playwright within " + testName + ". Passed");
            }
            catch (PlaywrightException ex)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Title Playwright within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()");
                updatedResult.Wait();
            }
        }
    }

    [TestMethod]
    public async Task GetStartedLink()
    {
        Task<string> updatedResult = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
            Assert.Fail();
        }
        else
        {
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found Installation heading within " + testName + ". Passed");
            }
            catch (PlaywrightException ex)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()");
                updatedResult.Wait();
            }
        }
    }

    [TestMethod]
    public async Task WrongTitle()
    {
        //Demonstrates a test failure
        Task<string> updatedResult = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
            Assert.Fail();
        }
        else
        {
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            try
            {
                await Expect(Page).ToHaveTitleAsync(new Regex("WRONGTITLE"));
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found title WRONGTITLE within " + testName + ". Failed");
            }
            catch (PlaywrightException ex)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find title WRONGTITLE within " + testName + ". Passed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()");
                updatedResult.Wait();
            }
        }
    }

    [TestMethod]
    public async Task StopExample()
    {
        //Demonstrates a Stop action during a test run
        Task<string> updatedResult = null;
        string? testName = TestContext.TestName;
        //Hardcoded sleep just to provide opportunity to click on Stop within the Test Execution Controller UI (for demo purposes)
        //Not recommended for Production
        Thread.Sleep(20000);
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
            //Hardcoded sleep just to provide opportunity to verify that the Test Execution Controller UI has been updated to diplay Stopped (for demo purposes)
            //Not recommended for Production
            Thread.Sleep(20000);
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
            Assert.Fail();
        }
        else
        {
            CheckTestState(testName);
            await Page.GotoAsync("https://playwright.dev");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found Installation heading within " + testName + ". Passed");
            }
            catch (PlaywrightException ex)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()");
                updatedResult.Wait();
            }
        }
    }

    [TestMethod]
    public async Task AbortExample()
    {
        //Demonstrates an Abort action during a test run
        Task<string> updatedResult = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
            Assert.Fail();
        }
        else
        {
            CheckTestState(testName);
            //Hardcoded sleep just to provide opportunity to click on Abort within the Test Execution Controller UI (for demo purposes)
            //Not recommended for Production
            Thread.Sleep(20000);
            await Page.GotoAsync("https://playwright.dev");
            if (CAST_Client_Service.CAST_Client_Service._abortRun)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
                updatedResult.Wait();
                //Hardcoded sleep just to provide opportunity to verify that the Test Execution Controller UI has been updated to diplay Aborted (for demo purposes)
                //Not recommended for Production
                Thread.Sleep(20000);
                Assert.Fail();
            }
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found heading Installation within " + testName + ". Passed");
            }
            catch (PlaywrightException ex)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()");
                updatedResult.Wait();
            }
        }
    }

    [TestMethod]
    public async Task PauseAndResumeExample()
    {
        //Demonstrates Pause/Resume actions during a test run
        Task<string> updatedResult = null;
        string? testName = TestContext.TestName;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
            Assert.Inconclusive();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
            Assert.Fail();
        }
        else
        {
            CheckTestState(testName);
            //Hardcoded sleep just to provide opportunity to click on Pause within the Test Execution Controller UI (for demo purposes)
            //Not recommended for Production
            Thread.Sleep(20000);
            await Page.GotoAsync("https://playwright.dev");
            PauseTest(testName);
            await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
            try
            {
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Found heading Installation within " + testName + ". Passed");
            }
            catch (PlaywrightException ex)
            {
                CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Failed to find Installation heading within " + testName + ". Failed");
                throw new PlaywrightException(ex.Message);
            }
            finally
            {
                updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("COMPLETED TEST " + testName + "()");
                updatedResult.Wait();
            }
        }
    }


    public async void CheckTestState(string? testMethodName)
    {
        //Wait until Action Start Run has been received to begin testing
        Task<string> updatedResult = null;
        if (CAST_Client_Service.CAST_Client_Service._stopRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test " + testMethodName + " has been skipped");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("STOPPED");
            updatedResult.Wait();
        }
        else if (CAST_Client_Service.CAST_Client_Service._abortRun)
        {
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Testsuite " + testsuiteName + " has been aborted");
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("ABORTED");
            updatedResult.Wait();
        }
        else
        {
            while (!CAST_Client_Service.CAST_Client_Service._startRun)
            {
                Thread.Sleep(5000);
            }
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("RUNNING TEST " + testMethodName + "()");
            updatedResult.Wait();
            Thread.Sleep(20000);
        }
    }

    public async void PauseTest(string? testMethodName)
    {
        //If Pause Action is received then go to Sleep until Resume Action is received
        Task<string> updatedResult = null;
        if (CAST_Client_Service.CAST_Client_Service._pauseRun)
        {
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("PAUSED");
            updatedResult.Wait();
            //The Test Execution Controller UI has been updated to diplay PAUSED
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test run for " + testMethodName + " has been paused");
            while (!CAST_Client_Service.CAST_Client_Service._resumeRun)
            {
                //Check every 5 seconds to see if Resume has been clicked
                Thread.Sleep(5000);
            }
            updatedResult = CAST_Client_Service.CAST_Client_Service.updateState("RESUMED");
            updatedResult.Wait();
            //Hardcoded sleep just to provide opportunity to verify that RESUMED is displayed within the Test Execution Controller UI (for demo purposes)
            //Not recommended for Production
            Thread.Sleep(20000);
            CAST_Client_Service.CAST_Client_Service.updateResult("Playwright: Test run for " + testMethodName + " has resumed");
        }
    }
}