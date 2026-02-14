export interface StateRequirement {
  state: string;
  abbreviation: string;
  certifications: string[];
  requiredHours: number;
  requiredSkills: string[];
  continuingEducation: string;
  reciprocity: string[];
  adoptedCode: string;
  notes: string;
}

export const STATE_REQUIREMENTS: Record<string, StateRequirement> = {
  California: {
    state: 'California',
    abbreviation: 'CA',
    certifications: ['C-10 Electrical Contractor License', 'General Electrician Certification', 'Residential Electrician Certification'],
    requiredHours: 8000,
    requiredSkills: ['General wiring', 'Grounding & bonding', 'Conduit bending', 'Service installations', 'Panel work', 'GFCI/AFCI systems', 'Seismic bracing', 'Solar/renewable energy systems', 'EV charging installations', 'Fire alarm systems'],
    continuingEducation: '32 hours every 3 years',
    reciprocity: ['Nevada (partial)', 'Arizona (partial)'],
    adoptedCode: 'NEC 2023 with California Amendments (Title 24)',
    notes: 'California requires seismic bracing certification and renewable energy training not required in other states.',
  },
  Texas: {
    state: 'Texas',
    abbreviation: 'TX',
    certifications: ['Master Electrician License', 'Journeyman Electrician License', 'Apprentice Registration'],
    requiredHours: 8000,
    requiredSkills: ['General wiring', 'Grounding & bonding', 'Conduit bending', 'Service installations', 'Panel work', 'GFCI/AFCI systems', 'Motor controls', 'Commercial installations'],
    continuingEducation: '4 hours annually',
    reciprocity: ['Louisiana (full)', 'Oklahoma (partial)'],
    adoptedCode: 'NEC 2023 with Texas amendments',
    notes: 'Texas Department of Licensing and Regulation (TDLR) administers licensing. Municipal jurisdictions may have additional requirements.',
  },
  Arizona: {
    state: 'Arizona',
    abbreviation: 'AZ',
    certifications: ['Journeyman Electrician License', 'Residential Electrician License', 'Commercial Electrician License'],
    requiredHours: 8000,
    requiredSkills: ['General wiring', 'Grounding & bonding', 'Conduit bending', 'Service installations', 'Panel work', 'GFCI/AFCI systems', 'Low voltage systems', 'Swimming pool electrical'],
    continuingEducation: '16 hours every 2 years',
    reciprocity: ['Nevada (partial)', 'New Mexico (partial)'],
    adoptedCode: 'NEC 2023 with Arizona amendments',
    notes: 'Arizona Registrar of Contractors handles licensing. Some cities (Phoenix, Tucson) have additional local amendments.',
  },
};

export interface GapAnalysisResult {
  requirement: string;
  status: 'satisfied' | 'gap' | 'partial';
  details: string;
  courseSuggestion?: string;
  coursePrice?: string;
  courseHours?: string;
}

export function getGapAnalysis(
  userSkills: string[],
  targetState: string
): { gaps: GapAnalysisResult[]; overallMatch: number } {
  const stateReqs = STATE_REQUIREMENTS[targetState];
  if (!stateReqs) return { gaps: [], overallMatch: 0 };

  const normalizedUserSkills = userSkills.map((s) => s.toLowerCase());

  const gaps: GapAnalysisResult[] = stateReqs.requiredSkills.map((req) => {
    const reqLower = req.toLowerCase();
    const hasExact = normalizedUserSkills.some((s) => s.includes(reqLower) || reqLower.includes(s));
    const hasPartial = normalizedUserSkills.some((s) => {
      const words = reqLower.split(/\s+/);
      return words.some((w) => w.length > 3 && s.includes(w));
    });

    if (hasExact) {
      return { requirement: req, status: 'satisfied' as const, details: 'Verified through portfolio analyses' };
    }
    if (hasPartial) {
      return {
        requirement: req,
        status: 'partial' as const,
        details: 'Related experience found — additional verification needed',
        courseSuggestion: `${req} Certification Course`,
        coursePrice: '$89',
        courseHours: '4',
      };
    }
    return {
      requirement: req,
      status: 'gap' as const,
      details: `No verified experience in ${req}`,
      courseSuggestion: `${req} Training & Certification`,
      coursePrice: req.includes('Solar') || req.includes('Seismic') ? '$149' : '$89',
      courseHours: req.includes('Solar') || req.includes('Seismic') ? '8' : '4',
    };
  });

  const satisfied = gaps.filter((g) => g.status === 'satisfied').length;
  const partial = gaps.filter((g) => g.status === 'partial').length;
  const overallMatch = Math.round(((satisfied + partial * 0.5) / gaps.length) * 100);

  return { gaps, overallMatch };
}
