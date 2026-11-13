using System.Media;

namespace CyberSecurityAwarenessChatbot;

/// <summary>
/// Plays only the greeting WAV supplied by the project author.
/// No computer-generated speech or other voice fallback is used.
/// </summary>
internal static class VoicePlayer
{
    private static SoundPlayer? _player;

    public static bool TryPlayGreeting(string? text, out string error)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "greeting.wav");

        if (!File.Exists(path))
        {
            error = $"The greeting recording was not found. Add your WAV file here: {path}";
            return false;
        }

        try
        {
            _player?.Stop();
            _player?.Dispose();
            _player = new SoundPlayer(path);
            _player.Load();
            _player.Play();
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = "The greeting WAV could not be played: " + ex.Message;
            return false;
        }
    }
}
