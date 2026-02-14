"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  CheckCircle,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  MapPin,
  RefreshCw,
} from "lucide-react";
import { cn } from "@/lib/utils";
import AnalysisCard from "@/components/AnalysisCard";

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

interface Analysis {
  description: string;
  isCompliant: boolean;
  complianceScore: number;
  violations: Violation[];
  correctItems: string[];
  skillsDemonstrated: string[];
  overallAssessment: string;
}

interface AnalysisResultsProps {
  analysis: Analysis;
  onRecheck: () => void;
  photoUrl: string;
}

function ScoreCircle({ score }: { score: number }) {
  const radius = 50;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (score / 100) * circumference;

  const color =
    score >= 80
      ? "text-green-500"
      : score >= 60
        ? "text-yellow-500"
        : "text-red-500";

  const strokeColor =
    score >= 80
      ? "stroke-green-500"
      : score >= 60
        ? "stroke-yellow-500"
        : "stroke-red-500";

  return (
    <div className="relative flex items-center justify-center w-32 h-32 mx-auto">
      <svg className="w-32 h-32 -rotate-90" viewBox="0 0 120 120">
        <circle
          cx="60"
          cy="60"
          r={radius}
          fill="none"
          stroke="currentColor"
          strokeWidth="8"
          className="text-slate-200"
        />
        <circle
          cx="60"
          cy="60"
          r={radius}
          fill="none"
          strokeWidth="8"
          strokeLinecap="round"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          className={cn(strokeColor, "transition-all duration-1000 ease-out")}
        />
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center">
        <span className={cn("text-3xl font-bold", color)}>{score}</span>
        <span className="text-xs text-slate-500">/ 100</span>
      </div>
    </div>
  );
}

export default function AnalysisResults({
  analysis,
  onRecheck,
  photoUrl,
}: AnalysisResultsProps) {
  const [showCorrectItems, setShowCorrectItems] = useState(false);

  const violationCount = analysis.violations.length;
  const criticalCount = analysis.violations.filter(
    (v) => v.severity === "critical"
  ).length;

  return (
    <div className="space-y-4">
      {/* Location chip */}
      <div className="flex justify-center">
        <Badge className="bg-slate-100 text-slate-600 text-xs gap-1">
          <MapPin className="w-3 h-3" />
          San Jose, CA
        </Badge>
      </div>

      {/* Score section */}
      <Card className="p-5 text-center">
        <ScoreCircle score={analysis.complianceScore} />

        <div className="mt-3">
          {analysis.isCompliant ? (
            <Badge className="bg-green-100 text-green-800 text-sm gap-1 px-3 py-1">
              <CheckCircle className="w-4 h-4" />
              Compliant
            </Badge>
          ) : (
            <Badge className="bg-red-100 text-red-800 text-sm gap-1 px-3 py-1">
              <AlertTriangle className="w-4 h-4" />
              {violationCount} Violation{violationCount !== 1 ? "s" : ""} Found
              {criticalCount > 0 && ` (${criticalCount} critical)`}
            </Badge>
          )}
        </div>

        <p className="text-sm text-slate-600 mt-3">{analysis.description}</p>
      </Card>

      {/* Photo preview */}
      {photoUrl && (
        <Card className="p-3">
          <div className="rounded-xl overflow-hidden border border-slate-200">
            <img
              src={photoUrl}
              alt="Analyzed work photo"
              className="w-full h-auto max-h-48 object-contain bg-slate-50"
            />
          </div>
        </Card>
      )}

      {/* Violations list */}
      {analysis.violations.length > 0 && (
        <div className="space-y-3">
          <h3 className="text-sm font-semibold text-slate-800 px-1">
            Violations
          </h3>
          {analysis.violations.map((violation, index) => (
            <AnalysisCard key={index} violation={violation} />
          ))}
        </div>
      )}

      {/* What you did right - collapsible */}
      {analysis.correctItems.length > 0 && (
        <Card className="p-4">
          <button
            onClick={() => setShowCorrectItems(!showCorrectItems)}
            className="flex items-center justify-between w-full"
          >
            <h3 className="text-sm font-semibold text-green-800 flex items-center gap-2">
              <CheckCircle className="w-4 h-4 text-green-600" />
              What You Did Right ({analysis.correctItems.length})
            </h3>
            {showCorrectItems ? (
              <ChevronUp className="w-4 h-4 text-slate-400" />
            ) : (
              <ChevronDown className="w-4 h-4 text-slate-400" />
            )}
          </button>

          {showCorrectItems && (
            <ul className="mt-3 space-y-2">
              {analysis.correctItems.map((item, index) => (
                <li
                  key={index}
                  className="flex items-start gap-2 text-sm text-slate-700"
                >
                  <CheckCircle className="w-4 h-4 text-green-500 mt-0.5 shrink-0" />
                  {item}
                </li>
              ))}
            </ul>
          )}
        </Card>
      )}

      {/* Skills demonstrated */}
      {analysis.skillsDemonstrated.length > 0 && (
        <Card className="p-4">
          <h3 className="text-sm font-semibold text-slate-800 mb-2">
            Skills Demonstrated
          </h3>
          <div className="flex flex-wrap gap-1.5">
            {analysis.skillsDemonstrated.map((skill, index) => (
              <Badge
                key={index}
                className="bg-blue-50 text-blue-700 text-xs"
              >
                {skill}
              </Badge>
            ))}
          </div>
        </Card>
      )}

      {/* Overall assessment */}
      <Card className="p-4">
        <h3 className="text-sm font-semibold text-slate-800 mb-2">
          Overall Assessment
        </h3>
        <p className="text-sm text-slate-600 leading-relaxed">
          {analysis.overallAssessment}
        </p>
      </Card>

      {/* Re-check button */}
      {!analysis.isCompliant && (
        <Button
          onClick={onRecheck}
          className="w-full h-14 text-base bg-blue-600 hover:bg-blue-700 active:bg-blue-800 text-white rounded-xl flex items-center justify-center gap-2"
        >
          <RefreshCw className="w-5 h-5" />
          I&apos;ve Fixed It &mdash; Re-check
        </Button>
      )}
    </div>
  );
}
