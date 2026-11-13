using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Part 2 welcome dialog used to personalise the chatbot conversation.</summary>
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
        ClientSize = new Size(410, 190);
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(18, 42, 66);
        ForeColor = Color.White;
        Controls.Add(new Label { AutoSize = true, Location = new Point(24, 22), Text = "What should Cyber Bot call you?", ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
        Controls.Add(new Label { AutoSize = true, Location = new Point(24, 55), Text = "Your name helps personalise the advice.", ForeColor = Color.FromArgb(184, 216, 235) });
        _name.Location = new Point(24, 88);
        _name.Size = new Size(360, 26);
        _name.Font = new Font("Segoe UI", 10F);
        Controls.Add(_name);
        _validation.AutoSize = true;
        _validation.Location = new Point(24, 145);
        _validation.ForeColor = Color.FromArgb(255, 184, 108);
        Controls.Add(_validation);
        var ok = new Button { Text = "Continue", Location = new Point(279, 137), Size = new Size(105, 32), BackColor = Color.FromArgb(85, 224, 190), FlatStyle = FlatStyle.Flat };
        ok.FlatAppearance.BorderSize = 0;
        ok.Click += (_, _) => { if (NameValue.Length < 2) { _validation.Text = "Please enter at least two characters."; return; } DialogResult = DialogResult.OK; };
        Controls.Add(ok);
        AcceptButton = ok;
    }
}
