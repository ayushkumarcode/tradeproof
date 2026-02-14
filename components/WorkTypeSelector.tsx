"use client";

import {
  Box,
  LayoutGrid,
  Plug,
  Pipette,
  Zap,
  Home,
  Lightbulb,
  Wrench,
  type LucideIcon,
} from "lucide-react";
import { Card } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface WorkTypeSelectorProps {
  selected: string;
  onSelect: (workType: string) => void;
}

interface WorkTypeOption {
  id: string;
  label: string;
  icon: LucideIcon;
  photoTip: string;
}

const workTypes: WorkTypeOption[] = [
  {
    id: "junction_box",
    label: "Junction Box",
    icon: Box,
    photoTip: "Photo tip: Capture all wire connections and the box interior",
  },
  {
    id: "panel",
    label: "Panel / Breaker Box",
    icon: LayoutGrid,
    photoTip: "Photo tip: Capture the full panel with cover removed",
  },
  {
    id: "outlet",
    label: "Outlet / Receptacle",
    icon: Plug,
    photoTip: "Photo tip: Capture the outlet face and visible wiring",
  },
  {
    id: "conduit",
    label: "Conduit / Raceway",
    icon: Pipette,
    photoTip: "Photo tip: Capture the full conduit run and fittings",
  },
  {
    id: "grounding",
    label: "Grounding / Bonding",
    icon: Zap,
    photoTip:
      "Photo tip: Capture the grounding conductor path and bonding connections",
  },
  {
    id: "service_entrance",
    label: "Service Entrance",
    icon: Home,
    photoTip:
      "Photo tip: Capture the meter base, service conductors, and grounding electrode",
  },
  {
    id: "lighting",
    label: "Lighting / Luminaire",
    icon: Lightbulb,
    photoTip:
      "Photo tip: Capture the fixture mounting, wiring connections, and box",
  },
  {
    id: "general",
    label: "Other / General",
    icon: Wrench,
    photoTip:
      "Photo tip: Capture the full installation with visible wiring and components",
  },
];

export default function WorkTypeSelector({
  selected,
  onSelect,
}: WorkTypeSelectorProps) {
  const selectedType = workTypes.find((w) => w.id === selected);

  return (
    <div className="space-y-3">
      <p className="text-sm font-medium text-slate-700">
        What type of work are you documenting?
      </p>

      {/* 2x4 grid of tappable cards */}
      <div className="grid grid-cols-2 gap-2.5">
        {workTypes.map((type) => {
          const Icon = type.icon;
          const isSelected = selected === type.id;

          return (
            <button
              key={type.id}
              onClick={() => onSelect(type.id)}
              className={cn(
                "flex flex-col items-center justify-center gap-2 p-4 rounded-xl border-2 transition-all duration-150 active:scale-[0.97]",
                isSelected
                  ? "border-blue-500 bg-blue-50 shadow-sm"
                  : "border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50"
              )}
            >
              <Icon
                className={cn(
                  "w-6 h-6",
                  isSelected ? "text-blue-600" : "text-slate-500"
                )}
              />
              <span
                className={cn(
                  "text-xs font-medium text-center leading-tight",
                  isSelected ? "text-blue-700" : "text-slate-600"
                )}
              >
                {type.label}
              </span>
            </button>
          );
        })}
      </div>

      {/* Photo tip for selected type */}
      {selectedType && (
        <Card className="bg-blue-50 border-blue-200 p-3">
          <p className="text-sm text-blue-700">{selectedType.photoTip}</p>
        </Card>
      )}
    </div>
  );
}
