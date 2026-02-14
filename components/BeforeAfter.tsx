"use client";

import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  CheckCircle,
  XCircle,
  AlertCircle,
  AlertTriangle,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface ViolationStatus {
  description: string;
  status: "resolved" | "unresolved" | "partially_resolved";
}

interface RecheckResult {
  originalViolationStatus: ViolationStatus[];
  newViolationsFound: string[];
  complianceScore: number;
  isCompliant: boolean;
}

interface BeforeAfterProps {
  beforeImage: string;
  afterImage: string;
  originalViolations: string[];
  recheckResult: RecheckResult;
}

const statusConfig = {
  resolved: {
    icon: CheckCircle,
    label: "Resolved",
    badgeBg: "bg-green-100 text-green-800",
    iconColor: "text-green-600",
  },
  unresolved: {
    icon: XCircle,
    label: "Unresolved",
    badgeBg: "bg-red-100 text-red-800",
    iconColor: "text-red-600",
  },
  partially_resolved: {
    icon: AlertCircle,
    label: "Partially",
    badgeBg: "bg-yellow-100 text-yellow-800",
    iconColor: "text-yellow-600",
  },
};

export default function BeforeAfter({
  beforeImage,
  afterImage,
  originalViolations,
  recheckResult,
}: BeforeAfterProps) {
  const resolvedCount = recheckResult.originalViolationStatus.filter(
    (v) => v.status === "resolved"
  ).length;
  const totalOriginal = recheckResult.originalViolationStatus.length;

  const scoreColor =
    recheckResult.complianceScore >= 80
      ? "text-green-600"
      : recheckResult.complianceScore >= 60
        ? "text-yellow-600"
        : "text-red-600";

  return (
    <div className="space-y-4">
      {/* Side-by-side images */}
      <Card className="p-3">
        <div className="flex flex-col md:flex-row gap-3">
          {/* Before */}
          <div className="flex-1">
            <div className="relative rounded-xl overflow-hidden border border-slate-200">
              <img
                src={beforeImage}
                alt="Before - original work photo"
                className="w-full h-auto max-h-56 object-contain bg-slate-50"
              />
              <div className="absolute top-2 left-2">
                <Badge className="bg-slate-800/80 text-white text-xs">
                  Before
                </Badge>
              </div>
            </div>
          </div>

          {/* After */}
          <div className="flex-1">
            <div className="relative rounded-xl overflow-hidden border border-slate-200">
              <img
                src={afterImage}
                alt="After - corrected work photo"
                className="w-full h-auto max-h-56 object-contain bg-slate-50"
              />
              <div className="absolute top-2 left-2">
                <Badge className="bg-blue-600/90 text-white text-xs">
                  After
                </Badge>
              </div>
            </div>
          </div>
        </div>
      </Card>

      {/* Violation status list */}
      <Card className="p-4">
        <h3 className="text-sm font-semibold text-slate-800 mb-1">
          Violation Status
        </h3>
        <p className="text-xs text-slate-500 mb-3">
          {resolvedCount} of {totalOriginal} resolved
        </p>

        <div className="space-y-2.5">
          {recheckResult.originalViolationStatus.map((item, index) => {
            const config = statusConfig[item.status];
            const StatusIcon = config.icon;

            return (
              <div
                key={index}
                className="flex items-start gap-2.5 py-2 border-b border-slate-100 last:border-0"
              >
                <StatusIcon
                  className={cn("w-5 h-5 mt-0.5 shrink-0", config.iconColor)}
                />
                <div className="flex-1 min-w-0">
                  <p className="text-sm text-slate-700">{item.description}</p>
                </div>
                <Badge className={cn("text-xs shrink-0", config.badgeBg)}>
                  {config.label}
                </Badge>
              </div>
            );
          })}
        </div>
      </Card>

      {/* New violations found */}
      {recheckResult.newViolationsFound.length > 0 && (
        <Card className="p-4 border-l-4 border-l-orange-400">
          <h3 className="text-sm font-semibold text-orange-800 flex items-center gap-2 mb-2">
            <AlertTriangle className="w-4 h-4" />
            New Issues Found
          </h3>
          <ul className="space-y-1.5">
            {recheckResult.newViolationsFound.map((violation, index) => (
              <li
                key={index}
                className="flex items-start gap-2 text-sm text-slate-700"
              >
                <span className="text-orange-400 mt-1 shrink-0">&#8226;</span>
                {violation}
              </li>
            ))}
          </ul>
        </Card>
      )}

      {/* Final compliance score */}
      <Card
        className={cn(
          "p-5 text-center",
          recheckResult.isCompliant
            ? "bg-green-50 border-green-200"
            : "bg-slate-50"
        )}
      >
        <p className="text-xs font-medium text-slate-500 uppercase tracking-wide mb-1">
          Updated Compliance Score
        </p>
        <p className={cn("text-4xl font-bold", scoreColor)}>
          {recheckResult.complianceScore}
          <span className="text-lg text-slate-400">/100</span>
        </p>
        {recheckResult.isCompliant ? (
          <Badge className="bg-green-100 text-green-800 text-sm mt-2 gap-1 px-3 py-1">
            <CheckCircle className="w-4 h-4" />
            Compliant
          </Badge>
        ) : (
          <p className="text-sm text-slate-500 mt-2">
            Fix remaining issues and re-check again
          </p>
        )}
      </Card>
    </div>
  );
}
