import type {
  MarketplaceWorker,
  MarketplaceHomeowner,
  MarketplaceJob,
  MarketplaceMessage,
} from '@/lib/marketplace-storage';

// ---------------------------------------------------------------------------
// 6 Demo Workers — Bay Area electricians with varying skill levels
// ---------------------------------------------------------------------------

export const DEMO_WORKERS: MarketplaceWorker[] = [
  {
    id: 'worker-maria-santos',
    name: 'Maria Santos',
    type: 'freelancer',
    license: 'CA-ELC-78234',
    insurance: 'State Farm Commercial GL',
    skills: ['Panel Upgrades', 'Rewiring', 'EV Charger Install', 'Smart Home', 'Troubleshooting'],
    certifications: [
      { skill: 'Panel Upgrades', level: 2, source: 'field' },
      { skill: 'Rewiring', level: 2, source: 'field' },
      { skill: 'EV Charger Install', level: 1, source: 'vr' },
      { skill: 'Smart Home', level: 1, source: 'field' },
    ],
    rate: 85,
    availability: 'available',
    complianceScore: 96,
    totalAnalyses: 47,
    jobHistory: ['job-demo-3', 'job-demo-4'],
    location: 'San Jose, CA',
    bio: 'Licensed journeyman electrician with 8 years of experience in residential and light commercial work. Specializing in panel upgrades and whole-house rewiring for older Bay Area homes. NEC 2023 certified.',
    createdAt: '2025-06-10T08:00:00Z',
  },
  {
    id: 'worker-james-chen',
    name: 'James Chen',
    type: 'freelancer',
    license: 'CA-ELC-91045',
    skills: ['Outlet Install', 'Lighting', 'Troubleshooting', 'Panel Upgrades'],
    certifications: [
      { skill: 'Outlet Install', level: 2, source: 'field' },
      { skill: 'Lighting', level: 2, source: 'vr' },
      { skill: 'Troubleshooting', level: 1, source: 'field' },
      { skill: 'Panel Upgrades', level: 1, source: 'vr' },
    ],
    rate: 65,
    availability: 'available',
    complianceScore: 88,
    totalAnalyses: 23,
    jobHistory: [],
    location: 'Oakland, CA',
    bio: 'Apprentice-turned-journeyman specializing in residential electrical. Fast, reliable, and always code-compliant. Great with older homes in the East Bay.',
    createdAt: '2025-08-22T08:00:00Z',
  },
  {
    id: 'worker-priya-patel',
    name: 'Priya Patel',
    type: 'agency',
    license: 'CA-ELC-65890',
    insurance: 'Hartford Business Insurance',
    skills: ['Panel Upgrades', 'Rewiring', 'Commercial', 'EV Charger Install', 'Solar'],
    certifications: [
      { skill: 'Panel Upgrades', level: 2, source: 'field' },
      { skill: 'Rewiring', level: 2, source: 'field' },
      { skill: 'EV Charger Install', level: 2, source: 'field' },
      { skill: 'Solar', level: 1, source: 'vr' },
    ],
    rate: 110,
    availability: 'busy',
    complianceScore: 98,
    totalAnalyses: 82,
    jobHistory: ['job-demo-3'],
    location: 'Palo Alto, CA',
    bio: 'Master electrician running BrightSpark Electric — a 5-person crew handling everything from panel swaps to full solar+EV installations across the Peninsula. We bring the whole package.',
    createdAt: '2025-03-15T08:00:00Z',
  },
  {
    id: 'worker-derek-johnson',
    name: 'Derek Johnson',
    type: 'freelancer',
    skills: ['Outlet Install', 'Lighting', 'Troubleshooting'],
    certifications: [
      { skill: 'Outlet Install', level: 1, source: 'vr' },
      { skill: 'Lighting', level: 1, source: 'vr' },
      { skill: 'Troubleshooting', level: 1, source: 'field' },
    ],
    rate: 45,
    availability: 'available',
    complianceScore: 72,
    totalAnalyses: 8,
    jobHistory: [],
    location: 'Fremont, CA',
    bio: 'Second-year apprentice looking to build my portfolio. Eager to learn and always double-checking my work against code. Affordable rates while I grow my skills.',
    createdAt: '2025-11-05T08:00:00Z',
  },
  {
    id: 'worker-sarah-kim',
    name: 'Sarah Kim',
    type: 'agency',
    license: 'CA-ELC-54321',
    insurance: 'Travelers Commercial',
    skills: ['Rewiring', 'Panel Upgrades', 'Troubleshooting', 'Outlet Install', 'Lighting'],
    certifications: [
      { skill: 'Rewiring', level: 2, source: 'field' },
      { skill: 'Panel Upgrades', level: 2, source: 'field' },
      { skill: 'Troubleshooting', level: 2, source: 'field' },
      { skill: 'Outlet Install', level: 1, source: 'field' },
    ],
    rate: 95,
    availability: 'available',
    complianceScore: 94,
    totalAnalyses: 61,
    jobHistory: ['job-demo-4'],
    location: 'Berkeley, CA',
    bio: 'Kim Electric has been serving the East Bay for 12 years. Our team of 3 licensed electricians handles residential rewiring, service upgrades, and code correction work. We pull permits for every job.',
    createdAt: '2025-04-20T08:00:00Z',
  },
  {
    id: 'worker-marcus-rivera',
    name: 'Marcus Rivera',
    type: 'freelancer',
    license: 'CA-ELC-83456',
    skills: ['EV Charger Install', 'Smart Home', 'Panel Upgrades', 'Lighting'],
    certifications: [
      { skill: 'EV Charger Install', level: 2, source: 'field' },
      { skill: 'Smart Home', level: 2, source: 'vr' },
      { skill: 'Panel Upgrades', level: 1, source: 'field' },
      { skill: 'Lighting', level: 1, source: 'vr' },
    ],
    rate: 75,
    availability: 'available',
    complianceScore: 91,
    totalAnalyses: 34,
    jobHistory: [],
    location: 'San Francisco, CA',
    bio: 'Focused on the future of residential electrical — EV chargers, smart home automation, and energy efficiency. Tesla-certified installer. If it connects to an app, I can wire it.',
    createdAt: '2025-07-12T08:00:00Z',
  },
  // ---- More workers for swiping ----
  worker('worker-alex-nguyen', 'Alex Nguyen', 'freelancer', 78, 92, 'San Jose, CA', ['Panel Upgrades', 'Rewiring', 'Outlet Install'], 'Licensed journeyman. Panel upgrades and rewires are my bread and butter. Clean permit history.'),
  worker('worker-jessica-wright', 'Jessica Wright', 'freelancer', 62, 86, 'Oakland, CA', ['Lighting', 'Outlet Install', 'Troubleshooting'], 'Residential specialist. Great with older homes and troubleshooting mystery issues.'),
  worker('worker-andre-costa', 'Andre Costa', 'agency', 105, 97, 'San Francisco, CA', ['Panel Upgrades', 'EV Charger Install', 'Solar', 'Rewiring'], 'Small crew, big jobs. We do full solar + EV + panel upgrades. Licensed and insured.'),
  worker('worker-maya-johnson', 'Maya Johnson', 'freelancer', 55, 79, 'Berkeley, CA', ['Outlet Install', 'Lighting', 'Troubleshooting'], 'Third-year apprentice. Careful, code-focused, and building experience.'),
  worker('worker-ryan-o-brien', 'Ryan O\'Brien', 'freelancer', 88, 94, 'Palo Alto, CA', ['Panel Upgrades', 'EV Charger Install', 'Smart Home'], 'Master electrician. Tesla and ChargePoint certified. Residential and light commercial.'),
  worker('worker-yuki-tanaka', 'Yuki Tanaka', 'freelancer', 70, 89, 'Fremont, CA', ['Rewiring', 'Outlet Install', 'Lighting'], 'Residential rewires and kitchen/bath updates. Neat work, on time.'),
  worker('worker-carlos-mendez', 'Carlos Mendez', 'agency', 95, 96, 'San Jose, CA', ['Panel Upgrades', 'Rewiring', 'Troubleshooting', 'Outlet Install'], 'Family business, 15 years. We handle everything from service upgrades to full rewires.'),
  worker('worker-hannah-baker', 'Hannah Baker', 'freelancer', 58, 84, 'Sunnyvale, CA', ['Outlet Install', 'Lighting', 'Smart Home'], 'Smart home and lighting focus. Lutron and Nest installs.'),
  worker('worker-tyler-james', 'Tyler James', 'freelancer', 72, 87, 'Santa Clara, CA', ['EV Charger Install', 'Panel Upgrades', 'Outlet Install'], 'EV charger specialist. All brands. Permit and inspection included.'),
  worker('worker-lisa-chen', 'Lisa Chen', 'freelancer', 68, 90, 'Mountain View, CA', ['Troubleshooting', 'Rewiring', 'Outlet Install'], 'Diagnostics and repairs. I find the problem and fix it right the first time.'),
  worker('worker-marcus-hall', 'Marcus Hall', 'agency', 100, 95, 'Oakland, CA', ['Panel Upgrades', 'Rewiring', 'Commercial', 'Outlet Install'], 'Commercial and residential. Service upgrades, tenant improvements, and maintenance.'),
  worker('worker-olivia-martin', 'Olivia Martin', 'freelancer', 52, 76, 'Redwood City, CA', ['Outlet Install', 'Lighting', 'Troubleshooting'], 'Apprentice. Eager, detail-oriented. Looking to grow in residential work.'),
  worker('worker-daniel-kim', 'Daniel Kim', 'freelancer', 82, 93, 'Cupertino, CA', ['Panel Upgrades', 'EV Charger Install', 'Smart Home', 'Lighting'], 'Full-service residential. Panel, EV, smart home, and lighting. One call.'),
  worker('worker-ashley-rodriguez', 'Ashley Rodriguez', 'freelancer', 65, 85, 'San Jose, CA', ['Rewiring', 'Outlet Install', 'Panel Upgrades'], 'Older homes are my specialty. Knob and tube replacement and panel upgrades.'),
  worker('worker-brandon-scott', 'Brandon Scott', 'freelancer', 74, 88, 'San Francisco, CA', ['EV Charger Install', 'Outlet Install', 'Troubleshooting'], 'EV and general residential. Fast, permitted, and tidy.'),
  worker('worker-natalie-white', 'Natalie White', 'agency', 98, 97, 'Palo Alto, CA', ['Panel Upgrades', 'Rewiring', 'EV Charger Install', 'Solar'], 'High-end residential and light commercial. Peninsula and South Bay.'),
  worker('worker-ethan-clark', 'Ethan Clark', 'freelancer', 60, 82, 'Berkeley, CA', ['Lighting', 'Outlet Install', 'Smart Home'], 'Lighting design and install. Recessed, pendants, and smart switches.'),
  worker('worker-victoria-lee', 'Victoria Lee', 'freelancer', 76, 91, 'Fremont, CA', ['Panel Upgrades', 'Rewiring', 'Outlet Install'], 'Journeyman. Panel upgrades and whole-house rewires. NEC 2023.'),
  worker('worker-noah-adams', 'Noah Adams', 'freelancer', 48, 71, 'Oakland, CA', ['Outlet Install', 'Lighting', 'Troubleshooting'], 'New to the trade but thorough. Affordable rates for straightforward jobs.'),
  worker('worker-chloe-brown', 'Chloe Brown', 'freelancer', 80, 94, 'San Jose, CA', ['EV Charger Install', 'Panel Upgrades', 'Smart Home'], 'EV and panel specialist. Tesla, ChargePoint, JuiceBox. Permits pulled.'),
  worker('worker-lucas-garcia', 'Lucas Garcia', 'agency', 90, 92, 'San Francisco, CA', ['Rewiring', 'Panel Upgrades', 'Outlet Install', 'Troubleshooting'], 'Residential and small commercial. Rewires, service changes, and service calls.'),
  worker('worker-zoey-taylor', 'Zoey Taylor', 'freelancer', 66, 83, 'Sunnyvale, CA', ['Outlet Install', 'Lighting', 'EV Charger Install'], 'Residential electrician. Outlets, lighting, and Level 2 EV installs.'),
  worker('worker-henry-wilson', 'Henry Wilson', 'freelancer', 84, 95, 'Santa Clara, CA', ['Panel Upgrades', 'Rewiring', 'EV Charger Install', 'Troubleshooting'], 'Master electrician. Complex panel and rewire jobs. Clear communication.'),
  worker('worker-ava-martinez', 'Ava Martinez', 'freelancer', 56, 78, 'San Jose, CA', ['Outlet Install', 'Lighting'], 'Apprentice. Reliable and code-conscious. GFCI, lighting, and small repairs.'),
  worker('worker-sebastian-lopez', 'Sebastian Lopez', 'agency', 102, 98, 'Oakland, CA', ['Panel Upgrades', 'Rewiring', 'Commercial', 'EV Charger Install'], 'Full-service electrical. Residential to mid-size commercial. Licensed and bonded.'),
  worker('worker-ella-anderson', 'Ella Anderson', 'freelancer', 64, 86, 'Palo Alto, CA', ['Troubleshooting', 'Outlet Install', 'Lighting'], 'Troubleshooting and repairs. I track down the issue and fix it.'),
  worker('worker-jackson-thomas', 'Jackson Thomas', 'freelancer', 69, 84, 'Mountain View, CA', ['Panel Upgrades', 'Outlet Install', 'EV Charger Install'], 'Service upgrades and EV installs. Clean work, on schedule.'),
  worker('worker-grace-harris', 'Grace Harris', 'freelancer', 77, 90, 'Berkeley, CA', ['Rewiring', 'Panel Upgrades', 'Outlet Install'], 'Older homes and rewires. Permits and inspections. No shortcuts.'),
  worker('worker-benjamin-young', 'Benjamin Young', 'freelancer', 73, 88, 'Fremont, CA', ['EV Charger Install', 'Smart Home', 'Outlet Install'], 'EV and smart home. Wall connectors, smart panels, and automation.'),
];

