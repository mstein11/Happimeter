using System;
using System.Windows.Input;
using Happimeter.Core.Database;
using Happimeter.Interfaces;
using Happimeter.Services;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsPageViewModel : BaseViewModel
    {
        public SettingsPageViewModel()
        {
            UserEmail = ServiceLocator
                .Instance
                .Get<IAccountStoreService>()
                .GetAccount().Username;
            
            GenericQuestionGroupId = ServiceLocator
                .Instance
                .Get<IConfigService>()
                .GetConfigValueByKey(ConfigService.GenericQuestionGroupIdKey);

            Logout = new Command(() =>
            {
                ServiceLocator
                    .Instance
                    .Get<IAccountStoreService>()
                    .DeleteAccount();

                ServiceLocator
                    .Instance
                    .Get<ISharedDatabaseContext>()
                    .ResetDatabase();

                ServiceLocator
                    .Instance
                    .Get<INativeNavigationService>()
                    .NavigateToLoginPage();
            });

            ChangeGenericQuestionGroupId = new Command(() =>
            {
                ServiceLocator
                    .Instance
                    .Get<IConfigService>()
                    .AddOrUpdateConfigEntry(ConfigService.GenericQuestionGroupIdKey, GenericQuestionGroupId);
            });
        }

        private string _userEmail;
        public string UserEmail 
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        private string _genericQuestionGroupId;
        public string GenericQuestionGroupId 
        {
            get => _genericQuestionGroupId;
            set => SetProperty(ref _genericQuestionGroupId, value);
        }

        public ICommand Logout { protected set; get; }
        public ICommand ChangeGenericQuestionGroupId { protected set; get; }
    }
}
