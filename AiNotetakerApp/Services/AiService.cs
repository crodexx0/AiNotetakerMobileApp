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
                ResponseFormat = AudioTranscriptionFormat.Text,

                // This acts as a "hint" to the AI. Include common English, Tagalog, and Cebuano words.
                // It tells Whisper not to panic if the language suddenly switches.
                Prompt = "This is a meeting in the Philippines. It contains a mix of English, Tagalog, and Cebuano or Bisaya. Okay ra ba? Sige, let's start our discussion po. Salamat."
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
            string systemPrompt = @"You are a professional meeting assistant. 
                                    The provided transcript is from a meeting in the Philippines and contains a mix of English, Tagalog, and Cebuano (Bisaya). 

                                    Instructions:
                                    1. Mentally translate all Tagalog and Cebuano text into English.
                                    2. Return your final output strictly in English.
                                    3. You MUST format your response exactly using these two headings:
                                    SUMMARY:
                                    (your summary here)
                                    ACTION ITEMS:
                                    (your bulleted list here)";

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage($"Transcript: {transcript}")
            };

            var response = await chatClient.CompleteChatAsync(messages);
            string fullAiText = response.Value.Content[0].Text;

            // Split the text based on the ACTION ITEMS: divider
            string[] separator = new[] { "ACTION ITEMS:" };
            var parts = fullAiText.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            string summary = parts.Length > 0 ? parts[0].Replace("SUMMARY:", "").Trim() : fullAiText;
            string actionItems = parts.Length > 1 ? parts[1].Trim() : "No clear action items found.";

            return (summary, actionItems);
        }
    }
}