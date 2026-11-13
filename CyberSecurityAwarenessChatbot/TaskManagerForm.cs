using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

public sealed class TaskManagerForm : Form
{
    private readonly TaskRepository _repository;
    private readonly ActivityLog _log;
    private readonly DataGridView _grid = new();
    private readonly Label _summary = new();

    public TaskManagerForm(TaskRepository repository, ActivityLog log)
    {
        _repository = repository;
        _log = log;
        Text = "My Cybersecurity Tasks";
        Size = new Size(970, 590);
        MinimumSize = new Size(770, 480);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = AppTheme.BodyFont;
        DoubleBuffered = true;

        var header = AppTheme.CreateHeader("My Tasks", "Plan security habits, set reminders and keep track of progress.");
        header.Controls.Add(new Label { Text = "LOCAL SQLITE", Dock = DockStyle.Right, Width = 165, TextAlign = ContentAlignment.MiddleCenter, ForeColor = AppTheme.Accent, Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) });
        ConfigureGrid();
        var gridArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 10, 18, 4), BackColor = AppTheme.Background };
        gridArea.Controls.Add(_grid);

        _summary.AutoSize = true;
        _summary.ForeColor = AppTheme.MutedText;
        _summary.Margin = new Padding(12, 11, 14, 0);
        var add = AppTheme.CreateButton("Add task"); add.Click += async (_, _) => await AddTaskAsync();
        var reminder = AppTheme.CreateButton("Set reminder", secondary: true); reminder.Click += async (_, _) => await SetReminderAsync();
        var complete = AppTheme.CreateButton("Mark complete", secondary: true); complete.Click += async (_, _) => await CompleteAsync();
        var delete = AppTheme.CreateButton("Delete", secondary: true); delete.Click += async (_, _) => await DeleteAsync();
        var refresh = AppTheme.CreateButton("Refresh", secondary: true); refresh.Click += async (_, _) => await RefreshAsync();
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(18, 8, 18, 8), BackColor = AppTheme.Panel, WrapContents = false, AutoScroll = true };
        commands.Controls.Add(_summary); commands.Controls.Add(add); commands.Controls.Add(reminder); commands.Controls.Add(complete); commands.Controls.Add(delete); commands.Controls.Add(refresh);

        Controls.Add(gridArea); Controls.Add(commands); Controls.Add(header);
        Shown += async (_, _) => await RefreshAsync();
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BorderStyle = BorderStyle.None;
        _grid.BackgroundColor = AppTheme.Surface;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle = AppTheme.GridHeaderStyle;
        _grid.DefaultCellStyle = AppTheme.GridCellStyle;
        _grid.AlternatingRowsDefaultCellStyle = AppTheme.GridAlternateCellStyle;
        _grid.RowTemplate.Height = 42;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TASK", DataPropertyName = nameof(TaskItem.Title), Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DESCRIPTION", DataPropertyName = nameof(TaskItem.Description), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "REMINDER", DataPropertyName = nameof(TaskItem.ReminderAt), Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "ddd, dd MMM  HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "STATUS", DataPropertyName = nameof(TaskItem.Status), Width = 115 });
    }

    private TaskItem? Selected => _grid.CurrentRow?.DataBoundItem as TaskItem;

    private async Task RefreshAsync()
    {
        try
        {
            var tasks = await _repository.GetAllAsync();
            _grid.DataSource = tasks;
            var active = tasks.Count(task => !task.IsCompleted);
            var due = tasks.Count(task => !task.IsCompleted && task.ReminderAt is not null && task.ReminderAt <= DateTime.Now);
            _summary.Text = due > 0 ? $"{active} active • {due} reminder{(due == 1 ? "" : "s")} due" : $"{active} active • {tasks.Count - active} completed";
        }
        catch (Exception ex)
        {
            _log.Add("Tasks", "Could not load tasks because the database was unavailable");
            MessageBox.Show(this, "Tasks could not be loaded. Check that the application can create its local SQLite data file.\n\nDetails: " + ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task AddTaskAsync()
    {
        using var editor = new TaskEditorForm();
        if (editor.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var id = await _repository.AddAsync(new TaskItem { Title = editor.TaskTitle, Description = editor.Description });
            _log.Add("Tasks", $"Created task #{id}: {editor.TaskTitle}");
            await RefreshAsync();
        }
        catch (Exception ex) { _log.Add("Tasks", "Task creation failed because the database was unavailable"); ShowTaskError("The task could not be saved.", ex); }
    }

    private async Task SetReminderAsync()
    {
        var task = Selected;
        if (task is null) { AskToSelectTask(); return; }
        using var dialog = new ReminderForm(task.ReminderAt);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            await _repository.SetReminderAsync(task.Id, dialog.ReminderAt);
            _log.Add("Tasks", $"Set reminder for task #{task.Id}: {task.Title}");
            await RefreshAsync();
        }
        catch (Exception ex) { _log.Add("Tasks", $"Could not set reminder for task #{task.Id}"); ShowTaskError("The reminder could not be saved.", ex); }
    }

    private async Task CompleteAsync()
    {
        var task = Selected;
        if (task is null) { AskToSelectTask(); return; }
        if (task.IsCompleted) { MessageBox.Show(this, "This task is already marked complete.", "Task complete", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        try
        {
            await _repository.SetCompletedAsync(task.Id, true);
            _log.Add("Tasks", $"Completed task #{task.Id}: {task.Title}");
            await RefreshAsync();
        }
        catch (Exception ex) { _log.Add("Tasks", $"Could not complete task #{task.Id}"); ShowTaskError("The task could not be updated.", ex); }
    }

    private async Task DeleteAsync()
    {
        var task = Selected;
        if (task is null) { AskToSelectTask(); return; }
        if (MessageBox.Show(this, $"Delete '{task.Title}'? This cannot be undone.", "Delete task", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            if (!await _repository.DeleteAsync(task.Id)) throw new InvalidOperationException("The task no longer exists.");
            _log.Add("Tasks", $"Deleted task #{task.Id}: {task.Title}");
            await RefreshAsync();
        }
        catch (Exception ex) { _log.Add("Tasks", $"Could not delete task #{task.Id}"); ShowTaskError("The task could not be deleted.", ex); }
    }

    private void AskToSelectTask() => MessageBox.Show(this, "Select a task first.", "Choose a task", MessageBoxButtons.OK, MessageBoxIcon.Information);
    private void ShowTaskError(string message, Exception ex) => MessageBox.Show(this, message + "\n\nDetails: " + ex.Message, "Task Manager error", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private sealed class TaskEditorForm : Form
    {
        private readonly TextBox _title = new();
        private readonly TextBox _description = new();
        public string TaskTitle => _title.Text.Trim();
        public string Description => _description.Text.Trim();
        public TaskEditorForm()
        {
            Text = "Add task"; Size = new Size(500, 350); StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.Background; ForeColor = AppTheme.Text; Font = AppTheme.BodyFont;
            var header = AppTheme.CreateHeader("Add a task", "Keep the title short and add a helpful description.");
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22, 18, 22, 12) };
            var titleLabel = new Label { Text = "TASK TITLE", Dock = DockStyle.Top, Height = 24, ForeColor = AppTheme.MutedText, Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) };
            _title.Dock = DockStyle.Top; _title.Height = 34; _title.MaxLength = 160; _title.BackColor = Color.FromArgb(28, 48, 70); _title.ForeColor = AppTheme.Text;
            var spacer = new Panel { Dock = DockStyle.Top, Height = 16 };
            var descriptionLabel = new Label { Text = "DESCRIPTION", Dock = DockStyle.Top, Height = 24, ForeColor = AppTheme.MutedText, Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) };
            _description.Dock = DockStyle.Fill; _description.Multiline = true; _description.MaxLength = 2000; _description.BackColor = Color.FromArgb(28, 48, 70); _description.ForeColor = AppTheme.Text;
            body.Controls.Add(_description); body.Controls.Add(descriptionLabel); body.Controls.Add(spacer); body.Controls.Add(_title); body.Controls.Add(titleLabel);
            var save = AppTheme.CreateButton("Save task"); save.DialogResult = DialogResult.OK;
            save.Click += (_, _) => { if (TaskTitle.Length == 0) { MessageBox.Show(this, "Enter a task title.", "Task title needed", MessageBoxButtons.OK, MessageBoxIcon.Information); DialogResult = DialogResult.None; } };
            var cancel = AppTheme.CreateButton("Cancel", secondary: true); cancel.DialogResult = DialogResult.Cancel;
            var footer = Footer(save, cancel); Controls.Add(body); Controls.Add(footer); Controls.Add(header); AcceptButton = save; CancelButton = cancel;
        }
    }

    private sealed class ReminderForm : Form
    {
        private readonly DateTimePicker _picker = new();
        public DateTime ReminderAt => _picker.Value;
        public ReminderForm(DateTime? current)
        {
            Text = "Set reminder"; Size = new Size(430, 245); StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.Background; ForeColor = AppTheme.Text; Font = AppTheme.BodyFont;
            var header = AppTheme.CreateHeader("Set a reminder", "Choose when this task should be due.");
            _picker.SetBounds(24, 115, 360, 34); _picker.Format = DateTimePickerFormat.Custom; _picker.CustomFormat = "dddd, dd MMMM yyyy  HH:mm"; _picker.ShowUpDown = true; _picker.Value = current ?? DateTime.Now.AddHours(1);
            var save = AppTheme.CreateButton("Save reminder"); save.DialogResult = DialogResult.OK;
            var cancel = AppTheme.CreateButton("Cancel", secondary: true); cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(_picker); Controls.Add(Footer(save, cancel)); Controls.Add(header); AcceptButton = save; CancelButton = cancel;
        }
    }

    private static FlowLayoutPanel Footer(Button primary, Button secondary)
    {
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(16, 9, 16, 8), BackColor = AppTheme.Panel, FlowDirection = FlowDirection.RightToLeft };
        footer.Controls.Add(primary); footer.Controls.Add(secondary); return footer;
    }
}
