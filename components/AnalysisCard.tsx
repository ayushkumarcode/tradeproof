"use client";

import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  AlertTriangle,
  AlertCircle,
  Info,
  ChevronDown,
  ChevronUp,
} from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";

interface Violation {
  description: string;
  codeSection: string;
  localAmendment?: string;
  severity: "critical" | "major" | "minor";
  confidence: "high" | "medium" | "low";
  fixInstruction: string;
  whyThisMatters: string;
  visualEvidence?: string;
}

const severityConfig = {
  critical: {
    border: "border-l-red-500",
    bg: "bg-red-50",
    badgeBg: "bg-red-100 text-red-800",
    icon: AlertTriangle,
    label: "Critical",
  },
  major: {
    border: "border-l-orange-500",
    bg: "bg-orange-50",
    badgeBg: "bg-orange-100 text-orange-800",
    icon: AlertCircle,
    label: "Major",
  },
  minor: {
    border: "border-l-yellow-500",
    bg: "bg-yellow-50",
    badgeBg: "bg-yellow-100 text-yellow-800",
    icon: Info,
    label: "Minor",
  },
};

const confidenceConfig = {
  high: "bg-green-100 text-green-800",
  medium: "bg-yellow-100 text-yellow-800",
  low: "bg-slate-100 text-slate-600",
};

export default function AnalysisCard({
  violation,
}: {
  violation: Violation;
}) {
  const [expanded, setExpanded] = useState(false);

  const severity = severityConfig[violation.severity];
  const SeverityIcon = severity.icon;

  return (
    <Card
      className={cn(
        "border-l-4 p-4 gap-3",
        severity.border
      )}
    >
      {/* Header row: severity + confidence */}
      <div className="flex items-center justify-between gap-2">
        <Badge className={cn("text-xs font-medium", severity.badgeBg)}>
          <SeverityIcon className="w-3 h-3" />
          {severity.label}
        </Badge>
        <Badge className={cn("text-xs", confidenceConfig[violation.confidence])}>
          {violation.confidence} confidence
        </Badge>
      </div>

      {/* Description */}
      <p className="text-sm font-medium text-slate-800">
        {violation.description}
      </p>

      {/* Code section */}
      <div className="flex items-center gap-2">
        <span className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
          Code
        </span>
        <span className="text-sm font-mono font-semibold text-blue-700 bg-blue-50 px-2 py-0.5 rounded">
          {violation.codeSection}
        </span>
      </div>

      {/* Local amendment if present */}
      {violation.localAmendment && (
        <p className="text-xs text-slate-500 italic">
          Local amendment: {violation.localAmendment}
        </p>
      )}

      {/* Visual evidence */}
      {violation.visualEvidence && (
        <p className="text-xs text-slate-600 bg-slate-50 px-3 py-2 rounded-lg">
          <span className="font-medium">Visual evidence:</span>{" "}
          {violation.visualEvidence}
        </p>
      )}

      {/* Fix instruction - highlighted */}
      <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
        <p className="text-xs font-semibold text-amber-800 mb-0.5">
          How to Fix
        </p>
        <p className="text-sm text-amber-900">{violation.fixInstruction}</p>
      </div>

      {/* Expandable "Why This Matters" */}
      <button
        onClick={() => setExpanded(!expanded)}
        className="flex items-center gap-1 text-xs font-medium text-slate-500 hover:text-slate-700 transition-colors"
      >
        {expanded ? (
          <ChevronUp className="w-3.5 h-3.5" />
        ) : (
          <ChevronDown className="w-3.5 h-3.5" />
        )}
        Why This Matters
      </button>

      {expanded && (
        <div className="text-sm text-slate-600 bg-slate-50 px-3 py-2 rounded-lg animate-in fade-in slide-in-from-top-1 duration-200">
          {violation.whyThisMatters}
        </div>
      )}
    </Card>
  );
}
