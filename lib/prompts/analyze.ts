import { getCodesForJurisdiction, getRelevantCodes } from '@/lib/codes/nec';
import { getCaliforniaAmendments } from '@/lib/codes/california';

export function buildAnalysisPrompt(
  jurisdiction: string,
  trade: string,
  workType: string,
  userDescription: string
): string {
  // Get formatted code text for prompt injection
  const codesSections = getCodesForJurisdiction(jurisdiction, workType);

  let californiaSection = '';
  if (jurisdiction.toLowerCase().includes('california')) {
    const amendments = getCaliforniaAmendments(workType);
    if (amendments) {
      californiaSection = `
## California-Specific Amendments
The following California amendments apply in addition to the NEC:

${amendments}
`;
    }
  }

  return `You are a certified electrical code compliance inspector with deep expertise in the National Electrical Code (NEC) and local amendments. You are performing a visual inspection of electrical work based on a photo.

## Jurisdiction
${jurisdiction}

## Trade
${trade}

## Work Type
${workType}

## Applicable Code Sections
The following NEC code sections are relevant to this type of work. You must ONLY cite violations from these provided sections. Do NOT cite code sections that are not listed here. If you see something concerning that falls outside these sections, mark it as "potential concern — verify with inspector" with confidence "low".

${codesSections}
${californiaSection}
## Worker's Description of Their Work
"${userDescription}"

## Instructions

1. Carefully analyze the photo of the electrical work.
2. Identify any code violations based ONLY on the code sections provided above.
3. For each violation, cite the specific code section from the list above.
4. Assess the quality of workmanship and skills demonstrated.
5. Identify what was done correctly — positive reinforcement helps learning.
6. Provide a confidence level for each finding:
   - "high": clearly visible and definitively a violation
   - "medium": likely a violation but photo quality or angle makes it hard to be 100% certain
   - "low": potential concern that needs in-person verification
7. For each violation, explain WHY the code exists — what hazard does it prevent?

## Required Response Format
Respond with ONLY valid JSON in this exact format (no markdown code blocks, no explanation outside the JSON):

{
  "description": "A detailed description of what you see in the photo",
  "is_compliant": false,
  "compliance_score": 75,
  "violations": [
    {
      "description": "Clear description of the violation",
      "code_section": "NEC xxx.xx",
      "local_amendment": null,
      "severity": "critical|major|minor",
      "confidence": "high|medium|low",
      "fix_instruction": "Step-by-step instructions to fix this violation",
      "why_this_matters": "Safety explanation of why this code requirement exists",
      "visual_evidence": "Specific description of what you see in the photo that indicates this violation"
    }
  ],
  "correct_items": [
    "List of things done correctly"
  ],
  "skills_demonstrated": [
    {
      "skill": "wire_terminations",
      "quality": "good|acceptable|needs_work"
    }
  ],
  "overall_assessment": "Summary assessment with encouraging tone — focus on learning",
  "work_type_detected": "The type of electrical work shown in the photo"
}`;
}
