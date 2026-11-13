using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

public sealed class TaskManagerForm : Form
{
    private readonly TaskRepository _repository;
    private readonly ActivityLog _log;
    private readonly DataGridView _grid=new();
    public TaskManagerForm(TaskRepository repository,ActivityLog log)
    {
        _repository=repository;_log=log;Text="Task Manager";Size=new Size(850,520);StartPosition=FormStartPosition.CenterParent;
        BackColor=Color.FromArgb(12,24,43);ForeColor=Color.White;
        _grid.Dock=DockStyle.Fill;_grid.ReadOnly=true;_grid.AutoGenerateColumns=false;_grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect=false;_grid.AllowUserToAddRows=false;_grid.BackgroundColor=Color.FromArgb(19,35,57);
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="ID",DataPropertyName=nameof(TaskItem.Id),Width=55});
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="Title",DataPropertyName=nameof(TaskItem.Title),Width=190});
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="Description",DataPropertyName=nameof(TaskItem.Description),Width=250});
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="Reminder",DataPropertyName=nameof(TaskItem.ReminderAt),Width=145});
        _grid.Columns.Add(new DataGridViewCheckBoxColumn{HeaderText="Complete",DataPropertyName=nameof(TaskItem.IsCompleted),Width=80});
        var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=55,Padding=new Padding(8)};
        Add(bar,"Add Task",AddTaskAsync);Add(bar,"Set Reminder",SetReminderAsync);Add(bar,"Mark Complete",CompleteAsync);Add(bar,"Delete",DeleteAsync);Add(bar,"Refresh",RefreshAsync);
        Controls.Add(_grid);Controls.Add(bar);Shown+=async(_,_)=>await RefreshAsync();
    }
    private void Add(Control parent,string title,Func<Task> action){var b=new Button{Text=title,AutoSize=true,BackColor=Color.Teal,ForeColor=Color.White};b.Click+=async(_,_)=>await action();parent.Controls.Add(b);}
    private TaskItem? Selected=>_grid.CurrentRow?.DataBoundItem as TaskItem;
    private async Task RefreshAsync(){try{_grid.DataSource=await _repository.GetAllAsync();}catch(Exception ex){MessageBox.Show(this,ex.Message,"Database error");}}
    private async Task AddTaskAsync()
    {
        using var f=new Form{Text="New Task",Size=new Size(420,300),StartPosition=FormStartPosition.CenterParent};
        var title=new TextBox{Left=15,Top=35,Width=370};var desc=new TextBox{Left=15,Top=95,Width=370,Height=90,Multiline=true};
        f.Controls.AddRange([new Label{Text="Title",Left=15,Top=12},title,new Label{Text="Description",Left=15,Top=72},desc]);
        var ok=new Button{Text="Save",Left=300,Top=205,DialogResult=DialogResult.OK};f.Controls.Add(ok);f.AcceptButton=ok;
        if(f.ShowDialog(this)==DialogResult.OK&&title.Text.Trim().Length>0){await _repository.AddAsync(new TaskItem{Title=title.Text.Trim(),Description=desc.Text.Trim()});_log.Add($"Task added: {title.Text.Trim()}");await RefreshAsync();}
    }
    private async Task SetReminderAsync()
    {
        var task=Selected;if(task is null)return;
        using var f=new Form{Text="Set Reminder",Size=new Size(330,150),StartPosition=FormStartPosition.CenterParent};
        var picker=new DateTimePicker{Left=15,Top=15,Width=280,Format=DateTimePickerFormat.Custom,CustomFormat="yyyy-MM-dd HH:mm",ShowUpDown=true};
        var ok=new Button{Text="Save",Left=220,Top=60,DialogResult=DialogResult.OK};f.Controls.AddRange([picker,ok]);
        if(f.ShowDialog(this)==DialogResult.OK){task.ReminderAt=picker.Value;await UpdateReminderAsync(task);_log.Add($"Reminder set for task #{task.Id}");await RefreshAsync();}
    }
    private async Task UpdateReminderAsync(TaskItem task)
    {
        // Reuse the repository's parameterized update method.
        await _repository.SetReminderAsync(task.Id,task.ReminderAt);
    }
    private async Task CompleteAsync(){var t=Selected;if(t is null)return;await _repository.SetCompletedAsync(t.Id,true);_log.Add($"Task completed: #{t.Id}");await RefreshAsync();}
    private async Task DeleteAsync(){var t=Selected;if(t is null)return;if(MessageBox.Show(this,$"Delete '{t.Title}'?","Confirm",MessageBoxButtons.YesNo)==DialogResult.Yes){await _repository.DeleteAsync(t.Id);_log.Add($"Task deleted: #{t.Id}");await RefreshAsync();}}
}