"use client";

import { useEffect } from "react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Camera,
  Shield,
  TrendingUp,
  Globe,
  MapPin,
  ArrowRight,
  Zap,
} from "lucide-react";
import { isDemoLoaded, loadDemoDataFromSeed } from "@/lib/storage";
import {
  DEMO_PROFILE,
  DEMO_ANALYSES,
  DEMO_SKILL_SCORES,
} from "@/data/demo-data";

const features = [
  {
    icon: Zap,
    title: "Instant Analysis",
    description:
      "Snap a photo of your work and get AI-powered code compliance analysis in seconds.",
    color: "text-yellow-500",
    bg: "bg-yellow-50",
  },
  {
    icon: TrendingUp,
    title: "Build Your Portfolio",
    description:
      "Track your skill growth over time with verified, data-backed analyses.",
    color: "text-green-500",
    bg: "bg-green-50",
  },
  {
    icon: Globe,
    title: "Cross-State Passport",
    description:
      "See how your skills map to requirements in any state. Move your career anywhere.",
    color: "text-blue-500",
    bg: "bg-blue-50",
  },
];

export default function Home() {
  useEffect(() => {
    if (!isDemoLoaded()) {
      loadDemoDataFromSeed(DEMO_PROFILE, DEMO_ANALYSES, DEMO_SKILL_SCORES);
    }
  }, []);

  return (
    <div className="min-h-screen bg-white">
      <div className="max-w-lg mx-auto px-4 pt-6 pb-8">
        {/* Location Chip */}
        <div className="flex justify-center mb-6">
          <Badge className="bg-slate-100 text-slate-600 text-xs gap-1 px-3 py-1">
            <MapPin className="w-3 h-3" />
            San Jose, CA
          </Badge>
        </div>

        {/* Hero Section */}
        <div className="text-center mb-8">
          <div className="flex items-center justify-center gap-2 mb-4">
            <Shield className="w-8 h-8 text-blue-600" />
            <h1 className="text-2xl font-bold text-slate-900">TradeProof</h1>
          </div>
          <p className="text-lg font-semibold text-slate-800 mb-2">
            AI-Powered Code Compliance for Skilled Trades
          </p>
          <p className="text-sm text-slate-500 leading-relaxed max-w-xs mx-auto">
            Photograph your work. Get instant code analysis. Build your verified
            portfolio.
          </p>
        </div>

        {/* CTA Button */}
        <div className="mb-10">
          <Link href="/analyze">
            <Button className="w-full h-14 text-lg bg-blue-600 hover:bg-blue-700 active:bg-blue-800 text-white rounded-xl flex items-center justify-center gap-3 shadow-lg shadow-blue-200">
              <Camera className="w-6 h-6" />
              Check My Work
              <ArrowRight className="w-5 h-5" />
            </Button>
          </Link>
        </div>

        {/* Feature Cards */}
        <div className="space-y-3">
          {features.map((feature) => {
            const Icon = feature.icon;
            return (
              <Card key={feature.title} className="border-slate-200">
                <CardContent className="flex items-start gap-4 pt-5 pb-5">
                  <div
                    className={`${feature.bg} p-2.5 rounded-xl shrink-0`}
                  >
                    <Icon className={`w-5 h-5 ${feature.color}`} />
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold text-slate-800 mb-1">
                      {feature.title}
                    </h3>
                    <p className="text-xs text-slate-500 leading-relaxed">
                      {feature.description}
                    </p>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      </div>
    </div>
  );
}
