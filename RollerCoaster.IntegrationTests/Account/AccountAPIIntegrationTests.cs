using DickinsonBros.Guid.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RollerCoaster.Account.API.Proxy;
using RollerCoaster.Account.Proxy.Models;
using RollerCoaster.IntegrationTests.Services.AccountDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RollerCoaster.IntegrationTests.Account
{
    public class AccountAPIIntegrationTests : IAccountAPIIntegrationTests
    {
        internal readonly IGuidService _guidService;
        internal readonly IAccountProxyService _accountProxyService;
        internal readonly IAccountDBService _accountDBService;
        public AccountAPIIntegrationTests
        (
            IGuidService guidService,
            IAccountProxyService accountProxyService,
            IAccountDBService accountDBService
        )
        {
            _guidService = guidService;
            _accountProxyService = accountProxyService;
            _accountDBService = accountDBService;
        }
        public async Task CreateAccountAsync_InvaildEmail_ReturnBadRequestAsync(List<string> successLog)
        {
            //Setup
            var request = new CreateAccountRequest
            {
                Email = $"",
                Password = _guidService.NewGuid().ToString(),
                Username = _guidService.NewGuid().ToString()
            };

            try
            {
                //Act
                var response = await _accountProxyService.CreateAccountAsync(request).ConfigureAwait(false);

                //Verify

                Assert.AreEqual(400, (int)response.HttpResponseMessage.StatusCode, $"StatusCode. Message: {await response.HttpResponseMessage.Content.ReadAsStringAsync()}");
                successLog.Add("StatusCode is 400");
            }
            finally
            {

            }
        }

        public async Task CreateAccountAsync_UsernameLessThen1Char_ReturnBadRequest(List<string> successLog)
        {
            //Setup
            var request = new CreateAccountRequest
            {
                Email = $"{_guidService.NewGuid().ToString()}@FakeMail.com",
                Password = _guidService.NewGuid().ToString(),
                Username = ""
            };

            try
            {
                //Act
                var response = await _accountProxyService.CreateAccountAsync(request).ConfigureAwait(false);

                //Verify

                Assert.AreEqual(400, (int)response.HttpResponseMessage.StatusCode, $"StatusCode. Message: {await response.HttpResponseMessage.Content.ReadAsStringAsync()}");
                successLog.Add("StatusCode is 400");
            }
            finally
            {

            }
        }

        public async Task CreateAccountAsync_PasswordLessThen8Chars_ReturnBadRequest(List<string> successLog)
        {
            //Setup
            var request = new CreateAccountRequest
            {
                Email = $"{_guidService.NewGuid().ToString()}@FakeMail.com",
                Password = "1234567",
                Username = _guidService.NewGuid().ToString(),
            };

            try
            {
                //Act
                var response = await _accountProxyService.CreateAccountAsync(request).ConfigureAwait(false);

                //Verify

                Assert.AreEqual(400, (int)response.HttpResponseMessage.StatusCode, $"StatusCode. Message: {await response.HttpResponseMessage.Content.ReadAsStringAsync()}");
                successLog.Add("StatusCode is 400");
            }
            finally
            {

            }
        }

        public async Task CreateAccountAsync_DuplicateEmail_Return409(List<string> successLog)
        {
            //Setup
            var firstRequest = new CreateAccountRequest
            {
                Email = $"{_guidService.NewGuid().ToString()}@FakeMail.com",
                Password = _guidService.NewGuid().ToString(),
                Username = _guidService.NewGuid().ToString(),
            };

            var secondRequest = new CreateAccountRequest
            {
                Email = firstRequest.Email,
                Password = _guidService.NewGuid().ToString(),
                Username = _guidService.NewGuid().ToString(),
            };

            try
            {
                //Act

                var firstCallResponse = await _accountProxyService.CreateAccountAsync(firstRequest).ConfigureAwait(false);
                var secondCallResponse = await _accountProxyService.CreateAccountAsync(secondRequest).ConfigureAwait(false);

                //Verify

                Assert.AreEqual(200, (int)firstCallResponse.HttpResponseMessage.StatusCode, $"StatusCode. First Call Message: {await firstCallResponse.HttpResponseMessage.Content.ReadAsStringAsync()}");
                successLog.Add("Setup - First Call StatusCode is 200");
                Assert.AreEqual(409, (int)secondCallResponse.HttpResponseMessage.StatusCode, $"StatusCode. Second Call Message");
                successLog.Add("Second Call StatusCode is 409");

            }
            finally
            {
                await _accountDBService.DeleteAccountByUsernameAsync(firstRequest.Username).ConfigureAwait(false);
            }
        }

        public async Task CreateAccountAsync_DuplicateUsername_Return409(List<string> successLog)
        {
            //Setup
            var firstRequest = new CreateAccountRequest
            {
                Email = $"{_guidService.NewGuid().ToString()}@FakeMail.com",
                Password = _guidService.NewGuid().ToString(),
                Username = _guidService.NewGuid().ToString(),
            };

            var secondRequest = new CreateAccountRequest
            {
                Email = $"{_guidService.NewGuid().ToString()}@FakeMail.com",
                Password = _guidService.NewGuid().ToString(),
                Username = firstRequest.Username
            };

            try
            {
                //Act
                var firstCallResponse = await _accountProxyService.CreateAccountAsync(firstRequest).ConfigureAwait(false);
                var secondCallResponse = await _accountProxyService.CreateAccountAsync(secondRequest).ConfigureAwait(false);

                //Verify

                Assert.AreEqual(200, (int)firstCallResponse.HttpResponseMessage.StatusCode, $"StatusCode. First Call Message: {await firstCallResponse.HttpResponseMessage.Content.ReadAsStringAsync()}");
                successLog.Add("Setup - First Call StatusCode is 200");
                Assert.AreEqual(409, (int)secondCallResponse.HttpResponseMessage.StatusCode, $"StatusCode. Second Call Message");
                successLog.Add("Second Call StatusCode is 409");
            }
            finally
            {
                await _accountDBService.DeleteAccountByUsernameAsync(firstRequest.Username).ConfigureAwait(false);
            }
        }

        public async Task CreateAccountAsync_NewUserWithoutEmail_Return200(List<string> successLog)
        {
            //Setup
            var request = new CreateAccountRequest
            {
                Email = null,
                Password = _guidService.NewGuid().ToString(),
                Username = _guidService.NewGuid().ToString(),
            };

            try
            {
                //Act

                var response = await _accountProxyService.CreateAccountAsync(request).ConfigureAwait(false);

                //Verify
                Assert.AreEqual(200, (int)response.HttpResponseMessage.StatusCode, $"StatusCode");
                successLog.Add("StatusCode is 200");

                var account = await _accountDBService.SelectAccountByUsernameAsync(request.Username).ConfigureAwait(false);
                
                Assert.IsNotNull(account, "AccountDBService.SelectAccountByUsernameAsync - Returned Null");
                successLog.Add($"AccountDBService.SelectAccountByUsernameAsync - Returned A Value");
                
                Assert.IsNull(account.Email, $"AccountDBService.SelectAccountByUsernameAsync - Email is not null");
                successLog.Add($"AccountDBService.SelectAccountByUsernameAsync - Email is null");
                
                Assert.AreEqual(request.Username, account.Username, $"AccountDBService.SelectAccountByUsernameAsync - Username does not match");
                successLog.Add($"AccountDBService.SelectAccountByUsernameAsync - Username matchs");

            }
            finally
            {
                await _accountDBService.DeleteAccountByUsernameAsync(request.Username).ConfigureAwait(false);
            }
        }

        public async Task CreateAccountAsync_NewUserWithEmail_Return200(List<string> successLog)
        {
            //Setup
            var request = new CreateAccountRequest
            {
                Email = $"{_guidService.NewGuid().ToString()}@FakeMail.com",
                Password = _guidService.NewGuid().ToString(),
                Username = _guidService.NewGuid().ToString(),
            };

            try
            {
                //Act

                var response = await _accountProxyService.CreateAccountAsync(request).ConfigureAwait(false);

                //Verify
                Assert.AreEqual(200, (int)response.HttpResponseMessage.StatusCode);
                successLog.Add("StatusCode is 200");

                var account = await _accountDBService.SelectAccountByEmailAsync(request.Email).ConfigureAwait(false);
                
                Assert.IsNotNull(account, "AccountDBService.SelectAccountByEmailAsync - Returned Null");
                successLog.Add($"AccountDBService.SelectAccountByEmailAsync - Returned A Value");
                
                Assert.AreEqual(request.Email, account.Email, $"AccountDBService.SelectAccountByEmailAsync - Email does not match");
                successLog.Add($"AccountDBService.SelectAccountByEmailAsync - Email matchs");
                
                Assert.AreEqual(request.Username, account.Username, $"AccountDBService.SelectAccountByEmailAsync - Username matchs");
                successLog.Add($"AccountDBService.SelectAccountByEmailAsync - Username matchs");
            }
            finally
            {
                await _accountDBService.DeleteAccountByUsernameAsync(request.Username).ConfigureAwait(false);
            }
        }
    }
}
