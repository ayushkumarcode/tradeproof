"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  MapPin,
  DollarSign,
  Shield,
  ArrowLeft,
  Search,
  Filter,
  Star,
  Users,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  getWorkers,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
} from "@/lib/marketplace-storage";
import type { MarketplaceWorker } from "@/lib/marketplace-storage";
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

export default function WorkersPage() {
  const [workers, setWorkers] = useState<MarketplaceWorker[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [skillFilter, setSkillFilter] = useState<string>("all");
  const [availFilter, setAvailFilter] = useState<string>("all");

  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
    setWorkers(getWorkers());
    setLoading(false);
  }, []);

  // Collect unique skills
  const allSkills = Array.from(
    new Set(workers.flatMap((w) => w.skills))
  ).sort();

  const filtered = workers.filter((w) => {
    const matchesSearch =
      !search ||
      w.name.toLowerCase().includes(search.toLowerCase()) ||
      w.location.toLowerCase().includes(search.toLowerCase()) ||
      w.bio.toLowerCase().includes(search.toLowerCase());
    const matchesSkill =
      skillFilter === "all" ||
      w.skills.some((s) => s.toLowerCase() === skillFilter.toLowerCase());
    const matchesAvail =
      availFilter === "all" || w.availability === availFilter;
    return matchesSearch && matchesSkill && matchesAvail;
  });

  // Sort by compliance score descending
  filtered.sort((a, b) => b.complianceScore - a.complianceScore);

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
            <h1 className="text-xl font-bold text-slate-900">
              Verified Workers
            </h1>
            <p className="text-slate-500 text-sm">
              {filtered.length} electrician{filtered.length !== 1 ? "s" : ""}
            </p>
          </div>
        </div>

        {/* Search */}
        <div className="space-y-3 mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <Input
              placeholder="Search workers or locations..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>

          {/* Skill Filter */}
          <div className="flex items-center gap-2 overflow-x-auto pb-1">
            <Filter className="w-4 h-4 text-slate-400 shrink-0" />
            <button
              onClick={() => setSkillFilter("all")}
              className={cn(
                "px-3 py-1 rounded-full text-xs font-medium whitespace-nowrap transition-colors",
                skillFilter === "all"
                  ? "bg-blue-600 text-white"
                  : "bg-white text-slate-600 border border-slate-200"
              )}
            >
              All Skills
            </button>
            {allSkills.map((s) => (
              <button
                key={s}
                onClick={() => setSkillFilter(s)}
                className={cn(
                  "px-3 py-1 rounded-full text-xs font-medium whitespace-nowrap transition-colors",
                  skillFilter === s
                    ? "bg-blue-600 text-white"
                    : "bg-white text-slate-600 border border-slate-200"
                )}
              >
                {s}
              </button>
            ))}
          </div>

          {/* Availability Filter */}
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-400 shrink-0">Status:</span>
            {["all", "available", "busy", "unavailable"].map((a) => (
              <button
                key={a}
                onClick={() => setAvailFilter(a)}
                className={cn(
                  "px-3 py-1 rounded-full text-xs font-medium whitespace-nowrap transition-colors",
                  availFilter === a
                    ? "bg-blue-600 text-white"
                    : "bg-white text-slate-600 border border-slate-200"
                )}
              >
                {a === "all"
                  ? "All"
                  : a.charAt(0).toUpperCase() + a.slice(1)}
              </button>
            ))}
          </div>
        </div>

        {/* Worker Cards */}
        {filtered.length === 0 ? (
          <div className="text-center py-16">
            <Users className="w-12 h-12 text-slate-300 mx-auto mb-3" />
            <p className="text-slate-500 font-medium">No workers found</p>
            <p className="text-slate-400 text-sm mt-1">
              Try adjusting your filters
            </p>
          </div>
        ) : (
          <div className="space-y-3">
            {filtered.map((worker) => {
              const avail = AVAILABILITY_CONFIG[worker.availability];
              return (
                <Link
                  key={worker.id}
                  href={`/marketplace/workers/${worker.id}`}
                  className="block"
                >
                  <Card className="hover:shadow-md transition-shadow cursor-pointer">
                    <CardContent>
                      <div className="flex items-start gap-3">
                        {/* Avatar / Score */}
                        <div
                          className={cn(
                            "flex-shrink-0 w-14 h-14 rounded-xl border-2 flex flex-col items-center justify-center",
                            scoreBg(worker.complianceScore)
                          )}
                        >
                          <Shield
                            className={cn(
                              "w-4 h-4",
                              scoreColor(worker.complianceScore)
                            )}
                          />
                          <span
                            className={cn(
                              "text-lg font-bold leading-tight",
                              scoreColor(worker.complianceScore)
                            )}
                          >
                            {worker.complianceScore}
                          </span>
                        </div>

                        {/* Info */}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <h3 className="font-semibold text-slate-900 text-sm truncate">
                              {worker.name}
                            </h3>
                            <Badge
                              variant="outline"
                              className={cn("text-[10px] shrink-0", avail.color)}
                            >
                              <span
                                className={cn(
                                  "w-1.5 h-1.5 rounded-full mr-1",
                                  avail.dot
                                )}
                              />
                              {avail.label}
                            </Badge>
                          </div>

                          <div className="flex items-center gap-3 mt-1 text-xs text-slate-500">
                            <span className="flex items-center gap-1">
                              <MapPin className="w-3 h-3" />
                              {worker.location}
                            </span>
                            <span className="flex items-center gap-1">
                              <DollarSign className="w-3 h-3" />$
                              {worker.rate}/hr
                            </span>
                          </div>

                          <div className="flex flex-wrap gap-1 mt-2">
                            {worker.skills.slice(0, 4).map((s) => (
                              <Badge
                                key={s}
                                variant="secondary"
                                className="text-[10px] px-1.5 py-0"
                              >
                                {s}
                              </Badge>
                            ))}
                            {worker.skills.length > 4 && (
                              <Badge
                                variant="secondary"
                                className="text-[10px] px-1.5 py-0"
                              >
                                +{worker.skills.length - 4}
                              </Badge>
                            )}
                          </div>

                          <div className="flex items-center gap-2 mt-2 text-[10px] text-slate-400">
                            <span>
                              {worker.type === "agency"
                                ? "Agency"
                                : "Freelancer"}
                            </span>
                            <span className="w-0.5 h-0.5 rounded-full bg-slate-300" />
                            <span>
                              {worker.totalAnalyses} analyses
                            </span>
                            {worker.license && (
                              <>
                                <span className="w-0.5 h-0.5 rounded-full bg-slate-300" />
                                <span className="flex items-center gap-0.5">
                                  <Star className="w-2.5 h-2.5" />
                                  Licensed
                                </span>
                              </>
                            )}
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </Link>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
