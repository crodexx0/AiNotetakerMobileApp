using AiNotetakerApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AiNotetakerApp.ViewModels
{
    // The QueryProperty allows us to pass the Meeting object when navigating to this page
    [QueryProperty(nameof(CurrentMeeting), "Meeting")]
    public partial class MeetingDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Meeting _currentMeeting;
    }
}