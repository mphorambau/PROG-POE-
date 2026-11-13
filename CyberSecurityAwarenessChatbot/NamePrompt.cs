using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Short, friendly welcome screen that starts the personalised chat.</summary>
internal sealed class NamePrompt : Form
{
    private readonly TextBox _name = new();
    private readonly Label _validation = new();
    public string NameValue => _name.Text.Trim();

    public NamePrompt()
    {
        Text = "Welcome to Cyber Bot";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(500, 300);
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = AppTheme.BodyFont;

        var header = AppTheme.CreateHeader("Welcome to Cyber Bot", "Your student-built guide to safer online choices.");
        header.Height = 100;
        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(26, 22, 26, 12) };
        body.Controls.Add(new Label { Text = "HOW SHOULD I ADDRESS YOU?", Dock = DockStyle.Top, Height = 25, ForeColor = AppTheme.MutedText, Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) });
        _name.Dock = DockStyle.Top;
        _name.Height = 36;
        _name.MaxLength = 40;
        _name.BackColor = Color.FromArgb(28, 48, 70);
        _name.ForeColor = AppTheme.Text;
        _name.Font = new Font("Segoe UI", 11);
        _name.PlaceholderText = "Enter your first name";
        body.Controls.Add(_name);
        _validation.Dock = DockStyle.Top;
        _validation.Height = 28;
        _validation.Padding = new Padding(0, 7, 0, 0);
        _validation.ForeColor = Color.FromArgb(246, 192, 105);
        body.Controls.Add(_validation);

        var start = AppTheme.CreateButton("Start learning");
        start.Click += (_, _) =>
        {
            if (NameValue.Length < 2)
            {
                _validation.Text = "Please enter at least two characters.";
                _name.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
        };
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 62, Padding = new Padding(18, 10, 18, 8), BackColor = AppTheme.Panel, FlowDirection = FlowDirection.RightToLeft };
        footer.Controls.Add(start);
        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = start;
        Shown += (_, _) => _name.Focus();
    }
}
