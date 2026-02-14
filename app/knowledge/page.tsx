"use client";

import { useState, useMemo } from "react";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import LearnPostCard from "@/components/LearnPostCard";
import { BookOpen, Shield, Search } from "lucide-react";
import { KNOWLEDGE_CLIPS } from "@/data/knowledge-clips";

const taskTypes = [
  { id: "all", label: "All" },
  { id: "panel", label: "Panel" },
  { id: "outlet", label: "Outlet" },
  { id: "junction_box", label: "Junction Box" },
  { id: "general", label: "General" },
];

function searchMatches(clip: (typeof KNOWLEDGE_CLIPS)[0], query: string): boolean {
  if (!query.trim()) return true;
  const q = query.toLowerCase().trim();
  const searchable = [
    clip.title,
    clip.content,
    clip.expertName,
    ...clip.triggerKeywords,
  ].join(" ");
  return searchable.toLowerCase().includes(q);
}

export default function KnowledgePage() {
  const [activeFilter, setActiveFilter] = useState("all");
  const [searchQuery, setSearchQuery] = useState("");

  const filteredClips = useMemo(() => {
    let list = KNOWLEDGE_CLIPS;
    if (activeFilter !== "all") {
      list = list.filter((clip) => clip.taskType === activeFilter);
    }
    if (searchQuery.trim()) {
      list = list.filter((clip) => searchMatches(clip, searchQuery));
    }
    return [...list].sort(
      (a, b) =>
        new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime()
    );
  }, [activeFilter, searchQuery]);

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="max-w-lg mx-auto px-4 pt-6 pb-24">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <BookOpen className="w-5 h-5 text-blue-600" />
            <h1 className="text-xl font-bold text-slate-900">Learn</h1>
          </div>
          <div className="flex items-center gap-1.5">
            <Shield className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-blue-600">
              TradeProof
            </span>
          </div>
        </div>

        <p className="text-sm text-slate-500 mb-4">
          Real-world knowledge from experienced electricians. Tips, stories, and
          how-tos — search and filter by topic.
        </p>

        {/* Search */}
        <div className="relative mb-4">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <Input
            type="search"
            placeholder="Search tips, panels, GFCI, wiring..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9 bg-white border-slate-200"
          />
        </div>

        {/* Category filters */}
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

        {/* Feed — newest first, mixed card styles */}
        <div className="space-y-5">
          {filteredClips.length > 0 ? (
            filteredClips.map((clip) => (
              <LearnPostCard key={clip.id} clip={clip} />
            ))
          ) : (
            <div className="text-center py-12">
              <Search className="w-10 h-10 text-slate-300 mx-auto mb-3" />
              <p className="text-sm text-slate-500">
                {searchQuery.trim()
                  ? "No posts match your search. Try another term or category."
                  : "No posts in this category yet."}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
