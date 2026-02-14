# TradeProof VR — Electrical Training for Meta Quest 3

A VR training platform for electricians that teaches NEC code compliance through hands-on simulated tasks. Built with Unity 2022.3 LTS and Meta XR SDK targeting Meta Quest 3.

## Training Tasks

### 1. Residential Panel Inspection (Beginner)
Identify NEC code violations in a residential electrical panel:
- Double-tapped breaker (NEC 408.41)
- Missing knockout cover (NEC 408.7)
- Incorrect wire gauge for breaker amperage (NEC 310.16)
- Missing panel directory (NEC 408.4)

### 2. 20A Circuit Wiring (Intermediate)
Wire a complete 20-amp circuit from panel to outlet:
- Select correct wire gauge (12 AWG for 20A)
- Route wire, strip ends
- Connect hot/neutral/ground at panel and outlet
- Secure breaker and label circuit

### Training Modes
- **Learn**: Guided tour with NEC code explanations
- **Practice**: User performs tasks with hint system and immediate feedback
- **Test**: Timed, no hints, scored and badge-eligible (80% pass threshold)

---

## Prerequisites

- **Unity 2022.3 LTS** (2022.3.20f1 or later)
- **Meta Quest 3** headset with developer mode enabled
- **Android Build Support** module installed in Unity Hub
- USB-C cable for deployment
- Meta Quest Developer Hub (optional, for wireless deployment)

---

## Setup Instructions

### Step 1: Install Unity

1. Download Unity Hub from https://unity.com/download
2. Install Unity **2022.3 LTS** (latest patch)
3. During installation, check:
   - Android Build Support
   - Android SDK & NDK Tools
   - OpenJDK

### Step 2: Open the Project

1. Open Unity Hub
2. Click "Open" and navigate to this `tradeproof-vr/` directory
3. Unity will import and compile all scripts
4. If prompted about render pipeline, select "Universal Render Pipeline (URP)"

### Step 3: Import Meta XR SDK

The `Packages/manifest.json` includes the Meta XR SDK dependency. If it does not resolve automatically:

1. Window > Package Manager
2. Click "+" > "Add package by name"
3. Enter: `com.meta.xr.sdk.all`
4. Click "Add"

If using scoped registry, ensure the Meta NPM registry is configured:
- Name: `Meta XR SDK`
- URL: `https://npm.developer.oculus.com`
- Scope: `com.meta.xr`

### Step 4: Configure Build Settings

1. File > Build Settings
2. Switch platform to **Android**
3. Set Texture Compression to **ASTC**
4. Click "Player Settings..."

### Step 5: Configure Player Settings

In **Player Settings > Android**:

1. **Other Settings**:
   - Minimum API Level: `Android 10.0 (API Level 29)`
   - Target API Level: `Automatic (Highest Installed)`
   - Scripting Backend: `IL2CPP`
   - Target Architectures: Check `ARM64` only
   - Color Space: `Linear`

2. **XR Plug-in Management**:
   - Check `Oculus` under Android tab
   - In Oculus settings, set Target Devices to `Quest 3`

3. **Quality Settings**:
   - Rendering > URP Asset: assign a URP quality setting
   - Set default quality to Medium or High

### Step 6: Configure OVR Manager

1. In Project Settings > XR Plug-in Management > Oculus:
   - Target Devices: Quest 3
   - Hand Tracking Support: Controllers and Hands
   - Hand Tracking Frequency: HIGH

---

## Scene Setup

Create a new scene (or modify the default SampleScene):

### 1. Camera Rig

1. Delete the default Main Camera
2. Create an empty GameObject named `OVRCameraRig`
3. Add the `OVR Camera Rig` component (from Meta XR SDK)
4. Add the `OVR Manager` component with these settings:
   - Tracking Origin Type: Floor Level
   - Hand Tracking Support: Controllers And Hands

### 2. Core Managers

Create empty GameObjects and attach scripts:

| GameObject | Script(s) |
|---|---|
| `GameManager` | `TradeProof.Core.GameManager` |
| `TaskManager` | `TradeProof.Core.TaskManager` |
| `ScoreManager` | `TradeProof.Core.ScoreManager` |
| `BadgeSystem` | `TradeProof.Core.BadgeSystem` |
| `AudioManager` | `TradeProof.Core.AudioManager` |
| `HighlightController` | `TradeProof.Interaction.HighlightController` |

All managers use the singleton pattern and persist across scenes via DontDestroyOnLoad.

### 3. Hand Interaction

For each hand:

1. Under the OVRCameraRig, find `LeftHandAnchor` and `RightHandAnchor`
2. Add child GameObjects `LeftHandInteraction` and `RightHandInteraction`
3. Attach `TradeProof.Interaction.HandInteraction` to each
4. Set `Hand Type` to the appropriate hand
5. Set `Controller Type` to `LTouch` or `RTouch`

Add `OVRHand` and `OVRSkeleton` components to each hand anchor for hand tracking.

### 4. UI Panels

Create empty GameObjects for each UI:

| GameObject | Script | Notes |
|---|---|---|
| `MainMenuUI` | `TradeProof.UI.MainMenuUI` | World-space canvas, auto-positioned |
| `TaskSelectionUI` | `TradeProof.UI.TaskSelectionUI` | Two floating cards |
| `HUDController` | `TradeProof.UI.HUDController` | Follows player gaze |
| `ResultsScreenUI` | `TradeProof.UI.ResultsScreenUI` | Score breakdown |
| `HintPanel` | `TradeProof.UI.HintPanel` | Auto-hide floating hint |
| `BadgeDisplayUI` | `TradeProof.UI.BadgeDisplayUI` | Badge gallery |

