using System.Media;
using System.Speech.Synthesis;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Plays the optional WAV greeting or falls back to an installed Windows speech voice.</summary>
internal static class VoicePlayer
{
    private static SoundPlayer? _player;
    private static SpeechSynthesizer? _speech;

    public static bool TryPlayGreeting(string? text, out string error)
    {
        error = string.Empty;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "greeting.wav");

        if (File.Exists(path))
        {
            try
            {
                _player?.Stop();
                _player?.Dispose();
                _player = new SoundPlayer(path);
                _player.Load();
                _player.Play();
                return true;
            }
            catch (Exception ex)
            {
                error = "The greeting WAV could not be played: " + ex.Message;
            }
        }

        try
        {
            _speech?.SpeakAsyncCancelAll();
            _speech?.Dispose();
            _speech = new SpeechSynthesizer();

            var installedVoices = _speech.GetInstalledVoices()
                .Where(voice => voice.Enabled)
                .ToList();

            if (installedVoices.Count == 0)
            {
                error = "Windows has no enabled text-to-speech voices. Install a voice in Settings > Time & language > Speech, then restart the chatbot.";
                return false;
            }

            _speech.SelectVoice(installedVoices[0].VoiceInfo.Name);
            _speech.Volume = 100;
            _speech.Rate = 0;
            _speech.SpeakAsync(string.IsNullOrWhiteSpace(text)
                ? "Welcome to the Cybersecurity Awareness Assistant. Stay safe online."
                : text);

            return true;
        }
        catch (Exception ex)
        {
            error = "Windows text-to-speech could not start: " + ex.Message;
            return false;
        }
    }
}
