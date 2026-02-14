export function buildRecheckPrompt(
  jurisdiction: string,
  originalViolations: {
    description: string;
    code_section: string;
    severity: string;
    fix_instruction: string;
    [key: string]: unknown;
  }[],
  userDescription?: string
): string {
  const violationsList = originalViolations
    .map(
      (v, i) =>
        `${i + 1}. [${v.severity.toUpperCase()}] ${v.description}
   Code Section: ${v.code_section}
   Required Fix: ${v.fix_instruction}`
    )
    .join('\n');

  const descriptionSection = userDescription
    ? `\n## Worker's Description of Fixes\n"${userDescription}"\n`
    : '';

  return `You are a certified electrical code compliance inspector performing a RE-CHECK inspection. The worker previously submitted a photo that had code violations. They have now submitted a new photo showing their fixes. You must compare the original photo with the fixed photo to determine if violations have been addressed.

## Jurisdiction
${jurisdiction}

## Original Violations Found
The following violations were identified in the original inspection:
${violationsList}
${descriptionSection}
## Instructions

1. Compare the ORIGINAL photo (first image) with the FIXED photo (second image).
2. For EACH original violation listed above, determine its status:
   - "resolved": The violation has been clearly and properly fixed.
   - "partially_resolved": Some effort was made to fix the violation, but it is not fully corrected.
   - "unresolved": The violation still appears to be present with no meaningful change.
3. Check for any NEW violations that may have been introduced during the fix process.
4. Provide an updated compliance score based on the current state.

## Required Response Format
Respond with ONLY valid JSON in this exact format (no markdown, no explanation outside the JSON):

{
  "description": "A detailed description of what you see in the fixed photo and how it compares to the original",
  "is_compliant": false,
  "compliance_score": 85,
  "original_violation_status": [
    {
      "original_description": "Description of the original violation",
      "original_code_section": "NEC xxx.xx",
      "status": "resolved|partially_resolved|unresolved",
      "notes": "Details about what was fixed or what still needs attention"
    }
  ],
  "new_violations_found": [
    {
      "description": "Description of a new violation introduced during the fix",
      "code_section": "NEC xxx.xx",
      "severity": "critical|major|minor",
      "fix_instruction": "How to fix this new violation"
    }
  ],
  "overall_assessment": "Summary of the re-check results, progress made, and any remaining work needed"
}`;
}
