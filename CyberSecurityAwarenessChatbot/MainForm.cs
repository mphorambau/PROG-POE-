using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

public sealed class MainForm : Form
{
    private readonly ChatbotEngine _engine = new();
    private readonly ActivityLog _log = new();
    private readonly RichTextBox _transcript = new();
    private readonly TextBox _input = new();
    private readonly TaskRepository? _tasks;
    private readonly NlpIntentRouter _intentRouter = new();
    private readonly Label _storageStatus = new();

    public MainForm()
    {
        Text = "Cybersecurity Awareness Assistant";
        MinimumSize = new Size(960, 680);
        Size = new Size(1120, 790);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = AppTheme.BodyFont;
        DoubleBuffered = true;

        var connection = Environment.GetEnvironmentVariable("CYBERBOT_MYSQL");
        if (!string.IsNullOrWhiteSpace(connection)) _tasks = new TaskRepository(connection);

        var header = AppTheme.CreateHeader("Cybersecurity Awareness Assistant", "Learn safer digital habits, one confident decision at a time.");
        _storageStatus.Text = _tasks is null ? "TASKS: SETUP NEEDED" : "TASKS: DATABASE CONNECTED";
        _storageStatus.Dock = DockStyle.Right;
        _storageStatus.Width = 200;
        _storageStatus.TextAlign = ContentAlignment.MiddleCenter;
        _storageStatus.ForeColor = _tasks is null ? Color.FromArgb(246, 192, 105) : AppTheme.Accent;
        _storageStatus.Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
        header.Controls.Add(_storageStatus);

        _transcript.Dock = DockStyle.Fill;
        _transcript.ReadOnly = true;
        _transcript.BackColor = AppTheme.Surface;
        _transcript.ForeColor = AppTheme.Text;
        _transcript.BorderStyle = BorderStyle.None;
        _transcript.Font = new Font("Segoe UI", 10.5f);
        _transcript.DetectUrls = true;
        _transcript.HideSelection = false;
        _transcript.ScrollBars = RichTextBoxScrollBars.Vertical;
        _transcript.Padding = new Padding(18);

        var conversationCard = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Color.FromArgb(32, 61, 82) };
        conversationCard.Controls.Add(_transcript);
        var conversationArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 10, 18, 10), BackColor = AppTheme.Background };
        conversationArea.Controls.Add(conversationCard);

        _input.Dock = DockStyle.Fill;
        _input.BackColor = Color.FromArgb(28, 48, 70);
        _input.ForeColor = Color.White;
        _input.BorderStyle = BorderStyle.FixedSingle;
        _input.Font = new Font("Segoe UI", 10.5f);
        _input.Margin = new Padding(4, 3, 8, 3);
        _input.PlaceholderText = "Ask about phishing, passwords, privacy, malware or suspicious links…";
        _input.KeyDown += async (_, e) =>
        {
            if (e.KeyCode != Keys.Enter || e.Shift) return;
            e.SuppressKeyPress = true;
            await SendAsync();
        };
        var send = AppTheme.CreateButton("Send message");
        send.Dock = DockStyle.Fill;
        send.Click += async (_, _) => await SendAsync();
        var composer = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 66, ColumnCount = 2, Padding = new Padding(18, 10, 18, 10), BackColor = AppTheme.Panel };
        composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        composer.Controls.Add(_input, 0, 0);
        composer.Controls.Add(send, 1, 0);

        var toolsLabel = new Label { Text = "EXPLORE TOOLS", AutoSize = true, ForeColor = AppTheme.MutedText, Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold), Margin = new Padding(6, 12, 12, 0) };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 68, Padding = new Padding(18, 9, 18, 8), BackColor = AppTheme.Panel, WrapContents = false, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight };
        actions.Controls.Add(toolsLabel);
        AddAction(actions, "My Tasks", OpenTasksAsync);
        AddAction(actions, "Cyber Quiz", OpenQuiz);
        AddAction(actions, "Activity Log", ShowActivity, secondary: true);
        AddAction(actions, "Play my greeting", PlayGreeting, secondary: true);
        foreach (var topic in new[] { "Privacy", "Malware", "Suspicious links", "Passwords", "Phishing" })
            AddAction(actions, topic, () => { Append("Assistant", _engine.Respond(topic)); _log.Add("Learning", $"Opened advice about {topic.ToLowerInvariant()}"); }, secondary: true);

        Controls.Add(conversationArea);
        Controls.Add(composer);
        Controls.Add(actions);
        Controls.Add(header);

        var displayName = AskForName();
        _engine.RememberName(displayName);
        Shown += (_, _) => PlayGreeting();
        Append("Assistant", $"Welcome, {displayName}! Ask a question, choose a topic, or use the tools below to manage tasks, take the quiz and review your activity.");
        _log.Add("Application", "Application started");
    }

    private string AskForName()
    {
        using var prompt = new NamePrompt();
        return prompt.ShowDialog() == DialogResult.OK && prompt.NameValue.Length >= 2 ? prompt.NameValue : "friend";
    }

    private void PlayGreeting()
    {
        if (VoicePlayer.TryPlayGreeting(out var error))
        {
            Append("Assistant", "Your recorded WAV greeting is playing.");
            _log.Add("Voice", "Played the author's recorded WAV greeting");
            return;
        }

        Append("Assistant", "The recorded greeting could not play: " + error);
        _log.Add("Voice", "Recorded WAV greeting could not be played");
        MessageBox.Show(this, error, "Recorded greeting unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static void AddAction(Control parent, string label, Action action, bool secondary = false)
    {
        var button = AppTheme.CreateButton(label, secondary);
        button.Click += (_, _) => action();
        parent.Controls.Add(button);
    }

    private static void AddAction(Control parent, string label, Func<Task> action, bool secondary = false)
    {
        var button = AppTheme.CreateButton(label, secondary);
        button.Click += async (_, _) => await action();
        parent.Controls.Add(button);
    }

    private void Append(string speaker, string message)
    {
        _transcript.SelectionStart = _transcript.TextLength;
        _transcript.SelectionColor = speaker == "You" ? Color.FromArgb(129, 192, 255) : AppTheme.Accent;
        _transcript.SelectionFont = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _transcript.AppendText($"{speaker}  ");
        _transcript.SelectionColor = AppTheme.MutedText;
        _transcript.SelectionFont = new Font("Segoe UI", 9);
        _transcript.AppendText($"{DateTime.Now:HH:mm}\n");
        _transcript.SelectionColor = AppTheme.Text;
        _transcript.SelectionFont = new Font("Segoe UI", 10.5f);
        _transcript.AppendText(message + Environment.NewLine + Environment.NewLine);
        _transcript.SelectionStart = _transcript.TextLength;
        _transcript.ScrollToCaret();
    }

    private async Task SendAsync()
    {
        var input = _input.Text.Trim();
        if (input.Length == 0) return;
        _input.Clear();
        Append("You", input);
        var intent = _intentRouter.Detect(input);
        _log.Add("Conversation", $"Asked about {DescribeIntent(intent)}");
        switch (intent)
        {
            case ChatIntent.TaskAdd:
                if (_tasks is null)
                {
                    Append("Assistant", "Task storage is not configured. Set CYBERBOT_MYSQL and create the database with database/schema.sql to save tasks.");
                    _log.Add("Tasks", "Could not create a task because database setup is incomplete");
                    break;
                }
                var title = input.Replace("add task", "", StringComparison.OrdinalIgnoreCase).Replace("create task", "", StringComparison.OrdinalIgnoreCase).Trim();
                if (title.Length == 0) title = "New cybersecurity task";
                try
                {
                    var id = await _tasks.AddAsync(new TaskItem { Title = title, Description = "Added through the chatbot" });
                    _log.Add("Tasks", $"Created task #{id}: {title}");
                    Append("Assistant", $"Task #{id} was added: {title}. Open My Tasks to add a reminder or mark it complete.");
                }
                catch (Exception ex)
                {
                    _log.Add("Tasks", "Task creation failed because the database was unavailable");
                    Append("Assistant", "I couldn't save that task: " + ex.Message);
                }
                break;
            case ChatIntent.TaskList: await OpenTasksAsync(); break;
            case ChatIntent.TaskDelete: Append("Assistant", "Open My Tasks, select the task, then choose Delete. This protects you from deleting the wrong item."); break;
            case ChatIntent.TaskComplete: Append("Assistant", "Open My Tasks, select the task, then choose Mark Complete."); break;
            case ChatIntent.Quiz: OpenQuiz(); break;
            case ChatIntent.Activity: ShowActivity(); break;
            case ChatIntent.Reminder: Append("Assistant", "You can add a date and time reminder from My Tasks. The task list will clearly show its reminder status."); break;
            default: Append("Assistant", _engine.Respond(input)); break;
        }
    }

    private void OpenQuiz()
    {
        _log.Add("Quiz", "Opened the cybersecurity quiz");
        using var quiz = new QuizForm(_log);
        quiz.ShowDialog(this);
    }

    private async Task OpenTasksAsync()
    {
        if (_tasks is null)
        {
            _log.Add("Tasks", "Opened task setup guidance");
            MessageBox.Show(this,
                "The Tasks feature needs a MySQL connection.\n\n1. Run database/schema.sql in MySQL Workbench.\n2. Set CYBERBOT_MYSQL to your connection string.\n3. Restart the application and choose My Tasks again.",
                "MySQL setup required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            _log.Add("Tasks", "Opened task manager");
            using var form = new TaskManagerForm(_tasks, _log);
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _log.Add("Tasks", "Task manager could not open because the database was unavailable");
            MessageBox.Show(this, "The Task Manager could not open. Check that MySQL is running and that the database schema has been created.\n\nDetails: " + ex.Message,
                "Task Manager error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        await Task.CompletedTask;
    }

    private void ShowActivity()
    {
        _log.Add("Activity", "Viewed the activity log");
        using var activity = new ActivityLogForm(_log);
        activity.ShowDialog(this);
    }

    private static string DescribeIntent(ChatIntent intent) => intent switch
    {
        ChatIntent.TaskAdd => "creating a task", ChatIntent.TaskList => "their task list", ChatIntent.TaskDelete => "deleting a task",
        ChatIntent.TaskComplete => "completing a task", ChatIntent.Quiz => "the quiz", ChatIntent.Activity => "activity history",
        ChatIntent.Reminder => "task reminders", ChatIntent.Cybersecurity => "cybersecurity advice", _ => "cybersecurity guidance"
    };
}
