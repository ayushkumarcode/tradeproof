"use client";

import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import KnowledgeCard from "@/components/KnowledgeCard";
import { BookOpen, Shield } from "lucide-react";
import { KNOWLEDGE_CLIPS } from "@/data/knowledge-clips";

const taskTypes = [
  { id: "all", label: "All" },
  { id: "panel", label: "Panel" },
  { id: "outlet", label: "Outlet" },
  { id: "junction_box", label: "Junction Box" },
  { id: "general", label: "General" },
];

export default function KnowledgePage() {
  const [activeFilter, setActiveFilter] = useState("all");

  const filteredClips =
    activeFilter === "all"
      ? KNOWLEDGE_CLIPS
      : KNOWLEDGE_CLIPS.filter((clip) => clip.taskType === activeFilter);

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="max-w-lg mx-auto px-4 pt-6 pb-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2">
            <BookOpen className="w-5 h-5 text-blue-600" />
            <h1 className="text-xl font-bold text-slate-900">
              Expert Insights
            </h1>
          </div>
          <div className="flex items-center gap-1.5">
            <Shield className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-blue-600">
              TradeProof
            </span>
          </div>
        </div>

        {/* Description */}
        <p className="text-sm text-slate-500 mb-4">
          Real-world knowledge from experienced master electricians. Learn from
          the experts who have seen it all.
        </p>

        {/* Filter Tabs */}
        <div className="flex gap-2 overflow-x-auto pb-2 mb-6 no-scrollbar">
          {taskTypes.map((type) => (
            <button
              key={type.id}
              onClick={() => setActiveFilter(type.id)}
              className="shrink-0"
            >
              <Badge
                className={
                  activeFilter === type.id
                    ? "bg-blue-600 text-white text-xs px-3 py-1 cursor-pointer"
                    : "bg-slate-100 text-slate-600 text-xs px-3 py-1 cursor-pointer hover:bg-slate-200"
                }
              >
                {type.label}
              </Badge>
            </button>
          ))}
        </div>

        {/* Knowledge Clips */}
        <div className="space-y-4">
          {filteredClips.length > 0 ? (
            filteredClips.map((clip) => (
              <KnowledgeCard key={clip.id} clip={clip} />
            ))
          ) : (
            <div className="text-center py-12">
              <BookOpen className="w-10 h-10 text-slate-300 mx-auto mb-3" />
              <p className="text-sm text-slate-500">
                No clips for this category yet.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