function worker(
  id: string,
  name: string,
  type: 'freelancer' | 'agency',
  rate: number,
  complianceScore: number,
  location: string,
  skills: string[],
  bio: string
): MarketplaceWorker {
  const hash = id.split('').reduce((acc, c) => acc + c.charCodeAt(0), 0);
  const certs = skills.slice(0, 4).map((skill, i) => ({
    skill,
    level: (i < 2 ? 2 : 1) as 1 | 2,
    source: (i % 2 === 0 ? 'field' : 'vr') as 'vr' | 'field',
  }));
  const availability: 'available' | 'busy' | 'unavailable' = (hash % 5) === 0 ? 'busy' : (hash % 7) === 0 ? 'unavailable' : 'available';
  return {
    id,
    name,
    type,
    license: type === 'agency' || (hash % 3) !== 0 ? `CA-ELC-${10000 + (hash % 90000)}` : undefined,
    insurance: type === 'agency' ? 'Commercial GL' : undefined,
    skills,
    certifications: certs,
    rate,
    availability,
    complianceScore,
    totalAnalyses: 8 + (hash % 80),
    jobHistory: [],
    location,
    bio,
    createdAt: new Date(Date.now() - (hash % 300) * 24 * 60 * 60 * 1000).toISOString(),
  };
}

