using Plugin.Maui.Audio;
using System.IO;
using System.Threading.Tasks;

namespace AiNotetakerApp.Services
{
    public class AudioService
    {
        private readonly IAudioManager _audioManager;
        private IAudioRecorder _audioRecorder;
        private string _currentFilePath;

        public AudioService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public async Task StartRecordingAsync()
        {
            // Request permission from the user at runtime
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                // Handle denied permission (e.g., show an alert)
                return;
            }

            _audioRecorder = _audioManager.CreateRecorder();
            await _audioRecorder.StartAsync();
        }

        public async Task<string> StopRecordingAsync()
        {
            if (_audioRecorder != null && _audioRecorder.IsRecording)
            {
                var recordedAudio = await _audioRecorder.StopAsync();

                // Save the file to the app's secure local storage
                _currentFilePath = Path.Combine(FileSystem.AppDataDirectory, $"Meeting_{System.DateTime.Now:yyyyMMdd_HHmmss}.wav");

                using var fileStream = File.Create(_currentFilePath);
                var audioStream = recordedAudio.GetAudioStream();
                await audioStream.CopyToAsync(fileStream);

                return _currentFilePath;
            }

            return null;
        }
    }
}