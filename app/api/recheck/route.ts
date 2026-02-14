import { NextRequest, NextResponse } from 'next/server';
import { v4 as uuidv4 } from 'uuid';
import { recheckPhoto } from '@/lib/claude';
import { buildRecheckPrompt } from '@/lib/prompts/recheck';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    const {
      originalImage,
      fixedImage,
      originalViolations,
      jurisdiction = 'California',
      userDescription,
    } = body;

    if (!originalImage) {
      return NextResponse.json(
        { error: 'Missing required field: originalImage' },
        { status: 400 }
      );
    }

    if (!fixedImage) {
      return NextResponse.json(
        { error: 'Missing required field: fixedImage' },
        { status: 400 }
      );
    }

    if (!originalViolations || !Array.isArray(originalViolations)) {
      return NextResponse.json(
        {
          error:
            'Missing or invalid required field: originalViolations (must be an array)',
        },
        { status: 400 }
      );
    }

    if (originalViolations.length === 0) {
      return NextResponse.json(
        {
          error:
            'originalViolations array is empty. Nothing to re-check.',
        },
        { status: 400 }
      );
    }

    const systemPrompt = buildRecheckPrompt(
      jurisdiction,
      originalViolations,
      userDescription
    );

    const result = await recheckPhoto(
      originalImage,
      fixedImage,
      originalViolations,
      systemPrompt
    );

    const recheckId = uuidv4();

    return NextResponse.json({
      id: recheckId,
      timestamp: new Date().toISOString(),
      jurisdiction,
      ...result,
    });
  } catch (error) {
    console.error('Recheck API error:', error);

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
