namespace CyberSecurityAwarenessChatbot;

public sealed record ChatMessage(string Speaker, string Message, DateTime Timestamp)
{
    public override string ToString() => $"[{Timestamp:HH:mm:ss}] {Speaker}: {Message}";
}