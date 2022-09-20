using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using UiPath.FreeRdp.Tests.TestInfra;

namespace UiPath.FreeRdp.Tests.Faking;

public class UserExistsDetail
{
    private NetworkCredential _credentials = new NetworkCredential();
    public string UserNameUPN { get => _credentials.UserName + "@" + _credentials.Domain; }
    public string UserName { get => _credentials.GetFullUserName(); set => _credentials.SetFullUserName(value); }
    public string Password { get => _credentials.Password; set => _credentials.Password = value; }

    public List<string> Groups { get; } = new() { "Remote Desktop Users", "Administrators" };
}

public interface IUserContext
{
    Task EnsureUserExists(UserExistsDetail userDetail);
}


public class UserContextReal : UserContextBase
{
    private readonly ILogger<IUserContext> _log;

    public UserContextReal(ILogger<IUserContext> log)
    {
        _log = log;
    }

    protected override async Task DoCreateUser(UserExistsDetail userDetail)
    {
        var username = userDetail.UserName.ToLowerInvariant().Replace(UserNames.DefaultDomainName.ToLowerInvariant() + "\\", "");
        await new ProcessStartInfo
        {
            Arguments = $"user {username} {userDetail.Password} /add",
            FileName = "net"
        }.ExecuteWithLogs(_log);
        await new ProcessStartInfo
        {
            Arguments = $"user {username} /active /expires:never",
            FileName = "net"
        }.ExecuteWithLogs(_log);
        await new ProcessStartInfo
        {
            Arguments = $"user {username} /passwordchg:no",
            FileName = "net"
        }.ExecuteWithLogs(_log);
        await new ProcessStartInfo
        {
            Arguments = $"user {username}",
            FileName = "net"
        }.ExecuteWithLogs(_log);

        foreach (var g in userDetail.Groups)
        {
            await new ProcessStartInfo
            {
                Arguments = $"localgroup \"{g}\" {username} /add",
                FileName = "net"
            }.ExecuteWithLogs(_log);
        }
        await new ProcessStartInfo
        {
            Arguments = $"localgroup \"Remote Desktop Users\"",
            FileName = "net"
        }.ExecuteWithLogs(_log);
        await new ProcessStartInfo
        {
            Arguments = $"localgroup \"Administrators\"",
            FileName = "net"
        }.ExecuteWithLogs(_log);
    }

}
public abstract class UserContextBase : IUserContext
{
    protected static readonly ConcurrentDictionary<string, string> CreatedUsersByName = new();

    public async Task EnsureUserExists(UserExistsDetail userDetail)
    {
        var userDetailAsJson = JsonConvert.SerializeObject(userDetail);
        CreatedUsersByName.TryGetValue(userDetail.UserName, out var user);
        if (user == userDetailAsJson)
            return;
        await DoCreateUser(userDetail);
        CreatedUsersByName.TryAdd(userDetail.UserName, userDetailAsJson);
    }

    public UserExistsDetail? GetUser(string userName)
    {
        if (CreatedUsersByName.TryGetValue(userName, out var user))
            return JsonConvert.DeserializeObject<UserExistsDetail>(user);

        return null;
    }

    protected abstract Task DoCreateUser(UserExistsDetail userDetail);
}

public class UserContextFake : UserContextBase
{
    protected override Task DoCreateUser(UserExistsDetail userDetail)
    => Task.CompletedTask;
}