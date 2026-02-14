# TradeProof — Project Status & Full Context

> Last updated: Feb 14, 2026. For new Claude Code sessions starting from scratch.

## What Is TradeProof

AI-powered code compliance tool for skilled trades (starting with electricians). A tradesperson photographs their work, AI analyzes it against jurisdiction-specific building codes (NEC + California amendments), identifies violations with exact code citations, tells them how to fix it, and verifies the fix. Every analysis builds a competency portfolio.

## Project Location

```
/Users/kumar/Documents/research/treehacks/tradeproof-app/
```

## Tech Stack

- **Framework:** Next.js 16 (App Router, Turbopack)
- **Language:** TypeScript (strict mode)
- **Styling:** Tailwind CSS + shadcn/ui
- **Charts:** recharts
- **Icons:** lucide-react
- **AI:** Claude Sonnet 4.5 via @anthropic-ai/sdk
- **Data:** localStorage (no database, no auth — hackathon demo)
- **IDs:** uuid
- **Path alias:** `@/*` maps to project root

## Current State: FULLY BUILT & TESTED

`npx next build` passes with zero errors. All 8 screens functional. AI analysis pipeline tested end-to-end. Dev server runs on `http://localhost:3000`.

## Environment

`.env.local` contains:
```
ANTHROPIC_API_KEY=sk-ant-api03-... (already set, DO NOT commit)
```
That's the only key needed. No Supabase, no OpenAI.

---

## File Structure (every source file)

### Pages (8 screens + 2 API routes)

| File | Route | Type | Description |
|------|-------|------|-------------|
| `app/page.tsx` | `/` | Client | Landing page. "Check My Work" CTA. Auto-loads demo data on first visit via `loadDemoDataFromSeed()`. |
| `app/analyze/page.tsx` | `/analyze` | Client | 4-step guided flow: select work type → describe work → take photo → submit. POSTs to `/api/analyze`, saves result to localStorage, redirects to `/results/[id]`. |
| `app/results/[id]/page.tsx` | `/results/:id` | Client (dynamic) | Analysis results. Shows compliance score, violation cards, correct items, knowledge clips. "I've Fixed It" triggers re-check: upload fixed photo → POST `/api/recheck` → show BeforeAfter component. |
| `app/dashboard/page.tsx` | `/dashboard` | Client | Portfolio. Stats row (count, avg score, trend), SkillRadar progress bars, ComplianceTrend line chart, recent analyses list linking to `/results/[id]`. |
| `app/knowledge/page.tsx` | `/knowledge` | Client | 5 expert insight cards with task-type filter tabs. |
| `app/passport/page.tsx` | `/passport` | Client | Cross-state gap analysis. Current state CA, target TX or AZ. Shows requirements breakdown with course suggestions, path comparison. |
| `app/credential/[userId]/page.tsx` | `/credential/:userId` | Client (dynamic) | Employer-facing credential. Resume vs TradeProof side-by-side, CredentialCard with stats, share link with copy button. |
| `app/layout.tsx` | (root) | Server | Root layout. Metadata "TradeProof". Includes BottomNav component. `pb-20` padding for fixed nav. |
| `app/api/analyze/route.ts` | `POST /api/analyze` | Server | Accepts `{ image, workType, userDescription, jurisdiction? }`. Builds prompt, calls Claude, returns structured JSON analysis with UUID. |
| `app/api/recheck/route.ts` | `POST /api/recheck` | Server | Accepts `{ originalImage, fixedImage, originalViolations, jurisdiction? }`. Compares before/after, returns violation status + new violations. |

### Components (11 custom + 7 shadcn/ui)

| File | Purpose | Key Props |
|------|---------|-----------|
| `components/CameraCapture.tsx` | Camera/upload with mobile capture + gallery fallback | `onCapture: (base64) => void`, `label?`, `existingImage?` |
| `components/WorkTypeSelector.tsx` | 2x4 grid of work types with photo tips | `selected: string`, `onSelect: (type) => void` |
| `components/AnalysisCard.tsx` | Single violation card with severity border, code badge, expandable "Why This Matters" | `violation: Violation` |
| `components/AnalysisResults.tsx` | Full results display: score circle, violations, correct items, re-check button | `analysis`, `onRecheck?`, `photoUrl?` |
| `components/BeforeAfter.tsx` | Side-by-side before/after with resolved/unresolved badges | `beforeImage`, `afterImage`, `originalViolations`, `recheckResult` |
| `components/SkillRadar.tsx` | Vertical progress bars per skill with trend arrows and check counts | `skills: SkillScore[]` |
| `components/ComplianceTrend.tsx` | recharts LineChart of compliance over time with reference line at 80 | `analyses: { date, score }[]` |
| `components/KnowledgeCard.tsx` | Expert insight card with avatar, quote styling | `clip: KnowledgeClip` |
| `components/GapAnalysis.tsx` | Cross-state gap analysis with course suggestions, path comparison | `currentState`, `targetState`, `gaps[]`, `overallMatch` |
| `components/CredentialCard.tsx` | Employer-facing credential with dark header, stats grid, skill badges | `profile`, `stats` |
| `components/BottomNav.tsx` | Fixed bottom nav: Check, Dashboard, Learn, Profile | (uses `usePathname()` for active state) |
| `components/ui/*` | shadcn/ui: button, card, input, badge, tabs, dialog, progress | Standard shadcn props |

