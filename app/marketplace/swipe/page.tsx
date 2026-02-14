"use client";

import { useState, useEffect, useMemo } from "react";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  MapPin,
  DollarSign,
  Clock,
  ArrowLeft,
  Briefcase,
  AlertTriangle,
  Users,
  Shield,
  Heart,
} from "lucide-react";
import { SwipeStack, type SwipeDirection } from "@/components/SwipeStack";
import {
  getJobs,
  getWorkers,
  getCurrentUserId,
  getCurrentUserType,
  setCurrentUser,
  getWorkerSwipedJobIds,
  getEmployerSwipedWorkerIds,
  recordWorkerSwipe,
  recordEmployerSwipe,
  getMatchJobIdsForWorker,
  getMatchWorkerIdsForJob,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
} from "@/lib/marketplace-storage";
import type { MarketplaceJob, MarketplaceWorker } from "@/lib/marketplace-storage";
import {
  DEMO_WORKERS,
  DEMO_HOMEOWNERS,
  DEMO_JOBS,
  DEMO_MESSAGES,
} from "@/data/marketplace-demo";
import { cn } from "@/lib/utils";

const JOB_TYPE_LABELS: Record<string, string> = {
  "panel-upgrade": "Panel Upgrade",
  rewiring: "Rewiring",
  "outlet-install": "Outlet Install",
  troubleshooting: "Troubleshooting",
  other: "Other",
};

const AVAILABILITY_CONFIG: Record<
  string,
  { label: string; color: string; dot: string }
> = {
  available: {
    label: "Available",
    color: "bg-emerald-50 text-emerald-700 border-emerald-200",
    dot: "bg-emerald-500",
  },
  busy: {
    label: "Busy",
    color: "bg-amber-50 text-amber-700 border-amber-200",
    dot: "bg-amber-500",
  },
  unavailable: {
    label: "Unavailable",
    color: "bg-slate-100 text-slate-500 border-slate-200",
    dot: "bg-slate-400",
  },
};

