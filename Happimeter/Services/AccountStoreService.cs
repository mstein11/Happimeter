using System;
using Xamarin.Auth;
using Happimeter.Interfaces;
using System.Linq;
using System.Diagnostics;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;

namespace Happimeter.Services
{
    public class AccountStoreService : IAccountStoreService
    {
        private AccountStore _store;

        private const string TokenPropertyName = "Token";
        private const string ExpiresPropertyName = "Expires";
        private const string UserIdPropertyName = "UserId";
        private const string AppName = "Happimeter";

        public AccountStoreService()
        {
            _store = AccountStore.Create();
        }

        public void SaveAccount(string userName, string token, int userId, DateTime expires)
        {
            var account = new Account();
            account.Username = userName;
            account.Properties.Add(TokenPropertyName, token);
            account.Properties.Add(ExpiresPropertyName, expires.ToLongDateString());
            account.Properties.Add(UserIdPropertyName, userId.ToString());
            _store.Save(account, AppName);
            ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.LoginEvent);
            ServiceLocator.Instance.Get<IPredictionService>().DownloadAndSavePrediction();
        }

        public Account GetAccount()
        {
            return _store.FindAccountsForService(AppName).FirstOrDefault();
        }

        public void DeleteAccount()
        {
            ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.LogoutEvent);
            _store.Delete(GetAccount(), AppName);
        }

        public int GetAccountUserId()
        {
            var account = GetAccount();
            return int.Parse(account.Properties.FirstOrDefault(x => x.Key == UserIdPropertyName).Value);
        }

        public bool IsAuthenticated()
        {
            var account = GetAccount();
            if (account == null)
            {
                return false;
            }
            var expiresString = account.Properties.FirstOrDefault(x => x.Key == ExpiresPropertyName).Value;
            var expiresDatetime = DateTime.Parse(expiresString);

            return expiresDatetime > DateTime.UtcNow;
        }

        public string GetToken()
        {
            if (IsAuthenticated())
            {
                return GetAccount().Properties.FirstOrDefault(x => x.Key == TokenPropertyName).Value;
            }
            return null;
        }

        public string GetAccountToken()
        {
            var account = GetAccount();
            if (account != null)
            {
                return account.Properties["Token"];
            }
            return null;
        }
    }
}
