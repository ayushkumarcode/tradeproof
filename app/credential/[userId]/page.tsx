"use client";

import { useState, useEffect, useMemo } from "react";
import { useParams } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import CredentialCard from "@/components/CredentialCard";
import {
  Shield,
  Share2,
  Copy,
  CheckCircle,
  Loader2,
  User,
  ArrowLeft,
  MapPin,
  Briefcase,
  Award,
} from "lucide-react";
import {
  getProfile,
  getAnalyses,
  getSkillScores,
} from "@/lib/storage";
import type { Analysis, SkillScore, UserProfile } from "@/lib/storage";
import Link from "next/link";

export default function CredentialPage() {
  const params = useParams();
  const userId = params.userId as string;

  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [analyses, setAnalyses] = useState<Analysis[]>([]);
  const [skillScores, setSkillScores] = useState<SkillScore[]>([]);
  const [loading, setLoading] = useState(true);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    const p = getProfile();
    if (p && p.id === userId) {
      setProfile(p);
    } else {
      // Fallback: use whatever profile is available
      setProfile(getProfile());
    }
    setAnalyses(getAnalyses());
    setSkillScores(getSkillScores());
    setLoading(false);
  }, [userId]);

  // Compute stats
  const stats = useMemo(() => {
    const totalAnalyses = analyses.length;
    const avgCompliance =
      totalAnalyses > 0
        ? Math.round(
            analyses.reduce((sum, a) => sum + a.complianceScore, 0) /
              totalAnalyses
          )
        : 0;

    // Trend from last few vs first few
    let trend: "up" | "stable" | "down" = "stable";
    if (analyses.length >= 4) {
      const recentAvg =
        analyses.slice(0, 3).reduce((sum, a) => sum + a.complianceScore, 0) / 3;
      const oldAvg =
        analyses.slice(-3).reduce((sum, a) => sum + a.complianceScore, 0) / 3;
      if (recentAvg - oldAvg > 5) trend = "up";
      else if (oldAvg - recentAvg > 5) trend = "down";
    }

    // Strong skills (score >= 85)
    const strongSkills = skillScores
      .filter((s) => s.score >= 85)
      .map((s) => s.skillName);

    // Developing skills (score < 85)
    const developingSkills = skillScores
      .filter((s) => s.score < 85)
      .map((s) => s.skillName);

    // Qualified states (California + any with high match)
    const qualifiedStates = ["California"];

    return {
      totalAnalyses,
      avgCompliance,
      trend,
      strongSkills,
      developingSkills,
      qualifiedStates,
    };
  }, [analyses, skillScores]);

  async function handleCopyLink() {
    try {
      const url = window.location.href;
      await navigator.clipboard.writeText(url);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // Fallback for older browsers
      const url = window.location.href;
      const input = document.createElement("input");
      input.value = url;
      document.body.appendChild(input);
      input.select();
      document.execCommand("copy");
      document.body.removeChild(input);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-lg mx-auto px-4 pt-8 text-center">
          <User className="w-12 h-12 text-slate-300 mx-auto mb-4" />
          <h2 className="text-lg font-semibold text-slate-800 mb-2">
            Profile Not Found
          </h2>
          <p className="text-sm text-slate-500 mb-6">
            This credential profile could not be loaded.
          </p>
          <Link href="/">
            <Button className="bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              Go Home
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  const credentialProfile = {
    fullName: profile.fullName,
    trade: profile.trade,
    experienceLevel: profile.experienceLevel,
    jurisdiction: profile.primaryJurisdiction,
  };

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="max-w-lg mx-auto px-4 pt-4 pb-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <Link
            href="/dashboard"
            className="flex items-center gap-1 text-slate-500 hover:text-slate-700 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            <span className="text-sm">Back</span>
          </Link>
          <div className="flex items-center gap-1.5">
            <Shield className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-slate-800">
              Credential
            </span>
          </div>
          <div className="w-12" />
        </div>

        {/* Before/After Comparison */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle className="text-base text-center">
              Traditional Resume vs TradeProof Credential
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-3">
              {/* Traditional Resume */}
              <div className="bg-slate-100 rounded-xl p-4 border border-slate-200">
                <div className="flex items-center gap-1.5 mb-3">
                  <Briefcase className="w-4 h-4 text-slate-500" />
                  <span className="text-xs font-semibold text-slate-500 uppercase">
                    Resume
                  </span>
                </div>
                <div className="space-y-2.5 text-xs text-slate-600">
                  <div>
                    <p className="font-medium text-slate-700">
                      {profile.fullName}
                    </p>
                    <p className="capitalize">{profile.trade} Apprentice</p>
                  </div>
                  <div className="border-t border-slate-200 pt-2">
                    <p className="font-medium text-slate-500 mb-1">
                      Experience
                    </p>
                    <p>&quot;1 year electrical experience&quot;</p>
                  </div>
                  <div className="border-t border-slate-200 pt-2">
                    <p className="font-medium text-slate-500 mb-1">Skills</p>
                    <p>&quot;Familiar with NEC code&quot;</p>
                  </div>
                  <div className="text-center pt-2">
                    <Badge className="bg-slate-200 text-slate-500 text-[10px]">
                      Unverified
                    </Badge>
                  </div>
                </div>
              </div>

              {/* TradeProof Credential */}
              <div className="bg-blue-50 rounded-xl p-4 border border-blue-200 ring-2 ring-blue-100">
                <div className="flex items-center gap-1.5 mb-3">
                  <Award className="w-4 h-4 text-blue-600" />
                  <span className="text-xs font-semibold text-blue-600 uppercase">
                    TradeProof
                  </span>
                </div>
                <div className="space-y-2.5 text-xs text-slate-700">
                  <div>
                    <p className="font-medium">{profile.fullName}</p>
                    <p className="capitalize">{profile.trade} Apprentice</p>
                  </div>
                  <div className="border-t border-blue-100 pt-2">
                    <p className="font-medium text-blue-700 mb-1">
                      Verified Work
                    </p>
                    <p>{stats.totalAnalyses} documented analyses</p>
                    <p>{stats.avgCompliance}% avg compliance</p>
                  </div>
                  <div className="border-t border-blue-100 pt-2">
                    <p className="font-medium text-blue-700 mb-1">
                      Proven Skills
                    </p>
                    <p>
                      {stats.strongSkills.length} verified skills
                    </p>
                  </div>
                  <div className="text-center pt-2">
                    <Badge className="bg-green-100 text-green-700 text-[10px] gap-0.5">
                      <CheckCircle className="w-3 h-3" />
                      AI Verified
                    </Badge>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Full Credential Card */}
        <div className="mb-6">
          <CredentialCard profile={credentialProfile} stats={stats} />
        </div>

        {/* Share Section */}
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <Share2 className="w-6 h-6 text-blue-600 mx-auto mb-2" />
              <h3 className="text-sm font-semibold text-slate-800 mb-1">
                Share This Credential
              </h3>
              <p className="text-xs text-slate-500 mb-4">
                Send this link to employers to showcase your verified skills
              </p>
              <div className="flex items-center gap-2">
                <div className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs text-slate-600 truncate text-left">
                  {typeof window !== "undefined"
                    ? window.location.href
                    : `/credential/${userId}`}
                </div>
                <Button
                  onClick={handleCopyLink}
                  variant="outline"
                  className="shrink-0 h-10 rounded-lg"
                >
                  {copied ? (
                    <>
                      <CheckCircle className="w-4 h-4 text-green-600 mr-1" />
                      <span className="text-green-600 text-xs">Copied</span>
                    </>
                  ) : (
                    <>
                      <Copy className="w-4 h-4 mr-1" />
                      <span className="text-xs">Copy</span>
                    </>
                  )}
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Navigation Links */}
        <div className="mt-6 space-y-3">
          <Link href="/passport" className="block">
            <Button
              variant="outline"
              className="w-full h-12 rounded-xl text-slate-600"
            >
              <MapPin className="w-4 h-4 mr-2" />
              View Skills Passport
            </Button>
          </Link>
          <Link href="/dashboard" className="block">
            <Button
              variant="outline"
              className="w-full h-12 rounded-xl text-slate-600"
            >
              <Shield className="w-4 h-4 mr-2" />
              View Full Portfolio
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
