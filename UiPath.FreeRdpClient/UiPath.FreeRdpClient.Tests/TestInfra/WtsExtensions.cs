using UiPath.Rdp;
using UiPath.SessionTools;

namespace UiPath.FreeRdp.Tests;

internal static class WtsExtensions
{
    public static int? FindFirstSession(this Wts wts, RdpConnectionSettings connectionSettings)
    {
        var sessionIds = wts.GetSessionIdList();

        foreach (int sessionId in sessionIds)
        {
            var sessionInfo = wts.QuerySessionInformation(sessionId).SessionInfo();
            if (DateTimeOffset
                .FromFileTime(sessionInfo.Data.ConnectTime) > connectionSettings.BeforeConnectTimestamp
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
