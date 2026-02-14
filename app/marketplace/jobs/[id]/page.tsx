"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  ArrowLeft,
  MapPin,
  DollarSign,
  Clock,
  Calendar,
  Shield,
  Star,
  AlertTriangle,
  CheckCircle,
  MessageSquare,
  Zap,
  User,
  Send,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  getJob,
  getWorker,
  getWorkers,
  updateJob,
  autoMatchWorker,
  generateId,
  getMessages,
  saveMessage,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
  setCurrentUser,
} from "@/lib/marketplace-storage";
import type { MarketplaceJob, MarketplaceWorker, MarketplaceMessage } from "@/lib/marketplace-storage";
import {
  DEMO_WORKERS,
  DEMO_HOMEOWNERS,
  DEMO_JOBS,
  DEMO_MESSAGES,
} from "@/data/marketplace-demo";

const JOB_TYPE_LABELS: Record<string, string> = {
  "panel-upgrade": "Panel Upgrade",
  rewiring: "Rewiring",
  "outlet-install": "Outlet Install",
  troubleshooting: "Troubleshooting",
  other: "Other",
};

const STATUS_CONFIG: Record<
  string,
  { label: string; color: string; icon: typeof CheckCircle }
> = {
  open: { label: "Open", color: "bg-emerald-100 text-emerald-700", icon: Clock },
  matched: { label: "Matched", color: "bg-blue-100 text-blue-700", icon: CheckCircle },
  "in-progress": { label: "In Progress", color: "bg-amber-100 text-amber-700", icon: Zap },
  completed: { label: "Completed", color: "bg-slate-100 text-slate-700", icon: CheckCircle },
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

export default function JobDetailPage() {
  const params = useParams();
  const router = useRouter();
  const jobId = params.id as string;

  const [job, setJob] = useState<MarketplaceJob | null>(null);
  const [loading, setLoading] = useState(true);
  const [applyOpen, setApplyOpen] = useState(false);
  const [quote, setQuote] = useState("");
  const [message, setMessage] = useState("");
  const [assignedWorker, setAssignedWorker] = useState<MarketplaceWorker | undefined>();
  const [applicantWorkers, setApplicantWorkers] = useState<
    (MarketplaceWorker & { quote: number; message: string; appliedAt: string })[]
  >([]);
  const [messages, setMessages] = useState<MarketplaceMessage[]>([]);
  const [newMsg, setNewMsg] = useState("");
  const [selectedWorkerId, setSelectedWorkerId] = useState<string>("");

  function loadData() {
    const j = getJob(jobId);
    if (!j) return;
    setJob(j);

    if (j.assignedWorkerId) {
      setAssignedWorker(getWorker(j.assignedWorkerId));
    }

    // Resolve applicant details
    const workers = getWorkers();
    const resolved = j.applicants.map((a) => {
      const w = workers.find((wk) => wk.id === a.workerId);
      return {
        id: w?.id || a.workerId,
        name: w?.name || "Unknown",
        type: w?.type || ("freelancer" as const),
        skills: w?.skills || [],
        certifications: w?.certifications || [],
        rate: w?.rate || 0,
        availability: w?.availability || ("unavailable" as const),
        complianceScore: w?.complianceScore || 0,
        totalAnalyses: w?.totalAnalyses || 0,
        jobHistory: w?.jobHistory || [],
        location: w?.location || "",
        bio: w?.bio || "",
        createdAt: w?.createdAt || "",
        quote: a.quote,
        message: a.message,
        appliedAt: a.appliedAt,
      };
    });
    // Sort by compliance score descending
    resolved.sort((a, b) => b.complianceScore - a.complianceScore);
    setApplicantWorkers(resolved);

    // Load messages
    setMessages(getMessages(jobId));
  }

  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
    loadData();
    setLoading(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [jobId]);

  function handleApply(e: React.FormEvent) {
    e.preventDefault();
    if (!job || !quote || !message) return;

    // Use a demo worker for applying
    const workerId = "worker-james-chen";
    setCurrentUser(workerId, "worker");

    // Prevent duplicate applications from the same worker
    if (job.applicants.some((a) => a.workerId === workerId)) {
      setApplyOpen(false);
      return;
    }

    const newApplicant = {
      workerId,
      quote: parseInt(quote, 10),
      message,
      appliedAt: new Date().toISOString(),
    };

    updateJob(jobId, {
      applicants: [...job.applicants, newApplicant],
    });

    setApplyOpen(false);
    setQuote("");
    setMessage("");
    loadData();
  }

  function handleAutoMatch() {
    if (!job) return;
    const match = autoMatchWorker(job);
    if (match) {
      updateJob(jobId, {
        assignedWorkerId: match.id,
        status: "in-progress",
      });
      loadData();
    }
  }

  function handleAssign(workerId: string) {
    if (!job) return;
    updateJob(jobId, {
      assignedWorkerId: workerId,
      status: "in-progress",
    });
    loadData();
  }

  function handleSendMessage(e: React.FormEvent) {
    e.preventDefault();
    if (!newMsg.trim()) return;

    const msg: MarketplaceMessage = {
      id: `msg-${generateId()}`,
      jobId,
      senderId: job?.posterId || "unknown",
      senderName: job?.posterName || "Unknown",
      text: newMsg.trim(),
      timestamp: new Date().toISOString(),
    };
    saveMessage(msg);
    setNewMsg("");
    loadData();
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-pulse text-slate-400">Loading...</div>
      </div>
    );
  }

  if (!job) {
    return (
      <div className="min-h-screen bg-slate-50 pb-24">
        <div className="max-w-lg mx-auto px-4 pt-6 text-center">
          <p className="text-slate-500 mt-20">Job not found</p>
          <Link href="/marketplace/jobs">
            <Button variant="link" className="mt-4">
              Back to Jobs
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  const statusCfg = STATUS_CONFIG[job.status];
  const StatusIcon = statusCfg.icon;

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      <div className="max-w-lg mx-auto px-4 pt-6">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <Link href="/marketplace/jobs">
            <Button variant="ghost" size="icon" className="shrink-0">
              <ArrowLeft className="w-5 h-5" />
            </Button>
          </Link>
          <div className="flex-1 min-w-0">
            <h1 className="text-lg font-bold text-slate-900 leading-tight">
              {job.title}
            </h1>
            <div className="flex items-center gap-2 mt-1">
              <Badge className={cn("text-[10px]", statusCfg.color)}>
                <StatusIcon className="w-3 h-3 mr-0.5" />
                {statusCfg.label}
              </Badge>
              <span className="text-xs text-slate-400">
                {timeAgo(job.createdAt)}
              </span>
            </div>
          </div>
        </div>

        {/* Job Details */}
        <Card className="mb-4">
          <CardContent className="space-y-4">
            <div>
              <h3 className="text-sm font-medium text-slate-700 mb-1">
                Description
              </h3>
              <p className="text-sm text-slate-600 leading-relaxed">
                {job.description}
              </p>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="flex items-center gap-2 text-sm text-slate-600">
                <MapPin className="w-4 h-4 text-slate-400" />
                {job.location}
              </div>
              <div className="flex items-center gap-2 text-sm text-slate-600">
                <DollarSign className="w-4 h-4 text-slate-400" />$
                {job.budget.min.toLocaleString()} - $
                {job.budget.max.toLocaleString()}
              </div>
              <div className="flex items-center gap-2 text-sm text-slate-600">
                <AlertTriangle className="w-4 h-4 text-slate-400" />
                {job.urgency.charAt(0).toUpperCase() + job.urgency.slice(1)}
              </div>
              <div className="flex items-center gap-2 text-sm text-slate-600">
                <User className="w-4 h-4 text-slate-400" />
                {job.posterName}
              </div>
            </div>

            {job.preferredDates.length > 0 && (
              <div>
                <span className="text-sm font-medium text-slate-700 flex items-center gap-1.5 mb-1.5">
                  <Calendar className="w-4 h-4 text-slate-400" />
                  Preferred Dates
                </span>
                <div className="flex flex-wrap gap-1.5">
                  {job.preferredDates.map((d) => (
                    <Badge key={d} variant="secondary" className="text-xs">
                      {d}
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            {job.requiredCerts.length > 0 && (
              <div>
                <span className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Required Certifications
                </span>
                <div className="flex flex-wrap gap-1.5">
                  {job.requiredCerts.map((c) => (
                    <Badge
                      key={c.skill}
                      variant="outline"
                      className="text-xs bg-amber-50 text-amber-700 border-amber-200"
                    >
                      {c.skill} Lv.{c.level}
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            <div className="text-xs text-slate-400">
              Type: {JOB_TYPE_LABELS[job.type] || job.type}
            </div>
          </CardContent>
        </Card>

        {/* Open: Apply Button */}
        {job.status === "open" && (
          <div className="mb-4">
            <Button
              className="w-full gap-2"
              onClick={() => setApplyOpen(true)}
            >
              <Send className="w-4 h-4" />
              Apply for This Job
            </Button>
          </div>
        )}

        {/* Open: Applicants (poster view) */}
        {job.status === "open" && applicantWorkers.length > 0 && (
          <Card className="mb-4">
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold text-slate-900 text-sm">
                  Applicants ({applicantWorkers.length})
                </h3>
                <Button
                  size="sm"
                  variant="outline"
                  className="gap-1.5 text-xs"
                  onClick={handleAutoMatch}
                >
                  <Zap className="w-3.5 h-3.5" />
                  Auto-Match
                </Button>
              </div>

              <div className="space-y-3">
                {applicantWorkers.map((aw) => (
                  <div
                    key={aw.id}
                    className={cn(
                      "p-3 rounded-lg border transition-colors",
                      selectedWorkerId === aw.id
                        ? "border-blue-300 bg-blue-50"
                        : "border-slate-200 bg-white"
                    )}
                  >
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <Link
                          href={`/marketplace/workers/${aw.id}`}
                          className="font-medium text-sm text-blue-600 hover:underline"
                        >
                          {aw.name}
                        </Link>
                        <span
                          className={cn(
                            "text-xs font-bold",
                            scoreColor(aw.complianceScore)
                          )}
                        >
                          <Shield className="w-3 h-3 inline mr-0.5" />
                          {aw.complianceScore}
                        </span>
                      </div>
                      <span className="text-sm font-semibold text-slate-900">
                        ${aw.quote.toLocaleString()}
                      </span>
                    </div>
                    <p className="text-xs text-slate-600 leading-relaxed mb-2">
                      {aw.message}
                    </p>
                    <div className="flex items-center justify-between">
                      <span className="text-[10px] text-slate-400">
                        Applied {timeAgo(aw.appliedAt)}
                      </span>
                      <Button
                        size="xs"
                        onClick={(e) => {
                          e.preventDefault();
                          handleAssign(aw.id);
                        }}
                      >
                        Select
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        {/* In-progress / Matched: Assigned Worker */}
        {(job.status === "in-progress" || job.status === "matched") &&
          assignedWorker && (
            <Card className="mb-4">
              <CardContent className="space-y-3">
                <h3 className="font-semibold text-slate-900 text-sm">
                  Assigned Worker
                </h3>
                <Link
                  href={`/marketplace/workers/${assignedWorker.id}`}
                  className="flex items-center gap-3 p-3 rounded-lg border border-blue-200 bg-blue-50"
                >
                  <div className="w-10 h-10 rounded-lg bg-blue-600 flex items-center justify-center">
                    <User className="w-5 h-5 text-white" />
                  </div>
                  <div className="flex-1">
                    <p className="font-medium text-sm text-slate-900">
                      {assignedWorker.name}
                    </p>
                    <div className="flex items-center gap-2 text-xs text-slate-500">
                      <span className={scoreColor(assignedWorker.complianceScore)}>
                        <Shield className="w-3 h-3 inline mr-0.5" />
                        {assignedWorker.complianceScore}
                      </span>
                      <span>{assignedWorker.location}</span>
                    </div>
                  </div>
                </Link>

                {/* Quick Messages */}
                <div className="space-y-2 pt-2 border-t border-slate-100">
                  <div className="flex items-center justify-between">
                    <h4 className="text-xs font-medium text-slate-700">
                      Messages
                    </h4>
                    <Link href={`/marketplace/messages/${jobId}`}>
                      <Button variant="ghost" size="xs" className="text-xs gap-1">
                        <MessageSquare className="w-3 h-3" />
                        View All
                      </Button>
                    </Link>
                  </div>
                  {messages.slice(-3).map((m) => (
                    <div key={m.id} className="text-xs p-2 rounded bg-slate-50">
                      <span className="font-medium text-slate-700">
                        {m.senderName}:
                      </span>{" "}
                      <span className="text-slate-600">{m.text}</span>
                    </div>
                  ))}
                  <form
                    onSubmit={handleSendMessage}
                    className="flex gap-2"
                  >
                    <Input
                      placeholder="Send a message..."
                      value={newMsg}
                      onChange={(e) => setNewMsg(e.target.value)}
                      className="text-xs h-8"
                    />
                    <Button type="submit" size="xs">
                      <Send className="w-3 h-3" />
                    </Button>
                  </form>
                </div>
              </CardContent>
            </Card>
          )}

        {/* Completed: Review */}
        {job.status === "completed" && (
          <>
            {assignedWorker && (
              <Card className="mb-4">
                <CardContent className="space-y-3">
                  <h3 className="font-semibold text-slate-900 text-sm">
                    Completed By
                  </h3>
                  <Link
                    href={`/marketplace/workers/${assignedWorker.id}`}
                    className="flex items-center gap-3 p-3 rounded-lg border border-emerald-200 bg-emerald-50"
                  >
                    <div className="w-10 h-10 rounded-lg bg-emerald-600 flex items-center justify-center">
                      <CheckCircle className="w-5 h-5 text-white" />
                    </div>
                    <div className="flex-1">
                      <p className="font-medium text-sm text-slate-900">
                        {assignedWorker.name}
                      </p>
                      <div className="flex items-center gap-2 text-xs text-slate-500">
                        <span
                          className={scoreColor(
                            assignedWorker.complianceScore
                          )}
                        >
                          <Shield className="w-3 h-3 inline mr-0.5" />
                          {assignedWorker.complianceScore}
                        </span>
                        <span>
                          Completed{" "}
                          {job.completedAt
                            ? new Date(job.completedAt).toLocaleDateString()
                            : ""}
                        </span>
                      </div>
                    </div>
                  </Link>
                </CardContent>
              </Card>
            )}

            {job.review && (
              <Card className="mb-4">
                <CardContent className="space-y-2">
                  <h3 className="font-semibold text-slate-900 text-sm">
                    Review
                  </h3>
                  <div className="flex items-center gap-1">
                    {Array.from({ length: 5 }).map((_, i) => (
                      <Star
                        key={i}
                        className={cn(
                          "w-4 h-4",
                          i < job.review!.rating
                            ? "text-amber-400 fill-amber-400"
                            : "text-slate-200"
                        )}
                      />
                    ))}
                    <span className="text-sm font-medium text-slate-700 ml-1">
                      {job.review.rating}/5
                    </span>
                  </div>
                  <p className="text-sm text-slate-600 leading-relaxed">
                    {job.review.text}
                  </p>
                </CardContent>
              </Card>
            )}
          </>
        )}

        {/* Apply Dialog */}
        <Dialog open={applyOpen} onOpenChange={setApplyOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Apply for Job</DialogTitle>
              <DialogDescription>
                Submit your quote and a message to {job.posterName}
              </DialogDescription>
            </DialogHeader>
            <form onSubmit={handleApply} className="space-y-4">
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Your Quote ($)
                </label>
                <Input
                  type="number"
                  placeholder="e.g., 2500"
                  value={quote}
                  onChange={(e) => setQuote(e.target.value)}
                  required
                />
                <p className="text-xs text-slate-400 mt-1">
                  Budget: ${job.budget.min.toLocaleString()} - $
                  {job.budget.max.toLocaleString()}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Message
                </label>
                <textarea
                  placeholder="Describe your experience with this type of work, timeline, etc."
                  value={message}
                  onChange={(e) => setMessage(e.target.value)}
                  rows={4}
                  required
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
                />
              </div>
              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setApplyOpen(false)}
                >
                  Cancel
                </Button>
                <Button type="submit">Submit Application</Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
