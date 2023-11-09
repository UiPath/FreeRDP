using Microsoft.Extensions.Logging;
using Moq;

namespace UiPath.SessionTools.Tests;

[Trait("Subject", nameof(ProcessRunner))]
public class ProcessRunnerTests
{
    [Fact]
    public async Task Run_ShouldProduceAConsistentReport_WhenCancellationOccurs()
    {
        const string reachable1 = "bf2d62bdc431482fbc5b8b5587cb5ee2";
        const string reachable2 = "4da0af0dae7246e998a5c579e922041f";
        const string unreachable = "44c681c32fd14b8fb3fda81371970f52";

        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var act = () => new ProcessRunner(mockLogger.Object).Run(
            fileName: "cmd.exe",
            arguments: $"/c echo {reachable1} & echo {reachable2} & ping -l 0 -n 30 127.0.0.1 & echo {unreachable}",
            workingDirectory: "",
            throwOnNonZero: true,
            killOnCancelation: true,
            ct: ctsTimeout.Token);

        var ex = await act.ShouldThrowAsync<OperationCanceledException>();

        ValidateAppearances(reachable1, Times.AtLeastOnce());
        ValidateAppearances(reachable2, Times.AtLeastOnce());
        ValidateAppearances(unreachable, Times.Never());

        void ValidateAppearances(string text, Times times)
        => mockLogger.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Split(text, StringSplitOptions.None).Length >= 3),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            times);        
    }
}
