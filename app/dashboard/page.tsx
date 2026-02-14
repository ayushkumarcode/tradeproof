"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import SkillRadar from "@/components/SkillRadar";
import ComplianceTrend from "@/components/ComplianceTrend";
import {
  Shield,
  TrendingUp,
  TrendingDown,
  Minus,
  CheckCircle,
  AlertTriangle,
  ArrowRight,
  BarChart3,
} from "lucide-react";
import {
  getProfile,
  getAnalyses,
  getSkillScores,
} from "@/lib/storage";
import type { Analysis, SkillScore, UserProfile } from "@/lib/storage";

export default function DashboardPage() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [analyses, setAnalyses] = useState<Analysis[]>([]);
  const [skillScores, setSkillScores] = useState<SkillScore[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setProfile(getProfile());
    setAnalyses(getAnalyses());
    setSkillScores(getSkillScores());
    setLoading(false);
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-pulse text-slate-400">Loading...</div>
      </div>
    );
  }

  const totalAnalyses = analyses.length;
  const avgScore =
    totalAnalyses > 0
      ? Math.round(
          analyses.reduce((sum, a) => sum + a.complianceScore, 0) /
            totalAnalyses
        )
      : 0;

  // Calculate trend from last 5 vs first 5
  let trend: "up" | "stable" | "down" = "stable";
  if (analyses.length >= 4) {
    const recentAvg =
      analyses
        .slice(0, Math.min(3, analyses.length))
        .reduce((sum, a) => sum + a.complianceScore, 0) /
      Math.min(3, analyses.length);
    const oldAvg =
      analyses
        .slice(-Math.min(3, analyses.length))
        .reduce((sum, a) => sum + a.complianceScore, 0) /
      Math.min(3, analyses.length);
    if (recentAvg - oldAvg > 5) trend = "up";
    else if (oldAvg - recentAvg > 5) trend = "down";
  }

  const trendIcon =
    trend === "up"
      ? TrendingUp
      : trend === "down"
      ? TrendingDown
      : Minus;
  const TrendIcon = trendIcon;
  const trendColor =
    trend === "up"
      ? "text-green-600"
      : trend === "down"
      ? "text-red-600"
      : "text-slate-400";
  const trendLabel =
    trend === "up" ? "Improving" : trend === "down" ? "Declining" : "Stable";

  // Map skill scores for SkillRadar component format
  const mappedSkills = skillScores.map((s) => ({
    skillName: s.skillName,
    score: s.score,
    totalInstances: s.totalInstances,
    trend: (s.trend === "improving"
      ? "up"
      : s.trend === "declining"
      ? "down"
      : "stable") as "up" | "stable" | "down",
  }));

  // Map analyses for ComplianceTrend
  const trendData = [...analyses]
    .sort(
      (a, b) =>
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
    )
    .map((a) => ({
      date: a.createdAt,
      score: a.complianceScore,
    }));

  // Recent analyses (last 5)
  const recentAnalyses = analyses.slice(0, 5);

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="max-w-lg mx-auto px-4 pt-6 pb-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-xl font-bold text-slate-900">Your Portfolio</h1>
            {profile && (
              <p className="text-sm text-slate-500">{profile.fullName}</p>
            )}
          </div>
          <div className="flex items-center gap-1.5">
            <Shield className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-blue-600">
              TradeProof
            </span>
          </div>
        </div>

        {/* Stats Row */}
        <div className="grid grid-cols-3 gap-3 mb-6">
          <Card>
            <CardContent className="pt-4 pb-4 text-center">
              <p className="text-2xl font-bold text-slate-800">
                {totalAnalyses}
              </p>
              <p className="text-xs text-slate-500">Analyses</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-4 pb-4 text-center">
              <p
                className={`text-2xl font-bold ${
                  avgScore >= 80
                    ? "text-green-600"
                    : avgScore >= 60
                    ? "text-yellow-600"
                    : "text-red-600"
                }`}
              >
                {avgScore}%
              </p>
              <p className="text-xs text-slate-500">Avg Score</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-4 pb-4 text-center">
              <div className="flex items-center justify-center gap-1">
                <TrendIcon className={`w-5 h-5 ${trendColor}`} />
              </div>
              <p className="text-xs text-slate-500 mt-1">{trendLabel}</p>
            </CardContent>
          </Card>
        </div>

        {/* Skill Radar */}
        <div className="mb-6">
          <SkillRadar skills={mappedSkills} />
        </div>

        {/* Compliance Trend */}
        <div className="mb-6">
          <ComplianceTrend analyses={trendData} />
        </div>

        {/* Recent Analyses */}
        <div>
          <h3 className="text-sm font-semibold text-slate-800 mb-3 flex items-center gap-2">
            <BarChart3 className="w-4 h-4 text-blue-600" />
            Recent Analyses
          </h3>
          <div className="space-y-2">
            {recentAnalyses.length === 0 ? (
              <Card>
                <CardContent className="pt-6 text-center">
                  <p className="text-sm text-slate-500">
                    No analyses yet. Check your first piece of work!
                  </p>
                  <Link href="/analyze">
                    <Badge className="bg-blue-100 text-blue-700 mt-3 cursor-pointer">
                      Start Analysis
                    </Badge>
                  </Link>
                </CardContent>
              </Card>
            ) : (
              recentAnalyses.map((a) => (
                <Link key={a.id} href={`/results/${a.id}`}>
                  <Card className="hover:border-blue-200 transition-colors cursor-pointer mb-2">
                    <CardContent className="pt-4 pb-4">
                      <div className="flex items-center justify-between">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <span className="text-sm font-medium text-slate-800 capitalize">
                              {a.workType.replace(/_/g, " ")}
                            </span>
                            <Badge
                              className={
                                a.isCompliant
                                  ? "bg-green-100 text-green-700 text-[10px]"
                                  : "bg-red-100 text-red-700 text-[10px]"
                              }
                            >
                              {a.isCompliant ? (
                                <CheckCircle className="w-3 h-3 mr-0.5" />
                              ) : (
                                <AlertTriangle className="w-3 h-3 mr-0.5" />
                              )}
                              {a.complianceScore}%
                            </Badge>
                          </div>
                          <p className="text-xs text-slate-400">
                            {new Date(a.createdAt).toLocaleDateString("en-US", {
                              month: "short",
                              day: "numeric",
                              year: "numeric",
                            })}
                          </p>
                        </div>
                        <ArrowRight className="w-4 h-4 text-slate-300 shrink-0" />
                      </div>
                    </CardContent>
                  </Card>
                </Link>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
