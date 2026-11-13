# Cybersecurity Awareness Chatbot — PROG POE

An original C# Windows Forms cybersecurity awareness assistant. This repository contains the application source, database setup script, and a Windows CI build workflow.

## Requirements
- Windows 10 or Windows 11
- .NET 10 SDK
- Visual Studio 2026 with the **.NET desktop development** workload, or VS Code with C# Dev Kit
- SQLite is included automatically for persistent task storage

## Runs with PART 1 AND 2 INTERGREATED 
https://github.com/mphorambau/CyberSecurityAwarenessChatbot.git

## Open and run
1. Clone this repository.
2. Open `CyberSecurityAwarenessChatbot.sln` in Visual Studio.
3. Restore NuGet packages.
4. Build the solution and run the `CyberSecurityAwarenessChatbot` project.

Chat, quiz, activity log, 
## Local SQLite task storage
On first use, the app creates its local database automatically at:

```text
CyberSecurityAwarenessChatbot\bin\Debug\net10.0-windows\Data\cyber_awareness.db
```

Use the SQLite extension you installed in Visual Studio to open that file if you want to inspect the saved tasks. No MySQL server, password or environment variable is needed.

## Features
- Personalised welcome and in-session name memory.
- Cybersecurity guidance for phishing, passwords, suspicious links, malware, privacy, and scams.
- Varied responses, sentiment-aware replies, and contextual follow-ups.
- Keyword and regular-expression intent routing.
- SQLite-backed task creation, listing, reminders, completion, deletion and due-reminder notifications.
- Twelve quiz questions with immediate explanations and final scoring.
- Timestamped activity history.
- The author's recorded WAV greeting only; no text-to-speech fallback is used.

## Repository layout
```text
PROG-POE-/
├── .github/workflows/build.yml
├── CyberSecurityAwarenessChatbot/
│   ├── CyberSecurityAwarenessChatbot.csproj
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── ChatbotEngine.cs
│   ├── NlpIntentRouter.cs
│   ├── VoicePlayer.cs
│   ├── QuizForm.cs
│   └── task and support classes
├── database/schema.sql
├── CyberSecurityAwarenessChatbot.sln
└── README.md
```

## Build verification
GitHub Actions is configured to restore and build the project on Windows for pushes and pull requests to `main`. Check the **Actions** tab for the actual run result. Source files being committed does not itself prove that the build passed.

## Presentation

