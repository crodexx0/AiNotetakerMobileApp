using AiNotetakerApp.Models;
using AiNotetakerApp.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AiNotetakerApp.Services;

namespace AiNotetakerApp.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly AudioService _audioService;
        private readonly AiService _aiService;

        // ObservableCollection automatically updates the UI when items are added or removed
        [ObservableProperty]
        private ObservableCollection<Meeting> _meetings;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRecording;

        // Constructor Injection: The app automatically provides the DatabaseService here
        public MainViewModel(DatabaseService databaseService, AudioService audioService, AiService aiService)
        {
            _databaseService = databaseService;
            _audioService = audioService;
            _aiService = aiService;
            _meetings = new ObservableCollection<Meeting>();
        }

        // The [RelayCommand} attribute automatically turns this method into a command
        // that you button can bind to in the XAML (e.g., Command="{Binding LoadMeetingsCommand}")
        [RelayCommand]
        public async Task LoadMeetingsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Fetch from database
                var meetingsFromDb = await _databaseService.GetMeetingAsync();
                
                // Force the UI to update on the Main Thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Clear and reload the observable collection
                    Meetings.Clear();
                    foreach (var meeting in meetingsFromDb)
                    {
                        Meetings.Add(meeting);
                    }
                });
            }
            finally { IsBusy = false; };
        }

        [RelayCommand]
        public async Task ToggleRecordingAsync()
        {
            if (IsRecording)
            {
                // STOP RECORDING
                IsRecording = false;
                IsBusy = true; // Show loading spinner while saving

                try
                {
                    var savedFilePath = await _audioService.StopRecordingAsync();

                    if (!string.IsNullOrEmpty(savedFilePath))
                    {
                        // 1. Create and save the initial record immediately
                        var newMeeting = new Meeting
                        {
                            Title = $"Meeting {System.DateTime.Now:MMM dd, hh:mm tt}",
                            StartTime = System.DateTime.Now,
                            AudioFilePath = savedFilePath,
                            Transcript = "Transcribing audio... Please wait.",
                            AiSummary = "Waiting for transcription..."
                        };

                        await _databaseService.SaveMeetingAsync(newMeeting);
                        await LoadMeetingsAsync(); // Refresh UI to show the new meeting 

                        // 2. Call OpenAI Whisper API
                        string transcript = await _aiService.TranscribeAudioAsync(savedFilePath);

                        // 3. Update the database record with the new text
                        newMeeting.Transcript = transcript;
                        newMeeting.AiSummary = "Generating AI Summary...";

                        await _databaseService.SaveMeetingAsync(newMeeting);
                        await LoadMeetingsAsync(); // Refresh UI to show the update

                        // 4. Call GPT-4o-mini for Summary and Action Items
                        var aiResult = await _aiService.GenerateSummaryAsync(transcript);

                        // 5. Final Database Update
                        newMeeting.AiSummary = aiResult.Summary;
                        newMeeting.ActionItems = aiResult.ActionItems;

                        await _databaseService.SaveMeetingAsync(newMeeting);
                        await LoadMeetingsAsync(); // Refresh UI to show the update
                    }
                } catch (Exception ex)
                {
                    // Force the alert to show on the Main Thread
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // If the API key is missing or the network drops, show the error in the UI
                        if (Application.Current?.Windows is { Count: > 0 } windows && windows[0].Page is Page page)
                        {
                            await page.DisplayAlertAsync("Error", $"Processing failed: {ex.Message}", "OK");
                        }
                    });
                } finally
                {
                    IsBusy = false; // Hide loading spinner no matter what happens
                }
            } else
            {
                // START RECORDING
                await _audioService.StartRecordingAsync();
                IsRecording = true;
            }
        }

        [RelayCommand]
        public async Task GoToDetailsAsync(Meeting selectedMeeting)
        {
            if (selectedMeeting == null)
                return;

            // Navigate to the detail page and pass the selected meeting data
            var navigationParameter = new Dictionary<string, object>
            {
                { "Meeting", selectedMeeting }
            };
            
            await Shell.Current.GoToAsync(nameof(MeetingDetailPage), navigationParameter);
        }
    }
}