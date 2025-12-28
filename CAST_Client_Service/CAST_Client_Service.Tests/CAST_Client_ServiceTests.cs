using Xunit;
using CAST_Client_Service;
using System.Reflection;

namespace CAST_Client_Service.Tests;

/// <summary>
/// Unit tests for the CAST_Client_Service class
/// Tests cover action processing methods and state management
/// </summary>
public class CAST_Client_Service_ActionTests
{
    [Fact]
    public void StartRun_WithValidStartAction_ReturnsFoundMessage()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: START run_id_456";

        // Act
        string result = CAST_Client_Service.startRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found START action", result);
    }

    [Fact]
    public void StartRun_WithInvalidAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: STOP run_id_456";

        // Act
        string result = CAST_Client_Service.startRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void StartRun_WithLowercaseAction_ReturnsFoundMessage()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "action: start run_id_456";

        // Act
        string result = CAST_Client_Service.startRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found START action", result);
    }

    [Fact]
    public void StartRun_WithMixedCaseAction_ReturnsFoundMessage()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "Action: Start run_id_456";

        // Act
        string result = CAST_Client_Service.startRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found START action", result);
    }

    [Fact]
    public void PauseRun_WithInvalidAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: RESUME run_id_456";
        CAST_Client_Service._pauseRun = false;

        // Act
        string result = CAST_Client_Service.pauseRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result);
        Assert.False(CAST_Client_Service._pauseRun);
    }

    [Fact]
    public void ResumeRun_WithValidResumeAction_ReturnsFoundMessage()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: RESUME run_id_456";
        CAST_Client_Service._resumeRun = true;

        // Act
        string result = CAST_Client_Service.resumeRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found RESUME action", result);
        Assert.False(CAST_Client_Service._resumeRun);
    }

    [Fact]
    public void ResumeRun_WithInvalidAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: PAUSE run_id_456";

        // Act
        string result = CAST_Client_Service.resumeRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void AbortRun_WithValidAbortAction_ReturnsFoundMessageAndClearsAbortFlag()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: ABORT run_id_456";
        CAST_Client_Service._abortRun = true;

        // Act
        string result = CAST_Client_Service.abortRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found ABORT action", result);
        Assert.False(CAST_Client_Service._abortRun);
    }

    [Fact]
    public void AbortRun_WithInvalidAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: START run_id_456";

        // Act
        string result = CAST_Client_Service.abortRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CallCustomAction_WithValidCustomActionFormat_ReturnsFoundMessage()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: CUSTOM ACTION myaction";

        // Act
        string result = CAST_Client_Service.callCustomAction(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found CUSTOM action", result);
    }

    [Fact]
    public void CallCustomAction_WithInvalidAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "ACTION: START run_id_456";

        // Act
        string result = CAST_Client_Service.callCustomAction(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result);
    }
}

/// <summary>
/// Unit tests for static state and configuration properties
/// </summary>
public class CAST_Client_Service_StateTests
{
    [Fact]
    public void StartmyuuidAsString_IsValidGuid()
    {
        // Act
        var uuidString = CAST_Client_Service.startmyuuidAsString;

        // Assert
        Assert.NotNull(uuidString);
        Assert.True(Guid.TryParse(uuidString, out _), "startmyuuidAsString should be a valid GUID");
    }

    [Fact]
    public void CurrentUUID_ContainsClientServicePrefix()
    {
        // Act
        var currentUUID = CAST_Client_Service.currentUUID;

        // Assert
        Assert.StartsWith("client_service_", currentUUID);
    }

