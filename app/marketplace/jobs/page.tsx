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
  Clock,
  Plus,
  Search,
  ArrowLeft,
  Briefcase,
  AlertTriangle,
  Filter,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  getJobs,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
} from "@/lib/marketplace-storage";
import type { MarketplaceJob } from "@/lib/marketplace-storage";
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

export default function JobsPage() {
  const [jobs, setJobs] = useState<MarketplaceJob[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("all");

  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
    setJobs(getJobs());
    setLoading(false);
  }, []);

  const openJobs = jobs.filter((j) => j.status === "open");

  const filteredJobs = openJobs.filter((j) => {
    const matchesSearch =
      !search ||
      j.title.toLowerCase().includes(search.toLowerCase()) ||
      j.location.toLowerCase().includes(search.toLowerCase()) ||
      j.description.toLowerCase().includes(search.toLowerCase());
    const matchesType = typeFilter === "all" || j.type === typeFilter;
    return matchesSearch && matchesType;
  });

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
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <Link href="/marketplace">
              <Button variant="ghost" size="icon" className="shrink-0">
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
            <div>
              <h1 className="text-xl font-bold text-slate-900">Open Jobs</h1>
              <p className="text-slate-500 text-sm">
                {filteredJobs.length} job{filteredJobs.length !== 1 ? "s" : ""}{" "}
                available
              </p>
            </div>
          </div>
          <Link href="/marketplace/jobs/new">
            <Button size="sm" className="gap-1.5">
              <Plus className="w-4 h-4" />
              Post Job
            </Button>
          </Link>
        </div>

        {/* Search & Filter */}
        <div className="space-y-3 mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <Input
              placeholder="Search jobs or locations..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>
          <div className="flex items-center gap-2 overflow-x-auto pb-1">
            <Filter className="w-4 h-4 text-slate-400 shrink-0" />
            {["all", "panel-upgrade", "rewiring", "outlet-install", "troubleshooting", "other"].map(
              (t) => (
                <button
                  key={t}
                  onClick={() => setTypeFilter(t)}
                  className={cn(
                    "px-3 py-1 rounded-full text-xs font-medium whitespace-nowrap transition-colors",
                    typeFilter === t
                      ? "bg-blue-600 text-white"
                      : "bg-white text-slate-600 border border-slate-200 hover:border-slate-300"
                  )}
                >
                  {t === "all" ? "All Types" : JOB_TYPE_LABELS[t] || t}
                </button>
              )
            )}
          </div>
        </div>

        {/* Job List */}
        {filteredJobs.length === 0 ? (
          <div className="text-center py-16">
            <Briefcase className="w-12 h-12 text-slate-300 mx-auto mb-3" />
            <p className="text-slate-500 font-medium">No open jobs found</p>
            <p className="text-slate-400 text-sm mt-1">
              Try adjusting your filters or check back later
            </p>
          </div>
        ) : (
          <div className="space-y-3">
            {filteredJobs.map((job) => (
              <Link
                key={job.id}
                href={`/marketplace/jobs/${job.id}`}
                className="block"
              >
                <Card className="hover:shadow-md transition-shadow cursor-pointer">
                  <CardContent className="space-y-3">
                    <div className="flex items-start justify-between gap-2">
                      <h3 className="font-semibold text-slate-900 text-sm leading-tight">
                        {job.title}
                      </h3>
                      <Badge
                        variant={
                          job.urgency === "urgent" ? "destructive" : "secondary"
                        }
                        className={cn(
                          "shrink-0 text-[10px]",
                          job.urgency === "urgent"
                            ? ""
                            : job.urgency === "normal"
                            ? "bg-blue-100 text-blue-700"
                            : "bg-slate-100 text-slate-600"
                        )}
                      >
                        {job.urgency === "urgent" && (
                          <AlertTriangle className="w-3 h-3 mr-0.5" />
                        )}
                        {job.urgency.charAt(0).toUpperCase() +
                          job.urgency.slice(1)}
                      </Badge>
                    </div>

                    <div className="flex flex-wrap gap-2">
                      <div className="flex items-center gap-1 text-xs text-slate-500">
                        <MapPin className="w-3.5 h-3.5" />
                        {job.location}
                      </div>
                      <div className="flex items-center gap-1 text-xs text-slate-500">
                        <DollarSign className="w-3.5 h-3.5" />$
                        {job.budget.min.toLocaleString()} - $
                        {job.budget.max.toLocaleString()}
                      </div>
                      <div className="flex items-center gap-1 text-xs text-slate-500">
                        <Clock className="w-3.5 h-3.5" />
                        {timeAgo(job.createdAt)}
                      </div>
                    </div>

                    {job.requiredCerts.length > 0 && (
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
                    )}

                    <div className="flex items-center justify-between pt-1 border-t border-slate-100">
                      <span className="text-xs text-slate-400">
                        Posted by {job.posterName}
                      </span>
                      <span className="text-xs text-slate-400">
                        {job.applicants.length} applicant
                        {job.applicants.length !== 1 ? "s" : ""}
                      </span>
                    </div>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
