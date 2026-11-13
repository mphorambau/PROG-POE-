using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Browsable, timestamped presentation of the last 100 user activities.</summary>
public sealed class ActivityLogForm : Form
{
    private const int PageSize = 10;
    private readonly ActivityLog _log;
    private readonly DataGridView _grid = new();
    private readonly Label _summary = new();
    private readonly Button _showMore;
    private int _visibleEntries = PageSize;

    public ActivityLogForm(ActivityLog log)
    {
        _log = log;
        Text = "Activity Log";
        Size = new Size(900, 560);
        MinimumSize = new Size(720, 440);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = AppTheme.BodyFont;
        DoubleBuffered = true;

        var header = AppTheme.CreateHeader("Activity Log", "A clear record of your tasks, quiz progress and conversations.");
        var icon = new Label
        {
            Text = "ACTIVITY",
            Dock = DockStyle.Right,
            Width = 115,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = AppTheme.Accent,
            Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold)
        };
        header.Controls.Add(icon);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.BorderStyle = BorderStyle.None;
        _grid.BackgroundColor = AppTheme.Surface;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle = AppTheme.GridHeaderStyle;
        _grid.DefaultCellStyle = AppTheme.GridCellStyle;
        _grid.AlternatingRowsDefaultCellStyle = AppTheme.GridAlternateCellStyle;
        _grid.RowTemplate.Height = 38;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "WHEN", DataPropertyName = nameof(ActivityEntry.Timestamp), Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "ddd, dd MMM  HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "AREA", DataPropertyName = nameof(ActivityEntry.Category), Width = 135 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "WHAT HAPPENED", DataPropertyName = nameof(ActivityEntry.Summary), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        _summary.AutoSize = true;
        _summary.ForeColor = AppTheme.MutedText;
        _summary.Margin = new Padding(8, 10, 14, 0);
        _showMore = AppTheme.CreateButton("Show more", secondary: true);
        _showMore.Click += (_, _) => { _visibleEntries += PageSize; RefreshEntries(); };
        var close = AppTheme.CreateButton("Close", secondary: true);
        close.Click += (_, _) => Close();
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(16, 9, 16, 8), BackColor = AppTheme.Panel, FlowDirection = FlowDirection.RightToLeft };
        footer.Controls.Add(close);
        footer.Controls.Add(_showMore);
        footer.Controls.Add(_summary);

        Controls.Add(_grid);
        Controls.Add(footer);
        Controls.Add(header);
        _log.Changed += LogChanged;
        FormClosed += (_, _) => _log.Changed -= LogChanged;
        RefreshEntries();
    }

    private void LogChanged(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(RefreshEntries); else RefreshEntries();
    }

    private void RefreshEntries()
    {
        var total = _log.Count;
        var shown = Math.Min(_visibleEntries, total);
        _grid.DataSource = _log.GetRecent(0, _visibleEntries);
        _summary.Text = total == 0 ? "No activity has been recorded yet." : $"Showing {shown} of {total} most recent entries";
        _showMore.Visible = shown < total;
    }
}
