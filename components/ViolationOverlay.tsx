"use client";

import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface ViolationMarker {
  id: number;
  description: string;
  codeSection: string;
  severity: "critical" | "major" | "minor";
  fixInstruction: string;
  locationX: number;
  locationY: number;
}

interface ViolationOverlayProps {
  image: string;
  violations: ViolationMarker[];
  label?: string;
}

const severityColors = {
  critical: {
    bg: "bg-red-500",
    ring: "ring-red-300",
    pulse: "bg-red-400",
  },
  major: {
    bg: "bg-orange-500",
    ring: "ring-orange-300",
    pulse: "bg-orange-400",
  },
  minor: {
    bg: "bg-yellow-500",
    ring: "ring-yellow-300",
    pulse: "bg-yellow-400",
  },
};

export default function ViolationOverlay({
  image,
  violations,
  label = "After",
}: ViolationOverlayProps) {
  const [activeMarker, setActiveMarker] = useState<number | null>(null);

  return (
    <div className="relative rounded-xl overflow-hidden border border-slate-200">
      <img
        src={image}
        alt={`${label} photo with violation markers`}
        className="w-full h-auto max-h-80 object-contain bg-slate-50"
      />

      {/* Label badge */}
      <div className="absolute top-2 left-2">
        <Badge className="bg-blue-600/90 text-white text-xs">{label}</Badge>
      </div>

      {/* Violation markers */}
      {violations.map((v) => {
        const colors = severityColors[v.severity];
        const isActive = activeMarker === v.id;

        return (
          <button
            key={v.id}
            onClick={() => setActiveMarker(isActive ? null : v.id)}
            className="absolute group"
            style={{
              left: `${v.locationX}%`,
              top: `${v.locationY}%`,
              transform: "translate(-50%, -50%)",
            }}
          >
            {/* Pulse animation ring */}
            <span
              className={cn(
                "absolute inset-0 rounded-full animate-ping opacity-40",
                colors.pulse
              )}
              style={{ width: "28px", height: "28px", margin: "-4px" }}
            />

            {/* Marker dot */}
            <span
              className={cn(
                "relative flex items-center justify-center w-6 h-6 rounded-full text-white text-xs font-bold ring-2 shadow-lg cursor-pointer transition-transform",
                colors.bg,
                colors.ring,
                isActive && "scale-125"
              )}
            >
              {v.id}
            </span>

            {/* Tooltip on click */}
            {isActive && (
              <div className="absolute z-20 bottom-full left-1/2 -translate-x-1/2 mb-2 w-56 bg-white rounded-lg shadow-xl border border-slate-200 p-3 text-left">
                <div className="flex items-center gap-1.5 mb-1">
                  <Badge
                    className={cn(
                      "text-xs",
                      v.severity === "critical"
                        ? "bg-red-100 text-red-800"
                        : v.severity === "major"
                          ? "bg-orange-100 text-orange-800"
                          : "bg-yellow-100 text-yellow-800"
                    )}
                  >
                    {v.severity}
                  </Badge>
                  <span className="text-xs font-mono font-semibold text-blue-700">
                    {v.codeSection}
                  </span>
                </div>
                <p className="text-xs text-slate-700 leading-relaxed">
                  {v.description}
                </p>
                {/* Arrow */}
                <div className="absolute top-full left-1/2 -translate-x-1/2 w-2 h-2 bg-white border-r border-b border-slate-200 rotate-45 -mt-1" />
              </div>
            )}
          </button>
        );
      })}

      {/* Violation count badge */}
      {violations.length > 0 && (
        <div className="absolute top-2 right-2">
          <Badge className="bg-red-500/90 text-white text-xs gap-1">
            {violations.length} issue{violations.length !== 1 ? "s" : ""}
          </Badge>
        </div>
      )}
    </div>
  );
}
