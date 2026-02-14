"use client";

import { useState, useEffect, useRef } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  ArrowLeft,
  Send,
  Briefcase,
  User,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  getJob,
  getMessages,
  saveMessage,
  generateId,
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

function formatTime(dateStr: string): string {
  const d = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  const time = d.toLocaleTimeString("en-US", {
    hour: "numeric",
    minute: "2-digit",
  });

  if (diffDays === 0) return `Today ${time}`;
  if (diffDays === 1) return `Yesterday ${time}`;
  return `${d.toLocaleDateString("en-US", { month: "short", day: "numeric" })} ${time}`;
}

export default function MessageThreadPage() {
  const params = useParams();
  const jobId = params.jobId as string;
  const scrollRef = useRef<HTMLDivElement>(null);

  const [job, setJob] = useState<MarketplaceJob | null>(null);
  const [messages, setMessages] = useState<MarketplaceMessage[]>([]);
  const [newMsg, setNewMsg] = useState("");
  const [loading, setLoading] = useState(true);
  const [sendAs, setSendAs] = useState<"poster" | "worker">("poster");

  function loadData() {
    const j = getJob(jobId);
    setJob(j || null);
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

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages]);

  function handleSend(e: React.FormEvent) {
    e.preventDefault();
    if (!newMsg.trim() || !job) return;

    // Determine sender based on toggle
    let senderId: string;
    let senderName: string;

    if (sendAs === "poster") {
      senderId = job.posterId;
      senderName = job.posterName;
    } else {
      // Use assigned worker or first applicant
      const workerId = job.assignedWorkerId || job.applicants[0]?.workerId || "unknown";
      const workerApplicant = job.applicants.find((a) => a.workerId === workerId);
      senderId = workerId;
      // Get worker name from demo data
      const workerNames: Record<string, string> = {
        "worker-maria-santos": "Maria Santos",
        "worker-james-chen": "James Chen",
        "worker-priya-patel": "Priya Patel",
        "worker-derek-johnson": "Derek Johnson",
        "worker-sarah-kim": "Sarah Kim",
        "worker-marcus-rivera": "Marcus Rivera",
      };
      senderName = workerNames[workerId] || "Worker";
    }

    const msg: MarketplaceMessage = {
      id: `msg-${generateId()}`,
      jobId,
      senderId,
      senderName,
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
          <Link href="/marketplace/dashboard">
            <Button variant="link" className="mt-4">
              Back to Dashboard
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  // Identify the poster for alignment
  const posterIds = [job.posterId];

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col">
      <div className="max-w-lg mx-auto w-full flex flex-col flex-1">
        {/* Header */}
        <div className="px-4 pt-6 pb-3 bg-white border-b border-slate-200 sticky top-0 z-10">
          <div className="flex items-center gap-3">
            <Link href={`/marketplace/jobs/${jobId}`}>
              <Button variant="ghost" size="icon" className="shrink-0">
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
            <div className="flex-1 min-w-0">
              <h1 className="text-sm font-bold text-slate-900 truncate">
                {job.title}
              </h1>
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <Briefcase className="w-3 h-3" />
                <span>{job.posterName}</span>
                <span className="w-0.5 h-0.5 rounded-full bg-slate-300" />
                <span className="capitalize">{job.status}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Messages */}
        <div
          ref={scrollRef}
          className="flex-1 overflow-y-auto px-4 py-4 space-y-3"
          style={{ maxHeight: "calc(100vh - 200px)" }}
        >
          {messages.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-slate-400 text-sm">
                No messages yet. Start the conversation!
              </p>
            </div>
          ) : (
            messages.map((msg) => {
              const isFromPoster = posterIds.includes(msg.senderId);
              return (
                <div
                  key={msg.id}
                  className={cn(
                    "flex",
                    isFromPoster ? "justify-end" : "justify-start"
                  )}
                >
                  <div
                    className={cn(
                      "max-w-[80%] rounded-2xl px-4 py-2.5",
                      isFromPoster
                        ? "bg-blue-600 text-white rounded-br-md"
                        : "bg-white border border-slate-200 text-slate-800 rounded-bl-md"
                    )}
                  >
                    <div
                      className={cn(
                        "flex items-center gap-1.5 mb-1",
                        isFromPoster ? "text-blue-100" : "text-slate-400"
                      )}
                    >
                      <User className="w-3 h-3" />
                      <span className="text-[10px] font-medium">
                        {msg.senderName}
                      </span>
                    </div>
                    <p className="text-sm leading-relaxed">{msg.text}</p>
                    <p
                      className={cn(
                        "text-[10px] mt-1.5",
                        isFromPoster ? "text-blue-200" : "text-slate-400"
                      )}
                    >
                      {formatTime(msg.timestamp)}
                    </p>
                  </div>
                </div>
              );
            })
          )}
        </div>

        {/* Input Area */}
        <div className="px-4 py-3 bg-white border-t border-slate-200 pb-24">
          {/* Send As Toggle */}
          <div className="flex items-center gap-2 mb-2">
            <span className="text-[10px] text-slate-400">Send as:</span>
            <button
              onClick={() => setSendAs("poster")}
              className={cn(
                "px-2 py-0.5 rounded-full text-[10px] font-medium transition-colors",
                sendAs === "poster"
                  ? "bg-blue-600 text-white"
                  : "bg-slate-100 text-slate-500"
              )}
            >
              {job.posterName}
            </button>
            <button
              onClick={() => setSendAs("worker")}
              className={cn(
                "px-2 py-0.5 rounded-full text-[10px] font-medium transition-colors",
                sendAs === "worker"
                  ? "bg-blue-600 text-white"
                  : "bg-slate-100 text-slate-500"
              )}
            >
              Worker
            </button>
          </div>
          <form onSubmit={handleSend} className="flex gap-2">
            <Input
              placeholder="Type a message..."
              value={newMsg}
              onChange={(e) => setNewMsg(e.target.value)}
              className="flex-1"
            />
            <Button
              type="submit"
              size="icon"
              disabled={!newMsg.trim()}
            >
              <Send className="w-4 h-4" />
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
