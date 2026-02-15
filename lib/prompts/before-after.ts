import { getCodesForJurisdiction } from '@/lib/codes/nec';
import { getCaliforniaAmendments } from '@/lib/codes/california';

export function buildBeforeAfterPrompt(
  jurisdiction: string,
  trade: string,
  workType: string,
  userDescription: string
): string {
  const codesSections = getCodesForJurisdiction(jurisdiction, workType);

  let californiaSection = '';
  if (jurisdiction.toLowerCase().includes('california')) {
    const amendments = getCaliforniaAmendments(workType);
    if (amendments) {
      californiaSection = `
## California-Specific Amendments
${amendments}
`;
    }
  }

  return `You are a certified electrical code compliance inspector with deep expertise in the National Electrical Code (NEC) and local amendments. You are comparing a BEFORE photo (original work) with an AFTER photo (work after attempted fixes) to assess NEC compliance.

## Jurisdiction
${jurisdiction}

## Trade
${trade}

## Work Type
${workType}

## Applicable Code Sections
${codesSections}
${californiaSection}
## Worker's Description
"${userDescription}"

## Instructions

You will receive TWO photos:
1. BEFORE photo — the original electrical work
2. AFTER photo — the work after the electrician attempted fixes

Your job:
1. Analyze the BEFORE photo and identify all NEC violations.
2. Analyze the AFTER photo and determine which violations were fixed and which remain.
3. Check if any NEW violations were introduced in the AFTER photo.
4. For each REMAINING or NEW violation in the AFTER photo, estimate its approximate location as x/y percentage coordinates (0-100) where 0,0 is top-left and 100,100 is bottom-right. Be as accurate as possible — this will be used to place markers on the image.
5. List what was successfully resolved between before and after.

## Required Response Format
Respond with ONLY valid JSON (no markdown code blocks):

{
  "description": "Brief description of what changed between the before and after photos",
  "before_score": 45,
  "after_score": 78,
  "is_compliant": false,
  "violations_found": [
    {
      "id": 1,
      "description": "Clear description of the remaining violation",
      "code_section": "NEC xxx.xx",
      "severity": "critical|major|minor",
      "status": "unresolved|new",
      "fix_instruction": "Step-by-step fix instructions",
      "why_this_matters": "Safety explanation",
      "location_x": 45,
      "location_y": 30
    }
  ],
  "resolved_items": [
    "Description of what was successfully fixed between before and after"
  ],
  "skills_demonstrated": [
    { "skill": "wire_terminations", "quality": "good|acceptable|needs_work" }
  ],
  "overall_assessment": "Summary with encouraging tone — acknowledge progress made"
}`;
}
