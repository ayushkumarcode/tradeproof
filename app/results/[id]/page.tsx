"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import AnalysisResults from "@/components/AnalysisResults";
import BeforeAfter from "@/components/BeforeAfter";
import CameraCapture from "@/components/CameraCapture";
import KnowledgeCard from "@/components/KnowledgeCard";
import {
  ArrowLeft,
  Shield,
  Loader2,
  MapPin,
  BarChart3,
  BookOpen,
  RefreshCw,
  ChevronRight,
} from "lucide-react";
import {
  getAnalysis,
  updateAnalysis,
  isDemoLoaded,
  loadDemoDataFromSeed,
} from "@/lib/storage";
import type { Analysis } from "@/lib/storage";
import {
  DEMO_PROFILE,
  DEMO_ANALYSES,
  DEMO_SKILL_SCORES,
} from "@/data/demo-data";
import { getRelevantClips } from "@/data/knowledge-clips";
import type { KnowledgeClip } from "@/data/knowledge-clips";

type RecheckResultState = {
  originalViolationStatus: { description: string; status: "resolved" | "unresolved" | "partially_resolved" }[];
  newViolationsFound: string[];
  complianceScore: number;
  isCompliant: boolean;
};

/** Build violations list for recheck API. For a subsequent round, use previous fix result. */
function buildOriginalViolationsForRecheck(analysis: Analysis): { description: string; code_section: string; severity: string; fix_instruction: string }[] {
  const fix = analysis.fixAnalysis as {
    original_violation_status?: { original_description: string; original_code_section?: string; status: string }[];
    new_violations_found?: { description: string; code_section: string; severity: string; fix_instruction: string }[];
  } | undefined;
  if (!fix?.original_violation_status?.length && !fix?.new_violations_found?.length) {
    return analysis.violations.map((v) => ({
      description: v.description,
      code_section: v.codeSection,
      severity: v.severity,
      fix_instruction: v.fixInstruction,
    }));
  }
  const list: { description: string; code_section: string; severity: string; fix_instruction: string }[] = [];
  for (const vs of fix.original_violation_status || []) {
    if (vs.status === "resolved") continue;
    const match = analysis.violations.find((v) => v.description === vs.original_description);
    list.push({
      description: vs.original_description,
      code_section: vs.original_code_section || match?.codeSection || "NEC",
      severity: match?.severity || "major",
      fix_instruction: match?.fixInstruction || "Address the violation.",
    });
  }
  for (const nv of fix.new_violations_found || []) {
    list.push({
      description: nv.description,
      code_section: nv.code_section,
      severity: nv.severity,
      fix_instruction: nv.fix_instruction,
    });
  }
  return list;
}

function getBeforeImageForRecheck(analysis: Analysis): string {
  return analysis.fixedPhotoUrl || analysis.photoUrl;
}

function isImageUrlValid(url: string): boolean {
  return (
    url.startsWith("data:image/") ||
    url.startsWith("https://") ||
    url.startsWith("http://")
  );
}

/** Fetch an image from URL and return as base64 data URL for the recheck API. */
async function imageUrlToBase64(url: string): Promise<string> {
  const res = await fetch(url, { mode: "cors" });
  if (!res.ok) throw new Error(`Failed to load image: ${res.status}`);
  const blob = await res.blob();
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onloadend = () => resolve(reader.result as string);
    reader.onerror = reject;
    reader.readAsDataURL(blob);
  });
}

