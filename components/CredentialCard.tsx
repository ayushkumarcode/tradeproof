"use client";

import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Shield,
  TrendingUp,
  TrendingDown,
  Minus,
  MapPin,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface Profile {
  fullName: string;
  trade: string;
  experienceLevel: string;
  jurisdiction: string;
}

interface Stats {
  totalAnalyses: number;
  avgCompliance: number;
  trend: "up" | "stable" | "down";
  strongSkills: string[];
  developingSkills: string[];
  qualifiedStates: string[];
}

interface CredentialCardProps {
  profile: Profile;
  stats: Stats;
}

const trendDisplay = {
  up: { icon: TrendingUp, color: "text-green-500", label: "Improving" },
  stable: { icon: Minus, color: "text-slate-400", label: "Stable" },
  down: { icon: TrendingDown, color: "text-red-500", label: "Declining" },
};

export default function CredentialCard({
  profile,
  stats,
}: CredentialCardProps) {
  const trendInfo = trendDisplay[stats.trend];
  const TrendIcon = trendInfo.icon;

  const complianceColor =
    stats.avgCompliance >= 80
      ? "text-green-600"
      : stats.avgCompliance >= 60
        ? "text-yellow-600"
        : "text-red-600";

  return (
    <Card className="overflow-hidden p-0">
      {/* Dark header */}
      <div className="bg-slate-900 px-5 py-4">
        <div className="flex items-center gap-2 mb-3">
          <Shield className="w-5 h-5 text-blue-400" />
          <span className="text-sm font-semibold text-white tracking-wide">
            TradeProof Verified
          </span>
        </div>
        <h2 className="text-xl font-bold text-white">{profile.fullName}</h2>
        <div className="flex items-center gap-2 mt-1.5">
          <Badge className="bg-blue-600/80 text-blue-100 text-xs border-0">
            {profile.trade}
          </Badge>
          <Badge className="bg-slate-700 text-slate-300 text-xs border-0">
            {profile.experienceLevel}
          </Badge>
        </div>
        <div className="flex items-center gap-1.5 mt-2 text-xs text-slate-400">
          <MapPin className="w-3 h-3" />
          {profile.jurisdiction}
        </div>
      </div>

      {/* Stats grid */}
      <div className="grid grid-cols-3 divide-x divide-slate-200 border-b border-slate-200">
        <div className="px-4 py-3 text-center">
          <p className="text-lg font-bold text-slate-800">
            {stats.totalAnalyses}
          </p>
          <p className="text-xs text-slate-500">Analyses</p>
        </div>
        <div className="px-4 py-3 text-center">
          <p className={cn("text-lg font-bold", complianceColor)}>
            {stats.avgCompliance}%
          </p>
          <p className="text-xs text-slate-500">Avg Compliance</p>
        </div>
        <div className="px-4 py-3 text-center">
          <div className="flex items-center justify-center gap-1">
            <TrendIcon className={cn("w-4 h-4", trendInfo.color)} />
            <span className={cn("text-sm font-bold", trendInfo.color)}>
              {trendInfo.label}
            </span>
          </div>
          <p className="text-xs text-slate-500">Trend</p>
        </div>
      </div>

      {/* Skills sections */}
      <div className="px-5 py-4 space-y-4">
        {/* Strong skills */}
        {stats.strongSkills.length > 0 && (
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2">
              Strong Skills
            </p>
            <div className="flex flex-wrap gap-1.5">
              {stats.strongSkills.map((skill) => (
                <Badge
                  key={skill}
                  className="bg-green-100 text-green-800 text-xs"
                >
                  {skill}
                </Badge>
              ))}
            </div>
          </div>
        )}

        {/* Developing skills */}
        {stats.developingSkills.length > 0 && (
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2">
              Developing Skills
            </p>
            <div className="flex flex-wrap gap-1.5">
              {stats.developingSkills.map((skill) => (
                <Badge
                  key={skill}
                  className="bg-yellow-100 text-yellow-800 text-xs"
                >
                  {skill}
                </Badge>
              ))}
            </div>
          </div>
        )}

        {/* Qualified states */}
        {stats.qualifiedStates.length > 0 && (
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2">
              Qualified Jurisdictions
            </p>
            <div className="flex flex-wrap gap-1.5">
              {stats.qualifiedStates.map((state) => (
                <Badge
                  key={state}
                  className="bg-slate-100 text-slate-700 text-xs gap-1"
                >
                  <MapPin className="w-3 h-3" />
                  {state}
                </Badge>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Footer */}
      <div className="border-t border-slate-200 px-5 py-3 bg-slate-50">
        <div className="flex items-center justify-center gap-1.5 text-xs text-slate-500">
          <Shield className="w-3.5 h-3.5 text-blue-500" />
          Verified by TradeProof AI
        </div>
      </div>
    </Card>
  );
}
