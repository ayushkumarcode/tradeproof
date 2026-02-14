// Marketplace storage layer for TradeProof
// Uses localStorage, following the same patterns as lib/storage.ts

// ---------------------------------------------------------------------------
// Keys
// ---------------------------------------------------------------------------

const MK = {
  workers: 'tradeproof_mp_workers',
  homeowners: 'tradeproof_mp_homeowners',
  jobs: 'tradeproof_mp_jobs',
  messages: 'tradeproof_mp_messages',
  demoLoaded: 'tradeproof_mp_demo_loaded',
  currentUserId: 'tradeproof_mp_current_user',
  currentUserType: 'tradeproof_mp_current_user_type',
} as const;

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface MarketplaceWorker {
  id: string;
  name: string;
  type: 'freelancer' | 'agency';
  license?: string;
  insurance?: string;
  skills: string[];
  certifications: { skill: string; level: 1 | 2; source: 'vr' | 'field' }[];
  rate: number;
  availability: 'available' | 'busy' | 'unavailable';
  complianceScore: number;
  totalAnalyses: number;
  jobHistory: string[];
  location: string;
  bio: string;
  avatarUrl?: string;
  createdAt: string;
}

export interface MarketplaceHomeowner {
  id: string;
  name: string;
  location: string;
  phone?: string;
  jobsPosted: string[];
  createdAt: string;
}

export interface MarketplaceJob {
  id: string;
  posterId: string;
  posterType: 'homeowner' | 'contractor';
  posterName: string;
  title: string;
  type: string;
  description: string;
  location: string;
  budget: { min: number; max: number };
  preferredDates: string[];
  urgency: 'urgent' | 'normal' | 'flexible';
  requiredCerts: { skill: string; level: number }[];
  status: 'open' | 'matched' | 'in-progress' | 'completed';
  applicants: { workerId: string; quote: number; message: string; appliedAt: string }[];
  assignedWorkerId?: string;
  completedAt?: string;
  review?: { rating: number; text: string };
  createdAt: string;
}

export interface MarketplaceMessage {
  id: string;
  jobId: string;
  senderId: string;
  senderName: string;
  text: string;
  timestamp: string;
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

function generateId(): string {
  return Date.now().toString(36) + Math.random().toString(36).slice(2);
}

// ---------------------------------------------------------------------------
// Current user (marketplace session)
// ---------------------------------------------------------------------------

export function setCurrentUser(id: string, type: 'worker' | 'homeowner'): void {
  if (!isBrowser()) return;
  localStorage.setItem(MK.currentUserId, id);
  localStorage.setItem(MK.currentUserType, type);
}

export function getCurrentUserId(): string | null {
  if (!isBrowser()) return null;
  return localStorage.getItem(MK.currentUserId);
}

export function getCurrentUserType(): 'worker' | 'homeowner' | null {
  if (!isBrowser()) return null;
  const t = localStorage.getItem(MK.currentUserType);
  if (t === 'worker' || t === 'homeowner') return t;
  return null;
}

// ---------------------------------------------------------------------------
// Workers
// ---------------------------------------------------------------------------

export function getWorkers(): MarketplaceWorker[] {
  if (!isBrowser()) return [];
  return read<MarketplaceWorker[]>(MK.workers) ?? [];
}

export function getWorker(id: string): MarketplaceWorker | undefined {
  return getWorkers().find((w) => w.id === id);
}

export function saveWorker(worker: MarketplaceWorker): void {
  if (!isBrowser()) return;
  const workers = getWorkers();
  const idx = workers.findIndex((w) => w.id === worker.id);
  if (idx !== -1) {
    workers[idx] = worker;
  } else {
    workers.push(worker);
  }
  write(MK.workers, workers);
}

// ---------------------------------------------------------------------------
// Homeowners
// ---------------------------------------------------------------------------

export function getHomeowners(): MarketplaceHomeowner[] {
  if (!isBrowser()) return [];
  return read<MarketplaceHomeowner[]>(MK.homeowners) ?? [];
}

export function getHomeowner(id: string): MarketplaceHomeowner | undefined {
  return getHomeowners().find((h) => h.id === id);
}

export function saveHomeowner(homeowner: MarketplaceHomeowner): void {
  if (!isBrowser()) return;
  const homeowners = getHomeowners();
  const idx = homeowners.findIndex((h) => h.id === homeowner.id);
  if (idx !== -1) {
    homeowners[idx] = homeowner;
  } else {
    homeowners.push(homeowner);
  }
  write(MK.homeowners, homeowners);
}

// ---------------------------------------------------------------------------
// Jobs
// ---------------------------------------------------------------------------

export function getJobs(): MarketplaceJob[] {
  if (!isBrowser()) return [];
  const jobs = read<MarketplaceJob[]>(MK.jobs) ?? [];
  return jobs.sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );
}

