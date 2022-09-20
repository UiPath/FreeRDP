using Microsoft.Extensions.DependencyInjection;

namespace UiPath.FreeRdp.Tests.Faking;

public static class UserContextExtensions
{
    public static UserExistsDetail UserAdmin = new() { UserName = "UserAdmin", Password = "somePass4A@3" };
    public static UserExistsDetail Other = new() { UserName = "UserOther", Password = "somePass4B@3" };

    public static async Task<UserExistsDetail> GivenUser(this TestHost host)
    => await host.EnsureUserExists(UserAdmin);

    public static async Task<UserExistsDetail> GivenAdmin(this TestHost host)
    => await host.EnsureUserExists(UserAdmin);

    public static async Task<UserExistsDetail> GivenUserOther(this TestHost host)
    => await host.EnsureUserExists(Other);

    public static async Task<UserExistsDetail> EnsureUserExists(this TestHost host, UserExistsDetail user)
    {
        var uc = host.GetRequiredService<IUserContext>();
        await uc.EnsureUserExists(user);
        return user;
    }
}