using AiNotetakerApp.ViewModels;

namespace AiNotetakerApp
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();

            // Set the BindingContext so the XAML knows where to find its data
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Automatically load meetings when the page appears on screen
            await _viewModel.LoadFolderAsync();
            await _viewModel.LoadMeetingsAsync();
        }
    }
}
