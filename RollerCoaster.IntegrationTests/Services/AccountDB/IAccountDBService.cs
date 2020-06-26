using System.Threading.Tasks;

namespace RollerCoaster.IntegrationTests.Services.AccountDB
{
    public interface IAccountDBService
    {
        Task<Models.Account> SelectAccountByEmailAsync(string email);
        Task DeleteAccountByUsernameAsync(string email);
        Task<Models.Account> SelectAccountByUsernameAsync(string username);
    }
}