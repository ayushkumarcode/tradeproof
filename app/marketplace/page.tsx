"use client";

import { useEffect } from "react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Briefcase, Users, Shield, Store, ChevronRight, Heart } from "lucide-react";
import {
  isMarketplaceDemoLoaded,
  loadMarketplaceDemoData,
} from "@/lib/marketplace-storage";
import {
  DEMO_WORKERS,
  DEMO_HOMEOWNERS,
  DEMO_JOBS,
  DEMO_MESSAGES,
} from "@/data/marketplace-demo";

export default function MarketplacePage() {
  useEffect(() => {
    if (!isMarketplaceDemoLoaded()) {
      loadMarketplaceDemoData(
        DEMO_WORKERS,
        DEMO_HOMEOWNERS,
        DEMO_JOBS,
        DEMO_MESSAGES
      );
    }
  }, []);

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      <div className="max-w-lg mx-auto px-4 pt-8">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-blue-100 mb-4">
            <Store className="w-7 h-7 text-blue-600" />
          </div>
          <h1 className="text-2xl font-bold text-slate-900">
            TradeProof Marketplace
          </h1>
          <p className="text-slate-500 mt-2 text-sm max-w-xs mx-auto">
            Find your next job or hire electricians with verified compliance
            scores.
          </p>
        </div>

        {/* Swipe to Match */}
        <Link href="/marketplace/swipe" className="block group mb-6">
          <Card className="hover:shadow-lg transition-all cursor-pointer border-2 border-rose-200 bg-gradient-to-br from-rose-50 to-white hover:border-rose-300">
            <CardContent className="flex items-center gap-4 py-5">
              <div className="flex-shrink-0 w-12 h-12 rounded-xl bg-rose-500 flex items-center justify-center group-hover:bg-rose-600 transition-colors">
                <Heart className="w-6 h-6 text-white" />
              </div>
              <div className="flex-1 min-w-0">
                <h2 className="font-semibold text-slate-900 text-lg">
                  Swipe to Match
                </h2>
                <p className="text-slate-500 text-sm mt-0.5">
                  Workers swipe on jobs · Employers swipe on candidates
                </p>
              </div>
              <ChevronRight className="w-5 h-5 text-slate-300 group-hover:text-rose-500 flex-shrink-0 transition-colors" />
            </CardContent>
          </Card>
        </Link>

        {/* Browse Jobs & Browse Workers */}
        <div className="space-y-4 mb-8">
          <Link href="/marketplace/jobs" className="block group">
            <Card className="hover:shadow-md transition-all cursor-pointer border-amber-200/80 bg-white hover:border-amber-300">
              <CardContent className="flex items-center gap-4 py-5">
                <div className="flex-shrink-0 w-12 h-12 rounded-xl bg-amber-500 flex items-center justify-center group-hover:bg-amber-600 transition-colors">
                  <Briefcase className="w-6 h-6 text-white" />
                </div>
                <div className="flex-1 min-w-0">
                  <h2 className="font-semibold text-slate-900 text-lg">
                    Browse Jobs
                  </h2>
                  <p className="text-slate-500 text-sm mt-0.5">
                    Open positions — apply with your compliance score
                  </p>
                </div>
                <ChevronRight className="w-5 h-5 text-slate-300 group-hover:text-amber-600 flex-shrink-0 transition-colors" />
              </CardContent>
            </Card>
          </Link>

          <Link href="/marketplace/workers" className="block group">
            <Card className="hover:shadow-md transition-all cursor-pointer border-blue-200/80 bg-white hover:border-blue-300">
              <CardContent className="flex items-center gap-4 py-5">
                <div className="flex-shrink-0 w-12 h-12 rounded-xl bg-blue-600 flex items-center justify-center group-hover:bg-blue-700 transition-colors">
                  <Users className="w-6 h-6 text-white" />
                </div>
                <div className="flex-1 min-w-0">
                  <h2 className="font-semibold text-slate-900 text-lg">
                    Browse Workers
                  </h2>
                  <p className="text-slate-500 text-sm mt-0.5">
                    Verified electricians — see skills and compliance history
                  </p>
                </div>
                <ChevronRight className="w-5 h-5 text-slate-300 group-hover:text-blue-600 flex-shrink-0 transition-colors" />
              </CardContent>
            </Card>
          </Link>
        </div>

        {/* Trust Banner */}
        <Card className="bg-slate-900 text-white border-0 shadow-lg">
          <CardContent className="flex items-start gap-3 py-4">
            <Shield className="w-8 h-8 text-blue-400 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="font-semibold text-sm">
                Compliance-Verified Workers
              </h3>
              <p className="text-slate-300 text-xs mt-1 leading-relaxed">
                Every worker has a real-time compliance score from AI-verified
                field work. No fake reviews — code-checked proof of quality.
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Footer links */}
        <div className="mt-8 flex flex-col items-center gap-2 text-center">
          <Link
            href="/marketplace/dashboard"
            className="text-slate-500 text-sm hover:text-slate-800 font-medium transition-colors"
          >
            My jobs & messages
          </Link>
          <Link
            href="/marketplace/jobs/new"
            className="text-slate-400 text-xs hover:text-slate-600 transition-colors"
          >
            Post a job
          </Link>
        </div>
      </div>
    </div>
  );
}
