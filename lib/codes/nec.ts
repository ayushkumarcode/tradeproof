export interface NECSection {
  code: string;
  title: string;
  requirement: string;
  visualIndicators: string[];
  fixInstructions: string;
  severity: 'critical' | 'major' | 'minor';
  applicableWorkTypes: string[];
}

export const NEC_SECTIONS: Record<string, NECSection> = {
  '110.12': {
    code: '110.12',
    title: 'Mechanical Execution of Work',
    requirement: 'Electrical equipment shall be installed in a neat and workmanlike manner.',
    visualIndicators: [
      'sloppy or disorganized wiring',
      'cables not neatly routed',
      'visible damage to insulation',
      'crooked devices or covers',
      'excessive wire loops or tangles',
    ],
    fixInstructions: 'Re-route and organize all conductors neatly. Replace any damaged components. Ensure all devices are mounted straight and securely.',
    severity: 'minor',
    applicableWorkTypes: ['junction_box', 'panel', 'outlet', 'conduit', 'grounding', 'service_entrance', 'lighting', 'general'],
  },
  '110.14(B)': {
    code: '110.14(B)',
    title: 'Terminal Connections — Splices',
    requirement: 'Splices shall be made with devices identified for the purpose. All splices and joints shall be covered with insulation equivalent to that of the conductors.',
    visualIndicators: [
      'exposed copper at wire nut connections',
      'wire nut not fully seated or twisted on',
      'electrical tape used instead of proper connectors',
      'bare wire visible beyond splice point',
      'push-in connectors used improperly',
    ],
    fixInstructions: 'Remove wire nut, re-strip conductor to proper length (5/8" typically), re-twist ensuring no bare copper is visible beyond the wire nut base. Use listed connectors only.',
    severity: 'major',
    applicableWorkTypes: ['junction_box', 'outlet', 'lighting', 'general'],
  },
  '200.7': {
    code: '200.7',
    title: 'Neutral Conductor Identification',
    requirement: 'The grounded conductor (neutral) shall be identified by a continuous white or gray outer finish.',
    visualIndicators: [
      'neutral wire not white or gray',
      'white wire used as hot conductor without re-identification',
      'color coding inconsistency',
    ],
    fixInstructions: 'Ensure all neutral conductors are white or gray. If a white conductor is used as a hot, re-identify it with tape or paint at all visible points per 200.7(C).',
    severity: 'major',
    applicableWorkTypes: ['junction_box', 'outlet', 'panel', 'lighting', 'general'],
  },
  '210.8': {
    code: '210.8',
    title: 'GFCI Protection',
    requirement: 'GFCI protection required for receptacles in bathrooms, garages, outdoors, crawl spaces, basements, kitchens (within 6 feet of sink), laundry areas, and boathouses.',
    visualIndicators: [
      'standard receptacle in wet/damp location',
      'no GFCI receptacle or breaker visible',
      'GFCI at end of circuit instead of beginning',
      'missing GFCI test/reset buttons on outlet near water',
    ],
    fixInstructions: 'Install GFCI protection either at the receptacle (GFCI outlet) or at the breaker panel (GFCI breaker). GFCI must be first device in circuit to protect downstream outlets.',
    severity: 'critical',
    applicableWorkTypes: ['outlet', 'panel', 'general'],
  },
  '210.12': {
    code: '210.12',
    title: 'Arc-Fault Circuit-Interrupter Protection',
    requirement: 'AFCI protection required for all 120V, single-phase, 15- and 20-amp branch circuits supplying outlets in dwelling unit bedrooms, living rooms, hallways, closets, and similar rooms.',
    visualIndicators: [
      'standard breaker where AFCI required',
      'no AFCI indication on breaker',
      'bedroom or living room circuit without AFCI',
    ],
    fixInstructions: 'Replace standard breaker with combination AFCI breaker, or install AFCI receptacle as first outlet in the circuit.',
    severity: 'critical',
    applicableWorkTypes: ['panel', 'outlet', 'general'],
  },
  '250.24': {
    code: '250.24',
    title: 'Grounding Electrode System',
    requirement: 'A grounding electrode conductor shall connect the equipment grounding conductors, the service-equipment enclosures, and the grounded service conductor to the grounding electrode.',
    visualIndicators: [
      'missing grounding electrode conductor',
      'loose connection at grounding rod',
      'grounding conductor not properly bonded to panel',
      'improper grounding rod installation',
    ],
    fixInstructions: 'Install proper grounding electrode conductor from service panel to grounding electrode. Ensure all connections are tight and use listed clamps.',
    severity: 'critical',
    applicableWorkTypes: ['grounding', 'service_entrance', 'panel'],
  },
  '250.119': {
    code: '250.119',
    title: 'Equipment Grounding Conductor Identification',
    requirement: 'Equipment grounding conductors shall be identified by green, green with yellow stripes, or bare conductor.',
    visualIndicators: [
      'ground wire wrong color (not green or bare)',
      'green wire used for hot or neutral',
      'ground wire missing from device',
      'ground wire not connected to box or device',
    ],
    fixInstructions: 'Ensure all equipment grounding conductors are green, green with yellow stripe, or bare copper. Connect ground to device green screw and metal box.',
    severity: 'major',
    applicableWorkTypes: ['junction_box', 'outlet', 'grounding', 'panel', 'general'],
  },
  '300.4': {
    code: '300.4',
    title: 'Protection Against Physical Damage',
    requirement: 'Cables through wood framing shall be protected by steel nail plates where cable is within 1-1/4 inches of the framing edge.',
    visualIndicators: [
      'missing nail plates on wood studs',
      'cables visible near edge of framing without protection',
      'NM cable through metal stud without bushings',
    ],
    fixInstructions: 'Install 1/16" thick steel nail plates wherever cables pass through studs within 1-1/4" of the edge. Use listed bushings for metal framing.',
    severity: 'major',
    applicableWorkTypes: ['general', 'conduit'],
  },
  '300.14': {
    code: '300.14',
    title: 'Length of Free Conductors',
    requirement: 'At each outlet, junction, and switch point, at least 6 inches of free conductor shall be left for splices or connection. Conductors shall extend at least 3 inches outside the box.',
    visualIndicators: [
      'conductors cut too short inside box',
      'wires barely reaching connectors',
      'insufficient slack for proper connection',
      'wire pulled tight to terminal',
    ],
    fixInstructions: 'Pull additional cable into box to provide minimum 6 inches of free conductor measured from where cable enters box. At least 3 inches must extend beyond box opening.',
    severity: 'major',
    applicableWorkTypes: ['junction_box', 'outlet', 'lighting', 'general'],
  },
  '310.10': {
    code: '310.10',
    title: 'Conductor Identification',
    requirement: 'Conductors shall be distinguishable from each other by color coding or other approved means.',
    visualIndicators: [
      'same color wire used for different purposes',
      'no color coding on conductors',
      'white wire used as hot without re-marking',
    ],
    fixInstructions: 'Use proper color coding: black/red for hot, white for neutral, green/bare for ground. Re-identify any repurposed conductors with colored tape.',
    severity: 'minor',
    applicableWorkTypes: ['junction_box', 'panel', 'outlet', 'general'],
  },
  '314.16': {
    code: '314.16',
    title: 'Box Fill Calculations',
    requirement: 'Boxes shall be of sufficient size to provide free space for all enclosed conductors. Each conductor counts as one or more volume allowances.',
    visualIndicators: [
      'too many wires crammed into box',
      'box appears overfull',
      'difficulty closing box cover due to wire volume',
      'conductors bunched and compressed',
    ],
    fixInstructions: 'Count all conductors, clamps, devices, and ground wires. Calculate required box volume per NEC 314.16 table. Upgrade to larger box if overfilled.',
    severity: 'major',
    applicableWorkTypes: ['junction_box', 'outlet', 'lighting', 'general'],
  },
  '314.17': {
    code: '314.17',
    title: 'Conductors Entering Boxes',
    requirement: 'Cables shall be secured to the box with approved cable clamps. Cable sheath must extend at least 1/4 inch inside the box.',
    visualIndicators: [
      'missing cable clamps at box entry',
      'cables loosely entering box',
      'cable sheath not visible inside box',
      'knockout open without cable or cover',
    ],
    fixInstructions: 'Install approved cable clamps at each box entry point. Ensure NM cable sheath extends at least 1/4" inside box. Cover unused knockouts.',
    severity: 'major',
    applicableWorkTypes: ['junction_box', 'outlet', 'lighting', 'general'],
  },
  '314.28': {
    code: '314.28',
    title: 'Pull and Junction Box Sizing',
    requirement: 'Pull boxes shall be sized according to the raceway or cable size. For straight pulls, box length shall be at least 8x the trade size of the largest raceway.',
    visualIndicators: [
      'pull box too small for conduit size',
      'conductors cramped in pull box',
      'inadequate bending space',
    ],
    fixInstructions: 'Calculate required box dimensions based on conduit trade size and pull type (straight vs angle). Install appropriately sized box.',
    severity: 'major',
    applicableWorkTypes: ['conduit', 'junction_box'],
  },
  '334.30': {
    code: '334.30',
    title: 'NM Cable Securing and Supporting',
    requirement: 'NM cable shall be secured within 12 inches of every box and supported at intervals not exceeding 4-1/2 feet.',
    visualIndicators: [
      'NM cable hanging loose without staples',
      'cable not secured near box entry',
      'excessive unsupported cable runs',
      'cable stapled too far from box',
    ],
    fixInstructions: 'Staple NM cable within 12 inches of each box entry. Add staples every 4.5 feet along the run. Use proper NM staples — do not damage cable sheath.',
    severity: 'minor',
    applicableWorkTypes: ['junction_box', 'outlet', 'lighting', 'general'],
  },
  '404.2': {
    code: '404.2',
    title: 'Switch Connections',
    requirement: 'Single-pole switches shall be wired so that they disconnect the hot conductor. Grounded (neutral) conductor shall not be switched.',
    visualIndicators: [
      'white wire connected to switch without re-identification',
      'switch appears to be switching neutral',
      'missing neutral in switch box (required since NEC 2011)',
    ],
    fixInstructions: 'Ensure switch interrupts the ungrounded (hot) conductor only. Provide neutral conductor in switch box. Re-identify white conductor used as switch leg.',
    severity: 'major',
    applicableWorkTypes: ['outlet', 'lighting', 'general'],
  },
  '406.4(D)': {
    code: '406.4(D)',
    title: 'Receptacle GFCI Replacement',
    requirement: 'Where replacements are made at locations where GFCI protection is required, GFCI-protected receptacles shall be installed.',
    visualIndicators: [
      'standard receptacle in GFCI-required location',
      'old two-prong outlet replaced without GFCI',
      'receptacle near sink without GFCI protection',
    ],
    fixInstructions: 'Install GFCI-protected receptacle when replacing any receptacle in a location where 210.8 requires GFCI protection.',
    severity: 'critical',
    applicableWorkTypes: ['outlet', 'general'],
  },
  '406.12': {
    code: '406.12',
    title: 'Tamper-Resistant Receptacles',
    requirement: 'All 125V, 15- and 20-amp receptacles in dwelling units shall be tamper-resistant (TR).',
    visualIndicators: [
      'receptacle without TR marking',
      'non-tamper-resistant receptacle in dwelling',
      'missing TR designation on outlet face',
    ],
    fixInstructions: 'Replace with tamper-resistant (TR) rated receptacle. Look for "TR" marking on receptacle face.',
    severity: 'minor',
    applicableWorkTypes: ['outlet', 'general'],
  },
  '408.4': {
    code: '408.4',
    title: 'Circuit Directory — Panel Labeling',
    requirement: 'Every circuit and circuit modification shall be legibly identified as to its clear, evident, and specific purpose. The identification shall include sufficient detail to allow each circuit to be distinguished from all others.',
    visualIndicators: [
      'panel without circuit directory',
      'blank or illegible panel schedule',
      'vague circuit labels like "misc" or "spare"',
      'handwritten labels that are hard to read',
    ],
    fixInstructions: 'Create clear, typed circuit directory identifying every circuit by specific location and load. Mount inside panel door.',
    severity: 'minor',
    applicableWorkTypes: ['panel'],
  },
  '410.16': {
    code: '410.16',
    title: 'Luminaire Support',
    requirement: 'Luminaires and lampholders shall be securely supported. A luminaire that weighs more than 6 lbs shall be supported independently of the outlet box unless the box is listed for the weight.',
    visualIndicators: [
      'light fixture hanging by wires only',
      'no visible mounting hardware',
      'heavy fixture on standard box',
      'fixture not flush or secure to ceiling/wall',
    ],
    fixInstructions: 'Mount luminaire to listed box or independent support. For fixtures over 6 lbs or ceiling fans, use fan-rated box with proper bracing.',
    severity: 'major',
    applicableWorkTypes: ['lighting', 'general'],
  },
  '422.16': {
    code: '422.16',
    title: 'Flexible Cord Connection — Appliances',
    requirement: 'Flexible cords used to connect appliances shall be suitable for the environment and properly secured with strain relief.',
    visualIndicators: [
      'cord without strain relief at appliance',
      'damaged or frayed appliance cord',
      'cord run through wall or ceiling',
      'undersized cord for appliance load',
    ],
    fixInstructions: 'Install proper strain relief fitting. Replace damaged cords. Never run flexible cords through walls, ceilings, or floors.',
    severity: 'major',
    applicableWorkTypes: ['general', 'lighting'],
  },
  '480.4': {
    code: '480.4',
    title: 'Battery Systems — Ventilation and Protection',
    requirement: 'Battery systems shall be installed in areas with adequate ventilation and shall be protected against physical damage.',
    visualIndicators: [
      'batteries in unventilated enclosure',
      'battery terminals exposed without cover',
      'no overcurrent protection visible for battery system',
    ],
    fixInstructions: 'Ensure adequate ventilation for battery gases. Install terminal covers. Provide listed overcurrent protection for battery circuits.',
    severity: 'critical',
    applicableWorkTypes: ['service_entrance', 'general'],
  },
  '110.26': {
    code: '110.26',
    title: 'Working Space About Electrical Equipment',
    requirement: 'Sufficient access and working space shall be provided around all electrical equipment. Minimum 3 feet clear in front of panels.',
    visualIndicators: [
      'items stored in front of electrical panel',
      'panel access appears blocked',
      'insufficient clearance visible around equipment',
    ],
    fixInstructions: 'Clear minimum 36 inches of working space in front of panel, 30 inches wide, floor to ceiling or 6.5 feet height. NOTE: This violation may be difficult to assess from a close-up photo.',
    severity: 'major',
    applicableWorkTypes: ['panel', 'service_entrance'],
  },
};

