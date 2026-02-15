import Anthropic from '@anthropic-ai/sdk';

const client = new Anthropic();

type ImageMediaType = 'image/jpeg' | 'image/png';

function detectMediaType(base64Image: string): ImageMediaType {
  if (base64Image.startsWith('iVBOR')) {
    return 'image/png';
  }
  return 'image/jpeg';
}

function stripDataUrlPrefix(base64Image: string): string {
  const match = base64Image.match(/^data:image\/[a-zA-Z]+;base64,(.+)$/);
  if (match) {
    return match[1];
  }
  return base64Image;
}

export interface AnalysisResult {
  description: string;
  is_compliant: boolean;
  compliance_score: number;
  violations: {
    description: string;
    code_section: string;
    local_amendment: string | null;
    severity: 'critical' | 'major' | 'minor';
    confidence: 'high' | 'medium' | 'low';
    fix_instruction: string;
    why_this_matters: string;
    visual_evidence: string;
  }[];
  correct_items: string[];
  skills_demonstrated: {
    skill: string;
    quality: 'good' | 'acceptable' | 'needs_work';
  }[];
  overall_assessment: string;
  work_type_detected: string;
}

export interface BeforeAfterResult {
  description: string;
  before_score: number;
  after_score: number;
  is_compliant: boolean;
  violations_found: {
    id: number;
    description: string;
    code_section: string;
    severity: 'critical' | 'major' | 'minor';
    status: 'unresolved' | 'new';
    fix_instruction: string;
    why_this_matters: string;
    location_x: number;
    location_y: number;
  }[];
  resolved_items: string[];
  skills_demonstrated: {
    skill: string;
    quality: 'good' | 'acceptable' | 'needs_work';
  }[];
  overall_assessment: string;
}

export interface RecheckResult {
  description: string;
  is_compliant: boolean;
  compliance_score: number;
  original_violation_status: {
    original_description: string;
    original_code_section: string;
    status: 'resolved' | 'partially_resolved' | 'unresolved';
    notes: string;
  }[];
  new_violations_found: {
    description: string;
    code_section: string;
    severity: string;
    fix_instruction: string;
  }[];
  overall_assessment: string;
}

export async function analyzePhoto(
  base64Image: string,
  systemPrompt: string,
  userDescription: string,
  workType: string,
  beforeBase64Image?: string
): Promise<AnalysisResult> {
  try {
    const cleanBase64 = stripDataUrlPrefix(base64Image);
    const mediaType = detectMediaType(cleanBase64);

    const contentParts: Anthropic.MessageCreateParams['messages'][0]['content'] = [];

    // If a before image is provided, include it first for comparison
    if (beforeBase64Image) {
      const cleanBefore = stripDataUrlPrefix(beforeBase64Image);
      const beforeMediaType = detectMediaType(cleanBefore);
      contentParts.push(
        {
          type: 'text',
          text: 'Here is the BEFORE photo (before work was performed):',
        },
        {
          type: 'image',
          source: {
            type: 'base64',
            media_type: beforeMediaType,
            data: cleanBefore,
          },
        },
        {
          type: 'text',
          text: 'Here is the AFTER photo (after work was completed):',
        }
      );
    }

    contentParts.push(
      {
        type: 'image',
        source: {
          type: 'base64',
          media_type: mediaType,
          data: cleanBase64,
        },
      },
      {
        type: 'text',
        text: beforeBase64Image
          ? `Work type: ${workType}\n\nDescription from the worker: ${userDescription}\n\nPlease compare the before and after photos, analyze the completed work for code compliance, and respond with the JSON format specified in your instructions.`
          : `Work type: ${workType}\n\nDescription from the worker: ${userDescription}\n\nPlease analyze this photo for code compliance and respond with the JSON format specified in your instructions.`,
      }
    );

    const response = await client.messages.create({
      model: 'claude-sonnet-4-5-20250929',
      max_tokens: 4096,
      system: systemPrompt,
      messages: [
        {
          role: 'user',
          content: contentParts,
        },
      ],
    });

    const textBlock = response.content.find((block) => block.type === 'text');
    if (!textBlock || textBlock.type !== 'text') {
      throw new Error('No text response received from Claude');
    }

    const rawText = textBlock.text;
    const jsonMatch = rawText.match(/```(?:json)?\s*([\s\S]*?)```/);
    const jsonString = jsonMatch ? jsonMatch[1].trim() : rawText.trim();

    const result: AnalysisResult = JSON.parse(jsonString);
    return result;
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error('Failed to parse Claude response as JSON');
    }
    throw error;
  }
}

