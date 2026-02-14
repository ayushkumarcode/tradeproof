// localStorage-based storage layer for TradeProof hackathon demo
// Replaces Supabase - all data persists in browser localStorage

const KEYS = {
  profile: 'tradeproof_profile',
  analyses: 'tradeproof_analyses',
  skillScores: 'tradeproof_skill_scores',
  demoLoaded: 'tradeproof_demo_loaded',
} as const;

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface UserProfile {
  id: string;
  fullName: string;
  trade: string;
  experienceLevel: 'apprentice' | 'journeyman' | 'master';
  primaryJurisdiction: string;
  createdAt: string;
}

/** One round of "fix and re-check" in a thread (original → fix 1 → fix 2 → …). */
export interface AnalysisRevision {
  fixedPhotoUrl: string;
  complianceScore: number;
  isCompliant: boolean;
  fixAnalysis: unknown;
  createdAt: string;
}

export interface Analysis {
  id: string;
  userId: string;
  photoUrl: string; // base64 data URL — always saved for portfolio
  beforePhotoUrl?: string; // optional "before" photo for before/after comparison
  jurisdiction: string;
  trade: string;
  workType: string;
  userDescription: string;
  isCompliant: boolean;
  complianceScore: number;
  violations: Violation[];
  correctItems: string[];
  skillsDemonstrated: SkillDemo[];
  overallAssessment: string;
  /** Latest fix only (current "after" image and result). */
  fixedPhotoUrl?: string;
  fixVerified?: boolean;
  fixComplianceScore?: number;
  fixAnalysis?: unknown;
  /** Previous rounds when user did "try again for a better grade". */
  revisionHistory?: AnalysisRevision[];
  createdAt: string;
}

export interface Violation {
  description: string;
  codeSection: string;
  localAmendment?: string;
  severity: 'critical' | 'major' | 'minor';
  confidence: 'high' | 'medium' | 'low';
  fixInstruction: string;
  whyThisMatters: string;
  visualEvidence?: string;
}

export interface SkillDemo {
  skill: string;
  quality: 'good' | 'acceptable' | 'needs_work';
}

export interface SkillScore {
  skillName: string;
  score: number; // 0-100
  totalInstances: number;
  trend: 'improving' | 'stable' | 'declining';
  lastUpdated: string;
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

function read<T>(key: string): T | null {
  if (!isBrowser()) return null;
  try {
    const raw = localStorage.getItem(key);
    if (!raw) return null;
    return JSON.parse(raw) as T;
  } catch {
    return null;
  }
}

function write<T>(key: string, value: T): void {
  if (!isBrowser()) return;
  localStorage.setItem(key, JSON.stringify(value));
}

// ---------------------------------------------------------------------------
// Default profile (used if nothing is stored yet)
// ---------------------------------------------------------------------------

const DEFAULT_PROFILE: UserProfile = {
  id: 'demo-alex-smith',
  fullName: 'Alex Smith',
  trade: 'electrical',
  experienceLevel: 'apprentice',
  primaryJurisdiction: 'California',
  createdAt: '2025-09-15T08:00:00Z',
};

// ---------------------------------------------------------------------------
// Profile
// ---------------------------------------------------------------------------

export function getProfile(): UserProfile {
  if (!isBrowser()) return DEFAULT_PROFILE;
  return read<UserProfile>(KEYS.profile) ?? DEFAULT_PROFILE;
}

export function saveProfile(profile: UserProfile): void {
  write(KEYS.profile, profile);
}

// ---------------------------------------------------------------------------
// Analyses
// ---------------------------------------------------------------------------

export function getAnalyses(): Analysis[] {
  if (!isBrowser()) return [];
  const analyses = read<Analysis[]>(KEYS.analyses) ?? [];
  // Sort by date descending (newest first)
  return analyses.sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );
}

