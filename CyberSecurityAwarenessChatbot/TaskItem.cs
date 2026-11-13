namespace CyberSecurityAwarenessChatbot;

public sealed class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime? ReminderAt { get; set; }
    public bool IsCompleted { get; set; }
    public string Status => IsCompleted ? "Complete" : ReminderAt is not null && ReminderAt <= DateTime.Now ? "Reminder due" : "Active";
}
