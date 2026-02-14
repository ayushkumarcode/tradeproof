"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Camera, BarChart3, Store, BookOpen, User } from "lucide-react";
import { cn } from "@/lib/utils";

const navItems = [
  { href: "/analyze", label: "Check", icon: Camera },
  { href: "/dashboard", label: "Dashboard", icon: BarChart3 },
  { href: "/marketplace", label: "Market", icon: Store },
  { href: "/knowledge", label: "Learn", icon: BookOpen },
  { href: "/credential/demo-alex-smith", label: "Profile", icon: User },
];

export default function BottomNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 bg-white border-t border-slate-200 safe-area-pb">
      <div className="max-w-lg mx-auto flex items-center justify-around h-16">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive =
            pathname === item.href || pathname.startsWith(item.href + "/");

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex flex-col items-center justify-center gap-0.5 w-16 h-full transition-colors",
                isActive
                  ? "text-blue-600"
                  : "text-slate-400 hover:text-slate-600"
              )}
            >
              <Icon className="w-5 h-5" />
              <span className="text-[10px] font-medium">{item.label}</span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
