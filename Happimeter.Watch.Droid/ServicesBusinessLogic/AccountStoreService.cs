using System;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Xamarin.Auth;
using Happimeter.Watch.Droid.Database;
namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
	public class AccountStoreService : IAccountStoreService
	{
		public AccountStoreService()
		{
		}

		public void DeleteAccount()
		{
			throw new NotImplementedException();
		}

		public Account GetAccount()
		{
			var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive);
			if (pairing == null)
			{
				return null;
			}
			var name = pairing.PairedWithUserName;
			var id = pairing.PairedWithUserId;
			var account = new Account($"{name} ({id})");
			return account;
		}

		public string GetAccountToken()
		{
			throw new NotImplementedException();
		}

		public int GetAccountUserId()
		{
			throw new NotImplementedException();
		}

		public string GetToken()
		{
			throw new NotImplementedException();
		}

		public bool IsAuthenticated()
		{
			throw new NotImplementedException();
		}

		public void SaveAccount(string userName, string token, int Id, DateTime expires)
		{
			throw new NotImplementedException();
		}
	}
}
