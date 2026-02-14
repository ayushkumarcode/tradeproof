"use client";

import { Card } from "@/components/ui/card";
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ReferenceLine,
} from "recharts";

interface AnalysisDataPoint {
  date: string;
  score: number;
}

interface ComplianceTrendProps {
  analyses: AnalysisDataPoint[];
}

function formatDateLabel(dateStr: string): string {
  const date = new Date(dateStr);
  const months = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ];
  return months[date.getMonth()];
}

interface CustomTooltipProps {
  active?: boolean;
  payload?: Array<{ value: number }>;
  label?: string;
}

function CustomTooltip({ active, payload, label }: CustomTooltipProps) {
  if (!active || !payload?.length) return null;

  return (
    <div className="bg-white border border-slate-200 rounded-lg shadow-lg px-3 py-2">
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-sm font-semibold text-slate-800">
        Score: {payload[0].value}
      </p>
    </div>
  );
}

export default function ComplianceTrend({ analyses }: ComplianceTrendProps) {
  const chartData = analyses.map((item) => ({
    ...item,
    label: formatDateLabel(item.date),
  }));

  return (
    <Card className="p-4">
      <h3 className="text-sm font-semibold text-slate-800 mb-4">
        Compliance Over Time
      </h3>

      {chartData.length > 1 ? (
        <div className="w-full h-48">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart
              data={chartData}
              margin={{ top: 5, right: 10, left: -20, bottom: 5 }}
            >
              <XAxis
                dataKey="label"
                tick={{ fontSize: 12, fill: "#94a3b8" }}
                tickLine={false}
                axisLine={{ stroke: "#e2e8f0" }}
              />
              <YAxis
                domain={[0, 100]}
                tick={{ fontSize: 12, fill: "#94a3b8" }}
                tickLine={false}
                axisLine={false}
                tickCount={6}
              />
              <Tooltip content={<CustomTooltip />} />
              <ReferenceLine
                y={80}
                stroke="#94a3b8"
                strokeDasharray="4 4"
                label={{
                  value: "Target",
                  position: "right",
                  fill: "#94a3b8",
                  fontSize: 11,
                }}
              />
              <Line
                type="monotone"
                dataKey="score"
                stroke="#3b82f6"
                strokeWidth={2.5}
                dot={{ r: 4, fill: "#3b82f6", strokeWidth: 2, stroke: "#fff" }}
                activeDot={{
                  r: 6,
                  fill: "#3b82f6",
                  strokeWidth: 2,
                  stroke: "#fff",
                }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      ) : (
        <div className="flex items-center justify-center h-48 text-sm text-slate-400">
          {chartData.length === 1
            ? "Complete more analyses to see your trend"
            : "No analysis data yet"}
        </div>
      )}

      {/* Legend */}
      <div className="flex items-center justify-center gap-4 mt-3">
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-0.5 bg-blue-500 rounded" />
          <span className="text-xs text-slate-500">Your Score</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-0.5 border-t border-dashed border-slate-400" />
          <span className="text-xs text-slate-500">Target (80)</span>
        </div>
      </div>
    </Card>
  );
}
