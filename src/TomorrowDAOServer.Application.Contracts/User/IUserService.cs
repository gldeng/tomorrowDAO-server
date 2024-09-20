using System.Threading.Tasks;

namespace TomorrowDAOServer.User;

public interface IUserService
{
    public Task<string> GetCurrentUserAddressAsync(string chainId);
}