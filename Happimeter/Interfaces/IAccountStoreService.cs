using System;
using Xamarin.Auth;

namespace Happimeter.Interfaces
{
    public interface IAccountStoreService
    {
        Account GetAccount();
        string GetAccountToken();
        void SaveAccount(string userName, string token, DateTime expires);
        bool IsAuthenticated();
        string GetToken();
    }
}