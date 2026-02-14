"use client";

import { useState, useRef, useCallback } from "react";
import { Camera, Upload, RotateCcw, CheckCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";

interface CameraCaptureProps {
  onCapture: (imageBase64: string) => void;
  label?: string;
  existingImage?: string;
}

export default function CameraCapture({
  onCapture,
  label = "Capture Work Photo",
  existingImage,
}: CameraCaptureProps) {
  const [preview, setPreview] = useState<string | null>(existingImage || null);
  const [isProcessing, setIsProcessing] = useState(false);
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelected = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (!file) return;

      setIsProcessing(true);
      const reader = new FileReader();
      reader.onloadend = () => {
        const base64 = reader.result as string;
        setPreview(base64);
        onCapture(base64);
        setIsProcessing(false);
      };
      reader.onerror = () => {
        setIsProcessing(false);
      };
      reader.readAsDataURL(file);
    },
    [onCapture]
  );

  const handleRetake = useCallback(() => {
    setPreview(null);
    if (cameraInputRef.current) cameraInputRef.current.value = "";
    if (galleryInputRef.current) galleryInputRef.current.value = "";
  }, []);

  return (
    <Card className="p-4">
      <p className="text-sm font-medium text-slate-700 mb-3">{label}</p>

      {/* Hidden file inputs */}
      <input
        ref={cameraInputRef}
        type="file"
        accept="image/*"
        capture="environment"
        onChange={handleFileSelected}
        className="hidden"
        aria-label="Take photo with camera"
      />
      <input
        ref={galleryInputRef}
        type="file"
        accept="image/*"
        onChange={handleFileSelected}
        className="hidden"
        aria-label="Upload from gallery"
      />

      {!preview ? (
        <div className="space-y-3">
          {/* Main camera button - large tap target */}
          <Button
            onClick={() => cameraInputRef.current?.click()}
            disabled={isProcessing}
            className="w-full h-20 text-lg bg-blue-600 hover:bg-blue-700 active:bg-blue-800 text-white rounded-xl flex items-center justify-center gap-3"
          >
            <Camera className="w-7 h-7" />
            <span>Take Photo</span>
          </Button>

          {/* Gallery upload - secondary option */}
          <Button
            variant="outline"
            onClick={() => galleryInputRef.current?.click()}
            disabled={isProcessing}
            className="w-full h-14 text-base border-slate-300 text-slate-600 rounded-xl flex items-center justify-center gap-3"
          >
            <Upload className="w-5 h-5" />
            <span>Upload from Gallery</span>
          </Button>

          {isProcessing && (
            <p className="text-center text-sm text-slate-500 animate-pulse">
              Processing image...
            </p>
          )}

          {/* Existing image indicator for re-check */}
          {existingImage && !preview && (
            <div className="flex items-center gap-2 text-sm text-blue-600 bg-blue-50 p-3 rounded-lg">
              <CheckCircle className="w-4 h-4" />
              <span>Previous photo on file. Take a new photo to re-check.</span>
            </div>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {/* Image preview */}
          <div className="relative rounded-xl overflow-hidden border border-slate-200">
            <img
              src={preview}
              alt="Captured work photo"
              className="w-full h-auto max-h-80 object-contain bg-slate-50"
            />
            <div className="absolute top-2 right-2">
              <div className="bg-green-500 text-white text-xs font-medium px-2 py-1 rounded-full flex items-center gap-1">
                <CheckCircle className="w-3 h-3" />
                Captured
              </div>
            </div>
          </div>

          {/* Retake button */}
          <Button
            variant="outline"
            onClick={handleRetake}
            className="w-full h-12 text-base border-slate-300 text-slate-600 rounded-xl flex items-center justify-center gap-2"
          >
            <RotateCcw className="w-5 h-5" />
            <span>Retake Photo</span>
          </Button>
        </div>
      )}
    </Card>
  );
}
