import { NextRequest, NextResponse } from 'next/server';
import { v4 as uuidv4 } from 'uuid';
import { analyzePhoto, analyzeBeforeAfter } from '@/lib/claude';
import { buildAnalysisPrompt } from '@/lib/prompts/analyze';
import { buildBeforeAfterPrompt } from '@/lib/prompts/before-after';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    const {
      image,
      beforeImage,
      afterImage,
      workType,
      userDescription,
      jurisdiction = 'California',
    } = body;

    if (!workType) {
      return NextResponse.json(
        { error: 'Missing required field: workType' },
        { status: 400 }
      );
    }

    if (!userDescription) {
      return NextResponse.json(
        { error: 'Missing required field: userDescription' },
        { status: 400 }
      );
    }

    const analysisId = uuidv4();

    // Before/After mode
    if (beforeImage && afterImage) {
      const systemPrompt = buildBeforeAfterPrompt(
        jurisdiction,
        'electrical',
        workType,
        userDescription
      );

      const result = await analyzeBeforeAfter(
        beforeImage,
        afterImage,
        systemPrompt,
        userDescription,
        workType
      );

      return NextResponse.json({
        id: analysisId,
        timestamp: new Date().toISOString(),
        jurisdiction,
        workType,
        mode: 'before_after',
        ...result,
      });
    }

    // Single photo mode (legacy)
    if (!image) {
      return NextResponse.json(
        { error: 'Missing required field: image (or beforeImage + afterImage)' },
        { status: 400 }
      );
    }

    const systemPrompt = buildAnalysisPrompt(
      jurisdiction,
      'electrical',
      workType,
      userDescription,
      !!beforeImage
    );

    const result = await analyzePhoto(
      image,
      systemPrompt,
      userDescription,
      workType,
      beforeImage || undefined
    );

    return NextResponse.json({
      id: analysisId,
      timestamp: new Date().toISOString(),
      jurisdiction,
      workType,
      mode: 'single',
      ...result,
    });
  } catch (error) {
    console.error('Analysis API error:', error);

    if (error instanceof SyntaxError) {
      return NextResponse.json(
        { error: 'Invalid request body: expected JSON' },
        { status: 400 }
      );
    }

    const message =
      error instanceof Error ? error.message : 'Internal server error';

    if (message.includes('Failed to parse Claude response')) {
      return NextResponse.json(
        { error: 'AI response parsing failed. Please try again.' },
        { status: 502 }
      );
    }

    return NextResponse.json({ error: message }, { status: 500 });
  }
}
