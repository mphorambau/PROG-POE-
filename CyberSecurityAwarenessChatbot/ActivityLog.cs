namespace CyberSecurityAwarenessChatbot;

public sealed record ActivityEntry(DateTime Timestamp, string Category, string Summary);

public sealed class ActivityLog
{
    private const int MaximumEntries = 100;
    private readonly List<ActivityEntry> _entries = [];
    private readonly object _sync = new();

    public event EventHandler? Changed;

    public void Add(string category, string summary)
    {
        if (string.IsNullOrWhiteSpace(summary)) return;

        lock (_sync)
        {
            _entries.Add(new ActivityEntry(
                DateTime.Now,
                string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                summary.Trim()));
            if (_entries.Count > MaximumEntries) _entries.RemoveRange(0, _entries.Count - MaximumEntries);
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Add(string summary) => Add("General", summary);

    public IReadOnlyList<ActivityEntry> GetRecent(int skip, int take)
    {
        lock (_sync)
            return _entries.AsEnumerable().Reverse().Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToArray();
    }

    public int Count
    {
        get { lock (_sync) return _entries.Count; }
    }
}