export function getJob(id: string): MarketplaceJob | undefined {
  return getJobs().find((j) => j.id === id);
}

export function saveJob(job: MarketplaceJob): void {
  if (!isBrowser()) return;
  const jobs = read<MarketplaceJob[]>(MK.jobs) ?? [];
  const idx = jobs.findIndex((j) => j.id === job.id);
  if (idx !== -1) {
    jobs[idx] = job;
  } else {
    jobs.push(job);
  }
  write(MK.jobs, jobs);
}

export function updateJob(id: string, updates: Partial<MarketplaceJob>): void {
  if (!isBrowser()) return;
  const jobs = read<MarketplaceJob[]>(MK.jobs) ?? [];
  const idx = jobs.findIndex((j) => j.id === id);
  if (idx === -1) return;
  jobs[idx] = { ...jobs[idx], ...updates };
  write(MK.jobs, jobs);
}

// ---------------------------------------------------------------------------
// Messages
// ---------------------------------------------------------------------------

export function getMessages(jobId?: string): MarketplaceMessage[] {
  if (!isBrowser()) return [];
  const all = read<MarketplaceMessage[]>(MK.messages) ?? [];
  if (jobId) {
    return all
      .filter((m) => m.jobId === jobId)
      .sort(
        (a, b) =>
          new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
      );
  }
  return all.sort(
    (a, b) =>
      new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
  );
}

export function saveMessage(message: MarketplaceMessage): void {
  if (!isBrowser()) return;
  const messages = read<MarketplaceMessage[]>(MK.messages) ?? [];
  messages.push(message);
  write(MK.messages, messages);
}

// ---------------------------------------------------------------------------
// Auto-match logic
// ---------------------------------------------------------------------------

export function autoMatchWorker(job: MarketplaceJob): MarketplaceWorker | null {
  const workers = getWorkers();

  // Filter: must be available and have required certs at required level
  const eligible = workers.filter((w) => {
    if (w.availability !== 'available') return false;
    for (const req of job.requiredCerts) {
      const has = w.certifications.find(
        (c) =>
          c.skill.toLowerCase() === req.skill.toLowerCase() && c.level >= req.level
      );
      if (!has) return false;
    }
    return true;
  });

  if (eligible.length === 0) return null;

  // Find max rate for normalization
  const maxRate = Math.max(...eligible.map((w) => w.rate));

  // Score each worker
  const scored = eligible.map((w) => {
    const complianceNorm = w.complianceScore / 100; // 0-1
    const locationMatch =
      w.location.toLowerCase().includes(job.location.toLowerCase()) ||
      job.location.toLowerCase().includes(w.location.toLowerCase())
        ? 1
        : 0;
    const rateNorm = maxRate > 0 ? 1 - w.rate / maxRate : 0.5; // lower is better

    const score =
      complianceNorm * 0.5 + locationMatch * 0.3 + rateNorm * 0.2;

    return { worker: w, score };
  });

  scored.sort((a, b) => b.score - a.score);
  return scored[0].worker;
}

// ---------------------------------------------------------------------------
// Demo data management
// ---------------------------------------------------------------------------

export function isMarketplaceDemoLoaded(): boolean {
  if (!isBrowser()) return false;
  return localStorage.getItem(MK.demoLoaded) === 'true';
}

export function loadMarketplaceDemoData(
  workers: MarketplaceWorker[],
  homeowners: MarketplaceHomeowner[],
  jobs: MarketplaceJob[],
  messages: MarketplaceMessage[]
): void {
  if (!isBrowser()) return;
  write(MK.workers, workers);
  write(MK.homeowners, homeowners);
  write(MK.jobs, jobs);
  write(MK.messages, messages);
  localStorage.setItem(MK.demoLoaded, 'true');
}

export { generateId };
