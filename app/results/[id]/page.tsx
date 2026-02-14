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
} from "lucide-react";
import {
  getAnalysis,
  updateAnalysis,
} from "@/lib/storage";
import type { Analysis } from "@/lib/storage";
import { getRelevantClips } from "@/data/knowledge-clips";
import type { KnowledgeClip } from "@/data/knowledge-clips";

export default function ResultsPage() {
  const params = useParams();
  const id = params.id as string;

  const [analysis, setAnalysis] = useState<Analysis | null>(null);
  const [loading, setLoading] = useState(true);
  const [showRecheck, setShowRecheck] = useState(false);
  const [fixedImage, setFixedImage] = useState<string | null>(null);
  const [isRechecking, setIsRechecking] = useState(false);
  const [recheckResult, setRecheckResult] = useState<{
    originalViolationStatus: { description: string; status: "resolved" | "unresolved" | "partially_resolved" }[];
    newViolationsFound: string[];
    complianceScore: number;
    isCompliant: boolean;
  } | null>(null);
  const [relevantClips, setRelevantClips] = useState<KnowledgeClip[]>([]);

  useEffect(() => {
    const data = getAnalysis(id);
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
  }

  async function handleSubmitRecheck() {
    if (!fixedImage || !analysis) return;

    setIsRechecking(true);

    try {
      // Build violations array in API format
      const originalViolations = analysis.violations.map((v) => ({
        description: v.description,
        code_section: v.codeSection,
        severity: v.severity,
        fix_instruction: v.fixInstruction,
      }));

      const response = await fetch("/api/recheck", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          originalImage: analysis.photoUrl,
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

      // Map the API response to the BeforeAfter component format
      const mappedResult = {
        originalViolationStatus: (result.original_violation_status || []).map(
          (vs: { original_description: string; status: string; notes?: string }) => ({
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

      // Update analysis in localStorage
      updateAnalysis(id, {
        fixedPhotoUrl: fixedImage,
        fixVerified: result.is_compliant,
        fixComplianceScore: result.compliance_score,
        fixAnalysis: result,
      });

      // Refresh the analysis data
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

  // Map Analysis to the AnalysisResults component interface
  const analysisForComponent = {
    description: analysis.overallAssessment,
    isCompliant: analysis.isCompliant,
    complianceScore: analysis.complianceScore,
    violations: analysis.violations,
    correctItems: analysis.correctItems,
    skillsDemonstrated: analysis.skillsDemonstrated.map((s) => s.skill),
    overallAssessment: analysis.overallAssessment,
  };

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

        {/* Re-check result (Before/After) */}
        {recheckResult && (
          <div className="mb-6">
            <BeforeAfter
              beforeImage={analysis.photoUrl}
              afterImage={fixedImage || ""}
              originalViolations={analysis.violations.map(
                (v) => v.description
              )}
              recheckResult={recheckResult}
            />
          </div>
        )}

        {/* Show previously stored recheck if exists */}
        {!recheckResult && analysis.fixAnalysis && analysis.fixedPhotoUrl && (
          <div className="mb-6">
            <BeforeAfter
              beforeImage={analysis.photoUrl}
              afterImage={analysis.fixedPhotoUrl}
              originalViolations={analysis.violations.map(
                (v) => v.description
              )}
              recheckResult={{
                originalViolationStatus: (
                  analysis.fixAnalysis.original_violation_status || []
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
                  analysis.fixAnalysis.new_violations_found || []
                ).map((nv: { description: string }) => nv.description),
                complianceScore: analysis.fixAnalysis.compliance_score,
                isCompliant: analysis.fixAnalysis.is_compliant,
              }}
            />
          </div>
        )}

        {/* Main analysis results */}
        {!recheckResult && !analysis.fixAnalysis && (
          <AnalysisResults
            analysis={analysisForComponent}
            onRecheck={handleRecheckClick}
            photoUrl={analysis.photoUrl}
          />
        )}

        {/* Re-check flow */}
        {showRecheck && (
          <div className="mt-6 space-y-4">
            <Card className="border-blue-200 bg-blue-50">
              <CardContent className="pt-6">
                <h3 className="text-sm font-semibold text-blue-800 mb-3">
                  Take a photo of your fixed work
                </h3>
                <CameraCapture
                  onCapture={(base64) => setFixedImage(base64)}
                  label="Photo of fixed work"
                  existingImage={analysis.photoUrl}
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