// ---------------------------------------------------------------------------
// 3 Demo Homeowners (+ 12 more)
// ---------------------------------------------------------------------------

export const DEMO_HOMEOWNERS: MarketplaceHomeowner[] = [
  {
    id: 'homeowner-lisa-wong',
    name: 'Lisa Wong',
    location: 'San Jose, CA',
    phone: '(408) 555-0142',
    jobsPosted: ['job-demo-1', 'job-demo-3', 'job-fake-21'],
    createdAt: '2025-10-01T08:00:00Z',
  },
  {
    id: 'homeowner-robert-garcia',
    name: 'Robert Garcia',
    location: 'Oakland, CA',
    phone: '(510) 555-0198',
    jobsPosted: ['job-demo-2', 'job-fake-22'],
    createdAt: '2025-09-15T08:00:00Z',
  },
  {
    id: 'homeowner-anita-sharma',
    name: 'Anita Sharma',
    location: 'Palo Alto, CA',
    phone: '(650) 555-0277',
    jobsPosted: ['job-demo-4', 'job-fake-23'],
    createdAt: '2025-08-20T08:00:00Z',
  },
  // Extra homeowners for more open jobs
  { id: 'homeowner-mike-taylor', name: 'Mike Taylor', location: 'San Francisco, CA', phone: '(415) 555-0101', jobsPosted: ['job-fake-1', 'job-fake-2'], createdAt: '2025-11-01T08:00:00Z' },
  { id: 'homeowner-jenny-lopez', name: 'Jenny Lopez', location: 'San Jose, CA', phone: '(408) 555-0202', jobsPosted: ['job-fake-3', 'job-fake-4'], createdAt: '2025-11-05T08:00:00Z' },
  { id: 'homeowner-david-park', name: 'David Park', location: 'Oakland, CA', phone: '(510) 555-0303', jobsPosted: ['job-fake-5', 'job-fake-6'], createdAt: '2025-11-10T08:00:00Z' },
  { id: 'homeowner-emily-chen', name: 'Emily Chen', location: 'Berkeley, CA', phone: '(510) 555-0404', jobsPosted: ['job-fake-7'], createdAt: '2025-11-12T08:00:00Z' },
  { id: 'homeowner-chris-moore', name: 'Chris Moore', location: 'Fremont, CA', phone: '(510) 555-0505', jobsPosted: ['job-fake-8', 'job-fake-9'], createdAt: '2025-11-15T08:00:00Z' },
  { id: 'homeowner-rachel-green', name: 'Rachel Green', location: 'Sunnyvale, CA', phone: '(408) 555-0606', jobsPosted: ['job-fake-10'], createdAt: '2025-11-18T08:00:00Z' },
  { id: 'homeowner-kevin-brown', name: 'Kevin Brown', location: 'Santa Clara, CA', phone: '(408) 555-0707', jobsPosted: ['job-fake-11', 'job-fake-12'], createdAt: '2025-11-20T08:00:00Z' },
  { id: 'homeowner-amanda-wilson', name: 'Amanda Wilson', location: 'Mountain View, CA', phone: '(650) 555-0808', jobsPosted: ['job-fake-13'], createdAt: '2025-11-22T08:00:00Z' },
  { id: 'homeowner-tyler-davis', name: 'Tyler Davis', location: 'Redwood City, CA', phone: '(650) 555-0909', jobsPosted: ['job-fake-14', 'job-fake-15'], createdAt: '2025-11-25T08:00:00Z' },
  { id: 'homeowner-nina-kumar', name: 'Nina Kumar', location: 'Cupertino, CA', phone: '(408) 555-1010', jobsPosted: ['job-fake-16'], createdAt: '2025-11-28T08:00:00Z' },
  { id: 'homeowner-jake-martinez', name: 'Jake Martinez', location: 'San Jose, CA', phone: '(408) 555-1111', jobsPosted: ['job-fake-17', 'job-fake-18'], createdAt: '2025-12-01T08:00:00Z' },
  { id: 'homeowner-sophie-lee', name: 'Sophie Lee', location: 'Palo Alto, CA', phone: '(650) 555-1212', jobsPosted: ['job-fake-19', 'job-fake-20'], createdAt: '2025-12-05T08:00:00Z' },
];