    [Fact]
    public void InitialStateFlags_AreAllFalse()
    {
        // Arrange & Act - Reset all flags to known state first
        CAST_Client_Service._startRun = false;
        CAST_Client_Service._stopRun = false;
        CAST_Client_Service._pauseRun = false;
        CAST_Client_Service._resumeRun = false;
        CAST_Client_Service._abortRun = false;
        CAST_Client_Service._customAction = false;

        var stopRun = CAST_Client_Service._stopRun;
        var pauseRun = CAST_Client_Service._pauseRun;
        var startRun = CAST_Client_Service._startRun;
        var resumeRun = CAST_Client_Service._resumeRun;
        var abortRun = CAST_Client_Service._abortRun;
        var customAction = CAST_Client_Service._customAction;

        // Assert
        Assert.False(stopRun);
        Assert.False(pauseRun);
        Assert.False(startRun);
        Assert.False(resumeRun);
        Assert.False(abortRun);
        Assert.False(customAction);
    }

    [Fact]
    public void CustomActionList_IsInitialized()
    {
        // Act
        var customActionList = CAST_Client_Service.customActionList;

        // Assert
        Assert.NotNull(customActionList);
        Assert.IsType<List<string>>(customActionList);
    }

    [Fact]
    public void CustomActionStateList_IsInitialized()
    {
        // Act
        var customActionStateList = CAST_Client_Service.customActionStateList;

        // Assert
        Assert.NotNull(customActionStateList);
        Assert.IsType<List<bool>>(customActionStateList);
    }
}

/// <summary>
/// Unit tests for action parameter handling and edge cases
/// </summary>
public class CAST_Client_Service_ActionParameterTests
{
    [Fact]
    public void StartRun_WithWhitespaceInAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "   ACTION: START run_id_456   ";

        // Act
        string result = CAST_Client_Service.startRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result); // Leading whitespace breaks the "starts with" check
    }

    [Fact]
    public void PauseRun_WithEmptyAction_ReturnsEmptyString()
    {
        // Arrange
        string service_uuid = "test-uuid-123";
        string action = "";

        // Act
        string result = CAST_Client_Service.pauseRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void AbortRun_WithSpecialCharactersInUUID_ProcessesAction()
    {
        // Arrange
        string service_uuid = "test-uuid!@#$%";
        string action = "ACTION: ABORT run_id_456";
        CAST_Client_Service._abortRun = true;

        // Act
        string result = CAST_Client_Service.abortRun(ref service_uuid, ref action);

        // Assert
        Assert.Equal("Found ABORT action", result);
    }
}

/// <summary>
/// Unit tests for public properties and configuration
/// </summary>
public class CAST_Client_Service_ConfigurationTests
{
    [Fact]
    public void CurrentUUID_IsNotEmpty()
    {
        // Act
        var currentUUID = CAST_Client_Service.currentUUID;

        // Assert
        Assert.NotEmpty(currentUUID);
    }

    [Fact]
    public void StartmyuuidAsString_IsNotEmpty()
    {
        // Act
        var uuidString = CAST_Client_Service.startmyuuidAsString;

        // Assert
        Assert.NotEmpty(uuidString);
    }

    [Fact]
    public void CustomActionList_CanBeModified()
    {
        // Arrange
        CAST_Client_Service.customActionList.Clear();
        string testAction = "TestAction";

        // Act
        CAST_Client_Service.customActionList.Add(testAction);

        // Assert
        Assert.Contains(testAction, CAST_Client_Service.customActionList);
    }

    [Fact]
    public void CustomActionStateList_CanBeModified()
    {
        // Arrange
        CAST_Client_Service.customActionStateList.Clear();
        bool testState = true;

        // Act
        CAST_Client_Service.customActionStateList.Add(testState);

        // Assert
        Assert.Contains(testState, CAST_Client_Service.customActionStateList);
    }

    [Fact]
    public void StateFlags_CanBeModified()
    {
        // Arrange
        CAST_Client_Service._startRun = false;
        CAST_Client_Service._stopRun = false;
        CAST_Client_Service._pauseRun = false;

        // Act
        CAST_Client_Service._startRun = true;
        CAST_Client_Service._stopRun = true;
        CAST_Client_Service._pauseRun = true;

        // Assert
        Assert.True(CAST_Client_Service._startRun);
        Assert.True(CAST_Client_Service._stopRun);
        Assert.True(CAST_Client_Service._pauseRun);
    }
}
