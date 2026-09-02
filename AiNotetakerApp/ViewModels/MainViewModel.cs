using AiNotetakerApp.Models;
using AiNotetakerApp.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AiNotetakerApp.Services;
using System.Linq;


namespace AiNotetakerApp.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly AudioService _audioService;
        private readonly AiService _aiService;
        private readonly CalendarService _calendarService;

        // ObservableCollection automatically updates the UI when items are added or removed
        [ObservableProperty]
        private ObservableCollection<Meeting> _meetings;

        [ObservableProperty]
        private ObservableCollection<Folder> _folders;

        [ObservableProperty]
        private Folder _selectedFolder;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRecording;

        // Constructor Injection: The app automatically provides the DatabaseService here
        public MainViewModel(DatabaseService databaseService, AudioService audioService, AiService aiService, CalendarService calendarService)
        {
            _databaseService = databaseService;
            _audioService = audioService;
            _aiService = aiService;
            _calendarService = calendarService;

            _meetings = new ObservableCollection<Meeting>();
            _folders = new ObservableCollection<Folder>();
        }

        partial void OnSelectedFolderChanged(Folder value)
        {
            LoadMeetingsCommand.Execute(null); // Refresh meetings when folder changes
        }

        // The [RelayCommand} attribute automatically turns this method into a command
        // that you button can bind to in the XAML (e.g., Command="{Binding LoadMeetingsCommand}")
        [RelayCommand]
        public async Task LoadFolderAsync()
        {
            var foldersFromDb = await _databaseService.GetFoldersAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Folders.Clear();

                // Add a default "All Meetings" folder at the front
                Folders.Add(new Folder { Id = 0, Name = "All Meetings" });

                foreach (var folder in foldersFromDb)
                {
                    Folders.Add(folder);
                }

                // Select "All Meetings" by default if nothing is selected
                if (SelectedFolder == null)
                {
                    SelectedFolder = Folders.First();
                }
            });
        }

        [RelayCommand]
        public async Task CreateFolderAsync()
        {
            // Prompt the user for a folder name directly on the screen
            string folderName = await Application.Current.MainPage.DisplayPromptAsync("New Folder", "Enter folder name:");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var newFolder = new Folder { Name = folderName.Trim() };
                await _databaseService.SaveFolderAsync(newFolder);
                await LoadFolderAsync(); // Refresh the folder list
            }
        }

        [RelayCommand]
        public async Task LoadMeetingsAsync()
        {
            var meetingsFromDb = await _databaseService.GetMeetingAsync();

            // Filter meetings based on the selected folder (unless "All Meetings" ID 0 is selected)
            if (SelectedFolder != null && SelectedFolder.Id != 0)
            {
                meetingsFromDb = meetingsFromDb.Where(m => m.FolderId == SelectedFolder.Id).ToList();
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Meetings.Clear();
                foreach (var meeting in meetingsFromDb)
                {
                    Meetings.Add(meeting);
                }
            });

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
                            AiSummary = "Waiting for transcription...",
                            FolderId = SelectedFolder?.Id ?? 0 // Assign to selected folder or default to 0
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

                        // --- NEW CALENDAR SYNC CODE ---
                        string eventId = await _calendarService.CreateMeetingEventAsync(newMeeting);
                        if (!string.IsNullOrEmpty(eventId))
                        {
                            newMeeting.CalendarEventId = eventId;
                        }

                        await _databaseService.SaveMeetingAsync(newMeeting);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoadMeetingsCommand.Execute(null); // Refresh UI to show the update
                        }); 
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

        [RelayCommand]
        public async Task GoToSettingsAsync()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}