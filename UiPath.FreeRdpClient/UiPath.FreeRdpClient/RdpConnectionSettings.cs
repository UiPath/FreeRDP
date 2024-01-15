using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace UiPath.Rdp;

public class RdpConnectionSettings
{
    public string Username { get; }
    public string Domain { get; }
    public string Password { get; }

    public int DesktopWidth { get; set; } = 1024;
    public int DesktopHeight { get; set; } = 768;
    public int ColorDepth { get; set; } = 32;
    public bool FontSmoothing { get; set; }

    public string HostName { get; set; } = "localhost";
    public int? Port { get; set; }

    [MaxLength(15, ErrorMessage = "used for generating an eventName and logging scope name")]
    public string ScopeName { get; set; }
    [MaxLength(15, ErrorMessage = "Sometimes :) Windows returns only first 15 chars for a session ClientName")]
    public string ClientName { get; set; }

    public DateTimeOffset? BeforeConnectTimestamp { get; set; }

    public RdpConnectionSettings(string username, string domain, string password)
    {
        Username = username;
        Domain = domain;
        Password = password;
        ScopeName = GetUniqueScopeName();
    }

    private const int Keep4DecimalsDivider = 1_0000;
    private const int Keep6DecimalsDivider = 1_000_000;

    private static volatile int ConnectionId = 0;
    private static readonly string ScopeNameBase = "RDP_" + (Environment.ProcessId % Keep6DecimalsDivider).ToString(CultureInfo.InvariantCulture) + "_";
    private static string GetUniqueScopeName() => ScopeNameBase
        + (Interlocked.Increment(ref ConnectionId) % Keep4DecimalsDivider).ToString(CultureInfo.InvariantCulture);
}
