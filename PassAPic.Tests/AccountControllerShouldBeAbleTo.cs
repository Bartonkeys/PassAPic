using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PassAPic.Tests
{
    [TestClass]
    public class AccountControllerShouldBeAbleTo
    {
        private List<AccountModel> receivedAccounts = new List<AccountModel>();

        [TestMethod]
        public async Task RegisterAnonUsers()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://passapic.apphb.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var usernames = new List<String> {"user1","user2","user3","user4"};

                foreach (var username in usernames)
                {
                    var user = new AccountModel() { Username = username };
                    var response = await client.PostAsJsonAsync("api/account/anon", user);
                    if (response.IsSuccessStatusCode)
                    {
                        var account = await response.Content.ReadAsAsync<AccountModel>();
                        receivedAccounts.Add(account);
                    }
                    else Assert.Fail();
                }

                Assert.IsTrue(receivedAccounts.Count == 4);

             }
        }

        [TestMethod]
        public async Task RegisterAnonUsersAndGenerateRandomUserName()
        {
            AccountModel account = new AccountModel();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://passapic.apphb.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var user = new AccountModel {Username = String.Empty};
                var response = await client.PostAsJsonAsync("api/account/anon", user);
                if (response.IsSuccessStatusCode)
                {
                    account = await response.Content.ReadAsAsync<AccountModel>();
                }
                else Assert.Fail();

                Assert.IsTrue(account.Username.Length == 8);

            }
        }

        [TestMethod]
        public async Task LoginUsers()
        {
            HttpResponseMessage response;
            List<AccountModel> onlineAccounts = new List<AccountModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://passapic.apphb.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var users = new List<AccountModel>
                {
                    new AccountModel{UserId = 1},
                    new AccountModel{UserId = 2},
                    new AccountModel{UserId = 3},
                    new AccountModel{UserId = 4}
                };

                foreach (var account in users)
                {
                    response = await client.PostAsync("api/account/login/" +account.UserId, null);   
                    if(!response.IsSuccessStatusCode) Assert.Fail();
                }

                response = await client.GetAsync("api/account/online");

                if (response.IsSuccessStatusCode)
                {
                    var accounts = await response.Content.ReadAsAsync<List<AccountModel>>();               
                    onlineAccounts = accounts;
                }

                Assert.IsTrue(onlineAccounts.Count == 4);

            }
        }

        [TestMethod]
        public async Task LogoutUsers()
        {
            HttpResponseMessage response;
            List<AccountModel> onlineAccounts = new List<AccountModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://passapic.apphb.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var users = new List<AccountModel>
                {
                    new AccountModel{UserId = 1},
                    new AccountModel{UserId = 2},
                    new AccountModel{UserId = 3},
                    new AccountModel{UserId = 4}
                };

                foreach (var account in users)
                {
                    response = await client.PostAsync("api/account/logout/" + account.UserId, null);
                    if (!response.IsSuccessStatusCode) Assert.Fail();
                }

                response = await client.GetAsync("api/account/online");

                if (response.IsSuccessStatusCode)
                {
                    var accounts = await response.Content.ReadAsAsync<List<AccountModel>>();
                    onlineAccounts = accounts;
                }

                Assert.IsTrue(onlineAccounts.Count == 0);

            }
        }
    }
}
