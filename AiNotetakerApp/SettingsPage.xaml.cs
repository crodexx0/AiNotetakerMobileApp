using AiNotetakerApp.ViewModels;

namespace AiNotetakerApp
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadApiKeyCommand.Execute(null);
        }
    }
}