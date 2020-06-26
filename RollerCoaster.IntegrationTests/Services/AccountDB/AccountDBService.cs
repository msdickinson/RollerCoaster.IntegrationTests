using DickinsonBros.SQL.Abstractions;
using Microsoft.Extensions.Options;
using RollerCoaster.IntegrationTests.Services.AccountDB.Models;
using System.Data;
using System.Threading.Tasks;

namespace RollerCoaster.IntegrationTests.Services.AccountDB
{
    public class AccountDBService : IAccountDBService
    {
        internal readonly string _connectionString;
        internal readonly ISQLService _sqlService;

        internal const string SELECT_ACCOUNT_BY_EMAIL = "[Account].[SelectAccountByEmail]";
        internal const string SELECT_ACCOUNT_BY_USERNAME = "[Account].[SelectAccountByUserName]";
        internal const string DELETE_ACCOUNT = "[Account].[DeleteAccount]";

        public AccountDBService
        (
            IOptions<RollerCoasterDBOptions> rollerCoasterDBOptions,
            ISQLService sqlService
        )
        {
            _connectionString = rollerCoasterDBOptions.Value.ConnectionString;
            _sqlService = sqlService;
        }

        public async Task<Models.Account> SelectAccountByEmailAsync(string email)
        {
            return await _sqlService
                        .QueryFirstAsync<Models.Account>
                         (
                             _connectionString,
                             SELECT_ACCOUNT_BY_EMAIL,
                             new
                             {
                                 Email = email
                             },
                             commandType: CommandType.StoredProcedure
                         );
        }

        public async Task<Models.Account> SelectAccountByUsernameAsync(string username)
        {
            return await _sqlService
                        .QueryFirstAsync<Models.Account>
                         (
                             _connectionString,
                             SELECT_ACCOUNT_BY_USERNAME,
                             new
                             {
                                 Username = username
                             },
                             commandType: CommandType.StoredProcedure
                         );
        }

        public async Task DeleteAccountByUsernameAsync(string username)
        {
            await _sqlService
                  .ExecuteAsync
                   (
                       _connectionString,
                       DELETE_ACCOUNT,
                       new
                       {
                           Username = username
                       },
                       commandType: CommandType.StoredProcedure
                   );
        }

    }
}
