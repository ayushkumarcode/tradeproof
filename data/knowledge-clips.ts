export interface KnowledgeClip {
  id: string;
  expertName: string;
  expertYears: number;
  trade: string;
  taskType: string;
  triggerKeywords: string[];
  title: string;
  content: string;
  buildingEra?: string;
}

export const KNOWLEDGE_CLIPS: KnowledgeClip[] = [
  {
    id: 'clip-1',
    expertName: 'Mike Torres',
    expertYears: 32,
    trade: 'electrical',
    taskType: 'panel',
    triggerKeywords: ['federal pacific', 'fpe', 'stab-lok', 'old panel', 'panel replacement', 'breaker trip', 'panel recall'],
    title: 'Federal Pacific Panels — The Hidden Fire Hazard',
    content: "I\'ve replaced over 200 Federal Pacific panels in my career, and every single one was a ticking time bomb. These FPE Stab-Lok panels were installed in millions of homes from the 1950s through the 1980s. The breakers are notorious for not tripping during overloads — I\'ve personally tested breakers that welded themselves shut. First thing when you see one: check the bus bar connections. Pull a breaker out and look for arc damage — blackened or pitted contacts. Never trust these breakers to protect a circuit. My rule is simple: if it\'s Federal Pacific, it needs to be replaced, period. Don\'t try to add circuits, don\'t try to swap breakers. Budget for a full panel swap and do it right. Your client\'s family is sleeping behind that panel.",
    buildingEra: '1950s-1980s',
  },
  {
    id: 'clip-2',
    expertName: 'Sarah Chen',
    expertYears: 28,
    trade: 'electrical',
    taskType: 'outlet',
    triggerKeywords: ['gfci', 'afci', 'ground fault', 'arc fault', 'bathroom', 'kitchen', 'garage outlet', 'wet location', 'tripping'],
    title: 'GFCI & AFCI — The Mistakes I See Every Week',
    content: "The number one GFCI mistake I see from apprentices is putting the GFCI at the end of the circuit instead of the beginning. If your GFCI outlet is the last device on the line, it only protects itself — everything upstream is unprotected. Always put it first. The second mistake is not understanding the difference between the LINE and LOAD terminals. LINE is your power in, LOAD feeds downstream outlets. Mix those up and nothing downstream is protected. For AFCI, here\'s what most people don\'t realize: combination AFCI breakers can be sensitive. If you\'re getting nuisance trips, check for shared neutrals first — that\'s the cause 80% of the time. Also, keep your wire runs neat. Parallel conductors running side by side for long distances can cause false trips. I always tell my apprentices: GFCI protects people from shock, AFCI protects homes from fire. Both are non-negotiable.",
  },
  {
    id: 'clip-3',
    expertName: 'James Washington',
    expertYears: 35,
    trade: 'electrical',
    taskType: 'general',
    triggerKeywords: ['aluminum', 'aluminium', 'old wiring', 'older home', '1960s', '1970s', 'oxidation', 'co/alr', 'pigtail'],
    title: 'Aluminum Wiring — Handle With Extreme Care',
    content: "Aluminum wiring was used extensively in the late 1960s and 1970s when copper prices spiked. It\'s not inherently dangerous, but it requires specific handling that most electricians weren\'t trained for. The problem is the connection points — aluminum oxidizes, which creates resistance, which creates heat. I\'ve pulled outlet covers off and found scorch marks behind them from overheated aluminum connections. Rule one: never connect aluminum directly to a device rated only for copper. Look for CO/ALR ratings on switches and receptacles. Rule two: use anti-oxidant compound (like Noalox) on every aluminum connection. Rule three: never use push-in terminals with aluminum — side-screw only, torqued properly. If you\'re working in a 1960s-70s home and see the telltale silver wiring, slow down. This isn\'t a quick outlet swap anymore. Check every connection, check for signs of overheating, and consider pigtailing with copper using approved purple wire nuts rated for Al/Cu connections.",
    buildingEra: '1960s-1970s',
  },
  {
    id: 'clip-4',
    expertName: 'Maria Rodriguez',
    expertYears: 24,
    trade: 'electrical',
    taskType: 'junction_box',
    triggerKeywords: ['wire nut', 'wirenut', 'splice', 'connection', 'twist', 'exposed copper', 'bare wire', 'marrette', 'wago', 'push connector'],
    title: 'Wire Nut Technique — Getting It Right Every Time',
    content: "I failed my first inspection because of wire nuts. The inspector pulled on one splice and it came right apart. That was 24 years ago and I\'ve never forgotten it. Here\'s what I teach every apprentice: First, strip your conductors to the right length — about 5/8 inch for most wire nuts. Too long and you get exposed copper. Too short and the connection is weak. Second, hold the conductors parallel, not crossed. Twist them clockwise with your lineman\'s — at least three full turns before the wire nut goes on. Third, the wire nut twists on clockwise too. Keep twisting until you feel resistance and the insulation starts to grip. Fourth — and this is the test — tug on each conductor individually. If any wire pulls out, start over. I prefer Ideal brand wire nuts, the tan ones for 14 gauge and red for 12 gauge. WAGO lever connectors are also excellent and faster for rough-in work. Whatever you use, no copper should be visible when you\'re done. None. If I can see copper, it\'s wrong.",
  },
  {
    id: 'clip-5',
    expertName: 'Robert Kim',
    expertYears: 30,
    trade: 'electrical',
    taskType: 'panel',
    triggerKeywords: ['panel upgrade', 'load calculation', '200 amp', '100 amp', 'service upgrade', 'main breaker', 'bus bar', 'subpanel', 'capacity'],
    title: 'Panel Upgrades — Do the Math Before You Start',
    content: "I see electricians jump straight into panel upgrades without doing a proper load calculation, and then they wonder why the inspector sends them back. Before you touch anything, do your NEC Article 220 load calculation. Add up the general lighting load (3 VA per square foot), small appliance circuits (1500 VA each, minimum two), laundry circuit (1500 VA), and every fixed appliance. Apply your demand factors. I\'ve seen 100-amp services that were perfectly adequate and 200-amp services that were already overloaded. The numbers don\'t lie. When you\'re doing the physical upgrade, here\'s my sequence: utility coordination first (always), then service entrance conductors sized for the new panel, proper grounding electrode system, neutral-ground bond only at the main panel, and label every single circuit clearly. The most common mistake on panel upgrades is forgetting to separate neutrals and grounds in a subpanel — they bond at the main only. And please, use a torque wrench on your lugs. The specs are right there on the panel. I\'ve seen house fires from loose connections that a $30 torque wrench would have prevented.",
  },
];

export function getRelevantClips(keywords: string[]): KnowledgeClip[] {
  if (!keywords || keywords.length === 0) return [];

  const normalizedKeywords = keywords.map((k) => k.toLowerCase());

  return KNOWLEDGE_CLIPS.filter((clip) =>
    clip.triggerKeywords.some((trigger) =>
      normalizedKeywords.some(
        (keyword) => keyword.includes(trigger) || trigger.includes(keyword)
      )
    )
  );
}