### Lib (AI engine + data + storage)

| File | Purpose | Key Exports |
|------|---------|-------------|
| `lib/claude.ts` | Anthropic SDK wrapper. Handles base64 images, JSON extraction from markdown. | `analyzePhoto(base64, systemPrompt, description, workType)` → `AnalysisResult`, `recheckPhoto(original, fixed, violations, prompt)` → `RecheckResult` |
| `lib/prompts/analyze.ts` | Builds analysis system prompt with NEC codes + CA amendments injected | `buildAnalysisPrompt(jurisdiction, trade, workType, userDescription)` → string |
| `lib/prompts/recheck.ts` | Builds recheck prompt with original violations listed | `buildRecheckPrompt(jurisdiction, originalViolations, userDescription?)` → string |
| `lib/codes/nec.ts` | 22 NEC code sections with visual indicators, fix instructions, severity | `NEC_SECTIONS`, `getCodesForJurisdiction(jurisdiction, workType?)` → formatted string, `getRelevantCodes(workType)` → `NECSection[]` |
| `lib/codes/california.ts` | 5 CA amendments (seismic, solar, EV, energy, GFCI) | `CALIFORNIA_AMENDMENTS`, `getCaliforniaAmendments(workType?)` → formatted string |
| `lib/codes/state-requirements.ts` | Licensing requirements for CA, TX, AZ | `STATE_REQUIREMENTS`, `getGapAnalysis(userSkills, targetState)` → `{ gaps, overallMatch }` |
| `lib/storage.ts` | localStorage persistence layer. SSR-safe. | `getProfile()`, `saveProfile()`, `getAnalyses()`, `saveAnalysis()`, `getAnalysis(id)`, `updateAnalysis()`, `getSkillScores()`, `updateSkillScores()`, `loadDemoDataFromSeed()`, `isDemoLoaded()`, `clearAll()` |
| `lib/utils.ts` | shadcn utility (`cn()` for class merging) | `cn(...inputs)` |

### Data

| File | Purpose | Key Exports |
|------|---------|-------------|
| `data/demo-data.ts` | Alex Smith demo profile, 15 analyses (Sept 2025 → Feb 2026), 8 skill scores | `DEMO_PROFILE`, `DEMO_ANALYSES`, `DEMO_SKILL_SCORES` |
| `data/knowledge-clips.ts` | 5 expert insight cards with trigger keywords | `KNOWLEDGE_CLIPS`, `getRelevantClips(keywords[])` |

---

## Architecture Decisions

1. **No auth/Supabase** — localStorage only. Pre-loaded "Alex Smith" profile. Saves 4+ hours.
2. **No RAG** — NEC codes stored as TypeScript objects, injected directly into Claude prompt. Same result, 1/4 the setup time.
3. **Constrained citations** — Prompt tells Claude "cite ONLY from provided sections." Anything else → "potential concern — verify with inspector." Prevents hallucinated code references.
4. **Progress bars, not radar chart** — Honest visualization. Shows "(N checks)" sample size. Radar with 1-3 data points is misleading.
5. **California baked in** — No jurisdiction dropdown. We're at Stanford. "San Jose, CA" shown as a chip.
6. **Expert text cards, not video clips** — 5 written cards with realistic personas. Triggered by keyword match on analysis results.
7. **Before/after side-by-side** — Re-check shows original violations with RESOLVED/UNRESOLVED badges + any new violations.

## Key Type Interfaces

```typescript
// From lib/storage.ts
interface Analysis {
  id: string; userId: string; photoUrl: string;
  jurisdiction: string; trade: string; workType: string;
  userDescription: string; isCompliant: boolean;
  complianceScore: number; violations: Violation[];
  correctItems: string[]; skillsDemonstrated: SkillDemo[];
  overallAssessment: string;
  fixedPhotoUrl?: string; fixVerified?: boolean;
  fixComplianceScore?: number; fixAnalysis?: any;
  createdAt: string;
}

interface Violation {
  description: string; codeSection: string;
  localAmendment?: string;
  severity: 'critical' | 'major' | 'minor';
  confidence: 'high' | 'medium' | 'low';
  fixInstruction: string; whyThisMatters: string;
  visualEvidence?: string;
}

interface SkillScore {
  skillName: string; score: number;
  totalInstances: number;
  trend: 'improving' | 'stable' | 'declining';
  lastUpdated: string;
}
```

