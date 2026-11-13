using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

public sealed class QuizForm : Form
{
    private sealed record Question(string Text, string[] Options, int Answer, string Explanation);
    private readonly Question[] _questions =
    [
        new("What is phishing?", ["A fake message designed to steal information", "A type of firewall", "A secure backup"], 0, "Phishing impersonates trusted sources to trick people into revealing information or clicking harmful links."),
        new("Which password is strongest?", ["Summer2026", "P@ssword1", "A unique long passphrase"], 2, "Long, unique passwords are harder to guess; use a password manager."),
        new("What should you do with an unexpected attachment?", ["Open it immediately", "Verify the sender first", "Forward it to friends"], 1, "Unexpected attachments can contain malware. Verify through a trusted channel."),
        new("What does MFA add?", ["A second verification factor", "More spam", "A public profile"], 0, "Multi-factor authentication adds another layer beyond a password."),
        new("True or False: Reusing one password everywhere is safe.", ["True", "False"], 1, "A breach on one service can expose every account using that password."),
        new("What is ransomware?", ["Software that encrypts or locks data for payment", "A browser update", "A password manager"], 0, "Ransomware blocks access to data and demands payment."),
        new("A website uses HTTPS. Does that guarantee it is legitimate?", ["Yes", "No"], 1, "HTTPS encrypts the connection but does not prove the site itself is trustworthy."),
        new("What is a safe response to a suspicious link?", ["Click to inspect", "Enter your password", "Avoid it and verify independently"], 2, "Do not use unexpected links; visit the official site directly."),
        new("True or False: Software updates can fix security vulnerabilities.", ["True", "False"], 0, "Updates often patch weaknesses attackers could exploit."),
        new("Which is safer on public Wi-Fi?", ["Access banking on any open hotspot", "Use trusted networks and a VPN where appropriate", "Disable device security"], 1, "Avoid sensitive activity on untrusted networks and keep protections enabled."),
        new("What should you do after suspecting an account compromise?", ["Ignore it", "Change credentials from a trusted device and report it", "Share the password"], 1, "Secure the account, enable MFA, review sessions, and report the incident."),
        new("What is social engineering?", ["Manipulating people to reveal information or take unsafe actions", "A hardware upgrade", "A type of encryption"], 0, "Social engineering exploits trust, urgency, fear, or helpfulness.")
    ];

    private int _index;
    private int _score;
    private readonly ActivityLog _log;
    private readonly Label _question = new();
    private readonly Label _feedback = new();
    private readonly Label _progress = new();
    private readonly List<RadioButton> _answers = [];
    private readonly Button _next = AppTheme.CreateButton("Check answer");

    public QuizForm(ActivityLog log)
    {
        _log = log;
        Text = "Cybersecurity Knowledge Quiz";
        Size = new Size(790, 570);
        MinimumSize = new Size(660, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = AppTheme.BodyFont;
        DoubleBuffered = true;

        var header = AppTheme.CreateHeader("Cybersecurity Challenge", "12 questions • learn from every answer • no trick questions");
        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 18, 28, 14), BackColor = AppTheme.Background };
        _progress.Dock = DockStyle.Top; _progress.Height = 26; _progress.ForeColor = AppTheme.Accent; _progress.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        _question.Dock = DockStyle.Top; _question.Height = 76; _question.Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold); _question.ForeColor = AppTheme.Text;
        var answersPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 145, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4, 4, 4, 4) };
        for (var i = 0; i < 3; i++)
        {
            var answer = new RadioButton { Width = 690, Height = 38, ForeColor = AppTheme.Text, Font = new Font("Segoe UI", 10.5f), Margin = new Padding(0, 2, 0, 4) };
            _answers.Add(answer); answersPanel.Controls.Add(answer);
        }
        _feedback.Dock = DockStyle.Fill; _feedback.Padding = new Padding(8, 12, 8, 4); _feedback.ForeColor = AppTheme.MutedText; _feedback.Font = new Font("Segoe UI", 10.5f);
        body.Controls.Add(_feedback); body.Controls.Add(answersPanel); body.Controls.Add(_question); body.Controls.Add(_progress);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(22, 9, 22, 8), BackColor = AppTheme.Panel, FlowDirection = FlowDirection.RightToLeft };
        _next.Click += Submit; footer.Controls.Add(_next);
        Controls.Add(body); Controls.Add(footer); Controls.Add(header);
        _log.Add("Quiz", "Started the 12-question cybersecurity quiz");
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        var question = _questions[_index];
        _progress.Text = $"QUESTION {_index + 1} OF {_questions.Length}     •     CURRENT SCORE: {_score}";
        _question.Text = question.Text;
        for (var i = 0; i < 3; i++)
        {
            _answers[i].Text = i < question.Options.Length ? question.Options[i] : string.Empty;
            _answers[i].Visible = i < question.Options.Length;
            _answers[i].Checked = false;
            _answers[i].Enabled = true;
        }
        _feedback.Text = "Choose the safest answer, then check it to see why.";
        _feedback.ForeColor = AppTheme.MutedText;
        _next.Text = "Check answer";
    }

    private void Submit(object? sender, EventArgs e)
    {
        if (_next.Text == "Next question" || _next.Text == "See results")
        {
            _index++;
            if (_index >= _questions.Length) { Finish(); return; }
            ShowQuestion();
            return;
        }
        var selected = _answers.FindIndex(answer => answer.Checked);
        if (selected < 0) { MessageBox.Show(this, "Choose an answer before continuing.", "Answer needed", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        var question = _questions[_index];
        var correct = selected == question.Answer;
        if (correct) _score++;
        _log.Add("Quiz", $"Question {_index + 1} answered {(correct ? "correctly" : "incorrectly")}");
        _feedback.Text = (correct ? "Correct. " : "Not quite. ") + question.Explanation;
        _feedback.ForeColor = correct ? AppTheme.Accent : Color.FromArgb(246, 192, 105);
        foreach (var answer in _answers) answer.Enabled = false;
        _next.Text = _index == _questions.Length - 1 ? "See results" : "Next question";
    }

    private void Finish()
    {
        _log.Add("Quiz", $"Completed quiz with a score of {_score}/{_questions.Length}");
        var percent = (int)Math.Round(100.0 * _score / _questions.Length);
        var feedback = percent >= 80 ? "Excellent work! Keep practising safe habits." : percent >= 50 ? "Good effort! Review the explanations and try again." : "Keep learning—small security habits make a big difference.";
        MessageBox.Show(this, $"You scored {_score} out of {_questions.Length} ({percent}%).\n\n{feedback}", "Quiz complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
