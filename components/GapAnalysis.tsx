"use client";

import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  CheckCircle,
  XCircle,
  AlertCircle,
  GraduationCap,
  Clock,
  DollarSign,
  ArrowRight,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface Gap {
  requirement: string;
  status: "satisfied" | "gap" | "partial";
  details: string;
  courseSuggestion?: string;
  coursePrice?: string;
  courseHours?: number;
}

interface GapAnalysisProps {
  currentState: string;
  targetState: string;
  gaps: Gap[];
  overallMatch: number;
}

const statusConfig = {
  satisfied: {
    icon: CheckCircle,
    color: "text-green-600",
    label: "Satisfied",
  },
  gap: {
    icon: XCircle,
    color: "text-red-500",
    label: "Gap",
  },
  partial: {
    icon: AlertCircle,
    color: "text-yellow-500",
    label: "Partial",
  },
};

function getMatchColor(match: number): string {
  if (match >= 80) return "[&_[data-slot=progress-indicator]]:bg-green-500";
  if (match >= 50) return "[&_[data-slot=progress-indicator]]:bg-yellow-500";
  return "[&_[data-slot=progress-indicator]]:bg-red-500";
}

export default function GapAnalysis({
  currentState,
  targetState,
  gaps,
  overallMatch,
}: GapAnalysisProps) {
  const satisfiedCount = gaps.filter((g) => g.status === "satisfied").length;
  const gapCount = gaps.filter((g) => g.status === "gap").length;
  const partialCount = gaps.filter((g) => g.status === "partial").length;

  return (
    <div className="space-y-4">
      {/* Header with match percentage */}
      <Card className="p-4">
        <div className="flex items-center gap-2 text-xs text-slate-500 mb-2">
          <Badge className="bg-slate-100 text-slate-600 text-xs">
            {currentState}
          </Badge>
          <ArrowRight className="w-3.5 h-3.5 text-slate-400" />
          <Badge className="bg-blue-100 text-blue-700 text-xs">
            {targetState}
          </Badge>
        </div>

        <p className="text-sm font-medium text-slate-700 mb-3">
          Your portfolio satisfies{" "}
          <span
            className={cn(
              "font-bold text-base",
              overallMatch >= 80
                ? "text-green-600"
                : overallMatch >= 50
                  ? "text-yellow-600"
                  : "text-red-600"
            )}
          >
            {overallMatch}%
          </span>{" "}
          of {targetState} requirements
        </p>

        <Progress
          value={overallMatch}
          className={cn("h-3", getMatchColor(overallMatch))}
        />

        {/* Summary counts */}
        <div className="flex items-center gap-4 mt-3 text-xs text-slate-500">
          <span className="flex items-center gap-1">
            <CheckCircle className="w-3.5 h-3.5 text-green-500" />
            {satisfiedCount} met
          </span>
          <span className="flex items-center gap-1">
            <AlertCircle className="w-3.5 h-3.5 text-yellow-500" />
            {partialCount} partial
          </span>
          <span className="flex items-center gap-1">
            <XCircle className="w-3.5 h-3.5 text-red-500" />
            {gapCount} gap{gapCount !== 1 ? "s" : ""}
          </span>
        </div>
      </Card>

      {/* Requirements list */}
      <Card className="p-4">
        <h3 className="text-sm font-semibold text-slate-800 mb-3">
          Requirements Breakdown
        </h3>

        <div className="space-y-3">
          {gaps.map((gap, index) => {
            const config = statusConfig[gap.status];
            const StatusIcon = config.icon;

            return (
              <div
                key={index}
                className="border-b border-slate-100 last:border-0 pb-3 last:pb-0"
              >
                <div className="flex items-start gap-2.5">
                  <StatusIcon
                    className={cn("w-5 h-5 mt-0.5 shrink-0", config.color)}
                  />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-slate-800">
                      {gap.requirement}
                    </p>
                    <p className="text-xs text-slate-500 mt-0.5">
                      {gap.details}
                    </p>

                    {/* Course suggestion for gaps */}
                    {gap.courseSuggestion && gap.status !== "satisfied" && (
                      <div className="mt-2 bg-blue-50 rounded-lg px-3 py-2 flex items-start gap-2">
                        <GraduationCap className="w-4 h-4 text-blue-600 mt-0.5 shrink-0" />
                        <div className="text-xs">
                          <p className="font-medium text-blue-800">
                            {gap.courseSuggestion}
                          </p>
                          <div className="flex items-center gap-3 mt-1 text-blue-600">
                            {gap.courseHours && (
                              <span className="flex items-center gap-1">
                                <Clock className="w-3 h-3" />
                                {gap.courseHours}h
                              </span>
                            )}
                            {gap.coursePrice && (
                              <span className="flex items-center gap-1">
                                <DollarSign className="w-3 h-3" />
                                {gap.coursePrice}
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </Card>

      {/* Traditional vs TradeProof path comparison */}
      <Card className="p-4 bg-gradient-to-br from-slate-50 to-blue-50 border-blue-100">
        <h3 className="text-sm font-semibold text-slate-800 mb-3">
          Path Comparison
        </h3>

        <div className="grid grid-cols-2 gap-3">
          {/* Traditional path */}
          <div className="bg-white rounded-xl p-3 border border-slate-200">
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2">
              Traditional Path
            </p>
            <div className="space-y-1.5 text-xs text-slate-600">
              <p>4-5 year apprenticeship</p>
              <p>Classroom hours required</p>
              <p>Single-state license</p>
              <p>Supervisor sign-off</p>
            </div>
          </div>

          {/* TradeProof path */}
          <div className="bg-blue-600 rounded-xl p-3 text-white">
            <p className="text-xs font-semibold text-blue-200 uppercase tracking-wide mb-2">
              TradeProof Path
            </p>
            <div className="space-y-1.5 text-xs text-blue-50">
              <p>Skill-based verification</p>
              <p>Real work documentation</p>
              <p>Multi-state portfolio</p>
              <p>AI-verified compliance</p>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
}
