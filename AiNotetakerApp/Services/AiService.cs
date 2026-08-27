using OpenAI;
using OpenAI.Audio;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using OpenAI.Chat;

namespace AiNotetakerApp.Services
{
    public class AiService
    {
        // private readonly AudioClient _audioClient;

        private async Task<string> GetApiKeyAsync()
        {
            var key = await SecureStorage.Default.GetAsync("OpenAiKey");
            return string.IsNullOrEmpty(key) ? "ENTER_KEY_IN_APP_SETTINGS" : key;
        }

        //public AiService()
        //{
        //    string apiKey = "ENTER_KEY_IN_APP_SETTINGS";
        //    _audioClient = new AudioClient("whisper-1", apiKey);
        //}

        public async Task<string> TranscribeAudioAsync(string filePath)
        {
            string apiKey = await GetApiKeyAsync();
            var audioClient = new AudioClient("whisper-1", apiKey);

            // Configure options to return plain text
            AudioTranscriptionOptions options = new()
            {
                ResponseFormat = AudioTranscriptionFormat.Text
            };

            //Send the local audio file to Whisper
            AudioTranscription transcription = await audioClient.TranscribeAudioAsync(filePath, options);

            return transcription.Text;
        }

        public async Task<(string Summary, string ActionItems)> GenerateSummaryAsync(string transcript)
        {
            string apiKey = await GetApiKeyAsync();
            var chatClient = new ChatClient("gpt-4o-mini", apiKey);

            // Give AI a persona and strict instructions
            string systemPrompt = "You are a professional meeting assistant. Analyze the transcript. Return two sections separated by a '---' divider. Section 1: A brief 2-3 sentence summary. Section 2: A bulleted list of action items. Do not include introductory text.";

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage($"Transcript: {transcript}")
            };

            var response = await chatClient.CompleteChatAsync(messages);
            string fullAiText = response.Value.Content[0].Text;

            // Split the text based on the --- divider
            var parts = fullAiText.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

            string summary = parts.Length > 0 ? parts[0].Trim() : fullAiText;
            string actionItems = parts.Length > 1 ? parts[1].Trim() : "No clear action items found.";

            return (summary, actionItems);
        }
    }
}