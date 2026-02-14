"use client";

import { useState, useRef, useCallback } from "react";
import { cn } from "@/lib/utils";
import { X, Heart } from "lucide-react";

export type SwipeDirection = "like" | "pass";

export interface SwipeStackProps<T> {
  items: T[];
  keyFn: (item: T) => string;
  renderCard: (item: T) => React.ReactNode;
  onSwipe: (item: T, direction: SwipeDirection) => void;
  emptyMessage?: React.ReactNode;
  className?: string;
}

// Distance to drag left/right to commit (pixels). Kept small so laptop drag is easy.
const SWIPE_THRESHOLD = 55;

export function SwipeStack<T>({
  items,
  keyFn,
  renderCard,
  onSwipe,
  emptyMessage = "No more cards",
  className,
}: SwipeStackProps<T>) {
  const [dragX, setDragX] = useState(0);
  const [exiting, setExiting] = useState<string | null>(null);
  const dragStartX = useRef(0);
  const pointerStartX = useRef(0);
  const cardRef = useRef<HTMLDivElement>(null);
  const isDragging = useRef(false);

  const topItem = items[0];
  const restItems = items.slice(1);

  const commitSwipe = useCallback(
    (item: T, direction: SwipeDirection) => {
      const key = keyFn(item);
      if (exiting) return;
      setExiting(key);
      const sign = direction === "like" ? 1 : -1;
      setDragX(sign * 400);
      setTimeout(() => {
        onSwipe(item, direction);
        setExiting(null);
        setDragX(0);
      }, 200);
    },
    [keyFn, onSwipe, exiting]
  );

  const handlePointerDown = useCallback(
    (e: React.PointerEvent) => {
      if (!topItem || exiting) return;
      e.preventDefault();
      isDragging.current = true;
      pointerStartX.current = e.clientX;
      dragStartX.current = dragX;
      const el = cardRef.current;
      if (el) {
        el.setPointerCapture(e.pointerId);
      }
    },
    [topItem, exiting, dragX]
  );

  const handlePointerMove = useCallback(
    (e: React.PointerEvent) => {
      if (!isDragging.current || !topItem || exiting) return;
      const dx = e.clientX - pointerStartX.current;
      // Purely horizontal drag: follow cursor left/right only
      const newX = dragStartX.current + dx;
      setDragX(newX);
    },
    [topItem, exiting]
  );

  const handlePointerUp = useCallback(
    (e: React.PointerEvent) => {
      const el = cardRef.current;
      if (el) {
        try {
          el.releasePointerCapture(e.pointerId);
        } catch {
          // ignore if already released
        }
      }
      isDragging.current = false;
      if (!topItem || exiting) return;

      // Commit based only on how far you dragged (no velocity). Works great with trackpad/mouse.
      if (dragX >= SWIPE_THRESHOLD) {
        commitSwipe(topItem, "like");
      } else if (dragX <= -SWIPE_THRESHOLD) {
        commitSwipe(topItem, "pass");
      } else {
        setDragX(0);
      }
    },
    [topItem, exiting, dragX, commitSwipe]
  );

  const handlePass = useCallback(() => {
    if (topItem && !exiting) commitSwipe(topItem, "pass");
  }, [topItem, exiting, commitSwipe]);

  const handleLike = useCallback(() => {
    if (topItem && !exiting) commitSwipe(topItem, "like");
  }, [topItem, exiting, commitSwipe]);

  if (items.length === 0) {
    return (
      <div
        className={cn(
          "flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-slate-200 bg-slate-50/80 py-16 px-6 text-center",
          className
        )}
      >
        {emptyMessage}
      </div>
    );
  }

  const rotate = Math.max(-12, Math.min(12, dragX / 15));
  const opacity = dragX !== 0 ? 1 - Math.min(0.25, Math.abs(dragX) / 400) : 1;
  const likeOverlay = dragX > 35;
  const passOverlay = dragX < -35;

  return (
    <div className={cn("relative flex flex-col", className)}>
      {/* Card area: takes all available space so the card is tall */}
      <div className="relative flex-1 min-h-0 w-full">
        {/* Card stack: back cards */}
        {restItems.slice(0, 2).map((item, i) => (
          <div
            key={keyFn(item)}
            className="absolute inset-0 flex items-center justify-center"
            style={{
              zIndex: 10 - i,
              top: (i + 1) * 8,
              left: (i + 1) * 4,
              right: (i + 1) * 4,
              bottom: -(i + 1) * 8,
              transform: `scale(${1 - (i + 1) * 0.04})`,
            }}
          >
            <div className="h-full w-full rounded-2xl overflow-hidden">
              {renderCard(item)}
            </div>
          </div>
        ))}

        {/* Top card: click and drag left or right */}
        <div
          className="absolute inset-0 flex items-center justify-center"
          style={{ zIndex: 20 }}
        >
          <div
            ref={cardRef}
            className="h-full w-full rounded-2xl overflow-hidden select-none cursor-grab active:cursor-grabbing"
            style={{
              touchAction: "none",
              transform: `translate(${dragX}px, 0) rotate(${rotate}deg)`,
              transition: exiting ? "transform 0.2s ease-out" : "none",
              opacity,
            }}
            onPointerDown={handlePointerDown}
            onPointerMove={handlePointerMove}
            onPointerUp={handlePointerUp}
            onPointerCancel={handlePointerUp}
          >
            {/* Overlay badges */}
            {likeOverlay && (
              <div className="absolute inset-0 flex items-center justify-center pointer-events-none z-10">
                <div className="rounded-2xl border-4 border-emerald-500 bg-emerald-500/20 px-8 py-4 rotate-12">
                  <span className="text-2xl font-bold text-emerald-600 flex items-center gap-2">
                    <Heart className="w-8 h-8 fill-current" />
                    Like
                  </span>
                </div>
              </div>
            )}
            {passOverlay && (
              <div className="absolute inset-0 flex items-center justify-center pointer-events-none z-10">
                <div className="rounded-2xl border-4 border-slate-400 bg-slate-400/20 px-8 py-4 -rotate-12">
                  <span className="text-2xl font-bold text-slate-600 flex items-center gap-2">
                    <X className="w-8 h-8" />
                    Pass
                  </span>
                </div>
              </div>
            )}
            <div className="pointer-events-none h-full w-full">
              {renderCard(topItem)}
            </div>
          </div>
        </div>
      </div>

      {/* Action buttons: fixed below the card area */}
      <div className="flex-shrink-0 flex items-center justify-center gap-8 pt-6 pb-2">
        <button
          type="button"
          onClick={handlePass}
          disabled={!topItem || !!exiting}
          className="w-16 h-16 rounded-full border-2 border-slate-300 bg-white text-slate-500 shadow-md active:scale-95 disabled:opacity-50 flex items-center justify-center hover:bg-slate-50 hover:border-slate-400 transition-colors"
          aria-label="Pass"
        >
          <X className="w-8 h-8" />
        </button>
        <button
          type="button"
          onClick={handleLike}
          disabled={!topItem || !!exiting}
          className="w-16 h-16 rounded-full border-2 border-emerald-400 bg-white text-emerald-600 shadow-md active:scale-95 disabled:opacity-50 flex items-center justify-center hover:bg-emerald-50 hover:border-emerald-500 transition-colors"
          aria-label="Like"
        >
          <Heart className="w-8 h-8" />
        </button>
      </div>
      <p className="text-center text-slate-600 text-sm mt-3 font-medium">
        Drag &amp; drop the card
      </p>
      <p className="text-center text-slate-500 text-xs mt-1">
        Drag right to like · Drag left to pass · Or use the buttons above
      </p>
    </div>
  );
}