const WORK_TYPE_MAP: Record<string, string[]> = {
  junction_box: ['110.12', '110.14(B)', '200.7', '250.119', '300.14', '310.10', '314.16', '314.17', '334.30'],
  panel: ['110.12', '110.26', '200.7', '210.8', '210.12', '250.24', '250.119', '310.10', '408.4'],
  outlet: ['110.12', '110.14(B)', '200.7', '210.8', '250.119', '300.14', '314.16', '314.17', '404.2', '406.4(D)', '406.12'],
  conduit: ['110.12', '300.4', '314.28', '334.30'],
  grounding: ['250.24', '250.119'],
  service_entrance: ['110.12', '110.26', '250.24', '480.4'],
  lighting: ['110.12', '110.14(B)', '300.14', '314.16', '314.17', '410.16', '422.16'],
  general: ['110.12', '110.14(B)', '200.7', '210.8', '250.119', '300.4', '300.14', '314.16', '314.17', '334.30', '406.12'],
};

export function getRelevantCodes(workType: string): NECSection[] {
  const codes = WORK_TYPE_MAP[workType] || WORK_TYPE_MAP['general'];
  return codes.map((code) => NEC_SECTIONS[code]).filter(Boolean);
}

export function getCodesForJurisdiction(jurisdiction: string, workType?: string): string {
  const sections = workType ? getRelevantCodes(workType) : Object.values(NEC_SECTIONS);

  return sections
    .map(
      (s) =>
        `### NEC ${s.code} — ${s.title} [${s.severity.toUpperCase()}]
Requirement: ${s.requirement}
Visual Indicators: ${s.visualIndicators.join('; ')}
Fix: ${s.fixInstructions}`
    )
    .join('\n\n');
}
