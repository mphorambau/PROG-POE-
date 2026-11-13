# Cybersecurity Awareness Chatbot — PROG POE

An original C# Windows Forms cybersecurity awareness assistant. This repository contains the application source, database setup script, and a Windows CI build workflow.

## Requirements
- Windows 10 or Windows 11
- .NET 10 SDK
- Visual Studio 2026 with the **.NET desktop development** workload, or VS Code with C# Dev Kit
- MySQL 8+ only for persistent task storage

## Open and run
1. Clone this repository.
2. Open `CyberSecurityAwarenessChatbot.sln` in Visual Studio.
3. Restore NuGet packages.
4. Build the solution and run the `CyberSecurityAwarenessChatbot` project.

Chat, quiz, activity log, and the author's recorded WAV greeting do not require MySQL.

## Optional MySQL task storage
1. Run [database/schema.sql](database/schema.sql) in MySQL Workbench or your MySQL client.
2. Set a Windows environment variable named `CYBERBOT_MYSQL`, for example:

   ```text
   Server=localhost;Port=3306;Database=cyber_awareness;User ID=root;Password=YOUR_PASSWORD;
   ```

3. Restart the application and open **Tasks**.

Never commit real passwords or secrets.

## Features
- Personalised welcome and in-session name memory.
- Cybersecurity guidance for phishing, passwords, suspicious links, malware, privacy, and scams.
- Varied responses, sentiment-aware replies, and contextual follow-ups.
- Keyword and regular-expression intent routing.
- MySQL-backed task creation, listing, reminders, completion, and deletion.
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
Add the unlisted YouTube voice-over demonstration link here after recording and uploading it.

## Originality
The supplied example repository was used only as a guide to repository organisation and presentation. Its source implementation was not copied. This application is based on the author's own earlier chatbot project and has been adjusted for this repository.
