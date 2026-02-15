"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import WorkTypeSelector from "@/components/WorkTypeSelector";
import CameraCapture from "@/components/CameraCapture";
import {
  Shield,
  Loader2,
  ArrowLeft,
  MapPin,
  CheckCircle,
} from "lucide-react";
import { saveAnalysis, updateSkillScores } from "@/lib/storage";
import type { Analysis } from "@/lib/storage";
import Link from "next/link";

export default function AnalyzePage() {
  const router = useRouter();
  const [workType, setWorkType] = useState("");
  const [description, setDescription] = useState("");
  const [beforeImage, setBeforeImage] = useState<string | null>(null);
  const [afterImage, setAfterImage] = useState<string | null>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const canSubmit =
    workType && description.trim() && beforeImage && afterImage && !isAnalyzing;

  const currentStep = !workType
    ? 1
    : !description.trim()
      ? 2
      : !beforeImage
        ? 3
        : !afterImage
          ? 4
          : 5;

  async function handleAnalyze() {
    if (!canSubmit || !beforeImage || !afterImage) return;

    setIsAnalyzing(true);
    setError(null);

    try {
      const response = await fetch("/api/analyze", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          beforeImage,
          afterImage,
          workType,
          userDescription: description,
          jurisdiction: "California",
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Analysis failed");
      }

      const result = await response.json();

      // Build the Analysis object for localStorage
      const analysis: Analysis = {
        id: result.id,
        userId: "demo-alex-smith",
        photoUrl: beforeImage,
        fixedPhotoUrl: afterImage,
        jurisdiction: result.jurisdiction || "California",
        trade: "electrical",
        workType: result.workType || workType,
        userDescription: description,
        isCompliant: result.is_compliant,
        complianceScore: result.after_score || result.compliance_score || 0,
        violations: (result.violations_found || []).map(
          (v: {
            id: number;
            description: string;
            code_section: string;
            severity: "critical" | "major" | "minor";
            status: string;
            fix_instruction: string;
            why_this_matters: string;
            location_x: number;
            location_y: number;
          }) => ({
            description: v.description,
            codeSection: v.code_section,
            severity: v.severity,
            confidence: "high" as const,
            fixInstruction: v.fix_instruction,
            whyThisMatters: v.why_this_matters,
            visualEvidence: `Location: ${v.location_x}%, ${v.location_y}%`,
          })
        ),
        correctItems: result.resolved_items || [],
        skillsDemonstrated: (result.skills_demonstrated || []).map(
          (s: {
            skill: string;
            quality: "good" | "acceptable" | "needs_work";
          }) => ({
            skill: s.skill,
            quality: s.quality,
          })
        ),
        overallAssessment: result.overall_assessment || "",
        createdAt: result.timestamp || new Date().toISOString(),
        fixAnalysis: result,
        fixVerified: result.is_compliant,
        fixComplianceScore: result.after_score,
      };

      saveAnalysis(analysis);

      if (analysis.skillsDemonstrated.length > 0) {
        updateSkillScores(analysis.skillsDemonstrated);
      }

      router.push(`/results/${analysis.id}`);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Something went wrong";
      setError(message);
      alert(`Analysis failed: ${message}`);
    } finally {
      setIsAnalyzing(false);
    }
  }

  return (
    <div className="min-h-screen bg-white">
      <div className="max-w-lg mx-auto px-4 pt-4 pb-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <Link
            href="/"
            className="flex items-center gap-1 text-slate-500 hover:text-slate-700 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            <span className="text-sm">Back</span>
          </Link>
          <div className="flex items-center gap-1.5">
            <Shield className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-semibold text-slate-800">
              TradeProof
            </span>
          </div>
          <Badge className="bg-slate-100 text-slate-500 text-xs gap-1">
            <MapPin className="w-3 h-3" />
            CA
          </Badge>
        </div>

        {/* Progress indicator */}
        <div className="flex items-center gap-2 mb-6">
          {[1, 2, 3, 4, 5].map((step) => (
            <div
              key={step}
              className={`h-1.5 flex-1 rounded-full transition-colors ${
                step <= currentStep ? "bg-blue-500" : "bg-slate-200"
              }`}
            />
          ))}
        </div>

        <div className="space-y-6">
          {/* Step 1: Work Type */}
          <div>
            <div className="flex items-center gap-2 mb-3">
              <div
                className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                  workType
                    ? "bg-green-500 text-white"
                    : "bg-blue-500 text-white"
                }`}
              >
                {workType ? <CheckCircle className="w-4 h-4" /> : "1"}
              </div>
              <span className="text-sm font-medium text-slate-700">
                Select Work Type
              </span>
            </div>
            <WorkTypeSelector selected={workType} onSelect={setWorkType} />
          </div>

          {/* Step 2: Description */}
          {workType && (
            <div>
              <div className="flex items-center gap-2 mb-3">
                <div
                  className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                    description.trim()
                      ? "bg-green-500 text-white"
                      : "bg-blue-500 text-white"
                  }`}
                >
                  {description.trim() ? (
                    <CheckCircle className="w-4 h-4" />
                  ) : (
                    "2"
                  )}
                </div>
                <span className="text-sm font-medium text-slate-700">
                  What did you do?
                </span>
              </div>
              <Input
                placeholder="e.g., Installed a GFCI outlet in the kitchen near the sink"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="h-12 text-base rounded-xl"
              />
              <p className="text-xs text-slate-400 mt-1 ml-1">
                One sentence describing the work you performed
              </p>
            </div>
          )}

          {/* Step 3: Before Photo */}
          {workType && description.trim() && (
            <div>
              <div className="flex items-center gap-2 mb-3">
                <div
                  className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                    beforeImage
                      ? "bg-green-500 text-white"
                      : "bg-blue-500 text-white"
                  }`}
                >
                  {beforeImage ? <CheckCircle className="w-4 h-4" /> : "3"}
                </div>
                <span className="text-sm font-medium text-slate-700">
                  Before Photo
                </span>
                <Badge className="bg-red-100 text-red-700 text-xs">
                  Original Work
                </Badge>
              </div>
              <CameraCapture
                onCapture={(base64) => setBeforeImage(base64)}
                label="Photo of the original work (before fixes)"
              />
            </div>
          )}

          {/* Step 4: After Photo */}
          {workType && description.trim() && beforeImage && (
            <div>
              <div className="flex items-center gap-2 mb-3">
                <div
                  className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                    afterImage
                      ? "bg-green-500 text-white"
                      : "bg-blue-500 text-white"
                  }`}
                >
                  {afterImage ? <CheckCircle className="w-4 h-4" /> : "4"}
                </div>
                <span className="text-sm font-medium text-slate-700">
                  After Photo
                </span>
                <Badge className="bg-green-100 text-green-700 text-xs">
                  After Fixes
                </Badge>
              </div>
              <CameraCapture
                onCapture={(base64) => setAfterImage(base64)}
                label="Photo of the work after your fixes"
              />
            </div>
          )}

          {/* Step 5: Submit */}
          {canSubmit && (
            <div>
              <Button
                onClick={handleAnalyze}
                disabled={isAnalyzing}
                className="w-full h-14 text-lg bg-blue-600 hover:bg-blue-700 active:bg-blue-800 text-white rounded-xl flex items-center justify-center gap-2 shadow-lg shadow-blue-200"
              >
                {isAnalyzing ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin" />
                    Comparing...
                  </>
                ) : (
                  <>
                    <Shield className="w-5 h-5" />
                    Analyze Before &amp; After
                  </>
                )}
              </Button>
            </div>
          )}

          {/* Loading state */}
          {isAnalyzing && (
            <Card className="border-blue-200 bg-blue-50">
              <CardContent className="pt-6 text-center">
                <Loader2 className="w-8 h-8 animate-spin text-blue-600 mx-auto mb-3" />
                <p className="text-sm font-medium text-blue-800">
                  Comparing before &amp; after...
                </p>
                <p className="text-xs text-blue-600 mt-1">
                  Checking against NEC codes and California amendments
                </p>
              </CardContent>
            </Card>
          )}

          {/* Error state */}
          {error && (
            <Card className="border-red-200 bg-red-50">
              <CardContent className="pt-6">
                <p className="text-sm text-red-800">{error}</p>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
