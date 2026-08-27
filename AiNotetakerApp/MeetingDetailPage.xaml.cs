using AiNotetakerApp.ViewModels;

namespace AiNotetakerApp
{
    public partial class MeetingDetailPage : ContentPage
    {
        public MeetingDetailPage(MeetingDetailViewModel viewModel)
        {
            InitializeComponent();
            
            // This is the missing link! 
            // It tells the UI where to find {Binding CurrentMeeting.Title}
            BindingContext = viewModel;
        }
    }
}