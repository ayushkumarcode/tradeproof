"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  ArrowLeft,
  Plus,
  X,
  MapPin,
  DollarSign,
  Calendar,
  AlertTriangle,
  Briefcase,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  saveJob,
  saveHomeowner,
  getHomeowner,
  generateId,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
  setCurrentUser,
} from "@/lib/marketplace-storage";
import type { MarketplaceJob } from "@/lib/marketplace-storage";
import {
  DEMO_WORKERS,
  DEMO_HOMEOWNERS,
  DEMO_JOBS,
  DEMO_MESSAGES,
} from "@/data/marketplace-demo";

const JOB_TYPES = [
  { value: "panel-upgrade", label: "Panel Upgrade" },
  { value: "rewiring", label: "Rewiring" },
  { value: "outlet-install", label: "Outlet Install" },
  { value: "troubleshooting", label: "Troubleshooting" },
  { value: "other", label: "Other" },
];

const CERT_OPTIONS = [
  "Panel Upgrades",
  "Rewiring",
  "Outlet Install",
  "Troubleshooting",
  "EV Charger Install",
  "Smart Home",
  "Lighting",
  "Solar",
  "Commercial",
];

export default function NewJobPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(true);

  // Form state
  const [posterType, setPosterType] = useState<"homeowner" | "contractor">(
    "homeowner"
  );
  const [posterName, setPosterName] = useState("");
  const [title, setTitle] = useState("");
  const [type, setType] = useState("panel-upgrade");
  const [description, setDescription] = useState("");
  const [location, setLocation] = useState("");
  const [budgetMin, setBudgetMin] = useState("");
  const [budgetMax, setBudgetMax] = useState("");
  const [dateInput, setDateInput] = useState("");
  const [dates, setDates] = useState<string[]>([]);
  const [urgency, setUrgency] = useState<"urgent" | "normal" | "flexible">(
    "normal"
  );
  const [requiredCerts, setRequiredCerts] = useState<
    { skill: string; level: number }[]
  >([]);
  const [certSkill, setCertSkill] = useState(CERT_OPTIONS[0]);
  const [certLevel, setCertLevel] = useState<number>(1);

  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
    setLoading(false);
  }, []);

  function addDate() {
    if (dateInput && !dates.includes(dateInput)) {
      setDates([...dates, dateInput]);
      setDateInput("");
    }
  }

  function removeDate(d: string) {
    setDates(dates.filter((x) => x !== d));
  }

  function addCert() {
    if (!requiredCerts.find((c) => c.skill === certSkill)) {
      setRequiredCerts([...requiredCerts, { skill: certSkill, level: certLevel }]);
    }
  }

  function removeCert(skill: string) {
    setRequiredCerts(requiredCerts.filter((c) => c.skill !== skill));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!title || !description || !location || !budgetMin || !budgetMax || !posterName) {
      return;
    }

    // Create/update homeowner or use contractor identity
    const posterId = `poster-${generateId()}`;
    if (posterType === "homeowner") {
      if (!getHomeowner(posterId)) {
        saveHomeowner({
          id: posterId,
          name: posterName,
          location,
          jobsPosted: [],
          createdAt: new Date().toISOString(),
        });
      }
      setCurrentUser(posterId, "homeowner");
    }

    const jobId = `job-${generateId()}`;

    const newJob: MarketplaceJob = {
      id: jobId,
      posterId,
      posterType,
      posterName,
      title,
      type,
      description,
      location,
      budget: {
        min: parseInt(budgetMin, 10),
        max: parseInt(budgetMax, 10),
      },
      preferredDates: dates,
      urgency,
      requiredCerts,
      status: "open",
      applicants: [],
      createdAt: new Date().toISOString(),
    };

    saveJob(newJob);

    // Update homeowner's jobs list
    if (posterType === "homeowner") {
      const ho = getHomeowner(posterId);
      if (ho) {
        saveHomeowner({ ...ho, jobsPosted: [...ho.jobsPosted, jobId] });
      }
    }

    router.push("/marketplace/jobs");
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-pulse text-slate-400">Loading...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      <div className="max-w-lg mx-auto px-4 pt-6">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <Link href="/marketplace">
            <Button variant="ghost" size="icon" className="shrink-0">
              <ArrowLeft className="w-5 h-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-xl font-bold text-slate-900">Post a Job</h1>
            <p className="text-slate-500 text-sm">
              Find a verified electrician for your project
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Poster Type Toggle */}
          <Card>
            <CardContent>
              <label className="text-sm font-medium text-slate-700 mb-2 block">
                I am a...
              </label>
              <div className="grid grid-cols-2 gap-2">
                <button
                  type="button"
                  onClick={() => setPosterType("homeowner")}
                  className={cn(
                    "py-2.5 px-4 rounded-lg text-sm font-medium transition-colors border",
                    posterType === "homeowner"
                      ? "bg-blue-600 text-white border-blue-600"
                      : "bg-white text-slate-600 border-slate-200 hover:border-slate-300"
                  )}
                >
                  Homeowner
                </button>
                <button
                  type="button"
                  onClick={() => setPosterType("contractor")}
                  className={cn(
                    "py-2.5 px-4 rounded-lg text-sm font-medium transition-colors border",
                    posterType === "contractor"
                      ? "bg-blue-600 text-white border-blue-600"
                      : "bg-white text-slate-600 border-slate-200 hover:border-slate-300"
                  )}
                >
                  Contractor
                </button>
              </div>
            </CardContent>
          </Card>

          {/* Basic Info */}
          <Card>
            <CardContent className="space-y-4">
              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Your Name
                </label>
                <Input
                  placeholder="e.g., John Smith"
                  value={posterName}
                  onChange={(e) => setPosterName(e.target.value)}
                  required
                />
              </div>

              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Job Title
                </label>
                <Input
                  placeholder="e.g., 200A Panel Upgrade for 1960s Ranch"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  required
                />
              </div>

              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Job Type
                </label>
                <select
                  value={type}
                  onChange={(e) => setType(e.target.value)}
                  className="w-full h-9 rounded-md border border-input bg-background px-3 text-sm shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                >
                  {JOB_TYPES.map((jt) => (
                    <option key={jt.value} value={jt.value}>
                      {jt.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 block">
                  Description
                </label>
                <textarea
                  placeholder="Describe the work needed, current setup, any special requirements..."
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={4}
                  required
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
                />
              </div>

              <div>
                <label className="text-sm font-medium text-slate-700 mb-1.5 flex items-center gap-1.5">
                  <MapPin className="w-4 h-4 text-slate-400" />
                  Location
                </label>
                <Input
                  placeholder="e.g., San Jose, CA"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  required
                />
              </div>
            </CardContent>
          </Card>

          {/* Budget */}
          <Card>
            <CardContent className="space-y-4">
              <label className="text-sm font-medium text-slate-700 flex items-center gap-1.5">
                <DollarSign className="w-4 h-4 text-slate-400" />
                Budget Range
              </label>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <span className="text-xs text-slate-400 mb-1 block">
                    Minimum
                  </span>
                  <Input
                    type="number"
                    placeholder="1000"
                    value={budgetMin}
                    onChange={(e) => setBudgetMin(e.target.value)}
                    required
                  />
                </div>
                <div>
                  <span className="text-xs text-slate-400 mb-1 block">
                    Maximum
                  </span>
                  <Input
                    type="number"
                    placeholder="3000"
                    value={budgetMax}
                    onChange={(e) => setBudgetMax(e.target.value)}
                    required
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Dates */}
          <Card>
            <CardContent className="space-y-3">
              <label className="text-sm font-medium text-slate-700 flex items-center gap-1.5">
                <Calendar className="w-4 h-4 text-slate-400" />
                Preferred Dates
              </label>
              <div className="flex gap-2">
                <Input
                  type="date"
                  value={dateInput}
                  onChange={(e) => setDateInput(e.target.value)}
                  className="flex-1"
                />
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addDate}
                  disabled={!dateInput}
                >
                  <Plus className="w-4 h-4" />
                </Button>
              </div>
              {dates.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {dates.map((d) => (
                    <Badge
                      key={d}
                      variant="secondary"
                      className="text-xs gap-1"
                    >
                      {d}
                      <button
                        type="button"
                        onClick={() => removeDate(d)}
                        className="hover:text-red-500"
                      >
                        <X className="w-3 h-3" />
                      </button>
                    </Badge>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Urgency */}
          <Card>
            <CardContent>
              <label className="text-sm font-medium text-slate-700 mb-2 flex items-center gap-1.5">
                <AlertTriangle className="w-4 h-4 text-slate-400" />
                Urgency
              </label>
              <div className="grid grid-cols-3 gap-2">
                {(["urgent", "normal", "flexible"] as const).map((u) => (
                  <button
                    key={u}
                    type="button"
                    onClick={() => setUrgency(u)}
                    className={cn(
                      "py-2 px-3 rounded-lg text-sm font-medium transition-colors border",
                      urgency === u
                        ? u === "urgent"
                          ? "bg-red-600 text-white border-red-600"
                          : "bg-blue-600 text-white border-blue-600"
                        : "bg-white text-slate-600 border-slate-200 hover:border-slate-300"
                    )}
                  >
                    {u.charAt(0).toUpperCase() + u.slice(1)}
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Required Certifications */}
          <Card>
            <CardContent className="space-y-3">
              <label className="text-sm font-medium text-slate-700 flex items-center gap-1.5">
                <Briefcase className="w-4 h-4 text-slate-400" />
                Required Certifications
              </label>
              <div className="flex gap-2">
                <select
                  value={certSkill}
                  onChange={(e) => setCertSkill(e.target.value)}
                  className="flex-1 h-9 rounded-md border border-input bg-background px-3 text-sm shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                >
                  {CERT_OPTIONS.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
                <select
                  value={certLevel}
                  onChange={(e) => setCertLevel(Number(e.target.value))}
                  className="w-20 h-9 rounded-md border border-input bg-background px-2 text-sm shadow-xs"
                >
                  <option value={1}>Lv.1</option>
                  <option value={2}>Lv.2</option>
                </select>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addCert}
                >
                  <Plus className="w-4 h-4" />
                </Button>
              </div>
              {requiredCerts.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {requiredCerts.map((c) => (
                    <Badge
                      key={c.skill}
                      variant="outline"
                      className="text-xs gap-1 bg-amber-50 text-amber-700 border-amber-200"
                    >
                      {c.skill} Lv.{c.level}
                      <button
                        type="button"
                        onClick={() => removeCert(c.skill)}
                        className="hover:text-red-500"
                      >
                        <X className="w-3 h-3" />
                      </button>
                    </Badge>
                  ))}
                </div>
              )}
              <p className="text-xs text-slate-400">
                Optional: Only workers with these certifications can apply
              </p>
            </CardContent>
          </Card>

          {/* Submit */}
          <Button type="submit" className="w-full h-12 text-base font-semibold">
            Post Job
          </Button>
        </form>
      </div>
    </div>
  );
}