```typescript
// Claude API response (snake_case) — from lib/claude.ts
interface AnalysisResult {
  description: string; is_compliant: boolean;
  compliance_score: number;
  violations: { description, code_section, local_amendment,
    severity, confidence, fix_instruction,
    why_this_matters, visual_evidence }[];
  correct_items: string[];
  skills_demonstrated: { skill, quality }[];
  overall_assessment: string;
  work_type_detected: string;
}
```

Note: The API returns **snake_case** (Claude's response format). The frontend `analyze/page.tsx` maps these to **camelCase** `Analysis` type before saving to localStorage.

## Demo Data Story

Alex Smith, 1st-year electrical apprentice, San Jose CA. 15 analyses from Sept 2025 to Feb 2026 showing improvement:

- **Early (Sept-Oct):** Scores 45-58, multiple violations per analysis. Overfilled boxes, missing GFCI, exposed copper, unsupported cables.
- **Mid (Nov):** Scores 68-72, transitional. Fewer violations, less severe.
- **Late (Dec-Feb):** Scores 75-97, mostly compliant. Panel work still challenging (76%), conduit excellent (94%).

8 skill categories tracked: Wire Terminations (87%), Box Fill (91%), Grounding (89%), GFCI/AFCI (85%), Conduit (94%), Panel Work (76%), Cable Securing (88%), Mechanical Execution (82%).

## NEC Codes Included (22 sections)

110.12, 110.14(B), 110.26, 200.7, 210.8, 210.12, 250.24, 250.119, 300.4, 300.14, 310.10, 314.16, 314.17, 314.28, 334.30, 404.2, 406.4(D), 406.12, 408.4, 410.16, 422.16, 480.4

Each has: code, title, requirement text, visual indicators array, fix instructions, severity, applicable work types.

## California Amendments (5)

CA-SEISMIC-1 (seismic bracing), CA-SOLAR-T24 (solar readiness), CA-EV-READY (EV charging), CA-ENERGY-EFF (lighting controls), CA-GFCI-EXPAND (expanded GFCI)

## Knowledge Clips (5 experts)

1. Mike Torres (32yr) — Federal Pacific panels danger
2. Sarah Chen (28yr) — GFCI/AFCI common mistakes
3. James Washington (35yr) — Aluminum wiring in older homes
4. Maria Rodriguez (24yr) — Wire nut technique
5. Robert Kim (30yr) — Panel upgrades and load calculations

## E2E Test Results (Feb 14, 2026)

All verified via Playwright + curl:

| Test | Status | Notes |
|------|--------|-------|
| Landing page renders | PASS | Demo data auto-loads, CTA works |
| Analyze flow (4 steps) | PASS | Work type → description → photo → submit |
| POST /api/analyze | PASS | Claude returns structured JSON, ~8 sec response |
| Results page renders violations | PASS | Score circle, violation cards, knowledge clips |
| Dashboard shows portfolio | PASS | 16 analyses (15 demo + 1 live test), skill bars, trend chart |
| Knowledge page shows 5 clips | PASS | Filter tabs work |
| Passport gap analysis | PASS | CA→TX at 50% match, course suggestions |
| Credential page | PASS | Resume vs TradeProof comparison, share link |
| Analysis saves to localStorage | PASS | Dashboard count incremented after live analysis |
| `next build` | PASS | Zero errors, zero warnings |

## What's NOT Built (Out of Scope for Demo)

- Authentication / user accounts
- Database (Supabase) — using localStorage
- Re-check flow E2E tested with real photos (API route works, UI flow built, needs real before/after electrical photos to fully test)
- Multiple trades (plumbing, HVAC) — electrical only
- Real expert video/audio clips
- Offline mode
- Payment/billing
- Admin panel
- Push notifications
- Full NEC PDF RAG pipeline

## Known Issues / Rough Edges

1. Next.js dev overlay can intercept Playwright clicks (use `evaluate` to hide it, or navigate directly)
2. HMR websocket errors in automated testing (cosmetic, not real errors)
3. Test image (forest scene) pulled avg compliance down to 69% — clear localStorage or reload demo data to reset
4. The `getRelevantCodes(workType)` in `nec.ts` returns `NECSection[]` but `getCodesForJurisdiction()` returns a formatted `string` — don't confuse them
5. `getCaliforniaAmendments()` also returns a formatted `string`, not an array

## Commands

```bash
cd /Users/kumar/Documents/research/treehacks/tradeproof-app
npm run dev          # Start dev server on localhost:3000
npx next build       # Production build (verify zero errors)
npx tsc --noEmit     # Type check only
```

## Strategic Context

- **Target vertical:** Electricians first (photographable violations, NEC code citations, state licensing portability)
- **Demo story:** Alex Smith apprentice improving over 5 months. Live analysis on stage. Before/after re-check. Portfolio + passport.
- **LinkedIn integration:** Build own marketplace first. LinkedIn as marketing channel (shareable credential link), not data store.
- **Hackathon:** TreeHacks at Stanford. Judges care about: real AI analysis (not fake), compelling demo narrative, market insight.
