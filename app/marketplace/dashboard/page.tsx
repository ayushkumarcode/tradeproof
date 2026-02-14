"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import {
  ArrowLeft,
  Briefcase,
  Send,
  MessageSquare,
  MapPin,
  DollarSign,
  Clock,
  Shield,
  ArrowRight,
  CheckCircle,
  Zap,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  getJobs,
  getMessages,
  getWorkers,
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
} from "@/lib/marketplace-storage";
import type { MarketplaceJob, MarketplaceMessage } from "@/lib/marketplace-storage";
import {
  DEMO_WORKERS,
  DEMO_HOMEOWNERS,
  DEMO_JOBS,
  DEMO_MESSAGES,
} from "@/data/marketplace-demo";

const STATUS_CONFIG: Record<
  string,
  { label: string; color: string }
> = {
  open: { label: "Open", color: "bg-emerald-100 text-emerald-700" },
  matched: { label: "Matched", color: "bg-blue-100 text-blue-700" },
  "in-progress": { label: "In Progress", color: "bg-amber-100 text-amber-700" },
  completed: { label: "Completed", color: "bg-slate-100 text-slate-700" },
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

export default function DashboardPage() {
  const [jobs, setJobs] = useState<MarketplaceJob[]>([]);
  const [messages, setMessages] = useState<MarketplaceMessage[]>([]);
  const [loading, setLoading] = useState(true);

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
    setMessages(getMessages());
    setLoading(false);
  }, []);

  // For demo, show all jobs as "my jobs" and use worker IDs to find applications
  const myJobs = jobs;
  const myApplications = jobs.filter((j) =>
    j.applicants.some((a) =>
      [
        "worker-maria-santos",
        "worker-james-chen",
        "worker-sarah-kim",
      ].includes(a.workerId)
    )
  );

  // Group messages by jobId
  const messageGroups = messages.reduce<Record<string, MarketplaceMessage[]>>(
    (acc, msg) => {
      if (!acc[msg.jobId]) acc[msg.jobId] = [];
      acc[msg.jobId].push(msg);
      return acc;
    },
    {}
  );

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
          <h1 className="text-xl font-bold text-slate-900">
            Marketplace Dashboard
          </h1>
        </div>

        {/* Tabs */}
        <Tabs defaultValue="jobs">
          <TabsList className="w-full">
            <TabsTrigger value="jobs" className="flex-1 gap-1">
              <Briefcase className="w-3.5 h-3.5" />
              My Jobs
            </TabsTrigger>
            <TabsTrigger value="applications" className="flex-1 gap-1">
              <Send className="w-3.5 h-3.5" />
              Applications
            </TabsTrigger>
            <TabsTrigger value="messages" className="flex-1 gap-1">
              <MessageSquare className="w-3.5 h-3.5" />
              Messages
            </TabsTrigger>
          </TabsList>

          {/* My Jobs Tab */}
          <TabsContent value="jobs" className="mt-4">
            {myJobs.length === 0 ? (
              <div className="text-center py-12">
                <Briefcase className="w-10 h-10 text-slate-300 mx-auto mb-3" />
                <p className="text-slate-500 text-sm">No jobs posted yet</p>
                <Link href="/marketplace/jobs/new">
                  <Button size="sm" className="mt-3">
                    Post a Job
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="space-y-3">
                {myJobs.map((job) => {
                  const sCfg = STATUS_CONFIG[job.status];
                  return (
                    <Link
                      key={job.id}
                      href={`/marketplace/jobs/${job.id}`}
                      className="block"
                    >
                      <Card className="hover:shadow-md transition-shadow">
                        <CardContent>
                          <div className="flex items-start justify-between gap-2 mb-2">
                            <h3 className="font-medium text-sm text-slate-900 leading-tight">
                              {job.title}
                            </h3>
                            <Badge
                              className={cn(
                                "shrink-0 text-[10px]",
                                sCfg.color
                              )}
                            >
                              {sCfg.label}
                            </Badge>
                          </div>
                          <div className="flex items-center gap-3 text-xs text-slate-500">
                            <span className="flex items-center gap-1">
                              <MapPin className="w-3 h-3" />
                              {job.location}
                            </span>
                            <span className="flex items-center gap-1">
                              <DollarSign className="w-3 h-3" />$
                              {job.budget.min.toLocaleString()} - $
                              {job.budget.max.toLocaleString()}
                            </span>
                            <span className="flex items-center gap-1">
                              <Clock className="w-3 h-3" />
                              {timeAgo(job.createdAt)}
                            </span>
                          </div>
                          <div className="flex items-center justify-between mt-2 pt-2 border-t border-slate-100">
                            <span className="text-xs text-slate-400">
                              {job.applicants.length} applicant
                              {job.applicants.length !== 1 ? "s" : ""}
                            </span>
                            <ArrowRight className="w-4 h-4 text-slate-300" />
                          </div>
                        </CardContent>
                      </Card>
                    </Link>
                  );
                })}
              </div>
            )}
          </TabsContent>

          {/* Applications Tab */}
          <TabsContent value="applications" className="mt-4">
            {myApplications.length === 0 ? (
              <div className="text-center py-12">
                <Send className="w-10 h-10 text-slate-300 mx-auto mb-3" />
                <p className="text-slate-500 text-sm">
                  No applications yet
                </p>
                <Link href="/marketplace/jobs">
                  <Button size="sm" className="mt-3">
                    Browse Jobs
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="space-y-3">
                {myApplications.map((job) => {
                  const sCfg = STATUS_CONFIG[job.status];
                  const isAssigned = job.assignedWorkerId && [
                    "worker-maria-santos",
                    "worker-james-chen",
                    "worker-sarah-kim",
                  ].includes(job.assignedWorkerId);

                  return (
                    <Link
                      key={job.id}
                      href={`/marketplace/jobs/${job.id}`}
                      className="block"
                    >
                      <Card className="hover:shadow-md transition-shadow">
                        <CardContent>
                          <div className="flex items-start justify-between gap-2 mb-2">
                            <h3 className="font-medium text-sm text-slate-900 leading-tight">
                              {job.title}
                            </h3>
                            <div className="flex items-center gap-1.5 shrink-0">
                              {isAssigned && (
                                <Badge className="text-[10px] bg-emerald-100 text-emerald-700">
                                  <CheckCircle className="w-3 h-3 mr-0.5" />
                                  Selected
                                </Badge>
                              )}
                              <Badge
                                className={cn(
                                  "text-[10px]",
                                  sCfg.color
                                )}
                              >
                                {sCfg.label}
                              </Badge>
                            </div>
                          </div>
                          <div className="flex items-center gap-3 text-xs text-slate-500">
                            <span className="flex items-center gap-1">
                              <MapPin className="w-3 h-3" />
                              {job.location}
                            </span>
                            <span className="flex items-center gap-1">
                              <DollarSign className="w-3 h-3" />$
                              {job.budget.min.toLocaleString()} - $
                              {job.budget.max.toLocaleString()}
                            </span>
                          </div>
                        </CardContent>
                      </Card>
                    </Link>
                  );
                })}
              </div>
            )}
          </TabsContent>

          {/* Messages Tab */}
          <TabsContent value="messages" className="mt-4">
            {Object.keys(messageGroups).length === 0 ? (
              <div className="text-center py-12">
                <MessageSquare className="w-10 h-10 text-slate-300 mx-auto mb-3" />
                <p className="text-slate-500 text-sm">No messages yet</p>
              </div>
            ) : (
              <div className="space-y-3">
                {Object.entries(messageGroups).map(([jobId, msgs]) => {
                  const job = jobs.find((j) => j.id === jobId);
                  const lastMsg = msgs[msgs.length - 1];
                  return (
                    <Link
                      key={jobId}
                      href={`/marketplace/messages/${jobId}`}
                      className="block"
                    >
                      <Card className="hover:shadow-md transition-shadow">
                        <CardContent>
                          <div className="flex items-start justify-between gap-2 mb-1.5">
                            <h3 className="font-medium text-sm text-slate-900 leading-tight">
                              {job?.title || "Job"}
                            </h3>
                            <Badge variant="secondary" className="text-[10px] shrink-0">
                              {msgs.length} msg{msgs.length !== 1 ? "s" : ""}
                            </Badge>
                          </div>
                          {lastMsg && (
                            <p className="text-xs text-slate-500 line-clamp-2">
                              <span className="font-medium">
                                {lastMsg.senderName}:
                              </span>{" "}
                              {lastMsg.text}
                            </p>
                          )}
                          <div className="flex items-center justify-between mt-2 pt-2 border-t border-slate-100">
                            <span className="text-[10px] text-slate-400">
                              {lastMsg
                                ? timeAgo(lastMsg.timestamp)
                                : ""}
                            </span>
                            <ArrowRight className="w-4 h-4 text-slate-300" />
                          </div>
                        </CardContent>
                      </Card>
                    </Link>
                  );
                })}
              </div>
            )}
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
