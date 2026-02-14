export interface CaliforniaAmendment {
  code: string;
  title: string;
  requirement: string;
  visualIndicators: string[];
  fixInstructions: string;
  severity: 'critical' | 'major' | 'minor';
  applicableWorkTypes: string[];
}

export const CALIFORNIA_AMENDMENTS: CaliforniaAmendment[] = [
  {
    code: 'CA-SEISMIC-1',
    title: 'Seismic Bracing for Electrical Equipment',
    requirement: 'All electrical panels, transformers, and heavy equipment shall be seismically braced per CBC Chapter 16 and ASCE 7. Equipment over 20 lbs must be anchored.',
    visualIndicators: [
      'panel not bolted to wall with seismic anchors',
      'missing seismic strapping on heavy equipment',
      'conduit without flexible connections at building joints',
    ],
    fixInstructions: 'Install seismic anchor bolts for panel mounting. Add flexible conduit connections at seismic joints. Brace all equipment per CBC requirements.',
    severity: 'major',
    applicableWorkTypes: ['panel', 'service_entrance'],
  },
  {
    code: 'CA-SOLAR-T24',
    title: 'Solar Readiness — Title 24',
    requirement: 'New residential construction shall include solar-ready provisions including designated roof area, conduit from roof to electrical panel, and reserved breaker spaces.',
    visualIndicators: [
      'no solar conduit pathway visible',
      'panel without reserved solar breaker spaces',
      'missing solar disconnect labeling',
    ],
    fixInstructions: 'Install minimum 1" conduit from roof area to main panel. Reserve minimum 2 breaker spaces labeled "SOLAR". Mark designated roof area on plans.',
    severity: 'major',
    applicableWorkTypes: ['panel', 'service_entrance'],
  },
  {
    code: 'CA-EV-READY',
    title: 'EV Charging Readiness',
    requirement: 'New dwelling units with attached garages shall have at least one 208/240V, 40-amp dedicated branch circuit for EV charging, or raceway to panel.',
    visualIndicators: [
      'garage without dedicated EV outlet',
      'no 240V receptacle or conduit in garage',
      'panel without reserved EV circuit space',
    ],
    fixInstructions: 'Install dedicated 40-amp, 240V circuit to garage with NEMA 14-50 receptacle or EV connector. Alternatively, install raceway from panel to garage for future EV circuit.',
    severity: 'minor',
    applicableWorkTypes: ['panel', 'outlet', 'service_entrance'],
  },
  {
    code: 'CA-ENERGY-EFF',
    title: 'Energy Efficiency — Lighting Controls',
    requirement: 'Per Title 24 Part 6, all residential lighting must be high-efficacy (LED) or controlled by dimmer, occupancy sensor, or vacancy sensor.',
    visualIndicators: [
      'incandescent fixtures without dimmer control',
      'no occupancy sensor in required locations',
      'non-high-efficacy lighting installed',
    ],
    fixInstructions: 'Install high-efficacy (LED) luminaires or add dimmer/occupancy sensor controls to meet Title 24 energy requirements.',
    severity: 'minor',
    applicableWorkTypes: ['lighting', 'general'],
  },
  {
    code: 'CA-GFCI-EXPAND',
    title: 'Expanded GFCI Requirements — California',
    requirement: 'California expands GFCI requirements beyond NEC to include all 125V/250V receptacles in garages, accessory buildings, and any location with equipment likely to be used outdoors.',
    visualIndicators: [
      'non-GFCI receptacle in garage',
      'outdoor-accessible receptacle without GFCI',
      'accessory building receptacle without GFCI protection',
    ],
    fixInstructions: 'Install GFCI protection on all receptacles in garages, accessory buildings, and outdoor-accessible locations. Use GFCI breaker or GFCI receptacle.',
    severity: 'critical',
    applicableWorkTypes: ['outlet', 'general'],
  },
];

export function getCaliforniaAmendments(workType?: string): string {
  const amendments = workType
    ? CALIFORNIA_AMENDMENTS.filter((a) => a.applicableWorkTypes.includes(workType))
    : CALIFORNIA_AMENDMENTS;

  if (amendments.length === 0) return '';

  return amendments
    .map(
      (a) =>
        `### California Amendment: ${a.code} — ${a.title} [${a.severity.toUpperCase()}]
Requirement: ${a.requirement}
Visual Indicators: ${a.visualIndicators.join('; ')}
Fix: ${a.fixInstructions}`
    )
    .join('\n\n');
}
