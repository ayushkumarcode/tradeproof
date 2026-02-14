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
];

// ---------------------------------------------------------------------------
// 3 Demo Homeowners
// ---------------------------------------------------------------------------

export const DEMO_HOMEOWNERS: MarketplaceHomeowner[] = [
  {
    id: 'homeowner-lisa-wong',
    name: 'Lisa Wong',
    location: 'San Jose, CA',
    phone: '(408) 555-0142',
    jobsPosted: ['job-demo-1', 'job-demo-3'],
    createdAt: '2025-10-01T08:00:00Z',
  },
  {
    id: 'homeowner-robert-garcia',
    name: 'Robert Garcia',
    location: 'Oakland, CA',
    phone: '(510) 555-0198',
    jobsPosted: ['job-demo-2'],
    createdAt: '2025-09-15T08:00:00Z',
  },
  {
    id: 'homeowner-anita-sharma',
    name: 'Anita Sharma',
    location: 'Palo Alto, CA',
    phone: '(650) 555-0277',
    jobsPosted: ['job-demo-4'],
    createdAt: '2025-08-20T08:00:00Z',
  },
];

// ---------------------------------------------------------------------------
// 4 Demo Jobs — mix of statuses
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
];

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