// ---------------------------------------------------------------------------
// 4 Demo Jobs — mix of statuses (original)
// ---------------------------------------------------------------------------

export const DEMO_JOBS: MarketplaceJob[] = [
  // Open job 1
  {
    id: 'job-demo-1',
    posterId: 'homeowner-lisa-wong',
    posterType: 'homeowner',
    posterName: 'Lisa Wong',
    title: '200A Panel Upgrade — 1960s Ranch',
    type: 'panel-upgrade',
    description:
      'Our 1962 ranch home in Willow Glen still has the original 100A Federal Pacific panel. Need a full upgrade to 200A service with new main breaker panel. Planning to add EV charger and heat pump in the next year so want capacity for future loads. Permit and inspection required.',
    location: 'San Jose, CA',
    budget: { min: 3500, max: 5500 },
    preferredDates: ['2026-03-10', '2026-03-15', '2026-03-20'],
    urgency: 'normal',
    requiredCerts: [{ skill: 'Panel Upgrades', level: 2 }],
    status: 'open',
    applicants: [
      {
        workerId: 'worker-james-chen',
        quote: 4200,
        message:
          'I can handle this upgrade. Have done several FPE panel replacements in Willow Glen. Would include a dedicated 50A circuit for your future EV charger.',
        appliedAt: '2026-02-10T14:30:00Z',
      },
    ],
    createdAt: '2026-02-08T09:00:00Z',
  },
  // Open job 2
  {
    id: 'job-demo-2',
    posterId: 'homeowner-robert-garcia',
    posterType: 'homeowner',
    posterName: 'Robert Garcia',
    title: 'Kitchen Rewire + GFCI Outlets',
    type: 'rewiring',
    description:
      'Renovating our 1940s Craftsman kitchen. Need to rewire the kitchen circuit — currently has old cloth-wrapped wiring. Install 4 new GFCI outlets per current code, dedicated 20A circuits for fridge and microwave. Walls will be open during renovation so easy access.',
    location: 'Oakland, CA',
    budget: { min: 2000, max: 3500 },
    preferredDates: ['2026-03-01', '2026-03-05'],
    urgency: 'urgent',
    requiredCerts: [
      { skill: 'Rewiring', level: 1 },
      { skill: 'Outlet Install', level: 1 },
    ],
    status: 'open',
    applicants: [],
    createdAt: '2026-02-12T11:00:00Z',
  },
  // In-progress job
  {
    id: 'job-demo-3',
    posterId: 'homeowner-lisa-wong',
    posterType: 'homeowner',
    posterName: 'Lisa Wong',
    title: 'EV Charger Installation — Tesla Wall Connector',
    type: 'outlet-install',
    description:
      'Install a Tesla Wall Connector Gen 3 in attached garage. Panel is on the opposite wall of the garage so conduit run should be short. Need 60A breaker and appropriate wire gauge. Charger already purchased.',
    location: 'San Jose, CA',
    budget: { min: 800, max: 1500 },
    preferredDates: ['2026-02-20'],
    urgency: 'normal',
    requiredCerts: [{ skill: 'EV Charger Install', level: 1 }],
    status: 'in-progress',
    applicants: [
      {
        workerId: 'worker-maria-santos',
        quote: 1100,
        message:
          'Tesla-certified installer here. I have done 20+ Wall Connector installs. Your setup sounds straightforward — should be a half-day job.',
        appliedAt: '2026-02-05T10:00:00Z',
      },
      {
        workerId: 'worker-priya-patel',
        quote: 1350,
        message:
          'BrightSpark Electric can handle this. We include a post-install inspection and 1-year warranty on labor.',
        appliedAt: '2026-02-05T16:00:00Z',
      },
    ],
    assignedWorkerId: 'worker-maria-santos',
    createdAt: '2026-02-03T08:00:00Z',
  },
  // Completed job
  {
    id: 'job-demo-4',
    posterId: 'homeowner-anita-sharma',
    posterType: 'homeowner',
    posterName: 'Anita Sharma',
    title: 'Whole-House Troubleshooting — Flickering Lights',
    type: 'troubleshooting',
    description:
      'Lights throughout the house flicker intermittently, especially when the HVAC kicks on. Breaker trips occasionally on the kitchen circuit. Need someone to diagnose and fix the issue. House is a 2005 build.',
    location: 'Palo Alto, CA',
    budget: { min: 300, max: 800 },
    preferredDates: ['2026-01-15'],
    urgency: 'urgent',
    requiredCerts: [{ skill: 'Troubleshooting', level: 1 }],
    status: 'completed',
    applicants: [
      {
        workerId: 'worker-sarah-kim',
        quote: 550,
        message:
          'This sounds like it could be a loose neutral or undersized feeder. I will bring my thermal imager and circuit analyzer to get to the bottom of it quickly.',
        appliedAt: '2026-01-10T09:00:00Z',
      },
      {
        workerId: 'worker-maria-santos',
        quote: 600,
        message: 'Happy to take a look. Usually flickering on HVAC start is a connection or sizing issue.',
        appliedAt: '2026-01-10T14:00:00Z',
      },
    ],
    assignedWorkerId: 'worker-sarah-kim',
    completedAt: '2026-01-16T17:00:00Z',
    review: {
      rating: 5,
      text: 'Sarah found a loose neutral at the main panel in under an hour. Tightened all connections and the flickering is completely gone. Very professional and explained everything clearly. Highly recommend!',
    },
    createdAt: '2026-01-08T10:00:00Z',
  },
  // ---- Many more OPEN jobs for swiping ----
  openJob('job-fake-1', 'homeowner-mike-taylor', 'Mike Taylor', '100A to 200A Panel Upgrade', 'panel-upgrade', 'Older home needs service upgrade for AC and EV. Federal Pacific panel must go.', 'San Francisco, CA', 3200, 5000, 'normal', [{ skill: 'Panel Upgrades', level: 2 }]),
  openJob('job-fake-2', 'homeowner-mike-taylor', 'Mike Taylor', 'EV Charger — NEMA 14-50 Outlet', 'outlet-install', 'Need 240V outlet in garage for Level 2 charging. Panel has capacity.', 'San Francisco, CA', 600, 1200, 'flexible', [{ skill: 'EV Charger Install', level: 1 }]),
  openJob('job-fake-3', 'homeowner-jenny-lopez', 'Jenny Lopez', 'Bathroom GFCI and Lighting', 'outlet-install', 'Two bathrooms need GFCI outlets and new vanity lights. Code compliance required.', 'San Jose, CA', 400, 900, 'normal', [{ skill: 'Outlet Install', level: 1 }, { skill: 'Lighting', level: 1 }]),
  openJob('job-fake-4', 'homeowner-jenny-lopez', 'Jenny Lopez', 'Subpanel for Garage Workshop', 'panel-upgrade', 'Add 60A subpanel in garage for tools and future EV. Main panel in basement.', 'San Jose, CA', 1800, 2800, 'normal', [{ skill: 'Panel Upgrades', level: 1 }]),
  openJob('job-fake-5', 'homeowner-david-park', 'David Park', 'Knob and Tube Removal — Living Room', 'rewiring', 'Living room and dining still have K&T. Renovation in progress, walls open.', 'Oakland, CA', 2500, 4200, 'urgent', [{ skill: 'Rewiring', level: 2 }]),
  openJob('job-fake-6', 'homeowner-david-park', 'David Park', 'Flickering Lights Diagnosis', 'troubleshooting', 'Lights dim when AC runs. Breaker panel is 20 years old. Need diagnosis and fix.', 'Oakland, CA', 250, 600, 'urgent', [{ skill: 'Troubleshooting', level: 1 }]),
  openJob('job-fake-7', 'homeowner-emily-chen', 'Emily Chen', 'Whole-House Surge Protection', 'panel-upgrade', 'Install whole-house surge protector at main panel. Panel is Eaton.', 'Berkeley, CA', 500, 900, 'normal', [{ skill: 'Panel Upgrades', level: 1 }]),
  openJob('job-fake-8', 'homeowner-chris-moore', 'Chris Moore', 'Tesla Wall Connector Install', 'outlet-install', 'Wall Connector Gen 3, 48A. Panel in garage. Short run.', 'Fremont, CA', 800, 1400, 'normal', [{ skill: 'EV Charger Install', level: 2 }]),
  openJob('job-fake-9', 'homeowner-chris-moore', 'Chris Moore', 'Kitchen Island Outlets', 'outlet-install', 'Add two outlets to kitchen island. Crawl space below. Need permit.', 'Fremont, CA', 350, 700, 'flexible', [{ skill: 'Outlet Install', level: 1 }]),
  openJob('job-fake-10', 'homeowner-rachel-green', 'Rachel Green', 'Recessed Lighting — 8 Cans', 'lighting', 'Replace old fixtures with LED recessed in living room. Attic access.', 'Sunnyvale, CA', 600, 1100, 'normal', [{ skill: 'Lighting', level: 2 }]),
  openJob('job-fake-11', 'homeowner-kevin-brown', 'Kevin Brown', '200A Service + Panel Upgrade', 'panel-upgrade', 'Full 200A upgrade, new meter base and panel. Old Zinsco panel.', 'Santa Clara, CA', 4500, 6500, 'normal', [{ skill: 'Panel Upgrades', level: 2 }]),
  openJob('job-fake-12', 'homeowner-kevin-brown', 'Kevin Brown', 'Smart Switches — 12 Locations', 'other', 'Replace switches with smart dimmers. Neutral available. Lutron or similar.', 'Santa Clara, CA', 900, 1800, 'flexible', [{ skill: 'Smart Home', level: 1 }]),
  openJob('job-fake-13', 'homeowner-amanda-wilson', 'Amanda Wilson', 'Basement Rewire for Office', 'rewiring', 'New circuits for home office: 4 outlets, dedicated 20A. Finished basement.', 'Mountain View, CA', 1200, 2200, 'normal', [{ skill: 'Rewiring', level: 1 }, { skill: 'Outlet Install', level: 1 }]),
  openJob('job-fake-14', 'homeowner-tyler-davis', 'Tyler Davis', 'Pool Pump Circuit', 'outlet-install', 'Dedicated 240V circuit for new pool pump. Run from panel to equipment pad.', 'Redwood City, CA', 550, 950, 'normal', [{ skill: 'Outlet Install', level: 2 }]),
  openJob('job-fake-15', 'homeowner-tyler-davis', 'Tyler Davis', 'Panel Inspection and Labeling', 'troubleshooting', 'Panel is a mess. Need full inspection, labeling, and any safety fixes.', 'Redwood City, CA', 200, 450, 'flexible', [{ skill: 'Troubleshooting', level: 1 }]),
  openJob('job-fake-16', 'homeowner-nina-kumar', 'Nina Kumar', 'EV Charger — JuiceBox 40', 'outlet-install', 'JuiceBox 40 hardwire install. Panel in garage. Prefer permitted work.', 'Cupertino, CA', 700, 1300, 'normal', [{ skill: 'EV Charger Install', level: 1 }]),
  openJob('job-fake-17', 'homeowner-jake-martinez', 'Jake Martinez', 'Garage Subpanel + Outlets', 'panel-upgrade', '60A subpanel in garage, 4 outlets for tools. Main panel full.', 'San Jose, CA', 1600, 2600, 'urgent', [{ skill: 'Panel Upgrades', level: 1 }, { skill: 'Outlet Install', level: 1 }]),
  openJob('job-fake-18', 'homeowner-jake-martinez', 'Jake Martinez', 'Outdoor GFCI and Landscape Lighting', 'outlet-install', 'Two GFCI outlets on patio, low-voltage landscape lighting. Transformer install.', 'San Jose, CA', 800, 1500, 'flexible', [{ skill: 'Outlet Install', level: 2 }, { skill: 'Lighting', level: 1 }]),
  openJob('job-fake-19', 'homeowner-sophie-lee', 'Sophie Lee', 'Full Home Rewire — 1950s', 'rewiring', 'Original wiring throughout. Want full rewire with modern panel. Phased OK.', 'Palo Alto, CA', 12000, 18000, 'normal', [{ skill: 'Rewiring', level: 2 }, { skill: 'Panel Upgrades', level: 2 }]),
  openJob('job-fake-20', 'homeowner-sophie-lee', 'Sophie Lee', 'Tripping Breaker — Kitchen', 'troubleshooting', 'Kitchen breaker trips when toaster and microwave run. Need fix.', 'Palo Alto, CA', 150, 400, 'urgent', [{ skill: 'Troubleshooting', level: 1 }]),
  // More open jobs (no cert requirements to maximize worker pool)
  openJob('job-fake-21', 'homeowner-lisa-wong', 'Lisa Wong', 'Ceiling Fan Install — 3 Rooms', 'other', 'Install 3 ceiling fans with existing boxes. No new wiring.', 'San Jose, CA', 300, 600, 'flexible', []),
  openJob('job-fake-22', 'homeowner-robert-garcia', 'Robert Garcia', 'Smoke Detector Upgrade', 'other', 'Replace old smoke detectors with 10-year sealed battery units. Interconnect.', 'Oakland, CA', 250, 500, 'normal', []),
  openJob('job-fake-23', 'homeowner-anita-sharma', 'Anita Sharma', 'Doorbell Transformer and Chime', 'other', 'Replace doorbell transformer and install new chime. Low voltage.', 'Palo Alto, CA', 150, 350, 'flexible', []),
  openJob('job-fake-24', 'homeowner-mike-taylor', 'Mike Taylor', 'Dryer Outlet — 4-Prong', 'outlet-install', 'Replace 3-prong dryer outlet with 4-prong. Laundry room.', 'San Francisco, CA', 200, 450, 'normal', [{ skill: 'Outlet Install', level: 1 }]),
  openJob('job-fake-25', 'homeowner-jenny-lopez', 'Jenny Lopez', 'Exterior Motion Lights', 'lighting', 'Two motion sensor lights at front and back. Replace existing fixtures.', 'San Jose, CA', 350, 650, 'flexible', [{ skill: 'Lighting', level: 1 }]),
  openJob('job-fake-26', 'homeowner-david-park', 'David Park', 'Generator Interlock Install', 'panel-upgrade', 'Install generator interlock and inlet. Panel is Square D. Have generator.', 'Oakland, CA', 600, 1100, 'urgent', [{ skill: 'Panel Upgrades', level: 2 }]),
  openJob('job-fake-27', 'homeowner-emily-chen', 'Emily Chen', 'Under-Cabinet LED Strip', 'lighting', 'Kitchen under-cabinet LED strips, plug-in or hardwire. Prefer hardwire.', 'Berkeley, CA', 200, 500, 'flexible', [{ skill: 'Lighting', level: 1 }]),
  openJob('job-fake-28', 'homeowner-chris-moore', 'Chris Moore', 'Hot Tub Circuit', 'outlet-install', '50A dedicated circuit for hot tub. Run from panel to deck.', 'Fremont, CA', 700, 1200, 'normal', [{ skill: 'Outlet Install', level: 2 }]),
  openJob('job-fake-29', 'homeowner-kevin-brown', 'Kevin Brown', 'Attic Fan Thermostat', 'troubleshooting', 'Attic fan not turning on. Thermostat or wiring issue. Need diagnosis.', 'Santa Clara, CA', 100, 300, 'flexible', [{ skill: 'Troubleshooting', level: 1 }]),
  openJob('job-fake-30', 'homeowner-amanda-wilson', 'Amanda Wilson', 'EV Charger — ChargePoint Home', 'outlet-install', 'ChargePoint Home Flex. 50A circuit. Panel in garage.', 'Mountain View, CA', 750, 1400, 'normal', [{ skill: 'EV Charger Install', level: 1 }]),
];

