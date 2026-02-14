import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

interface ExpertClip {
  expertName: string;
  expertYears: number;
  title: string;
  content: string;
  taskType: string;
}

interface KnowledgeCardProps {
  clip: ExpertClip;
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/);
  if (parts.length === 0) return "";
  if (parts.length === 1) return parts[0][0].toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export default function KnowledgeCard({ clip }: KnowledgeCardProps) {
  return (
    <Card className="bg-blue-50 border-blue-100 p-4">
      {/* Expert header */}
      <div className="flex items-center gap-3 mb-3">
        {/* Avatar with initials */}
        <div className="w-10 h-10 rounded-full bg-blue-600 text-white flex items-center justify-center text-sm font-semibold shrink-0">
          {getInitials(clip.expertName)}
        </div>
        <div className="min-w-0">
          <p className="text-sm font-bold text-slate-800 truncate">
            {clip.expertName}
          </p>
          <p className="text-xs text-slate-500">
            {clip.expertYears} years experience
          </p>
        </div>
        <Badge className="bg-blue-100 text-blue-700 text-xs ml-auto shrink-0">
          {clip.taskType}
        </Badge>
      </div>

      {/* Title */}
      <p className="text-sm font-semibold text-slate-800 mb-2">{clip.title}</p>

      {/* Decorative quote + content */}
      <div className="relative pl-4">
        <span
          className="absolute top-0 left-0 text-3xl leading-none text-blue-300 font-serif select-none"
          aria-hidden="true"
        >
          &ldquo;
        </span>
        <p className="text-sm text-slate-700 italic leading-relaxed">
          {clip.content}
        </p>
      </div>
    </Card>
  );
}
