# TradeProof

AI-powered code compliance and training platform for electricians. Photograph your work for instant NEC code analysis, or train in VR with hands-on electrical tasks.

Built at [TreeHacks 2026](https://www.treehacks.com/) at Stanford University.

## Overview

TradeProof has two components:

- **Web App** — Upload photos of electrical work, get AI-powered compliance analysis against NEC codes and California amendments, track skill progression over time, and build a shareable credential portfolio.
- **VR App** — Immersive "Day in the Life of an Electrician" training on Meta Quest 2. Seven hands-on tasks from outlet installation to circuit troubleshooting, with an AI mentor, career progression, and NEC code validation.

## Web App

A Next.js application that uses Claude AI to analyze photos of electrical work against building codes.

### Features

- **Photo Analysis** — Upload a photo, select work type, get structured compliance results with exact NEC code citations, severity ratings, and fix instructions
- **Before/After Re-check** — Fix a violation, upload the corrected photo, see side-by-side comparison with resolved/unresolved status
- **Dashboard** — Portfolio tracking with compliance trend charts, skill progress bars, and analysis history
- **Knowledge Base** — Expert insight cards from experienced electricians
- **Credential Passport** — Cross-state license gap analysis (CA, TX, AZ) and shareable employer-facing credential page

### Tech Stack

- Next.js 16 (App Router) + TypeScript + Tailwind CSS
- Claude Sonnet 4.5 via `@anthropic-ai/sdk`
- shadcn/ui + Recharts + Lucide icons
- localStorage for data persistence

### Setup

```bash
npm install
```

Create `.env.local`:

```
ANTHROPIC_API_KEY=your_key_here
```

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000).

## VR App

A Unity project targeting Meta Quest 2 with 7 electrician training tasks, procedural environments, and career progression.

### Training Tasks

| Task | Difficulty | Steps | Description |
|------|-----------|-------|-------------|
| Panel Inspection | Beginner | 5 violations | Identify NEC code violations in an electrical panel |
| Circuit Wiring | Beginner | 6 steps | Wire a circuit from breaker to outlet with proper connections |
| Outlet Installation | Beginner | 9 steps | Strip romex, connect hot/neutral/ground, mount and cover |
| 3-Way Switch Wiring | Intermediate | 7 steps | Wire two 3-way switches to control a light fixture |
| GFCI Testing | Intermediate | 7 steps | Test, diagnose, and replace faulty GFCI outlets |
| EMT Conduit Bending | Intermediate | 8 steps | Measure, bend, and ream EMT conduit to spec |
| Circuit Troubleshooting | Advanced | 10 steps | Diagnose a dead outlet using multimeter and voltage tester |

### Features

- **Three Training Modes** — Learn (guided walkthrough), Practice (hints available), Test (timed, no help)
- **Day-in-the-Life Mode** — Start your day, accept work orders, drive to job sites, complete tasks, earn XP
- **AI Mentor** — Progressive hint system that adapts to your skill level
- **Career Progression** — Apprentice → Journeyman → Master with XP, badges, and certifications
- **17 NEC Codes** — Real electrical code validation (310.16, 408.4, 250.50, 210.8, etc.)
- **Procedural Environments** — Workshop hub, residential job sites, all built from code (no prefabs)
- **Circuit Simulator** — Graph-based energy propagation for realistic electrical behavior
- **Daily Challenges** — Date-seeded challenges with bonus XP rewards

### Tech Stack

- Unity 6000.0.67f1 (Unity 6 LTS)
- Meta XR SDK 68.0.3 (`com.meta.xr.sdk.core`)
- C# with all assets built procedurally via `GameObject.CreatePrimitive()`
- Target: Quest 2, Android API 29+, IL2CPP, ARM64

### Building

Open `tradeproof-vr/` in Unity Hub. Build for Android (Quest 2) via:

**Unity Menu** → TradeProof → Configure Quest 2 Settings, then TradeProof → Build Quest 2 APK

Or from command line:

```bash
/Applications/Unity/Hub/Editor/6000.0.67f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath tradeproof-vr \
  -executeMethod Quest2BuildScript.BuildAPK \
  -quit
```

## Project Structure

```
├── app/                    # Next.js pages (8 screens + 2 API routes)
├── components/             # React components (11 custom + shadcn/ui)
├── lib/                    # AI engine, NEC codes, storage
├── data/                   # Demo data and knowledge clips
├── public/                 # Static assets
└── tradeproof-vr/          # Unity VR project
    ├── Assets/
    │   ├── Scripts/
    │   │   ├── AI/         # Mentor, hints, adaptive difficulty, NPC
    │   │   ├── Core/       # GameManager, TaskManager, DayManager, etc.
    │   │   ├── Data/       # Task definitions, player progress, NEC codes
    │   │   ├── Electrical/ # Panel, breakers, outlets, switches, conduit, etc.
    │   │   ├── Environment/# Procedural rooms, hub workshop, workbench
    │   │   ├── Interaction/# Hand tracking, grab system, tools
    │   │   ├── Training/   # 7 training tasks + trackers
    │   │   └── UI/         # HUD, menus, career progress, mentor dialogue
    │   ├── Resources/
    │   │   ├── NECCodes/   # NEC code database (JSON)
    │   │   └── TaskDefinitions/ # Task step definitions (JSON)
    │   └── Scenes/
    └── Packages/
```

## Team

Built by Anish Cheraku and Ayush Kumar at TreeHacks 2026.

## License

MIT
