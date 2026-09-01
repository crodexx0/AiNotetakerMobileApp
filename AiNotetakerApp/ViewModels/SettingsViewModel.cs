using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace AiNotetakerApp.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _apiKey;

        [RelayCommand]
        private async Task LoadApiKeyAsync()
        {
            // Retrieve the saved key when the page opens
            var savedKey = await SecureStorage.Default.GetAsync("OpenAiKey");
            if (!string.IsNullOrEmpty(savedKey))
            {
                ApiKey = savedKey;
            }
        }

        [RelayCommand]
        public async Task SaveApiKeyAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid API Key.", "OK");
                return;
            }

            // Save the key securely to the device's secure storage
            await SecureStorage.Default.SetAsync("OpenAiKey", ApiKey.Trim());

            await Application.Current.MainPage.DisplayAlert("Success", "API Key saved securely.", "OK");

            // Navigate back to the home screen
            await Shell.Current.GoToAsync("..");
        }
    }
}