Each UI script creates its own Canvas and elements programmatically. You just need the empty GameObjects.

### 5. Electrical Panel (Panel Inspection Task)

1. Create empty GameObject `ElectricalPanel`
2. Attach `TradeProof.Electrical.ElectricalPanel`
3. Position at approximately `(0, 1.3, 1.5)` — eye level, 1.5m in front of player
4. The script auto-generates the panel body, bus bars, knockouts, and breaker snap points

5. Create child GameObject `PanelInspectionTask`
6. Attach `TradeProof.Training.PanelInspectionTask`
7. Drag `ElectricalPanel` into the `electricalPanel` field

### 6. Circuit Wiring Workbench

1. Create empty GameObject `Workbench` at position `(0, 0.8, 1.0)`
2. Create child `ElectricalPanel_Wiring` with `TradeProof.Electrical.ElectricalPanel`
3. Create child `OutletBox` with `TradeProof.Electrical.Outlet`
4. Position the outlet ~0.8m to the right of the panel

5. Create `CircuitWiringTask` GameObject
6. Attach `TradeProof.Training.CircuitWiringTask`
7. Assign the panel and outlet references

### 7. Tool Belt

1. Create empty GameObject `ToolBelt`
2. Attach `TradeProof.Interaction.ToolBelt`
3. The belt auto-follows the player's waist position

---

## Building and Deploying

### Build to Quest 3 via USB

1. Connect Quest 3 via USB-C
2. Enable developer mode on Quest 3 (Settings > System > Developer)
3. Accept USB debugging prompt on headset
4. File > Build Settings > Build And Run
5. Select output APK location
6. Unity builds and deploys automatically

### Build APK Only

1. File > Build Settings > Build
2. APK file is created at your chosen location
3. Install manually: `adb install -r tradeproof-vr.apk`

### Testing with Meta XR Simulator (Desktop)

1. Window > Meta XR > Meta XR Simulator
2. Enable the simulator in XR Plug-in Management
3. Press Play in the Editor
4. Use WASD + mouse to navigate
5. Simulate hand tracking with keyboard shortcuts

---

## Project Architecture

```
Assets/Scripts/
├── Core/              # Managers (GameManager, TaskManager, ScoreManager, BadgeSystem, AudioManager)
├── Training/          # Task logic (TrainingTask base, PanelInspection, CircuitWiring, ViolationMarker, WireSegment)
├── Interaction/       # VR input (HandInteraction, GrabInteractable, SnapPoint, ToolBelt, HighlightController)
├── Electrical/        # Electrical components (ElectricalPanel, CircuitBreaker, Wire, Outlet, BusBar, NECCodeReference)
├── UI/                # VR UI panels (MainMenu, TaskSelection, HUD, Results, HintPanel, FloatingLabel, BadgeDisplay)
└── Data/              # Data classes (TaskDefinition, PlayerProgress, ViolationData, CertificationData, NECDatabase)
```

### Key Design Decisions

**Physical Alignment**: All violation markers and snap points use LOCAL positions relative to their parent transforms. This ensures electrical components, connection points, and violation indicators align correctly regardless of where the panel or workbench is placed in the room.

**Snap System**: Connection points use a 0.02m (2cm) snap radius — precise enough to feel intentional but forgiving enough to not frustrate users in VR.

**Grip Offsets**: Each tool type has a defined grip offset so objects appear held naturally:
- Wire: `(0, -0.02, 0.05)` — between thumb and index finger
- Screwdriver: `(0, 0, 0.08)` — in fist, extending forward
- Wire strippers: `(0, 0, 0.06)` — in fist

**Panel Dimensions**: Based on real residential panels — 14.5" x 28" x 4" (0.368m x 0.711m x 0.102m).

---

## Data Persistence

Player progress, scores, and badges are saved via `PlayerPrefs` as JSON. Badges can be exported as a JSON file for marketplace profile sync via the Badge Display UI.

Exported badge JSON is saved to `Application.persistentDataPath/tradeproof_badges.json`.

---

## NEC Codes Covered

| Code | Title | Used In |
|---|---|---|
| 310.16 | Conductor Ampacity | Both tasks |
| 408.4 | Circuit Directory | Both tasks |
| 408.7 | Unused Openings | Panel Inspection |
| 408.41 | Conductor Terminations (Double-Tap) | Both tasks |
| 408.54 | Maximum Overcurrent Devices | Panel Inspection |
| 250.50 | Grounding Electrode System | Both tasks |
| 210.21 | Receptacle Rating | Circuit Wiring |

---

## Troubleshooting

**Scripts not compiling**: Ensure TextMeshPro is imported (Window > TextMeshPro > Import TMP Essential Resources). Ensure Meta XR SDK is installed.

**OVRHand / OVRSkeleton errors**: These require Meta XR SDK. If building for editor testing without the SDK, you can add `#if !UNITY_EDITOR` guards around OVR-specific calls, though the scripts are designed to gracefully handle missing OVR components.

**Hand tracking not working**: In OVR Manager, set Hand Tracking Support to "Controllers And Hands" and Hand Tracking Frequency to "HIGH".

**UI panels not visible**: UI canvases are world-space and auto-position in front of the player camera. Ensure the OVRCameraRig has a camera tagged as MainCamera.

**Task definitions not loading**: Verify JSON files are in `Assets/Resources/TaskDefinitions/` and `Assets/Resources/NECCodes/`. Unity's Resources.Load requires files to be in a `Resources` folder.
