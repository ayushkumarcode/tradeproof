"use client";

import { useEffect } from "react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Wrench,
  Home,
  Users,
  Briefcase,
  ArrowRight,
  Shield,
  Store,
} from "lucide-react";
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
          <p className="text-slate-500 mt-2 text-sm">
            Hire verified, compliance-scored electricians or find your next job
          </p>
        </div>

        {/* Primary CTA Cards */}
        <div className="space-y-4 mb-8">
          <Link href="/marketplace/workers" className="block">
            <Card className="hover:shadow-md transition-shadow cursor-pointer border-blue-200 bg-gradient-to-br from-blue-50 to-white">
              <CardContent className="flex items-center gap-4">
                <div className="flex-shrink-0 w-14 h-14 rounded-2xl bg-blue-600 flex items-center justify-center">
                  <Wrench className="w-7 h-7 text-white" />
                </div>
                <div className="flex-1 min-w-0">
                  <h2 className="font-semibold text-slate-900 text-lg">
                    I&apos;m a Trade Worker
                  </h2>
                  <p className="text-slate-500 text-sm mt-0.5">
                    Browse jobs, apply, and showcase your compliance score
                  </p>
                </div>
                <ArrowRight className="w-5 h-5 text-slate-400 flex-shrink-0" />
              </CardContent>
            </Card>
          </Link>

          <Link href="/marketplace/jobs/new" className="block">
            <Card className="hover:shadow-md transition-shadow cursor-pointer border-emerald-200 bg-gradient-to-br from-emerald-50 to-white">
              <CardContent className="flex items-center gap-4">
                <div className="flex-shrink-0 w-14 h-14 rounded-2xl bg-emerald-600 flex items-center justify-center">
                  <Home className="w-7 h-7 text-white" />
                </div>
                <div className="flex-1 min-w-0">
                  <h2 className="font-semibold text-slate-900 text-lg">
                    I Need Work Done
                  </h2>
                  <p className="text-slate-500 text-sm mt-0.5">
                    Post a job and get matched with verified electricians
                  </p>
                </div>
                <ArrowRight className="w-5 h-5 text-slate-400 flex-shrink-0" />
              </CardContent>
            </Card>
          </Link>
        </div>

        {/* Secondary Links */}
        <div className="grid grid-cols-2 gap-3 mb-8">
          <Link href="/marketplace/jobs">
            <Card className="hover:shadow-md transition-shadow cursor-pointer h-full">
              <CardContent className="flex flex-col items-center text-center py-2">
                <Briefcase className="w-6 h-6 text-amber-600 mb-2" />
                <span className="font-medium text-slate-900 text-sm">
                  Browse Jobs
                </span>
                <span className="text-slate-400 text-xs mt-0.5">
                  Open positions
                </span>
              </CardContent>
            </Card>
          </Link>
          <Link href="/marketplace/workers">
            <Card className="hover:shadow-md transition-shadow cursor-pointer h-full">
              <CardContent className="flex flex-col items-center text-center py-2">
                <Users className="w-6 h-6 text-purple-600 mb-2" />
                <span className="font-medium text-slate-900 text-sm">
                  Browse Workers
                </span>
                <span className="text-slate-400 text-xs mt-0.5">
                  Verified pros
                </span>
              </CardContent>
            </Card>
          </Link>
        </div>

        {/* Trust Banner */}
        <Card className="bg-slate-900 text-white border-0">
          <CardContent className="flex items-start gap-3">
            <Shield className="w-8 h-8 text-blue-400 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="font-semibold text-sm">
                Compliance-Verified Workers
              </h3>
              <p className="text-slate-300 text-xs mt-1 leading-relaxed">
                Every worker on TradeProof has a real-time compliance score
                based on AI-verified field work. No fake reviews — just
                code-checked proof of quality.
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Dashboard Link */}
        <div className="mt-6 text-center">
          <Link href="/marketplace/dashboard">
            <Button variant="ghost" className="text-slate-500 text-sm">
              Go to Marketplace Dashboard
              <ArrowRight className="w-4 h-4 ml-1" />
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
