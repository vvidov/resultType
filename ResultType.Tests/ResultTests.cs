using Xunit;

namespace ResultType.Tests;

public class ResultTests
{
    [Fact]
    public void WhenCreatingErrorResult_ShouldHaveError()
    {
        // Arrange
        const string errorCode = "ERR_001";
        const string errorMessage = "Invalid operation";
        Error error = new(errorCode, errorMessage);

        // Act
        Result<int, Error> result = error;

        // Assert
        Assert.False(result.HasResult());
        Assert.Equal(0, result.Value); // default value for int
        Assert.Equal(error, result.Error);
        Assert.Equal(errorCode, result.Error?.Code);
        Assert.Equal(errorMessage, result.Error?.Message);
    }

    [Fact]
    public void WhenMatchingSuccess_ShouldExecuteSuccessAction()
    {
        // Arrange
        Result<int, Error> result = 42;
        var wasSuccessCalled = false;

        // Act
        result.Match(
            success: _ => wasSuccessCalled = true,
            failure: _ => Assert.Fail("Should not call failure"));

        // Assert
        Assert.True(wasSuccessCalled);
    }

    [Fact]
    public void WhenMatchingError_ShouldExecuteFailureAction()
    {
        // Arrange
        var error = new Error("ERR_002", "Test error");
        Result<int, Error> result = error;
        var wasFailureCalled = false;

        // Act
        result.Match(
            success: _ => Assert.Fail("Should not call success"),
            failure: _ => wasFailureCalled = true);

        // Assert
        Assert.True(wasFailureCalled);
    }

    [Fact]
    public void WhenChainingSeveralOperations_ShouldPreserveError()
    {
        // Arrange
        var error = new Error("ERR_003", "Initial error");
        Result<int, Error> result = error;

        // Act
        var final = result
            .OnSuccess(x => x * 2)
            .OnSuccess(x => x + 1);

        // Assert
        Assert.False(final.HasResult());
        Assert.Equal(error, final.Error);
    }

    [Fact]
    public void WhenValidatingWithError_ShouldStopAtFirstError()
    {
        // Arrange
        Result<int, Error> result = 15;

        // Act
        var final = result
            .OnSuccess<int>(x => x > 0 
                ? x
                : new Error("ERR_004", "Must be positive"))
            .OnSuccess<int>(x => x % 2 == 0 
                ? x
                : new Error("ERR_005", "Must be even"))
            .OnSuccess<int>(x => x < 100 
                ? x
                : new Error("ERR_006", "Must be less than 100"));

        // Assert
        Assert.False(final.HasResult());
        Assert.Equal("ERR_005", final.Error?.Code);
        Assert.Equal("Must be even", final.Error?.Message);
    }

    [Fact]
    public void WhenCreatingErrorWithoutMessage_ShouldHaveOnlyCode()
    {
        // Arrange
        const string errorCode = "ERR_008";
        Error error = new(errorCode);
        Result<string, Error> result = error;

        // Assert
        Assert.False(result.HasResult());
        Assert.Equal(errorCode, result.Error?.Code);
        Assert.Null(result.Error?.Message);
    }

    [Fact]
    public void WhenTransformingTypes_ShouldWorkCorrectly()
    {
        // Arrange
        Result<int, Error> result = 42;

        // Act
        var stringResult = result
            .OnSuccess(x => x.ToString());

        // Assert
        Assert.True(stringResult.HasResult());
        Assert.Equal("42", stringResult.Value);
    }
}