export async function analyzeBeforeAfter(
  beforeBase64: string,
  afterBase64: string,
  systemPrompt: string,
  userDescription: string,
  workType: string
): Promise<BeforeAfterResult> {
  try {
    const cleanBefore = stripDataUrlPrefix(beforeBase64);
    const cleanAfter = stripDataUrlPrefix(afterBase64);
    const beforeMediaType = detectMediaType(cleanBefore);
    const afterMediaType = detectMediaType(cleanAfter);

    const response = await client.messages.create({
      model: 'claude-sonnet-4-5-20250929',
      max_tokens: 4096,
      system: systemPrompt,
      messages: [
        {
          role: 'user',
          content: [
            {
              type: 'text',
              text: 'Here is the BEFORE photo (original work):',
            },
            {
              type: 'image',
              source: {
                type: 'base64',
                media_type: beforeMediaType,
                data: cleanBefore,
              },
            },
            {
              type: 'text',
              text: 'Here is the AFTER photo (work after fixes):',
            },
            {
              type: 'image',
              source: {
                type: 'base64',
                media_type: afterMediaType,
                data: cleanAfter,
              },
            },
            {
              type: 'text',
              text: `Work type: ${workType}\n\nDescription: ${userDescription}\n\nPlease compare both photos and respond with the JSON format specified in your instructions.`,
            },
          ],
        },
      ],
    });

    const textBlock = response.content.find((block) => block.type === 'text');
    if (!textBlock || textBlock.type !== 'text') {
      throw new Error('No text response received from Claude');
    }

    const rawText = textBlock.text;
    const jsonMatch = rawText.match(/```(?:json)?\s*([\s\S]*?)```/);
    const jsonString = jsonMatch ? jsonMatch[1].trim() : rawText.trim();

    const result: BeforeAfterResult = JSON.parse(jsonString);
    return result;
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error('Failed to parse Claude response as JSON');
    }
    throw error;
  }
}

export async function recheckPhoto(
  originalBase64: string,
  fixedBase64: string,
  originalViolations: AnalysisResult['violations'],
  systemPrompt: string
): Promise<RecheckResult> {
  try {
    const cleanOriginal = stripDataUrlPrefix(originalBase64);
    const cleanFixed = stripDataUrlPrefix(fixedBase64);
    const originalMediaType = detectMediaType(cleanOriginal);
    const fixedMediaType = detectMediaType(cleanFixed);

    const response = await client.messages.create({
      model: 'claude-sonnet-4-5-20250929',
      max_tokens: 4096,
      system: systemPrompt,
      messages: [
        {
          role: 'user',
          content: [
            {
              type: 'text',
              text: 'Here is the ORIGINAL photo (before fixes):',
            },
            {
              type: 'image',
              source: {
                type: 'base64',
                media_type: originalMediaType,
                data: cleanOriginal,
              },
            },
            {
              type: 'text',
              text: 'Here is the FIXED photo (after fixes):',
            },
            {
              type: 'image',
              source: {
                type: 'base64',
                media_type: fixedMediaType,
                data: cleanFixed,
              },
            },
            {
              type: 'text',
              text: `Original violations found:\n${JSON.stringify(originalViolations, null, 2)}\n\nPlease compare the original and fixed photos, evaluate each original violation, check for new issues, and respond with the JSON format specified in your instructions.`,
            },
          ],
        },
      ],
    });

    const textBlock = response.content.find((block) => block.type === 'text');
    if (!textBlock || textBlock.type !== 'text') {
      throw new Error('No text response received from Claude');
    }

    const rawText = textBlock.text;
    const jsonMatch = rawText.match(/```(?:json)?\s*([\s\S]*?)```/);
    const jsonString = jsonMatch ? jsonMatch[1].trim() : rawText.trim();

    const result: RecheckResult = JSON.parse(jsonString);
    return result;
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error('Failed to parse Claude response as JSON');
    }
    throw error;
  }
}