export function saveAnalysis(analysis: Analysis): void {
  if (!isBrowser()) return;
  if (!analysis.photoUrl?.trim()) return; // picture is required for analysis
  const analyses = read<Analysis[]>(KEYS.analyses) ?? [];
  analyses.push(analysis);
  write(KEYS.analyses, analyses);
}

export function getAnalysis(id: string): Analysis | undefined {
  if (!isBrowser()) return undefined;
  const analyses = read<Analysis[]>(KEYS.analyses) ?? [];
  return analyses.find((a) => a.id === id);
}

export function updateAnalysis(id: string, updates: Partial<Analysis>): void {
  if (!isBrowser()) return;
  const analyses = read<Analysis[]>(KEYS.analyses) ?? [];
  const index = analyses.findIndex((a) => a.id === id);
  if (index === -1) return;
  analyses[index] = { ...analyses[index], ...updates };
  write(KEYS.analyses, analyses);
}

// ---------------------------------------------------------------------------
// Skill Scores
// ---------------------------------------------------------------------------

export function getSkillScores(): SkillScore[] {
  if (!isBrowser()) return [];
  return read<SkillScore[]>(KEYS.skillScores) ?? [];
}

export function updateSkillScores(skills: SkillDemo[]): void {
  if (!isBrowser()) return;

  const existing = read<SkillScore[]>(KEYS.skillScores) ?? [];
  const now = new Date().toISOString();

  for (const demo of skills) {
    const idx = existing.findIndex(
      (s) => s.skillName.toLowerCase() === demo.skill.toLowerCase()
    );

    // Map quality to a numeric value for averaging
    const qualityValue =
      demo.quality === 'good' ? 95 : demo.quality === 'acceptable' ? 75 : 50;

    if (idx !== -1) {
      const prev = existing[idx];
      const oldTotal = prev.totalInstances;
      const newTotal = oldTotal + 1;
      // Running weighted average
      const newScore = Math.round(
        (prev.score * oldTotal + qualityValue) / newTotal
      );

      // Determine trend: compare new score to previous
      let trend: 'improving' | 'stable' | 'declining' = prev.trend;
      const diff = newScore - prev.score;
      if (diff > 1) trend = 'improving';
      else if (diff < -1) trend = 'declining';
      else trend = 'stable';

      existing[idx] = {
        ...prev,
        score: newScore,
        totalInstances: newTotal,
        trend,
        lastUpdated: now,
      };
    } else {
      // New skill entry
      existing.push({
        skillName: demo.skill,
        score: qualityValue,
        totalInstances: 1,
        trend: 'stable',
        lastUpdated: now,
      });
    }
  }

  write(KEYS.skillScores, existing);
}

// ---------------------------------------------------------------------------
// Demo data management
// ---------------------------------------------------------------------------

export function isDemoLoaded(): boolean {
  if (!isBrowser()) return false;
  return localStorage.getItem(KEYS.demoLoaded) === 'true';
}

export function loadDemoData(): void {
  if (!isBrowser()) return;

  // Dynamic import would be ideal but we keep it simple for the demo.
  // The caller is expected to pass demo data or we lazy-import it.
  // This function is a thin wrapper that marks the flag; actual data
  // loading happens in the component layer via loadDemoDataFromSeed().
  // See below for the concrete loader.
}

/**
 * Loads seed data directly into localStorage. Call this with the exports
 * from `@/data/demo-data`.
 */
export function loadDemoDataFromSeed(
  profile: UserProfile,
  analyses: Analysis[],
  skillScores: SkillScore[]
): void {
  if (!isBrowser()) return;
  write(KEYS.profile, profile);
  write(KEYS.analyses, analyses);
  write(KEYS.skillScores, skillScores);
  localStorage.setItem(KEYS.demoLoaded, 'true');
}

export function clearAll(): void {
  if (!isBrowser()) return;
  localStorage.removeItem(KEYS.profile);
  localStorage.removeItem(KEYS.analyses);
  localStorage.removeItem(KEYS.skillScores);
  localStorage.removeItem(KEYS.demoLoaded);
}
