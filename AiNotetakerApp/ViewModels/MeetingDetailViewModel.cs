using AiNotetakerApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using Plugin.Maui.Audio;

namespace AiNotetakerApp.ViewModels
{
    // The QueryProperty allows us to pass the Meeting object when navigating to this page
    [QueryProperty(nameof(CurrentMeeting), "Meeting")]
    public partial class MeetingDetailViewModel : ObservableObject
    {
        private readonly IAudioManager _audioManagaer;
        private IAudioPlayer _audioPlayer;

        [ObservableProperty]
        private Meeting _currentMeeting;

        [ObservableProperty]
        private bool _isPlaying;

        public MeetingDetailViewModel(IAudioManager audioManager)
        {
            _audioManagaer = audioManager;
        }

        [RelayCommand]
        public void TogglePlayback()
        {
            // Verify the file path exists before trying to play it
            if (string.IsNullOrEmpty(CurrentMeeting?.AudioFilePath) || !File.Exists(CurrentMeeting.AudioFilePath))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Audio file not found on device", "OK");
                return;
            }

            // Initialize the player the first time the user hits play
            if (_audioPlayer == null)
            {
                var fileStream = File.OpenRead(CurrentMeeting.AudioFilePath);
                _audioPlayer = _audioManagaer.CreatePlayer(fileStream);

                // Automatically update the UI when the audio finishes playing
                _audioPlayer.PlaybackEnded += (s, e) => IsPlaying = false;
            }

            if (_audioPlayer.IsPlaying)
            {
                _audioPlayer.Pause();
                IsPlaying = false;
            } else
            {
                _audioPlayer.Play();
                IsPlaying = true;
            }
        }

        public void DisposePlayer()
        {
            if (_audioPlayer != null)
            {
                if (_audioPlayer.IsPlaying)
                {
                    _audioPlayer.Stop();
                }
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }
            IsPlaying = false;
        }
    }
}