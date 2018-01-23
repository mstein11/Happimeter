using System;
using Xamarin.Auth;
using Happimeter.Interfaces;
using System.Linq;
using System.Diagnostics;

namespace Happimeter.Services
{
    public class AccountStoreService : IAccountStoreService
    {
        private AccountStore _store;

        private const string TokenPropertyName = "Token";
        private const string ExpiresPropertyName = "Expires";
        private const string AppName = "Happimeter";

        public AccountStoreService()
        {
            _store = AccountStore.Create();
        }

        public void SaveAccount(string userName, string token, DateTime expires) {

            var account = new Account();
            account.Username = userName;
            account.Properties.Add(TokenPropertyName, token);
            account.Properties.Add(ExpiresPropertyName, expires.ToLongDateString());
            _store.Save(account, AppName);   
        }

        public Account GetAccount() {
            return _store.FindAccountsForService(AppName).FirstOrDefault();
        }

        public bool IsAuthenticated() {
            var account = GetAccount();
            var expiresString = account.Properties.FirstOrDefault(x => x.Key == ExpiresPropertyName).Value;
            var expiresDatetime = DateTime.Parse(expiresString);

            return expiresDatetime > DateTime.UtcNow;
        }

        public string GetToken() {
            if (IsAuthenticated()) {
                return GetAccount().Properties.FirstOrDefault(x => x.Key == TokenPropertyName).Value;
            }
            return null;
        }

        public string GetAccountToken() {
            var account = GetAccount();
            if (account != null) {
                return account.Properties["Token"];
            }
            return null;
        }
    }
}
