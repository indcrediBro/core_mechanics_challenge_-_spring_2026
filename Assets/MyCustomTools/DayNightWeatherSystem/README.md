# Day Night Weather System — Unity URP
### Version 1.0 | Requires: Unity 2021.3+ · URP 12+

---

## Quick Start (5 Steps)

### 1. Create the Environment Settings Asset
`Assets > Create > Day Night System > Environment Settings`
Adjust gradients, intensity curves, and thresholds to taste.

### 2. Set Up the Manager GameObject
Create an empty GameObject (name it `[DayNightSystem]`).
Add these components:
- `EnvironmentManager`
- `ProceduralSkyboxController`
- `PostProcessingVolumeManager`
- `WindManager`
- `CloudShadowManager`
- `VegetationManager`

### 3. Create the Skybox Material
`Assets > Create > Material`
Set the shader to `DayNightWeather/ProceduralSkybox`.
Drag the material into `ProceduralSkyboxController → Skybox Source Material`.

### 4. Wire Up Lights
- Create a **Directional Light** (Sun). Tag it `Sun`. Set `Shadow Type = Soft`.
- Create a second **Directional Light** (Moon). Add `MoonLightController` component.
- Drag both into `EnvironmentManager → Sun Light / Moon Light`.

### 5. Post Processing Volumes
Create 4 Global Volumes with Override profiles:
  `Day`, `Sunset`, `Night`, `Sunrise`
Drag them into `PostProcessingVolumeManager`.

Hit **Play** — or scrub the **Time of Day** slider in Edit Mode.

---

## Component Reference

### `EnvironmentManager`
The heartbeat of the system. Controls:
- Sun directional light (color, intensity, rotation)
- Moon directional light (color, intensity, rotation)  
- Ambient light (trilight sky/equator/ground)
- Fog (color, density, mode)
- Skybox material (all gradient properties)

**Key Inspector Controls:**
| Control | Description |
|---------|-------------|
| Environment Settings | The ScriptableObject asset with all curves/gradients |
| Time Of Day | 0–1 normalized scrubber (0 = midnight, 0.5 = noon) |
| Running | Advance time in Play Mode |
| Time Scale | Speed multiplier |
| Jump buttons | Dawn / Noon / Dusk / Midnight quick-set |

**Events:**
```csharp
EnvironmentManager.OnPhaseChanged += phase => Debug.Log(phase);
```

**Runtime API:**
```csharp
var env = GetComponent<EnvironmentManager>();
env.SetTimeHours(14.5f);   // 14:30
string t = env.GetTimeString(); // "14:30"
```

---

### `EnvironmentSettings` (ScriptableObject)
Stores all data. Fields include:
- `dayDurationSeconds` — real-world seconds per full cycle
- `startTimeNormalized` — which time of day to start at
- Sun/Moon/Ambient/Fog/Skybox gradients and intensity curves
- Star visibility, cloud speed/coverage/softness
- Phase thresholds: `sunriseStart`, `sunriseEnd`, `sunsetStart`, `sunsetEnd`

**Tip:** Create multiple assets (e.g., `Settings_Summer`, `Settings_Winter`)
and swap them at runtime for a seasonal system:
```csharp
envManager.settings = winterSettings;
```

---

### `PostProcessingVolumeManager`
Drives the weights of 4 PP Volumes according to current day phase.

| Volume | Active During |
|--------|--------------|
| Day | Sunrise end → Sunset start |
| Sunset | Sunset blend window |
| Night | After sunset → before sunrise |
| Sunrise | Sunrise blend window |

`blendSmoothing` — seconds over which weights transition (prevents hard cuts).

---

### `MoonLightController`
Attach to the Moon Directional Light.
- `simulateLunarPhases` — 29.5-day lunar cycle
- `enableFlicker` — subtle intensity scintillation
- `GetPhaseName()` → "Full Moon", "Waxing Crescent", etc.

---

### `WindManager`
Drives a `WindZone` and exposes global shader properties:

| Shader Property | Type | Description |
|----------------|------|-------------|
| `_WindDirection` | float4 | World-space XZ wind direction |
| `_WindStrength` | float | Current wind magnitude |
| `_WindTurbulence` | float | Turbulence amount |
| `_WindTime` | float | Elapsed wind time |

