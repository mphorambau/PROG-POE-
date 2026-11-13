using System.Collections.Concurrent;

namespace CyberSecurityAwarenessChatbot;

public sealed class ActivityLog
{
    private readonly ConcurrentQueue<string> _entries = new();
    public void Add(string description)
    {
        _entries.Enqueue($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {description}");
        while (_entries.Count > 100 && _entries.TryDequeue(out _)) { }
    }
    public IReadOnlyList<string> Latest(int count = 10) =>
        _entries.ToArray().Reverse().Take(Math.Clamp(count, 1, 10)).ToArray();
}