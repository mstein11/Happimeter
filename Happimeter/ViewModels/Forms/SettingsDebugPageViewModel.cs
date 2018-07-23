using System;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Xamarin.Forms;
using Happimeter.Interfaces;
namespace Happimeter.ViewModels.Forms
{
    public class SettingsDebugPageViewModel : BaseViewModel
    {
        public SettingsDebugPageViewModel()
        {
            UploadDebugSnapshotCommand = new Command(() =>
            {
                ServiceLocator.Instance.Get<ILoggingService>().CreateDebugSnapshot();
                Application.Current.MainPage.DisplayAlert("Debug Snapshot Saved", "You successfully saved the debug snapshot! It will help us make the happimeter a better experience, thank you!", "Ok");
            });
            UploadDatabaseCommand = new Command(async () =>
            {
                var oldText = UploadDatabaseButtonText;
                UploadDatabaseButtonText = "Loading...";
                UploadDatabaseButtonIsEnabled = false;
                var successful = await ServiceLocator.Instance.Get<IHappimeterApiService>().UploadDatabaseForDebug();
                UploadDatabaseButtonText = oldText;
                UploadDatabaseButtonIsEnabled = true;
                if (successful)
                {
                    Application.Current.MainPage.DisplayAlert("Database Uploaded", "You successfully uploaded your database! It will help us make the happimeter a better experience, thank you!", "Ok");
                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Error While Uploading Database", "Unfortunately, there was an error while uploading the database.", "Ok");
                }
            });
        }

        public Command UploadDebugSnapshotCommand { get; set; }
        public Command UploadDatabaseCommand { get; set; }
        private string _uploadDatabaseButtonText = "Upload";
        public string UploadDatabaseButtonText
        {
            get => _uploadDatabaseButtonText;
            set => SetProperty(ref _uploadDatabaseButtonText, value);
        }
        private bool _uploadDatabaseButtonIsEnabled = true;
        public bool UploadDatabaseButtonIsEnabled
        {
            get => _uploadDatabaseButtonIsEnabled;
            set => SetProperty(ref _uploadDatabaseButtonIsEnabled, value);
        }
    }
}
