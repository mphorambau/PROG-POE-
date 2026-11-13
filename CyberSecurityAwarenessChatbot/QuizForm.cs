using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

public sealed class QuizForm : Form
{
    private sealed record Question(string Text,string[] Options,int Answer,string Explanation);
    private readonly Question[] _questions=
    [
        new("What is phishing?",["A fake message designed to steal information","A type of firewall","A secure backup"],0,"Phishing impersonates trusted sources to trick people into revealing information or clicking harmful links."),
        new("Which password is strongest?",["Summer2026","P@ssword1","A unique long passphrase"],2,"Long, unique passwords are harder to guess; use a password manager."),
        new("What should you do with an unexpected attachment?",["Open it immediately","Verify the sender first","Forward it to friends"],1,"Unexpected attachments can contain malware. Verify through a trusted channel."),
        new("What does MFA add?",["A second verification factor","More spam","A public profile"],0,"Multi-factor authentication adds another layer beyond a password."),
        new("True or False: Reusing one password everywhere is safe.",["True","False"],1,"A breach on one service can expose every account using that password."),
        new("What is ransomware?",["Software that encrypts/locks data for payment","A browser update","A password manager"],0,"Ransomware blocks access to data and demands payment."),
        new("A website uses HTTPS. Does that guarantee it is legitimate?",["Yes","No"],1,"HTTPS encrypts the connection but does not prove the site itself is trustworthy."),
        new("What is a safe response to a suspicious link?",["Click to inspect","Enter your password","Avoid it and verify independently"],2,"Do not use unexpected links; visit the official site directly."),
        new("True or False: Software updates can fix security vulnerabilities.",["True","False"],0,"Updates often patch weaknesses attackers could exploit."),
        new("Which is safer on public Wi-Fi?",["Access banking on any open hotspot","Use trusted networks and a VPN where appropriate","Disable device security"],1,"Avoid sensitive activity on untrusted networks and keep protections enabled."),
        new("What should you do after suspecting an account compromise?",["Ignore it","Change credentials from a trusted device and report it","Share the password"],1,"Secure the account, enable MFA, review sessions, and report the incident."),
        new("What is social engineering?",["Manipulating people to reveal information or take unsafe actions","A hardware upgrade","A type of encryption"],0,"Social engineering exploits trust, urgency, fear, or helpfulness.")
    ];
    private int _index,_score;
    private readonly ActivityLog _log;
    private readonly Label _question=new(),_feedback=new(),_progress=new();
    private readonly List<RadioButton> _answers=[];
    private readonly Button _next=new(){Text="Submit Answer",AutoSize=true};
    public QuizForm(ActivityLog log)
    {
        _log=log;Text="Cybersecurity Knowledge Quiz";Size=new Size(760,520);StartPosition=FormStartPosition.CenterParent;
        BackColor=Color.FromArgb(12,24,43);ForeColor=Color.White;
        _progress.SetBounds(25,20,690,30);_question.SetBounds(25,65,690,65);_question.Font=new Font("Segoe UI",14,FontStyle.Bold);
        Controls.AddRange([_progress,_question]);
        for(int i=0;i<3;i++){var rb=new RadioButton{Left=35,Top=145+i*42,Width=670,ForeColor=Color.White,Font=new Font("Segoe UI",11)};_answers.Add(rb);Controls.Add(rb);}
        _feedback.SetBounds(25,285,690,100);_feedback.ForeColor=Color.FromArgb(90,220,190);Controls.Add(_feedback);
        _next.SetBounds(25,405,170,40);_next.Click+=Submit;Controls.Add(_next);
        _log.Add("Cybersecurity quiz started");ShowQuestion();
    }
    private void ShowQuestion(){var q=_questions[_index];_progress.Text=$"Question {_index+1} of {_questions.Length}  |  Score: {_score}";_question.Text=q.Text;
        for(int i=0;i<3;i++){_answers[i].Text=i<q.Options.Length?q.Options[i]:"";_answers[i].Visible=i<q.Options.Length;_answers[i].Checked=false;_answers[i].Enabled=true;}
        _feedback.Text="Choose an answer, then submit.";_next.Text="Submit Answer";}
    private void Submit(object? sender,EventArgs e)
    {
        if(_next.Text=="Next Question"){_index++;if(_index>=_questions.Length){Finish();return;}ShowQuestion();return;}
        var selected=_answers.FindIndex(a=>a.Checked);if(selected<0){MessageBox.Show(this,"Please select an answer.");return;}
        var q=_questions[_index];var correct=selected==q.Answer;if(correct)_score++;
        _feedback.Text=(correct?"Correct! ":"Not quite. ")+q.Explanation;foreach(var a in _answers)a.Enabled=false;
        _progress.Text=$"Question {_index+1} of {_questions.Length}  |  Score: {_score}";_next.Text=_index==_questions.Length-1?"See Results":"Next Question";
    }
    private void Finish()
    {
        _log.Add($"Cybersecurity quiz completed: {_score}/{_questions.Length}");
        var percent=(int)Math.Round(100.0*_score/_questions.Length);
        MessageBox.Show(this,$"You scored {_score} out of {_questions.Length} ({percent}%).\n"+(percent>=80?"Excellent work! Keep practising safe habits.":percent>=50?"Good effort! Review the explanations and try again.":"Keep learning—small security habits make a big difference."),"Quiz Complete");
        Close();
    }
}