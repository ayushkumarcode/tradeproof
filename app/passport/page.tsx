"use client";

import { useState, useEffect, useMemo } from "react";
import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import GapAnalysis from "@/components/GapAnalysis";
import {
  Globe,
  Shield,
  MapPin,
  ArrowLeft,
  Loader2,
} from "lucide-react";
import { getSkillScores, getProfile } from "@/lib/storage";
import type { SkillScore, UserProfile } from "@/lib/storage";
import {
  getGapAnalysis,
  STATE_REQUIREMENTS,
} from "@/lib/codes/state-requirements";

const TARGET_STATES = ["Texas", "Arizona"];

export default function PassportPage() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [skillScores, setSkillScores] = useState<SkillScore[]>([]);
  const [targetState, setTargetState] = useState("Texas");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setProfile(getProfile());
    setSkillScores(getSkillScores());
    setLoading(false);
  }, []);

  // Derive user skills from skill scores
  const userSkills = useMemo(
    () => skillScores.map((s) => s.skillName),
    [skillScores]
  );

  // Compute gap analysis
  const gapResult = useMemo(
    () => getGapAnalysis(userSkills, targetState),
    [userSkills, targetState]
  );

  // Map gap results to the GapAnalysis component format
  const mappedGaps = gapResult.gaps.map((g) => ({
    requirement: g.requirement,
    status: g.status === "satisfied" ? "satisfied" as const : g.status === "gap" ? "gap" as const : "partial" as const,
    details: g.details,
    courseSuggestion: g.courseSuggestion,
    coursePrice: g.coursePrice,
    courseHours: g.courseHours ? parseInt(g.courseHours) : undefined,
  }));

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  const currentState = profile?.primaryJurisdiction || "California";
  const currentReqs = STATE_REQUIREMENTS[currentState];
  const targetReqs = STATE_REQUIREMENTS[targetState];

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
            <Globe className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-slate-800">
              Skills Passport
            </span>
          </div>
          <Shield className="w-5 h-5 text-blue-600" />
        </div>

        {/* Current State */}
        <Card className="mb-4">
          <CardContent className="pt-5 pb-5">
            <div className="flex items-center gap-2 mb-2">
              <MapPin className="w-4 h-4 text-green-600" />
              <span className="text-sm font-semibold text-slate-700">
                Current State
              </span>
            </div>
            <div className="flex items-center gap-3">
              <Badge className="bg-green-100 text-green-800 text-sm px-3 py-1">
                {currentState}
              </Badge>
              {currentReqs && (
                <span className="text-xs text-slate-500">
                  {currentReqs.adoptedCode}
                </span>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Target State Selector */}
        <div className="mb-6">
          <p className="text-sm font-medium text-slate-700 mb-3">
            Where do you want to work?
          </p>
          <div className="flex gap-3">
            {TARGET_STATES.map((state) => (
              <Button
                key={state}
                variant={targetState === state ? "default" : "outline"}
                onClick={() => setTargetState(state)}
                className={`flex-1 h-12 rounded-xl ${
                  targetState === state
                    ? "bg-blue-600 text-white hover:bg-blue-700"
                    : "border-slate-300 text-slate-600"
                }`}
              >
                <MapPin className="w-4 h-4 mr-1.5" />
                {state}
              </Button>
            ))}
          </div>
        </div>

        {/* Target State Info */}
        {targetReqs && (
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                <Globe className="w-4 h-4 text-blue-600" />
                {targetState} Requirements
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3 text-sm text-slate-600">
                <div>
                  <span className="font-medium text-slate-700">Code: </span>
                  {targetReqs.adoptedCode}
                </div>
                <div>
                  <span className="font-medium text-slate-700">
                    Required Hours:{" "}
                  </span>
                  {targetReqs.requiredHours.toLocaleString()}
                </div>
                <div>
                  <span className="font-medium text-slate-700">CE: </span>
                  {targetReqs.continuingEducation}
                </div>
                <div>
                  <span className="font-medium text-slate-700">
                    Reciprocity:{" "}
                  </span>
                  {targetReqs.reciprocity.join(", ")}
                </div>
                <div>
                  <span className="font-medium text-slate-700">
                    Certifications:{" "}
                  </span>
                  <div className="flex flex-wrap gap-1 mt-1">
                    {targetReqs.certifications.map((cert) => (
                      <Badge
                        key={cert}
                        className="bg-slate-100 text-slate-600 text-xs"
                      >
                        {cert}
                      </Badge>
                    ))}
                  </div>
                </div>
                {targetReqs.notes && (
                  <p className="text-xs text-slate-500 italic">
                    {targetReqs.notes}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        )}

        {/* Gap Analysis */}
        <GapAnalysis
          currentState={currentState}
          targetState={targetState}
          gaps={mappedGaps}
          overallMatch={gapResult.overallMatch}
        />
      </div>
    </div>
  );
}
