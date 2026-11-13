namespace CyberSecurityAwarenessChatbot;

/// <summary>
/// Part 2 conversational engine retained in Part 3: varied topic guidance,
/// user-name and interest memory, sentiment recognition, and follow-up handling.
/// </summary>
public sealed class ChatbotEngine
{
    private readonly Random _random = new();
    private readonly Dictionary<string, string[]> _responses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["phishing"] =
        [
            "Phishing messages try to rush you into sharing a password, OTP, or banking detail. Pause before clicking, check the complete sender address, and contact the organisation through its official website or phone number. Never share a password or one-time code.",
            "Treat an unexpected request for information as suspicious, even if the message uses a familiar logo. Look for misspelled domains, strange grammar, threats, or pressure to act immediately. Visit the organisation's official site yourself instead of using the message link.",
            "A convincing phishing message may pretend to be your bank, delivery company, lecturer, or a friend. Verify it using a separate method before responding. If you already clicked a suspicious link, change the affected password from the real website."
        ],
        ["password"] =
        [
            "Use a long, unique passphrase for every account. A password manager can generate and safely remember strong passwords. Reusing one password is risky because a leak on one site can expose all your accounts.",
            "Avoid names, birthdays, favourite teams, and common words in passwords. Turn on multi-factor authentication wherever it is available. Never approve a login notification you did not request.",
            "Aim for at least 12 characters and make every password unique. Do not share it with anyone, including someone claiming to be support. If a password was exposed, change it and sign out of other devices."
        ],
        ["suspicious link"] =
        [
            "Do not click unexpected links. Preview the full address carefully and, if it seems urgent, open the official website or app yourself instead of using the link.",
            "Shortened, misspelled, or look-alike web addresses can hide dangerous sites. Check the spelling of the domain. When uncertain, search for the organisation independently.",
            "A link is not automatically safe because it came from a friend: their account could have been compromised. Verify through a different conversation before opening links or attachments."
        ],
        ["malware"] =
        [
            "Malware is harmful software that can steal data, show unwanted adverts, spy on activity, or lock files for money. Keep Windows, browsers, and apps updated, and install software only from trusted sources.",
            "Avoid cracked software, unknown USB drives, and unexpected email attachments. Scan downloads before opening them and back up important work regularly.",
            "Warning signs include sudden pop-ups, a slow device, unknown programs, or files you cannot open. Ask a trusted person or IT support for help rather than trying random fixes."
        ],
        ["privacy"] =
        [
            "Protect privacy by sharing less personal information online and reviewing social-media privacy settings. Posts can reveal your location, routines, school, family, or other details.",
            "Choose the smallest audience setting that makes sense. Keep your address, school, phone number, ID details, and travel plans private. Turn off location sharing for apps that do not need it.",
            "Review app permissions regularly. Remove permissions that are not necessary and uninstall apps you no longer use. Use a screen lock and sign out on shared devices."
        ],
        ["scam"] =
        [
            "Scams often create urgency, promise a prize, or ask for payment or personal information. Slow down and verify before acting. Pressure is a warning sign.",
            "Never send money, passwords, PINs, banking details, or OTP codes to someone who contacted you unexpectedly. Talk to a trusted person before making a payment.",
            "If an offer sounds too good to be true, it probably is. Search independently for the company, check its real contact details, and look for trustworthy reviews."
        ]
    };

    private readonly Dictionary<string, string> _memory = new(StringComparer.OrdinalIgnoreCase);
    private string? _currentTopic;

    public void RememberName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name)) _memory["name"] = name.Trim();
    }

    public string Respond(string input)
    {
        var original = input.Trim();
        var text = NlpIntentRouter.Normalize(original);
        if (text.Length == 0) return "Please type a question and I'll do my best to help.";

        if (text.StartsWith("my name is ", StringComparison.Ordinal))
        {
            var name = original.Length > 11 ? original[11..].Trim(' ', '.', '!', '?') : "";
            if (name.Length >= 2) { _memory["name"] = name; return $"Nice to meet you, {name}. I'll remember your name during this chat."; }
        }

        if (text.Contains("interested in") || text.Contains("favourite topic") || text.Contains("favorite topic"))
        {
            var interest = FindTopic(text);
            if (interest is not null) _memory["interest"] = interest;
        }

        if (text is "bye" or "goodbye" or "exit" or "quit")
            return $"Goodbye, {_memory.GetValueOrDefault("name", "there")}. Think before you click, and stay safe online!";

        var sentiment = DetectSentiment(text);
        var topic = FindTopic(text);
        if (IsFollowUp(text) && _currentTopic is not null)
            return Compose(_currentTopic, true, sentiment);

        if (topic is not null)
        {
            _currentTopic = topic;
            return Compose(topic, false, sentiment);
        }

        if (sentiment is not null)
            return sentiment + " " + (_currentTopic is null
                ? "Take one safe step now: do not share passwords or OTP codes, and verify unexpected messages before responding."
                : _responses[_currentTopic][_random.Next(_responses[_currentTopic].Length)]);

        if (text.Contains("hello") || text == "hi")
            return _memory.TryGetValue("name", out var name)
                ? $"Hello again, {name}! What can I help you with?"
                : "Hello! I'm your Cybersecurity Awareness Assistant. What would you like to learn about?";

        return "I can help with phishing, passwords, suspicious links, malware, privacy and scams. You can also ask for another tip, start the quiz, manage tasks, or view the activity log.";
    }

    private string Compose(string topic, bool followUp, string? sentiment)
    {
        var response = _responses[topic][_random.Next(_responses[topic].Length)];
        var lead = followUp ? "Here is another tip: " : "";
        if (sentiment is not null) lead += sentiment + " ";
        if (_memory.TryGetValue("interest", out var interest) && interest == topic)
            lead += $"Since you told me you are interested in {topic}, ";
        if (_memory.TryGetValue("name", out var name) && _random.Next(3) == 0)
            lead = $"{name}, " + lead;
        return lead + response;
    }

    private string? FindTopic(string text)
    {
        if (text.Contains("phishing") || text.Contains("phish")) return "phishing";
        if (text.Contains("password") || text.Contains("passphrase")) return "password";
        if (text.Contains("link") || text.Contains("url") || text.Contains("suspicious website")) return "suspicious link";
        if (text.Contains("malware") || text.Contains("virus") || text.Contains("ransomware")) return "malware";
        if (text.Contains("privacy") || text.Contains("personal information") || text.Contains("social media")) return "privacy";
        if (text.Contains("scam") || text.Contains("fraud") || text.Contains("otp")) return "scam";
        return null;
    }

    private static bool IsFollowUp(string text) =>
        text.Contains("another tip") || text.Contains("tell me more") ||
        text.Contains("explain more") || text.Contains("more detail") ||
        text.Contains("more information") || text == "more";

    private static string? DetectSentiment(string text)
    {
        if (text.Contains("worried") || text.Contains("scared") || text.Contains("anxious"))
            return "It is understandable to feel worried. You are doing the right thing by checking first.";
        if (text.Contains("frustrated") || text.Contains("overwhelmed") || text.Contains("confused"))
            return "Cybersecurity can feel overwhelming, so let us keep it simple and take one step at a time.";
        if (text.Contains("curious") || text.Contains("interested"))
            return "Great question — curiosity is one of the best ways to build safer online habits.";
        return null;
    }
}