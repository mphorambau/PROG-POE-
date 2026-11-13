using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

internal static class AppTheme
{
    public static readonly Color Background = Color.FromArgb(10, 20, 36);
    public static readonly Color Surface = Color.FromArgb(16, 31, 50);
    public static readonly Color Panel = Color.FromArgb(18, 39, 62);
    public static readonly Color Header = Color.FromArgb(18, 43, 69);
    public static readonly Color Accent = Color.FromArgb(91, 224, 210);
    public static readonly Color AccentDark = Color.FromArgb(27, 126, 132);
    public static readonly Color Text = Color.FromArgb(232, 240, 248);
    public static readonly Color MutedText = Color.FromArgb(178, 198, 216);
    public static readonly Font BodyFont = new("Segoe UI", 10);
    public static readonly DataGridViewCellStyle GridHeaderStyle = new()
    {
        BackColor = Header, ForeColor = Accent, Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
        Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0), SelectionBackColor = Header, SelectionForeColor = Accent
    };
    public static readonly DataGridViewCellStyle GridCellStyle = new()
    {
        BackColor = Surface, ForeColor = Text, SelectionBackColor = Color.FromArgb(29, 69, 88), SelectionForeColor = Text, Padding = new Padding(8, 0, 0, 0)
    };
    public static readonly DataGridViewCellStyle GridAlternateCellStyle = new()
    {
        BackColor = Color.FromArgb(19, 36, 57), ForeColor = Text, SelectionBackColor = Color.FromArgb(29, 69, 88), SelectionForeColor = Text, Padding = new Padding(8, 0, 0, 0)
    };

    public static Panel CreateHeader(string title, string subtitle)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Header, Padding = new Padding(24, 13, 18, 10) };
        panel.Controls.Add(new Label { Text = subtitle, Dock = DockStyle.Fill, ForeColor = MutedText, Font = new Font("Segoe UI", 9.5f), TextAlign = ContentAlignment.MiddleLeft });
        panel.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 38, ForeColor = Accent, Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });
        return panel;
    }

    public static Button CreateButton(string text, bool secondary = false)
    {
        var button = new Button
        {
            Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false,
            BackColor = secondary ? Color.FromArgb(35, 57, 79) : AccentDark,
            ForeColor = Text, Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Padding = new Padding(14, 7, 14, 7), Margin = new Padding(5, 4, 5, 4), Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = secondary ? Color.FromArgb(49, 75, 98) : Color.FromArgb(38, 158, 155);
        button.FlatAppearance.MouseDownBackColor = secondary ? Color.FromArgb(30, 49, 69) : Color.FromArgb(20, 103, 111);
        return button;
    }
}