// Helper to create open jobs with minimal boilerplate
function openJob(
  id: string,
  posterId: string,
  posterName: string,
  title: string,
  type: string,
  description: string,
  location: string,
  budgetMin: number,
  budgetMax: number,
  urgency: 'urgent' | 'normal' | 'flexible',
  requiredCerts: { skill: string; level: number }[]
): MarketplaceJob {
  const hash = id.split('').reduce((acc, c) => acc + c.charCodeAt(0), 0);
  return {
    id,
    posterId,
    posterType: 'homeowner',
    posterName,
    title,
    type,
    description,
    location,
    budget: { min: budgetMin, max: budgetMax },
    preferredDates: ['2026-03-01', '2026-03-15'],
    urgency,
    requiredCerts,
    status: 'open',
    applicants: [],
    createdAt: new Date(Date.now() - (hash % 30) * 24 * 60 * 60 * 1000).toISOString(),
  };
}

// ---------------------------------------------------------------------------
// Demo Messages — for the in-progress EV charger job
// ---------------------------------------------------------------------------

export const DEMO_MESSAGES: MarketplaceMessage[] = [
  {
    id: 'msg-demo-1',
    jobId: 'job-demo-3',
    senderId: 'homeowner-lisa-wong',
    senderName: 'Lisa Wong',
    text: 'Hi Maria! Thanks for taking the job. When would you like to come by for a site check?',
    timestamp: '2026-02-06T09:00:00Z',
  },
  {
    id: 'msg-demo-2',
    jobId: 'job-demo-3',
    senderId: 'worker-maria-santos',
    senderName: 'Maria Santos',
    text: 'Hi Lisa! I can swing by Thursday afternoon to check the panel and plan the conduit run. Does 2pm work?',
    timestamp: '2026-02-06T10:15:00Z',
  },
  {
    id: 'msg-demo-3',
    jobId: 'job-demo-3',
    senderId: 'homeowner-lisa-wong',
    senderName: 'Lisa Wong',
    text: 'Thursday at 2pm works great! The panel is in the garage on the east wall. I will leave the garage open.',
    timestamp: '2026-02-06T10:30:00Z',
  },
  {
    id: 'msg-demo-4',
    jobId: 'job-demo-3',
    senderId: 'worker-maria-santos',
    senderName: 'Maria Santos',
    text: 'Perfect. Just did the site visit — your panel has plenty of space for a 60A breaker and the conduit run will be about 8 feet. I will pull the permit tomorrow and we can schedule the install for the 20th as planned.',
    timestamp: '2026-02-08T16:45:00Z',
  },
  {
    id: 'msg-demo-5',
    jobId: 'job-demo-3',
    senderId: 'homeowner-lisa-wong',
    senderName: 'Lisa Wong',
    text: 'Sounds great, thanks for the update! Looking forward to it.',
    timestamp: '2026-02-08T17:00:00Z',
  },
];
