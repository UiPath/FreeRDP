using UiPath.Rdp;
using UiPath.SessionTools;

namespace UiPath.FreeRdp.Tests;

internal static class WtsExtensions
{
    public static async Task<int> FindSession(this Wts wts, RdpConnectionSettings connectionSettings)
    {
        int? sessionId = null;
        await WaitFor.Predicate(() => (sessionId = wts.FindFirstSession(connectionSettings)) is not null);
        return sessionId!.Value;
    }

    public static async Task WaitNoSession(this Wts wts, RdpConnectionSettings connectionSettings)
    {
        await WaitFor.Predicate(() => wts.FindFirstSession(connectionSettings) is null);
    }

    private static int? FindFirstSession(this Wts wts, RdpConnectionSettings connectionSettings)
    {
        var sessionIds = wts.GetSessionIdList();

        foreach (int sessionId in sessionIds)
        {
            var sessionInfo = wts.QuerySessionInformation(sessionId).SessionInfo();
            if (DateTimeOffset
                .FromFileTime(sessionInfo.Data.ConnectTime) >= connectionSettings.BeforeConnectTimestamp
                && sessionInfo.Data.ConnectTime > sessionInfo.Data.DisconnectTime
                && sessionInfo.Data.SessionId < 65000
                )
            {
                return sessionId;
            }
        }

        return null;
    }
}
