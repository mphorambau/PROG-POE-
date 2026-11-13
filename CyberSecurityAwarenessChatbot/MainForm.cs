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

    public MainForm()
    {
        Text = "Cybersecurity Awareness Assistant";
        MinimumSize = new Size(900, 650);
        Size = new Size(1080, 780);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(10, 20, 36);
        ForeColor = Color.FromArgb(232, 240, 248);
        Font = new Font("Segoe UI", 10);
        DoubleBuffered = true;
        var connection = Environment.GetEnvironmentVariable("CYBERBOT_MYSQL");
        if (!string.IsNullOrWhiteSpace(connection)) _tasks = new TaskRepository(connection);

        var header = new Panel { Dock=DockStyle.Top, Height=86, BackColor=Color.FromArgb(18, 43, 69), Padding=new Padding(24, 12, 18, 10) };
        var heading = new Label { Text="Cybersecurity Awareness Assistant", Dock=DockStyle.Top, Height=36,
            Font=new Font("Segoe UI Semibold", 18, FontStyle.Bold), ForeColor=Color.FromArgb(91, 224, 210), TextAlign=ContentAlignment.MiddleLeft };
        var subheading = new Label { Text="Your friendly guide to safer digital habits", Dock=DockStyle.Fill,
            Font=new Font("Segoe UI", 9.5f), ForeColor=Color.FromArgb(178, 198, 216), TextAlign=ContentAlignment.MiddleLeft };
        header.Controls.Add(subheading); header.Controls.Add(heading);
        _transcript.Dock=DockStyle.Fill; _transcript.ReadOnly=true; _transcript.BackColor=Color.FromArgb(16, 31, 50);
        _transcript.ForeColor=Color.FromArgb(232, 240, 248); _transcript.BorderStyle=BorderStyle.None;
        _transcript.Font=new Font("Segoe UI", 10.5f); _transcript.DetectUrls=true;
        _transcript.HideSelection=false;
        _input.Dock=DockStyle.Fill; _input.BackColor=Color.FromArgb(28, 48, 70); _input.ForeColor=Color.White;
        _input.BorderStyle=BorderStyle.FixedSingle; _input.Font=new Font("Segoe UI", 10.5f);
        _input.Margin=new Padding(4, 3, 8, 3);
        var send=MakeButton("Send"); send.Click += async (_,_)=>await SendAsync();
        _input.KeyDown += async (_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;await SendAsync();}};
        var composer=new TableLayoutPanel { Dock=DockStyle.Bottom, Height=62, ColumnCount=2, Padding=new Padding(14, 9, 14, 9), BackColor=Color.FromArgb(18, 39, 62) };
        composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));
        composer.Controls.Add(_input,0,0); composer.Controls.Add(send,1,0);
        var actions=new FlowLayoutPanel { Dock=DockStyle.Bottom, Height=88, Padding=new Padding(12, 8, 12, 8),
            BackColor=Color.FromArgb(18, 39, 62), WrapContents=true, AutoScroll=true, FlowDirection=FlowDirection.LeftToRight };
        var tasksButton = MakeButton("Tasks");
        tasksButton.Click += async (_, _) => await OpenTasksAsync();
        actions.Controls.Add(tasksButton);
        var displayName = "friend";
        AddAction(actions,"Test Voice",()=>PlayGreeting(displayName));
        AddAction(actions,"Cyber Quiz",()=>{_log.Add("Quiz opened");new QuizForm(_log).ShowDialog(this);});
        AddAction(actions,"Activity Log",ShowActivity);
        foreach(var topic in new[]{"Privacy","Malware","Suspicious links","Passwords","Phishing"})
            AddAction(actions,topic,()=>Append("Assistant",_engine.Respond(topic)));
        Controls.Add(_transcript); Controls.Add(composer); Controls.Add(actions); Controls.Add(header);
        using (var prompt = new NamePrompt())
        {
            if (prompt.ShowDialog() == DialogResult.OK && prompt.NameValue.Length >= 2)
            {
                displayName = prompt.NameValue;
                _engine.RememberName(displayName);
            }
        }
        Shown += (_, _) => PlayGreeting(displayName);
        Append("Assistant", $"Welcome, {displayName}! Ask me about cybersecurity, or use Tasks, Test Voice, Cyber Quiz and Activity Log.");
        _log.Add("Application started");
    }

    private void PlayGreeting(string displayName)
    {
        var spokenText = $"Welcome, {displayName}! Welcome to the Cybersecurity Awareness Assistant. Ask me about cybersecurity and stay safe online.";
        if (VoicePlayer.TryPlayGreeting(spokenText, out var error))
        {
            Append("Assistant", "Voice playback was requested. If you cannot hear it, click Test Voice and check Windows sound settings.");
        }
        else
        {
            Append("Assistant", "Voice playback failed: " + error);
            MessageBox.Show(this, error, "Voice playback unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static Button MakeButton(string text)
    {
        var button = new Button { Text=text, AutoSize=true, FlatStyle=FlatStyle.Flat,
            BackColor=Color.FromArgb(27, 126, 132), ForeColor=Color.White,
            Font=new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Padding=new Padding(13, 7, 13, 7), Margin=new Padding(5, 4, 5, 4),
            Cursor=Cursors.Hand, UseVisualStyleBackColor=false };
        button.FlatAppearance.BorderSize=0;
        button.FlatAppearance.MouseOverBackColor=Color.FromArgb(38, 158, 155);
        button.FlatAppearance.MouseDownBackColor=Color.FromArgb(20, 103, 111);
        return button;
    }
    private static void AddAction(Control parent,string label,Action action){var b=MakeButton(label);b.Click+=(_,_)=>action();parent.Controls.Add(b);}
    private void Append(string speaker,string message){_transcript.AppendText($"[{DateTime.Now:HH:mm:ss}] {speaker}: {message}{Environment.NewLine}{Environment.NewLine}");}
    private async Task SendAsync()
    {
        var input=_input.Text.Trim(); if(input.Length==0)return; _input.Clear(); Append("You",input);
        var intent=_intentRouter.Detect(input); _log.Add($"NLP request detected: {intent}");
        switch(intent)
        {
            case ChatIntent.TaskAdd:
                if(_tasks is null){Append("Assistant","Task storage is not configured. Set the CYBERBOT_MYSQL environment variable and create the database using database/schema.sql.");break;}
                var title=input.Replace("add task","",StringComparison.OrdinalIgnoreCase).Replace("create task","",StringComparison.OrdinalIgnoreCase).Trim();
                if(title.Length==0)title="New cybersecurity task";
                try{var id=await _tasks.AddAsync(new TaskItem{Title=title,Description="Added through the chatbot"});_log.Add($"Task created: {title}");Append("Assistant",$"Task #{id} added: {title}. Open Tasks to add a description or reminder.");}
                catch(Exception ex){Append("Assistant","I couldn't save that task: "+ex.Message);}
                break;
            case ChatIntent.TaskList: await OpenTasksAsync(); break;
            case ChatIntent.TaskDelete: Append("Assistant","Open Tasks, select the task, and choose Delete. This prevents deleting the wrong item.");break;
            case ChatIntent.TaskComplete: Append("Assistant","Open Tasks, select the task, and choose Mark Complete.");break;
            case ChatIntent.Quiz: _log.Add("Quiz started from chat");new QuizForm(_log).ShowDialog(this);break;
            case ChatIntent.Activity: ShowActivity();break;
            case ChatIntent.Reminder: Append("Assistant","You can attach a date and time reminder to a task in the Tasks window.");break;
            default: Append("Assistant",_engine.Respond(input));break;
        }
    }
    private async Task OpenTasksAsync()
    {
        if (_tasks is null)
        {
            MessageBox.Show(this,
                "The Tasks feature needs a MySQL connection.\n\n1. Create the database by running database/schema.sql in MySQL Workbench.\n2. Set the CYBERBOT_MYSQL environment variable to your connection string, for example:\nServer=localhost;Port=3306;Database=cyber_awareness;User ID=root;Password=YOUR_PASSWORD;\n3. Restart the application and click Tasks again.",
                "MySQL setup required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using var form = new TaskManagerForm(_tasks, _log);
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "The Task Manager could not open. Check that MySQL is running and that the database schema has been created.\n\nDetails: " + ex.Message,
                "Task Manager error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        await Task.CompletedTask;
    }
    private void ShowActivity(){_log.Add("Activity log viewed");MessageBox.Show(this,string.Join(Environment.NewLine,_log.Latest(10)),"Recent Activity",MessageBoxButtons.OK,MessageBoxIcon.Information);}
}