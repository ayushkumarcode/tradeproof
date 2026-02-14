"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { formatFeedDate } from "@/lib/utils";
import type { KnowledgeClip, PostType } from "@/data/knowledge-clips";
import Image from "next/image";
import { ChevronDown, ChevronUp } from "lucide-react";

const PREVIEW_LENGTH = 220;

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/);
  if (parts.length === 0) return "";
  if (parts.length === 1) return parts[0][0].toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

function formatTaskType(taskType: string): string {
  return taskType
    .split("_")
    .map((w) => w.charAt(0).toUpperCase() + w.slice(1))
    .join(" ");
}

/** Truncate at last space before limit so we don't cut mid-word */
function truncateAt(content: string, maxLen: number): string {
  if (content.length <= maxLen) return content;
  const cut = content.slice(0, maxLen);
  const lastSpace = cut.lastIndexOf(" ");
  return lastSpace > maxLen * 0.6 ? cut.slice(0, lastSpace) : cut;
}

interface LearnPostCardProps {
  clip: KnowledgeClip;
}

export default function LearnPostCard({ clip }: LearnPostCardProps) {
  const [expanded, setExpanded] = useState(false);
  const hasImage = !!clip.imageUrl;
  const postType: PostType = clip.postType ?? "tip";
  const content = clip.content;
  const isLong = content.length > PREVIEW_LENGTH;
  const showPreview = isLong && !expanded;
  const displayContent = showPreview ? truncateAt(content, PREVIEW_LENGTH) : content;

  return (
    <Card
      className={
        hasImage
          ? "overflow-hidden bg-white border-slate-200 p-0"
          : "bg-white border-slate-200 p-4"
      }
    >
      {/* Hero image for article-style posts */}
      {hasImage && (
        <div className="relative w-full aspect-[16/10] bg-slate-100">
          <Image
            src={clip.imageUrl!}
            alt=""
            fill
            className="object-cover"
            sizes="(max-width: 600px) 100vw, 400px"
          />
          <Badge className="absolute top-2 left-2 bg-black/60 text-white text-xs border-0">
            {formatTaskType(clip.taskType)}
          </Badge>
        </div>
      )}

      <div className={hasImage ? "p-4" : ""}>
        {/* Author row: avatar, name, experience, date */}
        <div className="flex items-center gap-3 mb-3">
          <div className="w-10 h-10 rounded-full bg-slate-700 text-white flex items-center justify-center text-sm font-semibold shrink-0">
            {getInitials(clip.expertName)}
          </div>
          <div className="min-w-0 flex-1">
            <p className="text-sm font-semibold text-slate-900 truncate">
              {clip.expertName}
            </p>
            <p className="text-xs text-slate-500">
              {clip.expertYears} yrs · {formatFeedDate(clip.publishedAt)}
            </p>
          </div>
          {!hasImage && (
            <Badge className="bg-slate-100 text-slate-600 text-xs shrink-0">
              {formatTaskType(clip.taskType)}
            </Badge>
          )}
        </div>

        {/* Title */}
        <h3 className="text-sm font-semibold text-slate-900 mb-2 leading-snug">
          {clip.title}
        </h3>

        {/* Content — collapsible when long */}
        <div
          className={
            postType === "story"
              ? "relative pl-4"
              : "text-slate-700 text-sm leading-relaxed"
          }
        >
          {postType === "story" && (
            <span
              className="absolute top-0 left-0 text-2xl leading-none text-slate-300 font-serif select-none"
              aria-hidden="true"
            >
              &ldquo;
            </span>
          )}
          <p
            className={
              postType === "story"
                ? "text-sm text-slate-600 italic leading-relaxed"
                : ""
            }
          >
            {displayContent}
            {showPreview && "…"}
          </p>
          {isLong && (
            <button
              type="button"
              onClick={() => setExpanded((e) => !e)}
              className="mt-2 flex items-center gap-1 text-xs font-medium text-blue-600 hover:text-blue-700 hover:underline focus:outline-none focus:underline"
            >
              {expanded ? (
                <>
                  <ChevronUp className="w-3.5 h-3.5" />
                  Show less
                </>
              ) : (
                <>
                  <ChevronDown className="w-3.5 h-3.5" />
                  Read more
                </>
              )}
            </button>
          )}
        </div>
      </div>
    </Card>
  );
}