export default function ResultsPage() {
  const params = useParams();
  const id = params.id as string;

  const [analysis, setAnalysis] = useState<Analysis | null>(null);
  const [loading, setLoading] = useState(true);
  const [showRecheck, setShowRecheck] = useState(false);
  const [fixedImage, setFixedImage] = useState<string | null>(null);
  const [isRechecking, setIsRechecking] = useState(false);
  const [recheckResult, setRecheckResult] = useState<RecheckResultState | null>(null);
  const [relevantClips, setRelevantClips] = useState<KnowledgeClip[]>([]);

  useEffect(() => {
    if (!isDemoLoaded()) {
      loadDemoDataFromSeed(DEMO_PROFILE, DEMO_ANALYSES, DEMO_SKILL_SCORES);
    }
    let data = getAnalysis(id);
    // If this is a demo analysis that still has a photo (old seed), re-seed to use current seed (no demo images)
    if (data?.id.startsWith("demo-") && data.photoUrl?.trim()) {
      loadDemoDataFromSeed(DEMO_PROFILE, DEMO_ANALYSES, DEMO_SKILL_SCORES);
      data = getAnalysis(id) ?? data;
    }
    if (data) {
      setAnalysis(data);

      // Build keywords from work type and violation descriptions
      const keywords: string[] = [data.workType];
      data.violations.forEach((v) => {
        // Extract key terms from the violation description
        const words = v.description.toLowerCase().split(/\s+/);
        keywords.push(...words.filter((w) => w.length > 4));
        keywords.push(v.codeSection);
      });
      const clips = getRelevantClips(keywords);
      setRelevantClips(clips);
    }
    setLoading(false);
  }, [id]);

  function handleRecheckClick() {
    setShowRecheck(true);
    setFixedImage(null);
  }

  async function handleSubmitRecheck() {
    if (!fixedImage || !analysis) return;

    setIsRechecking(true);

    try {
      const beforeImage = getBeforeImageForRecheck(analysis);
      const originalViolations = buildOriginalViolationsForRecheck(analysis);

      if (!beforeImage || !isImageUrlValid(beforeImage)) {
        alert(
          "Original photo is missing or invalid. Re-check compares your new photo to the previous one."
        );
        setIsRechecking(false);
        return;
      }

      if (originalViolations.length === 0) {
        alert("There are no remaining violations to re-check. Your previous fix resolved everything.");
        setIsRechecking(false);
        return;
      }

      const originalImageBase64 =
        beforeImage.startsWith("data:image/")
          ? beforeImage
          : await imageUrlToBase64(beforeImage);

      const response = await fetch("/api/recheck", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          originalImage: originalImageBase64,
          fixedImage,
          originalViolations,
          jurisdiction: analysis.jurisdiction,
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Re-check failed");
      }

      const result = await response.json();

      const mappedResult: RecheckResultState = {
        originalViolationStatus: (result.original_violation_status || []).map(
          (vs: { original_description: string; status: string }) => ({
            description: vs.original_description,
            status: vs.status as "resolved" | "unresolved" | "partially_resolved",
          })
        ),
        newViolationsFound: (result.new_violations_found || []).map(
          (nv: { description: string }) => nv.description
        ),
        complianceScore: result.compliance_score,
        isCompliant: result.is_compliant,
      };

      setRecheckResult(mappedResult);

      const revisionHistory = [...(analysis.revisionHistory || [])];
      if (analysis.fixedPhotoUrl && analysis.fixAnalysis) {
        revisionHistory.push({
          fixedPhotoUrl: analysis.fixedPhotoUrl,
          complianceScore: analysis.fixComplianceScore ?? 0,
          isCompliant: analysis.fixVerified ?? false,
          fixAnalysis: analysis.fixAnalysis,
          createdAt: new Date().toISOString(),
        });
      }

      updateAnalysis(id, {
        fixedPhotoUrl: fixedImage,
        fixVerified: result.is_compliant,
        fixComplianceScore: result.compliance_score,
        fixAnalysis: result,
        revisionHistory,
      });

      const updated = getAnalysis(id);
      if (updated) setAnalysis(updated);

      setShowRecheck(false);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Something went wrong";
      alert(`Re-check failed: ${message}`);
    } finally {
      setIsRechecking(false);
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!analysis) {
    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-lg mx-auto px-4 pt-8 text-center">
          <Shield className="w-12 h-12 text-slate-300 mx-auto mb-4" />
          <h2 className="text-lg font-semibold text-slate-800 mb-2">
            Analysis Not Found
          </h2>
          <p className="text-sm text-slate-500 mb-6">
            This analysis may have been removed or the link is invalid.
          </p>
          <Link href="/analyze">
            <Button className="bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              Start New Analysis
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  const analysisForComponent = {
    description: analysis.overallAssessment,
    isCompliant: analysis.isCompliant,
    complianceScore: analysis.complianceScore,
    violations: analysis.violations,
    correctItems: analysis.correctItems,
    skillsDemonstrated: analysis.skillsDemonstrated.map((s) => s.skill),
    overallAssessment: analysis.overallAssessment,
  };

  const hasExistingFix = analysis.fixAnalysis && analysis.fixedPhotoUrl;
  const beforeImageForDisplay = (analysis.revisionHistory?.length ?? 0) > 0
    ? analysis.revisionHistory![analysis.revisionHistory!.length - 1]!.fixedPhotoUrl
    : analysis.photoUrl;

  return (
    <div className="min-h-screen bg-white">
      <div className="max-w-lg mx-auto px-4 pt-4 pb-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <Link
            href="/dashboard"
            className="flex items-center gap-1 text-slate-500 hover:text-slate-700 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            <span className="text-sm">Dashboard</span>
          </Link>
          <div className="flex items-center gap-1.5">
            <Shield className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-slate-800">
              Results
            </span>
          </div>
          <Badge className="bg-slate-100 text-slate-500 text-xs gap-1">
            <MapPin className="w-3 h-3" />
            {analysis.jurisdiction === "California" ? "CA" : analysis.jurisdiction}
          </Badge>
        </div>

        {/* Date */}
        <p className="text-xs text-slate-400 mb-4 text-center">
          {new Date(analysis.createdAt).toLocaleDateString("en-US", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric",
          })}
        </p>

        {/* Work photo — always show when we have one */}
        {analysis.photoUrl && (
          <div className="mb-6 rounded-xl overflow-hidden border border-slate-200 bg-slate-50">
            <img
              src={analysis.photoUrl}
              alt="Work submitted for analysis"
              className="w-full h-auto max-h-72 object-contain"
              referrerPolicy="no-referrer"
              onError={(e) => {
                const target = e.currentTarget;
                target.onerror = null;
                target.src = "data:image/svg+xml," + encodeURIComponent('<svg xmlns="http://www.w3.org/2000/svg" width="400" height="200" viewBox="0 0 400 200"><rect fill="#f1f5f9" width="400" height="200"/><text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle" fill="#94a3b8" font-family="sans-serif" font-size="14">Photo unavailable</text></svg>');
              }}
            />
          </div>
        )}

        {/* Thread summary: rounds */}
        {hasExistingFix && (
          <div className="mb-4 flex items-center gap-2 flex-wrap">
            <span className="text-xs text-slate-500">Thread:</span>
            <span className="text-xs font-medium text-slate-700">
              Round 1: {analysis.complianceScore}%
              {(analysis.revisionHistory?.length ?? 0) > 0 &&
                analysis.revisionHistory!.map((r, i) => (
                  <span key={i}> → Round {i + 2}: {r.complianceScore}%</span>
                ))}
              {hasExistingFix && (
                <span> → Current: {analysis.fixComplianceScore}%</span>
              )}
            </span>
          </div>
        )}

        {/* Re-check result (Before/After) — just submitted */}
        {recheckResult && (
          <div className="mb-6">
            <BeforeAfter
              beforeImage={beforeImageForDisplay}
              afterImage={fixedImage || ""}
              originalViolations={analysis.violations.map(
                (v) => v.description
              )}
              recheckResult={recheckResult}
            />
          </div>
        )}

        {/* Show previously stored recheck if exists */}
        {!recheckResult && hasExistingFix && (
          <div className="mb-6">
            <BeforeAfter
              beforeImage={beforeImageForDisplay}
              afterImage={analysis.fixedPhotoUrl!}
              originalViolations={analysis.violations.map(
                (v) => v.description
              )}
              recheckResult={{
                originalViolationStatus: (
                  (analysis.fixAnalysis as { original_violation_status?: { original_description: string; status: string }[] }).original_violation_status || []
                ).map(
                  (vs: { original_description: string; status: string }) => ({
                    description: vs.original_description,
                    status: vs.status as
                      | "resolved"
                      | "unresolved"
                      | "partially_resolved",
                  })
                ),
                newViolationsFound: (
                  (analysis.fixAnalysis as { new_violations_found?: { description: string }[] }).new_violations_found || []
                ).map((nv: { description: string }) => nv.description),
                complianceScore: (analysis.fixAnalysis as { compliance_score: number }).compliance_score,
                isCompliant: (analysis.fixAnalysis as { is_compliant: boolean }).is_compliant,
              }}
            />
          </div>
        )}

        {/* Main analysis results — no fix yet */}
        {!recheckResult && !hasExistingFix && (
          <AnalysisResults
            analysis={analysisForComponent}
            onRecheck={
              isImageUrlValid(analysis.photoUrl || "")
                ? handleRecheckClick
                : () =>
                    alert(
                      "Re-check isn't available — no photo was saved for this analysis. Run a new Check My Work, then use re-check."
                    )
            }
            photoUrl={analysis.photoUrl}
          />
        )}

        {/* Try again for a better grade — show when there's already a fix and we have a valid before image */}
        {!showRecheck && hasExistingFix && isImageUrlValid(getBeforeImageForRecheck(analysis)) && (
          <div className="mb-6">
            <Button
              onClick={handleRecheckClick}
              variant="outline"
              className="w-full rounded-xl border-blue-200 text-blue-700 hover:bg-blue-50"
            >
              <RefreshCw className="w-4 h-4 mr-2" />
              Try again for a better grade
              <ChevronRight className="w-4 h-4 ml-2" />
            </Button>
          </div>
        )}

        {/* Re-check flow */}
        {showRecheck && (
          <div className="mt-6 space-y-4">
            <Card className="border-blue-200 bg-blue-50">
              <CardContent className="pt-6">
                <h3 className="text-sm font-semibold text-blue-800 mb-3">
                  {hasExistingFix ? "Take a new photo of your improved work" : "Take a photo of your fixed work"}
                </h3>
                <CameraCapture
                  onCapture={(base64) => setFixedImage(base64)}
                  label={hasExistingFix ? "Photo of improved work" : "Photo of fixed work"}
                  existingImage={getBeforeImageForRecheck(analysis)}
                />
                {fixedImage && (
                  <Button
                    onClick={handleSubmitRecheck}
                    disabled={isRechecking}
                    className="w-full h-12 mt-4 bg-green-600 hover:bg-green-700 text-white rounded-xl"
                  >
                    {isRechecking ? (
                      <>
                        <Loader2 className="w-4 h-4 animate-spin mr-2" />
                        Re-checking...
                      </>
                    ) : (
                      "Submit Re-Check"
                    )}
                  </Button>
                )}
                <Button
                  variant="outline"
                  onClick={() => {
                    setShowRecheck(false);
                    setFixedImage(null);
                  }}
                  className="w-full h-10 mt-2 rounded-xl"
                >
                  Cancel
                </Button>
              </CardContent>
            </Card>
          </div>
        )}

        {/* Relevant Knowledge Clips */}
        {relevantClips.length > 0 && (
          <div className="mt-8 space-y-3">
            <div className="flex items-center gap-2">
              <BookOpen className="w-4 h-4 text-blue-600" />
              <h3 className="text-sm font-semibold text-slate-700">
                Expert Insights
              </h3>
            </div>
            {relevantClips.map((clip) => (
              <KnowledgeCard key={clip.id} clip={clip} />
            ))}
          </div>
        )}

        {/* Bottom actions */}
        <div className="mt-8 space-y-3">
          <Link href="/dashboard" className="block">
            <Button
              variant="outline"
              className="w-full h-12 rounded-xl text-slate-600"
            >
              <BarChart3 className="w-4 h-4 mr-2" />
              Back to Dashboard
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