function timeAgo(dateStr: string): string {
  const now = new Date();
  const date = new Date(dateStr);
  const diffMs = now.getTime() - date.getTime();
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
  if (diffDays === 0) return "Today";
  if (diffDays === 1) return "Yesterday";
  if (diffDays < 7) return `${diffDays}d ago`;
  if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`;
  return `${Math.floor(diffDays / 30)}mo ago`;
}

function scoreColor(score: number): string {
  if (score >= 90) return "text-emerald-600";
  if (score >= 80) return "text-blue-600";
  if (score >= 70) return "text-amber-600";
  return "text-red-600";
}

function scoreBg(score: number): string {
  if (score >= 90) return "bg-emerald-50 border-emerald-200";
  if (score >= 80) return "bg-blue-50 border-blue-200";
  if (score >= 70) return "bg-amber-50 border-amber-200";
  return "bg-red-50 border-red-200";
}

// Placeholder images for job cards (tall hero aspect for Hinge/Raya style)
const HOUSE_IMAGES = [
  "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=800&h=600&fit=crop",
  "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop",
  "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&h=600&fit=crop",
  "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800&h=600&fit=crop",
  "https://images.unsplash.com/photo-1602343168117-bb8ffe3e2e9f?w=800&h=600&fit=crop",
];
const PANEL_IMAGES = [
  "https://images.unsplash.com/photo-1621905251189-08b45d6e5e0e?w=300&h=200&fit=crop",
  "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=300&h=200&fit=crop",
  "https://images.unsplash.com/photo-1581092160562-40aa08e78837?w=300&h=200&fit=crop",
];
// Tall portrait images for worker hero (Hinge/Raya style)
const WORKER_AVATARS = [
  "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=600&h=750&fit=crop&facepad=2",
  "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=600&h=750&fit=crop&facepad=2",
  "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=600&h=750&fit=crop&facepad=2",
  "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=600&h=750&fit=crop&facepad=2",
  "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=600&h=750&fit=crop&facepad=2",
  "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=600&h=750&fit=crop&facepad=2",
];
function pickImage(id: string, urls: string[]): string {
  const hash = id.split("").reduce((a, c) => a + c.charCodeAt(0), 0);
  return urls[hash % urls.length]!;
}

// ----- Job card: outer border, text + images inside (no one image filling the card) -----
function JobSwipeCard({ job }: { job: MarketplaceJob }) {
  const houseImg = pickImage(job.id, HOUSE_IMAGES);
  const panelImg = pickImage(job.id + "-panel", PANEL_IMAGES);
  return (
    <div className="flex flex-col h-full min-h-0 overflow-hidden bg-white rounded-2xl border-2 border-slate-200 shadow-lg">
      <div className="flex-1 flex flex-col min-h-0 overflow-y-auto p-4">
        {/* Title and meta */}
        <h2 className="font-bold text-slate-900 text-lg leading-tight">
          {job.title}
        </h2>
        <div className="flex items-center gap-3 mt-2 text-sm text-slate-500">
          <span className="flex items-center gap-1">
            <MapPin className="w-4 h-4 shrink-0" />
            {job.location}
          </span>
          <span className="flex items-center gap-1">
            <DollarSign className="w-4 h-4 shrink-0" />
            ${job.budget.min.toLocaleString()} – ${job.budget.max.toLocaleString()}
          </span>
        </div>
        <div className="flex items-center gap-2 mt-2 flex-wrap">
          <Badge
            variant={job.urgency === "urgent" ? "destructive" : "secondary"}
            className={cn(
              "text-xs",
              job.urgency === "normal" && "bg-blue-100 text-blue-700",
              job.urgency === "flexible" && "bg-slate-100 text-slate-600"
            )}
          >
            {job.urgency === "urgent" && <AlertTriangle className="w-3 h-3 mr-1 inline" />}
            {job.urgency.charAt(0).toUpperCase() + job.urgency.slice(1)}
          </Badge>
          <Badge variant="outline" className="text-xs">
            {JOB_TYPE_LABELS[job.type] || job.type}
          </Badge>
        </div>

        {/* Property image inside card (modest size) */}
        <div className="mt-4 rounded-xl overflow-hidden border border-slate-200 bg-slate-100">
          <img
            src={houseImg}
            alt=""
            className="w-full h-32 object-cover"
          />
          <p className="text-[10px] text-slate-500 text-center py-1.5 bg-slate-50 border-t border-slate-100">
            Property
          </p>
        </div>

        <p className="text-slate-600 text-sm leading-relaxed mt-4">
          {job.description}
        </p>

        {/* Panel image + certs inside card */}
        <div className="mt-4 flex gap-3">
          <div className="flex-shrink-0 w-28 rounded-lg overflow-hidden border border-slate-200 bg-slate-100">
            <img src={panelImg} alt="" className="w-full h-20 object-cover" />
            <p className="text-[10px] text-slate-500 text-center py-1 bg-slate-50">Panel</p>
          </div>
          <div className="flex-1 min-w-0">
            {job.requiredCerts.length > 0 ? (
              <>
                <p className="text-xs font-medium text-slate-500 mb-1">Required</p>
                <div className="flex flex-wrap gap-1.5">
                  {job.requiredCerts.map((cert) => (
                    <Badge
                      key={cert.skill}
                      variant="outline"
                      className="text-[10px] bg-amber-50 text-amber-700 border-amber-200"
                    >
                      {cert.skill} Lv.{cert.level}
                    </Badge>
                  ))}
                </div>
              </>
            ) : (
              <p className="text-xs text-slate-400">No specific certs required</p>
            )}
          </div>
        </div>

        <div className="flex items-center gap-2 text-xs text-slate-500 mt-4 pt-3 border-t border-slate-100">
          <Clock className="w-3.5 h-3.5" />
          {timeAgo(job.createdAt)}
          <span className="text-slate-300">·</span>
          <span>Posted by {job.posterName}</span>
        </div>
      </div>
    </div>
  );
}

// ----- Worker card: outer border, text + photo inside (no one image filling the card) -----
function WorkerSwipeCard({ worker }: { worker: MarketplaceWorker }) {
  const avail = AVAILABILITY_CONFIG[worker.availability];
  const avatarImg = pickImage(worker.id, WORKER_AVATARS);
  return (
    <div className="flex flex-col h-full min-h-0 overflow-hidden bg-white rounded-2xl border-2 border-slate-200 shadow-lg">
      <div className="flex-1 flex flex-col min-h-0 overflow-y-auto p-4">
        <h2 className="font-bold text-slate-900 text-lg">{worker.name}</h2>
        <div className="flex items-center gap-2 mt-2 flex-wrap">
          <Badge variant="outline" className={cn("text-xs", avail.color)}>
            <span className={cn("w-2 h-2 rounded-full mr-1.5", avail.dot)} />
            {avail.label}
          </Badge>
          <span className="text-sm text-slate-500 flex items-center gap-1">
            <MapPin className="w-3.5 h-3.5" />
            {worker.location}
          </span>
          <span className="text-sm font-medium text-slate-700">${worker.rate}/hr</span>
        </div>

        {/* Profile photo inside card (modest size, not full-bleed) */}
        <div className="mt-4 flex justify-center">
          <div className="w-28 h-28 rounded-xl overflow-hidden border-2 border-slate-200 bg-slate-100 flex-shrink-0">
            <img
              src={avatarImg}
              alt=""
              className="w-full h-full object-cover"
            />
          </div>
        </div>

        <div className="mt-3 flex justify-center">
          <div
            className={cn(
              "inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg border text-sm font-semibold",
              scoreBg(worker.complianceScore),
              scoreColor(worker.complianceScore)
            )}
          >
            <Shield className="w-4 h-4" />
            {worker.complianceScore} compliance
          </div>
        </div>

        <p className="text-xs font-medium text-slate-500 uppercase tracking-wide mt-4 mb-1">
          About
        </p>
        <p className="text-slate-600 text-sm leading-relaxed">
          {worker.bio}
        </p>

        <p className="text-xs font-medium text-slate-500 uppercase tracking-wide mt-4 mb-2">
          Skills
        </p>
        <div className="flex flex-wrap gap-2">
          {worker.skills.map((s) => (
            <Badge key={s} variant="secondary" className="text-xs px-2 py-0.5">
              {s}
            </Badge>
          ))}
        </div>

        <div className="text-xs text-slate-500 mt-4 pt-3 border-t border-slate-100 space-y-0.5">
          <p>
            {worker.type === "agency" ? "Agency" : "Freelancer"}
            {worker.license && " · Licensed"}
          </p>
          <p>{worker.totalAnalyses} analyses · Compliance verified</p>
        </div>
      </div>
    </div>
  );
}

export default function SwipePage() {
  const [userType, setUserTypeState] = useState<"worker" | "homeowner" | null>(
    null
  );
  const [userId, setUserId] = useState<string | null>(null);
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [matchCount, setMatchCount] = useState(0);
  const [stackKey, setStackKey] = useState(0);

  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
    setUserTypeState(getCurrentUserType());
    setUserId(getCurrentUserId());
    setLoading(false);
  }, []);

  const jobs = useMemo(() => getJobs().filter((j) => j.status === "open"), []);
  const workers = useMemo(() => getWorkers(), []);

  const openJobsForEmployer = useMemo(() => {
    if (!userId || userType !== "homeowner") return [];
    return jobs.filter((j) => j.posterId === userId);
  }, [jobs, userId, userType]);

  const jobStack = useMemo(() => {
    if (userType !== "worker" || !userId) return [];
    const { liked, passed } = getWorkerSwipedJobIds(userId);
    const seen = new Set([...liked, ...passed]);
    return jobs.filter((j) => !seen.has(j.id));
  }, [userType, userId, jobs, stackKey]);

  const workerStack = useMemo(() => {
    if (userType !== "homeowner" || !selectedJobId) return [];
    const { liked, passed } = getEmployerSwipedWorkerIds(selectedJobId);
    const seen = new Set([...liked, ...passed]);
    return workers.filter((w) => !seen.has(w.id));
  }, [userType, selectedJobId, workers, stackKey]);

  useEffect(() => {
    if (!userId) return;
    if (userType === "worker") {
      setMatchCount(getMatchJobIdsForWorker(userId).length);
    } else if (userType === "homeowner" && selectedJobId) {
      setMatchCount(getMatchWorkerIdsForJob(selectedJobId).length);
    }
  }, [userType, userId, selectedJobId, jobStack.length, workerStack.length]);

  const handleSetRole = (type: "worker" | "homeowner") => {
    if (type === "worker" && DEMO_WORKERS.length > 0) {
      const id = DEMO_WORKERS[0].id;
      setCurrentUser(id, "worker");
      setUserId(id);
      setUserTypeState("worker");
    } else if (type === "homeowner" && DEMO_HOMEOWNERS.length > 0) {
      const id = DEMO_HOMEOWNERS[0].id;
      setCurrentUser(id, "homeowner");
      setUserId(id);
      setUserTypeState("homeowner");
      const myJobs = jobs.filter((j) => j.posterId === id);
      if (myJobs.length === 1) setSelectedJobId(myJobs[0].id);
    }
  };

  const handleWorkerSwipe = (job: MarketplaceJob, direction: SwipeDirection) => {
    if (!userId) return;
    recordWorkerSwipe(userId, job.id, direction);
    setStackKey((k) => k + 1);
    setMatchCount(getMatchJobIdsForWorker(userId).length);
  };

  const handleEmployerSwipe = (
    worker: MarketplaceWorker,
    direction: SwipeDirection
  ) => {
    if (!selectedJobId) return;
    recordEmployerSwipe(selectedJobId, worker.id, direction);
    setStackKey((k) => k + 1);
    setMatchCount(getMatchWorkerIdsForJob(selectedJobId).length);
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="animate-pulse text-slate-400">Loading...</div>
      </div>
    );
  }

  if (!userType) {
    return (
      <div className="min-h-screen bg-slate-50 pb-24">
        <div className="max-w-lg mx-auto px-4 pt-8">
          <div className="flex items-center gap-3 mb-8">
            <Link href="/marketplace">
              <Button variant="ghost" size="icon" className="shrink-0">
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
            <h1 className="text-xl font-bold text-slate-900">Swipe to Match</h1>
          </div>
          <div className="text-center mb-8">
            <p className="text-slate-600 mb-6">
              Choose how you want to use the marketplace. Workers swipe on jobs;
              employers swipe on candidates.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <button
                type="button"
                onClick={() => handleSetRole("worker")}
                className="flex flex-col items-center gap-3 rounded-2xl border-2 border-amber-200 bg-white p-8 shadow-sm hover:border-amber-400 hover:shadow-md transition-all"
              >
                <Briefcase className="w-12 h-12 text-amber-600" />
                <span className="font-semibold text-slate-900">
                  I'm a Worker
                </span>
                <span className="text-sm text-slate-500">
                  Swipe on jobs you want
                </span>
              </button>
              <button
                type="button"
                onClick={() => handleSetRole("homeowner")}
                className="flex flex-col items-center gap-3 rounded-2xl border-2 border-blue-200 bg-white p-8 shadow-sm hover:border-blue-400 hover:shadow-md transition-all"
              >
                <Users className="w-12 h-12 text-blue-600" />
                <span className="font-semibold text-slate-900">
                  I'm an Employer
                </span>
                <span className="text-sm text-slate-500">
                  Swipe on candidates for your job
                </span>
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (userType === "homeowner" && openJobsForEmployer.length === 0) {
    return (
      <div className="min-h-screen bg-slate-50 pb-24">
        <div className="max-w-lg mx-auto px-4 pt-8">
          <div className="flex items-center gap-3 mb-6">
            <Link href="/marketplace">
              <Button variant="ghost" size="icon" className="shrink-0">
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
            <h1 className="text-xl font-bold text-slate-900">Swipe Candidates</h1>
          </div>
          <div className="rounded-2xl border-2 border-dashed border-slate-200 bg-white p-8 text-center">
            <Briefcase className="w-12 h-12 text-slate-300 mx-auto mb-3" />
            <p className="text-slate-600 font-medium">No open jobs</p>
            <p className="text-slate-500 text-sm mt-1">
              Post a job first, then come back to swipe on candidates.
            </p>
            <Link href="/marketplace/jobs/new" className="mt-4 inline-block">
              <Button>Post a job</Button>
            </Link>
          </div>
        </div>
      </div>
    );
  }

  if (
    userType === "homeowner" &&
    openJobsForEmployer.length > 0 &&
    !selectedJobId
  ) {
    return (
      <div className="min-h-screen bg-slate-50 pb-24">
        <div className="max-w-lg mx-auto px-4 pt-8">
          <div className="flex items-center gap-3 mb-6">
            <Link href="/marketplace">
              <Button variant="ghost" size="icon" className="shrink-0">
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
            <h1 className="text-xl font-bold text-slate-900">
              Pick a job to find candidates
            </h1>
          </div>
          <div className="space-y-2">
            {openJobsForEmployer.map((job) => (
              <button
                key={job.id}
                type="button"
                onClick={() => setSelectedJobId(job.id)}
                className="w-full text-left rounded-xl border border-slate-200 bg-white p-4 shadow-sm hover:border-blue-300 hover:shadow transition-all"
              >
                <p className="font-semibold text-slate-900">{job.title}</p>
                <p className="text-sm text-slate-500 mt-0.5">
                  {job.location} · $
                  {job.budget.min.toLocaleString()}–
                  ${job.budget.max.toLocaleString()}
                </p>
              </button>
            ))}
          </div>
        </div>
      </div>
    );
  }

  const isWorkerMode = userType === "worker";
  const selectedJob = selectedJobId
    ? jobs.find((j) => j.id === selectedJobId)
    : null;

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      <div className="max-w-lg mx-auto px-4 pt-6">
        <div className="flex items-center justify-between gap-3 mb-4">
          <div className="flex items-center gap-3">
            <Link href="/marketplace">
              <Button variant="ghost" size="icon" className="shrink-0">
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
            <div>
              <h1 className="text-xl font-bold text-slate-900">
                {isWorkerMode ? "Swipe Jobs" : "Swipe Candidates"}
              </h1>
              <p className="text-slate-500 text-sm">
                {isWorkerMode
                  ? `${jobStack.length} job${jobStack.length !== 1 ? "s" : ""} left`
                  : selectedJob
                  ? `${workerStack.length} candidate${workerStack.length !== 1 ? "s" : ""} left`
                  : ""}
                {matchCount > 0 && (
                  <span className="ml-2 text-emerald-600 font-medium flex items-center gap-1">
                    <Heart className="w-3.5 h-3.5 fill-current" />
                    {matchCount} match{matchCount !== 1 ? "es" : ""}
                  </span>
                )}
              </p>
            </div>
          </div>
          {matchCount > 0 && (
            <Link href="/marketplace/dashboard">
              <Button variant="outline" size="sm" className="gap-1">
                <Heart className="w-4 h-4" />
                Matches
              </Button>
            </Link>
          )}
        </div>

        {!isWorkerMode && selectedJob && (
          <div className="mb-4 rounded-xl bg-white border border-slate-200 px-4 py-2 flex items-center justify-between">
            <span className="text-sm text-slate-600 truncate">
              For: {selectedJob.title}
            </span>
            <Button
              variant="ghost"
              size="sm"
              className="shrink-0 text-xs"
              onClick={() => setSelectedJobId(null)}
            >
              Change
            </Button>
          </div>
        )}

        <div className="mb-3 rounded-xl bg-slate-800 text-white px-4 py-3 text-center">
          <p className="text-sm font-medium">
            Click and drag the card left or right to choose
          </p>
          <p className="text-slate-300 text-xs mt-1">
            Drag right = like · Drag left = pass · Or use the buttons below
          </p>
        </div>

        <div className="h-[min(720px,85vh)] max-w-sm mx-auto flex flex-col">
          {isWorkerMode ? (
            <SwipeStack
              className="flex-1 min-h-0"
              items={jobStack}
              keyFn={(j) => j.id}
              renderCard={(job) => <JobSwipeCard job={job} />}
              onSwipe={handleWorkerSwipe}
              emptyMessage={
                <div className="space-y-2">
                  <p className="text-slate-500 font-medium">No more jobs</p>
                  <p className="text-slate-400 text-sm">
                    Check back later or browse all jobs
                  </p>
                  <Link href="/marketplace/jobs">
                    <Button variant="outline" size="sm" className="mt-2">
                      Browse jobs
                    </Button>
                  </Link>
                </div>
              }
            />
          ) : (
            <SwipeStack
              className="flex-1 min-h-0"
              items={workerStack}
              keyFn={(w) => w.id}
              renderCard={(worker) => <WorkerSwipeCard worker={worker} />}
              onSwipe={handleEmployerSwipe}
              emptyMessage={
                <div className="space-y-2">
                  <p className="text-slate-500 font-medium">No more candidates</p>
                  <p className="text-slate-400 text-sm">
                    Change job or browse workers
                  </p>
                  <div className="flex gap-2 justify-center mt-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setSelectedJobId(null)}
                    >
                      Change job
                    </Button>
                    <Link href="/marketplace/workers">
                      <Button variant="outline" size="sm">
                        Browse workers
                      </Button>
                    </Link>
                  </div>
                </div>
              }
            />
          )}
        </div>

        <p className="text-center text-slate-500 text-sm mt-3 font-medium">
          Click and hold the card, then drag it left or right to choose
        </p>
        <p className="text-center text-slate-400 text-xs mt-1">
          Drag right = like · Drag left = pass · Or use the buttons below the card
        </p>
      </div>
    </div>
  );
}
