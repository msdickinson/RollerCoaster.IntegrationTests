using System.Collections.Generic;
using System.Threading.Tasks;

namespace RollerCoaster.IntegrationTests.Account
{
    public interface IAccountAPIIntegrationTests
    {
        Task CreateAccountAsync_DuplicateEmail_Return409(List<string> successLog);
        Task CreateAccountAsync_DuplicateUsername_Return409(List<string> successLog);
        Task CreateAccountAsync_InvaildEmail_ReturnBadRequestAsync(List<string> successLog);
        Task CreateAccountAsync_NewUserWithEmail_Return200(List<string> successLog);
        Task CreateAccountAsync_NewUserWithoutEmail_Return200(List<string> successLog);
        Task CreateAccountAsync_PasswordLessThen8Chars_ReturnBadRequest(List<string> successLog);
        Task CreateAccountAsync_UsernameLessThen1Char_ReturnBadRequest(List<string> successLog);
    }
}