**Weather states:** `Calm`, `Breezy`, `Windy`, `Storm`
```csharp
windManager.weatherState = WeatherState.Storm;
```

---

### `CloudShadowManager`
Scrolls a cloud texture as a Light Cookie on the Sun light, creating
moving cloud shadows over terrain and objects.

- Sync speed with `WindManager` via `syncWithWind = true`
- `cloudScale` controls world-space projection size
- `fadeAtNight` removes shadows at night automatically

Also pushes global shader properties for terrain/vegetation shaders:
`_CloudShadowTex`, `_CloudShadowOffset`, `_CloudShadowIntensity`, `_CloudShadowScale`

---

### `VegetationManager`
Sets global shader properties every frame. Your vegetation shader
must consume them. See the **Shader Property Reference** foldout
in the Inspector for the full list.

**Features:**
| Feature | Key Property | Shader Property |
|---------|-------------|-----------------|
| Terrain Blend | `terrainBlendStrength` | `_TerrainBlendStrength` |
| Slope Correction | `slopeCorrectionAngle` | `_SlopeCorrectionAngle` |
| Scale Variation | `scaleMin/Max` | `_VegetationScaleMin/Max` |
| Distance Fade | `fadeStartDistance` | `_VegetationFadeStart/End` |
| Wind Sway | `maxSwayAngle` | `_MaxSwayAngle` |

Register renderers for per-instance overrides:
```csharp
vegetationManager.RegisterRenderer(myTreeRenderer);
```

---

### `ProceduralSkyboxController`
Creates a runtime copy of the skybox material (prevents asset dirtying).
Exposes fine-grained controls for sun/moon disc sizes, star density,
cloud height, and horizon atmospheric glow.

---

### `ProceduralSkybox.shader`
Located at `Shaders/ProceduralSkybox.shader`.
Renders in a single pass:
1. Sky gradient (top / horizon / bottom tricolor)
2. Horizon atmospheric scatter glow
3. Procedural stars with per-star twinkle (Perlin noise)
4. Moon disc with optional texture and glow halo
5. Sun disc with limb darkening and bloom corona
6. FBM cloud layer with coverage threshold and horizon fade

All driven by material properties set by `EnvironmentManager` and
`ProceduralSkyboxController` each frame.

---

## Scripting Examples

### Listen for Phase Changes
```csharp
void Start()
{
    EnvironmentManager.OnPhaseChanged += OnPhaseChanged;
}

void OnPhaseChanged(TimeOfDayPhase phase)
{
    if (phase == TimeOfDayPhase.Night)
        EnableStreetLights();
}
```

### Seasonal Settings Swap
```csharp
[SerializeField] EnvironmentSettings[] seasonSettings;

void SetSeason(int seasonIndex)
{
    GetComponent<EnvironmentManager>().settings = seasonSettings[seasonIndex];
}
```

### Trigger a Storm
```csharp
WindManager wind = GetComponent<WindManager>();
wind.weatherState = WeatherState.Storm;
wind.weatherTransitionTime = 10f; // blend over 10 seconds
```

### Force a Specific Time
```csharp
EnvironmentManager env = GetComponent<EnvironmentManager>();
env.SetTimeHours(20.5f);   // 20:30 (8:30 PM)
env.running = false;        // freeze time here
```

---

## Folder Structure
```
DayNightWeatherSystem/
├── Core/
│   ├── EnvironmentManager.cs
│   └── EnvironmentSettings.cs
├── PostProcessing/
│   └── PostProcessingVolumeManager.cs
├── Lighting/
│   └── MoonLightController.cs
├── Weather/
│   ├── WindManager.cs
│   └── CloudShadowManager.cs
├── Vegetation/
│   └── VegetationManager.cs
├── Skybox/
│   └── ProceduralSkyboxController.cs
├── Shaders/
│   └── ProceduralSkybox.shader
└── Editor/
    ├── EnvironmentManagerEditor.cs
    └── VegetationManagerEditor.cs
```

---

## Notes & Tips
- All managers use `[ExecuteAlways]` — scrubbing works in Edit Mode.
- `EnvironmentSettings` gradients can be animated via Timeline/Animator.
- For LOD vegetation, call `RegisterRenderer()` dynamically as LODs swap.
- The skybox shader uses `_Time.y` — no script overhead for cloud/star animation.
- Disable `CloudShadowManager` on mobile for performance.
