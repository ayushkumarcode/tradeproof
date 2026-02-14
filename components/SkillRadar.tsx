"use client";

import { Progress } from "@/components/ui/progress";
import { Card } from "@/components/ui/card";
import { TrendingUp, Minus, TrendingDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface Skill {
  skillName: string;
  score: number;
  totalInstances: number;
  trend: "up" | "stable" | "down";
}

interface SkillRadarProps {
  skills: Skill[];
}

const trendConfig = {
  up: {
    icon: TrendingUp,
    color: "text-green-600",
    label: "Improving",
  },
  stable: {
    icon: Minus,
    color: "text-slate-400",
    label: "Stable",
  },
  down: {
    icon: TrendingDown,
    color: "text-red-500",
    label: "Declining",
  },
};

function getProgressColor(score: number): string {
  if (score > 85) return "[&_[data-slot=progress-indicator]]:bg-green-500";
  if (score >= 70) return "[&_[data-slot=progress-indicator]]:bg-yellow-500";
  return "[&_[data-slot=progress-indicator]]:bg-red-500";
}

function getScoreTextColor(score: number): string {
  if (score > 85) return "text-green-600";
  if (score >= 70) return "text-yellow-600";
  return "text-red-600";
}

export default function SkillRadar({ skills }: SkillRadarProps) {
  const sortedSkills = [...skills].sort((a, b) => b.score - a.score);

  return (
    <Card className="p-4">
      <h3 className="text-sm font-semibold text-slate-800 mb-4">
        Skills Overview
      </h3>

      <div className="space-y-4">
        {sortedSkills.map((skill) => {
          const trend = trendConfig[skill.trend];
          const TrendIcon = trend.icon;

          return (
            <div key={skill.skillName} className="space-y-1.5">
              {/* Skill name row */}
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-slate-700">
                    {skill.skillName}
                  </span>
                  <TrendIcon
                    className={cn("w-3.5 h-3.5", trend.color)}
                    aria-label={trend.label}
                  />
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-slate-400">
                    ({skill.totalInstances} check
                    {skill.totalInstances !== 1 ? "s" : ""})
                  </span>
                  <span
                    className={cn(
                      "text-sm font-semibold tabular-nums",
                      getScoreTextColor(skill.score)
                    )}
                  >
                    {skill.score}%
                  </span>
                </div>
              </div>

              {/* Progress bar */}
              <Progress
                value={skill.score}
                className={cn("h-2.5", getProgressColor(skill.score))}
              />
            </div>
          );
        })}
      </div>

      {skills.length === 0 && (
        <p className="text-sm text-slate-400 text-center py-6">
          Complete analyses to build your skill profile
        </p>
      )}
    </Card>
  );
}
