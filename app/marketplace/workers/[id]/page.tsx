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
  Shield,
  Star,
  Award,
  Briefcase,
  CheckCircle,
  User,
  Clock,
  Calendar,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  getWorker,
  getJobs,
  saveJob,
  generateId,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
} from "@/lib/marketplace-storage";
import type { MarketplaceWorker, MarketplaceJob } from "@/lib/marketplace-storage";
import {
  DEMO_WORKERS,
  DEMO_HOMEOWNERS,
  DEMO_JOBS,
  DEMO_MESSAGES,
} from "@/data/marketplace-demo";

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

export default function WorkerProfilePage() {
  const params = useParams();
  const router = useRouter();
  const workerId = params.id as string;

  const [worker, setWorker] = useState<MarketplaceWorker | null>(null);
  const [loading, setLoading] = useState(true);
  const [hireOpen, setHireOpen] = useState(false);
  const [hireTitle, setHireTitle] = useState("");
  const [hireDescription, setHireDescription] = useState("");
  const [hireLocation, setHireLocation] = useState("");
  const [hireBudget, setHireBudget] = useState("");
  const [completedJobs, setCompletedJobs] = useState<MarketplaceJob[]>([]);

  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
    const w = getWorker(workerId);
    setWorker(w || null);

    // Find completed jobs involving this worker
    if (w) {
      const allJobs = getJobs();
      const completed = allJobs.filter(
        (j) => j.assignedWorkerId === workerId && j.status === "completed"
      );
      setCompletedJobs(completed);
    }

    setLoading(false);
  }, [workerId]);

  function handleHire(e: React.FormEvent) {
    e.preventDefault();
    if (!worker || !hireTitle || !hireDescription || !hireLocation || !hireBudget) return;

    const budget = parseInt(hireBudget, 10);
    const jobId = `job-${generateId()}`;

    const newJob: MarketplaceJob = {
      id: jobId,
      posterId: `poster-${generateId()}`,
      posterType: "homeowner",
      posterName: "You",
      title: hireTitle,
      type: "other",
      description: hireDescription,
      location: hireLocation,
      budget: { min: budget, max: budget },
      preferredDates: [],
      urgency: "normal",
      requiredCerts: [],
      status: "in-progress",
      applicants: [
        {
          workerId: worker.id,
          quote: budget,
          message: "Direct hire",
          appliedAt: new Date().toISOString(),
        },
      ],
      assignedWorkerId: worker.id,
      createdAt: new Date().toISOString(),
    };

    saveJob(newJob);
    setHireOpen(false);
    router.push(`/marketplace/jobs/${jobId}`);
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-pulse text-slate-400">Loading...</div>
      </div>
    );
  }

  if (!worker) {
    return (
      <div className="min-h-screen bg-slate-50 pb-24">
        <div className="max-w-lg mx-auto px-4 pt-6 text-center">
          <p className="text-slate-500 mt-20">Worker not found</p>
          <Link href="/marketplace/workers">
            <Button variant="link" className="mt-4">
              Back to Workers
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  const avail = AVAILABILITY_CONFIG[worker.availability];

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      <div className="max-w-lg mx-auto px-4 pt-6">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <Link href="/marketplace/workers">
            <Button variant="ghost" size="icon" className="shrink-0">
              <ArrowLeft className="w-5 h-5" />
            </Button>
          </Link>
          <h1 className="text-xl font-bold text-slate-900">Worker Profile</h1>
        </div>

        {/* Profile Hero */}
        <Card className="mb-4">
          <CardContent>
            <div className="flex items-start gap-4">
              {/* Big Compliance Score */}
              <div
                className={cn(
                  "flex-shrink-0 w-20 h-20 rounded-2xl border-2 flex flex-col items-center justify-center",
                  scoreBg(worker.complianceScore)
                )}
              >
                <Shield
                  className={cn(
                    "w-5 h-5 mb-0.5",
                    scoreColor(worker.complianceScore)
                  )}
                />
                <span
                  className={cn(
                    "text-2xl font-bold leading-tight",
                    scoreColor(worker.complianceScore)
                  )}
                >
                  {worker.complianceScore}
                </span>
                <span className="text-[9px] text-slate-400 font-medium">
                  COMPLIANCE
                </span>
              </div>

              <div className="flex-1 min-w-0">
                <h2 className="text-lg font-bold text-slate-900">
                  {worker.name}
                </h2>
                <div className="flex items-center gap-2 mt-1">
                  <Badge
                    variant="outline"
                    className={cn("text-[10px]", avail.color)}
                  >
                    <span
                      className={cn(
                        "w-1.5 h-1.5 rounded-full mr-1",
                        avail.dot
                      )}
                    />
                    {avail.label}
                  </Badge>
                  <Badge variant="secondary" className="text-[10px]">
                    {worker.type === "agency" ? "Agency" : "Freelancer"}
                  </Badge>
                </div>
                <div className="flex items-center gap-3 mt-2 text-xs text-slate-500">
                  <span className="flex items-center gap-1">
                    <MapPin className="w-3 h-3" />
                    {worker.location}
                  </span>
                  <span className="flex items-center gap-1">
                    <DollarSign className="w-3 h-3" />${worker.rate}/hr
                  </span>
                </div>
              </div>
            </div>

            {/* Bio */}
            <p className="text-sm text-slate-600 leading-relaxed mt-4">
              {worker.bio}
            </p>

            {/* Action */}
            <Button
              className="w-full mt-4 gap-2"
              onClick={() => setHireOpen(true)}
            >
              <Briefcase className="w-4 h-4" />
              Hire {worker.name.split(" ")[0]} Directly
            </Button>
          </CardContent>
        </Card>

        {/* Certifications */}
        <Card className="mb-4">
          <CardContent className="space-y-3">
            <h3 className="font-semibold text-slate-900 text-sm flex items-center gap-1.5">
              <Award className="w-4 h-4 text-amber-500" />
              Certifications
            </h3>
            <div className="space-y-2">
              {worker.certifications.map((cert) => (
                <div
                  key={`${cert.skill}-${cert.source}`}
                  className="flex items-center justify-between p-2.5 rounded-lg bg-slate-50 border border-slate-100"
                >
                  <div className="flex items-center gap-2">
                    <CheckCircle className="w-4 h-4 text-emerald-500" />
                    <span className="text-sm font-medium text-slate-700">
                      {cert.skill}
                    </span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <Badge
                      variant="outline"
                      className={cn(
                        "text-[10px]",
                        cert.level === 2
                          ? "bg-amber-50 text-amber-700 border-amber-200"
                          : "bg-blue-50 text-blue-700 border-blue-200"
                      )}
                    >
                      Level {cert.level}
                    </Badge>
                    <Badge
                      variant="secondary"
                      className="text-[10px]"
                    >
                      {cert.source === "field" ? "Field" : "VR"}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Skills */}
        <Card className="mb-4">
          <CardContent>
            <h3 className="font-semibold text-slate-900 text-sm mb-3">
              Skills
            </h3>
            <div className="flex flex-wrap gap-2">
              {worker.skills.map((s) => (
                <Badge
                  key={s}
                  variant="secondary"
                  className="text-xs px-3 py-1"
                >
                  {s}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Stats */}
        <Card className="mb-4">
          <CardContent>
            <h3 className="font-semibold text-slate-900 text-sm mb-3">
              Stats
            </h3>
            <div className="grid grid-cols-3 gap-3">
              <div className="text-center p-3 rounded-lg bg-slate-50">
                <div className="text-lg font-bold text-slate-900">
                  {worker.totalAnalyses}
                </div>
                <div className="text-[10px] text-slate-500 mt-0.5">
                  Analyses
                </div>
              </div>
              <div className="text-center p-3 rounded-lg bg-slate-50">
                <div className="text-lg font-bold text-slate-900">
                  {worker.jobHistory.length}
                </div>
                <div className="text-[10px] text-slate-500 mt-0.5">
                  Jobs Done
                </div>
              </div>
              <div className="text-center p-3 rounded-lg bg-slate-50">
                <div className="text-lg font-bold text-slate-900">
                  {worker.license ? "Yes" : "No"}
                </div>
                <div className="text-[10px] text-slate-500 mt-0.5">
                  Licensed
                </div>
              </div>
            </div>

            {worker.license && (
              <div className="mt-3 p-2.5 rounded-lg bg-blue-50 border border-blue-100 text-xs">
                <span className="font-medium text-blue-700">License:</span>{" "}
                <span className="text-blue-600">{worker.license}</span>
              </div>
            )}
            {worker.insurance && (
              <div className="mt-2 p-2.5 rounded-lg bg-emerald-50 border border-emerald-100 text-xs">
                <span className="font-medium text-emerald-700">
                  Insurance:
                </span>{" "}
                <span className="text-emerald-600">{worker.insurance}</span>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Reviews from Completed Jobs */}
        {completedJobs.length > 0 && (
          <Card className="mb-4">
            <CardContent className="space-y-3">
              <h3 className="font-semibold text-slate-900 text-sm flex items-center gap-1.5">
                <Star className="w-4 h-4 text-amber-500" />
                Reviews
              </h3>
              {completedJobs.map((j) =>
                j.review ? (
                  <div
                    key={j.id}
                    className="p-3 rounded-lg bg-slate-50 border border-slate-100"
                  >
                    <div className="flex items-center gap-2 mb-1.5">
                      <div className="flex items-center gap-0.5">
                        {Array.from({ length: 5 }).map((_, i) => (
                          <Star
                            key={i}
                            className={cn(
                              "w-3 h-3",
                              i < j.review!.rating
                                ? "text-amber-400 fill-amber-400"
                                : "text-slate-200"
                            )}
                          />
                        ))}
                      </div>
                      <span className="text-xs text-slate-400">
                        {j.posterName}
                      </span>
                    </div>
                    <p className="text-xs text-slate-600 leading-relaxed">
                      {j.review.text}
                    </p>
                    <p className="text-[10px] text-slate-400 mt-1.5">
                      {j.title}
                    </p>
                  </div>
                ) : null
              )}
            </CardContent>
          </Card>
        )}

        {/* Member Since */}
        <div className="text-center text-xs text-slate-400 flex items-center justify-center gap-1 pb-4">
          <Calendar className="w-3 h-3" />
          Member since{" "}
          {new Date(worker.createdAt).toLocaleDateString("en-US", {
            month: "long",
            year: "numeric",
          })}
        </div>

        {/* Hire Dialog */}
        <Dialog open={hireOpen} onOpenChange={setHireOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>
                Hire {worker.name.split(" ")[0]} Directly
              </DialogTitle>
              <DialogDescription>
                Create a job pre-assigned to {worker.name}
              </DialogDescription>
            </DialogHeader>
            <form onSubmit={handleHire} className="space-y-4">
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Job Title
                </label>
                <Input
                  placeholder="e.g., Panel upgrade for my home"
                  value={hireTitle}
                  onChange={(e) => setHireTitle(e.target.value)}
                  required
                />
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Description
                </label>
                <textarea
                  placeholder="Describe what you need..."
                  value={hireDescription}
                  onChange={(e) => setHireDescription(e.target.value)}
                  rows={3}
                  required
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
                />
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Location
                </label>
                <Input
                  placeholder="e.g., San Jose, CA"
                  value={hireLocation}
                  onChange={(e) => setHireLocation(e.target.value)}
                  required
                />
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Budget ($)
                </label>
                <Input
                  type="number"
                  placeholder="e.g., 2000"
                  value={hireBudget}
                  onChange={(e) => setHireBudget(e.target.value)}
                  required
                />
              </div>
              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setHireOpen(false)}
                >
                  Cancel
                </Button>
                <Button type="submit">Create Job</Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
